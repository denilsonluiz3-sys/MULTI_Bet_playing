namespace MULTI_Bet_playing_Demo.Models;

public class CardItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = "🎰";
    public string Url { get; set; } = string.Empty;
    public string Category { get; set; } = "Outros";
    public bool IsFavorite { get; set; }
    public DateTime LastUsed { get; set; } = DateTime.UtcNow;
}
