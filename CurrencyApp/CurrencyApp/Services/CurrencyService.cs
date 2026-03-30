using System.Text.Json;
using CurrencyApp.Models;

namespace CurrencyApp.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.nbp.pl/api/exchangerates/tables/A/?format=json";

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(List<CurrencyRate> Rates, string EffectiveDate)> GetRatesAsync()
        {
            var response = await _httpClient.GetAsync(ApiUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Nie udało się pobrać kursów walut z API.");
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var tables = JsonSerializer.Deserialize<List<CurrencyTableResponse>>(json, options);

            if (tables == null || tables.Count == 0)
            {
                throw new Exception("API zwróciło pustą odpowiedź.");
            }

            var table = tables[0];

            return (table.Rates.OrderBy(r => r.Code).ToList(), table.EffectiveDate);
        }
    }
}