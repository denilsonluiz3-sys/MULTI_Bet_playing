using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class WebViewPage : ContentPage
{
    public WebViewPage(string url, string title = "Site")
    {
        InitializeComponent();

        if (!UrlValidator.TryNormalize(url, out var safe, out var err))
        {
            TitleLabel.Text = "URL bloqueada";
            Title = "Erro";
            MainWebView.Source = null;
            _ = DisplayAlert("Bloqueado", err, "OK");
            return;
        }

        TitleLabel.Text = title;
        MainWebView.Source = safe;
        Title = title;
    }

    private void OnReload(object? sender, EventArgs e) => MainWebView.Reload();

    private async void OnClose(object? sender, EventArgs e) => await Navigation.PopAsync();
}
