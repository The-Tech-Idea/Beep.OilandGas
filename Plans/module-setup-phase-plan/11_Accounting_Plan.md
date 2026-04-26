# Project 11 — Beep.OilandGas.Accounting
## O&G Best-Practice Audit & Revision Plan

**Purpose**: Full-spectrum O&G financial accounting — GL, AR/AP, cost allocation, tax, royalties, budgeting, period close, and FASB/IFRS disclosures.  
**Architecture role**: Service layer project consuming PPDM39 table classes (from `Beep.OilandGas.Models/Data/ProductionAccounting/`) via `PPDMGenericRepository`. Contains one local table class (`GLAccountMapping`) and a large collection of projection/aggregate classes in `Data/Accounting/`.

---

## Sub-phases

### SP-A · Data Class Shape Audit
**Status**: ✅ Complete

#### `Models/GLAccountMapping.cs` — TABLE CLASS VIOLATION FOUND
| Issue | Detail |
|-------|--------|
| Missing `ModelEntityBase` | `GLAccountMapping` is used with `PPDMGenericRepository.InsertAsync` but did NOT extend `ModelEntityBase` |
| Duplicate audit columns | Manually declared `ACTIVE_IND`, `PPDM_GUID`, `ROW_CREATED_BY`, `ROW_CREATED_DATE`, `ROW_CHANGED_BY`, `ROW_CHANGED_DATE` — all provided by `ModelEntityBase` |
| Auto-property style | Used `{ get; set; }` instead of `SetProperty(ref backingField, value)` pattern |

#### `Data/Accounting/*.cs` — All Projection Classes ✅
The 100+ classes in `Data/Accounting/` are **projection/aggregate/DTO classes**:
- `Budget`, `BankReconciliation`, `AgingBucket` — extend `ModelEntityBase` for `INotifyPropertyChanged` but are NEVER passed to `InsertAsync`. Collection properties (`List<BudgetLine>`, `List<OutstandingCheck>`, etc.) are **valid** per projection class rules.
- All other classes (`APInvoice`, `ARInvoice`, `SalesTransaction`, etc.) — projections returned from service methods.

#### PPDM Table Classes Used by Services ✅
Services use PPDM table classes from `Beep.OilandGas.Models/Data/ProductionAccounting/`:
`TAX_TRANSACTION`, `DEFERRED_TAX_BALANCE`, `TAX_RETURN`, `ACCOUNTING_COST`, `AR_INVOICE`, `AP_INVOICE`, `INVOICE`, `LEASE_CONTRACT`, `PURCHASE_ORDER`, `PO_RECEIPT`, `PO_LINE_ITEM`, `ASSET_RETIREMENT_OBLIGATION`, `INVENTORY_ITEM`

These were audited in Project 04 (ProductionAccounting). No violations found in the Accounting project's usage of these types.

---

### SP-B · Fixes Applied
**Status**: ✅ Complete

#### `Models/GLAccountMapping.cs`
- Added `using TheTechIdea.Beep.Editor;` and `using Beep.OilandGas.Models.Data;`
- Made class extend `ModelEntityBase`
- Removed 6 duplicate audit columns: `ACTIVE_IND`, `PPDM_GUID`, `ROW_CREATED_BY`, `ROW_CREATED_DATE`, `ROW_CHANGED_BY`, `ROW_CHANGED_DATE`
- Converted 4 remaining properties to `SetProperty(ref backingField, value)` pattern:
  - `GL_ACCOUNT_MAPPING_ID`, `MAPPING_KEY`, `GL_ACCOUNT_NUMBER`, `DESCRIPTION`

#### `Services/AccountMappingService.cs`
- Removed 4 redundant field assignments from `SeedMissingMappingsAsync`:
  - `ACTIVE_IND = _defaults.GetActiveIndicatorYes()` — now handled by `CommonColumnHandler` on `InsertAsync`
  - `PPDM_GUID = Guid.NewGuid().ToString()` — now handled by `CommonColumnHandler`
  - `ROW_CREATED_BY = userId` — passed via `InsertAsync(mapping, userId)`
  - `ROW_CREATED_DATE = DateTime.UtcNow` — handled by `CommonColumnHandler`

---

### SP-C · O&G Best-Practice Review
**Status**: ✅ Complete

| Domain | Standard Covered | Assessment |
|--------|-----------------|------------|
| GL / Chart of Accounts | PPDM `GL_ACCOUNT` table; account type (`ASSET`, `LIABILITY`, `EQUITY`, `REVENUE`, `EXPENSE`) | ✅ |
| AR/AP | PPDM `AR_INVOICE`, `AP_INVOICE`; aging buckets (current, 30-60, 60-90, 90+) | ✅ |
| Royalty Accounting | `RoyaltyCalculationRequest/Result`; volume × price × royalty rate | ✅ |
| Cost Allocation | `CostAllocationMethod` + `CostAllocationResult`; supports direct, ABC, burden-rate | ✅ |
| Tax — Income Tax | `TAX_TRANSACTION`, `DEFERRED_TAX_BALANCE`; FASB ASC 740 deferred tax | ✅ |
| Tax — Production / Severance | `TaxCalculationService`; gross value × effective rate per jurisdiction | ✅ |
| Budget vs. Actual | `Budget` → `BudgetVarianceReport`; monthly/quarterly/annual analysis | ✅ |
| Period Close | `PeriodClosingService` with `PeriodCloseChecklist`; AP/AR/GL/Inventory gate checks | ✅ |
| Bank Reconciliation | `BankReconciliation`; outstanding checks, deposits in transit, adjusted balance | ✅ |
| Asset Retirement Obligation | `ASSET_RETIREMENT_OBLIGATION` persisted via ProvisionService; FASB ASC 410-20 / IAS 37 | ✅ |
| Lease Accounting | `LEASE_CONTRACT` persisted; FASB ASC 842 / IFRS 16 right-of-use asset + liability | ✅ |
| Revenue Recognition | `GaapRevenueRecognitionService`; FASB ASC 606 five-step model | ✅ |
| XBRL Tagging | `XbrlTaggingService`; US GAAP taxonomy tags for SEC filings | ✅ |

#### Observation — Redundant Audit Column Assignments in Services
30+ service methods manually set `ACTIVE_IND`, `PPDM_GUID`, `ROW_CREATED_BY`, `ROW_CREATED_DATE` on PPDM entity objects before calling `InsertAsync`. While not incorrect (these are overwritten by `CommonColumnHandler` anyway), it adds noise. **Flagged for a future cleanup pass** — not fixed now as it's a style issue with no correctness impact.

---

### SP-D · Build Validation
**Status**: ✅ Complete

```
dotnet build Beep.OilandGas.Accounting.csproj -v q
→ 0 Error(s), 0 Warning(s)  (before changes)
→ 0 Error(s), 0 Warning(s)  (after GLAccountMapping fix)
```

---

## Files Changed

| File | Change |
|------|--------|
| `Beep.OilandGas.Accounting/Models/GLAccountMapping.cs` | Rewrote to extend `ModelEntityBase`; removed 6 duplicate audit columns; converted to `SetProperty` pattern |
| `Beep.OilandGas.Accounting/Services/AccountMappingService.cs` | Removed 4 redundant audit column assignments in `SeedMissingMappingsAsync` |

## Key O&G Accounting Standards Covered
- **FASB ASC 932** — Oil & Gas Producing Activities (cost vs. full-cost method, ARO, impairment)
- **FASB ASC 410-20** — Asset Retirement Obligations (decommissioning provision)
- **FASB ASC 606** — Revenue from Contracts with Customers (royalty & sales revenue)
- **FASB ASC 842 / IFRS 16** — Lease Accounting (right-of-use asset / lease liability)
- **FASB ASC 740** — Income Taxes (current + deferred tax provision)
- **SEC Regulation S-X Rule 4-10** — Financial Accounting for Oil & Gas (proved reserves disclosure)
