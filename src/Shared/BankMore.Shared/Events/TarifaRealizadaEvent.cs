namespace BankMore.Shared.Events;

public sealed record TarifaRealizadaEvent(
    Guid IdTarifa,
    string NumeroContaCorrente,
    decimal ValorTarifa,
    DateTime DataTarifa
);
