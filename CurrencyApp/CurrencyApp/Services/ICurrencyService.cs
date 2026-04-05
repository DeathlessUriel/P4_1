using CurrencyApp.Models;

namespace CurrencyApp.Services
{
    public interface ICurrencyService
    {
        Task<CurrencyTableResponse> GetRatesAsync();
        Task<List<HistoricalRate>> GetHistoricalRatesAsync(string code, DateTime startDate, DateTime endDate);
    }
}