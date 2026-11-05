# Implementation 159: Company accounting process HTTP

## API and existing process client

GET /api/accounting/processes/{id} now reads an explicitly company-owned AP_INVOICE or PURCHASE_ORDER process without relying on current FIELD selection. It uses the existing CompanyProcessReadService, canonical native invoice/order services and recorded company process payload. The route decodes the original escaped process ID exactly once, preserving international, slash and literal percent identities.

The actual controller requires current standard app roles and the relevant source read permission through the shared helper. It resolves native source evidence with the original company/actor and verifies source revisions/ownership before publication. Missing processes return NotFound; denied current authority returns Forbidden; foreign/incomplete/changed evidence returns a fixed Conflict. No process, allocation or task is created by this GET.

The existing BusinessProcessServiceClient/IBusinessProcessServiceClient adds GetCompanyAccountingInstanceAsync for this route. It captures company selection, encodes IDs, refuses late company changes and validates exact process/source payload ownership before returning a result. No second client/store or FIELD fallback is introduced. This complete client is source-only in the focused verification.

## Captured PRODUCTION and LIFECYCLE targets

CompanyAccountingProcessReadContext reuses CompanyModuleConnection, ModuleConnectionResolver and BoundProcessService. It captures the selected company's PRODUCTION binding and explicit LIFECYCLE connection/fingerprint, creates a fixed process service for that expected connection and rechecks both module bindings/actor/company before returning the context. Program registers this adapter under ICompanyAccountingProcessReadContext.

BoundProcessService adds an expected-connection overload; a changed/empty resolver target refuses before constructing the fixed native service. The existing no-argument behavior delegates to the same owner. A non-BoundProcessService production registration refuses rather than introducing an alternate connection path. Subsequent helper captures compare the combined original module binding while the actual process reads stay on the original fixed service.

The production adapter, Program/DI and actual fingerprint/provider behavior remain source-only; they are not proven by HTTP fixtures. The HTTP host supplies an explicit authoritative context/process fixture. Do not treat its binding string as actual live module proof.

## Focused verification

The complete controller/shared read helper and actual native AP/PO services/repository compile and execute in the affected Razor/controller gate. **451 tests pass**, zero failed/skipped, terminal exit 0, in 6.1404 seconds. Log: `%TEMP%/oilgas-accounting-process-http-159.log`. Fourteen new loopback Kestrel HTTP cases extend the previous 437 tests; the gate now includes 63 controller HTTP cases. They cover AP/PO source reads, anonymous/current role/permission denial, missing/foreign/FIELD scope, changed binding/process/native source, foreign native records and encoded process IDs across two company owners. No insert/update/delete/transaction/commit interaction is admitted by these cases.

The separate actual BoundProcessService/company read gate passes **38 tests**, zero failed/skipped, terminal exit 0, in 0.9552 seconds with zero compiler warnings/errors. Log: `%TEMP%/oilgas-process-binding-159.log`. Three new expected-connection/fixed-service checks extend the prior 35 company/FIELD cases. These scopes overlap and are not summed as one count. Whitespace verification passes.

The controller gate now imports cached process/interface metadata globally while explicit actualData aliases select current canonical AP/PO entities. Its 261 warnings comprise 125 actual-source precedence warnings, 93 prior obsolete-account warnings, three existing accounting-basis switch warnings and 40 existing nullable dependency warnings. Source precedence warnings increased because cached Models is visible for process contracts; current compiled source wins. No controller/helper nullable warning or compilation error is present. Full Models/Data consolidation remains open.

Data sources are unchanged from the previous clean 1.82-second complete Data build. Full production context/DI/role bridge/host/client, actual process persistence/native provider, proxy/browser/RTL/accessibility and performance remain unverified. No SQL, EF path, migration, stored workflow, task assignment, grant or financial command is executed.

## Next visible handoff

Consume this read in the existing company accounting source/assigned-work experience, explicitly route company accounting tasks independently of FIELD tasks in the same inbox owner and preserve exact process/source identities. Add authoritative persisted company process creation and durable allocation submission/reviewer task routing; require actual invoice-preparer separation, acceptance/policy and concurrent consumption before matching decisions. Definition/schema reconciliation and recorded/not-recorded/uncertain recovery remain open. No persona journey or roadmap phase closes.
