namespace BankMore.Tarifas.Domain.Entities;

public sealed class Tarifa
{
    public Guid IdTarifa { get; init; }
    public Guid IdTransferencia { get; init; }
    public string NumeroContaCorrente { get; init; } = string.Empty;
    public decimal ValorTarifa { get; init; }
    public DateTime DataTarifa { get; init; }

    public static Tarifa Calcular(Guid idTransferencia, string numeroConta, decimal valorTransferencia, decimal percentualTarifa)
        => new()
        {
            IdTarifa = Guid.NewGuid(),
            IdTransferencia = idTransferencia,
            NumeroContaCorrente = numeroConta,
            ValorTarifa = Math.Round(valorTransferencia * (percentualTarifa / 100m), 2),
            DataTarifa = DateTime.UtcNow
        };
}
