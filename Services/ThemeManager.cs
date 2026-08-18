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
        var app = Application.Current;
        var resources = app?.Resources;
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

        if (app != null)
            app.UserAppTheme = theme == Light ? AppTheme.Light : AppTheme.Dark;

        ApplyShellChrome(theme);
    }

    public static void ApplySavedTheme() => ApplyTheme(CurrentTheme);

    private static void ApplyShellChrome(string theme)
    {
        try
        {
            var shell = Shell.Current;
            if (shell == null) return;

            Color bg, surface, text, muted, primary;

            if (theme == Light)
            {
                bg = Color.FromArgb("#F3F4F6");
                surface = Colors.White;
                text = Color.FromArgb("#111827");
                muted = Color.FromArgb("#374151");
                primary = Color.FromArgb("#5B21B6");
            }
            else if (theme == Casino)
            {
                bg = Color.FromArgb("#1A0A2E");
                surface = Color.FromArgb("#2D1B4E");
                text = Color.FromArgb("#FFD700");
                muted = Color.FromArgb("#E8D5A3");
                primary = Color.FromArgb("#9B59B6");
            }
            else
            {
                bg = Color.FromArgb("#121212");
                surface = Color.FromArgb("#1E1E1E");
                text = Colors.White;
                muted = Color.FromArgb("#B0B0B0");
                primary = Color.FromArgb("#BB86FC");
            }

            shell.FlyoutBackgroundColor = surface;
            shell.BackgroundColor = bg;

            Shell.SetForegroundColor(shell, text);
            Shell.SetTitleColor(shell, text);
            Shell.SetUnselectedColor(shell, muted);
            Shell.SetTabBarBackgroundColor(shell, surface);
            Shell.SetTabBarForegroundColor(shell, primary);
            Shell.SetTabBarUnselectedColor(shell, muted);
            Shell.SetTabBarTitleColor(shell, primary);
            Shell.SetDisabledColor(shell, muted);
        }
        catch { }
    }
}
