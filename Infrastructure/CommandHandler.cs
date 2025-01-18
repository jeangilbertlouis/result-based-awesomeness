using Azure.Messaging.ServiceBus;
using ClassicApplication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public class CommandHandler(ServiceBusProcessor processor, IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;
        await processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await processor.StopProcessingAsync(cancellationToken);
        await processor.CloseAsync(cancellationToken);
    }
    
    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<VehicleService>();
        var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
        var idToAddCommand = serializer.Deserialize<string>(args.Message.Body.ToString());

        await service.UpdateVehicle(idToAddCommand);

        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
    }
    
    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        return Task.CompletedTask;
    }
}