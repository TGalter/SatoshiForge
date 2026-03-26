# SatoshiForge

SatoshiForge é um marketplace **Bitcoin-only**, projetado para operar com pagamentos em **Bitcoin on-chain** e **Lightning Network**, com arquitetura preparada para crescimento, observabilidade e evolução incremental por feature.

## Objetivo

Construir uma plataforma de marketplace com foco em boas práticas de engenharia, arquitetura escalável e integração nativa com a stack Bitcoin.

O projeto terá:

- área do comprador
- área do seller
- área administrativa
- catálogo de produtos
- busca
- carrinho
- pedidos
- checkout
- pagamentos em Bitcoin
- observabilidade
- arquitetura preparada para crescer com boas práticas

---

## Visão arquitetural

```text
                                   ┌──────────────────────────────┐
                                   │           USUÁRIOS           │
                                   │ Buyers / Sellers / Admins    │
                                   └──────────────┬───────────────┘
                                                  │
                                ┌─────────────────┴─────────────────┐
                                │        FRONTENDS / CLIENTS        │
                                │  Angular Storefront               │
                                │  Angular Seller Portal            │
                                │  Angular Admin Portal             │
                                └─────────────────┬─────────────────┘
                                                  │ HTTPS / WSS
                                      ┌───────────┴───────────┐
                                      │     API GATEWAY       │
                                      │  Auth, Rate Limit,    │
                                      │  Routing, BFF         │
                                      └───────────┬───────────┘
                                                  │
                 ┌────────────────────────────────┼────────────────────────────────┐
                 │                                │                                │
                 │                                │                                │
      ┌──────────┴──────────┐          ┌──────────┴──────────┐          ┌──────────┴──────────┐
      │  IDENTITY / ACCESS  │          │   MARKETPLACE CORE  │          │    ADMIN / RISK     │
      │ users, roles, MFA   │          │ catalog, cart,      │          │ disputes, review,   │
      │ JWT / OIDC / RBAC   │          │ orders, checkout    │          │ fraud rules, audit  │
      └──────────┬──────────┘          └──────────┬──────────┘          └──────────┬──────────┘
                 │                                │                                │
                 └────────────────────────────────┼────────────────────────────────┘
                                                  │
                                    ┌─────────────┴─────────────┐
                                    │    PAYMENT ORCHESTRATOR   │
                                    │ quotes, invoices,         │
                                    │ payment state machine,    │
                                    │ idempotency, reconciliation│
                                    └───────┬─────────┬──────────┘
                                            │         │
                                            │         │
                         ┌──────────────────┘         └──────────────────┐
                         │                                               │
              ┌──────────┴──────────┐                        ┌───────────┴───────────┐
              │ ON-CHAIN ADAPTER    │                        │ LIGHTNING ADAPTER      │
              │ address mgmt,       │                        │ invoice mgmt, settle,  │
              │ mempool, confs,     │                        │ expiry, routing state  │
              │ UTXO tracking       │                        │                        │
              └──────────┬──────────┘                        └───────────┬───────────┘
                         │                                               │
                ┌────────┴────────┐                            ┌─────────┴─────────┐
                │  BITCOIN CORE   │◄──── blockchain ───────►   │       LND         │
                │ full node       │                            │ own LN node        │
                │ wallet/zmq/rpc  │                            │ channels/macaroons │
                └────────┬────────┘                            └─────────┬─────────┘
                         │                                               │
                         └──────────────────────┬────────────────────────┘
                                                │
                                   ┌────────────┴────────────┐
                                   │     TREASURY / LEDGER   │
                                   │ double-entry ledger,    │
                                   │ balances, fees, split   │
                                   │ seller settlement       │
                                   └────────────┬────────────┘
                                                │
       ┌──────────────────────────────┬─────────┼─────────┬───────────────────────────────┐
       │                              │         │         │                               │
┌──────┴──────┐              ┌────────┴──────┐  │  ┌──────┴────────┐             ┌────────┴───────┐
│ PostgreSQL  │              │   RabbitMQ    │  │  │    Redis      │             │   OpenSearch   │
│ source of   │              │ async jobs /  │  │  │ cache, locks, │             │ product search │
│ truth       │              │ domain events │  │  │ idempotency   │             │ autocomplete   │
└──────┬──────┘              └────────┬──────┘  │  └──────┬────────┘             └────────┬───────┘
       │                               │         │         │                               │
       │                               │         │         │                               │
       │                 ┌─────────────┴─────────┴─────────┴─────────────┐                 │
       │                 │           BACKGROUND WORKERS (.NET)            │                 │
       │                 │ indexing, notifications, webhook handling,     │                 │
       │                 │ reconciliation, channel/liquidity jobs,        │                 │
       │                 │ settlement, exports                            │                 │
       │                 └─────────────┬──────────────────────────────────┘                 │
       │                               │                                                    │
       └───────────────────────────────┼────────────────────────────────────────────────────┘
                                       │
                          ┌────────────┴─────────────┐
                          │   OBSERVABILITY STACK    │
                          │ OpenTelemetry Collector  │
                          │ Prometheus / Grafana     │
                          │ Loki / Tempo             │
                          └──────────────────────────┘
```

---

## Princípios de arquitetura

- **Bitcoin-only** como premissa de domínio
- **PostgreSQL como source of truth**
- **OpenSearch** para busca e indexação
- **Redis** para cache, locks e dados efêmeros
- **RabbitMQ** para filas, jobs assíncronos e eventos
- **Bitcoin Core** para pagamentos on-chain
- **LND** como nó próprio para Lightning Network
- **Observabilidade desde a base** com OpenTelemetry, Serilog e Health Checks
- **Desenvolvimento incremental, feature por feature, ponta a ponta**

---

## Stack tecnológica

### Backend
- ASP.NET Core / .NET
- Entity Framework Core
- Npgsql para PostgreSQL

### Frontend
- Angular
- SCSS

### Dados e infraestrutura
- PostgreSQL
- Redis
- RabbitMQ
- OpenSearch

### Pagamentos Bitcoin
- Bitcoin Core
- LND

### Observabilidade
- OpenTelemetry
- Serilog
- Health Checks

### Infra local
- Docker Compose

### Versionamento
- Git

---

## Estrutura esperada do repositório

```text
SatoshiForge/
  backend/
    SatoshiForge.sln
    src/
      SatoshiForge.Api/
      SatoshiForge.Application/
      SatoshiForge.Domain/
      SatoshiForge.Infrastructure/
      SatoshiForge.Shared/
    tests/
      SatoshiForge.UnitTests/
      SatoshiForge.IntegrationTests/
  frontend/
    satoshiforge-frontend/
      projects/
        storefront/
        admin/
  infra/
    docker/
  scripts/
  docs/
  docker-compose.yml
  .env
  .gitignore
  .editorconfig
  LICENSE
  README.md
```

---

## Estrutura lógica do backend

### `SatoshiForge.Api`
Camada de entrada da aplicação.

Responsável por:
- endpoints HTTP
- configuração
- health checks
- OpenTelemetry
- logging
- startup da aplicação

### `SatoshiForge.Application`
Camada de casos de uso.

Responsável por:
- serviços de aplicação
- DTOs
- comandos e queries
- regras de orquestração
- contratos entre camadas

### `SatoshiForge.Domain`
Camada de domínio.

Responsável por:
- entidades
- enums
- value objects
- regras de negócio centrais

### `SatoshiForge.Infrastructure`
Camada de infraestrutura.

Responsável por:
- persistência
- `AppDbContext`
- configurações do EF Core
- migrations
- integrações com recursos externos

### `SatoshiForge.Shared`
Camada compartilhada.

Responsável por:
- contratos comuns
- abstrações compartilhadas
- helpers cross-cutting

### Testes
- `SatoshiForge.UnitTests`: testes unitários do backend
- `SatoshiForge.IntegrationTests`: testes de integração do backend

---

## Estrutura lógica do frontend

### `storefront`
Aplicação Angular para o comprador.

Responsável por:
- catálogo
- busca
- carrinho
- checkout
- autenticação do comprador
- histórico de pedidos

### `admin`
Aplicação Angular para operação interna e seller portal inicial.

Responsável por:
- login administrativo
- gestão de produtos
- gestão de pedidos
- gestão de usuários e sellers
- visão operacional

---

## Componentes principais

### Frontends / Clients
- Angular Storefront
- Angular Seller Portal
- Angular Admin Portal

### Backend
- API principal em ASP.NET Core
- Application layer
- Domain layer
- Infrastructure layer
- Background workers em .NET

### Infra de dados
- PostgreSQL
- Redis
- RabbitMQ
- OpenSearch

### Infra Bitcoin
- Bitcoin Core
- LND próprio

### Recursos transversais
- health checks
- logs estruturados
- traces
- métricas
- configuração por ambiente
- migrations
- testes
- ambiente Docker local

---

## Módulos de negócio previstos

### Identity / Access
- usuários
- roles
- MFA
- JWT / OIDC / RBAC

### Marketplace Core
- catálogo
- busca
- carrinho
- pedidos
- checkout

### Admin / Risk
- disputas
- revisão
- regras antifraude
- trilha de auditoria

### Payment Orchestrator
- quotes
- invoices
- máquina de estados de pagamento
- idempotência
- reconciliação

### On-Chain Adapter
- gerenciamento de endereços
- mempool
- confirmações
- tracking de UTXO

### Lightning Adapter
- gerenciamento de invoices
- liquidação
- expiração
- estado de roteamento

### Treasury / Ledger
- double-entry ledger
- saldos
- taxas
- split
- settlement de sellers

### Background Workers
- indexação
- notificações
- webhooks
- reconciliação
- jobs de liquidez/canais
- settlement
- exports

---

## Funcionalidades previstas

### Comprador
- cadastro e login
- catálogo de produtos
- busca
- filtros
- carrinho
- checkout
- pagamento em BTC
- histórico de pedidos

### Seller
- cadastro e autenticação
- cadastro de produtos
- gestão de estoque
- visualização de pedidos
- saldo e recebimentos futuramente

### Admin
- gestão de usuários
- gestão de sellers
- monitoramento de pedidos
- monitoramento de pagamentos
- reconciliação
- visão operacional

### Pagamentos
- pagamento on-chain
- pagamento Lightning
- cotação BTC
- expiração de pagamento
- tracking de pagamento
- confirmação
- reconciliação

---

## Estratégia de desenvolvimento

O projeto será desenvolvido **uma funcionalidade por vez**, sempre de ponta a ponta.

Cada feature deve seguir esta ordem:

1. definir escopo
2. modelar domínio e banco
3. implementar backend
4. criar testes de backend
5. implementar observability do backend
6. implementar frontend
7. criar testes de frontend
8. implementar observability do frontend
9. validar ponta a ponta
10. commit final

### Checklist padrão por feature

- [ ] Escopo fechado
- [ ] Domínio modelado
- [ ] Banco/migration criada
- [ ] Backend implementado
- [ ] Testes backend criados
- [ ] Observability backend criada
- [ ] Frontend implementado
- [ ] Testes frontend criados
- [ ] Observability frontend criada
- [ ] Fluxo ponta a ponta validado
- [ ] Commit final feito

---

## Estado atual

### Já definido / planejado na fundação
- fundação do repositório
- estrutura de pastas
- solution .NET
- apps Angular
- docker-compose base
- health checks
- observability mínima
- configuração centralizada
- OpenTelemetry Collector local
- modelagem inicial de `User`
- persistência inicial de `User`
- migration inicial aplicada
- remoção de hardcode da connection string da `AppDbContextFactory`

### Ainda não implementado
- cadastro de usuário
- login
- JWT
- tela de login
- catálogo
- busca
- carrinho
- checkout
- pagamentos
- integração com Bitcoin Core
- integração com LND
- testes de feature reais
- fluxo completo de negócio

---

## Diretrizes iniciais do projeto

- O backend seguirá o modelo de **monólito modular**
- O banco relacional será a fonte principal de verdade
- A busca será tratada como capacidade especializada, desacoplada da persistência principal
- Pagamentos serão tratados como domínio crítico, com orquestração própria
- Observabilidade não será opcional: novas features devem nascer com logs, métricas, traces e health checks quando aplicável
- O projeto deve evoluir com disciplina arquitetural, evitando complexidade distribuída prematura

---

## Fluxo local esperado

A infraestrutura local será executada com Docker Compose, incluindo:

- PostgreSQL
- Redis
- RabbitMQ
- OpenSearch
- Bitcoin Core
- LND
- componentes de observabilidade

A aplicação backend e os frontends serão integrados progressivamente sobre essa base local.

---

## Como abrir um novo chat por feature

Prompt-base sugerido:

```text
Estou desenvolvendo um projeto chamado SatoshiForge.
Resumo:
- É um marketplace Bitcoin-only
- Aceita pagamentos on-chain e Lightning Network
- Usa nó próprio de Lightning com LND
- Usa Bitcoin Core para on-chain

Stack:
- Backend: ASP.NET Core / .NET
- Frontend: Angular
- Banco: PostgreSQL
- Cache: Redis
- Mensageria: RabbitMQ
- Busca: OpenSearch
- Observability: OpenTelemetry + Serilog + Health Checks
- Infra local: Docker Compose

Estrutura:
- backend/src/SatoshiForge.Api
- backend/src/SatoshiForge.Application
- backend/src/SatoshiForge.Domain
- backend/src/SatoshiForge.Infrastructure
- backend/src/SatoshiForge.Shared
- backend/tests/SatoshiForge.UnitTests
- backend/tests/SatoshiForge.IntegrationTests
- frontend/satoshiforge-frontend/projects/storefront
- frontend/satoshiforge-frontend/projects/admin

Arquitetura:
- PostgreSQL como source of truth
- OpenSearch para busca
- Redis para cache/locks
- RabbitMQ para filas
- Bitcoin Core + LND para pagamentos
- desenvolvimento feature por feature, ponta a ponta

Padrão para cada feature:
1. definir escopo
2. modelar domínio e banco
3. implementar backend
4. testes backend
5. observability backend
6. implementar frontend
7. testes frontend
8. observability frontend
9. validar ponta a ponta
10. commit final

Quero implementar agora a feature: [NOME DA FEATURE].
Me conduza passo a passo, com comandos quando possível, seguindo esse padrão.
```

---

## Roadmap inicial sugerido

1. fundação do repositório
2. estrutura base do backend
3. estrutura base do frontend
4. docker compose inicial
5. identidade e autenticação
6. catálogo de produtos
7. busca de produtos
8. carrinho
9. checkout
10. pagamentos on-chain
11. pagamentos Lightning
12. pedidos
13. seller portal
14. admin portal
15. treasury / ledger
16. reconciliação e settlement

---

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details.
