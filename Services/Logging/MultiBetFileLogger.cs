using System.Text;
using System.Text.Json;

namespace MULTI_Bet_playing_Demo.Services.Logging;

public sealed class MultiBetFileLogger : IMultiBetLogger
{
    private readonly object _sync = new();
    private readonly string _directory;
    private readonly string _logPath;
    private readonly string _sessionId;

    public MultiBetFileLogger()
    {
        _directory = Path.Combine(FileSystem.AppDataDirectory, "logs");
        Directory.CreateDirectory(_directory);

        _sessionId = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        _logPath = Path.Combine(_directory, $"multibet-{DateTime.UtcNow:yyyyMMdd}.log.jsonl");

        Info($"Log session started: {_sessionId}", "Logging");
    }

    public void Debug(string message, string? source = null) => Write("DEBUG", message, source);
    public void Info(string message, string? source = null) => Write("INFO", message, source);
    public void Warning(string message, string? source = null) => Write("WARN", message, source);

    public void Error(string message, Exception? exception = null, string? source = null)
    {
        Write("ERROR", message, source, exception);
    }

    public string GetLogFilePath() => _logPath;

    public async Task<string> ExportSessionAsync(CancellationToken cancellationToken = default)
    {
        var exportPath = Path.Combine(_directory, $"logs.debugmultbet-{DateTime.UtcNow:yyyyMMddHHmmss}.md");
        var builder = new StringBuilder();
        builder.AppendLine("# MultiBetRiskGuard diagnostic log");
        builder.AppendLine();
        builder.AppendLine($"Session: `{_sessionId}`");
        builder.AppendLine($"Generated UTC: `{DateTime.UtcNow:O}`");
        builder.AppendLine();
        builder.AppendLine("```jsonl");

        if (File.Exists(_logPath))
        {
            await using var stream = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();
                builder.AppendLine(await reader.ReadLineAsync(cancellationToken));
            }
        }

        builder.AppendLine("```");
        await File.WriteAllTextAsync(exportPath, builder.ToString(), Encoding.UTF8, cancellationToken);
        return exportPath;
    }

    private void Write(string level, string message, string? source, Exception? exception = null)
    {
        var entry = new
        {
            timestampUtc = DateTime.UtcNow,
            level,
            source,
            message,
            exception = exception == null ? null : new
            {
                type = exception.GetType().FullName,
                message = exception.Message,
                stackTrace = exception.StackTrace
            },
            sessionId = _sessionId
        };

        try
        {
            var line = JsonSerializer.Serialize(entry) + Environment.NewLine;
            lock (_sync)
            {
                RotateIfNeeded();
                File.AppendAllText(_logPath, line, Encoding.UTF8);
            }
        }
        catch
        {
            // Logging must never crash the application.
        }
    }

    private void RotateIfNeeded()
    {
        const long maxBytes = 5 * 1024 * 1024;
        if (!File.Exists(_logPath) || new FileInfo(_logPath).Length < maxBytes)
            return;

        var archive = Path.Combine(_directory, $"multibet-{DateTime.UtcNow:yyyyMMddHHmmss}.log.jsonl");
        File.Move(_logPath, archive, overwrite: true);
    }
}
