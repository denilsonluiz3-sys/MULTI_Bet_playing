# MULTI Bet Playing Demo

App Android independente (.NET MAUI) que abre **links de sites de terceiros** em m\u00faltiplos WebViews (2 ou 4 telas).

> **N\u00e3o somos casa de apostas.** N\u00e3o processamos jogos, odds nem pagamentos.  
> Proibido para menores de 18 anos. Verifique a legalidade na sua regi\u00e3o.  
> Jogue com responsabilidade.

## Funcionalidades

| Aba | Fun\u00e7\u00e3o |
|-----|--------|
| **In\u00edcio** | Lista de links (voc\u00ea adiciona); favoritos; valida\u00e7\u00e3o de URL |
| **Demo** | At\u00e9 4 WebViews (2\u00d72) |
| **Play** | 2 WebViews + tela cheia |
| **Config** | Temas, limpar cookies, import/export JSON, aviso legal |

## Seguran\u00e7a e compliance (v1.1)

- Age gate + aceite de termos no primeiro uso  
- Valida\u00e7\u00e3o de URL (s\u00f3 http/https; bloqueio de schemes perigosos)  
- `allowBackup=false`, cleartext desabilitado, network security config  
- Handler WebView: sem file access  
- Limpar cookies/cache nas Configura\u00e7\u00f5es  
- Rodap\u00e9 e textos legais  
- Sem lista pr\u00e9-carregada de casas  

## CI

Workflow: `.github/workflows/build-android.yml`

---

v1.1.0
