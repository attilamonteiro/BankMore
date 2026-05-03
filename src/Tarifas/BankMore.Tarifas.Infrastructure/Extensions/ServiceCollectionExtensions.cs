using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BankMore.Shared.Database;
using BankMore.Shared.Events;
using BankMore.Tarifas.Domain.Interfaces;
using BankMore.Tarifas.Infrastructure.Database;
using BankMore.Tarifas.Infrastructure.Kafka;
using BankMore.Tarifas.Infrastructure.Repositories;
using KafkaFlow;
using KafkaFlow.Serializer;

namespace BankMore.Tarifas.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTarifasInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Tarifas")
            ?? throw new InvalidOperationException("ConnectionString 'Tarifas' não configurada");

        services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
        services.AddScoped<ITarifaRepository, TarifaRepository>();
        services.AddSingleton<DatabaseInitializer>();

        var kafkaBrokers = configuration["Kafka:Brokers"] ?? "localhost:9092";
        const string groupId = "tarifas-worker-group";

        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers([kafkaBrokers])
                .AddProducer<TarifaRealizadaEvent>(producer => producer
                    .DefaultTopic("tarifas-realizadas")
                    .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>()))
                .AddConsumer(consumer => consumer
                    .Topic("transferencias-realizadas")
                    .WithGroupId(groupId)
                    .WithBufferSize(100)
                    .WithWorkersCount(3)
                    .AddMiddlewares(m => m
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h
                            .WithHandlerLifetime(InstanceLifetime.Scoped)
                            .AddHandler<TransferenciaRealizadaConsumer>())))));

        return services;
    }
}
