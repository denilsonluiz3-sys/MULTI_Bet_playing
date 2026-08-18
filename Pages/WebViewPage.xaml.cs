using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class WebViewPage : ContentPage
{
    private readonly string _homeUrl;
    private readonly string _title;
    private bool _desktopUa;

    public WebViewPage(string url, string title = "Site")
    {
        InitializeComponent();
        _title = title;

        if (!UrlValidator.TryNormalize(url, out var safe, out var err))
        {
            TitleLabel.Text = "URL bloqueada";
            Title = "Erro";
            _homeUrl = string.Empty;
            MainWebView.Source = null;
            _ = DisplayAlertAsync("Bloqueado", err, "OK");
            return;
        }

        _homeUrl = safe;
        TitleLabel.Text = title;
        Title = title;
        UrlLabel.Text = safe;
        MainWebView.Source = safe;
    }

    protected override bool OnBackButtonPressed()
    {
        try
        {
            if (MainWebView.CanGoBack)
            {
                MainWebView.GoBack();
                return true;
            }
        }
        catch { }
        return base.OnBackButtonPressed();
    }

    private void OnBack(object? sender, EventArgs e)
    {
        try { if (MainWebView.CanGoBack) MainWebView.GoBack(); } catch { }
    }

    private void OnForward(object? sender, EventArgs e)
    {
        try { if (MainWebView.CanGoForward) MainWebView.GoForward(); } catch { }
    }

    private void OnReload(object? sender, EventArgs e)
    {
        try { MainWebView.Reload(); } catch { }
    }

    private void OnHome(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(_homeUrl))
            MainWebView.Source = _homeUrl;
    }

    private async void OnClose(object? sender, EventArgs e)
    {
        try { await Navigation.PopAsync(); } catch { }
    }

    private void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        try { if (!string.IsNullOrEmpty(e.Url)) UrlLabel.Text = e.Url; } catch { }
    }

    private void OnNavigated(object? sender, WebNavigatedEventArgs e)
    {
        try { if (!string.IsNullOrEmpty(e.Url)) UrlLabel.Text = e.Url; } catch { }
    }

    private async void OnMenu(object? sender, EventArgs e)
    {
        try
        {
            var choice = await DisplayActionSheetAsync(
                "Navegador", "Cancelar", null,
                "Abrir no Chrome / navegador do sistema",
                _desktopUa ? "Modo mobile (UA)" : "Modo desktop (UA)",
                "Copiar URL", "Compartilhar URL", "Limpar cookies deste app");

            var url = UrlLabel.Text ?? _homeUrl;

            switch (choice)
            {
                case "Abrir no Chrome / navegador do sistema":
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                        await Launcher.Default.OpenAsync(uri);
                    break;
                case "Modo desktop (UA)":
                case "Modo mobile (UA)":
                    _desktopUa = !_desktopUa;
                    ApplyUserAgent(_desktopUa);
                    MainWebView.Reload();
                    break;
                case "Copiar URL":
                    await Clipboard.Default.SetTextAsync(url);
                    await DisplayAlertAsync("OK", "URL copiada.", "OK");
                    break;
                case "Compartilhar URL":
                    await Share.Default.RequestAsync(new ShareTextRequest { Text = url, Title = _title });
                    break;
                case "Limpar cookies deste app":
                    WebViewSecurity.ClearCookiesAndCache();
                    await DisplayAlertAsync("OK", "Cookies limpos. Recarregue e tente o login de novo.", "OK");
                    break;
            }
        }
        catch (Exception ex)
        {
            try { await DisplayAlertAsync("Erro", ex.Message, "OK"); } catch { }
        }
    }

    private void ApplyUserAgent(bool desktop)
    {
#if ANDROID
        try
        {
            if (MainWebView.Handler?.PlatformView is Android.Webkit.WebView aw)
            {
                aw.Settings.UserAgentString = desktop
                    ? WebViewSecurity.ChromeDesktopUa
                    : WebViewSecurity.ChromeMobileUa;
            }
        }
        catch { }
#endif
    }
}
