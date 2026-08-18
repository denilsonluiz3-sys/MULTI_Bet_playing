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

### CI rápido e automação de merge

- `.github/workflows/ci-merge-queue.yml` executa o check rápido `fast-validation` em `pull_request` e também está preparado para o evento `merge_group` caso a Merge Queue esteja disponível no contexto do repositório.
- O único status check obrigatório do ruleset atual é `fast-validation`.
- `.github/workflows/build-android.yml` continua responsável pelo build/publicação Android e não é colocado no caminho rápido.
- `.github/workflows/auto-update-pr.yml` tenta atualizar automaticamente PRs internos quando `main` recebe um novo commit.
- `.github/workflows/auto-merge-queue.yml` habilita auto-merge para PRs internos não-draft quando os requisitos do repositório forem satisfeitos.
- `.github/scripts/setup-branch-protection.sh` configura apenas Pull Request, o check `fast-validation` e a exclusão automática de branches após merge.

> **Nota sobre Merge Queue:** a API do GitHub rejeitou a regra `merge_queue` neste repositório (`422: Invalid rule 'merge_queue'`). Portanto, ela não é configurada artificialmente. O workflow mantém o suporte ao evento `merge_group` para compatibilidade futura, enquanto o fluxo atual usa auto-merge + CI rápido.

### Testes

Os testes rápidos ficam em `tests/MULTI_Bet_playing_Demo.Tests/` e atualmente cobrem a superfície de segurança do `UrlValidator`, incluindo schemes perigosos, endereços locais, HTTPS e normalização.

## CI

- Fast CI: `.github/workflows/ci-merge-queue.yml`
- Auto-update de PRs: `.github/workflows/auto-update-pr.yml`
- Auto-merge: `.github/workflows/auto-merge-queue.yml`
- Build Android: `.github/workflows/build-android.yml`

---

v1.1.0
