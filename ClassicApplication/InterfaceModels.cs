namespace ClassicApplication;

public interface IClassicVehicleRepository
{
    Task<VehicleDto> GetById(string id);
    Task Upsert(VehicleDto vehicle);
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

public class VehicleDto
{
    public string Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Year { get; set; }
    public string Type { get; set; }
}