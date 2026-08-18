using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MULTI_Bet_playing_Demo.Models;

namespace MULTI_Bet_playing_Demo.Services;

public sealed class TrustSecurityEngine
{
    private readonly SpaPublicRegistrySource _registrySource;

    public TrustSecurityEngine(SpaPublicRegistrySource registrySource)
    {
        _registrySource = registrySource;
    }

    public async Task<TrustReport> VerifyAsync(string input, CancellationToken cancellationToken = default)
    {
        var query = input?.Trim() ?? string.Empty;
        if (query.Length == 0)
            return CreateInvalid(query, "Informe um nome, empresa, CNPJ ou URL.");

        Uri? uri = TryCreateHttpsOrHttpUri(query);
        var host = uri?.IdnHost.TrimEnd('.').ToLowerInvariant();

        IReadOnlyList<PublicOperatorRecord> records;
        try
        {
            records = await _registrySource.GetOperatorsAsync(cancellationToken);
        }
        catch
        {
            return new TrustReport
            {
                Query = query,
                NormalizedUrl = uri?.AbsoluteUri,
                Host = host,
                CheckedAt = DateTimeOffset.UtcNow,
                Status = TrustStatus.SourceUnavailable,
                Source = SpaPublicRegistrySource.SourceUrl,
                Summary = "A fonte pública oficial não pôde ser consultada agora. Nenhuma conclusão de legitimidade foi feita."
            };
        }

        var match = FindMatch(query, host, records);
        var checks = new List<TrustCheck>();

        if (uri is not null)
        {
            checks.Add(new TrustCheck(
                "Esquema seguro",
                string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase),
                uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                    ? "A URL usa HTTPS."
                    : "A URL usa HTTP; prefira o domínio HTTPS oficial."));

            var suspiciousIdn = host?.Contains("xn--", StringComparison.OrdinalIgnoreCase) == true || ContainsNonAsciiHost(host);
            checks.Add(new TrustCheck(
                "Domínio internacionalizado",
                !suspiciousIdn,
                suspiciousIdn ? "O domínio contém indicadores de IDN/punycode; verifique cuidadosamente a identidade visual." : "Nenhum indicador de IDN/punycode foi detectado."));
        }

        if (match is not null)
        {
            var exactDomain = host is not null && match.Domains.Any(d => string.Equals(d, host, StringComparison.OrdinalIgnoreCase));
            checks.Add(new TrustCheck("Domínio na fonte oficial", exactDomain, exactDomain ? "O domínio informado consta na relação pública da SPA." : "A empresa/marca foi encontrada, mas o domínio informado não corresponde a um domínio oficial listado.", SpaPublicRegistrySource.SourceUrl));

            var brandMatch = match.Brands.Any(b => NormalizeText(b) == NormalizeText(query));
            checks.Add(new TrustCheck("Identidade da plataforma", true, brandMatch ? "A marca pesquisada corresponde ao registro público." : "Foi encontrada uma relação pública compatível por empresa, CNPJ ou domínio.", SpaPublicRegistrySource.SourceUrl));
        }
        else
        {
            checks.Add(new TrustCheck("Registro público encontrado", false, "Não foi encontrada correspondência exata na fonte pública consultada.", SpaPublicRegistrySource.SourceUrl));
        }

        var possibleImitation = host is not null && match is null && FindSimilarDomain(host, records.SelectMany(x => x.Domains)) is not null;
        if (possibleImitation)
        {
            var similar = FindSimilarDomain(host!, records.SelectMany(x => x.Domains));
            checks.Add(new TrustCheck("Similaridade de domínio", false, $"O domínio informado é parecido com um domínio oficial ({similar}), mas não corresponde exatamente a ele."));
        }

        var status = DetermineStatus(uri, host, match, possibleImitation, checks);
        return BuildReport(query, uri, host, match, status, checks);
    }

    private static PublicOperatorRecord? FindMatch(string query, string? host, IEnumerable<PublicOperatorRecord> records)
    {
        var normalizedQuery = NormalizeText(query);
        var digits = new string(query.Where(char.IsDigit).ToArray());

        return records.FirstOrDefault(record =>
            (host is not null && record.Domains.Any(d => string.Equals(d, host, StringComparison.OrdinalIgnoreCase))) ||
            record.Brands.Any(b => NormalizeText(b) == normalizedQuery) ||
            NormalizeText(record.Company) == normalizedQuery ||
            (!string.IsNullOrEmpty(digits) && new string(record.Cnpj.Where(char.IsDigit).ToArray()) == digits));
    }

    private static TrustStatus DetermineStatus(Uri? uri, string? host, PublicOperatorRecord? match, bool possibleImitation, IReadOnlyList<TrustCheck> checks)
    {
        if (possibleImitation)
            return TrustStatus.PossibleImitation;
        if (match is null)
            return TrustStatus.NotVerified;
        if (uri is not null && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return TrustStatus.Attention;
        if (host is not null && !match.Domains.Contains(host, StringComparer.OrdinalIgnoreCase))
            return TrustStatus.Attention;
        if (checks.Any(x => !x.Passed && x.Name != "Domínio internacionalizado"))
            return TrustStatus.Attention;
        return TrustStatus.Verified;
    }

    private static TrustReport BuildReport(string query, Uri? uri, string? host, PublicOperatorRecord? match, TrustStatus status, IReadOnlyList<TrustCheck> checks)
    {
        var related = match?.Domains ?? Array.Empty<string>();
        var summary = status switch
        {
            TrustStatus.Verified => "A identidade consultada corresponde a um registro público oficial e o domínio informado corresponde ao domínio listado.",
            TrustStatus.Attention => "Há uma correspondência pública, mas existe alguma divergência técnica que precisa ser conferida antes do acesso.",
            TrustStatus.PossibleImitation => "O domínio não corresponde ao registro oficial e apresenta similaridade com um domínio listado. Trate como possível imitação.",
            TrustStatus.NotVerified => "Não foi possível confirmar a identidade na fonte pública consultada. Isso não significa, por si só, que seja fraude.",
            _ => "Não foi possível concluir a verificação."
        };

        return new TrustReport
        {
            Query = query,
            NormalizedUrl = uri?.AbsoluteUri,
            Host = host,
            Brand = match?.Brands.FirstOrDefault(),
            Company = match?.Company,
            Cnpj = match?.Cnpj,
            OfficialDomain = match?.Domains.FirstOrDefault(d => string.Equals(d, host, StringComparison.OrdinalIgnoreCase)) ?? match?.Domains.FirstOrDefault(),
            Authorization = match?.Authorization,
            RelatedDomains = related,
            Source = SpaPublicRegistrySource.SourceUrl,
            CheckedAt = DateTimeOffset.UtcNow,
            Status = status,
            Checks = checks,
            Summary = summary
        };
    }

    private static Uri? TryCreateHttpsOrHttpUri(string value)
    {
        var candidate = value.Contains("://", StringComparison.Ordinal) ? value : "https://" + value;
        return Uri.TryCreate(candidate, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp) &&
               !string.IsNullOrWhiteSpace(uri.Host)
            ? uri
            : null;
    }

    private static string? FindSimilarDomain(string host, IEnumerable<string> domains)
    {
        var best = domains
            .Select(domain => new { Domain = domain, Distance = Levenshtein(host, domain) })
            .Where(x => x.Distance <= Math.Max(2, Math.Min(3, host.Length / 5)))
            .OrderBy(x => x.Distance)
            .FirstOrDefault();
        return best?.Domain;
    }

    private static int Levenshtein(string a, string b)
    {
        var previous = Enumerable.Range(0, b.Length + 1).ToArray();
        var current = new int[b.Length + 1];
        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
            }
            (previous, current) = (current, previous);
        }
        return previous[b.Length];
    }

    private static string NormalizeText(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(c));
        return Regex.Replace(builder.ToString(), @"[^a-z0-9]+", "").Trim();
    }

    private static bool ContainsNonAsciiHost(string? host) => host?.Any(c => c > 127) == true;

    private static TrustReport CreateInvalid(string query, string summary) => new()
    {
        Query = query,
        Status = TrustStatus.Unknown,
        CheckedAt = DateTimeOffset.UtcNow,
        Summary = summary
    };
}
