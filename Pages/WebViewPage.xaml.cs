namespace MULTI_Bet_playing_Demo.Pages;

public partial class WebViewPage : ContentPage
{
    public WebViewPage(string url, string title = "Cassino")
    {
        InitializeComponent();
        TitleLabel.Text = title;
        MainWebView.Source = url;
        Title = title;
    }

    private void OnReload(object? sender, EventArgs e)
    {
        MainWebView.Reload();
    }

    private async void OnClose(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
