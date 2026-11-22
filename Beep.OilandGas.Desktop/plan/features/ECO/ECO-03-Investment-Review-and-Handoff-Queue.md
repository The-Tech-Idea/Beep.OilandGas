# ECO-03 — Investment review and handoff queue

| Field | Value |
|---|---|
| Stage | STG-09 — Economics and AFE |
| Area | Economics |
| Status | Gate met - see [evidence](../../evidence/STG-09-ECO-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Economics/InvestmentReview/` |
| External packages | none |
| Depends on | ECO-01, WFL-04 |
| Blocks | ACC-05 (commitment creation) |
| Web reference | `Components/Workflow/DevelopmentInvestmentPanel.razor`, `…AssignedPanel`, `…HandoffPanel`, `…QueuePanel`, `…RoutingPanel`, `Pages/PPDM39/Development/DevelopmentAssignedReview.razor` |

## 1. Purpose

The queue where investment proposals wait for a decision, and the handoff that turns an approval
into a financial commitment.

## 2. Source components

| Source | Reuse |
|---|---|
| `LifeCycle` approval engine and investment process services | decisions |
| `ProductionAccounting/Services/AfeService.cs` | commitment creation |
| `Components/Workflow/DevelopmentInvestment*Panel.razor` | behaviour to reproduce |

## 3. Screens

`Economics → Investment review`

- Queue: proposal, entity, amount, classification, submitted by, age, decision due.
- Detail: summary, economics (link to ECO-01), risks, AFE linkage, conditions.
- Decision panel: approve / conditional / return / reject, rationale.
- Handoff panel: create the AFE/commitment, with the exact reviewed amount and currency.
- Routing panel: who reviews next when multiple approvers are required.

## 4. Data and contracts

`OgInvestmentProposal { Id, EntityId, Amount, Currency, Class, SubmittedBy, State, DecisionDue }`
`OgInvestmentDecision { ProposalId, Decision, Rationale, Conditions[], DecidedBy, DecidedAt }`.

## 5. States

| State | Behaviour |
|---|---|
| Economics missing | decision disabled with a link to ECO-01 |
| Amount changed since review | decision refused; re-review required |
| Already decided | read-only |
| Handoff acknowledgement lost | retained original; no second AFE |
| Concurrent decision | loser refused, winner named |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View queue | `economics.read` | no |
| Decide | `economics.investment.approve` + SoD | yes |
| Handoff | `economics.investment.handoff` | yes |

## 7. Acceptance criteria

- AC-1 A decision records actor, date, rationale and conditions.
- AC-2 A decision on stale economics (changed amount) is refused.
- AC-3 Author cannot approve.
- AC-4 Handoff creates exactly one AFE per decision.
- AC-5 Conditions travel to the AFE.
- AC-6 A lost acknowledgement does not duplicate the AFE.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ECO-03"
```

## 9. Open questions and risks

- Five Web panels implement this flow; only queue, detail, decision and handoff are needed in 1.0.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-09-ECO-03/observed-evidence.json). Proposals are read and written
through `UnitofWork<T>` over LifeCycle's `APPROVAL_WORKFLOW` row (extended additively) and the handoff writes
ProductionAccounting's `AFE`.

**The open question, answered.** Of the five Web panels, this feature builds **queue, detail, decision and handoff**. The
routing and assignment panels are not built: there is no approver-routing model in the owner, the feature says only those
four are needed in 1.0, and inventing a routing table would have been worse than leaving it out.

**Why the decision lives in `APPROVAL_WORKFLOW`.** That table is the owner's generic approval record and already holds the
entity, the amount, who requested it, who approved it and when - which is most of what the contract's two records
describe. It was extended rather than duplicated, and **one row is both the proposal and its decision** because a decision
is final: "already decided - read-only", and a concurrent decision's loser is refused rather than creating a second row.
The queue reads only rows whose `ENTITY_NAME` is `INVESTMENT`, so the same table's other workflows are untouched.

**AC-2 is a review cycle, not a check.** `AmendAsync` changes the amount and **leaves `REVIEWED_AMOUNT` alone**, which is
what makes the evaluation stale; `ReviewAsync` copies the current amount into it. A decision between those two commands is
refused naming both amounts. The alternative - a stored "reviewed" flag someone could set without looking - would have
satisfied the letter and not the point.

**AC-4 and AC-6 needed a column on the AFE.** The handoff is idempotent in two directions: the proposal remembers the AFE
it created (`AFE_ID`), and the **AFE remembers the proposal** it came from (`SOURCE_PROPOSAL_ID`, added to the AFE). The
second is what recovers a lost acknowledgement - the approval row may still point nowhere, so the handoff asks the AFEs
which proposal they belong to and adopts the one it finds instead of creating another. The test clears the proposal's link
by hand and re-runs the handoff to prove the count stays at one.

**AC-5 and the conditions.** A conditional approval cannot be recorded without at least one condition, and the conditions
travel onto the AFE as text one per line, read back as a list.

**Not built:** the routing/assignment panels and any multi-approver chain, delegation or escalation (the decision is
single-step); a validated foreign key from the proposal to `ECONOMIC_ANALYSIS_RESULT` (the link is an id plus the NPV the
reviewer saw); a commitment at handoff (ACC-05 owns that); and notifications or a queue ageing report.
