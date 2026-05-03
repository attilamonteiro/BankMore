using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Events;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BankMore.ContaCorrente.Infrastructure.Kafka;

public sealed class TarifaRealizadaConsumer(
    IServiceScopeFactory scopeFactory,
    ILogger<TarifaRealizadaConsumer> logger
) : IMessageHandler<TarifaRealizadaEvent>
{
    public async Task Handle(IMessageContext context, TarifaRealizadaEvent message)
    {
        using var scope = scopeFactory.CreateScope();
        var contaRepo = scope.ServiceProvider.GetRequiredService<IContaCorrenteRepository>();
        var movimentoRepo = scope.ServiceProvider.GetRequiredService<IMovimentoRepository>();

        var conta = await contaRepo.GetByNumeroAsync(message.NumeroContaCorrente, CancellationToken.None);
        if (conta is null || !conta.Ativo)
        {
            logger.LogWarning("Tarifa ignorada: conta {Conta} não encontrada ou inativa", message.NumeroContaCorrente);
            return;
        }

        var movimento = new Movimento
        {
            IdMovimento = Guid.NewGuid(),
            IdContaCorrente = conta.IdContaCorrente,
            DataMovimento = DateTime.UtcNow,
            TipoMovimento = "D",
            Valor = message.ValorTarifa
        };

        await movimentoRepo.AddAsync(movimento, CancellationToken.None);

        logger.LogInformation("Tarifa {IdTarifa} de {Valor} debitada da conta {Conta}",
            message.IdTarifa, message.ValorTarifa, message.NumeroContaCorrente);
    }
}
