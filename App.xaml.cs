using MULTI_Bet_playing_Demo.Pages;
using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        ThemeManager.ApplySavedTheme();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Page root = ComplianceService.HasAccepted
            ? new AppShell()
            : new CompliancePage();
        return new Window(root);
    }
}
