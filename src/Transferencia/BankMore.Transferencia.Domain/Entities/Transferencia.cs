namespace BankMore.Transferencia.Domain.Entities;

public sealed class Transferencia
{
    public Guid IdTransferencia { get; init; }
    public string NumeroContaOrigem { get; init; } = string.Empty;
    public string NumeroContaDestino { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public DateTime DataTransferencia { get; init; }
    public string Status { get; private set; } = string.Empty;
    public string ChaveIdempotencia { get; init; } = string.Empty;

    public static Transferencia Criar(string numeroContaOrigem, string numeroContaDestino, decimal valor, string chaveIdempotencia)
        => new()
        {
            IdTransferencia = Guid.NewGuid(),
            NumeroContaOrigem = numeroContaOrigem,
            NumeroContaDestino = numeroContaDestino,
            Valor = valor,
            DataTransferencia = DateTime.UtcNow,
            Status = "Concluida",
            ChaveIdempotencia = chaveIdempotencia
        };

    public void Reverter() => Status = "Revertida";
}
