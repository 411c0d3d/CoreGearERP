# CoreGearERP Domain Model

This document covers the entity relationships, status progressions, cross-module flows, and the contracts and events that will be introduced as the system evolves. It is the reference for understanding what the system does and how the modules relate to each other.

---

## Module Overview

```
Procurement --> Inventory --> Production --> Sales
                    |                          |
                    └──────── Finance ─────────┘
```

- **Inventory** is the central module. Every other module reads from or writes to it.
- **Procurement** brings stock in.
- **Production** consumes stock and produces finished goods.
- **Sales** reserves and ships stock.
- **Finance** records the cost of every significant event across all modules.

---

## Inventory

### Entities

```
Warehouse
  has many StockItems

Product
  has many StockItems
  has many StockMovements

StockItem
  belongs to Product
  belongs to Warehouse
  QuantityOnHand    -- total physical stock
  QuantityReserved  -- held against open orders
  QuantityAvailable -- OnHand minus Reserved
  has many StockMovements

StockMovement (immutable, never updated or deleted)
  belongs to StockItem
  references source document by ReferenceId and ReferenceNumber
  MovementType determines direction and reason
```

### Status Progressions

```
Product:   Active --> Inactive --> Discontinued
Warehouse: Active --> Inactive
StockItem: no status progression, quantity changes via StockMovements only
StockMovement: Posted (created once, never changes)
```

### Rules

- Stock can never go negative
- StockMovements are the source of truth for all stock changes
- StockItem quantities are derived from StockMovements and cached on the entity
- A warehouse must be Active to receive stock
- A product must exist before a StockItem can be created

---

## Procurement

### Entities

```
Supplier
  has many PurchaseOrders

PurchaseOrder
  belongs to Supplier
  has many PurchaseOrderLines
  has many GoodsReceipts

PurchaseOrderLine
  belongs to PurchaseOrder
  references Product by Id (cross-module, no navigation property)
  Quantity ordered
  UnitPrice locked at creation -- supplier price changes do not affect open orders
  QuantityReceived updated on each GoodsReceipt

GoodsReceipt
  belongs to PurchaseOrder
  has many GoodsReceiptLines
  WarehouseId -- where stock landed
  ReceivedAt  -- UTC timestamp of receipt
  triggers StockMovement in Inventory on creation

GoodsReceiptLine
  belongs to GoodsReceipt
  references PurchaseOrderLine by Id
  references Product by Id
  QuantityReceived
  UnitPrice (Money) -- copied from PO line at receipt time
```

### Status Progressions

```
PurchaseOrder:
  Draft --> Confirmed --> PartiallyReceived --> Received
  Draft --> Cancelled
  Confirmed --> Cancelled (only if nothing received yet)

PurchaseOrderLine:
  Open --> PartiallyReceived --> Received
```

### Rules

- A PO must have at least one line item
- PO line quantity must be greater than zero
- Price is locked at PO creation
- Goods receipt quantity per line cannot exceed ordered quantity
- Supplier must be Active to raise a PO
- A confirmed PO cannot be cancelled if any goods have been received
- Each partial receipt against a PO line produces one GoodsReceipt document
- A GoodsReceipt is immutable once created -- no updates or deletes

---

## Production

### Entities

```
BillOfMaterials
  belongs to Product (the finished good being produced)
  has many BOMLines

BOMLine
  belongs to BillOfMaterials
  references Product (component) by Id (cross-module)
  Quantity required per production run

WorkCenter
  physical location where production happens
  has capacity

ProductionOrder
  references BillOfMaterials
  assigned to WorkCenter
  has many ProductionOrderLines (actual component consumption)
```

### Status Progressions

```
ProductionOrder:
  Draft --> Confirmed --> InProgress --> Completed --> Cancelled
  Draft --> Cancelled
  Confirmed requires: all components have sufficient available stock
  Completed is immutable -- no changes after completion
```

### Rules

- BOM must have at least one component
- BOM component quantity must be greater than zero
- A production order cannot be confirmed without sufficient component stock
- Component consumption is recorded at actual not planned -- variance is expected
- Completed production orders cannot be modified

---

## Sales

### Entities

```
Customer
  has many SalesOrders

SalesOrder
  belongs to Customer
  has many SalesOrderLines
  has many Shipments

SalesOrderLine
  belongs to SalesOrder
  references Product by Id (cross-module)
  Quantity ordered
  UnitPrice at time of order
  QuantityShipped updated on each Shipment

Shipment
  belongs to SalesOrder
  has many ShipmentLines
  triggers StockMovement in Inventory on creation (SalesShipment type)
```

### Status Progressions

```
SalesOrder:
  Draft --> Confirmed --> PartiallyShipped --> Shipped --> Cancelled
  Draft --> Cancelled
  Confirmed requires: stock reservation per line

Shipment:
  Pending --> Shipped --> Delivered
```

### Rules

- A sales order cannot be confirmed without a stock reservation per line
- Cancelling an order releases all reservations immediately
- Shipment quantity per line cannot exceed ordered quantity
- An order with a partial shipment remains open until fully shipped or explicitly cancelled
- Customer must be Active to place an order

---

## Finance

### Entities

```
CostEntry (immutable, append only like StockMovement)
  references source document (ProductionOrder, GoodsReceipt, Invoice)
  Amount + CurrencyCode
  belongs to a Period

Invoice
  Payable (belongs to Supplier) or Receivable (belongs to Customer)
  has many InvoiceLines
  has many Payments

Payment
  belongs to Invoice
  Amount cannot exceed invoice outstanding balance

Period
  Open or Closed
  No entries permitted into a closed period

GeneralLedger
  append only posting record
  references CostEntry or Invoice
```

### Status Progressions

```
Invoice:  Draft --> Issued --> PartiallyPaid --> Paid --> Cancelled
Payment:  Pending --> Cleared
Period:   Open --> Closed
```

### Rules

- Every cost entry must reference a source document
- Cost entries cannot be deleted, only reversed with a counter-entry
- Invoice total must equal the sum of its lines
- Payments cannot exceed the invoice outstanding balance
- No entries into a closed period

---

## Cross-Module Flows

### Goods Receipt Flow
```
Procurement: ReceiveGoodsCommand
  --> PurchaseOrderLine.QuantityReceived updated
  --> PurchaseOrder status updated (PartiallyReceived or Received)
  --> Inventory: StockItem.AddStock()           [in-process now, gRPC at M4]
  --> Inventory: StockMovement created (GoodsReceipt type)
  --> Finance: CostEntry created                [RabbitMQ event at M5]
```

### Sales Order Confirmation Flow
```
Sales: ConfirmSalesOrderCommand
  --> Inventory: check QuantityAvailable per line  [in-process now, gRPC at M4]
  --> Inventory: StockItem.Reserve() per line
  --> No StockMovement yet -- reservation only
```

### Shipment Flow
```
Sales: ShipOrderCommand
  --> Inventory: StockItem.RemoveStock()           [in-process now, gRPC at M4]
  --> Inventory: StockItem.ReleaseReservation()
  --> Inventory: StockMovement created (SalesShipment type)
  --> Finance: CostEntry + Invoice created         [RabbitMQ event at M5]
```

### Production Order Confirmation Flow
```
Production: ConfirmProductionOrderCommand
  --> Inventory: check component availability      [in-process now, gRPC at M4]
  --> Inventory: StockItem.Reserve() per component
```

### Production Order Completion Flow
```
Production: CompleteProductionOrderCommand
  --> Inventory: StockItem.RemoveStock() per component consumed
  --> Inventory: StockItem.AddStock() for finished good
  --> Inventory: StockMovements created (GoodsIssue + ProductionReceipt types)
  --> Finance: CostEntry created                   [RabbitMQ event at M5]
```

---

## What Becomes a gRPC Contract

Introduced when the first real cross-module synchronous call exists in the system. Currently in-process, replaced at M4.

| Caller | Contract | Provider |
|---|---|---|
| Procurement | GetStockLevel | Inventory |
| Sales | GetStockLevel | Inventory |
| Sales | ReserveStock | Inventory |
| Sales | ReleaseReservation | Inventory |
| Production | GetStockLevel | Inventory |
| Production | ReserveStock | Inventory |
| Production | ReleaseReservation | Inventory |

---

## What Becomes a RabbitMQ Event

Introduced when the first real async cross-module notification exists. Currently direct calls, replaced at M5.

| Publisher | Event | Subscriber | Trigger |
|---|---|---|---|
| Inventory | StockLevelLow | Procurement | QuantityOnHand drops below threshold |
| Procurement | GoodsReceived | Finance | GoodsReceipt created |
| Production | ProductionOrderCompleted | Finance, Inventory | ProductionOrder status set to Completed |
| Sales | SalesOrderShipped | Finance | Shipment status set to Shipped |

---

## Delivery Approach

The system is built in thin vertical slices. Each slice is a complete working flow from endpoint to database. Cross-module calls start in-process and are replaced with gRPC contracts and RabbitMQ events as the relevant milestone is reached.

The domain model and business rules defined here do not change. Only the communication mechanism between modules evolves.