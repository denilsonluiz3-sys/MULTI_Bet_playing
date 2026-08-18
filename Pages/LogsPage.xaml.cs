using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo.Pages;

public partial class LogsPage : ContentPage
{
    public LogsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Reload();
    }

    private void Reload()
    {
        PathLabel.Text = string.IsNullOrEmpty(AppLog.LogFilePath)
            ? "Arquivo ainda não criado"
            : AppLog.LogFilePath;
        LogViewer.Text = AppLog.ReadRecentLog();
    }

    private void OnRefresh(object? sender, EventArgs e)
    {
        AppLog.Info("LogsPage: recarregar");
        Reload();
    }

    private async void OnCopy(object? sender, EventArgs e)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(LogViewer.Text ?? "");
            AppLog.Info("LogsPage: log copiado para clipboard");
            await DisplayAlertAsync("OK", "Log copiado.", "OK");
        }
        catch (Exception ex)
        {
            AppLog.Exception("LogsPage.OnCopy", ex);
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void OnShare(object? sender, EventArgs e)
    {
        try
        {
            var path = AppLog.LogFilePath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                var tmp = Path.Combine(FileSystem.CacheDirectory, "multibet-log.txt");
                await File.WriteAllTextAsync(tmp, LogViewer.Text ?? "");
                path = tmp;
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "MULTI Bet log",
                File = new ShareFile(path)
            });
            AppLog.Info("LogsPage: compartilhar log");
        }
        catch (Exception ex)
        {
            AppLog.Exception("LogsPage.OnShare", ex);
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void OnClear(object? sender, EventArgs e)
    {
        if (!await DisplayAlertAsync("Limpar", "Apagar conteúdo do log atual?", "Sim", "Não"))
            return;
        AppLog.ClearLogs();
        Reload();
    }
}
