namespace CurrencyApp.Models
{
    public class HistoricalRateResponse
    {
        public string Table { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public List<HistoricalRate> Rates { get; set; } = new();
    }
}