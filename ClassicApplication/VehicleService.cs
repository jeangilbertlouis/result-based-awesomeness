namespace ClassicApplication;

public class VehicleService(IClassicVehicleRepository repository, IClassicVehicleGateway gateway)
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
            var vehicleDto =  await gateway.GetById(id);
            await repository.Add(vehicleDto);
            return MapToVehicle(vehicleDto);
        }
    }
    
    public async Task<List<Vehicle?>> GetVehicles(IEnumerable<string> ids)
    {
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
        await repository.Add(vehicleDto);
    }
    
    private static Vehicle MapToVehicle(VehicleDto vehicleDto)
    {
        return Vehicle.Create(vehicleDto.Id,vehicleDto.Type, vehicleDto.Make, vehicleDto.Model, vehicleDto.Year);
    }
}



