using BankMore.Shared.Events;

namespace BankMore.Transferencia.Application.Events;

public interface ITransferenciaEventPublisher
{
    Task PublishAsync(TransferenciaRealizadaEvent evento, CancellationToken ct);
}
