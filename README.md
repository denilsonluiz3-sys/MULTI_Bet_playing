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

## Fluxo de Desenvolvimento

```text
ANALISAR
    ↓
PESQUISAR E ENTENDER
    ↓
REAPROVEITAR/ADAPTAR O AURA quando houver solução funcional compatível
    ↓
IMPLEMENTAR
    ↓
ACTIONS
    ↓
GREEN = validado pelo CI
```

## CI/CD

- `.github/workflows/ci-merge-queue.yml`: validação rápida, com `pull_request` e suporte ao evento `merge_group`; o checkout usa `github.event.merge_group.head_sha` quando esse evento ocorrer.
- O check rápido publicado é `fast-validation`.
- `.github/workflows/project-validation.yml`: validação mais ampla do projeto, incluindo estrutura, workload MAUI Android, build Android Debug, testes e verificação de segredos.
- `.github/workflows/build-android.yml`: build e publicação do APK Android.
- `.github/workflows/auto-update-pr.yml`: atualiza automaticamente PRs internos quando `main` recebe commits.
- `.github/workflows/auto-merge-queue.yml`: solicita auto-merge para PRs internos elegíveis quando os requisitos do repositório estiverem satisfeitos.
- `.github/scripts/setup-branch-protection.sh`: configura o ruleset atual da `main` com Pull Request e o check `fast-validation`; também habilita a exclusão automática de branches após merge.

### Merge Queue

O workflow possui suporte ao evento `merge_group` para compatibilidade com ambientes em que a Merge Queue esteja disponível. A regra `merge_queue` **não está configurada neste repositório**, pois a API rejeitou essa regra para o contexto atual da conta/repositório.

### Testes

Os testes ficam em `tests/MULTI_Bet_playing_Demo.Tests/` e atualmente cobrem a superfície de segurança do `UrlValidator`, incluindo schemes perigosos, endereços locais, HTTPS e normalização.

## CI

- Fast CI: `.github/workflows/ci-merge-queue.yml`
- Project Validation: `.github/workflows/project-validation.yml`
- Auto-update de PRs: `.github/workflows/auto-update-pr.yml`
- Auto-merge: `.github/workflows/auto-merge-queue.yml`
- Build Android: `.github/workflows/build-android.yml`

---

v1.1.0
