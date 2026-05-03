using MediatR;
using BankMore.Shared.Models;

namespace BankMore.ContaCorrente.Application.Queries;

public sealed record ConsultarSaldoQuery(Guid AccountId) : IRequest<ConsultarSaldoResult>;

public sealed record ConsultarSaldoResult(
    bool Success,
    string? NumeroConta,
    string? NomeTitular,
    DateTime? DataHoraConsulta,
    decimal? Saldo,
    ErrorResponse? Error);
