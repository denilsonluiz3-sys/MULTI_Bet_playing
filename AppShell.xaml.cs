namespace MULTI_Bet_playing_Demo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(Pages.WebViewPage), typeof(Pages.WebViewPage));
    }
}
