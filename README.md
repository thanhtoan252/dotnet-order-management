# Order Management Microservices

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Angular 21](https://img.shields.io/badge/Angular-21-DD0031?logo=angular)
![Docker Compose](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)
![Keycloak](https://img.shields.io/badge/Keycloak-26.0-4D4D4D?logo=keycloak)
![Kafka](https://img.shields.io/badge/Kafka-KRaft-231F20?logo=apachekafka)

Order Management Microservices is a full-stack showcase for a .NET 10 microservices system. It uses Clean Architecture, DDD-style aggregates, lightweight CQRS, Kafka integration events, outbox/idempotent messaging, YARP API Gateway, Keycloak authentication, role-based authorization, SignalR notifications, and OpenTelemetry observability with Grafana, Tempo, Loki, and Prometheus.

## Architecture

```text
Angular UI :3000
    |
API Gateway :8080
    |-- /api/auth/login
    |-- /api/orders           -> Order API :8081
    |-- /api/products         -> Catalog API :8082
    |-- /api/inventory        -> Inventory API :8083
    |-- /api/identity         -> Identity API :8084
    |-- /api/notifications    -> Notifications API :8085
    |-- /hubs/notifications   -> Notifications SignalR hub
    |-- /internal/inventory   -> Inventory internal API

SQL Server per data-owning service:
OrderDb :1433, CatalogDb :1434, InventoryDb :1435, NotificationsDb :1436

Kafka :29092, Keycloak :8180, Kafka UI :9090
OpenTelemetry Collector :4317/:4318 -> Tempo :3200, Loki :3100, Prometheus :9091, Grafana :3001
```

## Bounded Contexts

| Service | Owns | Notes |
|---|---|---|
| Order | Order lifecycle, order items, shipping address, order status | Checks inventory availability synchronously before creating an order, then publishes `order.placed` |
| Catalog | Product name, SKU, price, description | Publishes product lifecycle events consumed by Inventory and Notifications |
| Inventory | Stock projection, on-hand/reserved/available quantity | Consumes order and product events; exposes an internal availability check endpoint |
| Identity | Keycloak users and realm-role administration | Stateless service backed by Keycloak Admin API |
| Notifications | Notification records, templates, unread/read state, SignalR delivery | Consumes integration events and pushes real-time messages |

## Messaging

Kafka topics are defined in `api/Shared/Shared.Contracts/Topics.cs`.

| Topic | Producer | Consumers |
|---|---|---|
| `order.placed` | Order | Inventory, Notifications |
| `order.cancelled` | Order | Inventory, Notifications |
| `inventory.stock-reserved` | Inventory | Order, Notifications |
| `inventory.stock-reservation-failed` | Inventory | Order, Notifications |
| `catalog.product-created` | Catalog | Inventory, Notifications |
| `catalog.product-renamed` | Catalog | Inventory, Notifications |
| `catalog.product-deleted` | Catalog | Inventory, Notifications |

Order placement flow:

```text
1. Client posts /api/orders.
2. Order API calls Inventory internal availability check.
3. If available, Order saves a Pending order and publishes order.placed through the outbox.
4. Inventory consumes order.placed and reserves stock or publishes reservation failure.
5. Order consumes the Inventory result and moves to Confirmed or Cancelled.
6. Notifications consumes the same business events and persists/pushes notifications.
```

Messaging infrastructure includes an outbox processor, processed-message idempotency tables, retry with exponential backoff, and dead-letter topics named `dlq.<topic>`.

## Tech Stack

| Area | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core Minimal APIs, EF Core 10, FluentValidation 12 |
| API Gateway | YARP 2.3, ASP.NET Core rate limiting, JWT bearer auth |
| Auth | Keycloak 26 realm import, password login via Gateway, realm roles |
| Messaging | Apache Kafka KRaft, Confluent.Kafka 2.14 |
| HTTP clients | Refit 10, Microsoft.Extensions.Http.Resilience |
| API contracts | OpenAPI YAML contracts with NSwag DTO generation |
| Realtime | SignalR notification hub |
| Observability | OpenTelemetry 1.15, Serilog, Tempo, Loki, Prometheus, Grafana |
| Frontend | Angular 21, TypeScript 5.9, PrimeNG 21, Tailwind CSS 4, TanStack Angular Query, Zod, Lucide Angular |

## Project Layout

```text
api/
  Directory.Packages.props
  OrderManagement.slnx
  Gateway/ApiGateway/
  Services/
    Order/
    Catalog/
    Inventory/
    Identity/
    Notifications/
  Shared/
    Shared.Core/
    Shared.Contracts/
    Shared.Messaging.Abstractions/
    Shared.Messaging/
    Shared.Observability/
    Shared.Web/
docker-compose/
  docker-compose.yml
  observability/
keycloak/
  realm-export.json
ui/
  src/app/
    core/
      auth/
      http/
      layout/
    features/
      orders/
      products/
      inventory/
      users/
    shared/
```

Each business service follows the same general shape:

| Layer | Responsibility |
|---|---|
| Domain | Entities, aggregates, domain errors/events |
| Application | Commands, queries, handlers, models, mappers, event consumers |
| Infrastructure | EF Core DbContext, migrations, outbox, Kafka registration, external clients |
| Api | Minimal API endpoints, validators, auth, versioning, OpenAPI contracts |

Identity has no Domain or database layer; it delegates user and role operations to Keycloak. Order, Catalog, Inventory, and Notifications have MigrationRunner projects used by Docker Compose before the API containers start.

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 22+
- Docker and Docker Compose

### Environment

Create `docker-compose/.env`:

```env
SA_PASSWORD=YourStr0ng!Pass
KEYCLOAK_DB_PASSWORD=keycloak
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin
IDENTITY_API_CLIENT_SECRET=identity-api-secret
ASPNETCORE_ENVIRONMENT=Docker-Compose
```

When running the UI outside Docker, the API base URL is configured in `ui/src/environments/environment.ts` (defaults to `http://localhost:8080/api`).

### Run the Full Stack

```bash
cd docker-compose
docker compose up --build
```

| Service | URL |
|---|---|
| UI | http://localhost:3000 |
| API Gateway | http://localhost:8080 |
| Order API | http://localhost:8081 |
| Catalog API | http://localhost:8082 |
| Inventory API | http://localhost:8083 |
| Identity API | http://localhost:8084 |
| Notifications API | http://localhost:8085 |
| Keycloak Admin | http://localhost:8180 |
| Kafka UI | http://localhost:9090 |
| Grafana | http://localhost:3001 (`admin` / `admin`) |
| Tempo | http://localhost:3200 |
| Loki | http://localhost:3100 |
| Prometheus | http://localhost:9091 |

The Keycloak realm is imported from `keycloak/realm-export.json` on first startup. Default users:

| User | Password | Role |
|---|---|---|
| `admin` | `admin123` | `admin` |
| `user` | `user123` | standard user |

### Run Locally

Start infrastructure with Docker:

```bash
cd docker-compose
docker compose up \
  order-db catalog-db inventory-db notifications-db \
  kafka kafka-ui \
  keycloak keycloak-db \
  otel-collector tempo loki prometheus grafana
```

Run APIs from the repository root:

```bash
dotnet run --project api/Services/Order/Order.Api
dotnet run --project api/Services/Catalog/Catalog.Api
dotnet run --project api/Services/Inventory/Inventory.Api
dotnet run --project api/Services/Identity/Identity.Api
dotnet run --project api/Services/Notifications/Notifications.Api
dotnet run --project api/Gateway/ApiGateway
```

Run the UI:

```bash
cd ui
npm install
npm start
```

## API Surface

All client-facing APIs are routed through `http://localhost:8080`. Service-level Scalar docs are available in Development mode through each service.

### Auth

| Method | Path | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Password login through Keycloak; returns access token, refresh token, username, roles, expiry |

### Orders

| Method | Path | Auth |
|---|---|---|
| `GET` | `/api/orders` | bearer |
| `GET` | `/api/orders/{id}` | bearer |
| `GET` | `/api/orders/customer/{customerId}` | bearer |
| `POST` | `/api/orders` | bearer |
| `POST` | `/api/orders/{id}/confirm` | `admin` role |
| `POST` | `/api/orders/{id}/ship` | `admin` role |
| `POST` | `/api/orders/{id}/deliver` | `admin` role |
| `POST` | `/api/orders/{id}/cancel` | bearer |
| `DELETE` | `/api/orders/{id}` | `admin` role |

### Products

| Method | Path | Auth |
|---|---|---|
| `GET` | `/api/products` | bearer |
| `POST` | `/api/products` | `admin` role |
| `POST` | `/api/products/import` | `admin` role |
| `PUT` | `/api/products/{id}` | `admin` role |
| `DELETE` | `/api/products/{id}` | `admin` role |

### Inventory

| Method | Path | Auth |
|---|---|---|
| `GET` | `/api/inventory` | bearer |
| `GET` | `/api/inventory/{productId}` | bearer |
| `POST` | `/api/inventory` | `admin` role |
| `POST` | `/api/inventory/{productId}/receive` | `admin` role |
| `POST` | `/api/inventory/{productId}/adjust` | `admin` role |
| `POST` | `/internal/inventory/availability` | internal route |

### Identity

All Identity endpoints require the `admin` role.

| Method | Path |
|---|---|
| `GET` | `/api/identity/users` |
| `GET` | `/api/identity/users/count` |
| `GET` | `/api/identity/users/realm-roles` |
| `GET` | `/api/identity/users/{id}` |
| `GET` | `/api/identity/users/{id}/roles` |
| `POST` | `/api/identity/users` |
| `PUT` | `/api/identity/users/{id}` |
| `DELETE` | `/api/identity/users/{id}` |
| `POST` | `/api/identity/users/{id}/reset-password` |
| `PUT` | `/api/identity/users/{id}/roles` |

### Notifications

| Method | Path | Auth |
|---|---|---|
| `GET` | `/api/notifications` | bearer |
| `GET` | `/api/notifications/unread-count` | bearer |
| `POST` | `/api/notifications/{id}/read` | bearer |
| `POST` | `/api/notifications/mark-all-read` | bearer |
| `POST` | `/api/notifications` | `admin` role |
| `GET` | `/api/notifications/templates` | `admin` role |
| `PUT` | `/api/notifications/templates/{id}` | `admin` role |
| SignalR | `/hubs/notifications` | bearer |

## Authorization

APIs validate JWT bearer tokens issued by the `order-management` Keycloak realm. Current service policies are role-based:

| Policy | Requirement |
|---|---|
| `order:confirm`, `order:ship`, `order:deliver`, `order:delete` | `admin` role |
| `product:create`, `product:update`, `product:delete` | `admin` role |
| `inventory:adjust` | `admin` role |
| `identity:manage` | `admin` role |
| `notifications:admin` | `admin` role |

## Frontend

The Angular SPA includes:

- Login via `/api/auth/login`
- Authenticated shell with responsive sidebar/topbar navigation for products, orders, inventory, and users
- Product CRUD plus `.xlsx` import
- Order list, place/cancel actions, and admin lifecycle actions
- Inventory stock receive/adjust workflows
- Admin user management for Keycloak users, passwords, and realm roles
- HTTP client with ProblemDetails handling and auth token interceptor
- TanStack Angular Query for API cache, refetch, and mutation state
- Reactive Forms validated with Zod schemas
- PrimeNG tables, dialogs, inputs, buttons, toasts, and an Aura-based emerald theme layered with Tailwind CSS utilities
- Lucide icons for navigation and action controls

## Observability

All APIs and the Gateway call `AddObservability("<service-name>")`.

| Signal | Implementation |
|---|---|
| Traces | ASP.NET Core, HttpClient, SqlClient, Kafka producer/consumer spans |
| Metrics | ASP.NET Core, HttpClient, runtime/process, Kafka custom meter |
| Logs | Serilog console/file/OTLP with trace and span correlation |

Signals are exported to the OpenTelemetry Collector, then to Tempo, Loki, and Prometheus. Grafana datasources and dashboards are provisioned from `docker-compose/observability/grafana/provisioning`.

Per-service rolling log files are bind-mounted under `docker-compose/logs/<service>/`.

## Health Checks

| Endpoint | Target |
|---|---|
| `GET /health` | API Gateway |
| `GET /services/order/health` | Order API |
| `GET /services/catalog/health` | Catalog API |
| `GET /services/inventory/health` | Inventory API |
| `GET /services/identity/health` | Identity API |
| `GET /services/notifications/health` | Notifications API |

## Development Commands

```bash
# Backend build
dotnet build api/OrderManagement.slnx

# Frontend
cd ui
npm run build
```

Add EF Core migrations:

```bash
dotnet ef migrations add <Name> \
  --project api/Services/Order/Order.Infrastructure \
  --startup-project api/Services/Order/Order.Api

dotnet ef migrations add <Name> \
  --project api/Services/Catalog/Catalog.Infrastructure \
  --startup-project api/Services/Catalog/Catalog.Api

dotnet ef migrations add <Name> \
  --project api/Services/Inventory/Inventory.Infrastructure \
  --startup-project api/Services/Inventory/Inventory.Api

dotnet ef migrations add <Name> \
  --project api/Services/Notifications/Notifications.Infrastructure \
  --startup-project api/Services/Notifications/Notifications.Api
```

## License

MIT. See `LICENSE`.
