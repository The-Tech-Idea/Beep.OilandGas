# LSE-02 — Acquisition pipeline and due diligence

| Field | Value |
|---|---|
| Stage | STG-03a — Lease acquisition |
| Area | Lease |
| Status | Gate met - see [evidence](../../evidence/STG-03a-LSE-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LeaseAcquisition` |
| Desktop project | `Modules/Lease/Pipeline/` |
| External packages | none |
| Depends on | LSE-01 |
| Blocks | LSE-03 |
| Web reference | none |

## 1. Purpose

Move an opportunity from "heard about it" to "signed", with due diligence, negotiation and rights
recorded at each step.

## 2. Source components

| Source | Reuse |
|---|---|
| `LeaseAcquisition/Services/LeaseAcquisitionService.Context.cs` | opportunity context |
| `…DueDiligence.cs` | diligence items and outcomes |
| `…Negotiation.cs` | offers, counter-offers, terms |
| `…Rights.cs` | mineral / surface / working interest rights |
| `…Stakeholders.cs` | parties and contacts |
| `Data/Lease/Contracts/*` | request records |
| Table `LEASE_ACQUISITION` | pipeline state |

## 3. Screens

`Lease → Acquisition pipeline`

- Board by stage: Opportunity → Screening → Due diligence → Negotiation → Rights review →
  Executed / Declined.
- Opportunity card: name, region, acreage, asking price, stage, owner, age.
- Detail: diligence checklist with outcomes and evidence, negotiation log, rights matrix,
  stakeholders, documents.
- Decision panel: advance / hold / decline, rationale mandatory.

## 4. Data and contracts

`OgLeaseOpportunity { Id, Name, Region, Acreage, AcreageUnit, AskingPrice, Currency, Stage, Owner,
StageEnteredAt }`
`OgDiligenceItem { Category, Question, Outcome, EvidenceRef, Reviewer, ReviewedAt }`

## 5. States

| State | Behaviour |
|---|---|
| Diligence incomplete | advance is disabled and lists the open items |
| Negotiation without a counterparty | refused |
| Stage unrecorded | appears in an "unclassified" column |
| Lost acknowledgement | retained original, explicit retry |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Record diligence | `lease.diligence.write` | yes |
| Record negotiation | `lease.negotiation.write` | yes |
| Advance / decline | `lease.pipeline.advance` | yes |

## 7. Acceptance criteria

- AC-1 Stage advance records actor, date and rationale.
- AC-2 Diligence items block advance while open.
- AC-3 Rights matrix distinguishes mineral, surface and working interest.
- AC-4 A declined opportunity keeps its history.
- AC-5 Money values are `decimal` with an explicit currency.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LeasePipeline"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LeasePipelineGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-03a-LSE-02/observed-evidence.json). The pipeline is read and written
through `UnitofWork<T>` over the LeaseAcquisition-owned entities; it contains no statement text.

## 9. Open questions and risks

- Nine service partials with overlapping method names; map one partial to one tab before coding.
