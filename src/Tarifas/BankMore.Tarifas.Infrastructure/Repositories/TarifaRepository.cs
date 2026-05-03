using Dapper;
using BankMore.Shared.Database;
using BankMore.Tarifas.Domain.Entities;
using BankMore.Tarifas.Domain.Interfaces;

namespace BankMore.Tarifas.Infrastructure.Repositories;

public sealed class TarifaRepository(IDbConnectionFactory connectionFactory) : ITarifaRepository
{
    public async Task AddAsync(Tarifa tarifa)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            """
            INSERT INTO tarifa (id_tarifa, id_transferencia, numero_conta_corrente, valor_tarifa, data_tarifa)
            VALUES (@IdTarifa, @IdTransferencia, @NumeroContaCorrente, @ValorTarifa, @DataTarifa)
            """,
            new
            {
                IdTarifa = tarifa.IdTarifa.ToString(),
                IdTransferencia = tarifa.IdTransferencia.ToString(),
                tarifa.NumeroContaCorrente,
                tarifa.ValorTarifa,
                tarifa.DataTarifa
            });
    }
}
