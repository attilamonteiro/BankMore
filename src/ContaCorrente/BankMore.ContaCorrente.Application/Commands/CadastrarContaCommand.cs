using MediatR;
using BankMore.Shared.Models;

namespace BankMore.ContaCorrente.Application.Commands;

public sealed record CadastrarContaCommand(string Cpf, string Senha) : IRequest<CadastrarContaResult>;
public sealed record CadastrarContaResult(bool Success, string? NumeroConta, ErrorResponse? Error);
