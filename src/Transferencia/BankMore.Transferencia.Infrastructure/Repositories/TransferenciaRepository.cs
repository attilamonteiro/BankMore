using Dapper;
using BankMore.Shared.Database;
using TransferenciaEntity = global::BankMore.Transferencia.Domain.Entities.Transferencia;
using BankMore.Transferencia.Domain.Interfaces;

namespace BankMore.Transferencia.Infrastructure.Repositories;

public sealed class TransferenciaRepository(IDbConnectionFactory connectionFactory) : ITransferenciaRepository
{
    public async Task AddAsync(TransferenciaEntity transferencia)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            """
            INSERT INTO transferencia (id_transferencia, numero_conta_origem, numero_conta_destino, valor, data_transferencia, status, chave_idempotencia)
            VALUES (@IdTransferencia, @NumeroContaOrigem, @NumeroContaDestino, @Valor, @DataTransferencia, @Status, @ChaveIdempotencia)
            """,
            new
            {
                IdTransferencia = transferencia.IdTransferencia.ToString(),
                transferencia.NumeroContaOrigem,
                transferencia.NumeroContaDestino,
                transferencia.Valor,
                transferencia.DataTransferencia,
                transferencia.Status,
                transferencia.ChaveIdempotencia
            });
    }
}
