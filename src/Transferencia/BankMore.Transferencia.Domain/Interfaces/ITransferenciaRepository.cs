using TransferenciaEntity = global::BankMore.Transferencia.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.Domain.Interfaces;

public interface ITransferenciaRepository
{
    Task AddAsync(TransferenciaEntity transferencia);
}
