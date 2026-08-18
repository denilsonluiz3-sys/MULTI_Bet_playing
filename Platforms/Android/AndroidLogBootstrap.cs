using Android.Content;
using Android.Provider;
using Android.Runtime;
using MULTI_Bet_playing_Demo.Services;
using System.Text;

namespace MULTI_Bet_playing_Demo;

public static class AndroidLogBootstrap
{
    private static Java.Lang.Thread.IUncaughtExceptionHandler? _previous;
    private static StreamWriter? _downloadWriter;
    private static bool _wired;

    public static void Init(Context context)
    {
        try
        {
            AppLog.Init();
            TryCreateDownloadMirror(context);
            WireJvmHandler();
            AppLog.Info("AndroidLogBootstrap.Init OK");
        }
        catch (Exception ex)
        {
            try { AppLog.Exception("AndroidLogBootstrap.Init", ex); } catch { }
        }
    }

    private static void TryCreateDownloadMirror(Context context)
    {
        try
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(29))
                return;

            var fileName = $"multibet_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            var values = new ContentValues();
            values.Put(MediaStore.Downloads.InterfaceConsts.DisplayName, fileName);
            values.Put(MediaStore.Downloads.InterfaceConsts.MimeType, "text/plain");
            values.Put(MediaStore.Downloads.InterfaceConsts.RelativePath, "Download/MULTIBet");

            var uri = context.ContentResolver?.Insert(MediaStore.Downloads.ExternalContentUri, values);
            if (uri == null) return;

            var stream = context.ContentResolver?.OpenOutputStream(uri, "wa");
            if (stream == null) return;

            _downloadWriter = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            AppLog.Info($"Espelho do log em Download/MULTIBet/{fileName}");
        }
        catch
        {
        }
    }

    private static void WireJvmHandler()
    {
        if (_wired) return;
        _wired = true;
        try
        {
            _previous = Java.Lang.Thread.DefaultUncaughtExceptionHandler;
            Java.Lang.Thread.DefaultUncaughtExceptionHandler =
                new MultiBetUncaughtHandler(_previous);
        }
        catch
        {
        }
    }

    private sealed class MultiBetUncaughtHandler : Java.Lang.Object, Java.Lang.Thread.IUncaughtExceptionHandler
    {
        private readonly Java.Lang.Thread.IUncaughtExceptionHandler? _next;

        public MultiBetUncaughtHandler(Java.Lang.Thread.IUncaughtExceptionHandler? next) => _next = next;

        public void UncaughtException(Java.Lang.Thread? thread, Java.Lang.Throwable? throwable)
        {
            AppLog.Error($"JVM UncaughtException [{thread?.Name}]: {throwable}");
            if (_next != null && !ReferenceEquals(_next, this))
            {
                try { _next.UncaughtException(thread, throwable); }
                catch { }
            }
        }
    }
}
