# Implementation 33 — Canonical PPDM client and mutation retries

Date: 2026-10-05. Tracker: CA-02/03/05/06 and FG-01/10/12. Status: shared contracts, active typed CRUD client and facade HTTP transport verified with focused checks; full app/API wiring and business row isolation remain open.

## Existing CRUD contract aligned

The facade's DataManagementService previously called `/api/ppdm39data/...` routes which the actual controller does not expose. It expected generic entities/lists and returned true after deleting, although the API returns GetEntitiesResponse and GenericEntityResponse envelopes. These five method signatures now use the existing real envelopes and GetEntitiesRequest/raw entity dictionaries. There are no generic CRUD compatibility overloads or additional API routes. Repository source search found no callers of the removed generic facade CRUD signatures; external callers must adopt the corrected contracts.

PpdmDataClient owns this facade's existing CRUD request construction. DataManagementService delegates to it through the active application facade. Lists POST to `/api/ppdm39/data/{table}`, inserts POST raw entity dictionaries to `{table}/insert`, and single reads/updates/deletes use `{table}/{id}` with GET/PUT/DELETE. Route segments are escaped. Mutations do not fabricate a returned entity; a failed result remains failed. Empty/null/malformed successful HTTP responses cannot become successful CRUD results. Connection defaults are not injected; list request assertions remain subject to the server binding gate.

PPDM39DataController's declared action return types now match the envelopes it already returns. Invalid entity bodies also use GenericEntityResponse. This aligns source/OpenAPI typing without changing its actual routes or creating another endpoint implementation. The Web client continues through its existing API client and the same contracts.

## Canonical shared definitions

GetEntitiesRequest, GetEntitiesResponse, GenericEntityRequest, GenericEntityResponse and their existing notifying ModelEntityBase moved from Models to TheTechIdea.Data. Namespaces, properties, notification behavior and JSON shape are preserved; old source definitions were removed, with no duplicate or forwarding classes. The shared base continues to implement the actual PPDM IPPDMEntity contract. Data now references the existing PPDM model assembly and the same Beep model package rather than copying that interface. The PPDM interface/model assembly remains in its existing location; its assembly-based entity lookup continues to work.

Focused tests now reference the real shared base/interface dependencies rather than source-linking them separately. Broader domain contract/entity ownership remains a planned audit; moving these active contracts does not establish that every legacy DTO is canonical.

The calculation service's existing model discovery previously anchored its result-model assembly on ModelEntityBase. It now anchors on its actual existing NODAL_ANALYSIS_RESULT model, so moving the shared base does not redirect persistent result discovery into Data. This calculation integration edit is source-only; no second discovery implementation was added.

## One facade HTTP sender

ClientApiTransport replaces ServiceBase's previous send/retry/authentication implementation. ServiceBase's existing HTTP helpers delegate to the application facade's one lazily created transport; PpdmDataClient uses that same transport. No alternate remote send implementation was retained in ServiceBase.

CompanyClientContext captures request selection before serialization/token lookup. New message instances retain that original capture on GET retries; both transport and response reading check freshness. A company switch during authentication, a failed GET or a delayed response cannot adopt the new company or publish the stale result. Cancellation/missing selection refuses before authentication/network access where applicable. Authentication, HTTP status/error payload, timeout and malformed response failures remain explicit.

Only GET requests retry configured transient connection/timeout failures. POST/PUT/DELETE are sent once because their first attempt may already have persisted. Read-only POSTs are also single attempts until an explicit endpoint idempotency contract exists. HTTP error statuses are not retried. GET retry attempts allocate fresh HttpRequestMessage instances, fixing the old attempt to resend a message already sent by HttpClient. Existing absolute addresses constructed by the facade's query helper are used once; absolute requests outside the configured API origin are refused before attaching credentials. These changes apply to all facade services using ServiceBase, not just CRUD.

Other existing client transports, including Web's higher-level update/version retry wrappers, are not changed by this facade transport extraction and need their own idempotency review. Database/configuration preferences and other historical data-management routes also remain open work. Prefixed reverse-proxy deployments need explicit company-policy/path-base verification.

## Verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false --logger "trx;LogFileName=company.trx" --results-directory TestResults/company
dotnet build Beep.OilandGas.Models/Beep.OilandGas.Models.csproj -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false
```

**241 passed, zero failed/skipped**: previous 216 plus 25 active client/transport/contract cases. Actual production transport and typed CRUD client are source-linked. A recording HttpMessageHandler supplies controlled HTTP responses and failures; this verifies request methods/routes/body/header capture, envelope decoding, failure status/content preservation, missing payload refusal, fresh message retry, single-attempt mutation failures/timeouts, delayed token/response switches, cancellation, authentication failure and origin checks. These are client boundary tests, not a running API/provider/browser proof.

TheTechIdea.Data and the real PPDM model library compile through the test reference graph. The existing Models project also builds with zero errors against the relocated base/DTOs (four existing inherited-member hiding warnings and three missing analyzer warnings). The contract test verifies canonical assembly ownership and retained property notifications. Existing repository provider model/migration, access, binding, queue, client selection and hub/progress cases continue to pass. `git diff --check` passes.

Full BeepOilandGasApp/ServiceBase/DataManagementService interface/delegate wiring and API action metadata changes are source-only in this slice; the entire Client/API/Web projects were not built or hosted. Missing shared host sources remain outside the focused gate, per user direction. No SDK or missing-library stubs were added.

## Remaining gates and next work

Apply authoritative company/field row policies, table/column permissions and direct ID/mutation checks through the existing PPDM data/repository path. Review other raw/default domain connection paths, datasource/cache scope, client endpoint/envelope contracts and uncertain mutation retries across transports. Strengthen physical identity/provisioning before promoting persistent workflows, then proceed to the entity-scoped accounting close and broader company phases in the approved plans.

Actual HTTP controller-to-service integration, provider CRUD/import/export, scope/configuration concurrency, OIDC/browser behavior and distributed recovery remain unverified. Business/module schemas stay on Beep Migration; this slice adds no migration, handwritten SQL or live database changes. No complete foundation/company phase is closed.
