namespace MULTI_Bet_playing_Demo.Services;

public static class ThemeManager
{
    public const string Dark = "Dark";
    public const string Light = "Light";
    public const string Casino = "Casino";

    private static ResourceDictionary? _currentThemeDict;

    public static string CurrentTheme => Preferences.Get("AppTheme", Dark);

    public static void ApplyTheme(string theme)
    {
        var resources = Application.Current?.Resources;
        if (resources == null) return;

        if (_currentThemeDict != null && resources.MergedDictionaries.Contains(_currentThemeDict))
            resources.MergedDictionaries.Remove(_currentThemeDict);

        _currentThemeDict = theme switch
        {
            Light => new Resources.Themes.LightTheme(),
            Casino => new Resources.Themes.CasinoTheme(),
            _ => new Resources.Themes.DarkTheme()
        };

        resources.MergedDictionaries.Add(_currentThemeDict);
        Preferences.Set("AppTheme", theme);
    }

    public static void ApplySavedTheme() => ApplyTheme(CurrentTheme);
}
