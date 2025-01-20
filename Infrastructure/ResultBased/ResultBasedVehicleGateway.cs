using System.Net;
using FluentResults;
using ResultBasedApplication;

namespace Infrastructure.ResultBased;

public class ResultBasedVehicleGateway(HttpClient httpClient, IResultBasedSerializer serializer) : IResultBasedVehicleGateway
{
    public async Task<Result<VehicleDto>> GetById(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return Result.Fail(new ValidationError("Id cannot be null or empty"));

            var response = await httpClient.GetAsync($"vehicles/{id}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
                return Result.Fail(new NotFoundError($"Vehicle with id {id} not found"));
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return serializer.Deserialize<ResultBasedApplication.VehicleDto>(content);
        }
        catch (Exception e)
        {
            return Result.Fail(new Error(e.Message).CausedBy(e));
        }
    }
}