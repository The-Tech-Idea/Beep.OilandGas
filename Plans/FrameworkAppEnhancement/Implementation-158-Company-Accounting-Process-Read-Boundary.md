# Implementation 158: Company accounting process read boundary

## Existing process owner and company scope

The existing PPDM process persistence stores PROCESS_DATA in PROCESS_INSTANCE.INSTANCE_DATA_JSON. A company accounting process can carry explicit ownership in that existing payload rather than inventing a FIELD association or a parallel workflow/task store. The Data-owned CompanyAccountingProcessBinding defines original company, process instance and exact AP_INVOICE/PURCHASE_ORDER source identity under the CompanyAccountingBinding payload key.

Binding validation requires the exact original tenant/company and process/source identities, supported entity type, bounded nonblank identities and one complete unambiguous JSON shape. Duplicate, extra, missing or incorrectly cased ownership properties refuse. Default PPDM process serialization uses this exact property shape; no alternate DataJson/backing-data fallback is accepted. This binding is an ownership contract, not a signed authorization token or permission grant.

No stored process is assigned ownership or converted in this slice. An authenticated future creation/submission command must establish the company binding from authoritative sources; a client-supplied binding alone cannot authorize creation. Existing unowned processes cannot be interpreted as current-company accounting processes.

## Canonical API read helper

CompanyProcessReadService adds ReadAccountingAsync alongside its existing FIELD read operations. It requires an authenticated principal, standard current app roles, original company/actor and fixed binding proof. AP invoices require Accounting.View and purchase orders require Procurement.View. The caller must capture both the original accounting/process database bindings and provide the canonical native source reader; native source revisions are not accepted from client input.

A company process must have no FIELD ID, one explicit payload ownership binding and a matching PROCESS_DATA.InstanceId. FIELD-owned, foreign, unsupported or incomplete processes refuse before native accounting reads. The reader invokes the native source boundary with the original company/actor and exact recorded entity identity. It rechecks authority before process reads, before native source reads and after source reads. It captures the process and native source twice and compares content revisions before returning the frozen closing process. Missing or changed closing evidence cannot be published.

This helper deliberately does not call FIELD/resource access checks for company accounting records. Their ownership and authority must come from the company process binding and native AP/PO source owner. Existing FIELD operations retain their original checks and remain covered in the focused gate. Two reads detect observed changes; they are not an atomic cross-module snapshot.

## Focused evidence

Current complete Data sources build with zero warnings/errors in 1.82 seconds. The extended Test-CompanyProcessReads.ps1 gate passes **35 tests**, zero failed/skipped, terminal exit 0, in 1.1867 seconds, with zero compiler warnings/errors. Log: `%TEMP%/oilgas-company-accounting-process-158.log`. Whitespace verification passes.

Twenty-six new cases extend the nine existing FIELD process read checks: invoice/PO company reads and frozen JSON payloads, eight foreign/missing/FIELD/source ownership faults, six current authority revocations, four changed/removed/invalid closing-source cases, missing process behavior and five strict JSON shape refusals. Strict mocked resource checks confirm that company reads never invoke selected-FIELD access. Process and native revision boundaries are explicit fixtures; no actual provider, persisted company process or full endpoint is claimed.

The actual Data contract and complete CompanyProcessReadService compile and run. HTTP/context adapter/module binding integration, process/task persistence, full parent/client/role bridge/browser/RTL/accessibility and actual provider acceptance remain unverified. No schema migration, SQL, EF owner, stored process/task, role grant or financial write is introduced. The prior allocation screen and definition gates remain scoped historical evidence and are not summed.

## Next visible reviewer integration

Wire this existing read owner into the company accounting API/inbox with captured PRODUCTION and LIFECYCLE bindings and actual AP/PO source delegates. Keep FIELD and company processes explicitly scoped in the same process/task owner, then route assigned work to the existing accounting source detail. Add authoritative invoice-preparer separation, accepted receipt/policy evidence, durable draft/submission identity, reviewer assignment and provider-backed concurrent consumption. Reconcile uncertain outcomes through recorded command/readback owners. Independent match/approval/post/payment and all remaining persona/multinational requirements stay open; no phase closes.
