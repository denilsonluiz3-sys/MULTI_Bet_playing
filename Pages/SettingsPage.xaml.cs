using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        LoadCurrentTheme();
    }

    private void LoadCurrentTheme()
    {
        var theme = ThemeManager.CurrentTheme;
        DarkRadio.IsChecked = theme == ThemeManager.Dark;
        LightRadio.IsChecked = theme == ThemeManager.Light;
        CasinoRadio.IsChecked = theme == ThemeManager.Casino;
    }

    private void OnThemeChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value || sender is not RadioButton radio) return;

        string theme = radio.Content?.ToString() switch
        {
            var s when s?.Contains("Light") == true => ThemeManager.Light,
            var s when s?.Contains("Casino") == true => ThemeManager.Casino,
            _ => ThemeManager.Dark
        };

        ThemeManager.ApplyTheme(theme);
    }
}
