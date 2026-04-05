namespace CurrencyApp.Models
{
    public class HistoricalRate
    {
        public string No { get; set; } = string.Empty;
        public string EffectiveDate { get; set; } = string.Empty;
        public decimal Mid { get; set; }
    }
}