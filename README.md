# Order Management System

A full-stack order management application built with .NET 10 and React 19, demonstrating Domain-Driven Design (DDD), Clean Architecture, Keycloak authentication, and high-concurrency handling.

---

## Tech Stack

### Backend

| Technology | Purpose |
|---|---|
| .NET 10 / ASP.NET Core Minimal API | Web framework |
| Entity Framework Core + SQL Server | ORM + database |
| Keycloak 26 (JWT + UMA) | Authentication & authorization |
| FluentValidation | Request validation |
| Serilog | Structured logging (console + rolling JSON file) |
| Scalar | API reference UI (dev only) |

### Frontend

| Technology | Purpose |
|---|---|
| React 19 + TypeScript | UI framework |
| Vite 8 | Build tool / dev server |
| Tailwind CSS 4 | Styling |
| Axios | HTTP client |
| keycloak-js | Keycloak SSO integration |
| Lucide React | Icons |
| Sonner | Toast notifications |

### Infrastructure

| Service | Image |
|---|---|
| SQL Server | `mcr.microsoft.com/azure-sql-edge:latest` |
| Keycloak | `quay.io/keycloak/keycloak:26.2` |
| Keycloak DB | `postgres:16-alpine` |

---

## Architecture

### Layer Dependencies

```
Domain  <──  Application  <──  Infrastructure
                  ^                   ^
                  └──────── Api ──────┘
```

| Layer | Project | Responsibility |
|---|---|---|
| **Domain** | `OrderManagement.Domain` | Pure C# — `Order` and `Product` aggregates, `Money`/`Address` value objects, `Result<T>`/`Error` types, `DomainErrors`, repository interfaces |
| **Application** | `OrderManagement.Application` | `OrderService` / `ProductService`, DTOs, `IUnitOfWork`. No EF Core dependency |
| **Infrastructure** | `OrderManagement.Infrastructure` | `OrderDbContext`, EF configurations, repository implementations, `UnitOfWork`, `DbSeeder` |
| **Api** | `OrderManagement.Api` | Minimal API endpoints, Keycloak JWT/UMA auth, `GlobalExceptionHandler`, Serilog setup, FluentValidation |

### Project Structure

```
order-management/
├── api/
│   ├── OrderManagement.Domain/
│   │   ├── Entities/          # Order, Product, OrderItem, AggregateRoot, BaseEntity
│   │   ├── ValueObjects/      # Money, Address
│   │   ├── Events/            # Domain events (OrderPlaced, Confirmed, Shipped, ...)
│   │   ├── Common/            # Result<T>, Error, DomainErrors, IDomainEvent
│   │   └── Repositories/      # IOrderRepository, IProductRepository
│   ├── OrderManagement.Application/
│   │   ├── Orders/            # OrderService + DTOs + Commands + Queries
│   │   └── Products/          # ProductService + DTOs
│   ├── OrderManagement.Infrastructure/
│   │   ├── Data/
│   │   │   ├── Configurations/    # EF entity configurations
│   │   │   ├── Migrations/        # EF migrations
│   │   │   ├── OrderDbContext.cs
│   │   │   └── DbSeeder.cs
│   │   ├── Repositories/          # OrderRepository, ProductRepository
│   │   └── Persistence/           # UnitOfWork
│   └── OrderManagement.Api/
│       ├── Endpoints/             # OrderEndpoints, ProductEndpoints
│       ├── Middleware/            # GlobalExceptionHandler
│       ├── Authorization/         # KeycloakAuthorizationHandler
│       ├── Validators/            # FluentValidation validators
│       ├── Mappers/               # Request/response mappers
│       └── Extensions/            # ServiceExtensions, WebApplicationExtensions
├── ui/
│   └── src/
│       ├── features/
│       │   ├── auth/              # Keycloak AuthProvider
│       │   ├── order-management/  # Orders CRUD + state transitions
│       │   └── product-management/# Products CRUD
│       ├── lib/
│       │   ├── api.ts             # Axios base client
│       │   └── keycloak.ts        # Keycloak instance
│       └── components/            # Shared UI components
├── keycloak/                      # Realm import config
├── docker-compose.yml
└── OrderManagement.sln
```

---

## Key Design Patterns

### Result Pattern
All service methods return `Result<T>` or `Result` — never throw for domain errors. Errors implicitly convert to `Result<T>`. Endpoints map failures via `ResultExtensions.ToProblem()`:
- `*.NotFound` → 404
- `*.InvalidState` / `*.InsufficientStock` → 409
- anything else → 400

### Order State Machine

```
Pending → Confirmed → Shipped → Delivered
   └─────────────────────────→ Cancelled  (blocked from Shipped/Delivered/Cancelled)
```

State transitions are enforced inside the `Order` aggregate and return `Result` failures on invalid transitions. `GlobalExceptionHandler` maps `DbUpdateConcurrencyException` → HTTP 409.

### Stock Management
`Order.AddItem()` calls `product.DeductStock()` inline — stock is reduced at order-creation time. `CancelOrderAsync` calls `product.RestoreStock()` for each item before cancelling. Both operations run inside `UnitOfWork.ExecuteInTransactionAsync` with EF retry strategy.

### Authentication & Authorization
JWT Bearer tokens validated against Keycloak. Fine-grained permissions use Keycloak UMA (`response_mode=decision`), with results cached for 30 seconds per user × permission. Supported policies:

| Policy | Resource |
|---|---|
| `order:confirm`, `order:ship`, `order:deliver`, `order:delete` | Order Resource |
| `product:create`, `product:update`, `product:delete` | Product Resource |

### EF Core Behaviors
- **Soft deletes**: global query filters exclude `IsDeleted = true` records automatically
- **`UpdatedAt` stamping**: auto-set on all modified `BaseEntity` entries in `SaveChangesAsync`
- **Domain events**: collected before save, dispatched after save succeeds (fire-and-forget via Serilog)
- **Concurrency tokens**: `DbUpdateConcurrencyException` → HTTP 409

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker + Docker Compose](https://docs.docker.com/get-docker/)

### Run with Docker Compose (full stack)

```bash
# Copy and edit environment variables
cp .env.example .env

# Start all services (Keycloak, SQL Server, API, UI)
docker compose up --build
```

| Service | URL |
|---|---|
| UI | http://localhost:3000 |
| API | http://localhost:8080 |
| API Reference (Scalar) | http://localhost:8080/scalar/v1 |
| Keycloak Admin | http://localhost:8180 |

Default Keycloak admin credentials: `admin` / `admin`

### Run locally (development)

**1. Start the database only:**

```bash
SA_PASSWORD=YourStrong@Password docker compose up db keycloak keycloak-db
```

**2. Run the API:**

```bash
dotnet run --project api/OrderManagement.Api
```

The API runs on `http://localhost:8080`. Migrations are applied automatically on startup (`MigrateOnStartup: true` in `appsettings.Development.json`). Products are seeded when `SeedOnStartup: true`.

**3. Run the UI:**

```bash
cd ui
npm install
npm run dev
```

The UI dev server runs on `http://localhost:5173` and connects to the API at `http://localhost:8080/api`.

---

## API Endpoints

### Orders

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/orders` | List all orders (paginated) |
| `GET` | `/api/orders/{id}` | Get order by ID |
| `GET` | `/api/orders/customer/{customerId}` | List orders for a customer (paginated) |
| `POST` | `/api/orders` | Place a new order |
| `POST` | `/api/orders/{id}/confirm` | Confirm order |
| `POST` | `/api/orders/{id}/ship` | Ship order |
| `POST` | `/api/orders/{id}/deliver` | Mark delivered |
| `POST` | `/api/orders/{id}/cancel` | Cancel order |
| `DELETE` | `/api/orders/{id}` | Soft-delete order (restores stock if Pending) |

### Products

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/products` | List products (paginated) |
| `POST` | `/api/products` | Create product |
| `PUT` | `/api/products/{id}` | Update product name, price, or stock |
| `DELETE` | `/api/products/{id}` | Soft-delete product |

---

## Development Commands

### Backend (.NET)

```bash
# Build solution
dotnet build OrderManagement.slnx

# Add EF Core migration
dotnet ef migrations add <MigrationName> \
  --project api/OrderManagement.Infrastructure \
  --startup-project api/OrderManagement.Api

# Apply migrations manually
dotnet ef database update \
  --project api/OrderManagement.Infrastructure \
  --startup-project api/OrderManagement.Api

# Publish release build
dotnet publish api/OrderManagement.Api/OrderManagement.Api.csproj \
  -c Release -o /app/publish
```

### Frontend

```bash
cd ui
npm run dev      # dev server with hot reload
npm run build    # production build (tsc + vite)
npm run lint     # ESLint
```

---

## Health Check

```
GET /health
```

Checks database connectivity via EF Core health check.
