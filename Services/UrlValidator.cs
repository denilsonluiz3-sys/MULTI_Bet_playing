namespace MULTI_Bet_playing_Demo.Services;

public static class UrlValidator
{
    private static readonly HashSet<string> BlockedSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "javascript", "data", "file", "content", "about", "blob", "intent", "market"
    };

    public static bool TryNormalize(string? input, out string normalized, out string error)
    {
        normalized = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "URL vazia.";
            return false;
        }

        var raw = input.Trim();
        var schemePart = raw.Split(':', 2)[0];
        if (BlockedSchemes.Contains(schemePart))
        {
            error = "Scheme de URL n\u00e3o permitido.";
            return false;
        }

        if (!raw.Contains("://", StringComparison.Ordinal))
            raw = "https://" + raw;

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
        {
            error = "URL inv\u00e1lida.";
            return false;
        }

        if (uri.Scheme is not ("https" or "http"))
        {
            error = "Use apenas http:// ou https://.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(uri.Host) || uri.Host.Contains(' '))
        {
            error = "Host inv\u00e1lido.";
            return false;
        }

        if (uri.IsLoopback ||
            uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            uri.Host.StartsWith("127.", StringComparison.Ordinal) ||
            uri.Host.StartsWith("169.254.", StringComparison.Ordinal) ||
            uri.Host.StartsWith("10.", StringComparison.Ordinal) ||
            uri.Host.StartsWith("192.168.", StringComparison.Ordinal))
        {
            error = "Endere\u00e7os locais n\u00e3o s\u00e3o permitidos.";
            return false;
        }

        normalized = uri.AbsoluteUri;
        return true;
    }

    public static bool IsHttpsPreferred(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var u) && u.Scheme == "https";
}
