using Dapper;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Database;

namespace BankMore.ContaCorrente.Infrastructure.Repositories;

public sealed class MovimentoRepository(IDbConnectionFactory connectionFactory) : IMovimentoRepository
{
    public async Task AddAsync(Domain.Entities.Movimento movimento, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            @"INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
              VALUES (@IdMovimento, @IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor)",
            new
            {
                IdMovimento = movimento.IdMovimento.ToString(),
                IdContaCorrente = movimento.IdContaCorrente.ToString(),
                DataMovimento = movimento.DataMovimento.ToString("O"),
                movimento.TipoMovimento,
                movimento.Valor
            });
    }

    public async Task<decimal> GetSaldoAsync(Guid idContaCorrente, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var saldo = await connection.QueryFirstOrDefaultAsync<decimal?>(
            @"SELECT COALESCE(
                SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE 0 END) -
                SUM(CASE WHEN tipomovimento = 'D' THEN valor ELSE 0 END),
                0)
              FROM movimento
              WHERE idcontacorrente = @Id",
            new { Id = idContaCorrente.ToString() });
        return saldo ?? 0m;
    }

    public async Task<Domain.Entities.ContaCorrente?> GetContaByNumeroAsync(string numero, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT idcontacorrente, numero, nome, ativo, senha, salt FROM contacorrente WHERE numero = @Numero",
            new { Numero = numero });
        if (row is null) return null;
        return new Domain.Entities.ContaCorrente
        {
            IdContaCorrente = Guid.Parse((string)row.idcontacorrente),
            Numero = (string)row.numero,
            Nome = (string)row.nome,
            Ativo = ((int)row.ativo) == 1,
            Senha = (string)row.senha,
            Salt = (string)row.salt
        };
    }
}
