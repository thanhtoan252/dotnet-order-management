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
                               │    :3000     │
                               └──────┬───────┘
                                      │
                               ┌──────▼───────┐
                               │ API Gateway  │
                               │ (YARP) :8080 │
                               │ Rate Limit   │
                               │ JWT Auth     │
                               └──┬────┬────┬─┘
                                  │    │    │
              ┌───────────────────┘    │    └───────────────────┐
              │                        │                        │
      ┌───────▼────────┐      ┌────────▼────────┐      ┌────────▼─────────┐
      │ Order Service  │      │ Catalog Service │      │Inventory Service │
      │     :8081      │      │     :8082       │      │      :8083       │
      └───┬────────┬───┘      └───┬─────────┬───┘      └────┬─────────┬───┘
          │        │              │         │               │         │
       ┌──▼────┐   │        ┌─────▼─────┐   │        ┌──────▼──────┐  │
       │OrderDb│   │        │ CatalogDb │   │        │ InventoryDb │  │
       └───────┘   │        └───────────┘   │        └─────────────┘  │
                   │                        │                         │
                   └────────────┬───────────┴─────────────────────────┘
                                │
                       ┌────────▼─────────┐
                       │  Apache Kafka    │
                       │   (KRaft mode)   │
                       └──────────────────┘
```

Each service owns its own database. Services communicate **only** through Kafka events — no synchronous cross-service HTTP calls.

### Bounded Contexts

| Service | Owns | Does NOT own |
|---|---|---|
| **Order** | Order lifecycle, order items (price/name snapshots) | Stock, product catalog |
| **Catalog** | Product CRUD (name, SKU, price, description) | Stock |
| **Inventory** | Stock levels (`OnHand`, `Reserved`, `Available`), reservations | Product master data |

`Catalog` publishes product-lifecycle events; `Inventory` consumes them and maintains its own `InventoryItem` projection (name + SKU) so it never calls Catalog synchronously.

### Event-Driven Communication (Choreography Saga)

Stock reservation on order placement:

```
1. Client → POST /api/orders → Order Service
   → Order created (Status = Pending)
   → Publishes "order.placed" to Kafka (via Outbox)

2. Inventory Service consumes "order.placed"
   → Sufficient stock → Reserve → Publishes "inventory.stock-reserved"
   → Insufficient stock → Publishes "inventory.stock-reservation-failed"

3. Order Service consumes the result
   → "stock-reserved"           → Order.Confirm()  → Status = Confirmed
   → "stock-reservation-failed" → Order.Cancel()   → Status = Cancelled
```

Order cancellation triggers `order.cancelled`, which Inventory consumes to restore previously reserved stock.

### Kafka Topics

| Topic | Producer | Consumer | Purpose |
|---|---|---|---|
| `order.placed` | Order | Inventory | Reserve stock for a newly placed order |
| `order.cancelled` | Order | Inventory | Restore previously reserved stock |
| `inventory.stock-reserved` | Inventory | Order | Auto-confirm the order |
| `inventory.stock-reservation-failed` | Inventory | Order | Auto-cancel the order |
| `catalog.product-created` | Catalog | Inventory | Seed a new `InventoryItem` projection |
| `catalog.product-renamed` | Catalog | Inventory | Update projection name/SKU |
| `catalog.product-deleted` | Catalog | Inventory | Soft-delete the projection |

Canonical list: `api/Shared/Shared.Contracts/Topics.cs`.

---

## Tech Stack

### Backend

| Technology | Purpose |
|---|---|
| .NET 10 / ASP.NET Core Minimal API | Web framework |
| Entity Framework Core 10 + SQL Server | ORM + database (per service) |
| Apache Kafka (Confluent.Kafka 2.8) | Event-driven messaging |
| YARP 2.3 Reverse Proxy | API Gateway |
| Keycloak 26 (JWT + UMA) | Authentication & authorization |
| Microsoft.Extensions.Http.Resilience | Retry + circuit breaker (Polly v8) |
| ASP.NET Core Rate Limiting | Fixed window, sliding window, concurrency |
| FluentValidation 12 | Request validation |
| Serilog 4.2 | Structured logging |
| Scalar | API reference UI (dev only) |
| NSwag | OpenAPI → C# DTO code generation |
| Central Package Management | Unified NuGet version control |

### Frontend

| Technology | Purpose |
|---|---|
| React 19 + TypeScript 5.9 | UI framework |
| Vite 8 | Build tool / dev server |
| Tailwind CSS 4 | Styling |
| Lucide React | Icon library |
| Sonner | Toast notifications |

### Infrastructure

| Service | Image | Port |
|---|---|---|
| Order DB (SQL Server) | `mcr.microsoft.com/azure-sql-edge:latest` | 1433 |
| Catalog DB (SQL Server) | `mcr.microsoft.com/azure-sql-edge:latest` | 1434 |
| Inventory DB (SQL Server) | `mcr.microsoft.com/azure-sql-edge:latest` | 1435 |
| Kafka (KRaft) | `confluentinc/cp-kafka:7.9.0` | 29092 |
| Kafka UI | `provectuslabs/kafka-ui:latest` | 9090 |
| Keycloak | `quay.io/keycloak/keycloak:26.0` | 8180 |
| Keycloak DB | `postgres:16-alpine` | internal |

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
│   ├── Shared.Contracts/                 # Integration event schemas + Topics.cs
│   │   └── IntegrationEvents/            # OrderPlaced, StockReserved, ProductCreated, ...
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
│   │   └── Order.MigrationRunner/        # Standalone console — applies EF Core migrations in Docker
│   ├── Catalog/
│   │   ├── Catalog.Domain/               # ProductAggregate (name, SKU, price — no stock)
│   │   ├── Catalog.Application/          # CQRS handlers, product-lifecycle event publishing
│   │   ├── Catalog.Infrastructure/       # CatalogDbContext, repositories, Kafka registration
│   │   ├── Catalog.Api/                  # Minimal API endpoints, auth, validators
│   │   └── Catalog.MigrationRunner/      # Standalone console — applies EF Core migrations in Docker
│   └── Inventory/
│       ├── Inventory.Domain/             # InventoryItem (OnHand, Reserved, Available), stock ops
│       ├── Inventory.Application/        # CQRS handlers, Kafka consumers for order + catalog events
│       ├── Inventory.Infrastructure/     # InventoryDbContext, repositories, Kafka registration
│       ├── Inventory.Api/                # Minimal API endpoints, auth, validators
│       └── Inventory.MigrationRunner/    # Standalone console — applies EF Core migrations in Docker
│
└── Gateway/
    └── ApiGateway/                       # YARP reverse proxy, rate limiting, JWT forwarding

docker-compose/
├── docker-compose.yml
├── keycloak/
│   └── realm-import.json                 # Auto-imported on first startup
└── .env

ui/                                       # React 19 frontend
├── src/
│   ├── features/
│   │   ├── auth/                         # Keycloak login, role-based access
│   │   ├── order-management/             # Order CRUD, state machine actions
│   │   └── product-management/           # Product CRUD
│   ├── components/                       # Shared UI (Modal, ErrorBoundary, Tooltip)
│   └── lib/api/                          # REST client, error handling
├── Dockerfile                            # Multi-stage Node → nginx
├── nginx.conf                            # SPA fallback + API proxy
└── vite.config.ts
```

**MigrationRunner projects** (`Order.MigrationRunner`, `Catalog.MigrationRunner`, `Inventory.MigrationRunner`) are standalone console apps that apply EF Core migrations in Docker Compose — they run before the API services start.

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

Commands and queries live alongside their handlers (e.g., `PlaceOrder.cs` contains both `PlaceOrderCommand` and `PlaceOrderHandler`).

### Outbox Pattern (Transactional Messaging)

Domain changes and outbox messages are saved in the same DB transaction. A background `OutboxProcessor` polls every 2s and publishes to Kafka, ensuring at-least-once delivery without distributed transactions.

### Idempotent Consumers

Each Kafka consumer checks a `ProcessedMessages` table before handling. Duplicate events are skipped automatically.

### Dead-Letter Topics

`KafkaConsumerHost<TEvent>` retries 3 times with exponential backoff (1s → 2s → 4s). After all retries are exhausted, the message is published to `dlq.<topic>` for inspection.

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

State transitions are enforced inside `OrderAggregate`. Stock reservation and confirmation happen asynchronously via the Kafka saga.

### API-First Design

OpenAPI 3.1 YAML contracts in each service's `Application/Contracts/` are the single source of truth for request/response types. DTOs are generated by NSwag at build time via `<OpenApiReference>` in `.csproj` — never written by hand.

---

## Frontend

The React 19 SPA provides a responsive dashboard for managing orders and products. Key design choices:

- **Authentication** — Direct Keycloak login form with JWT stored in localStorage. Bearer tokens are attached to every API request. Role-based UI guards hide actions the user lacks permissions for.
- **Navigation** — Hash-based routing (`#products`, `#orders`) without a router library
- **State management** — React Context + component-level state (no external library)
- **API layer** — Custom fetch-based REST client with ProblemDetails error handling and 30s timeout
- **UI** — Responsive sidebar layout (collapsible on desktop, hamburger on mobile), modal-based forms for CRUD operations, color-coded status badges, and toast notifications via Sonner

### Features

| Feature | Description |
|---|---|
| **Product Management** | List (paginated), create, edit, and delete products (name, SKU, price, currency, description) |
| **Order Management** | List (paginated), place order, confirm → ship → deliver lifecycle, cancel with reason, soft-delete |
| **Real-time Status** | Status badges reflect async saga results (Pending → Confirmed / Cancelled) |
| **Permission-aware UI** | Action buttons are shown/hidden based on the authenticated user's Keycloak roles |

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

For any synchronous outbound HTTP calls (via `Shared.Messaging.Resilience.HttpResilienceExtensions`):

- **Retry:** 3 attempts, exponential backoff with jitter
- **Circuit breaker:** breaks after 50% failure rate (10s window), stays open 30s
- **Timeouts:** 10s per attempt, 30s total

---

## Authentication & Authorization

JWT Bearer tokens are validated against Keycloak at the API Gateway and forwarded to downstream services. Fine-grained permissions use Keycloak UMA (`response_mode=decision`), cached for 30 seconds.

| Policy | Resource |
|---|---|
| `order:confirm`, `order:ship`, `order:deliver`, `order:delete` | Order |
| `product:create`, `product:update`, `product:delete` | Product |
| `inventory:adjust` | Inventory |

The `order-management` realm is auto-imported from `docker-compose/keycloak/realm-import.json` on first startup. Pre-configured users:

| User | Password | Role |
|---|---|---|
| `admin` | `admin123` | Admin |
| `user` | `user123` | Standard user |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker + Docker Compose](https://docs.docker.com/get-docker/)

### Environment Setup

#### 1. `docker-compose/.env`

```env
# ─── SQL Server ───────────────────────────────────────────────────────────────
SA_PASSWORD=YourStr0ng!Pass

# ─── Keycloak ─────────────────────────────────────────────────────────────────
KEYCLOAK_DB_PASSWORD=keycloak
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin

# ─── ASP.NET Core ────────────────────────────────────────────────────────────
ASPNETCORE_ENVIRONMENT=Docker-Compose
```

#### 2. `ui/.env.local`

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
| Inventory Service | http://localhost:8083 |
| Kafka UI | http://localhost:9090 |
| Keycloak Admin | http://localhost:8180 |

### Run locally (development)

**1. Start infrastructure only:**

```bash
cd docker-compose
docker compose up order-db catalog-db inventory-db kafka kafka-ui keycloak keycloak-db
```

**2. Run the services:**

```bash
# Terminal 1 — Order Service
dotnet run --project api/Services/Order/Order.Api

# Terminal 2 — Catalog Service
dotnet run --project api/Services/Catalog/Catalog.Api

# Terminal 3 — Inventory Service
dotnet run --project api/Services/Inventory/Inventory.Api

# Terminal 4 — API Gateway
dotnet run --project api/Gateway/ApiGateway
```

**3. Run the UI:**

```bash
cd ui
npm install
npm run dev
```

Kafka bootstrap: `localhost:29092` for local dev, `kafka:9092` inside Docker.

---

## API Endpoints

All endpoints are accessed through the API Gateway at `http://localhost:8080`.

Scalar API docs are available at `/scalar/v1` on each service in Development mode.

### Orders

| Method | Path | Description | Auth |
|---|---|---|---|
| `GET` | `/api/orders` | List all orders (paginated) | Bearer |
| `GET` | `/api/orders/{id}` | Get order by ID | Bearer |
| `GET` | `/api/orders/customer/{customerId}` | List orders for a customer | Bearer |
| `POST` | `/api/orders` | Place a new order | Bearer |
| `POST` | `/api/orders/{id}/confirm` | Confirm order | `order:confirm` |
| `POST` | `/api/orders/{id}/ship` | Mark as shipped | `order:ship` |
| `POST` | `/api/orders/{id}/deliver` | Mark as delivered | `order:deliver` |
| `POST` | `/api/orders/{id}/cancel` | Cancel order | Bearer |
| `DELETE` | `/api/orders/{id}` | Soft-delete order | `order:delete` |

### Products

| Method | Path | Description | Auth |
|---|---|---|---|
| `GET` | `/api/products` | List products (paginated) | Bearer |
| `POST` | `/api/products` | Create product | `product:create` |
| `PUT` | `/api/products/{id}` | Update product | `product:update` |
| `DELETE` | `/api/products/{id}` | Soft-delete product | `product:delete` |

### Inventory

| Method | Path | Description | Auth |
|---|---|---|---|
| `GET` | `/api/inventory` | List inventory items (paginated) | Bearer |
| `GET` | `/api/inventory/{productId}` | Get inventory item for a product | Bearer |
| `POST` | `/api/inventory` | Create inventory item for a product | `inventory:adjust` |
| `POST` | `/api/inventory/{productId}/receive` | Add quantity to on-hand stock | `inventory:adjust` |
| `POST` | `/api/inventory/{productId}/adjust` | Set on-hand stock to an absolute quantity | `inventory:adjust` |

### Internal (not exposed through the Gateway to external clients)

| Method | Path | Description |
|---|---|---|
| `POST` | `/internal/inventory/availability` | Check stock availability |

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

# Add EF Core migration (Inventory Service)
dotnet ef migrations add <MigrationName> \
  --project api/Services/Inventory/Inventory.Infrastructure \
  --startup-project api/Services/Inventory/Inventory.Api
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
| `GET /services/inventory/health` | Inventory Service |
