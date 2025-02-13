﻿using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ResultBasedApplication;

namespace Infrastructure.ResultBased;

public class ResultBasedCommandHandler(ServiceBusProcessor processor, IServiceProvider serviceProvider) : IHostedService
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
        var service = scope.ServiceProvider.GetRequiredService<ResultBasedVehicleService>();
        var serializer = scope.ServiceProvider.GetRequiredService<IResultBasedSerializer>();
        var idToAddCommandResult = serializer.Deserialize<string>(args.Message.Body.ToString());
        idToAddCommandResult.ThrowOnError();

        var updateResult = await service.UpdateVehicle(idToAddCommandResult.Value);
        updateResult.ThrowOnError();

        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
    }
    
    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        return Task.CompletedTask;
    }
}