using CurrencyApp.Models;

namespace CurrencyApp.Services
{
    public interface ICurrencyService
    {
        Task<(List<CurrencyRate> Rates, string EffectiveDate)> GetRatesAsync();
    }
}