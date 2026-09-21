# Implementation 06: recorded royalty inputs and preview

Date: 2026-09-21. Supersedes the pending decision in [implementation 05](Implementation-05-Direct-Cost-Calculation.md). The user delegates best-practice decisions and confirms no legacy compatibility requirement; the [architecture policy](02-Architecture-and-Plan-Alignment.md) records that direction.

## Design and sources

Use separate preview, posting and reporting actions, sharing one calculation implementation. This is an architectural choice informed by traceable royalty reporting and explicit API side effects, not a claim that a particular jurisdiction mandates this UI.

- [ONRR revenue reporting instructions](https://www.onrr.gov/document/RRM-Chapter.4.pdf) illustrate separately identified sales volumes, values, royalty rates and deductions. Used as a domain reference, not as universal contractual or jurisdictional rules.
- [Microsoft API design guidance](https://learn.microsoft.com/en-us/azure/architecture/microservices/design/api-design) supports clear operation semantics and response behavior.
- [Microsoft idempotency guidance](https://learn.microsoft.com/en-us/azure/azure-functions/functions-idempotent) supports duplicate detection and safe retry design. Database-enforced posting idempotency remains required work, not a delivered claim.

## Implemented

The field royalty action is now POST `/api/accounting/royalty/preview`, with a single `PreviewRoyaltiesRequest` containing required FieldId and ProductionDate. Removed the old field calculation route and its override-rich request, the unused ProductionRoyaltyCalculationResult and generic SaveRoyaltyCalculationAsync method. Updated the typed Web client and dialog together; there is no compatibility endpoint.

The API requires authentication, resolves the actor from the authenticated subject and checks app-owned field access. LifeCycle selects active field tickets by inclusive production dates, verifies unique ticket/detail identities and a single active allocation per ticket, then calls RoyaltyService.PreviewAsync. Preview and posting share CalculateCoreAsync; preview returns before any repository write or journal call. The UI shows source allocation, lease, owner, volume, recorded rate and amount, and clearly states no posting occurs.

RoyaltyService now requires one recorded effective lease/owner interest, a source ticket date, BBL units and a positive recorded ticket price. It no longer fabricates a 12.5% rate, market price, ownership-derived interest record or percentage deductions. ROYALTY_RATE is interpreted as percentage points, so 1 means one percent. Open-ended effective dates work; overlapping or missing terms fail. Corrected the ticket lookup from nonexistent ALLOCATION_REQUEST_ID to RUN_TICKET_ID. Persisted calculation inputs now include source volume, unit, product, price and production date.

Lease-level deduction costs have no approved per-allocation deduction basis in the present contract. Nonzero candidate deductions therefore block calculation rather than being charged in full to every owner. Lookup failures propagate; they cannot become zero deductions. This is intentionally limited support, not a complete deduction engine or jurisdictional compliance claim.

## Verification

**56 focused tests pass**: previous 43 plus 13 royalty input/preview/orchestration cases. The tests execute the real RoyaltyService and PPDMAccountingService sources, real repository implementation and mocked data providers/services. They cover rates including 1%, prices, missing/overlapping/expired interests, bad units, unresolved deductions, lookup failures, preview without writes, and field/date/connection propagation through the orchestrator.

API build advanced beyond LifeCycle and UserManagement. Missing imports and the UserAssetAccess field mapping were corrected; field assets use ASSET_TYPE=FIELD and ASSET_ID. Three aggregation constructors had malformed assignments and were repaired. Latest full API probe: **53 errors / 134 warnings**, 11.99 seconds, now in API declarations (missing types/imports and setup-step contract mismatches). These newly exposed errors mean FG-00 remains open. Focused tests do not validate the full API host, real provider, Razor compilation or browser behavior.

Evidence: `%TEMP%/beep-royalty-input-tests.log`, harness `TestResults/royalty-preview.trx`, `%TEMP%/beep-oilgas-build-current.log`; harness locator `%TEMP%/beep-notification-harness-path.txt`. Module ownership guard passed on 92 files; whitespace check passed.

## Required next work

1. Resolve API declaration errors, then run normal API/Web tests and Razor build; remove dependence on temporary harness evidence.
2. Harden posting as an explicit command accepting persisted allocation identity, reloading authoritative input, enforcing app permissions and approval, and using database idempotency plus atomic persistence/journal recovery. The existing single-allocation posting endpoint has not been certified by this slice.
3. Complete currency/valuation provenance, approved allocation-specific deductions, contract types and supported products. Current preview is oil/BBL with recorded ticket valuation; it does not cover gas or every royalty agreement.
4. Fix reporting field/pool resolution: existing reporting still maps field IDs to lease IDs and pool IDs to royalty-interest IDs. It must follow authoritative ownership/allocation relationships.
5. Complete FG-11's role bridge and review all field access paths, including deny/expiry behavior and the separate FieldAccessService global-scope handling. Compilation repairs do not certify its authorization semantics.
