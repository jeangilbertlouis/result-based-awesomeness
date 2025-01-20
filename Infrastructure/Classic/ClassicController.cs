using ClassicApplication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Classic;

public static class ClassicController
{
    public static WebApplication AddControllers(this WebApplication app)
    {
        app.MapGet("/cars/{id}", async (string id, ClassicVehicleService service) =>
        {
            try
            {
                var vehicle = await service.GetVehicle(id);
                return Results.Ok(vehicle);
            }
            catch(ArgumentException e)
            {
                return Results.BadRequest(e.Message);
            }
            catch (NotFoundException e)
            {
                return Results.NotFound(e.Message);
            }
            catch (Exception e)
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });
        app.MapGet("/cars", async (ClassicVehicleService service) =>
        {
            try
            {
                var ids = new List<string> { "1", "2", "3" };
                var vehicles = await service.GetVehicles(ids);
                return Results.Ok(vehicles);
            }
            catch(ArgumentException e)
            {
                return Results.BadRequest(e.Message);
            }
            catch (NotFoundException e)
            {
                return Results.NotFound(e.Message);
            }
            catch (Exception e)
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });
        return app;
    }
}