namespace MULTI_Bet_playing_Demo.Services.Logging;

/// <summary>
/// No-op implementation used when temporary diagnostics are disabled.
/// </summary>
public sealed class NullMultiBetLogger : IMultiBetLogger
{
    public void Debug(string message, string? source = null) { }
    public void Info(string message, string? source = null) { }
    public void Warning(string message, string? source = null) { }
    public void Error(string message, Exception? exception = null, string? source = null) { }
    public string GetLogFilePath() => string.Empty;
    public Task<string> ExportSessionAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(string.Empty);
}
