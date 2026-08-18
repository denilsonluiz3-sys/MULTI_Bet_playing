using MULTI_Bet_playing_Demo.Pages;
using MULTI_Bet_playing_Demo.Services;
using Microsoft.Extensions.Logging;

namespace MULTI_Bet_playing_Demo;

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

        builder.Services.AddSingleton<CardService>();

        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<DemoPage>();
        builder.Services.AddTransient<PlayPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<WebViewPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
