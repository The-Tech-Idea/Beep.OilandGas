# Implementation 82: process source read authority

Updated 2026-10-06. Supports [recorded source detail](Implementation-81-Recorded-Process-Source-Detail.md) and the persona queue-to-source journey. Partial: actual read orchestration focused verified; controller/host/native HTTP integration remains source-only.

## Boundary and active owners

Existing process read/history routes checked selected FIELD equality, but did not independently check the process entity's READ entitlement. The class already uses RequireCurrentFieldAccess, which performs canonical account/company/current-FIELD admission. Keep that mandatory boundary and the existing app-owned IAccessControlService as resource authority.

[CompanyProcessReadService](../../Beep.OilandGas.ApiService/Services/CompanyProcessReadService.cs) orchestrates existing IProcessService and resource checks; it is not a new authorization mechanism, domain store, permission source or duplicate contract. Both FIELD and exact process entity/type require READ access before and after loading. It captures initial source linkage and refuses disappearance or changed identity/FIELD/entity/type on closing read. History must have unique actual IDs and exact process identity; null history is unconfirmed rather than a fabricated empty success. Missing initial source returns unavailable without fetching history.

[BusinessProcessController](../../Beep.OilandGas.ApiService/Controllers/BusinessProcess/BusinessProcessController.cs) receives the existing resource service and delegates instance/history reads through this owner, using the server-derived actor and restored FIELD. One existing bound operation service keeps nested reads on the captured target. Unauthorized reads return Forbid; unconfirmed/changed reads return Conflict with a reload sentence and logged exceptions. No business actions or grants are inferred from persona preference.

## Focused evidence

[9 read cases](../../Beep.OilandGas.ApiService.Tests/CompanyProcessReadTests.cs) pass, zero failures/skips, terminal exit 0, 1.5257 seconds through [Test-CompanyProcessReads.ps1](../../Scripts/Test-CompanyProcessReads.ps1). Actual source is compiled against actual shared Data/Models/PPDM dependencies with explicit process/resource mocks. Log `%TEMP%/oilgas-company-process-reads.log`.

Cases verify valid source/history fresh resource checks, FIELD/entity denial before history fetch, resource revocation on closing checks, changed source linkage for both reads, foreign history refusal and missing source no history fetch. This is orchestration evidence, not actual resource-store/company membership/native HTTP/provider proof.

Existing WorkflowReadBoundaryTests source now supplies authenticated actor/resource fixtures and expects two source captures on the same bound target. WorkflowFieldQueryTests and WorkflowStepAuthorizationTests source constructors include the new resource dependency. Those controller suites are not executed evidence in this gate; full host/current identity/diagnostics projects remain unavailable. No shim/clone or replacement dependency.

## Remaining visible journey

Execute the actual controller/class-filter/typed-client/native HTTP boundary, with approved/revoked company roles, colliding source IDs across companies, restrictive resource grants, changed FIELD/binding and route refusal. Verify canonical diagnostics handling and consistent history snapshots under actual providers. Render queue -> FIELD choice -> process source and retry/error/sign-out behavior.

Then implement actual permitted workflow actions and finance posting with independently authorized approvers, self-approval rejection, reviewed source/history versions, retained uncertain intent, recorded readback and KPI/source drill-down. The current source read is a dependency for that end state, not a substitute for approval/posting.

No SQL, schema, new business EF path or synthetic records/metrics. Beep MigrationManager remains business schema owner. No persona/phase or full goal closes.
