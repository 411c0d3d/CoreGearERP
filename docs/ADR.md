# Architecture Decision Records

---

## ADR-000: Repository and Solution Structure

**Status:** Accepted

**Decision:** Single monorepo containing both server and client. Server is a modular monolith. Client is an Angular SPA. Both live under one `CoreGearERP` repository.

**Full repository structure:**

```
CoreGearERP/
  CoreGearERP.sln
  README.md
  appsettings.example.json
  .gitignore
  src/
    server/
      CoreGearERP.Host/
      CoreGearERP.Common/
      CoreGearERP.Inventory/
      CoreGearERP.Procurement/
      CoreGearERP.Production/
      CoreGearERP.Sales/
      CoreGearERP.Finance/
    client/
      coregear-ui/              -- Angular SPA, scaffolded at M6
  tests/
    CoreGearERP.Inventory.Tests/
    CoreGearERP.Procurement.Tests/
    CoreGearERP.Production.Tests/
    CoreGearERP.Sales.Tests/
    CoreGearERP.Finance.Tests/
  docs/
    ADR.md
    DB migration.md
    Domain model.md
```

Note: `client/` and remaining test projects do not exist yet. They are shown here for clarity and will be added at their respective milestones.

**Why:** One repo means one PR for changes that touch both backend and frontend. No cross-repo coordination overhead. Simpler for a single developer or small team. At M7 when Inventory is extracted it stays in the same repo as a second server project -- still one repo, now two deployables.

**Tradeoff:** As the team grows, a monorepo requires discipline around ownership boundaries. Tooling like Nx can help at that point but is not needed now.

---

## ADR-001: Monolith First, Extract Later

**Status:** Accepted

**Decision:** One deployable host. Modules are separate class libraries within a single host. Inventory is extracted to a standalone service at M7.

**Why:** The bounded contexts are deeply interrelated. Production depends on Inventory, Finance depends on Production. Forcing them into separate services before understanding the domain creates distributed transaction complexity too early. A modular monolith enforces the same boundary discipline at the code level without the operational overhead.

**Tradeoff:** Single deployment unit. A fault in one module can affect others. Mitigation: each module has its own DbContext and exception handling boundary.

---

## ADR-002: Pragmatic DDD as Domain Modeling Approach

**Status:** Accepted

**Decision:** CoreGearERP uses a Pragmatic DDD approach -- taking the high-value parts of Domain-Driven Design without the full ceremony. This is a deliberate choice, not an oversight.

**What we apply:**

- **Rich entities with behaviour** -- state changes go through domain methods, never external setters. `Product.Deactivate()` enforces the rule that a discontinued product cannot be deactivated. The domain protects its own invariants.
- **Value objects** -- `Money` and `Quantity` are immutable, equality by value, no identity. A bare decimal for a price or quantity is not allowed anywhere in the codebase.
- **Factory methods** -- private constructors, entity creation only through `Create()`. An entity cannot exist in an invalid state.
- **Domain exceptions** -- `DomainException` and `NotFoundException` thrown from inside the domain when a rule is violated. The application layer does not invent business rules.
- **Bounded contexts** -- each module owns its schema, its DbContext, and its domain model. No cross-module DbContext sharing. Cross-module references are by Id only, never by navigation property.

**What we deliberately skip:**

- **Formal aggregate roots** -- we have not explicitly defined aggregate boundaries. Entities that act as roots are implicit. The added formality does not pay off at this scale.
- **Repository pattern** -- handlers call DbContext directly. Repositories add an abstraction layer that rarely gets swapped out in practice and makes the code harder to read without meaningful benefit here.
- **Domain events inside entities** -- in strict DDD, `Product.Create()` would raise a `ProductCreated` domain event internally collected by the aggregate. We use RabbitMQ events at the application layer instead. The outcome is the same, the mechanism is more visible and easier to reason about.
- **Ubiquitous language sessions** -- there are no domain experts to align with. The language used in the code is informed by manufacturing ERP conventions.

**Why Pragmatic DDD and not full DDD:**

Full DDD earns its complexity when the domain is genuinely deep, the team is large, and the system will be maintained for years with changing requirements. CoreGearERP is a learning platform. Applying full DDD ceremony would mean writing more infrastructure than domain logic and spending more time on patterns than on understanding the actual business problem.

The goal is to understand pressure points, not to produce a textbook DDD implementation.

**Why not abandon DDD entirely:**

Anemic domain models -- entities with only getters and setters, all logic in service classes -- are the most common cause of ERP codebases becoming unmaintainable. Business rules scatter across handlers, validators, and services with no single source of truth. Rich domain models with encapsulated behaviour prevent this. That value is worth keeping regardless of scale.

**The label:**

Pragmatic DDD. Domain-influenced architecture. If asked in an interview: Clean Architecture per module with DDD-influenced domain modeling. Deliberate decisions were made about what to apply and what to skip, and those decisions can be explained.

---

## ADR-003: Internal Module Architecture

**Status:** Accepted

**Decision:** Each module follows Clean Architecture without ceremony. Three layers per module -- Domain, Application, Infrastructure. No fourth presentation layer; that responsibility belongs to the Host project.

```
CoreGearERP.Inventory/
  Domain/
    Entities/
    ValueObjects/
    Enums/
    Exceptions/
  Application/
    Commands/
    Queries/
    Contracts/        -- interfaces this module exposes to others
  Infrastructure/
    Persistence/
      InventoryDbContext.cs
      Configurations/
      Migrations/
    gRPC/
      InventoryGrpcService.cs
```

**Layer rules:**
- Domain has zero framework dependencies -- no EF Core, no MassTransit, no ASP.NET
- Application depends on Domain only -- defines interfaces, never implements them
- Infrastructure depends on Application and Domain -- implements interfaces, owns DbContext
- Host depends on all modules and is the composition root -- registers everything into DI

**Why not pure Onion:** Four rings with interfaces for everything adds ceremony without adding value at this scale. Things that will never be swapped out do not need an interface.

**Why not pure Vertical Slice:** Vertical slice organises by feature rather than layer. Works well for pure APIs but scatters the domain model across features. In an ERP where entities are shared across many features, a coherent domain layer is easier to reason about.

**Why this hybrid:** You get a clean domain model, testable application logic, and swappable infrastructure without the overhead of full onion ceremony. The boundary discipline comes from module separation, not from ring enforcement within a module.

**Tradeoff:** Developers need to agree on what belongs in each layer. Without that agreement the layers drift. The rule is simple -- if it touches a framework it belongs in Infrastructure.

### Folder Structure Per Module

```
CoreGearERP.{Module}/
  Domain/
    Entities/         -- domain entities, inherit from BaseEntity
    Enums/            -- module-specific status values and enumerations
  Application/
    Commands/         -- one file per write use case, mutates state
    Queries/          -- one file per read use case, never mutates
    Contracts/        -- interfaces this module exposes to other modules
  Infrastructure/
    Persistence/
      Configurations/ -- EF Core entity type configurations, one per entity
      Migrations/     -- EF Core generated migrations, never edit manually
    gRPC/             -- gRPC service implementation (Inventory and Production only)
```

### CoreGearERP.Common Structure

```
CoreGearERP.Common/
  Domain/
    Entities/         -- BaseEntity, inherited by every entity in every module
    ValueObjects/     -- Money, Quantity, shared across all modules
    Exceptions/       -- DomainException, NotFoundException
    Enums/            -- enumerations shared across modules
  Application/
    Interfaces/       -- ICurrentTenant, ICurrentUser
```

### CoreGearERP.Host Structure

```
CoreGearERP.Host/
  Extensions/         -- module registration extension methods
  Middleware/         -- tenant resolution, exception handling
```

### Layer Rules

- Domain has zero framework dependencies -- no EF Core, no MassTransit, no ASP.NET references
- Application depends on Domain only -- defines interfaces, never implements them
- Infrastructure depends on Application and Domain -- implements interfaces, owns DbContext
- Host depends on all modules -- composition root only, no business logic
- Modules never reference each other -- cross-module calls go through Contracts interfaces resolved via gRPC

### Folder Creation Rules

- `ValueObjects/` and `Exceptions/` are not created inside modules by default -- add them only when a module needs something not covered by Common
- `gRPC/` only exists in Inventory and Production at M4
- `Migrations/` stays empty until the first EF Core migration at end of M1
- A new feature always gets a Command or Query file, never logic added to an existing handler

---

## ADR-004: gRPC for Inter-Module Synchronous Communication

**Status:** Accepted

**Decision:** All synchronous cross-module calls use gRPC with Protobuf contracts. No direct DbContext sharing across module boundaries under any circumstance.

**Why:** Protobuf contracts are typed and versioned. Breaking changes are caught at compile time. REST with JSON does not give you this. When Inventory is extracted at M7 the contracts remain unchanged -- only the transport changes from in-process to network.

**Tradeoff:** More setup than REST. Not browser-friendly so it is internal only. REST remains the external API for Angular.

---

## ADR-005: RabbitMQ + MassTransit + Outbox Pattern

**Status:** Accepted

**Decision:** RabbitMQ self-hosted via Docker. MassTransit as the .NET abstraction layer. Outbox pattern for guaranteed at-least-once delivery.

**Why:** Self-hosted means full visibility into every part of the messaging infrastructure. The outbox pattern ensures events are not lost if the broker is unavailable -- the event is written to the database in the same transaction as the state change and dispatched by a background worker. MassTransit handles consumer registration, retry policies, and dead letter routing without boilerplate.

**Tradeoff:** Consumers must be idempotent. At-least-once delivery means duplicates are possible and must be handled. Dead letter queues require monitoring.

**Outbox flow:**
1. State change and OutboxMessage written in single DB transaction
2. Background worker polls OutboxMessage for unprocessed records
3. Worker publishes to RabbitMQ exchange
4. Worker marks OutboxMessage as processed
5. Consumers receive and process independently
6. Failed consumers retry per MassTransit policy then move to dead letter queue

---

## ADR-006: PostgreSQL

**Status:** Accepted

**Decision:** PostgreSQL for all modules, separate schema per module. Develop locally via Docker, deploy to Azure Database for PostgreSQL Flexible Server.

**Why:** Open source with no licensing cost. Strong ACID guarantees for financial data. JSONB column support for custom fields when needed. Widely used in the German enterprise market. EF Core support via Npgsql is mature.

**Tradeoff:** No SQL Server specific EF Core features. Connection string swap between local Docker and Azure is the only change needed at deploy time.

---

## ADR-007: Angular + PrimeNG

**Status:** Accepted

**Decision:** Angular standalone components. PrimeNG 21 for UI components. PrimeFlex for layout and spacing. ngx-echarts for charts. HttpClient with interceptors for auth. SignalR client for real-time updates.

**Why:** Angular dominates German enterprise frontend hiring, especially in ERP and finance contexts. PrimeNG covers 80+ components including the data tables, forms, and dialogs an ERP needs without a license wall. Used in production by SAP, Mercedes, and Lufthansa. ngx-echarts handles time series and operational charts cleanly without significant bundle bloat.

**Tradeoff:** PrimeNG major versions track Angular major versions closely. They must be pinned together and not allowed to drift.

---

## ADR-008: Caching Strategy

**Status:** Accepted

**Decision:** Three-layer approach depending on data characteristics.

| Layer | Tool | Used For |
|---|---|---|
| Reference data | IMemoryCache | Products, units of measure, currencies -- rarely changes |
| Stock levels | Read model projection | Updated via RabbitMQ events, not cached directly |
| Distributed cache | Redis | User permissions, session data -- must survive process restart |

**Why:** Stock levels must never be served from a simple cache. A stale value causes oversell. The read model projection is kept current by RabbitMQ domain events so it is fast without staleness risk. IMemoryCache is sufficient for data that changes infrequently. Redis is added only where the cache must be shared across instances or survive a restart.

**Tradeoff:** The read model is eventually consistent. There is a brief window between a stock movement and the projection updating. This is acceptable for dashboards. The actual stock reservation check always hits the write model via gRPC -- never the projection.

---



### Business Rules by Module

**Inventory**
- Stock can never go negative -- reservation must be validated before any outbound movement
- Stock movements are immutable -- never update or delete, compensate with a counter-movement
- A product must exist before a stock item can be created
- A warehouse location must be active to receive stock
- Quantity and unit of measure must be consistent per product

**Procurement**
- A purchase order must have at least one line item
- PO line quantity must be greater than zero
- A PO can only be cancelled if not fully received
- Goods receipt quantity cannot exceed ordered quantity per line
- Supplier must be active to raise a PO against them
- Price on a PO line is locked at creation -- supplier price changes do not affect open orders

**Production**
- A production order cannot be confirmed if any required component has insufficient stock
- Bill of Materials must have at least one component with quantity greater than zero
- A production order cannot be started if status is not Confirmed
- Completed production orders are immutable
- Component consumption is recorded at actual not planned

**Sales**
- A sales order cannot be confirmed without a stock reservation per line
- Cancelling an order releases all reservations immediately
- A customer must be active to place an order
- Shipment quantity cannot exceed ordered quantity per line
- An order with a partial shipment remains open until fully shipped or explicitly cancelled

**Finance**
- Every cost entry must reference a source document (production order, goods receipt, invoice)
- Financial periods can be locked -- no entries permitted into a closed period
- Cost entries cannot be deleted, only reversed with a counter-entry
- Invoice total must equal the sum of its lines
- Payments cannot exceed the invoice amount

**Cross-Cutting**
- Every entity belongs to a tenant -- no cross-tenant data access under any circumstance
- Soft delete only -- nothing is hard deleted
- Every state transition is timestamped and attributed to a user
- Money is always decimal with currency code -- never float
- Quantities are always stored with their unit of measure -- never a bare number

---

## Data Model Conventions

### Base Entity Shape

Every table carries these columns without exception:

```
Id              uuid            PRIMARY KEY
TenantId        uuid            NOT NULL, indexed
Status          varchar         NOT NULL
IsDeleted       bool            NOT NULL DEFAULT false
CreatedAt       timestamptz     NOT NULL
CreatedBy       uuid            NOT NULL
ModifiedAt      timestamptz     NOT NULL
ModifiedBy      uuid            NOT NULL
```

### Business Keys

Every entity with a human-facing identifier gets a separate business key alongside the UUID primary key. Business keys are unique per tenant, not globally. UUID is the join key. Business key is what humans read.

```
Id                    uuid      PRIMARY KEY
PurchaseOrderNumber   varchar   UNIQUE per tenant
```

### Money

Every monetary value is two columns, always stored together:

```
Amount          decimal(18,4)   NOT NULL
CurrencyCode    char(3)         NOT NULL   -- ISO 4217: EUR, USD, GBP
```

Never a single decimal column. Never a float.

### Quantity

Every quantity value is two columns, always stored together:

```
Quantity        decimal(18,4)   NOT NULL
UnitCode        varchar         NOT NULL   -- KG | PCS | LTR | MTR
```

### Status Transition Timestamps

For entities with meaningful state transitions, record when each transition happened alongside the Status column:

```
ConfirmedAt     timestamptz     -- null until confirmed
CompletedAt     timestamptz     -- null until completed
CancelledAt     timestamptz     -- null until cancelled
```

### Deliberately Deferred

Real patterns held back until the code actually needs them:

| Pattern | Add When |
|---|---|
| RowVersion / optimistic concurrency | Concurrent edit conflicts surface in testing |
| Extensions JSONB column | Custom fields become a real requirement at M5 |
| Temporal pricing tables | Pricing module is built |
| DocumentSequence table | Invoicing module is built |
| Reference data tables | Each module needs them organically |

---

## ADR-009: Authentication and Identity Strategy

**Status:** Accepted -- Production target. Dev token endpoint used during local development only.

**Decision:** Azure Entra ID (formerly Azure Active Directory) as the identity provider for production. External identity login via Microsoft and Google accounts. Multi-tenant with granular role-based permissions per tenant.

**Local Development:** A `/dev/token` endpoint in `CoreGearERP.Host` generates JWT tokens with fixed identities read from `appsettings.Development.json`. This endpoint is removed before any production deployment and must never be present in a production build.

**Production Flow:**
1. User authenticates via Azure Entra ID -- Microsoft or Google login
2. Entra ID issues a JWT with tenant and role claims
3. CoreGearERP validates the JWT using Entra ID's public keys
4. `ICurrentTenant` and `ICurrentUser` resolve from the validated claims
5. Every request is scoped to the tenant in the token

**Multi-tenancy in Entra ID:**
- Each tenant in CoreGearERP maps to an Entra ID tenant or an app registration with tenant-specific claims
- Tenant isolation is enforced at the EF Core query level via `TenantId` on every entity
- Role assignments are managed per tenant -- a user can be an admin in one tenant and read-only in another

**Granular Permissions:**
- Roles defined per module -- `Inventory.Read`, `Inventory.Write`, `Production.Confirm`, `Finance.Post`
- Claims-based authorization in ASP.NET Core
- Resource-based authorization for record-level ownership checks

**Why Entra ID:**
- Standard in German enterprise market, especially Microsoft stack shops
- Native support for Microsoft and Google login via external identity providers
- Managed service -- no auth infrastructure to maintain
- Integrates directly with Azure RBAC if the deployment target is Azure

**Tradeoff:** Entra ID adds Azure vendor dependency for auth. If the deployment target changes to on-prem or another cloud, Keycloak is the self-hosted alternative with the same OAuth2/OIDC standards and a similar feature set.