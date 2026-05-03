using MediatR;
using BankMore.Shared.Models;

namespace BankMore.ContaCorrente.Application.Commands;

public sealed record LoginCommand(string NumeroContaOuCpf, string Senha) : IRequest<LoginResult>;
public sealed record LoginResult(bool Success, string? Token, string? NumeroConta, ErrorResponse? Error);
