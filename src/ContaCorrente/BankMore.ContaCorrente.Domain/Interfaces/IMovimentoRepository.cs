namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IMovimentoRepository
{
    Task AddAsync(Entities.Movimento movimento, CancellationToken ct = default);
    Task<decimal> GetSaldoAsync(Guid idContaCorrente, CancellationToken ct = default);
    Task<Entities.ContaCorrente?> GetContaByNumeroAsync(string numero, CancellationToken ct = default);
}
