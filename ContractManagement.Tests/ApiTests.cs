using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ContractManagement.Data;
using ContractManagement.Models;
using ContractManagement.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class ApiTests : IDisposable
{
    private readonly string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
    private readonly WebApplicationFactory<Program> factory;
    public ApiTests()
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Key"] = "test-only-signing-key-at-least-32-bytes",
                ["Auth:Username"] = "tester",
                ["Auth:Password"] = "test-only-password",
                ["ConnectionStrings:Sqlite"] = "Data Source=" + path,
                ["Jobs:Enabled"] = "false",
                ["Database:UseSqlite"] = "true"
            }));
        });
    }
    private async Task<HttpClient> Login()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "tester", password = "test-only-password" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.GetProperty("token").GetString());
        return client;
    }
    private static object Payload(string status = "ativo", decimal value = 10.25m) => new
    {
        empresa = "Example", parteEnvolvida = "Supplier", dataInicio = "2026-01-01", dataFim = "2027-01-01",
        valor = value, periodicidade = "anual", status, documento = "", renovacaoAutomatica = true
    };
    [Fact]
    public async Task IssuedTokenAuthorizesCrudAndInvalidCredentialsFail()
    {
        using var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/contratos")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsJsonAsync("/api/auth/login", new { username = "tester", password = "bad" })).StatusCode);
        using var client = await Login();
        var created = await client.PostAsJsonAsync("/api/contratos", Payload());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = await created.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("2026-01-01", item.GetProperty("dataInicio").GetString());
        var url = created.Headers.Location!;
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(url)).StatusCode);
    }
    [Fact]
    public async Task InvalidContractsReturnValidationErrors()
    {
        using var client = await Login();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/contratos", Payload(value: -1))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/contratos", Payload(status: "unknown"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/contratos?limit=10001")).StatusCode);
    }
    [Fact]
    public async Task RenewalSkipsCancelledAndExpiredContractsAndIsIdempotent()
    {
        using var client = await Login();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var today = DateTime.UtcNow.Date;
        foreach (var (status, end) in new[] { ("ativo", today.AddDays(2)), ("cancelado", today.AddDays(2)), ("ativo", today.AddDays(-1)) })
            db.Contratos.Add(new Contrato { Empresa = "Test", ParteEnvolvida = "Test", DataInicio = today.AddYears(-1),
                DataFim = end, Valor = 10, Periodicidade = "anual", Status = status, Documento = "", RenovacaoAutomatica = true });
        await db.SaveChangesAsync();
        var job = scope.ServiceProvider.GetRequiredService<ContratoRenovacaoService>();
        await job.RenovarContratosAutomaticamenteAsync();
        await job.RenovarContratosAutomaticamenteAsync();
        var items = db.Contratos.OrderBy(x => x.Id).ToList();
        Assert.Equal(today.AddDays(2).AddYears(1), items[0].DataFim);
        Assert.Equal(today.AddDays(2), items[1].DataFim);
        Assert.Equal(today.AddDays(-1), items[2].DataFim);
    }
    public void Dispose()
    {
        factory.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { "", "-wal", "-shm" }) File.Delete(path + suffix);
    }
}
