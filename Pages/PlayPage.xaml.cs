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

    private async void OnLoadFavorites(object? sender, EventArgs e)
    {
        var favorites = await _cardService.GetFavoritesAsync();
        if (favorites.Count == 0)
        {
            await DisplayAlert("Aviso", "Nenhum favorito encontrado. Adicione cassinos na aba Início.", "OK");
            return;
        }

        WebViewTop.Source = favorites[0].Url;
        LabelTop.Text = favorites[0].Title;
        await _cardService.MarkUsedAsync(favorites[0].Id);

        if (favorites.Count > 1)
        {
            WebViewBottom.Source = favorites[1].Url;
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
        if (_isTopFullScreen || _isBottomFullScreen)
        {
            ResetFullScreen();
            return;
        }
        OnExpandTop(sender, e);
    }

    private void OnExpandTop(object? sender, EventArgs e)
    {
        if (_isTopFullScreen)
        {
            ResetFullScreen();
            return;
        }
        BorderBottom.IsVisible = false;
        Grid.SetRowSpan(BorderTop, 2);
        _isTopFullScreen = true;
        _isBottomFullScreen = false;
        ToggleFullBtn.Text = "Restaurar";
    }

    private void OnExpandBottom(object? sender, EventArgs e)
    {
        if (_isBottomFullScreen)
        {
            ResetFullScreen();
            return;
        }
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
        ToggleFullBtn.Text = "Tela Cheia ↑";
    }
}
