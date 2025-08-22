using System.Text.Json.Serialization;

namespace RevueCraftersTests.Models
{
    class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("revueid")]
        public string? RevueId { get; set; }
    }
}
