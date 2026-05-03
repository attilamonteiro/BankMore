# BankMore

Plataforma de banco digital baseada em microsserviços .NET 10 + Vue 3, desenvolvida como teste técnico para a **Ailos**.

---

## Arquitetura

```
BankMore/
├── src/
│   ├── Shared/                  # JWT, Dapper, Idempotência, Modelos de erro
│   ├── ContaCorrente/           # API REST — cadastro, login, movimentação, saldo
│   ├── Transferencia/           # API REST — transferência entre contas
│   ├── Tarifas/                 # Worker — desconto de tarifa via Kafka
│   └── Frontend/bankmore-app/   # SPA Vue 3 + Pinia
└── tests/
    ├── BankMore.ContaCorrente.UnitTests
    ├── BankMore.ContaCorrente.IntegrationTests
    ├── BankMore.Transferencia.UnitTests
    └── BankMore.Transferencia.IntegrationTests
```

### Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10, ASP.NET Core, MediatR (CQRS), Dapper, SQLite |
| Autenticação | JWT HS256 — `sub` = GUID interno |
| Idempotência | INSERT-first com UNIQUE constraint (atômico, sem race condition) |
| Senha | PBKDF2 — Rfc2898DeriveBytes, 100 k iterações, SHA-256, salt 32 bytes |
| Mensageria | Kafka (KRaft, sem Zookeeper) + KafkaFlow 3 |
| Resiliência | Polly — retry exponencial + circuit breaker |
| Frontend | Vue 3 (Composition API), Pinia, Vue Router 4, Axios, TypeScript, Tailwind CSS, Vite |
| Infraestrutura | Docker + Docker Compose, nginx (reverse proxy + SPA fallback) |
| Testes | xUnit, NSubstitute, FluentAssertions, WebApplicationFactory |

---

## Serviços

| Serviço | Porta | Descrição |
|---|---|---|
| Frontend | http://localhost:8090 | SPA Vue 3 via nginx |
| API Conta Corrente | http://localhost:5001 | Cadastro, login, movimentação, saldo |
| API Transferência | http://localhost:5002 | Transferência entre contas |
| Kafka | localhost:9092 | Broker KRaft |

---

## Como rodar

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Subir tudo com um comando

```bash
docker-compose up --build
```

Aguarde todos os containers ficarem `healthy` (~2 min na primeira vez).  
Acesse **http://localhost:8090**.

### Derrubar

```bash
docker-compose down -v
```

---

## Fluxo da aplicação

```
Usuário → Frontend (8090)
           ├─► nginx proxy → API ContaCorrente (5001)
           └─► nginx proxy → API Transferência (5002)
                                └─► HTTP → API ContaCorrente (débito/crédito)
                                └─► Kafka → Worker Tarifas (desconta 1%)
                                              └─► Kafka → API ContaCorrente (débito tarifa)
```

---

## Endpoints

### API Conta Corrente — `http://localhost:5001/swagger`

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| `POST` | `/api/contacorrente` | ❌ | Cadastrar conta (CPF + senha) |
| `POST` | `/api/contacorrente/login` | ❌ | Login → JWT + número da conta |
| `PATCH` | `/api/contacorrente/inativar` | ✅ | Inativar conta |
| `POST` | `/api/contacorrente/movimento` | ✅ | Crédito (C) ou débito (D) |
| `GET` | `/api/contacorrente/saldo` | ✅ | Consultar saldo |

### API Transferência — `http://localhost:5002/swagger`

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| `POST` | `/api/transferencia` | ✅ | Transferir entre contas |

---

## Decisões técnicas

**Idempotência** — cada operação aceita uma `chaveIdempotencia` (UUID v4). O primeiro request processa e salva o resultado; requests repetidos com a mesma chave retornam o resultado cacheado sem reprocessar.

**JWT** — o token carrega apenas `sub = GUID` da conta. CPF e número da conta nunca transitam entre serviços via token.

**Kafka + Tarifas** — após cada transferência, a API publica um evento em `transferencias-realizadas`. O Worker Tarifas consome, calcula 1% do valor e publica em `tarifas-realizadas`. A API Conta Corrente consome esse tópico e debita a tarifa automaticamente.

**Rollback de transferência** — se o crédito na conta destino falhar após o débito na origem, um crédito compensatório é aplicado automaticamente na origem com chave `{idempotencia}-rollback`.

**SQLite por serviço** — cada microsserviço tem seu próprio arquivo `.db`, isolado em volume Docker. WAL mode para melhor concorrência.

---

## Testes

```bash
dotnet test
```

- **Unit tests** — handlers com NSubstitute (mocks), validações de domínio (CPF, movimentos)
- **Integration tests** — `WebApplicationFactory` com SQLite real em memória, fluxo completo de ponta a ponta

---

## Estrutura do Frontend

```
src/Frontend/bankmore-app/
├── src/
│   ├── stores/          # Pinia — auth, conta, transferencia
│   ├── services/        # Axios — contaCorrenteApi, transferenciaApi
│   ├── composables/     # useIdempotency (UUID v4), useNotification (toasts)
│   ├── components/      # BaseButton, BaseInput, BaseAlert, AppLayout...
│   └── views/           # Login, Cadastro, Dashboard, Movimento, Transferência, Conta
└── Dockerfile           # node:20-alpine → nginx:alpine
```
