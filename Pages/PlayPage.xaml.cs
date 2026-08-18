using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class PlayPage : ContentPage
{
    private readonly CardService _cardService;
    private bool _isTopFullScreen;
    private bool _isBottomFullScreen;

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
        WebViewTop.IsVisible = false;
        WebViewBottom.IsVisible = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        WebViewTop.IsVisible = true;
        WebViewBottom.IsVisible = true;
    }

    private async void OnLoadFavorites(object? sender, EventArgs e)
    {
        var favorites = await _cardService.GetFavoritesAsync();
        if (favorites.Count == 0)
        {
            await DisplayAlert("Aviso", "Nenhum favorito. Adicione links na aba Inicio.", "OK");
            return;
        }

        if (!UrlValidator.TryNormalize(favorites[0].Url, out var url0, out var err0))
        {
            await DisplayAlert("URL invalida", err0, "OK");
            return;
        }

        WebViewTop.Source = url0;
        LabelTop.Text = favorites[0].Title;
        await _cardService.MarkUsedAsync(favorites[0].Id);

        if (favorites.Count > 1 && UrlValidator.TryNormalize(favorites[1].Url, out var url1, out _))
        {
            WebViewBottom.Source = url1;
            LabelBottom.Text = favorites[1].Title;
            await _cardService.MarkUsedAsync(favorites[1].Id);
        }
    }

    private void OnClearAll(object? sender, EventArgs e)
    {
        WebViewTop.Source = null;
        WebViewBottom.Source = null;
        LabelTop.Text = "Tela Superior";
        LabelBottom.Text = "Tela Inferior";
        ResetFullScreen();
    }

    private void OnToggleFullScreen(object? sender, EventArgs e)
    {
        if (_isTopFullScreen || _isBottomFullScreen) { ResetFullScreen(); return; }
        OnExpandTop(sender, e);
    }

    private void OnExpandTop(object? sender, EventArgs e)
    {
        if (_isTopFullScreen) { ResetFullScreen(); return; }
        BorderBottom.IsVisible = false;
        Grid.SetRowSpan(BorderTop, 2);
        _isTopFullScreen = true;
        _isBottomFullScreen = false;
        ToggleFullBtn.Text = "Restaurar";
    }

    private void OnExpandBottom(object? sender, EventArgs e)
    {
        if (_isBottomFullScreen) { ResetFullScreen(); return; }
        BorderTop.IsVisible = false;
        Grid.SetRow(BorderBottom, 1);
        Grid.SetRowSpan(BorderBottom, 2);
        _isBottomFullScreen = true;
        _isTopFullScreen = false;
        ToggleFullBtn.Text = "Restaurar";
    }

    private void ResetFullScreen()
    {
        BorderTop.IsVisible = true;
        BorderBottom.IsVisible = true;
        Grid.SetRow(BorderTop, 1);
        Grid.SetRowSpan(BorderTop, 1);
        Grid.SetRow(BorderBottom, 2);
        Grid.SetRowSpan(BorderBottom, 1);
        _isTopFullScreen = false;
        _isBottomFullScreen = false;
        ToggleFullBtn.Text = "Tela Cheia";
    }
}
