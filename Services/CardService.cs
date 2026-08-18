using System.Text.Json;
using MULTI_Bet_playing_Demo.Models;

namespace MULTI_Bet_playing_Demo.Services;

public class CardService
{
    private readonly string _filePath;
    private List<CardItem> _cards = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public CardService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MULTI_Bet",
            "cards.json"))
    {
    }

    public CardService(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("O caminho do arquivo é obrigatório.", nameof(filePath));

        _filePath = filePath;
    }

    public async Task<List<CardItem>> GetCardsAsync()
    {
        await LoadAsync();
        return _cards.OrderByDescending(c => c.LastUsed).ToList();
    }

    public async Task<List<CardItem>> GetFavoritesAsync()
    {
        await LoadAsync();
        return _cards.Where(c => c.IsFavorite).OrderByDescending(c => c.LastUsed).ToList();
    }

    public async Task<List<CardItem>> GetRecentsAsync(int count = 10)
    {
        await LoadAsync();
        return _cards.OrderByDescending(c => c.LastUsed).Take(count).ToList();
    }

    public async Task AddCardAsync(CardItem card)
    {
        ArgumentNullException.ThrowIfNull(card);
        await LoadAsync();
        _cards.Add(card);
        await SaveAsync();
    }

    public async Task UpdateCardAsync(CardItem card)
    {
        ArgumentNullException.ThrowIfNull(card);
        await LoadAsync();
        var index = _cards.FindIndex(c => c.Id == card.Id);
        if (index >= 0)
        {
            _cards[index] = card;
            await SaveAsync();
        }
    }

    public async Task RemoveCardAsync(string id)
    {
        await LoadAsync();
        _cards.RemoveAll(c => c.Id == id);
        await SaveAsync();
    }

    public async Task ToggleFavoriteAsync(string id)
    {
        await LoadAsync();
        var card = _cards.FirstOrDefault(c => c.Id == id);
        if (card != null)
        {
            card.IsFavorite = !card.IsFavorite;
            await SaveAsync();
        }
    }

    public async Task MarkUsedAsync(string id)
    {
        await LoadAsync();
        var card = _cards.FirstOrDefault(c => c.Id == id);
        if (card != null)
        {
            card.LastUsed = DateTime.UtcNow;
            await SaveAsync();
        }
    }

    private async Task LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            _cards = new List<CardItem>();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            _cards = JsonSerializer.Deserialize<List<CardItem>>(json) ?? new List<CardItem>();
        }
        catch
        {
            _cards = new List<CardItem>();
        }
    }

    private async Task SaveAsync()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(_cards, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
