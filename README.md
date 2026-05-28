# CoreGearERP

> A manufacturing ERP platform built on .NET 10 and Angular. It covers the core operational chain of a manufacturing business -- inventory, procurement, production, sales, and finance -- wired together with gRPC contracts and event-driven messaging via RabbitMQ. Built to learn. Structured to scale.

**Stack:** .NET 10 | Angular | PrimeNG | RabbitMQ | gRPC | PostgreSQL  
**Architecture:** Modular Monolith | Pragmatic DDD | Clean Architecture per module | CQRS  
**Approach:** Monolith first, extract one service at the end

---

## Repository Structure

```
CoreGearERP/
  CoreGearERP.sln
  README.md
  appsettings.example.json
  .gitignore
  .dockerignore
  compose.yaml
  docs/
    ADR.md                        -- Architecture Decision Records
    Domain model.md               -- Entity relationships, flows, contracts
    DB migrations.md              -- EF Core migration reference guide
  src/
    server/
      CoreGearERP.Host/           -- ASP.NET Core entry point, composition root
      CoreGearERP.Common/         -- Shared interfaces, value objects, base entities
      CoreGearERP.Inventory/      -- Inventory bounded context
      CoreGearERP.Procurement/    -- Procurement bounded context
      CoreGearERP.Production/     -- Production bounded context
      CoreGearERP.Sales/          -- Sales bounded context
      CoreGearERP.Finance/        -- Finance bounded context
      CoreGearERP.Messaging/      -- Shared MassTransit outbox context
      CoreGearERP.Tests/          -- Integration tests, Testcontainers + WebApplicationFactory
    client/
      coregear-ui/                -- Angular SPA, scaffolded at M7
```

---

## Domain Modules

| Module | Core Entities |
|---|---|
| Inventory | Product, StockItem, Warehouse, StockMovement |
| Procurement | Supplier, PurchaseOrder, PurchaseOrderLine, GoodsReceipt |
| Production | ProductionOrder, BillOfMaterials, WorkCenter |
| Sales | Customer, SalesOrder, SalesOrderLine, Shipment |
| Finance | CostEntry, FinancialPeriod |

---

## The Three Operational Cycles

**Procure-to-Pay:** Supplier -> PurchaseOrder -> GoodsReceipt -> StockMovement -> CostEntry

**Plan-to-Produce:** BillOfMaterials -> ProductionOrder -> ComponentConsumption -> FinishedGoodsReceipt -> CostEntry

**Order-to-Cash:** Customer -> SalesOrder -> StockReservation -> Shipment -> StockMovement -> CostEntry

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- Node.js 22+ (for Angular at M7)
- Rider or Visual Studio

### Local Infrastructure

Start PostgreSQL and RabbitMQ via Docker Compose:

```bash
docker compose up -d
```

Or start PostgreSQL manually:

```bash
docker run -d --name coregear-postgres \
  -e POSTGRES_USER=coregear \
  -e POSTGRES_PASSWORD=coregear123 \
  -e POSTGRES_DB=coregearerp \
  -p 5432:5432 postgres:16-alpine
```

### Configuration

Copy `appsettings.example.json` to `appsettings.Development.json` and fill in your values, or use user secrets:

```bash
cd src/server/CoreGearERP.Host
dotnet user-secrets init
dotnet user-secrets set "Auth:SecretKey" "<your-secret-key-min-32-chars>"
dotnet user-secrets set "ConnectionStrings:CoreGearERP" "Host=localhost;Port=5432;Database=coregearerp;Username=coregear;Password=coregear123"
dotnet user-secrets set "RabbitMq:Host" "localhost"
dotnet user-secrets set "RabbitMq:Port" "5672"
dotnet user-secrets set "RabbitMq:Username" "guest"
dotnet user-secrets set "RabbitMq:Password" "guest"
```

### Run Migrations

See `docs/DB migrations.md` for the full migration guide.

### Run the API

```bash
cd src/server/CoreGearERP.Host
dotnet run
```

API runs on `http://localhost:5014`. gRPC runs on `http://localhost:5015`.

### Get a Dev Token

```
POST http://localhost:5014/dev/token
```

Returns a JWT for local development. Use it as `Authorization: Bearer <token>` on all protected endpoints.

### Run Tests

```bash
dotnet test src/server/CoreGearERP.Tests/CoreGearERP.Tests.csproj
```

Tests use Testcontainers -- Docker must be running. Postgres and RabbitMQ containers start automatically.

---

## Milestones

### M1 -- Foundation and Infrastructure
Solution structure, authentication, database, and request pipeline.

- Modular solution with one class library per bounded context
- PostgreSQL with EF Core, one DbContext per module with separate schemas
- JWT authentication with tenant claim and global tenant scoping
- Custom CQRS dispatcher with pipeline behaviors (logging, validation)
- Result pattern -- exceptions never propagate through the middleware stack
- Serilog structured logging

**Done when:** register tenant, log in, hit a protected tenant-scoped endpoint

---

### M2 -- Core Domain: Inventory and Procurement
Inventory and procurement operational chain.

- Product, Warehouse, StockItem entities with rich domain behaviour
- StockMovement as immutable append-only records
- PurchaseOrder with line items and status progression
- Goods receipt flow: PO confirmed -> goods received -> stock updated
- Value objects: Money, Quantity

**Done when:** create PO, receive goods, stock level updates

---

### M3 -- Production and Sales

- ProductionOrder with BillOfMaterials
- Component availability check before confirming production order
- SalesOrder with line items and stock reservation on confirmation
- Shipment flow with stock reduction

**Done when:** create sales order, reserve stock, ship goods, confirm and complete production order

---

### M4 -- gRPC Contracts

- `.proto` contracts for all cross-module inventory queries and commands
- All Procurement, Production, and Sales cross-module inventory calls go through gRPC
- Kestrel serves HTTP on port 5014 and gRPC HTTP/2 on port 5015
- In-process implementations remain for gRPC server-side injection

**Done when:** production order creation validates stock via gRPC, no direct DbContext calls across modules

---

### M5 -- Event-Driven Messaging and Outbox

- RabbitMQ via Docker with MassTransit
- Outbox pattern for guaranteed at-least-once delivery
- Shared `OutboxDbContext` in `CoreGearERP.Messaging` -- single outbox for all publishing modules
- Finance consumers for GoodsReceived, ProductionOrderCompleted, SalesOrderShipped
- Financial period management via explicit API

**Done when:** complete production order, Finance receives event and posts cost entries automatically

---

### M6 -- Integration Tests

- Full HTTP-level integration test suite using Testcontainers and WebApplicationFactory
- Single `CoreGearERP.Tests` project covering all modules
- Collection-scoped Postgres and RabbitMQ containers
- Per-test data isolation via `/test/reset` endpoint
- CI pipeline with GitHub Actions

**Done when:** all non-outbox tests pass in CI, pipeline blocks on failure

---

### M7 -- Angular Frontend

- Angular standalone components with PrimeNG 21
- JWT auth flow with HTTP interceptor
- Inventory and production dashboards
- Real-time stock updates via SignalR

**Done when:** full operational flow visible in browser

---

### M8 -- Service Extraction

- Extract Inventory module to standalone .NET service
- All communication over gRPC and RabbitMQ only
- Docker Compose orchestration for multi-service deployment

**Done when:** Inventory runs as independent deployable, all modules communicate through contracts only

---

## Architecture

Each module follows Clean Architecture with Pragmatic DDD:

```
CoreGearERP.{Module}/
  Domain/
    Entities/         -- rich domain entities with behaviour
    Enums/            -- module-specific status values
  Application/
    {Entity}/         -- one folder per entity, all commands, queries, handlers
    Contracts/        -- interfaces exposed to other modules
  Infrastructure/
    Persistence/
      Configurations/ -- EF Core entity type configurations
      Migrations/     -- EF Core generated migrations
    gRPC/             -- gRPC service implementations (Inventory only)
    Messaging/        -- MassTransit consumers
  Extensions/
    {Module}Endpoints.cs
    {Module}Extensions.cs
```

See `docs/ADR.md` for all architectural decisions and rationale.  
See `docs/Domain model.md` for entity relationships, cross-module flows, and event contracts.