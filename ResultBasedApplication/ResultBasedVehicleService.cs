using FluentResults;

namespace ResultBasedApplication;

public class ResultBasedVehicleService(IResultBasedVehicleRepository repository, IResultBasedVehicleGateway gateway)
{
    public async Task<Result<Vehicle>> GetVehicle(string id)
    {
        if(string.IsNullOrWhiteSpace(id))
            return Result.Fail(new ValidationError("Id is required"));
        
        //Try get from repo
        var vehicleFromRepoResult = await repository.GetById(id);
        
        //Happy path
        if(vehicleFromRepoResult.IsSuccess)
            return MapToVehicle(vehicleFromRepoResult.Value);

        //If repo does not have the vehicle, try to get from gateway
        if (vehicleFromRepoResult.HasError<NotFoundError>())
        {
            var vehicleFromGatewayResult = await gateway.GetById(id);
            
            //If gateway fails, return the error
            if(vehicleFromGatewayResult.IsFailed)
                return vehicleFromGatewayResult.ToResult();
            
            //If gateway has the vehicle, save it to the repo
            var addResult = await repository.Add(vehicleFromGatewayResult.Value);
            addResult.LogIfFailed<ResultBasedVehicleService>();
            
            return MapToVehicle(vehicleFromGatewayResult.Value);
        }

        return vehicleFromRepoResult.ToResult();
    }

    public async Task<Result<List<Vehicle>>> GetVehicles(List<string>? ids)
    {
        if(ids == null || ids.Count == 0)
            return Result.Fail(new ValidationError("Ids are required"));
        
        var vehiclesResults = new List<Result<Vehicle>>();

        foreach (var id in ids)
        {
            var vehicleResult = await GetVehicle(id);
            vehiclesResults.Add(vehicleResult);
        }

        var mergedResult = vehiclesResults
            .ToArray()
            .Merge();
        
        mergedResult.LogIfFailed<ResultBasedVehicleService>();
        
        return vehiclesResults
            .Where(s=>s.IsSuccess)
            .Select(v => v.Value)
            .ToList();
    }
    
    //From change feed
    public async Task<Result> UpdateVehicle(string id)
    {
        var vehicleDtoResult = await gateway.GetById(id);
        if(vehicleDtoResult.IsFailed) return vehicleDtoResult.ToResult();
        
        return await repository.Add(vehicleDtoResult.Value);
    }

    private static Result<Vehicle> MapToVehicle(VehicleDto vehicleDto)
    {
        return Vehicle.Create(vehicleDto.Id,vehicleDto.Type, vehicleDto.Make, vehicleDto.Model, vehicleDto.Year);
    }
}