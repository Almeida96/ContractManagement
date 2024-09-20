using Microsoft.EntityFrameworkCore; // Importa o namespace do Entity Framework Core
using ContractManagement.Data; // Importa o namespace do ApplicationDbContext
using FluentValidation.AspNetCore; // Importa o namespace do FluentValidation
using ContractManagement.Converters; // Importa o conversor personalizado de data
using Hangfire; // Importa o Hangfire
using ContractManagement.Services; // Importa o servi�o de renova��o de contratos
using Microsoft.AspNetCore.Authentication.JwtBearer; // Importa para autentica��o JWT
using Microsoft.IdentityModel.Tokens; // Importa para valida��o de tokens JWT
using System.Text; // Para manipular a chave de criptografia

var builder = WebApplication.CreateBuilder(args);

// Configurar a chave secreta do JWT
var key = Encoding.ASCII.GetBytes("SuaChaveSecretaSuperSegura"); // Substitua por uma chave segura

// Configurar autentica��o JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Adicionar Autoriza��o
builder.Services.AddAuthorization();

// Configurar a conex�o com o banco de dados (a string est� no appsettings.json)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar os validadores do FluentValidation e o conversor de datas personalizado
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<ContractManagement.Validators.ContratoValidator>())
    .AddJsonOptions(options =>
    {
        // Configura o formato de data para "dd-MM-yyyy" utilizando o conversor personalizado
        options.JsonSerializerOptions.Converters.Add(new JsonConverterDateOnly());
    });

// Configurar Hangfire para agendamento de tarefas
builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// Registrar o servi�o de renova��o de contratos
builder.Services.AddTransient<ContratoRenovacaoService>();

// Configurar Swagger/OpenAPI e JWT no Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Configura o pipeline de requisi��es HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configurar Hangfire Dashboard
app.UseHangfireDashboard();
app.UseHangfireServer();

// Configura a autentica��o e autoriza��o no pipeline
app.UseAuthentication(); // Adiciona o middleware de autentica��o JWT
app.UseAuthorization();

// Agendar a renova��o de contratos automaticamente todos os dias �s 9h
RecurringJob.AddOrUpdate<ContratoRenovacaoService>(
    "renovar-contratos-automaticamente",
    service => service.RenovarContratosAutomaticamenteAsync(),
    Cron.Daily(9, 0)  // Executa diariamente �s 9h
);

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
