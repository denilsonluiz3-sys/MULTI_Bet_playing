using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class CompliancePage : ContentPage
{
    public CompliancePage()
    {
        InitializeComponent();
        DisclaimerLabel.Text = ComplianceService.DisclaimerFull;
    }

    private async void OnAccept(object? sender, EventArgs e)
    {
        if (AgeCheck.IsChecked != true || TermsCheck.IsChecked != true || RegionCheck.IsChecked != true)
        {
            await DisplayAlert(
                "Pend\u00eancias",
                "Marque todas as confirma\u00e7\u00f5es (idade, termos e regi\u00e3o) para continuar.",
                "OK");
            return;
        }

        ComplianceService.Accept();
        Application.Current!.MainPage = new AppShell();
    }

    private void OnExit(object? sender, EventArgs e)
    {
        Application.Current?.Quit();
#if ANDROID
        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#endif
    }
}
