# CoreGear

> A manufacturing ERP platform built on .NET 10 and Angular. It covers the core operational chain of a manufacturing business -- inventory, procurement, production, sales, and finance -- wired together with gRPC contracts and event-driven messaging via RabbitMQ. Built to learn. Structured to scale.

Stack: .NET 10 | Angular | PrimeNG | RabbitMQ | gRPC | PostgreSQL | Redis  
Approach: Monolith first, extract one service at the end

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

## Milestones

### M1 - Foundation
- Solution structure, one class library per module
- PostgreSQL + EF Core, one DbContext per module, separate schemas
- JWT auth with tenant claim
- EF Core global query filters for tenancy and soft delete
- FluentValidation pipeline behavior
- Serilog

**Done when:** register tenant, log in, hit a protected tenant-scoped endpoint

---

### M2 - Inventory and Procurement
- Product, StockItem, Warehouse entities
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
- Basic state machines for order progression
- CQRS: commands mutate, queries read, no shared handlers

**Done when:** create sales order, reserve stock, confirm production order

---

### M4 - gRPC Contracts
- Define .proto contracts for cross-module queries
  - Inventory.GetStockLevel
  - Inventory.ReserveStock
  - Production.GetOrderStatus
- Production calls Inventory via gRPC to check components
- Sales calls Inventory via gRPC to reserve stock
- gRPC client factory in DI

**Done when:** production order creation validates stock via gRPC, no direct DbContext calls across modules

---

### M5 - Event Driven Messaging and Caching
- RabbitMQ via Docker
- MassTransit on top for consumer registration, retries, dead letters
- Outbox table, background worker publishes to exchange
- Domain events:
  - StockLevelLow -> Procurement notification
  - ProductionOrderCompleted -> Finance cost entry
  - SalesOrderShipped -> customer notification placeholder
- Finance consumes ProductionOrderCompleted and posts cost entries
- IMemoryCache for reference data (products, units of measure, currencies)
- Read model projection for stock levels updated via RabbitMQ events, not cached directly
- Redis via Docker for distributed cache (user permissions, session data)

**Done when:** complete production order, Finance receives event and posts cost entries automatically, stock dashboard reads from projection not live query

---

### M6 - Angular Frontend
- Angular standalone components
- PrimeNG for all UI components (tables, forms, dialogs, menus)
- PrimeFlex for layout and spacing
- JWT auth flow with HTTP interceptor
- Inventory dashboard with stock levels using PrimeNG Table
- Production order creation form using PrimeNG reactive form components
- ngx-echarts for stock trend and production output charts
- SignalR client for real-time stock updates
  - RabbitMQ consumer pushes to SignalR hub
  - Angular reflects changes without refresh

**Done when:** full flow visible in browser, stock updates in real time

---

### M7 - Service Extraction
- Extract Inventory module to standalone .NET service
- All communication now over gRPC and RabbitMQ only
- Docker Compose with both services, RabbitMQ, PostgreSQL, Redis
- No shared DbContext, no shared memory

**Done when:** Inventory runs as independent deployable, other modules talk to it only through contracts

---

## ADRs

### ADR-001: Monolith First
**Decision:** One deployable host, modules as class libraries, extract Inventory at M7  
**Why:** Bounded contexts are interrelated. Forcing services early creates distributed transaction problems before you understand the domain.  
**Tradeoff:** Single deployment unit. A fault in one module can affect others.

---

### ADR-002: gRPC for Inter-Module Sync Communication
**Decision:** All synchronous cross-module calls use gRPC with Protobuf contracts. No direct DbContext sharing.  
**Why:** Typed contracts catch breaking changes at compile time. REST does not. When Inventory is extracted at M7, the contracts stay the same.  
**Tradeoff:** More setup than REST. Not browser-friendly, internal only.

---

### ADR-003: RabbitMQ + MassTransit + Outbox
**Decision:** RabbitMQ self-hosted via Docker. MassTransit as .NET abstraction. Outbox pattern for guaranteed delivery.  
**Why:** Self-hosted means you understand every part. Outbox ensures events survive broker downtime. MassTransit handles retry and dead letter routing without boilerplate.  
**Tradeoff:** Consumers must be idempotent. At-least-once delivery means duplicates are possible.

---

### ADR-004: PostgreSQL
**Decision:** PostgreSQL for all modules, separate schema per module. Develop locally via Docker, deploy to Azure Database for PostgreSQL.  
**Why:** Open source, strong ACID guarantees for financial data, JSONB for custom fields later, widely used in German market.  
**Tradeoff:** No SQL Server specific EF Core features.

---

### ADR-005: Angular + PrimeNG
**Decision:** Angular standalone components, PrimeNG 21 for UI components, PrimeFlex for layout, ngx-echarts for charts, HttpClient with interceptors, SignalR client.  
**Why:** Angular is dominant in German enterprise hiring, especially ERP and finance. PrimeNG covers 80+ components out of the box including the data tables, forms, and dialogs an ERP needs. Used in production by SAP, Mercedes, and Lufthansa. Fully free with no license wall. ngx-echarts handles time series and operational charts cleanly without bloat.  
**Tradeoff:** PrimeNG major versions track Angular major versions closely. Pin them together and do not let them drift.

---

### ADR-006: Caching Strategy
**Decision:** Three-layer approach. IMemoryCache for reference data. Read model projections for stock levels. Redis for distributed cache (permissions, session).  
**Why:** Stock levels must not be cached directly -- a stale value causes oversell. The read model is kept current by RabbitMQ events so it is fast without the staleness risk. IMemoryCache is enough for data that rarely changes. Redis only where cache must survive a process restart or be shared across instances.  
**Tradeoff:** Read model is eventually consistent. There is a brief window between a stock movement and the projection updating. Acceptable for dashboards, not acceptable for the actual reservation check which always hits the write model via gRPC.
