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
    public void RejectsBlockedSchemes(string input)
    {
        Assert.False(UrlValidator.TryNormalize(input, out _, out _));
    }

    [Theory]
    [InlineData("http://localhost/test")]
    [InlineData("http://127.0.0.1/test")]
    [InlineData("http://10.0.0.1/test")]
    [InlineData("http://192.168.1.10/test")]
    [InlineData("http://169.254.1.1/test")]
    public void RejectsLocalAndLinkLocalAddresses(string input)
    {
        Assert.False(UrlValidator.TryNormalize(input, out _, out _));
    }

    [Fact]
    public void AcceptsPublicHttpsUrl()
    {
        Assert.True(UrlValidator.TryNormalize("https://example.com", out var normalized, out var error), error);
        Assert.True(UrlValidator.IsHttpsPreferred(normalized));
    }

    [Fact]
    public void RejectsUnsupportedScheme()
    {
        Assert.False(UrlValidator.TryNormalize("ftp://example.com", out _, out _));
    }
}
