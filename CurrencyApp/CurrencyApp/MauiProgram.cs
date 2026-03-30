using CurrencyApp.Services;
using CurrencyApp.ViewModels;
using CurrencyApp.Views;
using Microsoft.Extensions.Logging;

namespace CurrencyApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<ICurrencyService, CurrencyService>();
            builder.Services.AddSingleton<LocalStorageService>();

            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}