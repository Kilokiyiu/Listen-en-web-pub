namespace IdentitySerivce.Infrastructure.Services;

using System.Text.Json.Serialization;

public class SendCloudResponseModel
{
    [JsonPropertyName("result")]
    public bool Result { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}