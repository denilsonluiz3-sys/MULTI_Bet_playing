namespace MULTI_Bet_playing_Demo.Services;

public static class AppLog
{
    public static string LogFilePath => string.Empty;
    public static void Init() { }
    public static void WireGlobalExceptionHandlers() { }
    public static void Info(string message) { }
    public static void Warning(string message) { }
    public static void Error(string message) { }
    public static void Fatal(string message) { }
    public static void Exception(string where, Exception? ex) { }
    public static string ReadRecentLog(int maxLines = 400) => string.Empty;
    public static void ClearLogs() { }
}
