using BankMore.Shared.Events;
using BankMore.Transferencia.Application.Events;
using KafkaFlow;

namespace BankMore.Transferencia.Infrastructure.Kafka;

public sealed class TransferenciaEventPublisher(IMessageProducer<TransferenciaRealizadaEvent> producer)
    : ITransferenciaEventPublisher
{
    public Task PublishAsync(TransferenciaRealizadaEvent evento, CancellationToken ct)
        => producer.ProduceAsync(evento.IdTransferencia.ToString(), evento);
}
