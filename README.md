# Order Management — Architecture Showcase

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![React 19](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)
![Keycloak](https://img.shields.io/badge/Keycloak-26-4D4D4D?logo=keycloak)

A showcase project demonstrating production-grade patterns with .NET 10 (ASP.NET Core Minimal API) and React 19 + TypeScript: Clean Architecture, Domain-Driven Design (DDD), CQRS without MediatR, API-first development with OpenAPI 3.1 + NSwag code generation, and fine-grained Keycloak JWT/UMA authorization — all containerized with Docker Compose.

---

## Tech Stack

### Backend

| Technology | Purpose |
|---|---|
| .NET 10 / ASP.NET Core Minimal API | Web framework |
| Entity Framework Core 10 + SQL Server | ORM + database |
| Keycloak 26 (JWT + UMA) | Authentication & authorization |
| FluentValidation | Request validation |
| Serilog | Structured logging (console + rolling JSON file) |
| Scalar | API reference UI (dev only) |
| NSwag | OpenAPI → C# DTO code generation |
| Central Package Management (`Directory.Packages.props`) | Unified NuGet version control |

### Frontend

| Technology | Purpose |
|---|---|
| React 19 + TypeScript 5 | UI framework |
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

| Layer | Project | Responsibility |
|---|---|---|
| **Domain** | `OrderManagement.Domain` | Pure C# — `Order` and `Product` aggregates, `Money`/`Address` value objects, `Result<T>`/`Error` types, `DomainErrors`, repository interfaces |
| **Application** | `OrderManagement.Application` | CQRS handlers, `IDispatcher`, OpenAPI contracts, generated DTOs, mappers, `IUnitOfWork`. No EF Core dependency |
| **Infrastructure** | `OrderManagement.Infrastructure` | `OrderDbContext`, EF configurations, repository implementations, `UnitOfWork`, `DbSeeder` |
| **Api** | `OrderManagement.Api` | Minimal API endpoints, Keycloak JWT/UMA auth, `GlobalExceptionHandler`, Serilog setup, FluentValidation |

### Project Structure

```
order-management/
├── api/
│   ├── Directory.Packages.props           # Central NuGet version management
│   ├── OrderManagement.slnx               # Solution file
│   ├── OrderManagement.Domain/
│   │   ├── Entities/          # Order, Product, OrderItem, AggregateRoot, BaseEntity
│   │   ├── ValueObjects/      # Money, Address
│   │   ├── Events/            # Domain events (OrderPlaced, Confirmed, Shipped, ...)
│   │   ├── Common/            # Result<T>, Error, DomainErrors, IDomainEvent
│   │   └── Repositories/      # IOrderRepository, IProductRepository
│   ├── OrderManagement.Application/
│   │   ├── Common/
│   │   │   ├── Dispatching/   # IDispatcher, Dispatcher
│   │   │   ├── Helpers/       # EnumMapper
│   │   │   └── Interfaces/    # ICommand, ICommandHandler, IQuery, IQueryHandler, IUnitOfWork
│   │   ├── DependencyInjection.cs         # Handler registrations
│   │   ├── Orders/
│   │   │   ├── Commands/      # PlaceOrder, ConfirmOrder, ShipOrder, DeliverOrder, CancelOrder, DeleteOrder
│   │   │   │   └── OrderCommandsGenerated.g.cs   # NSwag-generated request/response DTOs
│   │   │   ├── Contracts/                 # OpenAPI 3.1 schema files (source of truth)
│   │   │   │   ├── orders-management.yaml
│   │   │   │   └── order-query.yaml
│   │   │   ├── Queries/       # GetAllOrders, GetOrderById, GetCustomerOrders
│   │   │   │   └── OrderQueriesGenerated.g.cs    # NSwag-generated query DTOs
│   │   │   └── Mappers/       # OrderMapper (Domain → DTO)
│   │   └── Products/
│   │       ├── Commands/      # CreateProduct, UpdateProduct, DeleteProduct
│   │       │   └── ProductCommandsGenerated.g.cs # NSwag-generated request/response DTOs
│   │       ├── Contracts/                 # OpenAPI 3.1 schema files (source of truth)
│   │       │   ├── products-management.yaml
│   │       │   └── product-query.yaml
│   │       ├── Queries/       # GetAllProducts
│   │       │   └── ProductQueriesGenerated.g.cs  # NSwag-generated query DTOs
│   │       └── Mappers/       # ProductMapper (Domain → DTO)
│   ├── OrderManagement.Infrastructure/
│   │   ├── Data/
│   │   │   ├── Configurations/    # EF entity configurations
│   │   │   ├── Migrations/        # EF migrations
│   │   │   └── OrderDbContext.cs
│   │   ├── DependencyInjection.cs         # Infrastructure service registrations
│   │   ├── Persistence/           # UnitOfWork
│   │   └── Repositories/          # OrderRepository, ProductRepository
│   └── OrderManagement.Api/
│       ├── Authorization/         # KeycloakAuthorizationHandler
│       ├── Endpoints/             # OrderEndpoints, ProductEndpoints
│       ├── Extensions/            # ServiceExtensions, WebApplicationExtensions, ResultExtensions
│       ├── Middleware/            # GlobalExceptionHandler
│       └── Validators/            # FluentValidation validators
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
└── docker-compose/
    ├── docker-compose.yml
    └── .env
```

---

## CQRS Pattern

The Application layer uses a lightweight **CQRS** (Command Query Responsibility Segregation) implementation without an external library.

### Core Abstractions

```csharp
// A command that returns TResponse
public interface ICommand<TResponse>;

// Handles a specific command
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken ct = default);
}

// A query that returns TResponse
public interface IQuery<TResponse>;

// Handles a specific query
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken ct = default);
}
```

### Dispatcher

`IDispatcher` is the single entry point used by endpoints to dispatch commands and queries. It resolves the correct handler from DI at runtime using reflection:

```csharp
public interface IDispatcher
{
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct = default);
    Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct = default);
}
```

### Example — Placing an Order

```csharp
// Command definition (Application layer)
public record PlaceOrderCommand(PlaceOrderRequest Request, string PlacedBy)
    : ICommand<Result<OrderResponse>>;

// Endpoint (Api layer)
var result = await dispatcher.SendAsync(new PlaceOrderCommand(request, username), ct);
```

### Handlers Registered

| Type | Handler |
|---|---|
| `PlaceOrderCommand` | `PlaceOrderHandler` |
| `ConfirmOrderCommand` | `ConfirmOrderHandler` |
| `ShipOrderCommand` | `ShipOrderHandler` |
| `DeliverOrderCommand` | `DeliverOrderHandler` |
| `CancelOrderCommand` | `CancelOrderHandler` |
| `DeleteOrderCommand` | `DeleteOrderHandler` |
| `GetAllOrdersQuery` | `GetAllOrdersHandler` |
| `GetOrderByIdQuery` | `GetOrderByIdHandler` |
| `GetCustomerOrdersQuery` | `GetCustomerOrdersHandler` |
| `CreateProductCommand` | `CreateProductHandler` |
| `UpdateProductCommand` | `UpdateProductHandler` |
| `DeleteProductCommand` | `DeleteProductHandler` |
| `GetAllProductsQuery` | `GetAllProductsHandler` |

---

## API-First Design

The Application layer owns **OpenAPI 3.1 YAML contracts** as the single source of truth for all request/response types. DTOs are never written by hand — they are generated from the schema at build time.

### OpenAPI Contracts

| File | Operations |
|---|---|
| `orders-management.yaml` | PlaceOrder, ConfirmOrder, ShipOrder, DeliverOrder, CancelOrder, DeleteOrder |
| `order-query.yaml` | GetAllOrders (paginated), GetOrder, GetCustomerOrders (paginated) |
| `products-management.yaml` | CreateProduct, UpdateProduct, DeleteProduct |
| `product-query.yaml` | GetProducts (paginated) |

### DTO Code Generation (NSwag)

Each contract is referenced in `OrderManagement.Application.csproj` as an `<OpenApiReference>`. On every build, NSwag generates the corresponding `.g.cs` file:

```xml
<OpenApiReference Include="Orders/Contracts/orders-management.yaml"
                  Namespace="OrderManagement.Application.Orders.Commands"
                  OutputPath="Orders/Commands/OrderCommandsGenerated.g.cs"
                  CodeGenerator="NSwagCSharp">
  <Options>
    /GenerateClientClasses:false
    /GenerateClientInterfaces:false
    /GenerateDtoTypes:true
    /JsonLibrary:SystemTextJson
    /GenerateDefaultValues:true
    /GenerateNullableReferenceTypes:true
    /UseBaseUrl:false
    /DateType:System.DateTimeOffset
    /DateTimeType:System.DateTimeOffset
    /ArrayType:System.Collections.Generic.ICollection
    /ArrayInstanceType:System.Collections.Generic.List
  </Options>
</OpenApiReference>
```

Generated files follow the `*.g.cs` naming convention and are excluded from manual editing. Any change to the API contract is propagated automatically on the next build.

---

## Central Package Management

All NuGet package versions are declared once in `api/Directory.Packages.props`. Individual `.csproj` files reference packages without version numbers, ensuring every project in the solution uses consistent dependency versions.

```xml
<!-- Directory.Packages.props -->
<PropertyGroup>
  <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
</PropertyGroup>
<ItemGroup>
  <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
  <PackageVersion Include="NSwag.ApiDescription.Client" Version="14.6.3" />
  <!-- ... all other packages -->
</ItemGroup>
```

---

## Key Design Patterns

### Result Pattern

All handlers return `Result<T>` or `Result` — never throw for domain errors. Errors implicitly convert to `Result<T>`. Endpoints map failures via `ResultExtensions.ToProblem()`:

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

`Order.AddItem()` calls `product.DeductStock()` inline — stock is reduced at order-creation time. `CancelOrderHandler` calls `product.RestoreStock()` for each item before cancelling. Both operations run inside `UnitOfWork.ExecuteInTransactionAsync` with EF retry strategy.

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

### Environment Setup

The project uses two `.env` files — one for Docker Compose services and one for the UI dev server.

#### 1. `docker-compose/.env`

Create `docker-compose/.env` with the following values:

```env
# ─── SQL Server ───────────────────────────────────────────────────────────────
SA_PASSWORD=YourStr0ng!Pass

# ─── Connection Strings ───────────────────────────────────────────────────────
ConnectionStrings__DefaultConnection=Server=db,1433;Database=OrderManagement;User Id=sa;Password=YourStr0ng!Pass;TrustServerCertificate=True

# ─── Keycloak ─────────────────────────────────────────────────────────────────
KC_DB_PASSWORD=keycloak
KC_ADMIN=admin
KC_ADMIN_PASSWORD=admin
KC_HOSTNAME=localhost
KC_HTTP_PORT=8180

# ─── Keycloak App Settings ────────────────────────────────────────────────────
Keycloak__Authority=http://localhost:8180/realms/order-management
Keycloak__MetadataAddress=http://keycloak:8180/realms/order-management/.well-known/openid-configuration
Keycloak__TokenEndpoint=http://keycloak:8180/realms/order-management/protocol/openid-connect/token
Keycloak__Audience=order-api
Keycloak__ClientId=order-api
Keycloak__ClientSecret=order-api-secret

ASPNETCORE_ENVIRONMENT=Development
```

| Variable | Description |
|---|---|
| `SA_PASSWORD` | SQL Server SA password — must match the password in `ConnectionStrings__DefaultConnection` |
| `ConnectionStrings__DefaultConnection` | Connection string to the SQL Server container |
| `KC_DB_PASSWORD` | PostgreSQL password for the Keycloak database |
| `KC_ADMIN` / `KC_ADMIN_PASSWORD` | Keycloak admin credentials |
| `KC_HOSTNAME` | Public hostname of Keycloak (used for redirect URIs) |
| `KC_HTTP_PORT` | Keycloak HTTP port |
| `Keycloak__Authority` | Issuer URL for JWT validation (resolved from the browser/client side) |
| `Keycloak__MetadataAddress` | OpenID Connect discovery endpoint (resolved from inside the API container using the internal hostname `keycloak`) |
| `Keycloak__TokenEndpoint` | Token endpoint (resolved from inside the API container using the internal hostname `keycloak`) |
| `Keycloak__Audience` | Expected `aud` claim in the JWT |
| `Keycloak__ClientId` | Client ID used for UMA permission checks |
| `Keycloak__ClientSecret` | Client secret used for UMA permission checks |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment (`Development` / `Production`) |

#### 2. `ui/.env.local`

Create `ui/.env.local` for the UI dev server:

```env
VITE_API_BASE_URL=http://localhost:8080/api
```

| Variable | Description |
|---|---|
| `VITE_API_BASE_URL` | API base URL used by the Axios client in the UI |

---

### Run with Docker Compose (full stack)

```bash
# Create the .env file first (see Environment Setup above)
cd docker-compose

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

**1. Start infrastructure only:**

```bash
cd docker-compose
SA_PASSWORD=YourStrong@Password docker compose up db keycloak keycloak-db
```

**2. Run the API:**

```bash
dotnet run --project api/OrderManagement.Api
```

The API runs on `http://localhost:8080`. Migrations are applied automatically on startup when running in the `Development` environment.

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
| `POST` | `/api/orders/{id}/confirm` | Confirm order (`order:confirm`) |
| `POST` | `/api/orders/{id}/ship` | Mark order as shipped (`order:ship`) |
| `POST` | `/api/orders/{id}/deliver` | Mark order as delivered (`order:deliver`) |
| `POST` | `/api/orders/{id}/cancel` | Cancel order |
| `DELETE` | `/api/orders/{id}` | Soft-delete order, restores stock if Pending (`order:delete`) |

### Products

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/products` | List products (paginated) |
| `POST` | `/api/products` | Create product (`product:create`) |
| `PUT` | `/api/products/{id}` | Update product name, price, or stock (`product:update`) |
| `DELETE` | `/api/products/{id}` | Soft-delete product (`product:delete`) |

---

## Development Commands

### Backend (.NET)

```bash
# Build solution (also triggers NSwag DTO generation)
dotnet build api/OrderManagement.slnx

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
