namespace MULTI_Bet_playing_Demo.Models;

public enum TrustStatus
{
    Unknown,
    Verified,
    Attention,
    NotVerified,
    PossibleImitation,
    SourceUnavailable
}

public sealed record TrustInput(string Value);

public sealed record TrustCheck(
    string Name,
    bool Passed,
    string Detail,
    string? Source = null);

public sealed class TrustReport
{
    public TrustStatus Status { get; init; } = TrustStatus.Unknown;
    public string Query { get; init; } = string.Empty;
    public string? NormalizedUrl { get; init; }
    public string? Host { get; init; }
    public string? Brand { get; init; }
    public string? Company { get; init; }
    public string? Cnpj { get; init; }
    public string? OfficialDomain { get; init; }
    public string? Authorization { get; init; }
    public string? Source { get; init; }
    public DateTimeOffset CheckedAt { get; init; }
    public IReadOnlyList<TrustCheck> Checks { get; init; } = Array.Empty<TrustCheck>();
    public IReadOnlyList<string> RelatedDomains { get; init; } = Array.Empty<string>();
    public string Summary { get; init; } = string.Empty;
}

public sealed record PublicOperatorRecord(
    string Company,
    string Cnpj,
    IReadOnlyList<string> Brands,
    IReadOnlyList<string> Domains,
    string Authorization,
    string? Process = null);
