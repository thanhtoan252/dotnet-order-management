# Order Management — Microservices Architecture Showcase

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![React 19](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)
![Keycloak](https://img.shields.io/badge/Keycloak-26.0-4D4D4D?logo=keycloak)
![Kafka](https://img.shields.io/badge/Kafka-KRaft-231F20?logo=apachekafka)

A showcase project demonstrating production-grade microservices patterns with .NET 10: Clean Architecture, Domain-Driven Design (DDD), CQRS without MediatR, event-driven communication via Apache Kafka, API Gateway (YARP), resilience patterns (retry, circuit breaker, rate limiting), and Keycloak JWT/UMA authorization — all containerized with Docker Compose.

---

## System Architecture

```
                        ┌──────────────┐
                        │   React UI   │
                        │  :3000       │
                        └──────┬───────┘
                               │
                        ┌──────▼───────┐
                        │ API Gateway  │
                        │ (YARP) :8080 │
                        │ Rate Limit   │
                        │ JWT Auth     │
                        └──┬───────┬───┘
                           │       │
              ┌────────────▼─┐   ┌─▼────────────┐
              │Order Service │   │Catalog Service│
              │    :8081     │   │    :8082      │
              └──────┬───┬───┘   └───┬───┬───────┘
                     │   │           │   │
              ┌──────▼┐  │    ┌──────▼┐  │
              │OrderDb│  │    │CatalogDb  │
              └───────┘  │    └───────┘  │
                         │               │
                    ┌────▼───────────────▼────┐
                    │     Apache Kafka        │
                    │     (KRaft mode)        │
                    └────────────────────────┘
```

### Event-Driven Communication (Choreography Saga)

```
1. Client → POST /api/orders → Order Service
   → Order created (Status=Pending)
   → Publishes "order.placed" to Kafka (via Outbox)

2. Catalog Service consumes "order.placed"
   → Stock available → DeductStock → Publishes "catalog.stock-reserved"
   → Stock insufficient → Publishes "catalog.stock-reservation-failed"

3. Order Service consumes result
   → "stock-reserved" → Order.Confirm() → Status=Confirmed
   → "stock-reservation-failed" → Order.Cancel("Insufficient stock")
```

### Kafka Topics

| Topic | Producer | Consumer | Purpose |
|---|---|---|---|
| `order.placed` | Order Service | Catalog Service | Reserve stock |
| `order.cancelled` | Order Service | Catalog Service | Restore stock |
| `catalog.stock-reserved` | Catalog Service | Order Service | Auto-confirm order |
| `catalog.stock-reservation-failed` | Catalog Service | Order Service | Auto-cancel order |

---

## Tech Stack

### Backend

| Technology | Purpose |
|---|---|
| .NET 10 / ASP.NET Core Minimal API | Web framework |
| Entity Framework Core 10 + SQL Server | ORM + database (per service) |
| Apache Kafka (Confluent.Kafka) | Event-driven messaging |
| YARP Reverse Proxy | API Gateway |
| Keycloak 26 (JWT + UMA) | Authentication & authorization |
| Microsoft.Extensions.Http.Resilience | Retry + circuit breaker (Polly v8) |
| ASP.NET Core Rate Limiting | Fixed window, sliding window, concurrency |
| FluentValidation | Request validation |
| Serilog | Structured logging |
| Scalar | API reference UI (dev only) |
| NSwag | OpenAPI → C# DTO code generation |
| Central Package Management | Unified NuGet version control |

### Frontend

| Technology | Purpose |
|---|---|
| React 19 + TypeScript 5 | UI framework |
| Vite 8 | Build tool / dev server |
| Tailwind CSS 4 | Styling |
| Axios | HTTP client |
| keycloak-js | Keycloak SSO integration |

### Infrastructure

| Service | Image | Port |
|---|---|---|
| Order DB (SQL Server) | `mcr.microsoft.com/azure-sql-edge:latest` | 1433 |
| Catalog DB (SQL Server) | `mcr.microsoft.com/azure-sql-edge:latest` | 1434 |
| Kafka (KRaft) | `confluentinc/cp-kafka:7.9.0` | 29092 |
| Kafka UI | `provectuslabs/kafka-ui:latest` | 9090 |
| Keycloak | `quay.io/keycloak/keycloak:26.0` | 8180 |
| Keycloak DB | `postgres:16-alpine` | — |

---

## Project Structure

```
api/
├── Directory.Packages.props              # Central NuGet version management
├── OrderManagement.slnx                  # Solution file
│
├── Shared/
│   ├── Shared.Core/                      # Domain building blocks
│   │   ├── Domain/                       # BaseEntity, AggregateRoot, Result<T>, Error, IDomainEvent
│   │   ├── ValueObjects/                 # Money, Address
│   │   └── CQRS/                         # ICommand, IQuery, IDispatcher, Dispatcher, IUnitOfWork
│   ├── Shared.Contracts/                 # Integration event schemas
│   │   └── IntegrationEvents/            # OrderPlaced, StockReserved, etc.
│   └── Shared.Messaging/                 # Kafka infrastructure
│       ├── Abstractions/                 # IEventBus, IEventConsumer<T>
│       ├── Kafka/                        # KafkaProducer, KafkaConsumerHost, KafkaOptions
│       ├── Outbox/                       # OutboxMessage, OutboxProcessor, OutboxEventBus
│       └── Resilience/                   # HttpResilienceExtensions
│
├── Services/
│   ├── Order/
│   │   ├── Order.Domain/                 # OrderAggregate, OrderItem, OrderStatus, domain events
│   │   ├── Order.Application/            # CQRS handlers, Kafka consumers, OpenAPI contracts
│   │   ├── Order.Infrastructure/         # OrderDbContext, repositories, Kafka registration
│   │   ├── Order.Api/                    # Minimal API endpoints, auth, validators
│   │   └── Order.MigrationRunner/        # Standalone console app — applies EF Core migrations in Docker
│   └── Catalog/
│       ├── Catalog.Domain/               # Product aggregate, stock management
│       ├── Catalog.Application/          # CQRS handlers, Kafka consumers, OpenAPI contracts
│       ├── Catalog.Infrastructure/       # CatalogDbContext, repositories, Kafka registration
│       ├── Catalog.Api/                  # Minimal API endpoints, auth, validators
│       └── Catalog.MigrationRunner/      # Standalone console app — applies EF Core migrations in Docker
│
└── Gateway/
    └── ApiGateway/                       # YARP reverse proxy, rate limiting, JWT forwarding

docker-compose/
├── docker-compose.yml
└── .env

keycloak/                                 # Realm import config
ui/                                       # React 19 frontend
```

**MigrationRunner projects** (`Order.MigrationRunner`, `Catalog.MigrationRunner`) are standalone console apps that apply EF Core migrations in Docker Compose — they run before the API services start.

### Clean Architecture (per service)

| Layer | Responsibility |
|---|---|
| **Domain** | Aggregates, value objects, domain events, repository interfaces. Pure C# — no framework dependencies |
| **Application** | CQRS command/query handlers, Kafka event consumers, OpenAPI contracts, NSwag-generated DTOs, mappers |
| **Infrastructure** | EF Core DbContext, repository implementations, UnitOfWork, Outbox store, Kafka DI registration |
| **Api** | Minimal API endpoints, Keycloak auth, FluentValidation, exception handling, Serilog |

---

## Key Design Patterns

### CQRS (Command Query Responsibility Segregation)

Lightweight implementation without MediatR. `IDispatcher` resolves the correct handler from DI at runtime:

```csharp
public interface IDispatcher
{
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct = default);
    Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct = default);
}

// Endpoint usage
var result = await dispatcher.SendAsync(new PlaceOrderCommand(request, username), ct);
```

### Outbox Pattern (Transactional Messaging)

Domain changes and outbox messages are saved in the same DB transaction. A background `OutboxProcessor` polls and publishes to Kafka, ensuring at-least-once delivery without distributed transactions.

### Idempotent Consumers

Each Kafka consumer checks a `ProcessedMessages` table before handling. Duplicate events are skipped automatically.

### Dead-Letter Topics

Failed messages are retried 3 times with exponential backoff. After all retries are exhausted, the message is published to a `dlq.<topic>` dead-letter topic for inspection.

### Result Pattern

All handlers return `Result<T>` — never throw for domain errors. Endpoints map failures via `ResultExtensions.ToProblem()`:

- `*.NotFound` → 404
- `*.InvalidState` / `*.InsufficientStock` → 409
- anything else → 400

### Order State Machine

```
Pending → Confirmed → Shipped → Delivered
   └──────────────────────────→ Cancelled  (blocked from Shipped/Delivered/Cancelled)
```

State transitions are enforced inside the `OrderAggregate`. Stock reservation and confirmation are handled asynchronously via the Kafka saga.

### API-First Design

OpenAPI 3.1 YAML contracts are the single source of truth for request/response types. DTOs are generated by NSwag at build time — never written by hand.

---

## Resilience Patterns

### API Gateway — Rate Limiting

| Policy | Limit | Window |
|---|---|---|
| Fixed window | 100 requests | 1 minute |
| Sliding window | 1000 requests | 1 hour |
| Concurrency | 50 concurrent | — |
| Global (per IP) | 100 requests | 1 minute |

### Kafka Consumer — Retry + Dead-Letter

- 3 retry attempts with exponential backoff (1s → 2s → 4s)
- Failed messages moved to `dlq.<topic>` dead-letter topic
- Configurable via `KafkaOptions`

### HTTP Client — Retry + Circuit Breaker

For any synchronous inter-service calls (via `Shared.Messaging.Resilience.HttpResilienceExtensions`):

- **Retry:** 3 attempts, exponential backoff with jitter
- **Circuit breaker:** breaks after 50% failure rate (10s window), stays open 30s
- **Timeouts:** 10s per attempt, 30s total

---

## Authentication & Authorization

JWT Bearer tokens validated against Keycloak at the API Gateway level and forwarded to downstream services. Fine-grained permissions use Keycloak UMA (`response_mode=decision`), cached for 30 seconds.

| Policy | Resource |
|---|---|
| `order:confirm`, `order:ship`, `order:deliver`, `order:delete` | Order Resource |
| `product:create`, `product:update`, `product:delete` | Product Resource |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker + Docker Compose](https://docs.docker.com/get-docker/)

### Environment Setup

#### 1. `docker-compose/.env`

Create `docker-compose/.env`:

```env
# ─── SQL Server ───────────────────────────────────────────────────────────────
SA_PASSWORD=YourStr0ng!Pass

# ─── Keycloak ─────────────────────────────────────────────────────────────────
KEYCLOAK_DB_PASSWORD=keycloak
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin

# ─── ASP.NET Core (optional, defaults to Development) ────────────────────────
ASPNETCORE_ENVIRONMENT=Development
```

#### 2. `ui/.env.local`

Create `ui/.env.local`:

```env
VITE_API_BASE_URL=http://localhost:8080/api
```

### Run with Docker Compose (full stack)

```bash
cd docker-compose
docker compose up --build
```

**Startup order:** DB → MigrationRunner (waits for DB healthy) → API service (waits for migration success + Kafka healthy + Keycloak healthy) → Gateway (waits for APIs started) → UI (waits for Gateway).

| Service | URL |
|---|---|
| UI | http://localhost:3000 |
| API Gateway | http://localhost:8080 |
| Order Service | http://localhost:8081 |
| Catalog Service | http://localhost:8082 |
| Kafka UI | http://localhost:9090 |
| Keycloak Admin | http://localhost:8180 |

Default Keycloak admin credentials: `admin` / `admin`

### Run locally (development)

**1. Start infrastructure only:**

```bash
cd docker-compose
docker compose up order-db catalog-db kafka kafka-ui keycloak keycloak-db
```

**2. Run the services:**

```bash
# Terminal 1 — Order Service
dotnet run --project api/Services/Order/Order.Api

# Terminal 2 — Catalog Service
dotnet run --project api/Services/Catalog/Catalog.Api

# Terminal 3 — API Gateway
dotnet run --project api/Gateway/ApiGateway
```

**3. Run the UI:**

```bash
cd ui
npm install
npm run dev
```

---

## API Endpoints

All endpoints are accessed through the API Gateway at `http://localhost:8080`.

Scalar API docs available at `/scalar/v1` on each service in Development mode.

### Orders

| Method | Path | Description | Auth |
|---|---|---|---|
| `GET` | `/api/orders` | List all orders (paginated) | — |
| `GET` | `/api/orders/{id}` | Get order by ID | — |
| `GET` | `/api/orders/customer/{customerId}` | List orders for a customer | — |
| `POST` | `/api/orders` | Place a new order | — |
| `POST` | `/api/orders/{id}/confirm` | Confirm order | `order:confirm` |
| `POST` | `/api/orders/{id}/ship` | Mark as shipped | `order:ship` |
| `POST` | `/api/orders/{id}/deliver` | Mark as delivered | `order:deliver` |
| `POST` | `/api/orders/{id}/cancel` | Cancel order | — |
| `DELETE` | `/api/orders/{id}` | Soft-delete order | `order:delete` |

### Products

| Method | Path | Description | Auth |
|---|---|---|---|
| `GET` | `/api/products` | List products (paginated) | — |
| `POST` | `/api/products` | Create product | `product:create` |
| `PUT` | `/api/products/{id}` | Update product | `product:update` |
| `DELETE` | `/api/products/{id}` | Soft-delete product | `product:delete` |

---

## Development Commands

### Backend (.NET)

```bash
# Build entire solution (triggers NSwag DTO generation)
dotnet build api/OrderManagement.slnx

# Add EF Core migration (Order Service)
dotnet ef migrations add <MigrationName> \
  --project api/Services/Order/Order.Infrastructure \
  --startup-project api/Services/Order/Order.Api

# Add EF Core migration (Catalog Service)
dotnet ef migrations add <MigrationName> \
  --project api/Services/Catalog/Catalog.Infrastructure \
  --startup-project api/Services/Catalog/Catalog.Api
```

### Frontend

```bash
cd ui
npm run dev      # dev server with hot reload
npm run build    # production build
npm run lint     # ESLint
```

---

## Health Checks

Each service exposes a `/health` endpoint. The API Gateway proxies them:

| Endpoint | Target |
|---|---|
| `GET /health` | API Gateway self |
| `GET /services/order/health` | Order Service |
| `GET /services/catalog/health` | Catalog Service |
