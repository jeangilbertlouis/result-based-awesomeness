using FluentResults;

namespace ResultBasedApplication;

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
    
    public static Result<Vehicle> Create(string id, string type, string make, string model, string year)
    {
        return type switch
        {
            "Car" => new Car(id, make, model, year),
            "Truck" => new Truck(id, make, model, year),
            _ => Result.Fail("Invalid vehicle type")
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