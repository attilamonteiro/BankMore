using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankMore.Transferencia.Application.HttpClients;

namespace BankMore.Transferencia.Infrastructure.HttpClients;

public sealed class ContaCorrenteHttpClient(HttpClient httpClient) : IContaCorrenteHttpClient
{
    public async Task<bool> MovimentoAsync(string token, string chaveIdempotencia, string numeroContaCorrente, decimal valor, string tipoMovimento, CancellationToken ct)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            ChaveIdempotencia = chaveIdempotencia,
            NumeroContaCorrente = numeroContaCorrente,
            Valor = valor,
            TipoMovimento = tipoMovimento
        };

        var response = await httpClient.PostAsJsonAsync("/api/contacorrente/movimento", body, ct);
        return response.IsSuccessStatusCode;
    }
}
