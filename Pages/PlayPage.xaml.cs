using MULTI_Bet_playing_Demo.Models;
using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class PlayPage : ContentPage
{
    private readonly CardService _cardService;
    private bool _isLeftFull;
    private bool _isRightFull;
    private bool _mutedLeft;
    private bool _mutedRight;
    private bool _mutedAll;
    private string? _urlLeft;
    private string? _urlRight;
    private string? _titleLeft;
    private string? _titleRight;

    private const string JsMute = "(function(){ try { document.querySelectorAll('video,audio').forEach(function(m){ m.muted = true; m.volume = 0; }); return 'ok'; } catch(e) { return 'err'; } })();";
    private const string JsUnmute = "(function(){ try { document.querySelectorAll('video,audio').forEach(function(m){ m.muted = false; m.volume = 1; }); return 'ok'; } catch(e) { return 'err'; } })();";

    public PlayPage() : this(ResolveCardService()) { }

    public PlayPage(CardService cardService)
    {
        InitializeComponent();
        _cardService = cardService;
    }

    private static CardService ResolveCardService() =>
        Application.Current?.Handler?.MauiContext?.Services?.GetService<CardService>()
        ?? new CardService();

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WebViewLeft.IsVisible = false;
        WebViewRight.IsVisible = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        WebViewLeft.IsVisible = true;
        WebViewRight.IsVisible = true;
        AppLog.Info("PlayPage.OnAppearing (opções: mute, tela cheia, escolher, trocar)");
    }

    private async void OnLoadFavorites(object? sender, EventArgs e)
    {
        var favorites = await _cardService.GetFavoritesAsync();
        AppLog.Info($"Play: favoritos={favorites.Count}");
        if (favorites.Count == 0)
        {
            await DisplayAlertAsync("Aviso", "Nenhum favorito. Na Início toque ⭐ Favorito no card.", "OK");
            return;
        }

        await SetSlotAsync(true, favorites[0].Title, favorites[0].Url, favorites[0].Id);
        if (favorites.Count > 1)
            await SetSlotAsync(false, favorites[1].Title, favorites[1].Url, favorites[1].Id);
    }

    private async void OnPickSlots(object? sender, EventArgs e)
    {
        var cards = await _cardService.GetCardsAsync();
        if (cards.Count == 0)
        {
            await DisplayAlertAsync("Vazio", "Adicione links na Início (ou Demo → Exemplos Pragmatic).", "OK");
            return;
        }
        await PickForSideAsync(true, cards);
        await PickForSideAsync(false, cards);
    }

    private async Task PickForSideAsync(bool isLeft, List<CardItem> cards)
    {
        var side = isLeft ? "Tela 1 (esquerda)" : "Tela 2 (direita)";
        var names = cards.Select(c => c.Title).ToList();
        names.Insert(0, "(vazio)");
        var choice = await DisplayActionSheetAsync(side, "Pular", null, names.ToArray());
        if (string.IsNullOrEmpty(choice) || choice == "Pular") return;
        if (choice == "(vazio)") { ClearSide(isLeft); return; }
        var card = cards.FirstOrDefault(c => c.Title == choice);
        if (card != null)
            await SetSlotAsync(isLeft, card.Title, card.Url, card.Id);
    }

    private async Task SetSlotAsync(bool isLeft, string title, string rawUrl, string? cardId)
    {
        if (!UrlValidator.TryNormalize(rawUrl, out var url, out var err))
        {
            await DisplayAlertAsync("URL inválida", err, "OK");
            return;
        }

        if (isLeft)
        {
            _urlLeft = url; _titleLeft = title; LabelLeft.Text = title; WebViewLeft.Source = url;
            _mutedLeft = false; MuteLeftBtn.Text = "🔇";
        }
        else
        {
            _urlRight = url; _titleRight = title; LabelRight.Text = title; WebViewRight.Source = url;
            _mutedRight = false; MuteRightBtn.Text = "🔇";
        }

        if (!string.IsNullOrEmpty(cardId))
            await _cardService.MarkUsedAsync(cardId);
        AppLog.Info($"Play slot {(isLeft ? "E" : "D")}: {title}");
    }

    private void ClearSide(bool isLeft)
    {
        if (isLeft)
        {
            WebViewLeft.Source = null; LabelLeft.Text = "Tela 1";
            _urlLeft = _titleLeft = null; _mutedLeft = false; MuteLeftBtn.Text = "🔇";
        }
        else
        {
            WebViewRight.Source = null; LabelRight.Text = "Tela 2";
            _urlRight = _titleRight = null; _mutedRight = false; MuteRightBtn.Text = "🔇";
        }
    }

    private void OnSwap(object? sender, EventArgs e)
    {
        (_urlLeft, _urlRight) = (_urlRight, _urlLeft);
        (_titleLeft, _titleRight) = (_titleRight, _titleLeft);
        (_mutedLeft, _mutedRight) = (_mutedRight, _mutedLeft);
        LabelLeft.Text = _titleLeft ?? "Tela 1";
        LabelRight.Text = _titleRight ?? "Tela 2";
        WebViewLeft.Source = string.IsNullOrEmpty(_urlLeft) ? null : _urlLeft;
        WebViewRight.Source = string.IsNullOrEmpty(_urlRight) ? null : _urlRight;
        MuteLeftBtn.Text = _mutedLeft ? "🔊" : "🔇";
        MuteRightBtn.Text = _mutedRight ? "🔊" : "🔇";
        AppLog.Info("Play: trocar lados");
    }

    private async void OnMuteAll(object? sender, EventArgs e)
    {
        _mutedAll = !_mutedAll;
        _mutedLeft = _mutedAll;
        _mutedRight = _mutedAll;
        await ApplyMuteAsync(WebViewLeft, _mutedLeft);
        await ApplyMuteAsync(WebViewRight, _mutedRight);
        UpdateMuteButtons();
        AppLog.Info($"Play: mute all = {_mutedAll}");
    }

    private async void OnMuteLeft(object? sender, EventArgs e)
    {
        _mutedLeft = !_mutedLeft;
        await ApplyMuteAsync(WebViewLeft, _mutedLeft);
        UpdateMuteButtons();
    }

    private async void OnMuteRight(object? sender, EventArgs e)
    {
        _mutedRight = !_mutedRight;
        await ApplyMuteAsync(WebViewRight, _mutedRight);
        UpdateMuteButtons();
    }

    private void UpdateMuteButtons()
    {
        MuteLeftBtn.Text = _mutedLeft ? "🔊" : "🔇";
        MuteRightBtn.Text = _mutedRight ? "🔊" : "🔇";
        MuteAllBtn.Text = (_mutedLeft && _mutedRight) ? "🔊 Som" : "🔇 Mute";
        _mutedAll = _mutedLeft && _mutedRight;
    }

    private static async Task ApplyMuteAsync(WebView web, bool mute)
    {
        try { await web.EvaluateJavaScriptAsync(mute ? JsMute : JsUnmute); }
        catch (Exception ex) { AppLog.Exception("Play.ApplyMute", ex); }
    }

    private async void OnFullMenu(object? sender, EventArgs e)
    {
        var choice = await DisplayActionSheetAsync("Tela cheia", "Cancelar", null,
            "Só Tela 1 (esquerda)", "Só Tela 2 (direita)", "Restaurar as duas");
        switch (choice)
        {
            case "Só Tela 1 (esquerda)": ExpandLeft(); break;
            case "Só Tela 2 (direita)": ExpandRight(); break;
            case "Restaurar as duas": ResetFullScreen(); break;
        }
    }

    private void OnExpandLeft(object? sender, EventArgs e)
    {
        if (_isLeftFull) ResetFullScreen(); else ExpandLeft();
    }

    private void OnExpandRight(object? sender, EventArgs e)
    {
        if (_isRightFull) ResetFullScreen(); else ExpandRight();
    }

    private void ExpandLeft()
    {
        BorderRight.IsVisible = false;
        Grid.SetColumnSpan(BorderLeft, 2);
        _isLeftFull = true; _isRightFull = false;
        FullMenuBtn.Text = "⛶ Restaurar";
    }

    private void ExpandRight()
    {
        BorderLeft.IsVisible = false;
        Grid.SetColumn(BorderRight, 0);
        Grid.SetColumnSpan(BorderRight, 2);
        _isRightFull = true; _isLeftFull = false;
        FullMenuBtn.Text = "⛶ Restaurar";
    }

    private void ResetFullScreen()
    {
        BorderLeft.IsVisible = true;
        BorderRight.IsVisible = true;
        Grid.SetColumn(BorderLeft, 0); Grid.SetColumnSpan(BorderLeft, 1);
        Grid.SetColumn(BorderRight, 1); Grid.SetColumnSpan(BorderRight, 1);
        _isLeftFull = false; _isRightFull = false;
        FullMenuBtn.Text = "⛶ Tela cheia";
    }

    private async void OnReloadAll(object? sender, EventArgs e)
    {
        await ReloadSideAsync(true);
        await ReloadSideAsync(false);
    }

    private async void OnReloadLeft(object? sender, EventArgs e) => await ReloadSideAsync(true);
    private async void OnReloadRight(object? sender, EventArgs e) => await ReloadSideAsync(false);

    private async Task ReloadSideAsync(bool isLeft)
    {
        var url = isLeft ? _urlLeft : _urlRight;
        var web = isLeft ? WebViewLeft : WebViewRight;
        if (string.IsNullOrEmpty(url)) return;
        web.Source = null;
        await Task.Delay(50);
        web.Source = url;
        if (isLeft && _mutedLeft) await ApplyMuteAsync(web, true);
        if (!isLeft && _mutedRight) await ApplyMuteAsync(web, true);
        AppLog.Info($"Play reload {(isLeft ? "E" : "D")}");
    }

    private async void OnMenuLeft(object? sender, EventArgs e) => await SideMenuAsync(true);
    private async void OnMenuRight(object? sender, EventArgs e) => await SideMenuAsync(false);

    private async Task SideMenuAsync(bool isLeft)
    {
        var side = isLeft ? "Tela 1" : "Tela 2";
        var muted = isLeft ? _mutedLeft : _mutedRight;
        var choice = await DisplayActionSheetAsync(side, "Cancelar", null,
            muted ? "Ativar som" : "Silenciar",
            "Tela cheia", "Recarregar", "Escolher link",
            "Abrir em tela única", "Limpar este lado");

        switch (choice)
        {
            case "Silenciar":
            case "Ativar som":
                if (isLeft) OnMuteLeft(null, EventArgs.Empty);
                else OnMuteRight(null, EventArgs.Empty);
                break;
            case "Tela cheia":
                if (isLeft) ExpandLeft(); else ExpandRight();
                break;
            case "Recarregar":
                await ReloadSideAsync(isLeft);
                break;
            case "Escolher link":
                var cards = await _cardService.GetCardsAsync();
                if (cards.Count == 0) { await DisplayAlertAsync("Vazio", "Nenhum link salvo.", "OK"); break; }
                await PickForSideAsync(isLeft, cards);
                break;
            case "Abrir em tela única":
                var u = isLeft ? _urlLeft : _urlRight;
                var t = isLeft ? _titleLeft : _titleRight;
                if (!string.IsNullOrEmpty(u))
                    await Navigation.PushAsync(new WebViewPage(u, t ?? side));
                break;
            case "Limpar este lado":
                ClearSide(isLeft);
                break;
        }
    }

    private void OnClearAll(object? sender, EventArgs e)
    {
        ClearSide(true);
        ClearSide(false);
        ResetFullScreen();
        AppLog.Info("Play: limpar tudo");
    }
}
