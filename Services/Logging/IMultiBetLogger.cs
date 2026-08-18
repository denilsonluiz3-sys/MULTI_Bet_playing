namespace MULTI_Bet_playing_Demo.Services.Logging;

/// <summary>
/// Temporary diagnostic logger. Keep application code dependent only on this interface
/// so the entire diagnostic layer can be removed after stabilization.
/// </summary>
public interface IMultiBetLogger
{
    void Debug(string message, string? source = null);
    void Info(string message, string? source = null);
    void Warning(string message, string? source = null);
    void Error(string message, Exception? exception = null, string? source = null);
    string GetLogFilePath();
    Task<string> ExportSessionAsync(CancellationToken cancellationToken = default);
}
