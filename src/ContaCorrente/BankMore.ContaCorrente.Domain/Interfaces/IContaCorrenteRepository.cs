namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IContaCorrenteRepository
{
    Task<Entities.ContaCorrente?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Entities.ContaCorrente?> GetByNumeroAsync(string numero, CancellationToken ct = default);
    Task<Entities.ContaCorrente?> GetByCpfAsync(string cpf, CancellationToken ct = default);
    Task<string> AddAsync(Entities.ContaCorrente conta, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
}
