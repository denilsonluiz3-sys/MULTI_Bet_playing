using MULTI_Bet_playing_Demo.Services;
using Xunit;

namespace MULTI_Bet_playing_Demo.Tests;

public sealed class WebViewSecurityTests
{
    [Fact]
    public void MobileUserAgentIsChromeBased()
    {
        Assert.Contains("Android", WebViewSecurity.ChromeMobileUa);
        Assert.Contains("Chrome/", WebViewSecurity.ChromeMobileUa);
        Assert.Contains("Mobile", WebViewSecurity.ChromeMobileUa);
    }

    [Fact]
    public void DesktopUserAgentIsChromeBased()
    {
        Assert.Contains("Windows NT", WebViewSecurity.ChromeDesktopUa);
        Assert.Contains("Chrome/", WebViewSecurity.ChromeDesktopUa);
    }

    [Fact]
    public void MobileAndDesktopUserAgentsAreDistinct()
    {
        Assert.NotEqual(WebViewSecurity.ChromeMobileUa, WebViewSecurity.ChromeDesktopUa);
    }

    [Fact]
    public void ConfigureHandlersIsSafeOutsideAndroid()
    {
        var exception = Record.Exception(WebViewSecurity.ConfigureHandlers);

        Assert.Null(exception);
    }

    [Fact]
    public void ClearCookiesAndCacheIsSafeOutsideAndroid()
    {
        var exception = Record.Exception(WebViewSecurity.ClearCookiesAndCache);

        Assert.Null(exception);
    }
}
