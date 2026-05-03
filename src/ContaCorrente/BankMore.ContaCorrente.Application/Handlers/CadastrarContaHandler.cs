using MediatR;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Domain.ValueObjects;
using BankMore.Shared.Models;
using BankMore.Shared.Security;

namespace BankMore.ContaCorrente.Application.Handlers;

public sealed class CadastrarContaHandler(
    IContaCorrenteRepository repository,
    IPasswordHasher passwordHasher) : IRequestHandler<CadastrarContaCommand, CadastrarContaResult>
{
    public async Task<CadastrarContaResult> Handle(CadastrarContaCommand request, CancellationToken cancellationToken)
    {
        if (!Cpf.IsValid(request.Cpf))
            return new CadastrarContaResult(false, null, new ErrorResponse("CPF inválido", ErrorTypes.InvalidDocument));

        var cpfDigits = new string(request.Cpf.Where(char.IsDigit).ToArray());

        var (hash, salt) = passwordHasher.HashPassword(request.Senha);

        var numero = await GenerateUniqueNumeroAsync(cancellationToken);

        var conta = new Domain.Entities.ContaCorrente
        {
            IdContaCorrente = Guid.NewGuid(),
            Numero = numero,
            Nome = cpfDigits,
            Ativo = true,
            Senha = hash,
            Salt = salt
        };

        await repository.AddAsync(conta, cancellationToken);
        return new CadastrarContaResult(true, numero, null);
    }

    private async Task<string> GenerateUniqueNumeroAsync(CancellationToken ct)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var numero = Random.Shared.Next(10000000, 99999999).ToString();
            var existing = await repository.GetByNumeroAsync(numero, ct);
            if (existing is null) return numero;
        }
        throw new InvalidOperationException("Não foi possível gerar número de conta único.");
    }
}
