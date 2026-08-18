using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        ThemeManager.ApplySavedTheme();
    }

    private async void OnFilterAll(object? sender, EventArgs e)
    {
        FilterState.Current = FilterState.All;
        await GoHomeAsync();
    }

    private async void OnFilterFavorites(object? sender, EventArgs e)
    {
        FilterState.Current = FilterState.Favorites;
        await GoHomeAsync();
    }

    private async void OnFilterRecents(object? sender, EventArgs e)
    {
        FilterState.Current = FilterState.Recents;
        await GoHomeAsync();
    }

    private async Task GoHomeAsync()
    {
        try
        {
            FlyoutIsPresented = false;
            await GoToAsync("//tabs/home");
        }
        catch
        {
            try { await GoToAsync("//home"); } catch { }
        }
    }
}
