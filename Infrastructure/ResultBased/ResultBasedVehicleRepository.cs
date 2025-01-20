using System.Text;
using Azure;
using Azure.Storage.Blobs;
using FluentResults;
using ResultBasedApplication;

namespace Infrastructure.ResultBased;

public class ResultBasedVehicleRepository(IResultBasedSerializer serializer, BlobContainerClient containerClient) : IResultBasedVehicleRepository
{
    public async Task<Result<VehicleDto>> GetById(string id)
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
                return Result.Fail(new NotFoundError($"Vehicle with id {id} not found"));
            
            
            return Result.Fail(new ExceptionalError(e.Message,e));
        }
    }

    public async Task<Result> Upsert(VehicleDto? vehicle)
    {
        try
        {
            if(vehicle == null) return Result.Fail(new ValidationError("Vehicle cannot be null"));
        
            var blobClient = containerClient.GetBlobClient(vehicle.Id);
            var contentResult = serializer.Serialize(vehicle);
            if(contentResult.IsFailed) return contentResult.ToResult();
            await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(contentResult.Value)), true);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e.Message,e));
        }
    }
}