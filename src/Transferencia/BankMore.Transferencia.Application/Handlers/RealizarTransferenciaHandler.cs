using MediatR;
using BankMore.Shared.Idempotency;
using BankMore.Transferencia.Application.Commands;
using TransferenciaEntity = global::BankMore.Transferencia.Domain.Entities.Transferencia;
using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Application.HttpClients;

namespace BankMore.Transferencia.Application.Handlers;

public sealed class RealizarTransferenciaHandler(
    IIdempotencyService idempotency,
    IContaCorrenteHttpClient contaCorrenteClient,
    ITransferenciaRepository repository
) : IRequestHandler<RealizarTransferenciaCommand, RealizarTransferenciaResult>
{
    public async Task<RealizarTransferenciaResult> Handle(RealizarTransferenciaCommand request, CancellationToken ct)
    {
        var (isDuplicate, _) = await idempotency.CheckAndClaimAsync(request.ChaveIdempotencia, "transferencia");
        if (isDuplicate)
            return new RealizarTransferenciaResult(true);

        var debitKey = $"{request.ChaveIdempotencia}-debit";
        var debitResult = await contaCorrenteClient.MovimentoAsync(
            token: request.Token,
            chaveIdempotencia: debitKey,
            numeroContaCorrente: request.NumeroContaOrigem,
            valor: request.Valor,
            tipoMovimento: "D",
            ct: ct);

        if (!debitResult)
            return new RealizarTransferenciaResult(false, "Falha ao debitar conta de origem.");

        var creditKey = $"{request.ChaveIdempotencia}-credit";
        var creditResult = await contaCorrenteClient.MovimentoAsync(
            token: request.Token,
            chaveIdempotencia: creditKey,
            numeroContaCorrente: request.NumeroContaDestino,
            valor: request.Valor,
            tipoMovimento: "C",
            ct: ct);

        var transferencia = TransferenciaEntity.Criar(
            request.NumeroContaOrigem,
            request.NumeroContaDestino,
            request.Valor,
            request.ChaveIdempotencia);

        if (!creditResult)
        {
            transferencia.Reverter();

            var rollbackKey = $"{request.ChaveIdempotencia}-rollback";
            await contaCorrenteClient.MovimentoAsync(
                token: request.Token,
                chaveIdempotencia: rollbackKey,
                numeroContaCorrente: request.NumeroContaOrigem,
                valor: request.Valor,
                tipoMovimento: "C",
                ct: ct);

            await repository.AddAsync(transferencia);
            await idempotency.SaveResultAsync(request.ChaveIdempotencia, "revertida");
            return new RealizarTransferenciaResult(false, "Falha ao creditar conta de destino. Transferência revertida.");
        }

        await repository.AddAsync(transferencia);
        await idempotency.SaveResultAsync(request.ChaveIdempotencia, "concluida");
        return new RealizarTransferenciaResult(true);
    }
}
