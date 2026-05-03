using BankMore.Shared.Events;
using BankMore.Tarifas.Domain.Entities;
using BankMore.Tarifas.Domain.Interfaces;
using KafkaFlow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BankMore.Tarifas.Infrastructure.Kafka;

public sealed class TransferenciaRealizadaConsumer(
    IServiceScopeFactory scopeFactory,
    IMessageProducer<TarifaRealizadaEvent> producer,
    IConfiguration configuration,
    ILogger<TransferenciaRealizadaConsumer> logger
) : IMessageHandler<TransferenciaRealizadaEvent>
{
    public async Task Handle(IMessageContext context, TransferenciaRealizadaEvent message)
    {
        var percentual = configuration.GetValue<decimal>("Tarifas:PercentualTarifa", 1m);

        var tarifa = Tarifa.Calcular(
            message.IdTransferencia,
            message.NumeroContaOrigem,
            message.Valor,
            percentual);

        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITarifaRepository>();
        await repository.AddAsync(tarifa);

        var evento = new TarifaRealizadaEvent(
            tarifa.IdTarifa,
            tarifa.NumeroContaCorrente,
            tarifa.ValorTarifa,
            tarifa.DataTarifa);

        await producer.ProduceAsync(tarifa.IdTarifa.ToString(), evento);

        logger.LogInformation("Tarifa {IdTarifa} de {Valor} calculada para conta {Conta}",
            tarifa.IdTarifa, tarifa.ValorTarifa, tarifa.NumeroContaCorrente);
    }
}
