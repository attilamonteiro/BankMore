using Dapper;
using BankMore.Shared.Database;

namespace BankMore.Transferencia.Infrastructure.Database;

public sealed class DatabaseInitializer(IDbConnectionFactory connectionFactory)
{
    public async Task InitializeAsync()
    {
        using var conn = connectionFactory.CreateConnection();

        await conn.ExecuteAsync("PRAGMA journal_mode=WAL;");

        await conn.ExecuteAsync(
            """
            CREATE TABLE IF NOT EXISTS transferencia (
                id_transferencia TEXT PRIMARY KEY,
                numero_conta_origem TEXT NOT NULL,
                numero_conta_destino TEXT NOT NULL,
                valor REAL NOT NULL,
                data_transferencia TEXT NOT NULL,
                status TEXT NOT NULL,
                chave_idempotencia TEXT NOT NULL UNIQUE
            );
            """);

        await conn.ExecuteAsync(
            """
            CREATE TABLE IF NOT EXISTS idempotencia (
                chave_idempotencia TEXT PRIMARY KEY,
                requisicao TEXT NOT NULL,
                resultado TEXT,
                criado_em TEXT NOT NULL DEFAULT (datetime('now'))
            );
            """);
    }
}
