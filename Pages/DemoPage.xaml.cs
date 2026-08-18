using MULTI_Bet_playing_Demo.Models;
using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class DemoPage : ContentPage
{
    private readonly CardService _cardService;
    private WebView[] _wvs = null!;
    private Label[] _labels = null!;
    private readonly string?[] _urls = new string?[4];
    private bool _muted;

    private static readonly (string Title, string Url)[] Pragmatic =
    {
        ("Joker's Jewels", "https://www.pragmaticplay.com/pt/jogos/jokers-jewels/?gamelang=pt&cur=ALL"),
        ("Big Bass Splash 1000", "https://www.pragmaticplay.com/pt/jogos/big-bass-splash-1000/?gamelang=pt&cur=EUR"),
        ("Sweet Bonanza", "https://www.pragmaticplay.com/pt/jogos/sweet-bonanza/?gamelang=pt&cur=ALL"),
        ("Gates of Olympus", "https://www.pragmaticplay.com/pt/jogos/gates-of-olympus/?gamelang=pt&cur=ALL"),
    };

    private const string JsMute = "(function(){try{document.querySelectorAll('video,audio').forEach(function(m){m.muted=true;m.volume=0;});}catch(e){}})();";
    private const string JsUnmute = "(function(){try{document.querySelectorAll('video,audio').forEach(function(m){m.muted=false;m.volume=1;});}catch(e){}})();";

    public DemoPage() : this(Resolve()) { }
    public DemoPage(CardService cardService)
    {
        InitializeComponent();
        _cardService = cardService;
        _wvs = new[] { WebView1, WebView2, WebView3, WebView4 };
        _labels = new[] { Label1, Label2, Label3, Label4 };
    }
    private static CardService Resolve() => Application.Current?.Handler?.MauiContext?.Services?.GetService<CardService>() ?? new CardService();

    protected override void OnAppearing() { base.OnAppearing(); try { foreach (var w in _wvs) w.IsVisible = true; } catch { } }
    protected override void OnDisappearing() { base.OnDisappearing(); try { foreach (var w in _wvs) w.IsVisible = false; } catch { } }

    private async void OnLoadFavorites(object? sender, EventArgs e)
    {
        try {
            var fav = await _cardService.GetFavoritesAsync();
            if (fav.Count == 0) { await DisplayAlertAsync("Aviso", "Nenhum favorito. Marque ⭐ na Início.", "OK"); return; }
            for (int i = 0; i < 4; i++) { if (i < fav.Count) await SetSlot(i, fav[i].Title, fav[i].Url, fav[i].Id); else ClearSlot(i); }
        } catch (Exception ex) { await SafeAlert(ex.Message); }
    }

    private async void OnLoadPragmatic(object? sender, EventArgs e)
    {
        try {
            for (int i = 0; i < 4 && i < Pragmatic.Length; i++) await SetSlot(i, Pragmatic[i].Title, Pragmatic[i].Url, null);
            var existing = await _cardService.GetCardsAsync();
            foreach (var (title, url) in Pragmatic) {
                if (existing.Any(c => string.Equals(c.Url, url, StringComparison.OrdinalIgnoreCase))) continue;
                if (!UrlValidator.TryNormalize(url, out var safe, out _)) continue;
                await _cardService.AddCardAsync(new CardItem { Title = title, Url = safe, Icon = "🎮", IsFavorite = false, LastUsed = DateTime.UtcNow });
            }
        } catch (Exception ex) { await SafeAlert(ex.Message); }
    }

    private async void OnPickSlots(object? sender, EventArgs e)
    {
        try {
            var cards = await _cardService.GetCardsAsync();
            if (cards.Count == 0) { await DisplayAlertAsync("Vazio", "Adicione links ou use Exemplos Pragmatic.", "OK"); return; }
            for (int slot = 0; slot < 4; slot++) {
                var names = cards.Select(c => c.Title).ToList(); names.Insert(0, "(vazio)");
                var choice = await DisplayActionSheetAsync($"Tela {slot + 1}", "Pular", null, names.ToArray());
                if (string.IsNullOrEmpty(choice) || choice == "Pular") continue;
                if (choice == "(vazio)") { ClearSlot(slot); continue; }
                var card = cards.FirstOrDefault(c => c.Title == choice);
                if (card != null) await SetSlot(slot, card.Title, card.Url, card.Id);
            }
        } catch (Exception ex) { await SafeAlert(ex.Message); }
    }

    private async Task SetSlot(int i, string title, string raw, string? id)
    {
        if (!UrlValidator.TryNormalize(raw, out var url, out _)) { ClearSlot(i); return; }
        _urls[i] = url; _labels[i].Text = title; _wvs[i].Source = url;
        if (!string.IsNullOrEmpty(id)) await _cardService.MarkUsedAsync(id);
    }

    private void ClearSlot(int i) { _urls[i] = null; _labels[i].Text = $"Tela {i + 1}"; _wvs[i].Source = null; }

    private async void OnMuteAll(object? sender, EventArgs e)
    {
        try {
            _muted = !_muted; MuteBtn.Text = _muted ? "🔊 Som" : "🔇 Mute";
            var js = _muted ? JsMute : JsUnmute;
            foreach (var w in _wvs) { try { if (w.Source != null) await w.EvaluateJavaScriptAsync(js); } catch { } }
        } catch { }
    }

    private async void OnReloadAll(object? sender, EventArgs e)
    {
        try {
            for (int i = 0; i < 4; i++) {
                if (string.IsNullOrEmpty(_urls[i])) continue;
                _wvs[i].Source = null; await Task.Delay(20); _wvs[i].Source = _urls[i];
            }
            if (_muted) foreach (var w in _wvs) { try { if (w.Source != null) await w.EvaluateJavaScriptAsync(JsMute); } catch { } }
        } catch { }
    }

    private void OnClearAll(object? sender, EventArgs e)
    {
        try { for (int i = 0; i < 4; i++) ClearSlot(i); _muted = false; MuteBtn.Text = "🔇 Mute"; } catch { }
    }

    private async Task SafeAlert(string msg) { try { await DisplayAlertAsync("Erro", msg, "OK"); } catch { } }
}
