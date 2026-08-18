using MULTI_Bet_playing_Demo.Models;
using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class HomePage : ContentPage
{
    private readonly CardService _cardService;
    private string _currentFilter = "Todos";

    public HomePage() : this(ResolveCardService()) { }

    public HomePage(CardService cardService)
    {
        InitializeComponent();
        _cardService = cardService;
        FilterPicker.SelectedIndex = 0;
    }

    private static CardService ResolveCardService() =>
        Application.Current?.Handler?.MauiContext?.Services?.GetService<CardService>()
        ?? new CardService();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCardsAsync();
    }

    private async Task LoadCardsAsync()
    {
        List<CardItem> cards = _currentFilter switch
        {
            "Favoritos" => await _cardService.GetFavoritesAsync(),
            "Recentes" => await _cardService.GetRecentsAsync(),
            _ => await _cardService.GetCardsAsync()
        };
        CardsCollection.ItemsSource = cards;
    }

    private async void OnFilterChanged(object? sender, EventArgs e)
    {
        if (FilterPicker.SelectedItem is string filter)
        {
            _currentFilter = filter;
            await LoadCardsAsync();
        }
    }

    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not CardItem card) return;
        await OpenCardAsync(card);
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

    private async void OnCardSwiped(object? sender, SwipedEventArgs e)
    {
        if (e.Parameter is not CardItem card) return;
        await ShowCardMenuAsync(card);
    }

    private async Task ShowCardMenuAsync(CardItem card)
    {
        var action = await DisplayActionSheetAsync(
            card.Title,
            "Cancelar",
            null,
            "Abrir",
            card.IsFavorite ? "Remover dos Favoritos" : "Adicionar aos Favoritos",
            "Editar",
            "Remover");

        switch (action)
        {
            case "Abrir":
                await OpenCardAsync(card);
                break;
            case "Adicionar aos Favoritos":
            case "Remover dos Favoritos":
                await _cardService.ToggleFavoriteAsync(card.Id);
                await LoadCardsAsync();
                break;
            case "Remover":
                if (await DisplayAlertAsync("Confirmar", $"Remover {card.Title}?", "Sim", "Não"))
                {
                    await _cardService.RemoveCardAsync(card.Id);
                    await LoadCardsAsync();
                }
                break;
            case "Editar":
                await EditCardAsync(card);
                break;
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        string? title = await DisplayPromptAsync("Novo link", "Nome (ex.: Site A):", "Salvar", "Cancelar");
        if (string.IsNullOrWhiteSpace(title)) return;

        string? urlInput = await DisplayPromptAsync(
            "Novo link",
            "URL (https://…):",
            "Salvar",
            "Cancelar",
            "https://");

        if (string.IsNullOrWhiteSpace(urlInput)) return;

        if (!UrlValidator.TryNormalize(urlInput, out var url, out var error))
        {
            await DisplayAlertAsync("URL rejeitada", error, "OK");
            return;
        }

        if (!UrlValidator.IsHttpsPreferred(url))
        {
            var okHttp = await DisplayAlertAsync(
                "HTTP (não seguro)",
                "A URL não usa HTTPS. Continuar mesmo assim?",
                "Sim",
                "Não");
            if (!okHttp) return;
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

    private async Task EditCardAsync(CardItem card)
    {
        string? title = await DisplayPromptAsync("Editar", "Nome:", "Salvar", "Cancelar", initialValue: card.Title);
        if (string.IsNullOrWhiteSpace(title)) return;

        string? urlInput = await DisplayPromptAsync("Editar", "URL:", "Salvar", "Cancelar", initialValue: card.Url);
        if (string.IsNullOrWhiteSpace(urlInput)) return;

        if (!UrlValidator.TryNormalize(urlInput, out var url, out var error))
        {
            await DisplayAlertAsync("URL rejeitada", error, "OK");
            return;
        }

        card.Title = title.Trim();
        card.Url = url;
        await _cardService.UpdateCardAsync(card);
        await LoadCardsAsync();
    }
}
