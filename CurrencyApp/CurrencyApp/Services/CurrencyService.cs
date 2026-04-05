using System.Text.Json;
using CurrencyApp.Models;

namespace CurrencyApp.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.nbp.pl/api/exchangerates";

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CurrencyTableResponse> GetRatesAsync()
        {
            string url = $"{BaseUrl}/tables/A?format=json";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Nie udało się pobrać kursów walut z API NBP.");

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<List<CurrencyTableResponse>>(json, options);

            if (result == null || result.Count == 0)
                throw new Exception("API NBP zwróciło pustą odpowiedź.");

            return result[0];
        }

        public async Task<List<HistoricalRate>> GetHistoricalRatesAsync(string code, DateTime startDate, DateTime endDate)
        {
            string start = startDate.ToString("yyyy-MM-dd");
            string end = endDate.ToString("yyyy-MM-dd");

            string url = $"{BaseUrl}/rates/A/{code}/{start}/{end}/?format=json";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Nie udało się pobrać danych historycznych.");

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<HistoricalRateResponse>(json, options);

            if (result == null || result.Rates == null || result.Rates.Count == 0)
                throw new Exception("Brak danych historycznych dla wybranej waluty.");

            return result.Rates;
        }
    }
}