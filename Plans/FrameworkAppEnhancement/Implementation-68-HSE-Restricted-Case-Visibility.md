# Implementation 68 — recorded HSE case visibility

Date: 2026-10-06. Continues [Implementation 67](Implementation-67-Atomic-Corrective-Action-Addition.md). CA-03/05/06/32; FG-10/11/12/20/40/42. The [root tracker](../../MASTER-TODO-TRACKER.md) owns the full plan; this checkpoint closes no feature or phase.

## Existing owners and recorded evidence

The actual [company row policy](../../Beep.OilandGas.ApiService/Services/CompanyPPDMRowAccessPolicy.cs) already proves current app HSE.View, inherited entitled FIELD access, a unique active primary FIELD component/header and canonical role/type references. It now reviews RESTRICTION_ID/RESTRICTION_VERSION on every active component of each candidate case, including non-primary components. A primary FIELD association cannot conceal another component's restriction.

RESTRICTION is versioned by its full two-column key. Its classification/type/optional-duration references, current UTC dates and source-document identity are recorded facts. REST_ACTIVITY requires its complete three-column key and actual approved activity reference. REST_TEXT requires its complete three-column key and unique current observations. The existing [Beep evidence reader](../../Beep.OilandGas.ApiService/Services/CompanyPPDMEvidenceReader.cs) validates exact predicates, canonical keys/reference metadata, provider success and bounded evidence. Snapshots freeze declared model columns before subsequent provider awaits; reference/definition/text changes invalidate the reviewed fingerprint. Freeform text is hashed and never executed as policy.

REST_CONTACT records contacts, not a read-permission grant. USER.BUSINESS_ASSOCIATE_ID has no reviewed per-database personal-identity binding and is not guessed into case authority. A definition's operational/legal meaning also does not automatically create a data-read permission.

The metadata JSON declares the restriction/versioned child primary keys, but represents some relationships using the ID column alone. Reads enforce the full ID/version and actual logical parent identities. Focused metadata checks do not certify installed composite foreign keys or real schema conformance; that provider gate remains open.

## Explicit company interpretation and app authority

Extend the existing [Data-owned company interpretation configuration](../../TheTechIdea.Data/OilGas/PPDMFieldEligibility.cs), rather than adding a policy store. The existing Program options binding is PPDM:FieldDataAccess. Each company may declare HseCaseReadRestrictions. Each entry requires DatabaseFingerprint, RestrictionId, RestrictionVersion, DefinitionFingerprint and RequiredPermission. The selected company and physical reviewed database qualify the rule; exactly one matching rule must identify the current complete definition.

RequiredPermission must be one registered app permission: HSE.View or HSE.ViewRestricted. HSE.View is an explicit reviewed decision that ordinary authorized case readers may see this definition; it is never an implicit default. HSE.ViewRestricted is added to the existing [app permission catalog](../../Beep.OilandGas.Models/Data/Security/PermissionConstants.cs). The existing reflection-based role catalog recognizes it; no live assignment is made. Current app RBAC resolves actor and inspected-subject permissions. An Administrator's permissions do not substitute for the subject's permission, and IdentityServer remains authentication-only.

Missing, null, duplicate, unknown, stale or database-mismatched interpretations refuse. Unknown/orphan versions, invalid/non-UTC/event-dependent dates and ambiguous reference/component evidence refuse. A valid reviewed case whose subject lacks its required app permission supplies no readable predicates. The same policy governs all canonical incident-family reads and linked corrective-action project/step/status/participant reads and commands. Generic mutation fences remain intact. Actor authority withdrawals require a fresh request under the existing scope owner; fresh requests still apply case permissions independently.

Configuration bindings are frozen before app-role provider awaits and checked again afterwards. Definitions, components, incident headers/primary relationships, company/database and required subject permissions are checked again before returning predicates. These fences reject observed changes; they do not establish a cross-process transactional snapshot or performance certification.

## Review route and configuration procedure

The shared [HseRestrictionReview contract](../../TheTechIdea.Data/OilGas/HseRestrictionReview.cs) contains company/database/FIELD/incident qualification and restriction ID/version/fingerprint. The existing [HSE controller](../../Beep.OilandGas.ApiService/Controllers/HSE/HSEController.cs), [field facade](../../Beep.OilandGas.LifeCycle/Services/HSE/PPDMHSEService.cs), actual profile/account gateway and row policy expose GET /api/field/current/hse/incidents/{incidentId}/restrictions/review. No alternate controller, role engine or business store is introduced.

Review requires current HSE.ViewRestricted, HSE.ManageIncidents and ordinary HSE.View plus inherited eligible authority to the exact reviewed FIELD. It may review a currently unmapped definition through this controlled read-only boundary. A foreign incident or inspected-subject metadata request is refused. The response exposes qualified fingerprints, not freeform restriction text or participant payloads, and does not update rules or grants.

1. An already authorized reviewer requests the actual case's review route under its current company/FIELD selection.
2. Review the actual source definition and applicable company policy; choose the permitted app read permission explicitly. The returned fingerprint itself is not an approval decision.
3. Add the qualified ID/version/fingerprint/permission to that company's existing configuration source. Preserve the current database fingerprint and full definition fingerprint exactly; do not infer identifiers from display names.
4. Re-review any changed definition or database binding. A stale entry cannot authorize the changed case.

A configuration administration UI, persisted configuration revisions/change audit, delivery/reload behavior and operational review procedure remain full-plan work. This checkpoint supplies the controlled review contract and fail-closed interpretation, not live configuration deployment or permission provisioning.

## Complete population and existing KPI owner

The [existing Data row-policy contract](../../TheTechIdea.Data/OilGas/IPPDMRowAccessPolicy.cs) adds RequireCompleteReadScopeAsync with an exact FIELD ID. Other implementations refuse by default instead of pretending to prove completeness. The actual company owner requires current inherited access to that FIELD, a current actual FIELD target, identifiable case/header evidence and every in-scope current case's required read permission. Missing header evidence cannot become a zero-case total; explicitly inactive headers are excluded. The final current target/inventory is compared with the initial population, and grants are checked after the final provider inventory. Withheld cases, inventory change or grant withdrawal refuse the proof.

The actual [HSE KPI owner](../../Beep.OilandGas.PPDM39.DataManagement/Services/HSE/HSEKPIService.cs) requests complete-population proof before and after materializing its field incident IDs. CountInjuriesAsync no longer converts every exception into zero. Focused tests exercise the actual policy proof; KPI wiring compiles. Public rates still refuse because the approved exposure source is unavailable. The existing tier/severity, injury-role, population/time interval, denominator and stub metrics require their existing scientific owners to be repaired and verified. This checkpoint does not certify mathematical KPI outputs or a transactional reporting snapshot.

## Verification

Final-source acceptance: **135 passed, zero failed/skipped, terminal exit 0, in 2 minutes 53 seconds**. Log: `%TEMP%/oilgas-hse-restrictions-final-source.log`. This includes all 77 incident-policy cases (40 new), the withheld linked-CA command case, all six new actual review HTTP cases, both company/database preference cases and all 49 existing HSE account/controller checks. The latter includes all 45 active-account refusal cases across fifteen HSE write actions. The real row-policy completeness proof covers missing targets/header relationships, new cases and late grant/restriction changes; mathematical KPI outputs remain unverified. Focused compilation has zero errors and nine known warnings: four absent external analyzer projects, four existing HSE nullable assignments and the existing refusal-handler unnamed-enum warning.

Final-source access/registry/profile/hierarchy/eligibility gate: **713 passed, zero failed/skipped, terminal exit 0, in 2 minutes 14 seconds**. Log: `%TEMP%/oilgas-hse-restrictions-access-final.log`. This supersedes the one older snapshot fixture failure and adds the same-database preference restoration case.

Final-source dependent HSE/HAZOP/corrective-action policy gate: **341 passed, zero failed/skipped, terminal exit 0, in 10 minutes 58 seconds**. Log: `%TEMP%/oilgas-hse-restrictions-policy-final.log`. All 189 HSE/facade boundaries, 83 HAZOP authority cases and 68 corrective-action relationship cases pass, plus the actual atomic three-record addition/retry/conflicting-intent case. Together with final acceptance and access, these runs own acceptance for the final source. No assembly was rebuilt while its testhost was live; independent no-build runs used the same compiled source.

The earlier extended HSE/HAZOP/corrective-action/lifecycle gate completed with **488 passed, zero failed/skipped, terminal exit 0, in 30 minutes 14 seconds**. Log: `%TEMP%/oilgas-hse-restrictions-regression.log`. All 88 command cases and the previous relationship/authority/current-FIELD cases passed on that compiled snapshot. The subsequent source change tightens the final population inventory/grant ordering; the final-source gates own acceptance for that change. Scopes overlap across runs and are not a unique summed total.

Earlier acceptance logs remain honestly recorded as exit 1: the UTC fixture assigned the same timestamp with a different Kind, which PPDM's equality-based setter ignored; an older database-switch fixture relied on regenerating ConnectionProperties GUIDs. The corrected fixture uses a different non-UTC timestamp and an explicit actual binding revision change. The earlier 82-case acceptance (81 passed) and 712-case access run (711 passed) are not relabelled successful aggregates. Both corrections are covered by the final-source gates above. Initial checkpoints establish mapped primary/secondary access and actual permission/CA/subject denial. The shared test fixture now retains one configured datasource and connection GUID across fresh company requests; it had previously changed the database fingerprint accidentally. The existing database-snapshot test now changes the actual recorded binding revision explicitly. A companion test proves returning to the same company/database restores only that company's FIELD preference. No production role/binding workaround or artificial grant is introduced.

## Remaining full-plan requirements

- Complete participant/company/key/reference policies, reviewed personal identity and participant role interpretation, and attachment/blob/provenance ownership. A whole-case read boundary does not certify each participant payload or attachment.
- Canonical new-plan project + incident-response + roster creation, approval/closure, remaining typed HSE commands and scientific/exposure inputs through their existing owners.
- Durable retained intent/outbox, batch/process/history/workflow consistency, configuration revision/change administration, authoritative reference pickers and client circuit recovery.
- Canonical Data consolidation, active client/API rollout, actual Program/concrete orchestrator/DI/OIDC/Razor/browser acceptance, physical provider/schema/isolation/concurrency and bounded large-inventory/performance acceptance. The actual loopback HTTP fixture has explicit authentication/orchestrator/recording-provider boundaries.

No handwritten SQL, new business EF path, schema migration, alternate business store, live migration/grant, commit/push or deployment is introduced. Business schema ownership remains Beep MigrationManager; the existing identity/profile/access registry keeps its approved owner. The full master plan remains active.

Documentation validation: nine updated UTF-8 documents and all 291 local links resolve. No unfinished Implementation 68 test-gate markers remain. `git -c core.safecrlf=false diff --check` passes.

## Subsequent participant evidence

[Implementation 69](Implementation-69-HSE-Participant-and-Crew-Evidence.md) continues the existing case policy with recorded full participant keys and current role/status/person/company/crew/period/WELL evidence and exact unique investigator reads. Its focused results belong to that checkpoint; the historical results above retain their original scope. Participant disclosure and app-user identity, typed writes, attachments, configuration administration, scientific exposure, provider/host/browser acceptance and the full plan remain open.
