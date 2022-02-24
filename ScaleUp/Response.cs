using System.Text.Json.Serialization;

namespace ScaleUp;

public class Response
{
    [JsonPropertyName("job_id")]
    public int JobId { get; set; }

    [JsonPropertyName("output_url")]
    public Uri? OutputUrl { get; set; }
}