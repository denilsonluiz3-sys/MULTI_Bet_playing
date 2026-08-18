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

    private static CardService ResolveCardService()
    {
        return Application.Current?.Handler?.MauiContext?.Services?.GetService<CardService>()
               ?? new CardService();
    }

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
        await _cardService.MarkUsedAsync(card.Id);
        await Navigation.PushAsync(new WebViewPage(card.Url, card.Title));
    }

    private async void OnCardSwiped(object? sender, SwipedEventArgs e)
    {
        if (e.Parameter is not CardItem card) return;

        var action = await DisplayActionSheet(card.Title, "Cancelar", null,
            card.IsFavorite ? "Remover dos Favoritos" : "Adicionar aos Favoritos",
            "Remover",
            "Editar");

        switch (action)
        {
            case "Adicionar aos Favoritos":
            case "Remover dos Favoritos":
                await _cardService.ToggleFavoriteAsync(card.Id);
                await LoadCardsAsync();
                break;
            case "Remover":
                if (await DisplayAlert("Confirmar", $"Remover {card.Title}?", "Sim", "Não"))
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
        string? title = await DisplayPromptAsync("Novo Cassino", "Nome:", "Salvar", "Cancelar", "Ex: Bet365");
        if (string.IsNullOrWhiteSpace(title)) return;

        string? url = await DisplayPromptAsync("Novo Cassino", "URL:", "Salvar", "Cancelar", "https://");
        if (string.IsNullOrWhiteSpace(url)) return;

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            url = "https://" + url;

        await _cardService.AddCardAsync(new CardItem
        {
            Title = title.Trim(),
            Url = url.Trim(),
            Icon = "🎰",
            IsFavorite = false,
            LastUsed = DateTime.UtcNow
        });
        await LoadCardsAsync();
    }

    private async Task EditCardAsync(CardItem card)
    {
        string? title = await DisplayPromptAsync("Editar", "Nome:", "Salvar", "Cancelar", initialValue: card.Title);
        if (string.IsNullOrWhiteSpace(title)) return;

        string? url = await DisplayPromptAsync("Editar", "URL:", "Salvar", "Cancelar", initialValue: card.Url);
        if (string.IsNullOrWhiteSpace(url)) return;

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            url = "https://" + url;

        card.Title = title.Trim();
        card.Url = url.Trim();
        await _cardService.UpdateCardAsync(card);
        await LoadCardsAsync();
    }
}
