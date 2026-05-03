using Dapper;
using BankMore.Shared.Database;

namespace BankMore.ContaCorrente.Infrastructure.Database;

public sealed class DatabaseInitializer(IDbConnectionFactory connectionFactory)
{
    public async Task InitializeAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        await connection.ExecuteAsync("PRAGMA journal_mode=WAL;");

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS contacorrente (
                idcontacorrente TEXT PRIMARY KEY,
                numero TEXT NOT NULL UNIQUE,
                nome TEXT NOT NULL,
                ativo INTEGER NOT NULL DEFAULT 1,
                senha TEXT NOT NULL,
                salt TEXT NOT NULL
            )");

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS movimento (
                idmovimento TEXT PRIMARY KEY,
                idcontacorrente TEXT NOT NULL,
                datamovimento TEXT NOT NULL,
                tipomovimento TEXT NOT NULL,
                valor REAL NOT NULL,
                FOREIGN KEY (idcontacorrente) REFERENCES contacorrente(idcontacorrente)
            )");

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS idempotencia (
                chave_idempotencia TEXT PRIMARY KEY,
                requisicao TEXT NOT NULL,
                resultado TEXT NULL
            )");
    }
}
