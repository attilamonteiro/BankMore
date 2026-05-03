using MediatR;
using BankMore.ContaCorrente.Application.Queries;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Models;

namespace BankMore.ContaCorrente.Application.Handlers;

public sealed class ConsultarSaldoHandler(
    IContaCorrenteRepository contaRepository,
    IMovimentoRepository movimentoRepository) : IRequestHandler<ConsultarSaldoQuery, ConsultarSaldoResult>
{
    public async Task<ConsultarSaldoResult> Handle(ConsultarSaldoQuery request, CancellationToken cancellationToken)
    {
        var conta = await contaRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (conta is null)
            return new ConsultarSaldoResult(false, null, null, null, null,
                new ErrorResponse("Conta não encontrada", ErrorTypes.InvalidAccount));

        if (!conta.Ativo)
            return new ConsultarSaldoResult(false, null, null, null, null,
                new ErrorResponse("Conta inativa", ErrorTypes.InactiveAccount));

        var saldo = await movimentoRepository.GetSaldoAsync(conta.IdContaCorrente, cancellationToken);

        return new ConsultarSaldoResult(true, conta.Numero, conta.Nome, DateTime.UtcNow, saldo, null);
    }
}
