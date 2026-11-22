# ACC-07 — Royalties, joint interest billing and cost allocation

| Field | Value |
|---|---|
| Stage | STG-10 — Production accounting and financials |
| Area | Accounting |
| Status | Gate met - see [evidence](../../evidence/STG-10-ACC-07/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionAccounting` |
| Desktop project | `Modules/Accounting/Royalties/` |
| External packages | none |
| Depends on | ACC-03 |
| Blocks | ACC-08 |
| Web reference | `Pages/PPDM39/Accounting/Royalties.razor`, `RoyaltyDetails.razor`, `JointInterestBilling.razor`, `CostAllocation.razor`, `Pages/PPDM39/Compliance/PaymentDialog.razor`, `WaiveObligationDialog.razor` |

## 1. Purpose

Pay the owners: royalty calculation and disbursement, joint interest billing to partners, and
allocation of shared costs — each traceable to the volume and the ownership fraction that produced
it.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionAccounting/Services/RoyaltyService.cs` (+ `PostingReview`) | royalty |
| `ProductionAccounting/Services/RoyaltyDisputeService.cs` | disputes |
| `ProductionAccounting/Services/JointInterestBillingService.cs` | JIB |
| `ProductionAccounting/Services/CostAllocationService.cs` | allocation |
| `ProductionAccounting/Services/CopasOverheadService.cs` | overhead |
| `ProductionAccounting/Services/ImbalanceService.cs` | imbalances |
| `Beep.OilandGas.LifeCycle` `PPDMAccountingService` | orchestration |

## 3. Screens

`Accounting → Royalties and JIB`

Tabs:

- **Royalty calculation** — lease, volume, value, rate, deduction, net payable, owner.
- **Royalty details** — calculation history, payments, disputes, posting review.
- **JIB** — statement per partner: charges, credits, net, due date, approval.
- **Cost allocation** — pool, basis, allocated amounts, preview and commit.
- **Overhead** — COPAS overhead computation.
- **Imbalances** — overlift/underlift by partner with settlement options.

## 4. Data and contracts

`OgRoyalty { LeaseId, OwnerId, Volume, Unit, Value, Currency, Rate, NetPayable, JournalId }`
`OgJibStatement { PartnerId, Period, Charges[], Credits[], Net, Currency, State }`.

## 5. States

| State | Behaviour |
|---|---|
| Missing ownership fraction | calculation refused with the lease/owner named |
| Fractions do not sum to 1 | refused (unless the lease type permits a stated residual) |
| Disputed royalty | payment blocked with the dispute reference |
| Currency mismatch | refused |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Calculate royalty | `accounting.royalty.write` | yes |
| Pay royalty | `accounting.royalty.pay` + SoD | yes |
| Raise dispute | `accounting.royalty.dispute` | yes |
| Post JIB | `accounting.jib.post` + SoD | yes |

## 7. Acceptance criteria

- AC-1 Royalty nets exactly to value × rate − deductions, in `decimal`.
- AC-2 A missing or non-summing ownership fraction is refused.
- AC-3 A disputed royalty cannot be paid.
- AC-4 JIB net equals charges minus credits.
- AC-5 Allocation preview equals the committed result.
- AC-6 Every posting is traceable to the source volume.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ACC-07"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-10-ACC-07/observed-evidence.json). Royalty calculations, their
deductions, the joint-interest statements and the committed allocation are read and written through `UnitofWork<T>` over the
ProductionAccounting tables; the postings go to the ledger's `JOURNAL_ENTRY`/`JOURNAL_ENTRY_LINE` at the configured accounts.

**AC-1 against the owner's formula - a reading that is recorded, not silent.** The criterion reads "royalty nets exactly to
value x rate - deductions". Taken literally that takes the whole deduction off the royalty amount, charging the mineral owner
100 % of the cost instead of their rate share. The owner's own header states the relation this feature implements:

```
Royalty = Net Revenue x Rate        Net Revenue = Gross - Deductions      Rate in percentage points
```

That is the ASC 932/COPAS relation, and it is algebraically *value x rate - deductions x rate*. The unit test asserts both
forms and shows that the literal reading gives a different answer, so the difference is visible rather than assumed.

**AC-2's unit came from the owner's code.** `JointInterestBillingService` writes `NET_REVENUE_INTEREST` from a value it calls
`interestPercent`, and the owner's recorded-inputs test seeds a royalty rate of 12.5 - so ownership is in percentage points.
The gate therefore requires 100, accepts a declared residual (a lease may state the unowned share), and names the lease, every
owner and the total when it fails.

**AC-3 needed a home.** The owner's `ROYALTY_DISPUTE` is keyed to `ROYALTY_STATEMENT`, a table this app never populates, so a
dispute raised against it would point at nothing. The dispute is recorded on the calculation that is actually paid, with the
`DIS-<year>-<seq>` reference the criterion asks for; the payment refusal carries that reference and the reason.

**AC-4 and AC-5 cannot drift.** The statement's header totals are computed from the lines just written, and preview and
commit call the same `CostAllocationMath.Allocate` - which uses largest-remainder rounding so the shares add back to the pool
exactly, because a cost allocation that does not sum to the cost it allocates is a defect rather than a rounding note.

**AC-6 and SG-10-7.** The royalty carries its volume, unit, price, allocation detail and allocation result, so it can be
followed back to the production that produced it; a joint-interest statement refuses to be raised without a source volume
reference. Both post to the configured chart - royalty expense 6100 against accrued royalties 2200 on accrual, that accrual
cleared to cash 1000 on payment, and the bill to the receivable 1110 against operating expense 6001 - and a chart missing an
account refuses the command naming the number rather than moving money with no ledger effect.

**A model defect found and worked around, not deleted.** `ROYALTY_PAYMENT` declares both `STATUS` (the stored column) and
`Status` (a `[JsonIgnore]` alias delegating to it). The unit of work builds a case-insensitive property map and throws
`An item with the same key has already been added. Key: STATUS` before it writes, so no payment row can be persisted through
that entity - the same defect class as `PIPELINE_ANALYSIS_RESULT` in WTA-03. Deleting a public property is not additive and
cannot be verified here, because the ApiService test host does not build, so the payment is recorded on the calculation it
settles: `PAID_AT`, `PAID_BY`, `PAID_AMOUNT` and `PAYMENT_JOURNAL_ENTRY_ID`, with the journal proving the money moved.

**Not built, and stated as gaps:** overlift and underlift (the owner has the imbalance tables but the feature's tab needs a
nominated-versus-actual process this app does not have), COPAS overhead, a run-ticket-driven royalty (the volume and price are
stated and the allocation ids are carried for traceability only), jurisdiction rate tables for severance and ad valorem,
withholding and tax reporting, a per-line ledger composition for a bill, a journal for the allocation (it splits an existing
cost rather than moving money), and a fraction editor with a running total - the open question below, which is answered by
reading the owner's fractions and refusing anything that does not sum to 100 %.

## 9. Open questions and risks

- Ownership fractions are the most error-prone input in the product. Consider a dedicated fraction
  editor with a running total, and require explicit confirmation.
