using BankMore.Tarifas.Infrastructure.Database;
using BankMore.Tarifas.Infrastructure.Extensions;
using KafkaFlow;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddTarifasInfrastructure(builder.Configuration);

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

var kafkaBus = host.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() => kafkaBus.StopAsync().GetAwaiter().GetResult());

await host.RunAsync();
