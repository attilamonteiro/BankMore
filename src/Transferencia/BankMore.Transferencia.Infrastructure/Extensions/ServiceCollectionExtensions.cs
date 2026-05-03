using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BankMore.Shared.Database;
using BankMore.Shared.Events;
using BankMore.Shared.Idempotency;
using BankMore.Transferencia.Application.Events;
using BankMore.Transferencia.Application.HttpClients;
using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Infrastructure.Database;
using BankMore.Transferencia.Infrastructure.HttpClients;
using BankMore.Transferencia.Infrastructure.Kafka;
using BankMore.Transferencia.Infrastructure.Repositories;
using KafkaFlow;
using KafkaFlow.Serializer;

namespace BankMore.Transferencia.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransferenciaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Transferencia")
            ?? throw new InvalidOperationException("ConnectionString 'Transferencia' não configurada");

        services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
        services.AddSingleton<IIdempotencyService, IdempotencyService>();
        services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
        services.AddScoped<DatabaseInitializer>();

        var contaCorrenteBaseUrl = configuration["ContaCorrenteApi:BaseUrl"]
            ?? throw new InvalidOperationException("ContaCorrenteApi:BaseUrl não configurado");

        services.AddHttpClient<IContaCorrenteHttpClient, ContaCorrenteHttpClient>(client =>
        {
            client.BaseAddress = new Uri(contaCorrenteBaseUrl);
        }).AddPolicyHandler(ResiliencePolicies.GetRetryPolicy())
          .AddPolicyHandler(ResiliencePolicies.GetCircuitBreakerPolicy());

        var kafkaBrokers = configuration["Kafka:Brokers"] ?? "localhost:9092";

        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers([kafkaBrokers])
                .AddProducer<TransferenciaRealizadaEvent>(producer => producer
                    .DefaultTopic("transferencias-realizadas")
                    .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>()))));

        services.AddScoped<ITransferenciaEventPublisher, TransferenciaEventPublisher>();

        return services;
    }
}
