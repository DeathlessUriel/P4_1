using CurrencyApp.ViewModels;

namespace CurrencyApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is MainPageViewModel vm && vm.Rates.Count == 0)
            {
                await vm.LoadRatesAsync();
            }
        }
    }
}