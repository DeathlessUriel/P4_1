using System.Text.Json.Serialization;

namespace CurrencyApp.Models
{
    public class CurrencyTableResponse
    {
        [JsonPropertyName("table")]
        public string Table { get; set; } = string.Empty;

        [JsonPropertyName("no")]
        public string No { get; set; } = string.Empty;

        [JsonPropertyName("effectiveDate")]
        public string EffectiveDate { get; set; } = string.Empty;

        [JsonPropertyName("rates")]
        public List<CurrencyRate> Rates { get; set; } = new();
    }
}