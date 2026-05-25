# CoreGearERP -- EF Core and PostgreSQL Migration Guide

This guide covers everything you need to know about how EF Core migrations work in CoreGearERP, why certain decisions were made, and how to run migrations correctly for each module.

---

## How It All Fits Together

CoreGearERP uses one PostgreSQL database with a separate schema per module. Each module owns its schema completely -- no cross-schema joins in application code.

```
coregearerp (database)
  inventory   (schema) --> owned by CoreGearERP.Inventory
  procurement (schema) --> owned by CoreGearERP.Procurement
  production  (schema) --> owned by CoreGearERP.Production
  sales       (schema) --> owned by CoreGearERP.Sales
  finance     (schema) --> owned by CoreGearERP.Finance
```

Each module has its own `DbContext`. EF Core tracks migrations per DbContext so each module's schema evolves independently. A change in Inventory never touches the Procurement schema.

---

## Prerequisites

### Install EF Core CLI Tools

The `dotnet ef` CLI is a global tool. Install it once per machine.

```bash
dotnet tool install --global dotnet-ef
```

Verify it is installed:

```bash
dotnet ef --version
```

---

## PostgreSQL via Docker

### Start the Container

```bash
docker run -d --name coregear-postgres -e POSTGRES_USER=coregear -e POSTGRES_PASSWORD=coregear123 -e POSTGRES_DB=coregearerp -p 5432:5432 postgres:16-alpine
```

### Verify It Is Running

```bash
docker ps
```

You should see `coregear-postgres` in the list with port `5432` mapped.

### Stop and Start Again Later

```bash
docker stop coregear-postgres
docker start coregear-postgres
```

---

## Connection Strings

Each module gets its own connection string in `appsettings.json`. The `Search Path` tells PostgreSQL which schema to use by default for that connection.

```json
"ConnectionStrings": {
  "Inventory":   "Host=localhost;Port=5432;Database=coregearerp;Username=coregear;Password=coregear123;Search Path=inventory",
  "Procurement": "Host=localhost;Port=5432;Database=coregearerp;Username=coregear;Password=coregear123;Search Path=procurement",
  "Production":  "Host=localhost;Port=5432;Database=coregearerp;Username=coregear;Password=coregear123;Search Path=production",
  "Sales":       "Host=localhost;Port=5432;Database=coregearerp;Username=coregear;Password=coregear123;Search Path=sales",
  "Finance":     "Host=localhost;Port=5432;Database=coregearerp;Username=coregear;Password=coregear123;Search Path=finance"
}
```

---

## The Design Time Factory Problem

When you run `dotnet ef migrations add`, the EF Core CLI needs to instantiate your `DbContext` to inspect the model. The problem is that `InventoryDbContext` requires `ICurrentTenant` which is resolved from an HTTP request JWT claim. There is no HTTP request at design time so it fails.

EF Core provides a solution -- `IDesignTimeDbContextFactory<T>`. If EF Core finds a class implementing this interface in the same project as the DbContext, it uses that to create the DbContext instead of trying to resolve it through DI.

### The Second Problem -- Schema Does Not Exist Yet

When EF Core runs the first migration it tries to create the `__EFMigrationsHistory` table. In PostgreSQL this table goes into the schema specified by `Search Path`. But if the schema does not exist yet, PostgreSQL throws:

```
3F000: no schema has been selected to create in
```

The fix is to create the schema before EF Core tries to run anything. The `IDesignTimeDbContextFactory` is the right place to do this.

### The Factory -- One Per Module

Each module needs its own factory. The pattern is identical across all modules, only the schema name and DbContext type change.

#### Inventory

```csharp
using CoreGearERP.Common.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace CoreGearERP.Inventory.Infrastructure.Persistence;

/// <summary>
/// Design time factory for EF Core CLI tools.
/// Only used by dotnet ef commands -- never instantiated at runtime.
/// Creates the inventory schema if it does not exist before running migrations.
/// </summary>
public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=coregearerp;Username=coregear;Password=coregear123";

    public InventoryDbContext CreateDbContext(string[] args)
    {
        EnsureSchemaExists("inventory");

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql($"{ConnectionString};Search Path=inventory")
            .Options;

        return new InventoryDbContext(options, new DesignTimeCurrentTenant());
    }

    private static void EnsureSchemaExists(string schema)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA IF NOT EXISTS {schema};";
        command.ExecuteNonQuery();
    }

    /// <summary>Stub tenant used only during design time. TenantId is irrelevant for migrations.</summary>
    private class DesignTimeCurrentTenant : ICurrentTenant
    {
        public Guid TenantId => Guid.Empty;
    }
}
```

Copy this pattern for each module, replacing `inventory`, `InventoryDbContext`, and the namespace.

---

## The Global Query Filter Problem

The original approach was to apply a global query filter in `OnModelCreating` using a captured variable from `_currentTenant`:

```csharp
// This does not work -- EF Core cannot translate a captured variable
var tenantId = _currentTenant?.TenantId ?? Guid.Empty;
modelBuilder.ApplyGlobalFilters<BaseEntity>(e => e.TenantId == tenantId && !e.IsDeleted);
```

EF Core rejects this because the expression references a captured local variable it cannot evaluate at query build time. The error is:

```
The filter expression is invalid. The expression must accept a single parameter
of type 'Product' and return 'bool'.
```

### The Fix -- Apply Filters Per Entity Configuration

Instead of a global filter, apply the query filter directly in each entity's `IEntityTypeConfiguration`. EF Core can translate this correctly because the expression is self-contained.

```csharp
// In ProductConfiguration.cs
// Soft delete filter -- excludes deleted records from all queries automatically.
// Tenant filtering is handled at the query level in command and query handlers.
builder.HasQueryFilter(p => !p.IsDeleted);
```

Tenant filtering is enforced at the handler level using `ICurrentTenant` injected into the handler. This keeps the EF Core configuration simple and avoids design time issues entirely.

---

## The DbContext -- One Per Module

Each module's DbContext sets its own schema and applies configurations from its own assembly.

```csharp
using CoreGearERP.Common.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Infrastructure.Persistence;

/// <summary>EF Core DbContext scoped to the Inventory module. Owns the inventory schema only.</summary>
public class InventoryDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // All tables for this module go into the inventory schema.
        modelBuilder.HasDefaultSchema("inventory");

        // Scans this assembly for all IEntityTypeConfiguration implementations
        // and applies them automatically. One configuration file per entity.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
```

---

## Entity Configuration

Each entity gets its own `IEntityTypeConfiguration` file in `Infrastructure/Persistence/Configurations/`. This keeps mapping logic out of the DbContext and makes each entity's schema explicit.

```csharp
using CoreGearERP.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGearERP.Inventory.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping configuration for the Product entity.</summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(p => p.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(p => p.UnitCode).HasColumnName("unit_code").HasMaxLength(10).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(p => p.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(p => p.ModifiedAt).HasColumnName("modified_at").IsRequired();
        builder.Property(p => p.ModifiedBy).HasColumnName("modified_by").IsRequired();
        builder.Property(p => p.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(p => p.CompletedAt).HasColumnName("completed_at");
        builder.Property(p => p.CancelledAt).HasColumnName("cancelled_at");

        // Code is unique per tenant, not globally.
        builder.HasIndex(p => new { p.TenantId, p.Code })
            .IsUnique()
            .HasDatabaseName("ix_products_tenant_code");

        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("ix_products_tenant_id");

        // Soft delete filter applied per entity.
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
```

---

## Running Migrations

Always run migration commands from inside the module directory. Never from the Host or solution root.

The `--startup-project` flag points to the Host so EF Core can find the app configuration. The factory inside the module handles the actual DbContext creation.

---

### Inventory

```bash
cd src/server/CoreGearERP.Inventory

dotnet ef migrations add <MigrationName> --context InventoryDbContext --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj --output-dir Infrastructure/Persistence/Migrations

dotnet ef database update --context InventoryDbContext --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj
```

### Procurement

```bash
cd src/server/CoreGearERP.Procurement

dotnet ef migrations add <MigrationName> --context ProcurementDbContext --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj --output-dir Infrastructure/Persistence/Migrations

dotnet ef database update --context ProcurementDbContext --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj
```

### Production

```bash
cd src/server/CoreGearERP.Production

dotnet ef migrations add <MigrationName> --context ProductionDbContext --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj --output-dir Infrastructure/Persistence/Migrations

dotnet ef database update --context ProductionDbContext --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj
```

### Sales

```bash
cd src/server/CoreGearERP.Sales

dotnet ef migrations add <MigrationName> --context SalesDbContext --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj --output-dir Infrastructure/Persistence/Migrations

dotnet ef database update --context SalesDbContext --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj
```

### Finance

```bash
cd src/server/CoreGearERP.Finance

dotnet ef migrations add <MigrationName> --context FinanceDbContext --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj --output-dir Infrastructure/Persistence/Migrations

dotnet ef database update --context FinanceDbContext --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj
```

---

## Rollback and Remove

### Roll Back to a Specific Migration

```bash
dotnet ef database update <PreviousMigrationName> --context <ModuleDbContext> --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj
```

### Roll Back Everything

```bash
dotnet ef database update 0 --context <ModuleDbContext> --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj
```

### Remove the Last Migration

Only works if the migration has not been applied to the database yet:

```bash
dotnet ef migrations remove --context <ModuleDbContext> --startup-project ..\CoreGearERP.Host\CoreGearERP.Host.csproj
```

---

## Verify the Database

```bash
# List all schemas
docker exec -it coregear-postgres psql -U coregear -d coregearerp -c "\dn"

# List tables per schema
docker exec -it coregear-postgres psql -U coregear -d coregearerp -c "\dt inventory.*"
docker exec -it coregear-postgres psql -U coregear -d coregearerp -c "\dt procurement.*"
docker exec -it coregear-postgres psql -U coregear -d coregearerp -c "\dt production.*"
docker exec -it coregear-postgres psql -U coregear -d coregearerp -c "\dt sales.*"
docker exec -it coregear-postgres psql -U coregear -d coregearerp -c "\dt finance.*"
```

---

## Rules

- Never edit generated migration files manually
- Never run migrations from the Host or solution root directory
- Always use `--output-dir` so migrations land in the correct module folder
- Each module has its own factory -- copy the pattern exactly, change only the schema name and DbContext type
- Replace `<MigrationName>` with a descriptive name -- `InitialInventory`, `AddWarehouseEntity`, `AddSupplierContactColumn`