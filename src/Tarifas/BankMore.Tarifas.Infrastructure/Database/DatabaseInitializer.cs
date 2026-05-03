using Dapper;
using BankMore.Shared.Database;

namespace BankMore.Tarifas.Infrastructure.Database;

public sealed class DatabaseInitializer(IDbConnectionFactory connectionFactory)
{
    public async Task InitializeAsync()
    {
        using var conn = connectionFactory.CreateConnection();

        await conn.ExecuteAsync("PRAGMA journal_mode=WAL;");

        await conn.ExecuteAsync(
            """
            CREATE TABLE IF NOT EXISTS tarifa (
                id_tarifa TEXT PRIMARY KEY,
                id_transferencia TEXT NOT NULL,
                numero_conta_corrente TEXT NOT NULL,
                valor_tarifa REAL NOT NULL,
                data_tarifa TEXT NOT NULL
            );
            """);
    }
}
