namespace MULTI_Bet_playing_Demo.Services;

public static class ComplianceService
{
    private const string KeyAccepted = "ComplianceAccepted";
    private const string KeyAcceptedAt = "ComplianceAcceptedAt";
    private const string KeyAgeConfirmed = "AgeConfirmed18";

    public const int MinimumAge = 18;

    public static bool HasAccepted =>
        Preferences.Get(KeyAccepted, false) && Preferences.Get(KeyAgeConfirmed, false);

    public static void Accept()
    {
        Preferences.Set(KeyAgeConfirmed, true);
        Preferences.Set(KeyAccepted, true);
        Preferences.Set(KeyAcceptedAt, DateTime.UtcNow.ToString("O"));
    }

    public static void Reset()
    {
        Preferences.Remove(KeyAccepted);
        Preferences.Remove(KeyAcceptedAt);
        Preferences.Remove(KeyAgeConfirmed);
    }

    public static string DisclaimerShort =>
        "Este app apenas abre links de sites de terceiros em WebViews. " +
        "N\u00e3o operamos jogos, n\u00e3o processamos apostas nem pagamentos. " +
        "Verifique se o jogo online \u00e9 legal na sua regi\u00e3o. Proibido para menores de 18 anos.";

    public static string DisclaimerFull =>
        "AVISO LEGAL\n\n" +
        "\u2022 MULTI Bet \u00e9 um organizador de links / navegador multi-aba.\n" +
        "\u2022 N\u00e3o somos casa de apostas, n\u00e3o oferecemos odds e n\u00e3o intermediamos valores.\n" +
        "\u2022 Os sites abertos s\u00e3o de responsabilidade exclusiva de seus operadores.\n" +
        "\u2022 Jogo envolve risco de perda financeira. Jogue com responsabilidade.\n" +
        "\u2022 Proibido para menores de 18 anos (ou idade legal da sua jurisdi\u00e7\u00e3o).\n" +
        "\u2022 Voc\u00ea deve confirmar que o acesso a esses sites \u00e9 permitido onde voc\u00ea est\u00e1.\n" +
        "\u2022 N\u00e3o use em dispositivo compartilhado com contas reais de apostas.\n" +
        "\u2022 N\u00e3o automatizamos apostas nem contornamos geobloqueios.\n\n" +
        "Ao continuar, voc\u00ea declara ter 18+ anos e aceitar estes termos.";
}
