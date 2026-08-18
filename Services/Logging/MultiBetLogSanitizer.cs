using System.Text.RegularExpressions;

namespace MULTI_Bet_playing_Demo.Services.Logging;

internal static partial class MultiBetLogSanitizer
{
    public static string? Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var safe = value;

        // Never persist authentication material, API keys, or common secret formats.
        safe = BearerRegex().Replace(safe, "Bearer [REDACTED]");
        safe = JwtRegex().Replace(safe, "[JWT_REDACTED]");
        safe = KeyValueSecretRegex().Replace(safe, "$1[REDACTED]");

        // Avoid persisting query strings/fragments that may contain user-specific data.
        safe = QueryRegex().Replace(safe, "$1[QUERY_REDACTED]");

        // Common personal identifiers that should never be useful to diagnostics.
        safe = EmailRegex().Replace(safe, "[EMAIL_REDACTED]");
        safe = PhoneRegex().Replace(safe, "[PHONE_REDACTED]");
        safe = CpfRegex().Replace(safe, "[CPF_REDACTED]");

        return safe;
    }

    [GeneratedRegex(@"(?i)Bearer\s+[A-Za-z0-9._~+/=-]+", RegexOptions.CultureInvariant)]
    private static partial Regex BearerRegex();

    [GeneratedRegex(@"\beyJ[A-Za-z0-9_-]+\.[A-Za-z0-9._-]+\.[A-Za-z0-9._-]+\b", RegexOptions.CultureInvariant)]
    private static partial Regex JwtRegex();

    [GeneratedRegex(@"(?i)(api[-_]?key|access[-_]?token|refresh[-_]?token|authorization|password|passwd|secret|client[-_]?secret)\s*[:=]\s*[^\s,;&]+", RegexOptions.CultureInvariant)]
    private static partial Regex KeyValueSecretRegex();

    [GeneratedRegex(@"(https?://[^\s?#]+)[?#][^\s]+", RegexOptions.CultureInvariant)]
    private static partial Regex QueryRegex();

    [GeneratedRegex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(?<!\d)(?:\+?\d[\d ()-]{7,}\d)(?!\d)", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"\b\d{3}\.?\d{3}\.?\d{3}-?\d{2}\b", RegexOptions.CultureInvariant)]
    private static partial Regex CpfRegex();
}
