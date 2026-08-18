using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class PlayPage : ContentPage
{
    private readonly CardService _cardService;
    private bool _isLeftFull;
    private bool _isRightFull;

    public PlayPage() : this(ResolveCardService()) { }

    public PlayPage(CardService cardService)
    {
        InitializeComponent();
        _cardService = cardService;
    }

    private static CardService ResolveCardService() =>
        Application.Current?.Handler?.MauiContext?.Services?.GetService<CardService>()
        ?? new CardService();

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WebViewLeft.IsVisible = false;
        WebViewRight.IsVisible = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        WebViewLeft.IsVisible = true;
        WebViewRight.IsVisible = true;
        AppLog.Info("PlayPage.OnAppearing (2 telas lado a lado | 1 | 2 |)");
    }

    private async void OnLoadFavorites(object? sender, EventArgs e)
    {
        var favorites = await _cardService.GetFavoritesAsync();
        AppLog.Info($"Play: favoritos={favorites.Count}");
        if (favorites.Count == 0)
        {
            await DisplayAlertAsync("Aviso",
                "Nenhum favorito. Na Início toque ⭐ Favorito no card.",
                "OK");
            return;
        }

        if (!UrlValidator.TryNormalize(favorites[0].Url, out var url0, out var err0))
        {
            await DisplayAlertAsync("URL inválida", err0, "OK");
            return;
        }

        WebViewLeft.Source = url0;
        LabelLeft.Text = favorites[0].Title;
        await _cardService.MarkUsedAsync(favorites[0].Id);

        if (favorites.Count > 1 && UrlValidator.TryNormalize(favorites[1].Url, out var url1, out _))
        {
            WebViewRight.Source = url1;
            LabelRight.Text = favorites[1].Title;
            await _cardService.MarkUsedAsync(favorites[1].Id);
        }
    }

    private void OnClearAll(object? sender, EventArgs e)
    {
        WebViewLeft.Source = null;
        WebViewRight.Source = null;
        LabelLeft.Text = "Tela 1";
        LabelRight.Text = "Tela 2";
        ResetFullScreen();
    }

    private void OnToggleFullScreen(object? sender, EventArgs e)
    {
        if (_isLeftFull || _isRightFull)
        {
            ResetFullScreen();
            return;
        }
        OnExpandLeft(sender, e);
    }

    private void OnExpandLeft(object? sender, EventArgs e)
    {
        if (_isLeftFull)
        {
            ResetFullScreen();
            return;
        }
        BorderRight.IsVisible = false;
        Grid.SetColumnSpan(BorderLeft, 2);
        _isLeftFull = true;
        _isRightFull = false;
        ToggleFullBtn.Text = "Restaurar";
    }

    private void OnExpandRight(object? sender, EventArgs e)
    {
        if (_isRightFull)
        {
            ResetFullScreen();
            return;
        }
        BorderLeft.IsVisible = false;
        Grid.SetColumn(BorderRight, 0);
        Grid.SetColumnSpan(BorderRight, 2);
        _isRightFull = true;
        _isLeftFull = false;
        ToggleFullBtn.Text = "Restaurar";
    }

    private void ResetFullScreen()
    {
        BorderLeft.IsVisible = true;
        BorderRight.IsVisible = true;
        Grid.SetColumn(BorderLeft, 0);
        Grid.SetColumnSpan(BorderLeft, 1);
        Grid.SetColumn(BorderRight, 1);
        Grid.SetColumnSpan(BorderRight, 1);
        _isLeftFull = false;
        _isRightFull = false;
        ToggleFullBtn.Text = "Tela cheia";
    }
}
