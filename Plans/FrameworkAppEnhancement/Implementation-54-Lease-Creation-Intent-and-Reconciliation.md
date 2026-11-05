# Implementation 54 — Lease creation intent and reconciliation

Follow-up: [Implementation 55](Implementation-55-FIELD-Bound-Lease-Portfolio.md) consolidates lease routes onto explicit FIELD, updates the SDK/Web contracts and integrates the actual portfolio page with the existing selector event plus cancellation/stale/error and uncertain status controls. Historical counts and routing statements below describe this slice's original checkpoint; the master tracker and Implementation 55 own current evidence. Shared selector persistence/authority and wider driver/status/circuit/runtime gates remain open.

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/10/11/12/20/50. Status: implemented canonical registration replay, read-only reconciliation, SDK alignment and dialog state with focused evidence. Provider, complete experience and release acceptance remain open. No phase closure.

## Registration identity and existing persistence owner

[Implementation 53](Implementation-53-Lease-Transaction-and-Readback.md) established the existing Beep transaction/readback boundary. This change extends that owner; it introduces no command table, second lease store, handwritten SQL or EF business persistence. Business/module schema work remains owned by Beep MigrationManager. No schema change or live migration is required here.

The [Data-owned creation context and receipt](../../TheTechIdea.Data/OilGas/LeaseCreationContext.cs) bind a creation intent to exact company, opaque database fingerprint, authenticated application account, FIELD, reviewed subtype/company-component/status types and storage date zone. These are assertions. The [canonical service](../../Beep.OilandGas.LeaseAcquisition/Services/LeaseAcquisitionService.ModelsCoreImpl.cs) resolves live company Administrator/FIELD WRITE authority again and compares every assertion before business reads or writes. IdentityServer does not become the role authority.

[CreateLeaseAcquisition](../../TheTechIdea.Data/OilGas/CreateLeaseAcquisition.cs) requires a nonempty client-generated CommandId and reviewed context. The service freezes the existing DTO before awaits. LAND_RIGHT_ID is the command GUID in N format; retries preserve it. A matching registered replay returns that same ID without another insert or transaction. Before a new insert the service checks again inside the existing datasource transaction lane, then retains the strict original pre/postcommit value/actor readback for all five records.

Read-only reconciliation uses the original five complete LAND identities and requested business values. Every original ROW_CREATED_BY must match the current authenticated actor. All five absent returns an explicit NotRecorded receipt; all five matching returns Recorded plus the original ID. Partial, ambiguous, changed or wrong-actor evidence is a conflict. Reconciliation never repairs records or sends another write. Current field/account/membership/configuration/binding authority is checked before returning.

The two header/agreement ACTIVE_IND flags can change through the operational-status owner. Reconciliation permits valid Y/N flags there while requiring the original status sequence, all other original values and actor evidence. A Recorded receipt describes historical registration; it grants no legal title, financial working interest, operator approval or current operational status. Advanced workflows or external enrichment that change other compared columns can require explicit reconciliation by the domain owner.

This is retained-record replay evidence, not an immutable lifetime command ledger. Deletion, external enrichment, another server process, provider isolation/unique-key enforcement and fencing an abandoned or delayed intent still require acceptance/design through the existing durable workflow/persistence owners. No automatic retry or replacement ID is justified by an uncertain response.

## Actual API and client contract

The [existing operations controller](../../Beep.OilandGas.ApiService/Controllers/Operations/LeaseAcquisitionController.cs) adds Administrator-only creation-context GET and read-only creation-reconciliation POST. Creation returns the shared typed receipt. The service independently checks authoritative current company roles and FIELD authority. Controller/host authentication execution remains unverified while the existing external identity helper/dependency drift remains unresolved; focused tests do not substitute an authentication pipeline.

The [SDK lease service](../../Beep.OilandGas.Client/App/Services/Lease/LeaseService.cs) now uses the existing captured-company ClientApiTransport and actual /api/leaseacquisition routes. Its [active interface](../../TheTechIdea.Data/OilGas/ClientILeaseService.cs) moves to Data with its namespace preserved. Unsupported /api/lease wrappers and their local-mode branches are removed; advanced renewal/transfer/royalty/payment/geometry work remains with its existing domain owners and is not reported implemented. There were no active repository callers of those removed SDK methods. This is a contract change for any external SDK consumer and needs normal release review.

The SDK freezes creation bodies, verifies context against selected company/account/FIELD, rejects stale-generation responses and requires a complete matching typed receipt. Mutation POSTs remain single attempts. Missing/null evidence is an error; explicit empty lists and explicit zero calculated interest remain valid. JsonRequired prevents a missing calculated-interest number from being invented as zero. Read summaries preserve exact company-recorded status codes; write status commands still use the canonical supported operations.

The [Web lease adapter](../../Beep.OilandGas.Web/Services/LeaseServiceClient.cs) delegates to the SDK. Existing OperationsServiceClient lease methods delegate to that adapter, removing dynamic JSON access and duplicate HTTP paths. The facade factory uses its existing transport/context. Full facade/DI/Web integration remains a separate gate.

## Creation dialog and focused experience boundary

The [actual dialog state](../../Beep.OilandGas.Web/Services/LeaseCreationState.cs) reviews a context, creates one command ID, freezes one intent on submission and preserves it after an uncertain result. Another write stays disabled until explicit read-only reconciliation returns NotRecorded; an explicit retry sends the unchanged original intent and ID. Recorded reconciliation confirms the original registration without resending creation. Malformed receipts never unlock a retry. Closing/reopening the dialog preserves pending state in this component session; a company/account change hides that draft and blocks commands under another selection. Delayed results from an earlier selection generation cannot confirm success.

The [existing page](../../Beep.OilandGas.Web/Pages/PPDM39/Operations/LeaseAcquisition.razor) uses standard Authorize/AuthorizeView roles for creation/status controls, disables draft editing after submission, shows reconciliation/error states and checks its current FIELD before sending. The preset FeeMineral code and UTC effective-date default are removed. The form requires an exact reviewed agreement code and explicitly chosen date, and displays the reviewed date zone. A reviewed reference-code picker, complete load/error/stale portfolio state and localization/accessibility/browser verification remain required.

Pending state is not yet persisted across circuit loss, browser reload or page disposal. Invalid/revoked/conflicting original intents can remain conservatively unresolved; do not discard them and mint another ID without an explicit domain-owned reconciliation/fencing decision. The current-field API mirror assertion, status expected revision/history concurrency, full app role/account lifecycle and broader lease experience remain open. These limits prevent this slice being promoted as a complete lease delivery.

## Focused verification

Final combined focused gate: **1407 passed, 0 failed, 0 skipped**, adding 75 to the previous 1332, in 5 minutes 50 seconds. The expanded core gate passed 128 checks; the combined gate includes SDK/dialog cases for stored-status preservation, explicit required receipt fields and the original frozen draft. After the final SDK company/account snapshot capture tightening, its **42 client/dialog checks passed, 0 failed/skipped**. The latest actual-source dialog/interest markup check passed with zero warnings/errors; git diff --check passed. All relative links resolve in the seven updated documents.

The [core lease suite](../../Beep.OilandGas.Repository.Tests/CompanyLeaseDataTests.cs) runs the actual company lease gate, canonical service, generic Beep repository and transaction owner with the existing recording provider and real focused SQLite application authority. New cases cover exact context assertions, read-only absence, persisted replay with no new transaction, changed business values, each missing original record, actor evidence on each record, uncertain commit, confirmed rollback and explicit same-ID retry, later operational status changes and request mutation during authority resolution.

[SDK tests](../../Beep.OilandGas.Repository.Tests/LeaseClientTests.cs) use the actual SDK/HTTP transport with recording HTTP responses. [Dialog state tests](../../Beep.OilandGas.Repository.Tests/LeaseCreationUiTests.cs) exercise the actual production state owner with explicit API responses/failures. These establish routing, scope/body/receipt/refusal/state behavior; they are not end-to-end HTTP/auth/browser/provider proof.

[Test-LeaseWebMarkup.ps1](../../Scripts/Test-LeaseWebMarkup.ps1) compiles two exact calculated-interest fragments and the actual complete creation-dialog markup against MudBlazor 9.7.0: zero warnings/errors. The isolated markup fixture supplies only event signatures and member bindings; it does not execute page callbacks or replace host/auth services. No full-page/browser claim is made.

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger "console;verbosity=minimal"
./Scripts/Test-LeaseWebMarkup.ps1
git diff --check
```

Logs: `%TEMP%/oilgas-lease-intent-all-focused.log`, `%TEMP%/oilgas-lease-client-ui-focused.log` and `%TEMP%/oilgas-lease-dialog-markup.log`. Latest markup artifact: `%TEMP%/oilgas-lease-markup-6119fbfc3319495e9f15d0f309c56e71`. The installed-driver ownership limitation from Implementation 53 remains: acquire/accept the real conformant Beep driver and canonical area/parent/FK requirements before any live command promotion. No package cache, external checkout, live business connection or deployment is modified. The full approved plan remains active.
