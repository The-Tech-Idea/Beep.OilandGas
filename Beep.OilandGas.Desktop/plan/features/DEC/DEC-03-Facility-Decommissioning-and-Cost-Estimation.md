# DEC-03 — Facility decommissioning and cost estimation

| Field | Value |
|---|---|
| Stage | STG-12 — Decommissioning |
| Area | Decommissioning |
| Status | Gate met - see [evidence](../../evidence/STG-12-DEC-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Decommissioning` |
| Desktop project | `Modules/Decommissioning/FacilityCost/` |
| External packages | none |
| Depends on | DEC-01 |
| Blocks | ACC-02 (provision) |
| Web reference | `Pages/PPDM39/Decommissioning/FacilityDecommissioning.razor`, `CostEstimation.razor` |

## 1. Purpose

Decommission a facility and estimate what it will cost, with escalation and contingency, and record
environmental restoration separately.

## 2. Source components

| Source | Reuse |
|---|---|
| `Decommissioning` contracts `FacilityDecommissioning*`, `EnvironmentalRestoration*`, `DecommissioningCost*` | data shape |
| `Decommissioning/Services/FieldDecommissioningService.Analysis` | estimates |
| `ProductionOperations/Services/IFacilityManagementService.cs` | facility identity |
| Tables `DECOMMISSIONING_STATUS`, `ENVIRONMENTAL_RESTORATION` | persistence |

## 3. Screens

`Decommissioning → Facilities and cost`

- Facility scope: facility, type, decommissioning method, planned date.
- Cost estimate: line items (engineering, removal, disposal, restoration), base cost, escalation
  rate and period, contingency percentage, total.
- Revision: new version with a reason; prior retained.
- Restoration panel: scope, plan, completion state, evidence.
- Provision preview: the amount to be recognised in accounting (links to ACC-02).

## 4. Data and Contracts

`OgDecomCost { Lines[], Base, EscalationRate, EscalationYears, ContingencyPct, Total, Currency,
Revision }`
All money is `decimal`; escalation composes multiplicatively and is shown step by step.

## 5. States

| State | Behaviour |
|---|---|
| Missing escalation basis | total shows base + contingency only, labelled |
| Contingency below the configured minimum | warning |
| Restoration incomplete | facility state cannot reach "complete" |
| Revision | prior version retained and viewable |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit estimate | `decommissioning.cost.write` | yes |
| Revise | `decommissioning.cost.write` | yes |
| Approve | `decommissioning.cost.approve` + SoD | yes |

## 7. Acceptance Criteria

- AC-1 Total = (base × escalation) + contingency, computed in `decimal` and shown step by step.
- AC-2 A revision never mutates a prior version.
- AC-3 Restoration state is independent of facility state.
- AC-4 A provision preview links to the accounting period.
- AC-5 Currency is explicit on every amount.
- AC-6 Approval records actor, date and conditions.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DEC-03"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-12-DEC-03/observed-evidence.json). The status row held one estimated and one
actual figure, which cannot hold a previous version, so the versioned estimate is a new table in the owning module and the
restoration stays in the owner's own `ENVIRONMENTAL_RESTORATION`.

**Why `DECOMMISSIONING_COST_REVISION` was added.** AC-2 says a revision never mutates a prior version, and a single pair of
columns cannot express that. Each estimate is now its own row - revision number, the revision it replaces, its reason, its line
items as JSON, its base, rate, years, contingency, total, currency, status, author and approval - and the status row's
`ESTIMATED_COST` is the *current* revision's total, so DEC-01's cost strip reads the same figure it always did.

**The arithmetic is decimal and shown.** Total = (base × escalation) + contingency, where escalation composes
multiplicatively - `(1 + rate)` once per whole year, with a fractional year applied as its own fraction - and the contingency is
a percentage of the escalated base. An estimate of 1,000,000 at 3 % for two years with 10 % contingency reads
1,060,900.00 escalated, 106,190.00... precisely 106,090.00 contingency and **1,166,990.00** total, with each step printed. With
no escalation period the total is base + contingency and says so, which is the feature's "missing escalation basis" state.

**The stage's own warning held.** There are no escalation or contingency defaults in the repository, so nothing is filled in for
the operator: the currency, the rate, the period and the contingency all have to be stated, the service refuses a blank
currency, and the "contingency below the configured minimum" warning is *not implemented* because there is no configured
minimum to compare against - recorded rather than invented.

**Restoration is separate, and it gates rather than merges.** A restoration record is written into
`ENVIRONMENTAL_RESTORATION` with the facility as its site, with its own status and certification. Completion of the facility is
refused while any record is open; and when the restoration completes, the facility's own state is **still planned** - the
gateway asserts that before allowing the completion, which is SG-12-4 in one step.

**Approval is a decision, not a save.** It is refused for the revision's author, refused with blank conditions, and stores the
approver, the timestamp and the conditions.

**The provision is a preview, not a posting.** It names the approved total, the period derived from the planned date and that
ACC-02 owns the posting; nothing is recognised in the ledger from this screen (AC-4).

**The facility planned date closes DEC-01's last gap.** `DECOMMISSIONING_STATUS` gained additive `PLANNED_DATE`, `PLANNED_BY` and
`DECOMMISSIONING_METHOD`, so DEC-01 now reads a facility's planned year and overdue from the record.

**Not built, and stated as gaps:** a configured minimum contingency (so that warning has no threshold); line items as their own
queryable table (they are JSON on the revision); a real provision posting; restoration matching beyond `SITE_ID`; an approval
requirement before completion (the feature states none); a single transaction across the revision and the status row; and
per-command permission enforcement beyond the author-approver rule.

## 9. Open questions and risks

- Escalation and contingency defaults may not exist in the repository; require explicit input and
  record that decision here.
