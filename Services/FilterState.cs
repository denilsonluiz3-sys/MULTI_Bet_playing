namespace MULTI_Bet_playing_Demo.Services;

public static class FilterState
{
    private const string Key = "HomeFilter";

    public const string All = "Todos";
    public const string Favorites = "Favoritos";
    public const string Recents = "Recentes";

    public static event EventHandler? Changed;

    public static string Current
    {
        get => Preferences.Get(Key, All);
        set
        {
            var v = value is Favorites or Recents ? value : All;
            Preferences.Set(Key, v);
            AppLog.Info($"FilterState → {v}");
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }
}
