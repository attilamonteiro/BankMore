using MediatR;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Authentication;
using BankMore.Shared.Models;
using BankMore.Shared.Security;

namespace BankMore.ContaCorrente.Application.Handlers;

public sealed class LoginHandler(
    IContaCorrenteRepository repository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, LoginResult>
{
    private static readonly ErrorResponse Unauthorized =
        new("Credenciais inválidas", ErrorTypes.UserUnauthorized);

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var conta = await repository.GetByNumeroAsync(request.NumeroContaOuCpf, cancellationToken);

        if (conta is null)
        {
            var onlyDigits = new string(request.NumeroContaOuCpf.Where(char.IsDigit).ToArray());
            if (onlyDigits.Length == 11)
                conta = await repository.GetByCpfAsync(onlyDigits, cancellationToken);
        }

        if (conta is null)
            return new LoginResult(false, null, null, Unauthorized);

        if (!passwordHasher.VerifyPassword(request.Senha, conta.Senha, conta.Salt))
            return new LoginResult(false, null, null, Unauthorized);

        var token = jwtTokenService.GenerateToken(conta.IdContaCorrente);
        return new LoginResult(true, token, conta.Numero, null);
    }
}
