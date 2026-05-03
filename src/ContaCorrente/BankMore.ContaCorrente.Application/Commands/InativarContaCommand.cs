using MediatR;
using BankMore.Shared.Models;

namespace BankMore.ContaCorrente.Application.Commands;

public sealed record InativarContaCommand(Guid AccountId, string Senha) : IRequest<InativarContaResult>;
public sealed record InativarContaResult(bool Success, ErrorResponse? Error);
