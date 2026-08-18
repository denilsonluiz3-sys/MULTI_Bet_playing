using System.Text.Json;
using MULTI_Bet_playing_Demo.Models;
using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly CardService _cardService;

    public SettingsPage() : this(ResolveCardService()) { }

    public SettingsPage(CardService cardService)
    {
        InitializeComponent();
        _cardService = cardService;
        LegalLabel.Text = ComplianceService.DisclaimerFull;
        LoadCurrentTheme();
    }

    private static CardService ResolveCardService() =>
        Application.Current?.Handler?.MauiContext?.Services?.GetService<CardService>()
        ?? new CardService();

    private void LoadCurrentTheme()
    {
        var theme = ThemeManager.CurrentTheme;
        DarkRadio.IsChecked = theme == ThemeManager.Dark;
        LightRadio.IsChecked = theme == ThemeManager.Light;
        CasinoRadio.IsChecked = theme == ThemeManager.Casino;
    }

    private void OnThemeChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value || sender is not RadioButton radio) return;
        string theme = radio.Content?.ToString() switch
        {
            var s when s?.Contains("Light") == true => ThemeManager.Light,
            var s when s?.Contains("Casino") == true => ThemeManager.Casino,
            _ => ThemeManager.Dark
        };
        ThemeManager.ApplyTheme(theme);
    }

    private async void OnClearWebData(object? sender, EventArgs e)
    {
        var ok = await DisplayAlertAsync("Limpar dados web",
            "Isso remove cookies e armazenamento dos WebViews neste aparelho. Continuar?", "Limpar", "Cancelar");
        if (!ok) return;
        WebViewSecurity.ClearCookiesAndCache();
        await DisplayAlertAsync("Pronto", "Cookies e cache web limpos (melhor esforço).", "OK");
    }

    private async void OnExport(object? sender, EventArgs e)
    {
        try
        {
            var cards = await _cardService.GetCardsAsync();
            var json = JsonSerializer.Serialize(cards, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(FileSystem.CacheDirectory, $"multibet-links-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
            await File.WriteAllTextAsync(path, json);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Exportar links MULTI Bet",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Falha ao exportar: {ex.Message}", "OK");
        }
    }

    private async void OnImport(object? sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione o JSON de links",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/json", "text/plain", "*/*" } },
                    { DevicePlatform.WinUI, new[] { ".json" } },
                    { DevicePlatform.iOS, new[] { "public.json", "public.text" } }
                })
            });
            if (result == null) return;
            await using var stream = await result.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var cards = JsonSerializer.Deserialize<List<CardItem>>(json);
            if (cards == null || cards.Count == 0)
            {
                await DisplayAlertAsync("Importar", "Arquivo vazio ou inválido.", "OK");
                return;
            }
            int added = 0;
            foreach (var c in cards)
            {
                if (string.IsNullOrWhiteSpace(c.Url)) continue;
                if (!UrlValidator.TryNormalize(c.Url, out var url, out _)) continue;
                c.Url = url;
                c.Id = Guid.NewGuid().ToString();
                if (string.IsNullOrWhiteSpace(c.Title)) c.Title = url;
                await _cardService.AddCardAsync(c);
                added++;
            }
            await DisplayAlertAsync("Importar", $"{added} link(s) importado(s).", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Falha ao importar: {ex.Message}", "OK");
        }
    }

    private async void OnResetCompliance(object? sender, EventArgs e)
    {
        var ok = await DisplayAlertAsync("Redefinir aceite",
            "Na próxima abertura o app pedirá confirmação de idade e termos de novo.", "Redefinir", "Cancelar");
        if (!ok) return;
        ComplianceService.Reset();
        await DisplayAlertAsync("OK", "Aceite redefinido. Feche e abra o app.", "OK");
    }
}
