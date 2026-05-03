using Microsoft.Extensions.DependencyInjection;
using BankMore.ContaCorrente.Application.Commands;

namespace BankMore.ContaCorrente.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContaCorrenteApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CadastrarContaCommand).Assembly));
        return services;
    }
}
