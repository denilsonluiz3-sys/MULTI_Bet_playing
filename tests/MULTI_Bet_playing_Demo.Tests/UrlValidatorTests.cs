using MULTI_Bet_playing_Demo.Services;
using Xunit;

namespace MULTI_Bet_playing_Demo.Tests;

public sealed class UrlValidatorTests
{
    [Fact]
    public void NormalizesHostWithoutSchemeToHttps()
    {
        var ok = UrlValidator.TryNormalize("example.com/path", out var normalized, out var error);

        Assert.True(ok, error);
        Assert.Equal("https://example.com/path", normalized);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,test")]
    [InlineData("file:///tmp/test")]
    [InlineData("content://local")]
    [InlineData("intent://example.com")]
    [InlineData("market://details?id=test")]
    public void RejectsBlockedSchemes(string input)
    {
        Assert.False(UrlValidator.TryNormalize(input, out _, out _));
    }

    [Theory]
    [InlineData("http://localhost/test")]
    [InlineData("http://127.0.0.1/test")]
    [InlineData("http://10.0.0.1/test")]
    [InlineData("http://172.16.0.1/test")]
    [InlineData("http://172.31.255.254/test")]
    [InlineData("http://192.168.1.10/test")]
    [InlineData("http://169.254.1.1/test")]
    [InlineData("http://[::1]/test")]
    [InlineData("http://[fc00::1]/test")]
    public void RejectsLocalPrivateAndLinkLocalAddresses(string input)
    {
        Assert.False(UrlValidator.TryNormalize(input, out _, out _));
    }

    [Theory]
    [InlineData("http://8.8.8.8")]
    [InlineData("https://example.com")]
    public void AcceptsPublicHttpOrHttpsTargets(string input)
    {
        Assert.True(UrlValidator.TryNormalize(input, out _, out var error), error);
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("ws://example.com")]
    [InlineData("file://example.com")]
    public void RejectsUnsupportedProtocols(string input)
    {
        Assert.False(UrlValidator.TryNormalize(input, out _, out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectsEmptyInput(string? input)
    {
        Assert.False(UrlValidator.TryNormalize(input, out _, out _));
    }

    [Fact]
    public void RejectsHostContainingSpaces()
    {
        Assert.False(UrlValidator.TryNormalize("https://bad host.example", out _, out _));
    }

    [Fact]
    public void HttpsIsPreferredOnlyForHttps()
    {
        Assert.True(UrlValidator.IsHttpsPreferred("https://example.com"));
        Assert.False(UrlValidator.IsHttpsPreferred("http://example.com"));
        Assert.False(UrlValidator.IsHttpsPreferred("not-a-url"));
    }
}
