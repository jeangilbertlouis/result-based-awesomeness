using System.Net;
using ClassicApplication;
using VehicleDto = ClassicApplication.VehicleDto;

namespace Infrastructure.Classic;

public class ClassicVehicleGateway(HttpClient httpClient, ISerializer serializer) : IClassicVehicleGateway
{
    public async Task<VehicleDto> GetById(string id)
    {
        if(string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty", nameof(id));
        
        var response = await httpClient.GetAsync($"vehicles/{id}");
        
        if(response.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException($"Vehicle with id {id} not found");
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return serializer.Deserialize<VehicleDto>(content);
    }
}