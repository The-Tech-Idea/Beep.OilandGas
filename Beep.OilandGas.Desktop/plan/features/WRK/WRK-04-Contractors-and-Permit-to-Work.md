# WRK-04 — Contractors and permit-to-work

| Field | Value |
|---|---|
| Stage | STG-13 — Work management |
| Area | Work |
| Status | Gate met - see [evidence](../../evidence/STG-13-WRK-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `Modules/Work/Permits/` |
| External packages | none |
| Depends on | WRK-01, HSE-07 |
| Blocks | WRK-02 (execution start) |
| Web reference | `Components/BusinessProcess/WorkOrderExecutionPermitIssuer.razor`, `…PermitPersonnelSourcePanel`, `…PermitMaterialReadinessPanel`, `…PermitApprovalReviewPanel`, `…PermitDocumentSourcePanel`, `…ExecutionStartPanel`, `…ExecutionHandoffPanel` |

## 1. Purpose

Gate field execution: a work order may not start until the permit is issued and every readiness
item is satisfied.

## 2. Source components

| Source | Reuse |
|---|---|
| `WorkOrder/ContractorManagementService.cs` | contractor register and competency |
| `WorkOrder/WorkOrderExecutionPermitService.cs` | permit issue and readiness |
| `PermitsAndApplications` | linked regulatory permit (HSE-07) |
| `Components/BusinessProcess/WorkOrderPermit*Panel.razor` | readiness presentation |

## 3. Screens

`Work → Permit to work`

- Contractor register: name, scope, competency records with expiry, insurance, performance.
- Permit request: work order, scope, hazards, isolation, personnel, materials, documents.
- Readiness checklist: personnel competency, isolation, materials, permit document, gas test,
  supervision — each with satisfied/not and evidence.
- Issue panel: issuer, validity window, conditions.
- Execution start panel: enabled only when readiness is complete.

## 4. Data and contracts

`OgWorkPermit { WorkOrderId, Scope, Hazards[], IsolationRef, Personnel[], Validity, Issuer,
State }`
`OgReadinessItem { Kind, Required, Satisfied, EvidenceRef }`.

## 5. States

| State | Behaviour |
|---|---|
| Readiness incomplete | start disabled, listing the missing items |
| Competency expired | item unsatisfied with the expiry date |
| Permit expired | execution start refused |
| Linked regulatory permit not granted | readiness blocked |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Maintain contractors | `work.contractor.write` | yes |
| Request permit | `work.permit.write` | yes |
| Issue permit | `work.permit.issue` + SoD | yes |

## 7. Acceptance Criteria

- AC-1 Execution start is refused unless every readiness item is satisfied.
- AC-2 An expired competency or permit makes the item unsatisfied.
- AC-3 Only a different person may issue a permit they did not request.
- AC-4 Permit validity is a window with start and end.
- AC-5 Readiness items carry evidence references.
- AC-6 A lost acknowledgement does not issue two permits.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WRK-04"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-13-WRK-04/observed-evidence.json). The readiness list the open question
calls unenumerated is already in the schema.

**The open question, answered.** *"The readiness item list is not enumerated in the repository."* It is: a work order's
`WORK_ORDER_INSPECTION` rows **are** its execution prerequisites, and each carries `PERMIT_PREREQUISITE_KIND`, a result and the
personnel / material / document acceptance revisions that evidence it. Readiness is therefore:

| Item | Source |
|---|---|
| Each prerequisite | a `WORK_ORDER_INSPECTION` row: kind, result, acceptance revision or `REGULATORY_REF` |
| `PERSONNEL_COMPETENCY` | the assigned people's `BA_LICENSE` + latest `BA_LICENSE_STATUS` + expiry |
| `PERMIT_WINDOW` | the permit's `VALID_FROM_UTC` / `VALID_TO_UTC` |
| Contractor register | `BUSINESS_ASSOCIATE` with its `BA_LICENSE` rows and their latest status |

A condition is satisfied only when its result is a passing result **and** an evidence reference is recorded, so a prerequisite
with no evidence is not satisfied however it was marked.

**AC-3 needed a column.** The permit recorded `ISSUED_BY` but **no requester**, so "only a different person may issue" had
nothing to compare against. `REQUESTED_BY`, `REQUESTED_AT_UTC`, `CONDITIONS` and `REQUEST_INTENT_ID` are additive on the
class's own partial, and the class itself only gained the word `partial`.

**AC-6 is structural, not a check.** The permit id is derived from the request intent (`PTW-<intent>`), so a retry adopts the
permit it already wrote; the same key with a different order or scope is refused rather than silently reused; and issuing a
second permit that would cover the same moment on the same order is refused with the original retained.

**AC-2 is the licence rule, stated positively.** A licence is current only when it is active, its latest status is not one of
the refusing states, **and its expiry is recorded and not past** - a licence with no expiry is *not* current, because it cannot
be shown to be. A permit outside its window is not usable whichever end has passed.

**Two corrections came out of the tests.** A first competency row read *"not current: granted"* - naming the status that was
fine and dropping the reason it failed - and now carries *"expired on &lt;date&gt;"*. The gateway scenario first assigned the
expired contractor to the order under test, so the readiness refusal named the personnel item when the test was about the
material one; the expired person is now assigned only in the AC-2 step.

**A validation catch.** The owner marks `ISSUED_BY`, `INSPECTION_REVISION`, `ISOLATION_REFERENCE`, `RISK_REFERENCE`,
`PERSONNEL_REFERENCE` and `MATERIAL_REFERENCE` required, and a freshly requested permit has none of them. The request records
explicit placeholders (`NOT-ISSUED`, `NO-INSPECTION`, `NOT-RECORDED`) that the read model shows back as *"no reference
recorded"* - the absence is visible rather than blank.

**Not built, and stated as gaps:** a maintainable contractor register (the page reads it; insurance and performance have no
recorded attribute at all); a gas test as its own record (it can only be a condition row); personnel recorded only on the
execution workflow; the permit's positive status vocabulary (the start refuses anything that is not `ISSUED`); a database
constraint behind the second-permit rule; a lost-acknowledgement recovery command; per-command permission enforcement; and the
Web's seven panels in place of this page's two bars.

## 9. Open questions and risks

- The readiness item list is not enumerated in the repository. Read `WorkOrderExecutionPermitService`
  and write the definitive list into §4 before coding.
