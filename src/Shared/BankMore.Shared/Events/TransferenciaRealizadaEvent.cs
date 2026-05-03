namespace BankMore.Shared.Events;

public sealed record TransferenciaRealizadaEvent(
    Guid IdTransferencia,
    string NumeroContaOrigem,
    string NumeroContaDestino,
    decimal Valor,
    DateTime DataTransferencia
);
