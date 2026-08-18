# MULTI Bet Playing Demo

App Android independente (.NET MAUI) para jogar em **múltiplos cassinos online** ao mesmo tempo.

> Projeto separado do AURA — reutiliza padrões MAUI/Shell, mas é um app próprio.

## Funcionalidades

| Aba | Função |
|-----|--------|
| 🏠 **Início** | Cards de cassinos (adicionar, favoritar, editar, remover, filtrar) |
| 🧪 **Demo** | 4 WebViews em grid 2×2 |
| 🎮 **Play** | 2 WebViews empilhados + tela cheia |
| ⚙️ **Config** | Temas Dark / Light / Casino |

## Stack

- .NET MAUI (`net8.0-android`)
- Shell (TabBar + Flyout)
- WebView
- Persistência local JSON (`cards.json`)
- Temas via ResourceDictionary

## Como rodar

```bash
dotnet workload install maui-android
cd MULTI_Bet_playing_Demo
dotnet restore
dotnet build -f net8.0-android
dotnet build -t:Run -f net8.0-android
```

v1.0.0 — App independente 🎰
