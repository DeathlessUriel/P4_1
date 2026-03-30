using System.Text.Json;
using CurrencyApp.Models;

namespace CurrencyApp.Services
{
    public class LocalStorageService
    {
        private readonly string _ratesFilePath;
        private readonly string _historyFilePath;

        public LocalStorageService()
        {
            _ratesFilePath = Path.Combine(FileSystem.AppDataDirectory, "rates.json");
            _historyFilePath = Path.Combine(FileSystem.AppDataDirectory, "history.json");
        }

        public async Task SaveRatesAsync(List<CurrencyRate> rates, string effectiveDate)
        {
            var data = new CurrencyTableResponse
            {
                EffectiveDate = effectiveDate,
                Rates = rates
            };

            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(_ratesFilePath, json);
        }

        public async Task<(List<CurrencyRate> Rates, string EffectiveDate)> LoadRatesAsync()
        {
            if (!File.Exists(_ratesFilePath))
                return (new List<CurrencyRate>(), string.Empty);

            var json = await File.ReadAllTextAsync(_ratesFilePath);

            var data = JsonSerializer.Deserialize<CurrencyTableResponse>(json);

            if (data == null)
                return (new List<CurrencyRate>(), string.Empty);

            return (data.Rates, data.EffectiveDate);
        }

        public async Task SaveHistoryAsync(List<ConversionHistoryItem> history)
        {
            var json = JsonSerializer.Serialize(history);
            await File.WriteAllTextAsync(_historyFilePath, json);
        }

        public async Task<List<ConversionHistoryItem>> LoadHistoryAsync()
        {
            if (!File.Exists(_historyFilePath))
                return new List<ConversionHistoryItem>();

            var json = await File.ReadAllTextAsync(_historyFilePath);

            var data = JsonSerializer.Deserialize<List<ConversionHistoryItem>>(json);

            return data ?? new List<ConversionHistoryItem>();
        }
    }
}