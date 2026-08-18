using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class DemoPage : ContentPage
{
    private readonly CardService _cardService;
    private readonly WebView[] _webViews;
    private readonly Label[] _labels;

    public DemoPage() : this(ResolveCardService()) { }

    public DemoPage(CardService cardService)
    {
        InitializeComponent();
        _cardService = cardService;
        _webViews = new[] { WebView1, WebView2, WebView3, WebView4 };
        _labels = new[] { Label1, Label2, Label3, Label4 };
    }

    private static CardService ResolveCardService() =>
        Application.Current?.Handler?.MauiContext?.Services?.GetService<CardService>()
        ?? new CardService();

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        foreach (var wv in _webViews)
            wv.IsVisible = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        foreach (var wv in _webViews)
            wv.IsVisible = true;
    }

    private async void OnLoadFavorites(object? sender, EventArgs e)
    {
        var favorites = await _cardService.GetFavoritesAsync();
        if (favorites.Count == 0)
        {
            await DisplayAlert("Aviso", "Nenhum favorito. Adicione links na aba Inicio e marque como favorito.", "OK");
            return;
        }

        for (int i = 0; i < _webViews.Length; i++)
        {
            if (i < favorites.Count)
            {
                var card = favorites[i];
                if (!UrlValidator.TryNormalize(card.Url, out var url, out _))
                {
                    _webViews[i].Source = null;
                    _labels[i].Text = $"Tela {i + 1} (URL invalida)";
                    continue;
                }
                _webViews[i].Source = url;
                _labels[i].Text = card.Title;
                await _cardService.MarkUsedAsync(card.Id);
            }
            else
            {
                _webViews[i].Source = null;
                _labels[i].Text = $"Tela {i + 1}";
            }
        }
    }

    private void OnClearAll(object? sender, EventArgs e)
    {
        for (int i = 0; i < _webViews.Length; i++)
        {
            _webViews[i].Source = null;
            _labels[i].Text = $"Tela {i + 1}";
        }
    }
}
