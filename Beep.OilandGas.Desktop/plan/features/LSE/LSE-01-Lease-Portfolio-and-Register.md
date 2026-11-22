# LSE-01 — Lease portfolio and register

| Field | Value |
|---|---|
| Stage | STG-03a — Lease acquisition |
| Area | Lease |
| Status | Gate met - see [evidence](../../evidence/STG-03a-LSE-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LeaseAcquisition` |
| Desktop project | `Modules/Lease/Register/` |
| External packages | none |
| Depends on | WEL-01 |
| Blocks | LSE-02, LSE-03 |
| Web reference | `Pages/PPDM39/Operations/LeaseAcquisition.razor` (thin) |

## 1. Purpose

One register over every lease type, so land work is not scattered across four tables and one thin
page.

## 2. Source components

| Source | Reuse |
|---|---|
| `LeaseAcquisition/Services/LeaseManagementService.cs` | register reads/writes |
| `LeaseAcquisition/Core/Interfaces/ILeaseService.cs` | contract |
| Tables `LEASE`, `FEE_MINERAL_LEASE`, `GOVERNMENT_LEASE`, `NET_PROFIT_LEASE`, `R_LEASE_REFERENCE_CODE` | persistence |

## 3. Screens

`Lease → Register`

- Filter bar: lease type, state/region, operator, expiry window, status.
- Grid: lease number, type, name, acreage (+unit), state, operator, effective date, expiry,
  royalty rate, net revenue interest, status.
- Detail pane: identity, term, financials summary, parties, obligations, linked wells/fields.
- Actions: new (type chooser), edit, renew, expire, link to field or well, export.

## 4. Data and contracts

`OgLeaseSummary { Id, LeaseNumber, Kind, Name, Acreage, AcreageUnit, Region, Operator, Effective,
Expiry, RoyaltyRate, NetRevenueInterest, Status }`.

## 5. States

| State | Behaviour |
|---|---|
| No leases | "no leases recorded" with a create action |
| Expiring within 90 days | amber chip with the days remaining |
| Expired but active | red chip naming the conflict |
| Failed read | failed state, no empty register |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `lease.read` | no |
| Create/edit | `lease.write` | yes |
| Renew / expire | `lease.write` | yes |

## 7. Acceptance criteria

- AC-1 All four lease tables are visible from one register.
- AC-2 Acreage always carries a unit.
- AC-3 Expiry state is derived from dates and shown, not stored as a free-text status.
- AC-4 Linking a lease to a field records the actor and date.
- AC-5 Export matches the active filter.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LeaseRegister"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LeaseRegisterGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-03a-LSE-01/observed-evidence.json). The register is read and written
through `UnitofWork<T>` over the owning `LeaseAcquisition` entity models; it contains no statement text.

## 9. Open questions and risks

- Four separate tables with different columns: the register must show the union plus a
  type-specific detail pane, not flatten away differences.
