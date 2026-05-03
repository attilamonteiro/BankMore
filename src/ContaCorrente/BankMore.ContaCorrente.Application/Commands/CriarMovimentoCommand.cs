using MediatR;
using BankMore.Shared.Models;

namespace BankMore.ContaCorrente.Application.Commands;

public sealed record CriarMovimentoCommand(
    Guid AccountId,
    string ChaveIdempotencia,
    string? NumeroContaCorrente,
    decimal Valor,
    string TipoMovimento) : IRequest<CriarMovimentoResult>;

public sealed record CriarMovimentoResult(bool Success, ErrorResponse? Error);
