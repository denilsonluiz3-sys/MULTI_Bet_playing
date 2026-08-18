namespace MULTI_Bet_playing_Demo.Services;

public static class WebViewSecurity
{
    public const string ChromeMobileUa =
        "Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Mobile Safari/537.36";

    public const string ChromeDesktopUa =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36";

    public static void ConfigureHandlers()
    {
#if ANDROID
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("MultiBetBrowser", (handler, view) =>
        {
            try
            {
                var wv = handler.PlatformView;
                if (wv?.Settings == null) return;

                var s = wv.Settings;
                s.JavaScriptEnabled = true;
                s.DomStorageEnabled = true;
                s.DatabaseEnabled = true;
                s.LoadsImagesAutomatically = true;
                s.MediaPlaybackRequiresUserGesture = false;
                s.JavaScriptCanOpenWindowsAutomatically = true;
                s.SetSupportMultipleWindows(true);
                s.SetSupportZoom(true);
                s.BuiltInZoomControls = true;
                s.DisplayZoomControls = false;
                s.UseWideViewPort = true;
                s.LoadWithOverviewMode = true;

                s.AllowFileAccess = false;
                s.AllowContentAccess = true;
                s.AllowFileAccessFromFileURLs = false;
                s.AllowUniversalAccessFromFileURLs = false;

                if (string.IsNullOrEmpty(s.UserAgentString) || s.UserAgentString.Contains("; wv)"))
                    s.UserAgentString = ChromeMobileUa;

                var cm = Android.Webkit.CookieManager.Instance;
                if (cm != null)
                {
                    cm.SetAcceptCookie(true);
                    cm.SetAcceptThirdPartyCookies(wv, true);
                }

                wv.SetWebChromeClient(new MultiBetChromeClient(wv));
            }
            catch { }
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
        catch { }
#endif
    }

#if ANDROID
    sealed class MultiBetChromeClient : Android.Webkit.WebChromeClient
    {
        private readonly Android.Webkit.WebView _host;
        public MultiBetChromeClient(Android.Webkit.WebView host) => _host = host;

        public override bool OnCreateWindow(
            Android.Webkit.WebView? view, bool isDialog, bool isUserGesture, Android.OS.Message? resultMsg)
        {
            try
            {
                if (resultMsg?.Obj is Android.Webkit.WebView.WebViewTransport transport)
                {
                    transport.WebView = _host;
                    resultMsg.SendToTarget();
                    return true;
                }
            }
            catch { }
            return false;
        }

        public override void OnPermissionRequest(Android.Webkit.PermissionRequest? request)
        {
            try { request?.Deny(); } catch { }
        }
    }
#endif
}
