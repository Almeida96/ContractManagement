using ContractManagement;
using ContractManagement.Data;
using ContractManagement.Converters;
using ContractManagement.Services;
using FluentValidation.AspNetCore;
using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(services => AuthSettings.Read(services.GetRequiredService<IConfiguration>()));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme).Configure<AuthSettings>((options, auth) =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.Key)),
        ValidateIssuer = true, ValidIssuer = AuthSettings.Issuer,
        ValidateAudience = true, ValidAudience = AuthSettings.Audience,
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
    };
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("login", limit =>
    {
        limit.PermitLimit = 10;
        limit.Window = TimeSpan.FromMinutes(1);
        limit.QueueLimit = 0;
    });
});
var sqlite = builder.Configuration.GetValue("Database:UseSqlite", true);
var connection = builder.Configuration.GetConnectionString(sqlite ? "Sqlite" : "DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>((services, options) =>
{
    var config = services.GetRequiredService<IConfiguration>();
    if (sqlite) options.UseSqlite(config.GetConnectionString("Sqlite") ?? "Data Source=contracts.db");
    else options.UseSqlServer(config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection."));
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ContractManagement.Validators.ContratoValidator>();
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonConverterDateOnly()));
builder.Services.AddTransient<ContratoRenovacaoService>();
var jobs = builder.Configuration.GetValue<bool>("Jobs:Enabled");
if (jobs)
{
    if (sqlite) throw new InvalidOperationException("Scheduled jobs require SQL Server. Set Database__UseSqlite=false.");
    builder.Services.AddHangfire(options => options.UseSqlServerStorage(connection));
    builder.Services.AddHangfireServer();
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
_ = app.Services.GetRequiredService<AuthSettings>();
if (sqlite)
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
}
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing")) app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
if (jobs)
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<IRecurringJobManager>().AddOrUpdate<ContratoRenovacaoService>(
        "renew-contracts", service => service.RenovarContratosAutomaticamenteAsync(),
        Cron.Daily(9), new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
}
app.MapControllers();
app.Run();
public partial class Program { }
