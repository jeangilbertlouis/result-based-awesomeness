namespace ClassicApplication;

public class ClassicVehicleService(IClassicVehicleRepository repository, IClassicVehicleGateway gateway)
{
    public async Task<Vehicle> GetVehicle(string id)
    {
        //Validate id
        if(string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty", nameof(id));
        
        //Try to get vehicle from repository
        try
        {
            var vehicleDto = await repository.GetById(id);
            return MapToVehicle(vehicleDto);
        }
        catch (NotFoundException)
        {
            //If not found, try to get from gateway
            var vehicleDto =  await gateway.GetById(id);
            
            //Save to repository for future use
            await repository.Upsert(vehicleDto);
            return MapToVehicle(vehicleDto);
        }
    }
    
    public async Task<List<Vehicle?>> GetVehicles(List<string>? ids)
    {
        if(ids is null || ids.Count == 0)
            throw new ArgumentException("Ids cannot be null or empty", nameof(ids));
        
        var vehicles = new List<Vehicle?>();
        
        foreach (var id in ids)
        {
            Vehicle? vehicle = null;
            
            try
            {
                vehicle = await GetVehicle(id);
            }
            catch (Exception)
            {
                //Log error
                //Dont break the loop
            }
            if(vehicle is not null)vehicles.Add(vehicle);
        }
        
        return vehicles.ToList();
    }
    
    //From change feed
    public async Task UpdateVehicle(string id)
    {
        var vehicleDto = await gateway.GetById(id);
        await repository.Upsert(vehicleDto);
    }
    
    private static Vehicle MapToVehicle(VehicleDto vehicleDto)
    {
        return Vehicle.Create(vehicleDto.Id,vehicleDto.Type, vehicleDto.Make, vehicleDto.Model, vehicleDto.Year);
    }
}



