using MediatR;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Models;
using BankMore.Shared.Security;

namespace BankMore.ContaCorrente.Application.Handlers;

public sealed class InativarContaHandler(
    IContaCorrenteRepository repository,
    IPasswordHasher passwordHasher) : IRequestHandler<InativarContaCommand, InativarContaResult>
{
    public async Task<InativarContaResult> Handle(InativarContaCommand request, CancellationToken cancellationToken)
    {
        var conta = await repository.GetByIdAsync(request.AccountId, cancellationToken);
        if (conta is null)
            return new InativarContaResult(false, new ErrorResponse("Conta não encontrada", ErrorTypes.InvalidAccount));

        if (!passwordHasher.VerifyPassword(request.Senha, conta.Senha, conta.Salt))
            return new InativarContaResult(false, new ErrorResponse("Senha inválida", ErrorTypes.UserUnauthorized));

        await repository.DeactivateAsync(request.AccountId, cancellationToken);
        return new InativarContaResult(true, null);
    }
}
