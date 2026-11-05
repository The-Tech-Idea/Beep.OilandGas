# Implementation 75: recorded corrective-action plan creation review

Date: 2026-10-06. Owners: CA-32, FG-40/42. The [root master tracker](../../MASTER-TODO-TRACKER.md) owns overall execution. This extends the [existing public plan API/SDK](Implementation-74-Reviewed-Corrective-Action-Plan-API-and-SDK.md) and canonical command/policy; no database schema, SQL, business EF lane or duplicate domain store is introduced.

## Delivered behavior

The existing corrective-action command exposes a read-only creation review. The canonical row policy independently requires the current account/company/database/FIELD, current case visibility, app HSE permissions, inherited entitled FIELD WRITE and a recorded incident state permitting commands. It returns complete recorded participant role identities through the existing roster/reference/crew policy, and current recorded non-status response types. No names, contact details, inferred app-user-to-BA mapping or invented default codes are disclosed.

The review excludes inactive, expired and future response codes. Complete bounded reference inventory rejects invalid or duplicate identities and date interpretations. Existing project links, including inactive response history, require reconciliation before new-plan review; invalid, duplicate or exhausted response observations also refuse. The revision binds context, write/case/state authority, participant/reference evidence and full incident response history. Repeated closing reads refuse changed evidence; the SDK freezes returned collections and discards late results after company switching away and back.

The existing corrective-action service, Field facade, controller and typed Field SDK expose GET `/api/field/current/hse/incidents/{incidentId}/ca-plan-review`. The endpoint independently checks the active account/current FIELD. This review is distinct from existing-plan resolution and makes no writes or transactions.

## Source owners

- [Shared review contract](../../TheTechIdea.Data/OilGas/CorrectiveActionPlanContracts.cs) reuses [recorded participant contracts](../../TheTechIdea.Data/OilGas/InvestigatorAssignmentReview.cs).
- [Existing typed command](../../Beep.OilandGas.ApiService/Services/PPDMCompanyCorrectiveActionCommands.Plan.cs) delegates to the [canonical policy](../../Beep.OilandGas.ApiService/Services/CompanyPPDMRowAccessPolicy.HseCommands.cs).
- [Service](../../Beep.OilandGas.PPDM39.DataManagement/Services/HSE/CorrectiveActionService.cs), [Field facade](../../Beep.OilandGas.LifeCycle/Services/HSE/PPDMHSEService.cs), [controller](../../Beep.OilandGas.ApiService/Controllers/HSE/HSEController.cs) and [existing SDK helper](../../Beep.OilandGas.Client/App/Services/Field/HseIncidentClient.cs).
- [Native HTTP cases](../../Beep.OilandGas.Repository.Tests/CurrentFieldLifecycleGateTests.PlanCreation.cs) and [SDK validation/lifetime cases](../../Beep.OilandGas.Repository.Tests/InvestigatorClientTests.cs).

## Evidence and remaining boundaries

Final-source focused verification reached terminal exit 0: **88 passed**, zero failures/skips, in **9.7170 minutes**. This includes 55 SDK cases and 33 native plan/review HTTP cases; 27 are new review cases. The remaining 61 cover affected existing behavior. Counts overlap historical Implementation 74 gates and are not summed into a unique lifetime total. Production service/facade/controller/helper and canonical policy are source-linked in the focused assembly. Native HTTP uses explicit authentication/top-level orchestrator/outer-permission fixtures, real domain owners, simulated persisting Beep provider and SQLite app authorization. Complete SDK facade/client assembly, full host/DI/OIDC, rendered Web, physical provider/concurrency and performance remain unverified.

This is a recorded-choice review, not a durable intent receipt, qualification/disclosure/jurisdiction approval, or lifetime creation guarantee. Submission currently asserts the FIELD review and the command independently checks current references; **At this checkpoint, binding submission to this catalog revision remained open; [Implementation 76](Implementation-76-Catalog-Bound-Plan-Creation.md) delivers that enforcement, with current verification tracked there**. Retain and check the exact review through the existing submission/command owner next, alongside pending-intent/outcome state and the rendered picker after required UI guidance is supplied or explicitly overridden. No automatic retry, grant, migration, phase or feature closure is claimed.

Verification logs: `%TEMP%/oilgas-plan-creation-review.log` and `%TEMP%/oilgas-plan-creation-review-final.log`. The initial compile found two namespace errors and the first 27-case runtime run failed five cases because an unfiltered evidence read is intentionally unsupported. The implementation now uses bounded explicit Y/N predicates without weakening the shared reader. The corrected final source was rebuilt only after the first process reached terminal exit 1; the final gate passed. Existing source warnings were recorded without adding missing projects. Missing external analyzer projects are not cloned or replaced.


Executed final gate:

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter 'FullyQualifiedName~InvestigatorClientTests|FullyQualifiedName~Actual_HSE_plan_HTTP|FullyQualifiedName~Actual_HSE_plan_creation_review' --logger 'console;verbosity=normal'
```

Document checks: eight UTF-8 tracker/phase/index/evidence files and 329 local file links, zero missing targets; `git -c core.safecrlf=false diff --check` passed. Production source remained unchanged during the final gate. No rendered UI, full SDK facade/client assembly, physical provider or full-host acceptance claim.
