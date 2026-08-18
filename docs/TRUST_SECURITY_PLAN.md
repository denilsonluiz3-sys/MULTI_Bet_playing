# MULTI_Bet — Trust & Security / Central

## Objetivo

Criar um único núcleo de confiança que una pesquisa, verificação de plataforma/domínio, análise anti-phishing e organização dos acessos do usuário.

## Escopo da primeira implementação

1. Entrada por nome, marca, empresa, CNPJ ou URL.
2. Normalização segura de URL.
3. Consulta sob demanda a fontes públicas oficiais.
4. Fonte regulatória inicial: Ministério da Fazenda / SPA.
5. Relação marca ↔ empresa ↔ CNPJ ↔ domínio ↔ portaria quando os dados públicos permitirem.
6. Análise técnica do domínio: HTTPS, host, punycode/IDN, redirecionamentos e similaridade.
7. Resultado explicável:
   - Verificado
   - Atenção
   - Não verificado
   - Possível imitação
8. Registro local somente do resultado mínimo e da data da consulta; nenhuma credencial ou dado bancário.
9. A organização continua local: favoritos, recentes e categorias.
10. Nenhum mecanismo de recomendação de aposta, odds, tipster, automação de aposta ou pagamento.

## Fonte regulatória

A SPA publica relação de empresas autorizadas contendo empresa, CNPJ, marcas, domínios e portaria. A relação é atualizada periodicamente. O mecanismo deve guardar a fonte e a data da consulta e nunca tratar uma cópia antiga como autorização atual.

Fonte oficial: https://www.gov.br/fazenda/pt-br/composicao/orgaos/secretaria-de-premios-e-apostas/lista-de-empresas

Também existe uma relação separada para agentes que operam por determinação judicial. Essa situação não deve ser confundida com autorização administrativa comum.

## Regras de segurança

- Não afirmar que um domínio é golpe apenas porque não foi encontrado.
- Não confiar somente na extensão `.bet.br`.
- Não considerar similaridade como prova de fraude.
- Sempre apresentar a razão e a fonte do resultado.
- Toda verificação é sob demanda do usuário.
- Falha de rede não deve virar `Não verificado` sem indicar `Fonte indisponível`.
- O resultado regulatório deve ser separado da análise técnica do domínio.

## Organização

`CardItem` recebe categoria local opcional. A organização não altera a legitimidade da plataforma e não cria catálogo comercial automaticamente.

## Próximas fases

- testes automatizados do parser e do analisador de domínio;
- página de pesquisa integrada ao Shell;
- melhoria da organização por categoria;
- cache local controlado por data, sem transformar cache em fonte de autorização;
- revisão jurídica antes de produção.
