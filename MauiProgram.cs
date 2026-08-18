using Microsoft.Extensions.Logging;
using MULTI_Bet_playing_Demo.Services.Logging;

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

        if (MultiBetLoggingOptions.Enabled)
            builder.Services.AddSingleton<IMultiBetLogger, MultiBetFileLogger>();
        else
            builder.Services.AddSingleton<IMultiBetLogger, NullMultiBetLogger>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        app.Services.GetRequiredService<IMultiBetLogger>().Info("Application container initialized", "Startup");
        return app;
    }
}
