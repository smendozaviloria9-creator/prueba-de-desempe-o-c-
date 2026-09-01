using System.Text.Json.Serialization;

namespace Financiera.Models
{
    public class TrmDto
    {
        [JsonPropertyName("valor")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("vigenciadesde")]
        public string ValidFrom { get; set; } = string.Empty;

        [JsonPropertyName("vigenciahasta")]
        public string ValidUntil { get; set; } = string.Empty;
    }
}