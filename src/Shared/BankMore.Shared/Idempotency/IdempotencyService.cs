using BankMore.Shared.Database;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BankMore.Shared.Idempotency;

public sealed class IdempotencyService : IIdempotencyService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public IdempotencyService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(bool IsDuplicate, string? CachedResult)> CheckAndClaimAsync(string key, string requestJson)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        try
        {
            await connection.ExecuteAsync(
                "INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado) VALUES (@Key, @Request, NULL)",
                new { Key = key, Request = requestJson });

            return (false, null);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint violation
        {
            var cachedResult = await connection.QuerySingleOrDefaultAsync<string?>(
                "SELECT resultado FROM idempotencia WHERE chave_idempotencia = @Key",
                new { Key = key });

            return (true, cachedResult);
        }
    }

    public async Task SaveResultAsync(string key, string resultJson)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        await connection.ExecuteAsync(
            "UPDATE idempotencia SET resultado = @Result WHERE chave_idempotencia = @Key",
            new { Result = resultJson, Key = key });
    }
}
