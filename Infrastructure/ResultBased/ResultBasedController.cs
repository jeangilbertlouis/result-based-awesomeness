using FluentResults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ResultBasedApplication;

namespace Infrastructure.ResultBased;

public static class ResultBasedController
{
    public static WebApplication AddControllers(this WebApplication app)
    {
        app.MapGet("/cars/{id}", async (string id, ResultBasedVehicleService service) =>
        {
            var vehicleResult = await service.GetVehicle(id);

            return vehicleResult switch
            {
                _ when vehicleResult.IsSuccess => Results.Ok(vehicleResult.Value),
                _ when vehicleResult.HasError<ValidationError>() => Results.BadRequest(vehicleResult.Errors),
                _ when vehicleResult.HasError<NotFoundError>() => Results.NotFound(vehicleResult.Errors),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        });
        app.MapGet("/cars", async (ResultBasedVehicleService service) =>
        {
            var ids = new List<string> { "1", "2", "3" };
            var vehiclesResult = await service.GetVehicles(ids);
            
            return vehiclesResult switch
            {
                _ when vehiclesResult.IsSuccess => Results.Ok(vehiclesResult.Value),
                _ when vehiclesResult.HasError<ValidationError>() => Results.BadRequest(vehiclesResult.Errors),
                _ when vehiclesResult.HasError<NotFoundError>() => Results.NotFound(vehiclesResult.Errors),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        });
        return app;
    }
    
    public static IApplicationBuilder UseResultLogging(this IApplicationBuilder app)
    {
        var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
        Result.Setup(s=>s.Logger = new ResultLogger(loggerFactory));
        return app;
    }
}