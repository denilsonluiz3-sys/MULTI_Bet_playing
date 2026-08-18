using MULTI_Bet_playing_Demo.Models;
using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class PlayPage : ContentPage
{
    private readonly CardService _cardService;
    private bool _leftFull;
    private bool _rightFull;
    private bool _muted;
    private string? _urlL, _urlR, _titleL, _titleR;

    private const string JsMute = "(function(){try{document.querySelectorAll('video,audio').forEach(function(m){m.muted=true;m.volume=0;});}catch(e){}})();";
    private const string JsUnmute = "(function(){try{document.querySelectorAll('video,audio').forEach(function(m){m.muted=false;m.volume=1;});}catch(e){}})();";

    public PlayPage() : this(Resolve()) { }
    public PlayPage(CardService cardService) { InitializeComponent(); _cardService = cardService; }
    private static CardService Resolve() => Application.Current?.Handler?.MauiContext?.Services?.GetService<CardService>() ?? new CardService();

    protected override void OnAppearing() { base.OnAppearing(); try { WebViewLeft.IsVisible = true; WebViewRight.IsVisible = true; } catch { } }
    protected override void OnDisappearing() { base.OnDisappearing(); try { WebViewLeft.IsVisible = false; WebViewRight.IsVisible = false; } catch { } }

    private async void OnLoadFavorites(object? sender, EventArgs e)
    {
        try {
            var fav = await _cardService.GetFavoritesAsync();
            if (fav.Count == 0) { await DisplayAlertAsync("Aviso", "Nenhum favorito. Marque ⭐ na Início.", "OK"); return; }
            await SetAsync(true, fav[0]);
            if (fav.Count > 1) await SetAsync(false, fav[1]);
        } catch (Exception ex) { await SafeAlert("Erro", ex.Message); }
    }

    private async void OnPickSlots(object? sender, EventArgs e)
    {
        try {
            var cards = await _cardService.GetCardsAsync();
            if (cards.Count == 0) { await DisplayAlertAsync("Vazio", "Adicione links na Início.", "OK"); return; }
            await PickAsync(true, cards); await PickAsync(false, cards);
        } catch (Exception ex) { await SafeAlert("Erro", ex.Message); }
    }

    private async Task PickAsync(bool left, List<CardItem> cards)
    {
        var names = cards.Select(c => c.Title).ToList(); names.Insert(0, "(vazio)");
        var choice = await DisplayActionSheetAsync(left ? "Tela 1" : "Tela 2", "Pular", null, names.ToArray());
        if (string.IsNullOrEmpty(choice) || choice == "Pular") return;
        if (choice == "(vazio)") { ClearSide(left); return; }
        var card = cards.FirstOrDefault(c => c.Title == choice);
        if (card != null) await SetAsync(left, card);
    }

    private async Task SetAsync(bool left, CardItem card)
    {
        if (!UrlValidator.TryNormalize(card.Url, out var url, out var err)) { await SafeAlert("URL inválida", err); return; }
        if (left) { _urlL = url; _titleL = card.Title; LabelLeft.Text = card.Title; WebViewLeft.Source = url; }
        else { _urlR = url; _titleR = card.Title; LabelRight.Text = card.Title; WebViewRight.Source = url; }
        await _cardService.MarkUsedAsync(card.Id);
    }

    private void ClearSide(bool left)
    {
        if (left) { WebViewLeft.Source = null; LabelLeft.Text = "Tela 1"; _urlL = _titleL = null; }
        else { WebViewRight.Source = null; LabelRight.Text = "Tela 2"; _urlR = _titleR = null; }
    }

    private void OnSwap(object? sender, EventArgs e)
    {
        try {
            (_urlL, _urlR) = (_urlR, _urlL); (_titleL, _titleR) = (_titleR, _titleL);
            LabelLeft.Text = _titleL ?? "Tela 1"; LabelRight.Text = _titleR ?? "Tela 2";
            WebViewLeft.Source = _urlL; WebViewRight.Source = _urlR;
        } catch (Exception ex) { _ = SafeAlert("Erro", ex.Message); }
    }

    private async void OnMuteAll(object? sender, EventArgs e)
    {
        try {
            _muted = !_muted; MuteAllBtn.Text = _muted ? "🔊 Som" : "🔇 Mute";
            await TryJs(WebViewLeft, _muted ? JsMute : JsUnmute);
            await TryJs(WebViewRight, _muted ? JsMute : JsUnmute);
        } catch { }
    }

    private static async Task TryJs(WebView web, string js) { try { if (web.Source != null) await web.EvaluateJavaScriptAsync(js); } catch { } }

    private void OnExpandLeft(object? sender, EventArgs e)
    {
        try {
            if (_leftFull) { OnRestoreBoth(sender, e); return; }
            BorderRight.IsVisible = false; Grid.SetColumn(BorderLeft, 0); Grid.SetColumnSpan(BorderLeft, 2);
            _leftFull = true; _rightFull = false;
        } catch { }
    }

    private void OnExpandRight(object? sender, EventArgs e)
    {
        try {
            if (_rightFull) { OnRestoreBoth(sender, e); return; }
            BorderLeft.IsVisible = false; Grid.SetColumn(BorderRight, 0); Grid.SetColumnSpan(BorderRight, 2);
            _rightFull = true; _leftFull = false;
        } catch { }
    }

    private void OnRestoreBoth(object? sender, EventArgs e)
    {
        try {
            BorderLeft.IsVisible = true; BorderRight.IsVisible = true;
            Grid.SetColumn(BorderLeft, 0); Grid.SetColumnSpan(BorderLeft, 1);
            Grid.SetColumn(BorderRight, 1); Grid.SetColumnSpan(BorderRight, 1);
            _leftFull = false; _rightFull = false;
        } catch { }
    }

    private async void OnReloadAll(object? sender, EventArgs e)
    {
        try {
            await Reload(_urlL, WebViewLeft); await Reload(_urlR, WebViewRight);
            if (_muted) { await TryJs(WebViewLeft, JsMute); await TryJs(WebViewRight, JsMute); }
        } catch { }
    }

    private static async Task Reload(string? url, WebView web)
    {
        if (string.IsNullOrEmpty(url)) return; web.Source = null; await Task.Delay(30); web.Source = url;
    }

    private void OnClearAll(object? sender, EventArgs e)
    {
        try { ClearSide(true); ClearSide(false); OnRestoreBoth(sender, e); _muted = false; MuteAllBtn.Text = "🔇 Mute"; } catch { }
    }

    private async Task SafeAlert(string title, string msg) { try { await DisplayAlertAsync(title, msg, "OK"); } catch { } }
}
