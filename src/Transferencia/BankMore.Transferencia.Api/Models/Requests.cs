namespace BankMore.Transferencia.Api.Models;

public sealed record RealizarTransferenciaRequest(
    string ChaveIdempotencia,
    string NumeroContaOrigem,
    string NumeroContaDestino,
    decimal Valor
);
