# Implementation 61 — company HSE request and evidence boundary

Date: 2026-10-05. Execution owners: CA-03/05/06/32, FG-10/11/12/20/40/42. [Master tracker](../../MASTER-TODO-TRACKER.md) owns execution status. This continues [Implementation 60](Implementation-60-Company-Development-Request-and-Failure-Boundary.md) through the existing HSE facade and incident, RCA, barrier, corrective-action, HAZOP and KPI owners. The full plan remains active.

## Problem and resulting behavior

The HSE facade captured a supplied FIELD and constructed six services using a supplied global connection. The same six owners also had independent API registrations. Their repositories lacked required company row policy; direct service use bypassed the facade's FIELD checks. HAZOP study operations had no FIELD membership check, and incident/study requests silently rewrote the submitted FIELD. Three dynamic repository factories could fall back to object. Failed HAZOP summaries were skipped, malformed condition payloads became plain descriptions with invented empty consequences, and unavailable exposure hours became zero, including a replacement denominator of one in trends. Missing incident dates/states/severities also produced default interpretations; out-of-range tiers were clamped.

All 60 public asynchronous operations now review the authoritative profile/account/company/database/FIELD/revision: 27 in the existing facade and 33 across the six directly registered owners. Every request requires an available selected FIELD. Explicit FIELD parameters and the facade's captured FIELD must match exactly. Required actor assertions must match the actual profile account and cannot be missing. Public value and void success paths recheck the captured review; an in-flight operation cannot adopt a later selection or binding.

All 13 repository factories use the reviewed connection and mandatory row policy. The three dynamic factories resolve the actual canonical PPDM assembly and exact table metadata, refusing unavailable types/definitions instead of creating an object repository. Actual-source factory tests establish the selected connection, canonical type/table and required policy; they do not establish successful HSE row access.

RCA, barrier and corrective-action owners reuse the existing incident interface/implementation as a mandatory dependency. Each direct incident operation checks the returned exact incident/FIELD and rechecks its review before child storage. The facade keeps its own exact FIELD fence. HAZOP study operations require a unique active PROJECT_COMPONENT FIELD context using the existing FIELD_SOURCE marker and an exact active HAZOP PROJECT, through required policy. Incident/study creation refuses a submitted different FIELD rather than rewriting its intent. Missing existing cause/step/condition mutation targets explicitly refuse in the three repaired branches.

All six API registrations and the orchestrator's HSE factory supply the required dependencies. The full Program/concrete orchestrator/host remains source-only; no complete registered-host or auth/browser proof is inferred from the source-linked target. The incident interface documentation reflects independent canonical-service scoping.

## Evidence and failure behavior

Unavailable exposure hours explicitly refuse through the existing error path. Public exposure/KPI/TRIR/positive trend requests cannot return a zero denominator or calculated rate from an invented exposure of one. Nonpositive trend intervals refuse. A verified denominator, unit/calendar/jurisdiction/basis and numerical acceptance still need an approved existing owner; current KPI DTO/stub metric work remains open.

HAZOP summary load failures log and propagate instead of producing partial success. Malformed/absent stored condition JSON refuses interpretation; no plain-text payload/empty-consequence fallback is accepted. Missing incident date or recorded response state refuses instead of substituting current UTC or REPORTED. Missing/unsupported severity refuses instead of becoming tier four. The owner emits and accepts exact TIER_1..TIER_4 codes; invalid requested tiers refuse instead of clamping. Existing historical data must be reviewed explicitly before promotion; no data migration or rewrite occurs.

This repairs specific evidence failures. It does not certify complete incident classification, jurisdictional obligations, scientific rate definitions, approval/close-out prerequisites or durable command outcomes. Remaining domain defaults, parent/component interpretation, status/identity/date/currency semantics and workflow transitions retain their existing plan owners.

## Focused source and runtime evidence

The focused target compiles the complete actual facade and six HSE implementations with existing models, profile/company/registry/row policy and actual Beep generic repositories. No synthetic HSE class or missing production project is supplied. Eight compile warnings remain: four known missing-analyzer project warnings and four nullable assignments in the newly compiled existing incident/barrier source. Zero source compile errors. Those optional/required PPDM nullability and barrier-parent semantics need domain review; no fabricated value is introduced to silence them.

The cases cover all 60 missing-selection entry points, all supplied/missing actors, all 27 facade FIELD assertions and explicit direct FIELD parameters. Additional cases exercise unchanged submitted incident/study requests, exact incident/FIELD/case refusal, changed profile revision during incident input, unavailable exposure/rates, mandatory actual table policy, three actual canonical repository factories, malformed/missing HAZOP JSON, invalid requested tiers and unsupported recorded severity.

Profile/account/company/persistence/policy are actual sources. Beep metadata and row responses are the shared recording-provider boundary. Six child-membership cases and the changed-incident-input case explicitly mock the existing IHSEService interface to establish consumer validation; these do not prove an entire incident-to-corrective-action workflow. Private payload/severity helpers establish actual interpretation behavior without claiming provider reads or scientific validity. Existing normal API tests also link these cases; the full normal API external-dependency build is unverified.

The final affected gate passed **522 tests, 0 failed, 0 skipped, in 3 minutes 59 seconds**: 183 new HSE cases plus the previous 339 development/production/retirement/current-FIELD/selector/UI/MVC/profile/PPDM policy cases. This is an affected gate, not an all-inventory checkpoint. The final rebuild also surfaced four existing generated Models member-hiding warnings, in addition to the eight source-checkpoint warnings above; zero compile errors. Logs: `%TEMP%/oilgas-hse-source-build.log`, `%TEMP%/oilgas-hse-final-affected.log`. No new all-inventory, production OIDC/claims-bridge, browser, hosted provider or numerical certification is claimed.

Reproduce the affected gate:

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --filter 'FullyQualifiedName~CompanyHseBoundaryTests|FullyQualifiedName~CompanyDevelopmentBoundaryTests|FullyQualifiedName~CompanyProductionBoundaryTests|FullyQualifiedName~CompanyRetirementBoundaryTests|FullyQualifiedName~CurrentFieldLifecycleGateTests|FullyQualifiedName~CompanyPPDMRowAccessTests|FullyQualifiedName~CompanyFieldSelectionTests|FullyQualifiedName~FieldSelectionTests|FullyQualifiedName~FieldSelectionEndpointTests|FullyQualifiedName~RepositoryAccountProfileTests'
```

## Subsequent execution

[Implementation 62](Implementation-62-Company-HSE-Incident-Row-Authority.md) now supplies bounded read authority for six canonical incident family tables through the existing policy/evidence owners and actual incident/RCA reads. The unsupported-HSE-policy statements below describe this implementation's earlier checkpoint. Generic HSE writes and PROJECT authority remain unavailable; restricted case visibility, durable workflows and full release gates remain required.

## Canonical data and release work remains required

The actual metadata already defines HSE_INCIDENT, HSE_INCIDENT_COMPONENT/DETAIL/RESPONSE/CAUSE and PROJECT/COMPONENT/STEP/CONDITION/STATUS with complete composite keys. These differ from the missing retirement extension definitions. Their presence does not establish ownership policy, valid command references, transaction semantics or supported production behavior. Current company row policy does not yet authorize HSE or PROJECT tables: those operations explicitly refuse before provider row access. HSE workflow availability is not established by the refusal gate.

- Extend the existing company row-policy/evidence owners through canonical incident FIELD components, header/child references, exact active/primary roles, complete keys and current company FIELD data entitlement. Review restricted incident visibility and child authority explicitly; do not infer company or scope from textual IDs or merely present metadata.
- Extend actual HAZOP and corrective-action PROJECT/component/step/condition/status/BA relationship policies with complete identities. Repair alternate component selection, conditional JSON contents, step/deviation association, allocation concurrency and historical records through existing owners.
- Complete actual incident reporting/assignment/classification/status/history and RCA/barrier/corrective-action workflows with approved reference data, participant/equipment authority, meaningful completion evidence, durable approval/close-out prerequisites, transactions/readback and reconciliation. Tick-modulo incident IDs and max-plus-one child allocations remain open concurrency work.
- Define approved observed exposure, reporting basis, actual RCA/action counts, date/calendar/unit/jurisdiction and no-data contracts through the existing HSE/KPI/data owners; remove remaining DTO/metric defaults and certify calculations independently. HSE public contracts remain existing Models definitions and need canonical Data ownership consolidation; no duplicate contracts are added here.
- Continue exploration/process direct registrations and request/repository ownership, then actual concrete orchestrator/host/company/auth/standard-role/API/Web/HTTP/browser/provider/concurrent revocation acceptance. Production, development and retirement domain/persistence repairs remain required.
- Continue durable lease/history/creation/circuit/provider, financial/accounting, localization/jurisdiction, governance and all other master-plan recommendation owners.

No SQL, business EF migration, schema execution, live datasource mutation or deployment is introduced. Business/module schema work stays with Beep MigrationManager; the existing approved EF identity/access/profile registry stays with its owner. No HSE feature, phase, scientific result or full-plan readiness is marked complete.
