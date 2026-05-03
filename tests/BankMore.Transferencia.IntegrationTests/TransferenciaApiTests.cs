using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BankMore.Transferencia.IntegrationTests;

public sealed class TransferenciaApiTests(TransferenciaWebFactory factory)
    : IClassFixture<TransferenciaWebFactory>
{
    private HttpClient NewClient() => factory.CreateClient();

    [Fact]
    public async Task Post_Transferir_SemToken_Returns401()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/transferencia",
            new
            {
                ChaveIdempotencia = Guid.NewGuid().ToString(),
                NumeroContaOrigem = "12345678",
                NumeroContaDestino = "87654321",
                Valor = 100m
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Health_Returns200()
    {
        var client = NewClient();
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public sealed class TransferenciaWebFactory : WebApplicationFactory<global::Program>
{
    private readonly string _dbPath = $"test-transf-{Guid.NewGuid():N}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Transferencia"] = $"Data Source={_dbPath}",
                ["ContaCorrenteApi:BaseUrl"] = "http://localhost:9999"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
