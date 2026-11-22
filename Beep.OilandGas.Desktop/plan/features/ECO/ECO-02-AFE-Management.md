# ECO-02 — AFE management

| Field | Value |
|---|---|
| Stage | STG-09 — Economics and AFE |
| Area | Economics |
| Status | Gate met - see [evidence](../../evidence/STG-09-ECO-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionAccounting` |
| Desktop project | `Modules/Economics/Afe/` |
| External packages | none |
| Depends on | DEV-05, ACC-05 |
| Blocks | ECO-03, ACC-05 |
| Web reference | `Pages/PPDM39/Economics/AFEManagement.razor` |

## 1. Purpose

Raise, revise, approve and close an authorisation for expenditure, and keep it linked to the work
it authorises and the invoices it commits.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionAccounting/Services/AfeService.cs` and 5 partials | AFE lifecycle |
| `ProductionAccounting/Core/Interfaces/*` AFE contracts | contract |
| `Accounting/Services/PurchaseOrderService.cs` | commitment linkage |
| AFE tables under `ProductionAccounting` | persistence |

## 3. Screens

`Economics → AFE`

- Grid: AFE number, title, type, entity, amount, currency, status, approved by, approved date,
  committed, spent, remaining.
- Detail tabs: Scope · Cost estimate · Schedule · Approvals · Commitments · Supplements · Closure.
- Actions: submit, approve, supplement, transfer, close.
- Variance panel: budget vs committed vs actual.

## 4. Data and contracts

`OgAfe { Number, Title, Kind, EntityId, Amount, Currency, Status, Committed, Spent, Remaining,
Revisions[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Amount exceeded by commitments | flagged; new commitment blocked unless a supplement is approved |
| Supplement | creates a revision, prior retained |
| Approval by the author | refused |
| Closed AFE | read-only |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Raise | `economics.afe.write` | yes |
| Approve | `economics.afe.approve` + SoD | yes |
| Supplement | `economics.afe.supplement` | yes |
| Close | `economics.afe.close` | yes |

## 7. Acceptance criteria

- AC-1 A revision never mutates the approved version.
- AC-2 A commitment above the remaining amount is refused.
- AC-3 The author cannot approve.
- AC-4 Remaining = amount − committed, exactly.
- AC-5 A closed AFE is immutable.
- AC-6 A supplement is itself approved before it is effective.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ECO-02"
```

## 9. Open questions and risks

- Five partials with unclear state names; extract the transition list before building the UI.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-09-ECO-02/observed-evidence.json). AFEs are read and written through
`UnitofWork<T>` over ProductionAccounting's own `AFE` (extended additively) and the new `AFE_COMMITMENT`.

**The open question, answered - and it was worse than "unclear".** `AfeService` implements **exactly two transitions**:
`CreateAfeAsync` writes `DRAFT` and `ApproveAfeAsync` writes `APPROVED`. The `AFE_STATUS` reference seed contains **only
those two rows**, and there is no submit, rejection, supplement, transfer or closure anywhere in the project. The
transition list is now stated once, in the owner, as `AfeLifecycle`:

| From | To | Action | Who |
|---|---|---|---|
| DRAFT | SUBMITTED | submit | the author |
| SUBMITTED | APPROVED | approve | an approver who is not the author |
| SUBMITTED | RETURNED | return | an approver |
| RETURNED | DRAFT | revise | the author |
| APPROVED | CLOSED | close | the author |

A **supplement is not on that list**: it creates a new revision rather than moving the approved row, which is what makes
AC-1 true. `AfeLifecycle.StartsRevision` is that action, and the screen prints the whole list so no state is invented.

**AC-1 and SG-09-4, the important one.** `SupplementAsync` reads the approved row and inserts a **new** row with the same
`AFE_NUMBER`, `REVISION_NO + 1`, `SUPERSEDES_AFE_ID` pointing at it and `ROOT_AFE_ID` preserved, in `DRAFT`. The approved
row is **not written at all** - not even a status change. "Superseded" is *derived*: a revision is superseded when another
row's `SUPERSEDES_AFE_ID` points at it. The gateway test snapshots eleven of its columns before and after the supplement
and asserts they are identical, so a future change that stamps the old row will fail the test.

**AC-2 needed a table, not a column.** The AFE had no committed amount and no commitment rows, so "a commitment above the
remaining amount is refused" had nothing to measure. `AFE_COMMITMENT` was added to the owning module: committed is the
**sum of its rows**, never a scalar that could drift from them, and the gateway asserts the stored `SUM` as well as the
screen's number.

**AC-3, and a gap recorded rather than fixed.** The owner's `ApproveAfeAsync` sets the status and records who changed the
row but **never compares that person with the creator** - the author can approve their own AFE through it. It is not
changed here because the API calls it and this repo puts authorization in the client app; ECO-02 enforces the rule in its
own service before anything is written.

**AC-5 and AC-6.** Closing is the last write and records actor, date and note; afterwards every command checks the closure
first. A supplement starts in `DRAFT`, so a commitment against it is refused as not effective - it only becomes the
effective authorisation after submit and approval by someone other than its author.

**Not built:** transfer (it needs a destination AFE and a cost-split rule the feature does not define), AFE line items on
the screen, attachments, the plan's detail tabs as tabs (they are one page), and a procurement foreign key for
commitments - the reference is free text until ACC-05 lands, which the feature permits.
