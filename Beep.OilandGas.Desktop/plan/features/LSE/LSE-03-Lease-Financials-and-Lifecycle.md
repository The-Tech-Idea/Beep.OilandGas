# LSE-03 — Lease financials and lifecycle

| Field | Value |
|---|---|
| Stage | STG-03a — Lease acquisition |
| Area | Lease |
| Status | Gate met - AC-1..AC-5 met; AC-3 is consumed by the WFL-05 task inbox. Evidence: [STG-03a-LSE-03](../../evidence/STG-03a-LSE-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LeaseAcquisition`, `Beep.OilandGas.Accounting` (`LeaseAccountingService`) |
| Desktop project | `Modules/Lease/Financials/` |
| External packages | none |
| Depends on | LSE-02 |
| Blocks | ACC-07 |
| Web reference | none |

## 1. Purpose

The money side of a lease: what was paid, what is owed, what the net revenue interest really is,
and when it expires or must be renewed.

## 2. Source components

| Source | Reuse |
|---|---|
| `LeaseAcquisition/Services/LeaseAcquisitionService.Financial.cs` | bonus, rental, obligations |
| `LeaseAcquisition/Services/LeaseAcquisitionService.Lifecycle.cs` | renew, expire, relinquish |
| `Accounting/Services/LeaseAccountingService.cs` | accounting treatment |
| `ProductionAccounting/Services/RoyaltyService.cs` | royalty linkage (read-only) |

## 3. Screens

`Lease → Financials`

- Terms card: bonus, rental schedule, royalty rate, net revenue interest, overriding royalty,
  burdens.
- Obligation list: obligation, due date, minimum spend, status.
- Payment history grid.
- Lifecycle actions: renew (with term and consideration), relinquish, expire — each with rationale.
- Expiry calendar with 30/60/90-day buckets.

## 4. Data and contracts

`OgLeaseTerms { Bonus, BonusCurrency, RentalSchedule[], RoyaltyRate, NetRevenueInterest,
OverridingRoyalty, Burdens[] }`
NRI arithmetic: `decimal`, refuse unrepresentable values, refuse a sum outside [0, 1] unless the
lease type explicitly permits it.

## 5. States

| State | Behaviour |
|---|---|
| NRI unrepresentable | refused with the operation named |
| Obligation overdue | red chip with days overdue |
| Renewal pending | retained original on lost acknowledgement |
| Financial read failed | failed state; no zeroed figures |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit terms | `lease.financial.write` | yes |
| Record payment | `lease.financial.write` | yes |
| Renew / relinquish | `lease.lifecycle.write` | yes |

## 7. Acceptance criteria

- AC-1 NRI arithmetic refuses an unrepresentable result.
- AC-2 Royalty rate is stored as a fraction with an explicit precision.
- AC-3 Expiring leases appear in the task inbox (WFL-05).
- AC-4 Renewal creates a new term and retains the old one.
- AC-5 Overdue obligations are computed from dates and shown, not stored.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LeaseFinancial"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LeaseFinancialGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-03a-LSE-03/observed-evidence.json). The financial records are read
and written through `UnitofWork<T>` over the LeaseAcquisition-owned entities; they contain no statement text.

## 9. Open questions and risks

- `LeaseAccountingService` may already post journals on lease events; if so this screen must not
  duplicate posting. Confirm and record the boundary with ACC-07.
