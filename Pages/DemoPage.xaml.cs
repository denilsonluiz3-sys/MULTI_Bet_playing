using MULTI_Bet_playing_Demo.Models;
using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class DemoPage : ContentPage
{
    private readonly CardService _cardService;
    private readonly WebView[] _webViews = null!;
    private readonly Label[] _labels = null!;

    private static readonly (string Title, string Url)[] PragmaticExamples =
    {
        ("Joker's Jewels", "https://www.pragmaticplay.com/pt/jogos/jokers-jewels/?gamelang=pt&cur=ALL"),
        ("Big Bass Splash 1000", "https://www.pragmaticplay.com/pt/jogos/big-bass-splash-1000/?gamelang=pt&cur=EUR"),
        ("Sweet Bonanza", "https://www.pragmaticplay.com/pt/jogos/sweet-bonanza/?gamelang=pt&cur=ALL"),
        ("Gates of Olympus", "https://www.pragmaticplay.com/pt/jogos/gates-of-olympus/?gamelang=pt&cur=ALL"),
    };

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
        AppLog.Info("DemoPage.OnAppearing");
    }

    private async void OnLoadFavorites(object? sender, EventArgs e)
    {
        var favorites = await _cardService.GetFavoritesAsync();
        AppLog.Info($"Demo: favoritos={favorites.Count}");
        if (favorites.Count == 0)
        {
            await DisplayAlertAsync("Aviso", "Nenhum favorito. Na Início toque ⭐ no card.", "OK");
            return;
        }

        for (int i = 0; i < _webViews.Length; i++)
        {
            if (i < favorites.Count)
                await LoadSlotAsync(i, favorites[i].Title, favorites[i].Url, favorites[i].Id);
            else
                ClearSlot(i);
        }
    }

    private async void OnLoadPragmaticExamples(object? sender, EventArgs e)
    {
        AppLog.Info("Demo: exemplos Pragmatic Play");
        for (int i = 0; i < _webViews.Length && i < PragmaticExamples.Length; i++)
        {
            var (title, url) = PragmaticExamples[i];
            await LoadSlotAsync(i, title, url, null);
        }

        var existing = await _cardService.GetCardsAsync();
        foreach (var (title, url) in PragmaticExamples)
        {
            if (existing.Any(c => string.Equals(c.Url, url, StringComparison.OrdinalIgnoreCase)))
                continue;
            if (!UrlValidator.TryNormalize(url, out var safe, out _))
                continue;
            await _cardService.AddCardAsync(new CardItem
            {
                Title = title,
                Url = safe,
                Icon = "🎮",
                IsFavorite = false,
                LastUsed = DateTime.UtcNow
            });
        }

        await DisplayAlertAsync("Exemplos",
            "4 páginas Pragmatic no grid. Também em Início para favoritar.",
            "OK");
    }

    private async void OnPickSlots(object? sender, EventArgs e)
    {
        var cards = await _cardService.GetCardsAsync();
        if (cards.Count == 0)
        {
            await DisplayAlertAsync("Vazio", "Adicione links ou use Exemplos Pragmatic.", "OK");
            return;
        }

        for (int slot = 0; slot < 4; slot++)
        {
            var names = cards.Select(c => c.Title).ToList();
            names.Insert(0, "(vazio)");
            var choice = await DisplayActionSheetAsync($"Tela {slot + 1}", "Pular", null, names.ToArray());
            if (string.IsNullOrEmpty(choice) || choice == "Pular") continue;
            if (choice == "(vazio)") { ClearSlot(slot); continue; }
            var card = cards.FirstOrDefault(c => c.Title == choice);
            if (card != null)
                await LoadSlotAsync(slot, card.Title, card.Url, card.Id);
        }
    }

    private async Task LoadSlotAsync(int index, string title, string rawUrl, string? cardId)
    {
        if (!UrlValidator.TryNormalize(rawUrl, out var url, out var err))
        {
            _labels[index].Text = $"Tela {index + 1} (URL inválida)";
            _webViews[index].Source = null;
            AppLog.Warning($"Demo slot {index}: {err}");
            return;
        }
        _webViews[index].Source = url;
        _labels[index].Text = title;
        if (!string.IsNullOrEmpty(cardId))
            await _cardService.MarkUsedAsync(cardId);
    }

    private void ClearSlot(int i)
    {
        _webViews[i].Source = null;
        _labels[i].Text = $"Tela {i + 1}";
    }

    private void OnClearAll(object? sender, EventArgs e)
    {
        for (int i = 0; i < _webViews.Length; i++)
            ClearSlot(i);
        AppLog.Info("Demo: limpar todas");
    }
}
