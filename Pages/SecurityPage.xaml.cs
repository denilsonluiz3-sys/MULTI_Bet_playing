using MULTI_Bet_playing_Demo.Models;
using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class SecurityPage : ContentPage
{
    private readonly TrustSecurityEngine _engine;
    private readonly CardService _cardService;
    private TrustReport? _lastReport;

    public SecurityPage() : this(ResolveEngine(), ResolveCardService()) { }

    public SecurityPage(TrustSecurityEngine engine, CardService cardService)
    {
        InitializeComponent();
        _engine = engine;
        _cardService = cardService;
    }

    private static TrustSecurityEngine ResolveEngine() =>
        Application.Current?.Handler?.MauiContext?.Services?.GetService<TrustSecurityEngine>()
        ?? new TrustSecurityEngine(new SpaPublicRegistrySource());

    private static CardService ResolveCardService() =>
        Application.Current?.Handler?.MauiContext?.Services?.GetService<CardService>()
        ?? new CardService();

    private async void OnVerifyClicked(object? sender, EventArgs e)
    {
        var query = QueryEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            await DisplayAlertAsync("Verificar", "Informe uma marca, empresa, CNPJ ou URL.", "OK");
            return;
        }

        SetBusyState(true);
        try
        {
            var report = await _engine.VerifyAsync(query);
            _lastReport = report;
            Render(report);
        }
        finally
        {
            SetBusyState(false);
        }
    }

    private void Render(TrustReport report)
    {
        ResultCard.IsVisible = true;
        StatusLabel.Text = StatusText(report.Status);
        StatusLabel.TextColor = StatusColor(report.Status);
        SummaryLabel.Text = report.Summary;
        IdentityLabel.Text = BuildIdentity(report);
        SourceLabel.Text = $"Fonte: {report.Source ?? "não disponível"}\nConsulta: {report.CheckedAt.ToLocalTime():dd/MM/yyyy HH:mm:ss}";
        ChecksCollection.ItemsSource = report.Checks;
        AddButton.IsVisible = report.Status == TrustStatus.Verified && !string.IsNullOrWhiteSpace(report.OfficialDomain);
    }

    private async void OnAddToCentralClicked(object? sender, EventArgs e)
    {
        if (_lastReport?.Status != TrustStatus.Verified || string.IsNullOrWhiteSpace(_lastReport.OfficialDomain))
            return;

        var title = _lastReport.Brand ?? _lastReport.Company ?? _lastReport.OfficialDomain;
        var exists = (await _cardService.GetCardsAsync()).Any(c =>
            string.Equals(c.Url, $"https://{_lastReport.OfficialDomain}/", StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            await DisplayAlertAsync("Central", "Essa plataforma já está organizada na Central.", "OK");
            return;
        }

        await _cardService.AddCardAsync(new CardItem
        {
            Title = title,
            Url = $"https://{_lastReport.OfficialDomain}",
            Icon = "🛡️",
            Category = "Verificados",
            IsFavorite = false,
            LastUsed = DateTime.UtcNow
        });

        await DisplayAlertAsync("Central", "Plataforma adicionada à Central.", "OK");
    }

    private static string BuildIdentity(TrustReport report)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(report.Brand)) parts.Add($"Marca: {report.Brand}");
        if (!string.IsNullOrWhiteSpace(report.Company)) parts.Add($"Empresa: {report.Company}");
        if (!string.IsNullOrWhiteSpace(report.Cnpj)) parts.Add($"CNPJ: {report.Cnpj}");
        if (!string.IsNullOrWhiteSpace(report.OfficialDomain)) parts.Add($"Domínio: {report.OfficialDomain}");
        if (!string.IsNullOrWhiteSpace(report.Authorization)) parts.Add($"Autorização: {report.Authorization}");
        return parts.Count == 0 ? "Nenhuma identidade pública confirmada." : string.Join("\n", parts);
    }

    private static string StatusText(TrustStatus status) => status switch
    {
        TrustStatus.Verified => "🟢 VERIFICADO",
        TrustStatus.Attention => "🟡 ATENÇÃO",
        TrustStatus.PossibleImitation => "🚨 POSSÍVEL IMITAÇÃO",
        TrustStatus.NotVerified => "🔴 NÃO VERIFICADO",
        TrustStatus.SourceUnavailable => "⚪ FONTE INDISPONÍVEL",
        _ => "⚪ SEM CONCLUSÃO"
    };

    private static Color StatusColor(TrustStatus status) => status switch
    {
        TrustStatus.Verified => Colors.Green,
        TrustStatus.Attention => Colors.Orange,
        TrustStatus.PossibleImitation => Colors.Red,
        TrustStatus.NotVerified => Colors.Red,
        TrustStatus.SourceUnavailable => Colors.Gray,
        _ => Colors.Gray
    };

    private void SetBusyState(bool busy)
    {
        QueryEntry.IsEnabled = !busy;
        if (Content is ScrollView scroll && scroll.Content is Layout layout)
        {
            foreach (var child in layout.Children)
                if (child is Button button && button != AddButton)
                    button.IsEnabled = !busy;
        }
    }
}
