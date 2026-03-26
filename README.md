# SatoshiForge

Marketplace **Bitcoin-only** com suporte a pagamentos **on-chain** e **Lightning Network**, usando infraestrutura própria para operação de pagamentos, liquidação e observabilidade.

## Visão geral

O SatoshiForge é um marketplace orientado a Bitcoin, com foco em:

- pagamentos on-chain via **Bitcoin Core**
- pagamentos Lightning via **LND**
- backend em **ASP.NET Core / .NET**
- frontend em **Angular**
- **PostgreSQL** como source of truth
- **Redis** para cache, locks e idempotência
- **RabbitMQ** para mensageria e processamento assíncrono
- **OpenSearch** para busca de produtos
- observabilidade com **OpenTelemetry**, logs estruturados e health checks
- ambiente local com **Docker Compose**

A construção do projeto seguirá uma abordagem **feature por feature**, ponta a ponta, mantendo o sistema evolutivo, organizado e validável desde o início.

---

## Objetivos do projeto

- construir um marketplace modular e evolutivo
- manter a lógica de negócio centralizada no backend
- separar claramente leitura, escrita, busca, cache e processamento assíncrono
- tratar pagamentos Bitcoin como domínio de primeira classe
- garantir rastreabilidade, idempotência e reconciliação financeira
- permitir crescimento sem migrar cedo demais para microservices

---

## Direção arquitetural

O projeto será desenvolvido como um **monólito modular**, com separação clara entre domínios de negócio, integrações externas e componentes de suporte.

Princípios adotados:

- **PostgreSQL** como fonte primária da verdade
- **OpenSearch** como mecanismo de busca e autocomplete
- **Redis** para cache, locking distribuído e suporte à idempotência
- **RabbitMQ** para eventos, filas e jobs assíncronos
- **Bitcoin Core** para operações on-chain
- **LND** para operações Lightning
- desenvolvimento incremental, por feature, com validação contínua

---

## Arquitetura esperada

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


## License

This project is licensed under the MIT License.