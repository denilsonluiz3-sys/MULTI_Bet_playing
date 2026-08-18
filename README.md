# MULTI Bet Playing Demo

App Android independente (.NET MAUI) que abre **links de sites de terceiros** em múltiplos WebViews (2 ou 4 telas).

> **Não somos casa de apostas.** Não processamos jogos, odds nem pagamentos.
> Proibido para menores de 18 anos. Verifique a legalidade na sua região.
> Jogue com responsabilidade.

## Funcionalidades

| Aba | Função |
|-----|--------|
| **Início** | Lista de links (você adiciona); favoritos; validação de URL |
| **Demo** | Até 4 WebViews (2×2) |
| **Play** | 2 WebViews + tela cheia |
| **Config** | Temas, limpar cookies, import/export JSON, aviso legal |
| **Verificar** | Trust & Security Engine: pesquisa pública, relação de identidade e alerta de possível imitação |

## Segurança e compliance (v1.1)

- Age gate + aceite de termos no primeiro uso
- Validação de URL (só http/https; bloqueio de schemes perigosos)
- `allowBackup=false`, cleartext desabilitado, network security config
- Handler WebView: sem file access
- Limpar cookies/cache nas Configurações
- Rodapé e textos legais
- Sem lista pré-carregada de casas
- Verificação regulatória sob demanda em fonte pública oficial
- Resultado explicável: verificado, atenção, não verificado, possível imitação ou fonte indisponível
- Nenhuma credencial, dado bancário ou pagamento é tratado pelo Trust & Security Engine

## Fluxo de Desenvolvimento Autônomo

O fluxo do repositório é orientado a desenvolvimento contínuo:

```text
ANALISAR
    ↓
PESQUISAR E ENTENDER
    ↓
REAPROVEITAR O AURA quando já existir solução funcional
ou quando a adaptação for pequena e compatível
    ↓
IMPLEMENTAR
    ↓
ACTIONS
    ↓
GREEN = validado pelo CI
```

### CI rápido e Merge Queue

- `.github/workflows/ci-merge-queue.yml` executa os testes rápidos em `pull_request` e `merge_group`.
- O job `fast` é o check destinado a ser obrigatório para a fila.
- `.github/workflows/build-android.yml` continua responsável pelo build/publicação Android e não é colocado no caminho rápido da fila.
- `.github/workflows/auto-update-pr.yml` tenta atualizar automaticamente PRs internos quando `main` recebe um novo commit.
- `.github/scripts/setup-branch-protection.sh` configura a proteção/ruleset de `main`, incluindo Merge Queue e somente o check rápido como obrigatório.

O GitHub Merge Queue executa os checks no SHA temporário do grupo de merge; o workflow usa `github.event.merge_group.head_sha` explicitamente. O build Android pesado permanece paralelo/pós-merge para não transformar o fluxo cotidiano em uma espera desnecessária.

### Testes

Os testes rápidos ficam em `tests/MULTI_Bet_playing_Demo.Tests/` e atualmente cobrem a superfície de segurança do `UrlValidator`, incluindo schemes perigosos, endereços locais, HTTPS e normalização.

## CI

- Fast CI / Merge Queue: `.github/workflows/ci-merge-queue.yml`
- Auto-update de PRs: `.github/workflows/auto-update-pr.yml`
- Build Android: `.github/workflows/build-android.yml`

---

v1.1.0
