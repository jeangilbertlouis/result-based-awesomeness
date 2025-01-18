using System.Net;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using ClassicApplication;

namespace Infrastructure;

public class ClassicVehicleRepository(ISerializer serializer, BlobContainerClient containerClient)
    : IClassicVehicleRepository
{
    public async Task<VehicleDto> GetById(string id)
    {
        if(string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty", nameof(id));
        
        try
        {
            var blobClient = containerClient.GetBlobClient(id);
            var response = await blobClient.DownloadAsync();
            var content = await new StreamReader(response.Value.Content, Encoding.UTF8).ReadToEndAsync();
            return serializer.Deserialize<VehicleDto>(content);
        }
        catch (Exception e)
        {
            if(e is RequestFailedException { Status: 404 })
                throw new NotFoundException($"Vehicle with id {id} not found");
            
            throw;
        }
    }

    public async Task Add(VehicleDto vehicle)
    {
        if(vehicle == null)
            throw new ArgumentNullException(nameof(vehicle));
        
        var blobClient = containerClient.GetBlobClient(vehicle.Id);
        var content = serializer.Serialize(vehicle);
        await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)), true);
    }
}

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