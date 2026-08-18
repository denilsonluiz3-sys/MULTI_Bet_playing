using Microsoft.Maui.Controls;

namespace MULTI_Bet_playing_Demo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    private void OnFilterAll(object? sender, EventArgs e)
    {
        // Keep the shell menu action valid; filtering can be wired to the active page later.
    }

    private void OnFilterFavorites(object? sender, EventArgs e)
    {
        // Keep the shell menu action valid; filtering can be wired to the active page later.
    }

    private void OnFilterRecents(object? sender, EventArgs e)
    {
        // Keep the shell menu action valid; filtering can be wired to the active page later.
    }
}
