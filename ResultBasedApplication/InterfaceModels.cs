using FluentResults;

namespace ResultBasedApplication;

public interface IResultBasedVehicleRepository
{
    Task<Result<VehicleDto>> GetById(string id);
    Task<Result> Upsert(VehicleDto vehicle);
}

public class NotFoundError(string message) : Error(message);
public class ValidationError(string message) : Error(message);

public interface IResultBasedVehicleGateway
{
    Task<Result<VehicleDto>> GetById(string id);
}

public interface IResultBasedSerializer
{
    Result<string> Serialize(object data);
    Result<T> Deserialize<T>(string jsonData);
}

public class VehicleDto
{
    public string Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Year { get; set; }
    public string Type { get; set; }
}