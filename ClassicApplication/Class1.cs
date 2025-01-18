namespace ClassicApplication;

public class Class1
{
}

public interface IClassicVehicleRepository
{
    Task<VehicleDto> GetById(string id);
    Task Add(VehicleDto vehicle);
}

public class NotFoundException(string message) : Exception(message);

public interface IClassicVehicleGateway
{
    Task<VehicleDto> GetById(string id);
}

public interface ISerializer
{
    string Serialize(object data);
    T Deserialize<T>(string jsonData);
}

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
    
    public async Task UpdateVehicle(Vehicle vehicle)
    {
        var vehicleDto = new VehicleDto
        {
            Id = vehicle.Id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Type = vehicle.Type
        };
        
        await repository.Add(vehicleDto);
    }
    
    private static Vehicle MapToVehicle(VehicleDto vehicleDto)
    {
        return Vehicle.Create(vehicleDto.Id,vehicleDto.Type, vehicleDto.Make, vehicleDto.Model, vehicleDto.Year);
    }
}

public class VehicleDto
{
    public string Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Year { get; set; }
    public string Type { get; set; }
}

public abstract class Vehicle
{
    protected Vehicle(string id, string make, string model, string year)
    {
        Id = id;
        Make = make;
        Model = model;
        Year = year;
    }

    public string Id { get; }
    public string Make { get; }
    public string Model { get; }
    public string Year { get; }
    public abstract string Type { get; }
    
    public static Vehicle Create(string id, string type, string make, string model, string year)
    {
        return type switch
        {
            "Car" => new Car(id, make, model, year),
            "Truck" => new Truck(id, make, model, year),
            _ => throw new ArgumentException("Invalid vehicle type")
        };
    }
}

public class Car : Vehicle
{
    internal Car(string id, string make, string model, string year) : base(id ,make, model, year)
    {
    }

    public override string Type => "Car";
}

public class Truck : Vehicle
{
    internal Truck(string id, string make, string model, string year) : base(id, make, model, year)
    {
    }

    public override string Type => "Truck";
}