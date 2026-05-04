using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Sockets;

namespace BankMore.Shared.Observability;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddSqliteHealthCheck(
        this IHealthChecksBuilder builder,
        string connectionString,
        string name = "sqlite")
    {
        return builder.Add(new HealthCheckRegistration(
            name,
            _ => new SqliteHealthCheck(connectionString),
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready"]));
    }

    public static IHealthChecksBuilder AddKafkaHealthCheck(
        this IHealthChecksBuilder builder,
        string brokers,
        string name = "kafka")
    {
        return builder.Add(new HealthCheckRegistration(
            name,
            _ => new KafkaTcpHealthCheck(brokers),
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready"]));
    }

    private sealed class SqliteHealthCheck(string connectionString) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using var conn = new SqliteConnection(connectionString);
                await conn.OpenAsync(cancellationToken);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                await cmd.ExecuteScalarAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("SQLite connection failed", ex);
            }
        }
    }

    private sealed class KafkaTcpHealthCheck(string brokers) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            // brokers format: "host:port" or "host1:port1,host2:port2"
            var firstBroker = brokers.Split(',')[0].Trim();
            var parts = firstBroker.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
                return HealthCheckResult.Unhealthy($"Invalid broker format: {firstBroker}");

            var host = parts[0];
            try
            {
                using var tcp = new TcpClient();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(3));
                await tcp.ConnectAsync(host, port, cts.Token);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Kafka broker unreachable at {firstBroker}", ex);
            }
        }
    }
}
