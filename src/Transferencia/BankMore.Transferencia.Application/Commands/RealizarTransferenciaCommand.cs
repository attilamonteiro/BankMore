using MediatR;

namespace BankMore.Transferencia.Application.Commands;

public sealed record RealizarTransferenciaCommand(
    Guid AccountId,
    string ChaveIdempotencia,
    string NumeroContaOrigem,
    string NumeroContaDestino,
    decimal Valor,
    string Token
) : IRequest<RealizarTransferenciaResult>;

public sealed record RealizarTransferenciaResult(bool Success, string? Error = null);
