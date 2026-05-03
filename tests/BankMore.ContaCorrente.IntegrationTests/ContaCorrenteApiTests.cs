using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BankMore.ContaCorrente.IntegrationTests;

public sealed class ContaCorrenteApiTests(ContaCorrenteWebFactory factory)
    : IClassFixture<ContaCorrenteWebFactory>
{
    // Each test gets its own client to avoid auth header pollution
    private HttpClient NewClient() => factory.CreateClient();

    [Fact]
    public async Task Post_CadastrarConta_ValidCpf_Returns200WithNumeroConta()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/contacorrente",
            new { Cpf = "529.982.247-25", Senha = "senha123" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CadastrarResponse>();
        body!.NumeroConta.Should().MatchRegex(@"^\d{8}$");
    }

    [Fact]
    public async Task Post_CadastrarConta_InvalidCpf_Returns400()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/contacorrente",
            new { Cpf = "000.000.000-00", Senha = "senha123" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Movimento_SemToken_Returns401()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/contacorrente/movimento",
            new { ChaveIdempotencia = Guid.NewGuid().ToString(), NumeroContaCorrente = (string?)null, Valor = 100m, TipoMovimento = "C" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Saldo_SemToken_Returns401()
    {
        var client = NewClient();
        var response = await client.GetAsync("/api/contacorrente/saldo");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FluxoCompleto_CadastroLoginCreditoDebitoSaldo()
    {
        var client = NewClient();

        // Cadastrar com CPF único para este teste
        var cadastroResp = await client.PostAsJsonAsync("/api/contacorrente",
            new { Cpf = "111.444.777-35", Senha = "teste@123" });
        cadastroResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var cadastro = await cadastroResp.Content.ReadFromJsonAsync<CadastrarResponse>();

        // Login
        var loginResp = await client.PostAsJsonAsync("/api/contacorrente/login",
            new { NumeroContaOuCpf = cadastro!.NumeroConta, Senha = "teste@123" });
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();
        login!.Token.Should().NotBeNullOrEmpty();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        // Crédito 100
        var credito = await client.PostAsJsonAsync("/api/contacorrente/movimento",
            new { ChaveIdempotencia = $"cred-{Guid.NewGuid():N}", NumeroContaCorrente = cadastro.NumeroConta, Valor = 100m, TipoMovimento = "C" });
        credito.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Débito 30
        var debito = await client.PostAsJsonAsync("/api/contacorrente/movimento",
            new { ChaveIdempotencia = $"deb-{Guid.NewGuid():N}", NumeroContaCorrente = (string?)null, Valor = 30m, TipoMovimento = "D" });
        debito.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Saldo deve ser 70
        var saldoResp = await client.GetAsync("/api/contacorrente/saldo");
        saldoResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var saldo = await saldoResp.Content.ReadFromJsonAsync<SaldoResponse>();
        saldo!.Saldo.Should().Be(70m);
    }

    [Fact]
    public async Task Post_Movimento_IdempotenciaNaoDuplicaValor()
    {
        var client = NewClient();

        // CPF valid: 153.509.460-56
        var cadastroResp = await client.PostAsJsonAsync("/api/contacorrente",
            new { Cpf = "153.509.460-56", Senha = "abc123" });
        cadastroResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var cadastro = await cadastroResp.Content.ReadFromJsonAsync<CadastrarResponse>();

        var loginResp = await client.PostAsJsonAsync("/api/contacorrente/login",
            new { NumeroContaOuCpf = cadastro!.NumeroConta, Senha = "abc123" });
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);

        var chave = $"idp-{Guid.NewGuid():N}";
        var body = new { ChaveIdempotencia = chave, NumeroContaCorrente = cadastro.NumeroConta, Valor = 50m, TipoMovimento = "C" };

        var first = await client.PostAsJsonAsync("/api/contacorrente/movimento", body);
        var second = await client.PostAsJsonAsync("/api/contacorrente/movimento", body);

        first.StatusCode.Should().Be(HttpStatusCode.NoContent);
        second.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var saldoResp = await client.GetAsync("/api/contacorrente/saldo");
        var saldo = await saldoResp.Content.ReadFromJsonAsync<SaldoResponse>();
        saldo!.Saldo.Should().Be(50m);
    }

    private sealed record CadastrarResponse(string NumeroConta);
    private sealed record LoginResponse(string Token);
    private sealed record SaldoResponse(string NumeroConta, string NomeTitular, DateTime DataHoraConsulta, decimal Saldo);
}

public sealed class ContaCorrenteWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = $"test-{Guid.NewGuid():N}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ContaCorrente"] = $"Data Source={_dbPath}"
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
