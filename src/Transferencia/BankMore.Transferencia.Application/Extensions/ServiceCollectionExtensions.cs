using Microsoft.Extensions.DependencyInjection;
using BankMore.Transferencia.Application.Handlers;

namespace BankMore.Transferencia.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransferenciaApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RealizarTransferenciaHandler).Assembly));
        return services;
    }
}
