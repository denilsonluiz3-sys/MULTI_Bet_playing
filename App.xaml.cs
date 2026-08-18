using MULTI_Bet_playing_Demo.Pages;
using MULTI_Bet_playing_Demo.Services;
using MULTI_Bet_playing_Demo.Services.Logging;

namespace MULTI_Bet_playing_Demo;

public partial class App : Application
{
    private readonly IMultiBetLogger _logger;

    public App(IMultiBetLogger logger)
    {
        InitializeComponent();
        _logger = logger;
        ThemeManager.ApplySavedTheme();
        _logger.Info("Application initialized", "App");

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            _logger.Error("Unhandled application exception", args.ExceptionObject as Exception, "AppDomain");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            Page root = ComplianceService.HasAccepted
                ? new AppShell()
                : new CompliancePage();

            _logger.Info($"Root page created: {root.GetType().Name}", "Navigation");
            return new Window(root);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to create application window", ex, "Navigation");
            throw;
        }
    }
}
