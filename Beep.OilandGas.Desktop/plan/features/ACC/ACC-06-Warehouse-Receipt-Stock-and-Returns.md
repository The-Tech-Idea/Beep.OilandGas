# ACC-06 — Warehouse, receipt stock and returns

| Field | Value |
|---|---|
| Stage | STG-10 — Production accounting and financials |
| Area | Accounting |
| Status | Gate met - see [evidence](../../evidence/STG-10-ACC-06/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Accounting` |
| Desktop project | `Modules/Accounting/Warehouse/` |
| External packages | none |
| Depends on | ACC-05 |
| Blocks | WRK-02 |
| Web reference | `Pages/PPDM39/Accounting/WarehouseWorkbench.razor`, `ReceiptStockWorkbench.razor`, `Components/BusinessProcess/Inventory*Panel.razor`, `Components/Workflow/CompanyMaterialReservationQueuePanel.razor` |

## 1. Purpose

Track materials from receipt to issue to return, with the stock value and the reservation queue
that maintenance depends on.

## 2. Source components

| Source | Reuse |
|---|---|
| `Accounting/Services/InventoryService.cs` (+8 partials) | stock |
| `ProductionAccounting/Services/InventoryService.cs`, `InventoryLcmService.cs` | lower-of-cost-or-market |
| `Accounting/Services/JournalEntryService.cs` | postings |
| `Components/BusinessProcess/InventoryLink*Panel.razor` | link/review behaviour |

## 3. Screens

`Accounting → Warehouse`

- Item grid: code, description, unit, on hand, reserved, available, value, location.
- Receipt: from a PO, with quantity, unit, condition, bin.
- Issue / reservation: to a work order or cost centre, with availability check.
- Return: from a work order, with condition and restock decision.
- Link panel: link an inventory item to an asset or equipment record.
- Valuation panel: cost, LCM adjustment, resulting journal.

## 4. Data and contracts

`OgStockItem { Code, Description, Unit, OnHand, Reserved, Available, Value, Currency, Location }`
`OgStockMovement { Kind, Item, Quantity, Unit, Reference, Date, Actor }`.

## 5. States

| State | Behaviour |
|---|---|
| Insufficient availability | refused with the short quantity |
| Reserved by another document | shown with the holder |
| Return in worse condition | restock decision required |
| Negative stock | refused |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Receive | `accounting.stock.write` | yes |
| Issue / reserve | `accounting.stock.write` | yes |
| Adjust | `accounting.stock.adjust` + reason | yes |

## 7. Acceptance criteria

- AC-1 Available = on hand − reserved, exactly.
- AC-2 An issue above availability is refused.
- AC-3 A reservation names its holding document.
- AC-4 A return requires an explicit restock decision.
- AC-5 Stock value changes produce a linked journal.
- AC-6 Negative stock is impossible.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ACC-06"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-10-ACC-06/observed-evidence.json). Items, movements, reservations,
adjustments and valuations are read and written through `UnitofWork<T>` over the ProductionAccounting inventory tables; the
value changes post to the ledger's `JOURNAL_ENTRY`/`JOURNAL_ENTRY_LINE`.

**The open question, answered.** The two inventory services do different jobs, and reading both settles it. The
**accounting** `InventoryService` says it "manages inventory items, stock transactions, and valuations" over
`INVENTORY_ITEM` and `INVENTORY_TRANSACTION`, and documents the postings this feature uses. The **ProductionAccounting** one
says it "manages tank and pipeline inventory ... tracks volume levels", which is crude/tank volume on `CRUDE_OIL_INVENTORY`
and `TANK_INVENTORY` - a different fact on different tables, not a rival answer. So a warehouse movement is an
`INVENTORY_TRANSACTION`, a deduction carries the **negative quantity** the owner's own `UseStockAsync` writes, and the page
prints which service owns the number.

**The reservation had to be added.** Nothing in the model held stock for a document, so "reserved" had no source and
"available" could not be explained. `INVENTORY_RESERVATION` records the holding document and its kind; `reserved` is the
**sum of the open reservations**, never a scalar on the item, so availability cannot drift from what the documents hold, and
the item grid can always answer "held by whom" (AC-3).

**The match is availability, not on-hand.** An issue is checked against `on hand - reserved` and the refusal names all three
numbers - the shortfall, the on hand and the reserved. That is deliberate: an issuer who expected more stock needs to see
that a work order is holding it, not just that the number is smaller than they thought (AC-2).

**The restock decision is what moves the value.** A return must state its condition and its decision. RESTOCK puts the
quantity and its value back (inventory debit, cost of goods sold credit); SCRAP or REPAIR records the decision and moves
neither, which is why the decision cannot be defaulted (AC-4).

**Every value change posts, and the entry is linked.** Receipt debits inventory and credits payables; an issue debits cost of
goods sold and credits inventory; a restocked return reverses that; a count posts against cost of goods sold; a
lower-of-cost-or-market write-down debits impairment loss. A movement with no value change stores no journal and its own text
says so, rather than leaving a null that could be read as a missing posting (AC-5).

**A posting error the gateway test caught.** The first run failed on the receipt's ledger lines: the service had debited
payables and credited inventory, the reverse of the owner's documented posting. No screen could have shown that; only
asserting the stored rows did. It is corrected, and recorded in the evidence because it is the sort of defect this stage is
most exposed to.

**Not built, and stated as gaps:** receipts do not re-average the unit cost (value rises by quantity x cost while the unit
cost stays as recorded, so costing is at the current unit cost rather than by layer); a revaluation only writes down, which
is IAS 2's asymmetry but means a recovery is not reflected; items cannot be created here; the asset link is two columns
rather than a validated reference; a reservation is spent whole when issued against and has no expiry; and there is no count
sheet, no variance approval and no transfer between warehouses.

## 9. Open questions and risks

- Two inventory services exist (accounting and production accounting). Establish one owner of the
  stock quantity and record it here.
