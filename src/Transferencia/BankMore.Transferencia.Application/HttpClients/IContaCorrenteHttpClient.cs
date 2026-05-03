namespace BankMore.Transferencia.Application.HttpClients;

public interface IContaCorrenteHttpClient
{
    Task<bool> MovimentoAsync(string token, string chaveIdempotencia, string numeroContaCorrente, decimal valor, string tipoMovimento, CancellationToken ct);
}
