# HSE-07 — Permit applications

| Field | Value |
|---|---|
| Stage | STG-11 — HSE, compliance and permits |
| Area | HSE |
| Status | Gate met - see [evidence](../../evidence/STG-11-HSE-07/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PermitsAndApplications` |
| Desktop project | `Modules/Hse/Permits/` |
| External packages | `Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf` |
| Depends on | HSE-01 |
| Blocks | DEV-05 (permit linkage), WRK-04 |
| Web reference | `Pages/PPDM39/Permits/PermitDashboard.razor`, `NewPermitApplication.razor`, `PermitDetail.razor`, `Components/Workflow/PermitApplicationInbox.razor`, `PermitApplicationWorkflow.razor` |

## 1. Purpose

Apply for a permit, track its decision, and attach the conditions to the operation it authorises —
with legal transitions enforced.

## 2. Source components

| Source | Reuse |
|---|---|
| `PermitsAndApplications/Services/PermitApplicationLifecycleService.cs` (+5 partials incl. Documents, AttachmentContent, DocumentVocabulary) | lifecycle and documents |
| `PermitApplicationWorkflowService.cs` | workflow |
| `PermitComplianceCheckService.cs`, `PermitComplianceReportService.cs`, `PermitStatusHistoryService.cs` | compliance and history |
| `PermitStatusTransitionRules.cs` | legal transitions |
| `Forms/`, `DataMapping/`, `Metadata/PermitApplicationNativeMetadata` | form and mapping |
| `PermitsAndApplications.Pdf.Wkhtmltopdf` | PDF rendering |

## 3. Screens

`HSE → Permits`

- Inbox: applications by state, jurisdiction, applicant, due date.
- New application: jurisdiction (explicit), permit type, asset (well/facility/field), scope,
  attachments.
- Detail: all fields, documents, status history, compliance check results.
- Decision panel: legal transitions only, with rationale.
- PDF: generate and preview the application.

## 4. Data and contracts

`OgPermitApplication { Id, Number, Type, Jurisdiction, AssetId, Scope, State, SubmittedAt,
DecidedAt, Conditions[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Missing jurisdiction | application cannot be submitted |
| Illegal transition | not offered |
| Foreign asset | refused on read and on submit |
| PDF renderer unavailable | export disabled with the reason |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Create/Submit | `permits.write` | yes |
| Decide | `permits.approve` + SoD | yes |
| Generate PDF | `permits.read` | no |

## 7. Acceptance Criteria

- AC-1 Only legal transitions from `PermitStatusTransitionRules` are offered.
- AC-2 Jurisdiction is explicit and never defaulted.
- AC-3 A foreign asset is refused.
- AC-4 Every decision records actor, date, rationale and conditions.
- AC-5 The PDF matches the submitted application content.
- AC-6 Status history is append-only.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~HSE-07"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-11-HSE-07/observed-evidence.json). Nothing new was created: the
application is the owner's `PERMIT_APPLICATION` and its life is `PERMIT_STATUS_HISTORY`, both already in the PERMITS manifest.

**The transitions are the owner's, not a copy of them.** `PermitRules.NextStates` calls
`PermitStatusTransitionRules.IsTransitionAllowed` for every recorded state, so the picker and the command use the same table
the owner's service uses - `DRAFT` offers `SUBMITTED`/`WITHDRAWN`, `SUBMITTED` offers `UNDER_REVIEW`, `APPROVED`, `REJECTED`,
`ADDITIONAL_INFO_REQUIRED`, `WITHDRAWN`, and `WITHDRAWN` offers nothing. A refused transition names what *is* allowed.

**The state is read from the history, never from the header's enum.** `PERMIT_APPLICATION.STATUS` is an enum whose storage key
`ADDITIONAL_INFO_REQUIRED` does not match its member `AdditionalInformationRequired`, so a header written with the key cannot
carry that state. The register takes the state from the **latest history entry** (a plain string), and the header write maps a
key back to its member through the owner's own `ToStorageKey`. The gateway writes the exact key into a header and shows the
inbox still reporting its recorded state.

**A finding changed the write order.** The owner's `PermitStatusHistoryService` updates the header *then* appends the history,
so a failed append leaves the header already moved. The desktop **appends the history first** - it is what the register reads -
and updates the header afterwards; a header that cannot be written is reported as degraded, and the decision is not lost.

**A decision records four things.** The actor and the date are the history row's own columns; the rationale is required on
every transition; an approval additionally requires the conditions it is granted under. The applicant cannot move their own
application to `APPROVED` or `REJECTED` - submitting or withdrawing their own draft is allowed, because that is not a decision.

**AC-5 is the feature's own renderer state.** The PDF renderer shells out to a `wkhtmltopdf` binary that is not part of this
build, so the export is **disabled with that reason**, exactly as the feature's state table prescribes. The document a PDF would
render is written from the record itself - reference, id, type, jurisdiction, asset, field, state, applicant, dates, decision,
scope and the whole history - and the test asserts each field, so there is no second rendering path that could disagree.

**AC-6 is structural.** Every change appends a row and no row is ever edited; the gateway asserts the original `DRAFT` row and
its remarks survive a later decision.

**Not built, and stated as gaps:** the PDF file itself (REL-01 owns the native binary); attachments (the owner has
`APPLICATION_ATTACHMENT`, but this build has no document store, so the form offers nothing it cannot keep); the compliance
check and the workflow services (the screen offers the legal transitions directly, guarded by the rule and the segregation of
duties); a single transaction across the two writes; paging for the inbox; and per-command permission enforcement.

## 9. Open questions and risks

- `Wkhtmltopdf` requires a native binary; confirm installer packaging in REL-01.
- The Web create flow still defaulted to USA per the repository tracker; the desktop must not.
