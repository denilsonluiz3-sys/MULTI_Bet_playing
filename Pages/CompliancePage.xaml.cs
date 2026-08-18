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
            await DisplayAlertAsync(
                "Pendências",
                "Marque todas as confirmações (idade, termos e região) para continuar.",
                "OK");
            return;
        }

        ComplianceService.Accept();
        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = new AppShell();
    }

    private void OnExit(object? sender, EventArgs e)
    {
        Application.Current?.Quit();
#if ANDROID
        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#endif
    }
}
