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

        CardsCollection.ItemsSource = null;
        CardsCollection.ItemsSource = cards;
    }

    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        var card = e.Parameter as CardItem ?? (sender as Element)?.BindingContext as CardItem;
        if (card is null) return;
        await OpenCardAsync(card);
    }

    private async void OnFavoriteClicked(object? sender, EventArgs e)
    {
        var card = (sender as BindableObject)?.BindingContext as CardItem;
        if (card is null) return;

        await _cardService.ToggleFavoriteAsync(card.Id);
        await LoadCardsAsync();
    }

    private async void OnCardSwiped(object? sender, SwipedEventArgs e)
    {
        var card = e.Parameter as CardItem ?? (sender as BindableObject)?.BindingContext as CardItem;
        if (card is null) return;

        var action = await DisplayActionSheetAsync(
            card.Title, "Cancelar", null,
            card.IsFavorite ? "Remover dos Favoritos" : "Adicionar aos Favoritos",
            "Mudar categoria", "Abrir", "Remover link");

        switch (action)
        {
            case "Adicionar aos Favoritos":
            case "Remover dos Favoritos":
                await _cardService.ToggleFavoriteAsync(card.Id);
                await LoadCardsAsync();
                break;
            case "Mudar categoria":
                await ChangeCategoryAsync(card);
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

    private async Task ChangeCategoryAsync(CardItem card)
    {
        var categories = new[] { "Esportes", "Cassino", "Slots", "Favoritos", "Verificados", "Outros" };
        var choice = await DisplayActionSheetAsync("Categoria", "Cancelar", null, categories);
        if (string.IsNullOrWhiteSpace(choice) || choice == "Cancelar") return;

        card.Category = choice;
        await _cardService.UpdateCardAsync(card);
        await LoadCardsAsync();
    }

    private async Task OpenCardAsync(CardItem card)
    {
        if (!UrlValidator.TryNormalize(card.Url, out var url, out var err))
        {
            await DisplayAlertAsync("URL inválida", err, "OK");
            return;
        }

        await _cardService.MarkUsedAsync(card.Id);
        await Navigation.PushAsync(new WebViewPage(url, card.Title));
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        var title = await DisplayPromptAsync("Novo link", "Nome:", "Salvar", "Cancelar");
        if (string.IsNullOrWhiteSpace(title)) return;

        var urlInput = await DisplayPromptAsync("Novo link", "URL (https://…):", "Salvar", "Cancelar", "https://");
        if (string.IsNullOrWhiteSpace(urlInput)) return;

        if (!UrlValidator.TryNormalize(urlInput, out var url, out var error))
        {
            await DisplayAlertAsync("URL rejeitada", error, "OK");
            return;
        }

        if (!UrlValidator.IsHttpsPreferred(url) &&
            !await DisplayAlertAsync("HTTP", "URL sem HTTPS. Continuar?", "Sim", "Não"))
            return;

        var category = await DisplayActionSheetAsync(
            "Categoria", "Cancelar", null,
            "Esportes", "Cassino", "Slots", "Outros");
        if (string.IsNullOrWhiteSpace(category) || category == "Cancelar")
            category = "Outros";

        await _cardService.AddCardAsync(new CardItem
        {
            Title = title.Trim(),
            Url = url,
            Icon = "🔗",
            Category = category,
            IsFavorite = false,
            LastUsed = DateTime.UtcNow
        });

        await LoadCardsAsync();
    }
}
