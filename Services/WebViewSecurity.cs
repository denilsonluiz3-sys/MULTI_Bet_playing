namespace MULTI_Bet_playing_Demo.Services;

public static class WebViewSecurity
{
    public static void ConfigureHandlers()
    {
#if ANDROID
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("MultiBetSecure", (handler, view) =>
        {
            var wv = handler.PlatformView;
            if (wv?.Settings == null) return;

            wv.Settings.AllowFileAccess = false;
            wv.Settings.AllowContentAccess = false;
            wv.Settings.AllowFileAccessFromFileURLs = false;
            wv.Settings.AllowUniversalAccessFromFileURLs = false;
            wv.Settings.JavaScriptEnabled = true;
            wv.Settings.DomStorageEnabled = true;
            wv.Settings.JavaScriptCanOpenWindowsAutomatically = false;
            wv.Settings.SetSupportMultipleWindows(false);
        });
#endif
    }

    public static void ClearCookiesAndCache()
    {
#if ANDROID
        try
        {
            Android.Webkit.CookieManager.Instance?.RemoveAllCookies(null);
            Android.Webkit.CookieManager.Instance?.Flush();
            Android.Webkit.WebStorage.Instance?.DeleteAllData();
        }
        catch
        {
        }
#endif
    }
}
