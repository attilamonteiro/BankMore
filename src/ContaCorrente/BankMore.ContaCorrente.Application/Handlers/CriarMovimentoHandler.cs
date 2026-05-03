using MediatR;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Idempotency;
using BankMore.Shared.Models;

namespace BankMore.ContaCorrente.Application.Handlers;

public sealed class CriarMovimentoHandler(
    IContaCorrenteRepository contaRepository,
    IMovimentoRepository movimentoRepository,
    IIdempotencyService idempotencyService) : IRequestHandler<CriarMovimentoCommand, CriarMovimentoResult>
{
    public async Task<CriarMovimentoResult> Handle(CriarMovimentoCommand request, CancellationToken cancellationToken)
    {
        var (isDuplicate, cachedResult) = await idempotencyService.CheckAndClaimAsync(
            request.ChaveIdempotencia,
            System.Text.Json.JsonSerializer.Serialize(request));

        if (isDuplicate)
            return new CriarMovimentoResult(true, null);

        var contaAlvo = request.NumeroContaCorrente is not null
            ? await contaRepository.GetByNumeroAsync(request.NumeroContaCorrente, cancellationToken)
            : await contaRepository.GetByIdAsync(request.AccountId, cancellationToken);

        if (contaAlvo is null)
        {
            await idempotencyService.SaveResultAsync(request.ChaveIdempotencia, ErrorTypes.InvalidAccount);
            return new CriarMovimentoResult(false, new ErrorResponse("Conta não encontrada", ErrorTypes.InvalidAccount));
        }

        if (!contaAlvo.Ativo)
        {
            await idempotencyService.SaveResultAsync(request.ChaveIdempotencia, ErrorTypes.InactiveAccount);
            return new CriarMovimentoResult(false, new ErrorResponse("Conta inativa", ErrorTypes.InactiveAccount));
        }

        if (request.Valor <= 0)
        {
            await idempotencyService.SaveResultAsync(request.ChaveIdempotencia, ErrorTypes.InvalidValue);
            return new CriarMovimentoResult(false, new ErrorResponse("Valor deve ser positivo", ErrorTypes.InvalidValue));
        }

        if (request.TipoMovimento != "C" && request.TipoMovimento != "D")
        {
            await idempotencyService.SaveResultAsync(request.ChaveIdempotencia, ErrorTypes.InvalidType);
            return new CriarMovimentoResult(false, new ErrorResponse("Tipo de movimento inválido. Use C ou D", ErrorTypes.InvalidType));
        }

        // Só crédito é permitido para conta diferente do usuário logado
        if (request.NumeroContaCorrente is not null && contaAlvo.IdContaCorrente != request.AccountId && request.TipoMovimento == "D")
        {
            await idempotencyService.SaveResultAsync(request.ChaveIdempotencia, ErrorTypes.InvalidType);
            return new CriarMovimentoResult(false, new ErrorResponse("Débito não permitido em conta de terceiros", ErrorTypes.InvalidType));
        }

        var movimento = new Domain.Entities.Movimento
        {
            IdMovimento = Guid.NewGuid(),
            IdContaCorrente = contaAlvo.IdContaCorrente,
            DataMovimento = DateTime.UtcNow,
            TipoMovimento = request.TipoMovimento,
            Valor = request.Valor
        };

        await movimentoRepository.AddAsync(movimento, cancellationToken);
        await idempotencyService.SaveResultAsync(request.ChaveIdempotencia, "ok");
        return new CriarMovimentoResult(true, null);
    }
}
