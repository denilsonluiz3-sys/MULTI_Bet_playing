using MULTI_Bet_playing_Demo.Models;
using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class HomePage : ContentPage
{
    private readonly CardService _cardService;

    public HomePage() : this(ResolveCardService()) { }

    public HomePage(CardService cardService)
    {
        InitializeComponent();
        _cardService = cardService;
    }

    private static CardService ResolveCardService() =>
        Application.Current?.Handler?.MauiContext?.Services?.GetService<CardService>()
        ?? new CardService();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        FilterState.Changed -= OnFilterChanged;
        FilterState.Changed += OnFilterChanged;
        _ = LoadCardsAsync();
    }

    protected override void OnDisappearing()
    {
        FilterState.Changed -= OnFilterChanged;
        base.OnDisappearing();
    }

    private void OnFilterChanged(object? sender, EventArgs e) => _ = LoadCardsAsync();

    private async Task LoadCardsAsync()
    {
        var filter = FilterState.Current;
        FilterLabel.Text = $"Filtro: {filter}";

        List<CardItem> cards = filter switch
        {
            FilterState.Favorites => await _cardService.GetFavoritesAsync(),
            FilterState.Recents => await _cardService.GetRecentsAsync(),
            _ => await _cardService.GetCardsAsync()
        };

        var all = await _cardService.GetCardsAsync();
        var favCount = all.Count(c => c.IsFavorite);
        AppLog.Info($"HomePage.Load filter={filter} lista={cards.Count} total={all.Count} fav={favCount}");

        CardsCollection.ItemsSource = null;
        CardsCollection.ItemsSource = cards;
    }

    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        CardItem? card = e.Parameter as CardItem;
        if (card == null && sender is Element el)
            card = el.BindingContext as CardItem;
        if (card == null) return;
        await OpenCardAsync(card);
    }

    private async void OnFavoriteClicked(object? sender, EventArgs e)
    {
        CardItem? card = null;
        if (sender is BindableObject bo)
            card = bo.BindingContext as CardItem;
        if (card == null && sender is Button btn)
            card = btn.CommandParameter as CardItem;

        if (card == null)
        {
            AppLog.Warning("OnFavoriteClicked: card null (BindingContext/CommandParameter)");
            await DisplayAlertAsync("Favorito", "Não foi possível identificar o card. Tente de novo.", "OK");
            return;
        }

        AppLog.Info($"OnFavoriteClicked: {card.Title} id={card.Id} antes={card.IsFavorite}");
        await _cardService.ToggleFavoriteAsync(card.Id);

        var updated = (await _cardService.GetCardsAsync()).FirstOrDefault(c => c.Id == card.Id);
        var nowFav = updated?.IsFavorite ?? !card.IsFavorite;
        await DisplayAlertAsync(
            nowFav ? "Favorito" : "Removido",
            nowFav ? $"“{card.Title}” marcado como favorito." : $"“{card.Title}” removido dos favoritos.",
            "OK");

        await LoadCardsAsync();
    }

    private async void OnCardSwiped(object? sender, SwipedEventArgs e)
    {
        var card = e.Parameter as CardItem
            ?? (sender as BindableObject)?.BindingContext as CardItem;
        if (card == null) return;

        var action = await DisplayActionSheetAsync(
            card.Title, "Cancelar", null,
            card.IsFavorite ? "Remover dos Favoritos" : "Adicionar aos Favoritos",
            "Abrir", "Remover link");

        switch (action)
        {
            case "Adicionar aos Favoritos":
            case "Remover dos Favoritos":
                await _cardService.ToggleFavoriteAsync(card.Id);
                await LoadCardsAsync();
                break;
            case "Abrir":
                await OpenCardAsync(card);
                break;
            case "Remover link":
                if (await DisplayAlertAsync("Remover", $"Apagar “{card.Title}”?", "Sim", "Não"))
                {
                    await _cardService.RemoveCardAsync(card.Id);
                    await LoadCardsAsync();
                }
                break;
        }
    }

    private async Task OpenCardAsync(CardItem card)
    {
        if (!UrlValidator.TryNormalize(card.Url, out var url, out var err))
        {
            AppLog.Warning($"URL inválida: {err}");
            await DisplayAlertAsync("URL inválida", err, "OK");
            return;
        }

        await _cardService.MarkUsedAsync(card.Id);
        AppLog.Info($"Abrir WebView: {card.Title}");
        await Navigation.PushAsync(new WebViewPage(url, card.Title));
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        string? title = await DisplayPromptAsync("Novo link", "Nome:", "Salvar", "Cancelar");
        if (string.IsNullOrWhiteSpace(title)) return;

        string? urlInput = await DisplayPromptAsync("Novo link", "URL (https://…):", "Salvar", "Cancelar", "https://");
        if (string.IsNullOrWhiteSpace(urlInput)) return;

        if (!UrlValidator.TryNormalize(urlInput, out var url, out var error))
        {
            await DisplayAlertAsync("URL rejeitada", error, "OK");
            return;
        }

        if (!UrlValidator.IsHttpsPreferred(url))
        {
            if (!await DisplayAlertAsync("HTTP", "URL sem HTTPS. Continuar?", "Sim", "Não"))
                return;
        }

        await _cardService.AddCardAsync(new CardItem
        {
            Title = title.Trim(),
            Url = url,
            Icon = "🔗",
            IsFavorite = false,
            LastUsed = DateTime.UtcNow
        });
        await LoadCardsAsync();
    }
}
