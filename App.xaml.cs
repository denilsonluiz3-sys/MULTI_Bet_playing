using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        ThemeManager.ApplySavedTheme();
        MainPage = new AppShell();
    }
}
