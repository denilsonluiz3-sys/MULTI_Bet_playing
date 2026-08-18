# MULTI Bet — Arquitetura

## Princípios

- O aplicativo MAUI nunca acessa o gateway PIX diretamente.
- Credenciais e certificados do gateway ficam exclusivamente no backend.
- Saldo é derivado de transações financeiras persistidas; não usar incremento cego de saldo.
- Um `PixTxId` confirmado só pode gerar um crédito uma vez.
- IA e sistema de logs não fazem parte desta arquitetura.

## Camadas

```text
MULTI_Bet.Maui
      |
      | HTTPS
      v
MULTI_Bet.Api
      |
      +-- Authentication
      +-- Wallet
      +-- PIX
      +-- Webhook
      |
      v
MULTI_Bet.Infrastructure
      |
      +-- Database
      +-- Wallet ledger
      +-- Transactions
```

A integração PIX real será adicionada somente depois que o contrato da carteira e o fluxo mock estiverem validados.
