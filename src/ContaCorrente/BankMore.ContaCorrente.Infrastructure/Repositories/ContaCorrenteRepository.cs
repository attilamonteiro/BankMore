using Dapper;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Database;

namespace BankMore.ContaCorrente.Infrastructure.Repositories;

public sealed class ContaCorrenteRepository(IDbConnectionFactory connectionFactory) : IContaCorrenteRepository
{
    public async Task<Domain.Entities.ContaCorrente?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<ContaCorrenteRow>(
            "SELECT * FROM contacorrente WHERE idcontacorrente = @Id",
            new { Id = id.ToString() });
        return row?.ToEntity();
    }

    public async Task<Domain.Entities.ContaCorrente?> GetByNumeroAsync(string numero, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<ContaCorrenteRow>(
            "SELECT * FROM contacorrente WHERE numero = @Numero",
            new { Numero = numero });
        return row?.ToEntity();
    }

    public async Task<Domain.Entities.ContaCorrente?> GetByCpfAsync(string cpf, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<ContaCorrenteRow>(
            "SELECT * FROM contacorrente WHERE nome = @Cpf",
            new { Cpf = cpf });
        return row?.ToEntity();
    }

    public async Task<string> AddAsync(Domain.Entities.ContaCorrente conta, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            @"INSERT INTO contacorrente (idcontacorrente, numero, nome, ativo, senha, salt)
              VALUES (@Id, @Numero, @Nome, @Ativo, @Senha, @Salt)",
            new
            {
                Id = conta.IdContaCorrente.ToString(),
                conta.Numero,
                conta.Nome,
                Ativo = conta.Ativo ? 1 : 0,
                conta.Senha,
                conta.Salt
            });
        return conta.Numero;
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE contacorrente SET ativo = 0 WHERE idcontacorrente = @Id",
            new { Id = id.ToString() });
    }

    private sealed class ContaCorrenteRow
    {
        public string idcontacorrente { get; set; } = string.Empty;
        public string numero { get; set; } = string.Empty;
        public string nome { get; set; } = string.Empty;
        public int ativo { get; set; }
        public string senha { get; set; } = string.Empty;
        public string salt { get; set; } = string.Empty;

        public Domain.Entities.ContaCorrente ToEntity() => new()
        {
            IdContaCorrente = Guid.Parse(idcontacorrente),
            Numero = numero,
            Nome = nome,
            Ativo = ativo == 1,
            Senha = senha,
            Salt = salt
        };
    }
}
