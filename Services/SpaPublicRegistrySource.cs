using System.Net;
using System.Text.RegularExpressions;
using MULTI_Bet_playing_Demo.Models;

namespace MULTI_Bet_playing_Demo.Services;

public sealed class SpaPublicRegistrySource
{
    public const string SourceUrl = "https://www.gov.br/fazenda/pt-br/composicao/orgaos/secretaria-de-premios-e-apostas/lista-de-empresas";

    private readonly HttpClient _httpClient;

    public SpaPublicRegistrySource(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.All
        });
        _httpClient.Timeout = TimeSpan.FromSeconds(20);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MULTI_Bet/1.0 (public-source-verification)");
    }

    public async Task<IReadOnlyList<PublicOperatorRecord>> GetOperatorsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(SourceUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParseOperators(html);
    }

    internal static IReadOnlyList<PublicOperatorRecord> ParseOperators(string html)
    {
        var records = new List<PublicOperatorRecord>();

        foreach (Match rowMatch in Regex.Matches(html, "<tr\\b[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            var cells = Regex.Matches(rowMatch.Groups[1].Value, "<(?:td|th)\\b[^>]*>(.*?)</(?:td|th)>", RegexOptions.IgnoreCase | RegexOptions.Singleline)
                .Select(m => CleanHtml(m.Groups[1].Value))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (cells.Length < 5 || !LooksLikeCnpj(cells[1]))
                continue;

            var brands = SplitItems(cells[2]);
            var domains = SplitItems(cells[3]).Select(NormalizeDomain).Where(x => x.Length > 0).ToArray();
            if (domains.Length == 0)
                continue;

            records.Add(new PublicOperatorRecord(
                cells[0],
                cells[1],
                brands,
                domains,
                cells[4],
                cells.Length > 5 ? cells[5] : null));
        }

        return records
            .GroupBy(x => string.Join("|", x.Company, x.Cnpj, string.Join(",", x.Domains)), StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToArray();
    }

    private static string CleanHtml(string value)
    {
        var noTags = Regex.Replace(value, "<[^>]+>", " ", RegexOptions.Singleline);
        return WebUtility.HtmlDecode(noTags)
            .Replace('\u00a0', ' ')
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ")
            .Trim();
    }

    private static IReadOnlyList<string> SplitItems(string value) =>
        Regex.Split(value, @"\s{2,}|\s*[•·]\s*|\s*;\s*")
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string NormalizeDomain(string value)
    {
        value = value.Trim().TrimEnd('/');
        if (Uri.TryCreate(value.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? value : "https://" + value, UriKind.Absolute, out var uri))
            return uri.IdnHost.TrimEnd('.').ToLowerInvariant();
        return value.ToLowerInvariant();
    }

    private static bool LooksLikeCnpj(string value) =>
        Regex.IsMatch(value, @"\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}|\d{14}");
}
