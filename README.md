# Hub de Integração Bancária — Pix / Open Banking

API de **integração bancária para cobranças Pix** (Cob e CobV), construída em **.NET 10** seguindo **Clean Architecture**, **DDD** e **CQRS**. Projeto da Residência de Software (UNIT) — Squad 08.

O objetivo é emitir cobranças Pix, validar pagamentos e operar de forma **agnóstica de banco** (adapters por instituição), com o **Itaú** como primeira integração.

> ⚠️ **Regra de negócio central:** o webhook do banco **nunca** é fonte da verdade — ele apenas *dispara* o processo. A confirmação de pagamento ocorre **sempre** via **consulta ativa** ao banco (`GetChargeStatusAsync`).

---

## Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 (C#) |
| Banco | PostgreSQL 15 (EF Core) |
| Mensageria | RabbitMQ |
| CQRS | MediatR |
| Validação | FluentValidation |
| Mock de banco | MockServer |
| UI da API | Scalar (OpenAPI) |

## Arquitetura

```
src/
├─ Domain/          # Entidades, Agregados, Value Objects, Eventos (sem dependências externas)
├─ Application/     # Commands, Queries, Handlers (CQRS), Interfaces, DTOs
├─ Infrastructure/  # EF Core, Adapters de banco (Itaú), Repositórios, Mensageria
└─ ApiService/      # API REST, Controllers, Program.cs (composição/DI)
```

Solução: `BankingIntegration_squad_8.sln`

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Postgres, RabbitMQ e MockServer)

## Como rodar

```bash
# 1. Subir a infraestrutura (Postgres, RabbitMQ, MockServer)
docker compose up -d

# 2. Rodar a API
cd src/ApiService
dotnet run
```

A API sobe em **http://localhost:5243**.

| Recurso | URL |
|---|---|
| **UI interativa (Scalar)** | http://localhost:5243/ → redireciona pra `/scalar/v1` |
| Contrato OpenAPI (JSON) | http://localhost:5243/openapi/v1.json |

> Em **Development**, o schema do banco é criado automaticamente no startup (`EnsureCreated`). Em produção, use migrations (`dotnet ef database update`).

### Serviços (docker-compose)

| Serviço | Porta | Observação |
|---|---|---|
| PostgreSQL | 5432 | banco da aplicação |
| RabbitMQ | 5672 / 15672 | broker / painel de gestão |
| MockServer | 1080 | simula a API do banco para testes locais |

---

## Configuração

As configurações ficam em `src/ApiService/appsettings.json`:

- `ConnectionStrings:DefaultConnection` — conexão com o Postgres.
- `Itau` — credenciais OAuth2 e URL de token do banco.
- `ItauAdapter` — base URL e flag de sandbox do adapter.

No ambiente local, `Itau`/`ItauAdapter` apontam para o **MockServer** (`http://localhost:1080`) com credenciais fictícias. Para usar o **Itaú real**, troque as URLs e as credenciais (veja a seção de Segurança).

### 🔒 Segurança — não versione segredos

- **Nunca** faça commit de credenciais reais, `ClientSecret`, senhas de banco ou **certificados** (`.pfx`/`.pem`).
- Para credenciais locais, prefira **User Secrets**:
  ```bash
  cd src/ApiService
  dotnet user-secrets init
  dotnet user-secrets set "Itau:ClientId" "<seu-client-id>"
  dotnet user-secrets set "Itau:ClientCredential" "<seu-client-secret>"
  ```
- Em produção, use variáveis de ambiente / cofre de segredos (Key Vault, etc.).
- Os valores de exemplo no `appsettings.json` e no `docker-compose.yml` são **apenas para desenvolvimento local** — troque-os antes de qualquer deploy.

---

## Principais endpoints

| Método | Rota | Descrição |
|---|---|---|
| POST | `/cobranca/v1/cob` | Cria cobrança Pix imediata (Cob) |
| POST | `/cobranca/v1/cobv` | Cria cobrança Pix com vencimento (CobV) |
| POST | `/api/v1/webhooks/{bankId}` | Recebe webhook do banco (gatilho) |
| GET | `/api/v1/pix-charges/{txId}` | Consulta status da cobrança (com consulta ativa) |
| GET/POST/PUT/DELETE | `/contas` | CRUD de contas |
| GET/POST/PUT/DELETE | `/credentials` | CRUD de credenciais bancárias |
| POST | `/payments/mock` | Pagamento simulado (mock) |

A lista completa e a possibilidade de testar pelo navegador estão na **UI Scalar** (`/`).

---

## Fluxo de confirmação de pagamento via webhook

Demonstra a regra central (webhook dispara, confirmação vem da consulta ativa):

```bash
# 1. Cria uma cobrança e anote o txId retornado
curl -X POST http://localhost:5243/cobranca/v1/cob \
  -H "Content-Type: application/json" \
  -d '{"invoiceID":"INV-1","chargeType":"COB","amount":10.00,"pixKey":"x@y.com"}'

# 2. Dispara o webhook com esse txId
curl -X POST http://localhost:5243/api/v1/webhooks/ITAU \
  -H "Content-Type: application/json" \
  -d '{"pix":[{"txid":"<TXID>"}]}'
# -> "Pagamento confirmado via consulta ativa ao banco."

# 3. Consulta o status (deve estar "paid")
curl http://localhost:5243/api/v1/pix-charges/<TXID>
```

O processamento é **idempotente**: reenviar o mesmo webhook não reprocessa o pagamento.

### Mock do banco (Itaú)

Para o fluxo acima funcionar localmente, o MockServer precisa responder ao token OAuth2 e ao status da cobrança. As expectativas podem ser registradas via API do MockServer (`PUT http://localhost:1080/mockserver/expectation`):

- `POST /oauth/token` → retorna um `access_token`.
- `GET /cob/{txid}` e `GET /cobv/{txid}` → retornam `{"status":"CONCLUIDA", ...}`.

---

## Estado atual / limitações conhecidas

- ✅ Sobe, persiste no Postgres e o fluxo **Cob → webhook → confirmação via consulta ativa** funciona ponta a ponta (contra o mock).
- A confirmação roda contra o **MockServer**, não contra o Itaú real (basta trocar URLs/credenciais para produção).
- Os agregados `Invoice`/`PixCharge` ainda **não** são persistidos (mapeamento EF incompleto) e estão fora do modelo por enquanto.
- `EnsureCreated` é usado em Development; migrations são o caminho recomendado para produção.

---

## Notas

Documento técnico de referência: especificação da Residência (Clean Architecture, DDD, CQRS, API First). Em caso de divergência entre código e documento, o documento é a autoridade.
