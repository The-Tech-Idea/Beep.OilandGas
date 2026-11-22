# ACC-08 — Tax workbench

| Field | Value |
|---|---|
| Stage | STG-10 — Production accounting and financials |
| Area | Accounting |
| Status | Gate met - see [evidence](../../evidence/STG-10-ACC-08/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Accounting`, `Beep.OilandGas.ProductionAccounting` |
| Desktop project | `Modules/Accounting/Tax/` |
| External packages | none |
| Depends on | ACC-02 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Accounting/TaxWorkbench.razor` |

## 1. Purpose

Compute and post tax: production tax, severance, corporate provision and deferred tax — per
jurisdiction, with the basis for each figure recorded.

## 2. Source components

| Source | Reuse |
|---|---|
| `Accounting/Services/TaxProvisionService.cs` | provision and deferred tax |
| `ProductionAccounting/Services/ProductionTaxService.cs` | production/severance tax |
| `Accounting/Services/JournalEntryService.cs` | posting |
| Jurisdiction reference data | rates and rules |

## 3. Screens

`Accounting → Tax`

- Jurisdiction selector with the configured rates and effective dates.
- Production tax panel: volumes, value, rate, tax, per jurisdiction.
- Provision panel: current tax, deferred tax, movement, reconciliation.
- Posting preview: accounts affected and amounts.
- Post and review.
- Rate table view (read-only; changes are configuration).

## 4. Data and contracts

`OgTaxComputation { Jurisdiction, Basis, Amount, Rate, Tax, Currency, EffectiveFrom }`.

## 5. States

| State | Behaviour |
|---|---|
| Missing rate for a jurisdiction | computation refused with the jurisdiction named |
| Fiscal year incomplete | provision refused |
| Rate effective date after the period | refused with the date |
| Currency mismatch | refused |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Compute | `accounting.tax.read` | no |
| Post | `accounting.tax.post` + SoD | yes |

## 7. Acceptance criteria

- AC-1 A computation names its jurisdiction, rate and effective date.
- AC-2 A missing rate blocks the computation.
- AC-3 Provision refuses when the fiscal year is incomplete.
- AC-4 Posting creates exactly one journal entry per computation.
- AC-5 Rates are never entered on this screen.
- AC-6 Author cannot post their own computation.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ACC-08"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-10-ACC-08/observed-evidence.json). Computations are read and written
through `UnitofWork<T>` over `TAX_TRANSACTION` and `DEFERRED_TAX_BALANCE`; the rates are read from `GOVERNMENTAL_TAX_DATA`;
and the postings go to the ledger at the configured accounts.

**The open question, answered from the two owner services.** They cover different halves and neither is a superset:

| Owner | What it supplies | What this feature takes |
|---|---|---|
| `ProductionTaxService` | a `TAX_TRANSACTION` per severance / ad valorem assessment, the rate normalisation, and the `REMARK` key=value contract | the two production kinds, `TaxMath.NormalizeRate`, and `TaxRemark` |
| `TaxProvisionService` | a current provision posted to income tax expense against income tax payable, and a `DEFERRED_TAX_BALANCE` whose movement posts | the two provision kinds and the deferred-movement rule |

Both are reproduced as four kinds rather than one being chosen over the other.

**Where the rates actually live.** `GOVERNMENTAL_TAX_DATA` has severance and ad valorem columns but **no jurisdiction column
and no corporate-rate column**. The owner carries both in `REMARK` as `key=value` tokens under `ProductionTaxRemarkKeys`
(`JURISDICTION=`, `DEFERRED_TAX_RATE=`), so that contract is read with the owner's own keys and separators rather than a new
column being invented. Every computation stores the exact key in `RATE_SOURCE`, which matters because the corporate key is
named `DEFERRED_TAX_RATE` even though the current provision uses it too - a naming oddity of the owner's, recorded rather
than hidden.

**A design conflict the gateway test caught.** The first version of SG-10-8's gate required **all twelve months, including
the period-end month**, to be closed. That makes a year-end provision impossible to post: the computation lands in the year
it provides for, and the ledger refuses a write into a closed period - the test failed with *"Period 2026-12 is closed, so
the computation cannot be posted into it."* The gate now requires every month to exist and every month **before** the
period-end month to be closed, leaving that month open because the provision is one of its closing entries. Whether the
posting period then accepts a write remains the ledger's own rule, so a locked book still refuses and says so.

**A second rule the test caught.** Measuring the prior deferred balance as strictly before the period end meant a
re-estimate for the same period measured against nothing and posted the whole balance again - 6,000 where the movement was
1,000. The prior is now the latest balance at or before the period end, so a re-estimate measures against the balance it
replaces.

**Two owner records were not registered.** `TAX_TRANSACTION` and `DEFERRED_TAX_BALANCE` existed in the owner but were absent
from the module's entity list, so the module did not provision tables for the two records this screen writes. Both are now
registered (smoke moved from 416 to 418 entity types), which is the additive fix - no table was created and none duplicated.

**AC-5 is structural, not a promise.** The rate table is displayed read-only, the view has no rate field of any kind and never
names `GOVERNMENTAL_TAX_DATA`, and the service only ever reads that table; the unit test asserts all three.

**Not built, and stated as gaps:** a non-calendar fiscal year (the owner has no fiscal-calendar table, so the gate assumes
the calendar year), a corporate rate that varies by income band or a minimum tax, taxable income derived from the revenue and
cost records (it is stated), temporary differences modelled per difference (the owner's `TrackTemporaryDifference` path is
not used), and returns, prior-year adjustments, withholding and the ETR reconciliation.

## 9. Open questions and risks

- The Web tax workbench is a shell; there is no behavioural precedent. Design from
  `TaxProvisionService` and `ProductionTaxService` and record the mapping.
