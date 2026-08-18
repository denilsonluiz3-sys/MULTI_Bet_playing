using MULTI_Bet_playing_Demo.Models;
using MULTI_Bet_playing_Demo.Services;
using Xunit;

namespace MULTI_Bet_playing_Demo.Tests;

public sealed class CardServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "multibet-tests", Guid.NewGuid().ToString("N"));
    private readonly string _filePath;

    public CardServiceTests()
    {
        Directory.CreateDirectory(_directory);
        _filePath = Path.Combine(_directory, "cards.json");
    }

    [Fact]
    public void RejectsBlankStoragePath()
    {
        Assert.Throws<ArgumentException>(() => new CardService(" "));
    }

    [Fact]
    public async Task RejectsNullCards()
    {
        var service = new CardService(_filePath);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddCardAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateCardAsync(null!));
    }

    [Fact]
    public async Task StartsEmptyWhenStorageDoesNotExist()
    {
        var service = new CardService(_filePath);

        var cards = await service.GetCardsAsync();

        Assert.Empty(cards);
    }

    [Fact]
    public async Task AddAndReloadPersistsCard()
    {
        var card = new CardItem { Id = "1", Title = "Site A", Url = "https://example.com" };
        await new CardService(_filePath).AddCardAsync(card);

        var cards = await new CardService(_filePath).GetCardsAsync();

        var saved = Assert.Single(cards);
        Assert.Equal("1", saved.Id);
        Assert.Equal("Site A", saved.Title);
        Assert.Equal("https://example.com", saved.Url);
    }

    [Fact]
    public async Task FavoritesAndToggleFavoriteWork()
    {
        var service = new CardService(_filePath);
        await service.AddCardAsync(new CardItem { Id = "1", Title = "A", IsFavorite = false });
        await service.AddCardAsync(new CardItem { Id = "2", Title = "B", IsFavorite = true });

        var favorites = await service.GetFavoritesAsync();
        Assert.Equal("2", Assert.Single(favorites).Id);

        await service.ToggleFavoriteAsync("1");
        favorites = await service.GetFavoritesAsync();
        Assert.Equal(2, favorites.Count);

        await service.ToggleFavoriteAsync("2");
        favorites = await service.GetFavoritesAsync();
        Assert.Equal("1", Assert.Single(favorites).Id);
    }

    [Fact]
    public async Task UpdateAndRemoveWork()
    {
        var service = new CardService(_filePath);
        await service.AddCardAsync(new CardItem { Id = "1", Title = "Old" });

        await service.UpdateCardAsync(new CardItem { Id = "1", Title = "New" });
        var updated = Assert.Single(await service.GetCardsAsync());
        Assert.Equal("New", updated.Title);

        await service.RemoveCardAsync("1");
        Assert.Empty(await service.GetCardsAsync());
    }

    [Fact]
    public async Task MissingCardOperationsDoNotCreateCards()
    {
        var service = new CardService(_filePath);

        await service.UpdateCardAsync(new CardItem { Id = "missing" });
        await service.ToggleFavoriteAsync("missing");
        await service.MarkUsedAsync("missing");
        await service.RemoveCardAsync("missing");

        Assert.Empty(await service.GetCardsAsync());
    }

    [Fact]
    public async Task RecentsAreLimitedAndOrderedByLastUsed()
    {
        var service = new CardService(_filePath);
        var old = new CardItem { Id = "old", LastUsed = DateTime.UtcNow.AddMinutes(-10) };
        var recent = new CardItem { Id = "recent", LastUsed = DateTime.UtcNow };
        await service.AddCardAsync(old);
        await service.AddCardAsync(recent);

        var cards = await service.GetRecentsAsync(1);

        Assert.Equal("recent", Assert.Single(cards).Id);
    }

    [Fact]
    public async Task MarkUsedUpdatesTimestamp()
    {
        var service = new CardService(_filePath);
        var before = DateTime.UtcNow.AddSeconds(-1);
        await service.AddCardAsync(new CardItem { Id = "1", LastUsed = before });

        await service.MarkUsedAsync("1");

        var card = Assert.Single(await service.GetCardsAsync());
        Assert.True(card.LastUsed > before);
    }

    [Fact]
    public async Task CorruptStorageFallsBackToEmptyCollection()
    {
        await File.WriteAllTextAsync(_filePath, "not-json");

        var cards = await new CardService(_filePath).GetCardsAsync();

        Assert.Empty(cards);
    }

    public void Dispose()
    {
        try { Directory.Delete(_directory, recursive: true); } catch { }
    }
}
