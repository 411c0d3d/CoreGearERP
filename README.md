# CoreGearERP

> A manufacturing ERP platform built on .NET 10 and Angular. It covers the core operational chain of a manufacturing business -- inventory, procurement, production, sales, and finance -- wired together with gRPC contracts and event-driven messaging via RabbitMQ. Built to learn. Structured to scale.

**Stack:** .NET 10 | Angular | PrimeNG | RabbitMQ | gRPC | PostgreSQL | Redis  
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
  src/
    server/
      CoreGearERP.Host/           -- ASP.NET Core entry point, composition root
      CoreGearERP.Common/         -- Shared interfaces, value objects, base entities
      CoreGearERP.Inventory/      -- Inventory bounded context
      CoreGearERP.Procurement/    -- Procurement bounded context
      CoreGearERP.Production/     -- Production bounded context
      CoreGearERP.Sales/          -- Sales bounded context
      CoreGearERP.Finance/        -- Finance bounded context
    client/
      coregear-ui/                -- Angular SPA, scaffolded at M6
  tests/
    CoreGearERP.Inventory.Tests/
    CoreGearERP.Procurement.Tests/
    CoreGearERP.Production.Tests/
    CoreGearERP.Sales.Tests/
    CoreGearERP.Finance.Tests/
  docs/
    ADR.md                        -- Architecture Decision Records
    domain-model.md               -- Entity relationships, flows, contracts
    db-migration-steps.md         -- EF Core migration reference guide
```

---

## Domain Modules

| Module | Core Entities |
|---|---|
| Inventory | Product, StockItem, Warehouse, StockMovement |
| Procurement | Supplier, PurchaseOrder, PurchaseOrderLine, GoodsReceipt |
| Production | ProductionOrder, BillOfMaterials, WorkCenter |
| Sales | Customer, SalesOrder, SalesOrderLine, Shipment |
| Finance | CostEntry, Invoice, GeneralLedger |

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- Node.js 20+ (for Angular at M6)
- Rider or Visual Studio

### Local Infrastructure

Start PostgreSQL and Redis via Docker:

```bash
docker run -d --name coregear-postgres -e POSTGRES_USER=coregear -e POSTGRES_PASSWORD=coregear123 -e POSTGRES_DB=coregearerp -p 5432:5432 postgres:16-alpine
```

### Configuration

Copy `appsettings.example.json` and set up user secrets:

```bash
cd src/server/CoreGearERP.Host
dotnet user-secrets init
dotnet user-secrets set "Auth:SecretKey" "<your-secret-key>"
dotnet user-secrets set "Auth:Issuer" "coregear-api"
dotnet user-secrets set "Auth:Audience" "coregear-client"
dotnet user-secrets set "ConnectionStrings:Inventory" "Host=localhost;Port=5432;Database=coregearerp;Username=coregear;Password=coregear123;Search Path=inventory"
```

See `appsettings.example.json` for all required keys.

### Run Migrations

See `docs/db-migration-steps.md` for the full migration guide.

### Run the API

```bash
cd src/server/CoreGearERP.Host
dotnet run
```

API runs on `http://localhost:5014`.

### Get a Dev Token

```
POST http://localhost:5014/dev/token
```

Returns a JWT for local development. Use it as `Authorization: Bearer <token>` on all protected endpoints.

---

## Milestones

### M1 - Foundation and Infrastructure: Solution Structure, Authentication, Database, and Request Pipeline
- Modular solution structure with one class library per bounded context
- PostgreSQL with EF Core, one DbContext per module with separate schemas
- JWT authentication with tenant claim and global tenant scoping
- Custom CQRS dispatcher with pipeline behaviors (logging, validation)
- Result pattern -- exceptions never propagate through the middleware stack
- Serilog structured logging

**Done when:** register tenant, log in, hit a protected tenant-scoped endpoint

---

### M2 - Core Domain: Inventory and Procurement Operational Chain
- Product, Warehouse, StockItem entities with rich domain behaviour
- StockMovement as immutable append-only records
- PurchaseOrder with line items and status progression
- Goods receipt flow: PO confirmed -> goods received -> stock updated
- Value objects: Money, Quantity

**Done when:** create PO, receive goods, stock level updates

---

### M3 - Production and Sales
- ProductionOrder with BillOfMaterials
- Component availability check before confirming production order
- SalesOrder with line items and stock reservation on creation
- State machines for order progression

**Done when:** create sales order, reserve stock, confirm production order

---

### M4 - gRPC Contracts
- Define .proto contracts for cross-module queries
- Production calls Inventory via gRPC to check components
- Sales calls Inventory via gRPC to reserve stock
- gRPC client factory in DI

**Done when:** production order creation validates stock via gRPC, no direct DbContext calls across modules

---

### M5 - Event Driven Messaging and Caching
- RabbitMQ via Docker with MassTransit
- Outbox pattern for guaranteed at-least-once delivery
- Domain events with fan-out to multiple consumers
- IMemoryCache for reference data
- Read model projections for stock levels
- Redis for distributed cache

**Done when:** complete production order, Finance receives event and posts cost entries automatically

---

### M6 - Angular Frontend
- Angular standalone components with PrimeNG
- JWT auth flow with HTTP interceptor
- Inventory and production dashboards
- SignalR for real-time stock updates

**Done when:** full flow visible in browser, stock updates in real time

---

### M7 - Service Extraction
- Extract Inventory module to standalone .NET service
- All communication over gRPC and RabbitMQ only
- Docker Compose orchestration

**Done when:** Inventory runs as independent deployable, other modules talk to it only through contracts

---

## Architecture

Each module follows Clean Architecture with Pragmatic DDD:

```
CoreGearERP.{Module}/
  Domain/
    Entities/         -- rich domain entities with behaviour
    Enums/            -- module-specific status values
  Application/
    Commands/         -- write side, one file per use case
    Queries/          -- read side, one file per use case
    Contracts/        -- interfaces exposed to other modules
  Infrastructure/
    Persistence/
      Configurations/ -- EF Core entity type configurations
      Migrations/     -- EF Core generated migrations
    gRPC/             -- gRPC service implementations (Inventory, Production)
```

See `docs/ADR.md` for all architectural decisions and rationale.  
See `docs/Domain model.md` for entity relationships, cross-module flows, and event contracts.