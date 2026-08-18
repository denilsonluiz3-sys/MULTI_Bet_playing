using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MULTI_Bet_playing_Demo.Models;

namespace MULTI_Bet_playing_Demo.Services;

public sealed class SpaPublicRegistrySource
{
    public const string SourceUrl = "https://www.gov.br/fazenda/pt-br/composicao/orgaos/secretaria-de-premios-e-apostas/lista-de-empresas/confira-a-lista-de-empresas-autorizadas-a-ofertar-apostas-de-quota-fixa-em-2025";
    public const string ServiceUrl = "https://www.gov.br/pt-br/servicos/consultar-as-empresas-autorizadas-a-operar-apostas-de-quota-fixa";

    private readonly HttpClient _httpClient;

    public SpaPublicRegistrySource(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.All
        });
        _httpClient.Timeout = TimeSpan.FromSeconds(20);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MultiBetRiskGuard/1.0 (public-source-verification)");
    }

    public async Task<IReadOnlyList<PublicOperatorRecord>> GetOperatorsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(SourceUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        var csvLinks = ExtractCsvLinks(html, SourceUrl);
        var records = new List<PublicOperatorRecord>();

        if (csvLinks.Count > 0)
        {
            records.AddRange(ParseCsv(await DownloadTextAsync(csvLinks[0], cancellationToken), false));
            if (csvLinks.Count > 1)
                records.AddRange(ParseCsv(await DownloadTextAsync(csvLinks[1], cancellationToken), true));
        }

        if (records.Count == 0)
            records.AddRange(ParseOperators(html, false));

        return records
            .GroupBy(x => string.Join("|", x.Company, x.Cnpj, string.Join(",", x.Domains), x.IsJudicial), StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToArray();
    }

    internal static IReadOnlyList<PublicOperatorRecord> ParseOperators(string html, bool judicial)
    {
        var records = new List<PublicOperatorRecord>();
        foreach (Match rowMatch in Regex.Matches(html, "<tr\\b[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            var cells = Regex.Matches(rowMatch.Groups[1].Value, "<(?:td|th)\\b[^>]*>(.*?)</(?:td|th)>", RegexOptions.IgnoreCase | RegexOptions.Singleline)
                .Select(m => CleanHtml(m.Groups[1].Value)).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (cells.Length < 5 || !LooksLikeCnpj(cells[1])) continue;

            var domains = SplitItems(cells[3]).Select(NormalizeDomain).Where(x => x.Length > 0).ToArray();
            if (domains.Length == 0) continue;

            records.Add(new PublicOperatorRecord(
                cells[0], cells[1], SplitItems(cells[2]), domains, cells[4], cells.Length > 5 ? cells[5] : null,
                judicial, judicial ? "SPA/MF — determinação judicial" : "SPA/MF — empresas autorizadas"));
        }
        return records;
    }

    private async Task<string> DownloadTextAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static IReadOnlyList<string> ExtractCsvLinks(string html, string pageUrl)
    {
        var links = new List<string>();
        foreach (Match match in Regex.Matches(html, "<a\\b[^>]*href=[\\\"'](?<href>[^\\\"']+)[\\\"'][^>]*>(?<text>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            var href = WebUtility.HtmlDecode(match.Groups["href"].Value);
            var text = CleanHtml(match.Groups["text"].Value);
            if (!href.Contains(".csv", StringComparison.OrdinalIgnoreCase) && !text.Contains("CSV", StringComparison.OrdinalIgnoreCase)) continue;
            if (Uri.TryCreate(new Uri(pageUrl), href, out var absolute)) links.Add(absolute.AbsoluteUri);
        }
        return links.Distinct(StringComparer.OrdinalIgnoreCase).Take(2).ToArray();
    }

    private static IReadOnlyList<PublicOperatorRecord> ParseCsv(string csv, bool judicial)
    {
        var lines = csv.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return Array.Empty<PublicOperatorRecord>();

        var delimiter = lines[0].Contains(';') ? ';' : ',';
        var header = ParseCsvLine(lines[0], delimiter).Select(NormalizeHeader).ToArray();
        var companyIndex = FindColumn(header, "empresa", "denominacaosocial", "razaosocial");
        var cnpjIndex = FindColumn(header, "cnpj");
        var brandIndex = FindColumn(header, "marcas", "marca");
        var domainIndex = FindColumn(header, "dominios", "dominio");
        var authorizationIndex = FindColumn(header, "portaria", "autorizacao", "portariadeautorizacao");
        var processIndex = FindColumn(header, "processo", "requerimento");
        if (companyIndex < 0 || cnpjIndex < 0 || brandIndex < 0 || domainIndex < 0) return Array.Empty<PublicOperatorRecord>();

        var records = new List<PublicOperatorRecord>();
        foreach (var line in lines.Skip(1))
        {
            var cells = ParseCsvLine(line, delimiter);
            var max = Math.Max(Math.Max(companyIndex, cnpjIndex), Math.Max(brandIndex, domainIndex));
            if (cells.Count <= max || !LooksLikeCnpj(cells[cnpjIndex])) continue;

            var domains = SplitItems(cells[domainIndex]).Select(NormalizeDomain).Where(x => x.Length > 0).ToArray();
            if (domains.Length == 0) continue;

            records.Add(new PublicOperatorRecord(
                cells[companyIndex], cells[cnpjIndex], SplitItems(cells[brandIndex]), domains,
                authorizationIndex >= 0 && authorizationIndex < cells.Count ? cells[authorizationIndex] : "",
                processIndex >= 0 && processIndex < cells.Count ? cells[processIndex] : null,
                judicial, judicial ? "SPA/MF — determinação judicial" : "SPA/MF — empresas autorizadas"));
        }
        return records;
    }

    private static List<string> ParseCsvLine(string line, char delimiter)
    {
        var result = new List<string>();
        var value = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '"') { value.Append('"'); i++; }
                else quoted = !quoted;
            }
            else if (c == delimiter && !quoted) { result.Add(value.ToString().Trim()); value.Clear(); }
            else value.Append(c);
        }
        result.Add(value.ToString().Trim());
        return result;
    }

    private static int FindColumn(IReadOnlyList<string> header, params string[] candidates)
    {
        for (var i = 0; i < header.Count; i++)
            if (candidates.Any(candidate => header[i].Contains(candidate, StringComparison.OrdinalIgnoreCase))) return i;
        return -1;
    }

    private static string NormalizeHeader(string value) => Regex.Replace(value.Normalize(NormalizationForm.FormD), "[^A-Za-z0-9]", "").ToLowerInvariant();

    private static string CleanHtml(string value)
    {
        value = Regex.Replace(value, "<(?:br|/a|/li)\\b[^>]*>", "\n", RegexOptions.IgnoreCase);
        var noTags = Regex.Replace(value, "<[^>]+>", " ", RegexOptions.Singleline);
        return WebUtility.HtmlDecode(noTags).Replace('\u00a0', ' ').Replace("\r", "").Replace("\t", " ").Trim();
    }

    private static IReadOnlyList<string> SplitItems(string value) =>
        Regex.Split(value, "\\s*\\n\\s*|\\s{2,}|\\s*[•·]\\s*|\\s*;\\s*")
            .Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static string NormalizeDomain(string value)
    {
        value = value.Trim().TrimEnd('/');
        if (Uri.TryCreate(value.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? value : "https://" + value, UriKind.Absolute, out var uri))
            return uri.IdnHost.TrimEnd('.').ToLowerInvariant();
        return value.ToLowerInvariant();
    }

    private static bool LooksLikeCnpj(string value) => Regex.IsMatch(value, "\\d{2}\\.\\d{3}\\.\\d{3}/\\d{4}-\\d{2}|\\d{14}");
}
