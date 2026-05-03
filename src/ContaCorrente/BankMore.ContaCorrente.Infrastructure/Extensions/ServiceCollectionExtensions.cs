using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Database;
using BankMore.ContaCorrente.Infrastructure.Kafka;
using BankMore.ContaCorrente.Infrastructure.Repositories;
using BankMore.Shared.Database;
using BankMore.Shared.Events;
using BankMore.Shared.Idempotency;
using KafkaFlow;
using KafkaFlow.Serializer;

namespace BankMore.ContaCorrente.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContaCorrenteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ContaCorrente") ?? "Data Source=contacorrente.db";
        services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
        services.AddSingleton<IIdempotencyService, IdempotencyService>();
        services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
        services.AddScoped<IMovimentoRepository, MovimentoRepository>();
        services.AddSingleton<DatabaseInitializer>();

        var kafkaBrokers = configuration["Kafka:Brokers"] ?? "localhost:9092";

        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers([kafkaBrokers])
                .AddConsumer(consumer => consumer
                    .Topic("tarifas-realizadas")
                    .WithGroupId("contacorrente-tarifas-group")
                    .WithBufferSize(100)
                    .WithWorkersCount(1)
                    .AddMiddlewares(m => m
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h
                            .WithHandlerLifetime(InstanceLifetime.Scoped)
                            .AddHandler<TarifaRealizadaConsumer>())))));

        return services;
    }
}
