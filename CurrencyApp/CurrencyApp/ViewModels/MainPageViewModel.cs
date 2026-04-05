using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyApp.Models;
using CurrencyApp.Services;

namespace CurrencyApp.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly ICurrencyService _currencyService;
        private readonly LocalStorageService _localStorageService;

        private List<CurrencyRate> _allRates = new();

        public ObservableCollection<CurrencyRate> Rates { get; } = new();
        public ObservableCollection<ConversionHistoryItem> ConversionHistory { get; } = new();

        [ObservableProperty]
        private CurrencyRate? selectedCurrency;

        [ObservableProperty]
        private string amountPln = string.Empty;

        [ObservableProperty]
        private string amountForeign = string.Empty;

        [ObservableProperty]
        private string resultText = string.Empty;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string effectiveDate = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isOfflineMode;

        [ObservableProperty]
        private string currentRateText = "-";

        [ObservableProperty]
        private string minRateText = "-";

        [ObservableProperty]
        private string maxRateText = "-";

        [ObservableProperty]
        private string averageRateText = "-";

        [ObservableProperty]
        private string trendText = "-";

        [ObservableProperty]
        private string trendIcon = "➖";

        [ObservableProperty]
        private string trendColorHex = "#6B7280";
        public MainPageViewModel(ICurrencyService currencyService, LocalStorageService localStorageService)
        {
            _currencyService = currencyService;
            _localStorageService = localStorageService;
        }

        private void CalculateTrend(List<decimal> values)
        {
            if (values.Count < 2)
            {
                TrendText = "Brak danych";
                TrendIcon = "➖";
                TrendColorHex = "#6B7280";
                return;
            }

            decimal first = values.First();
            decimal last = values.Last();
            decimal difference = last - first;

            if (difference > 0.01m)
            {
                TrendText = "Trend rosnący";
                TrendIcon = "📈";
                TrendColorHex = "#10B981";
            }
            else if (difference < -0.01m)
            {
                TrendText = "Trend spadkowy";
                TrendIcon = "📉";
                TrendColorHex = "#EF4444";
            }
            else
            {
                TrendText = "Trend stabilny";
                TrendIcon = "➖";
                TrendColorHex = "#F59E0B";
            }
        }

        [RelayCommand]
        public async Task LoadStatisticsAsync()
        {
            if (SelectedCurrency == null)
            {
                ErrorMessage = "Najpierw wybierz walutę.";
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-14);

                var historicalRates = await _currencyService.GetHistoricalRatesAsync(
                    SelectedCurrency.Code,
                    startDate,
                    endDate);

                var data = historicalRates
                    .OrderBy(r => r.EffectiveDate)
                    .ToList();

                if (!data.Any())
                {
                    ErrorMessage = "Brak danych statystycznych.";
                    return;
                }

                var values = data.Select(x => x.Mid).ToList();

                CurrentRateText = data.Last().Mid.ToString("F4");
                MinRateText = values.Min().ToString("F4");
                MaxRateText = values.Max().ToString("F4");
                AverageRateText = values.Average().ToString("F4");

                CalculateTrend(values);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd ładowania statystyk: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        partial void OnSelectedCurrencyChanged(CurrencyRate? value)
        {
            if (value != null)
            {
                _ = LoadStatisticsAsync();
            }
            else
            {
                CurrentRateText = "-";
                MinRateText = "-";
                MaxRateText = "-";
                AverageRateText = "-";
                TrendText = "-";
                TrendIcon = "➖";
                TrendColorHex = "#6B7280";
            }
        }

        [RelayCommand]
        public async Task LoadRatesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                IsOfflineMode = false;

                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    await LoadOfflineRatesAsync("Brak internetu. Załadowano ostatnie zapisane dane.");
                    return;
                }

                Rates.Clear();

                var result = await _currencyService.GetRatesAsync();

                _allRates = result.Rates;
                EffectiveDate = $"Data kursów: {result.EffectiveDate}";

                foreach (var rate in result.Rates)
                {
                    Rates.Add(rate);
                }

                await _localStorageService.SaveRatesAsync(result.Rates, result.EffectiveDate);
                await LoadHistoryAsync();
            }
            catch (Exception ex)
            {
                await LoadOfflineRatesAsync($"Błąd pobierania danych online: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadOfflineRatesAsync(string message)
        {
            var offlineData = await _localStorageService.LoadRatesAsync();

            Rates.Clear();
            _allRates.Clear();

            if (offlineData.Rates.Any())
            {
                _allRates = offlineData.Rates.OrderBy(r => r.Code).ToList();

                foreach (var rate in _allRates)
                {
                    Rates.Add(rate);
                }

                EffectiveDate = $"Ostatnie zapisane dane: {offlineData.EffectiveDate}";
                ErrorMessage = message;
                IsOfflineMode = true;
            }
            else
            {
                ErrorMessage = "Nie udało się pobrać danych i brak zapisanych danych lokalnych.";
            }

            await LoadHistoryAsync();
        }

        [RelayCommand]
        public void ConvertFromPln()
        {
            if (SelectedCurrency == null)
            {
                ResultText = "Wybierz walutę.";
                return;
            }

            if (!TryParseDecimal(AmountPln, out decimal plnAmount))
            {
                ResultText = "Podaj poprawną kwotę w PLN.";
                return;
            }

            decimal converted = plnAmount / SelectedCurrency.Mid;
            ResultText = $"{plnAmount:F2} PLN = {converted:F2} {SelectedCurrency.Code}";

            AddToHistory(ResultText);
        }

        [RelayCommand]
        public void ConvertToPln()
        {
            if (SelectedCurrency == null)
            {
                ResultText = "Wybierz walutę.";
                return;
            }

            if (!TryParseDecimal(AmountForeign, out decimal foreignAmount))
            {
                ResultText = "Podaj poprawną kwotę w wybranej walucie.";
                return;
            }

            decimal converted = foreignAmount * SelectedCurrency.Mid;
            ResultText = $"{foreignAmount:F2} {SelectedCurrency.Code} = {converted:F2} PLN";

            AddToHistory(ResultText);
        }

        [RelayCommand]
        public void ClearHistory()
        {
            ConversionHistory.Clear();
            _ = _localStorageService.SaveHistoryAsync(ConversionHistory.ToList());
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterRates();
        }

        private void FilterRates()
        {
            Rates.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allRates
                : _allRates.Where(r =>
                    r.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Currency.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                  .ToList();

            foreach (var rate in filtered)
            {
                Rates.Add(rate);
            }
        }

        private bool TryParseDecimal(string input, out decimal result)
        {
            input = input.Replace(",", ".");

            return decimal.TryParse(
                input,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out result);
        }

        private async void AddToHistory(string description)
        {
            ConversionHistory.Insert(0, new ConversionHistoryItem
            {
                Description = description,
                Date = DateTime.Now
            });

            await _localStorageService.SaveHistoryAsync(ConversionHistory.ToList());
        }

        private async Task LoadHistoryAsync()
        {
            ConversionHistory.Clear();

            var history = await _localStorageService.LoadHistoryAsync();

            foreach (var item in history.OrderByDescending(h => h.Date))
            {
                ConversionHistory.Add(item);
            }
        }

    }
}