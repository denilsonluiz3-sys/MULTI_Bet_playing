using System.Text;

namespace MULTI_Bet_playing_Demo.Services;

public static class AppLog
{
    private static readonly object Sync = new();
    private static readonly StringBuilder PendingBuffer = new(8192);
    private static string _filePath = string.Empty;
    private static bool _fileReady;
    private const int MaxPendingChars = 32768;

    public static string LogFilePath
    {
        get { lock (Sync) return _filePath; }
    }

    public static void Init()
    {
        try
        {
            lock (Sync)
            {
                if (_fileReady) return;

                var baseDir = FileSystem.AppDataDirectory;
                var logsDir = Path.Combine(baseDir, "logs");
                Directory.CreateDirectory(logsDir);

                _filePath = Path.Combine(logsDir, $"multibet_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                _fileReady = true;

                if (PendingBuffer.Length > 0)
                {
                    File.AppendAllText(_filePath, PendingBuffer.ToString());
                    PendingBuffer.Clear();
                }
            }

            Info($"AppLog iniciado. Arquivo: {_filePath}");
            Info($"App v{AppInfo.VersionString} ({AppInfo.BuildString}) · {DeviceInfo.Platform} {DeviceInfo.VersionString}");
        }
        catch
        {
        }
    }

    public static void WireGlobalExceptionHandlers()
    {
        try
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                    Exception("AppDomain.UnhandledException", ex);
                else
                    Error("AppDomain.UnhandledException: " + (args.ExceptionObject?.ToString() ?? "null"));
            };

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                Exception("TaskScheduler.UnobservedTaskException", args.Exception);
                args.SetObserved();
            };
        }
        catch
        {
        }
    }

    public static void Info(string message) => Write("INFO ", message);
    public static void Warning(string message) => Write("WARN ", message);
    public static void Error(string message) => Write("ERROR", message);
    public static void Fatal(string message) => Write("FATAL", message);

    public static void Exception(string where, Exception? ex)
    {
        if (ex == null)
        {
            Error($"{where}: (null exception)");
            return;
        }

        Write("EXCPT", $"{where}: {ex}");
        var inner = ex.InnerException;
        var depth = 0;
        while (inner != null && depth < 10)
        {
            Write("EXCPT", $"  inner[{depth}]: {inner}");
            inner = inner.InnerException;
            depth++;
        }
    }

    public static string ReadRecentLog(int maxLines = 400)
    {
        try
        {
            lock (Sync)
            {
                if (!_fileReady || string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
                    return PendingBuffer.ToString();
            }

            var lines = File.ReadAllLines(_filePath);
            if (lines.Length <= maxLines)
                return string.Join(Environment.NewLine, lines);

            var sb = new StringBuilder();
            sb.AppendLine($"... (truncado de {lines.Length} linhas, últimas {maxLines}) ...");
            for (int i = lines.Length - maxLines; i < lines.Length; i++)
                sb.AppendLine(lines[i]);
            return sb.ToString();
        }
        catch
        {
            return "(falha ao ler o log)";
        }
    }

    public static void ClearLogs()
    {
        try
        {
            lock (Sync)
            {
                PendingBuffer.Clear();
                if (_fileReady && !string.IsNullOrEmpty(_filePath) && File.Exists(_filePath))
                    File.WriteAllText(_filePath, "");
            }
            Info("Log limpo pelo usuário.");
        }
        catch
        {
        }
    }

    private static void Write(string level, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";

        lock (Sync)
        {
            if (_fileReady)
            {
                try { File.AppendAllText(_filePath, line + Environment.NewLine); }
                catch { }
            }
            else
            {
                PendingBuffer.AppendLine(line);
                if (PendingBuffer.Length > MaxPendingChars)
                    PendingBuffer.Remove(0, MaxPendingChars / 2);
            }
        }

        try { Console.WriteLine(line); }
        catch { }

#if ANDROID
        try
        {
            if (level is "ERROR" or "FATAL" or "EXCPT")
                Android.Util.Log.Error("MULTIBet", line);
            else if (level == "WARN ")
                Android.Util.Log.Warn("MULTIBet", line);
            else
                Android.Util.Log.Info("MULTIBet", line);
        }
        catch { }
#endif
    }
}
