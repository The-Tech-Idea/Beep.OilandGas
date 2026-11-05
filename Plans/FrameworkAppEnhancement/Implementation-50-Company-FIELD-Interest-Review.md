# Implementation 50 — Company FIELD interest review

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/11/12/20. Status: API/SDK implemented with focused verification. Administration UI, legal/operator/JV decisions and provider acceptance remain open.

## Problem and resulting behavior

Company FIELD data entitlement establishes application data access, but it does not describe the company's recorded interest in the resource. INTEREST_SET and its components/partners have composite identities. Joining on INTEREST_SET_ID alone can mix sequences, and returning every partner can disclose another company's records.

The existing access facade/controller/SDK now exposes `GET /api/AccessControl/fields/{fieldId}/interest-review` through a shared [TheTechIdea.Data contract](../../TheTechIdea.Data/OilGas/FieldInterestReview.cs). The [API reviewer](../../Beep.OilandGas.ApiService/Services/PPDMCompanyFieldInterestReview.cs) requires a current selected-company Administrator and that actor's actual FIELD grant/entitlement. It reads canonical PPDM records through the existing Beep repository and returns only current selected-company partner records, with their complete interest-set/component keys and recorded percentages/roles. Empty Sets is an explicit successful result when no current matching company interest is recorded.

The review grants no title, operator, billing or additional data access. Canonical recorded roles and percentage values are returned without interpretation, normalization or financial calculation. It creates no second interest, grant, land-right or financial persistence owner. IdentityServer remains authentication-only; standard endpoint roles and live application/company authority are independently checked.

## Canonical identities and relationships

The checked-out [PPDM metadata](../../Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/PPDM39Metadata.json) and canonical models own these records:

| Record | Complete identity / review relationship |
|---|---|
| INTEREST_SET | INTEREST_SET_ID + INTEREST_SET_SEQ_NO; configured INTEREST_SET_TYPE. |
| INT_SET_COMPONENT | INTEREST_SET_ID + INTEREST_SET_SEQ_NO + COMPONENT_OBS_NO; exact requested FIELD_ID and configured COMPONENT_TYPE. |
| INT_SET_PARTNER | INTEREST_SET_ID + INTEREST_SET_SEQ_NO + PARTNER_BA_ID + PARTNER_OBS_NO; PARTNER_BA_ID equals selected company. |
| R_INT_SET_COMPONENT | Exact active configured COMPONENT_TYPE. |
| R_INT_SET_TYPE | Exact active configured INTEREST_SET_TYPE. |
| R_INT_SET_ROLE | Exact active recorded INTEREST_SET_ROLE for each returned company partner. |

Metadata declares the INTEREST_SET_ID foreign key without the full sequence relationship; queries explicitly retain INTEREST_SET_SEQ_NO from the canonical composite models. Metadata shape, primary-key sets and declared reference triples are checked before provider access. Every evidence query has exact equality/active predicates; provider results must honor them. Numeric keys compare invariant decimal values, so scale differences do not split one key. Fractional/negative observation numbers, missing raw numeric columns and duplicate complete keys are refused rather than defaulted or collapsed.

INT_SET_COMPONENT.BUSINESS_ASSOCIATE_ID represents a linked component associate and does not prove ownership of the entire interest set. The current company's partner row supplies the company relationship. Components returned are the configured FIELD links for that complete set identity; other companies' partner rows are excluded.

## Configuration, dates and response integrity

The existing `PPDM:FieldDataAccess` company configuration adds `InterestReviewComponentType` and `InterestReviewSetType`. They must be explicitly reviewed exact codes in R_INT_SET_COMPONENT and R_INT_SET_TYPE. The selected TenantId/BusinessAssociateId pair must match exactly one configuration entry. `TimeZoneId` is the existing explicitly reviewed company date zone. These review fields have no production defaults, seeds or aliases and are required only when interest review is requested. Existing FIELD entitlement configuration remains independent.

Set/component/partner EFFECTIVE_DATE is required. Effective dates are inclusive, EXPIRY_DATE is exclusive, and partner INACTIVE_DATE is an additional exclusive end boundary. UTC values are instants; Unspecified dates use the reviewed zone. Local, ambiguous/invalid DST and inverted ranges are conflicts. Reference effective/expiry boundaries are nullable. Current status and time windows are checked again after the final FIELD policy validation.

The new internal [evidence reader](../../Beep.OilandGas.ApiService/Services/CompanyPPDMEvidenceReader.cs) centralizes the existing entitlement reader's datasource, metadata, date and predicate checks. The [FIELD prerequisite](../../Beep.OilandGas.ApiService/Services/PPDMCompanyFieldEligibility.cs) uses that same helper, preserving one Beep repository path. Captured actor/company/database scopes are required constructor inputs; scope changes cannot be adopted after awaited metadata reads. Actual cached datasource target checks occur before open and across evidence reads. Provider-reported failures, failed opens, binding changes and revoked current company grants remain refusals/errors.

Each evidence lookup refuses more than 256 rows, and the returned sum of sets/components/company partners must not exceed 256. These are checked result limits, without silent truncation; provider-side pagination or bounded transfer is not proven. The final FIELD policy validation rechecks the administrator's current app grant and company data entitlement. This is not an atomic cross-table provider snapshot.

The existing [SDK access client](../../Beep.OilandGas.Client/App/Services/AccessControl/AccessControlService.cs) uses captured-company transport and the canonical shared review contract. Before deserialization, required raw sequence/observation/percentage fields must be present and numeric, preventing missing JSON numbers from becoming zero. Scope/field/fingerprint/server UTC timestamp, complete keys, company partner identity, cardinality and duplicate rows are validated. Wrong-company or malformed responses remain explicit client errors. No local datasource or alternate DTO path is added.

## Domain owner audit and remaining implementation order

The canonical LAND_RIGHT key is LAND_RIGHT_SUBTYPE + LAND_RIGHT_ID and has no company-owner column. LAND_RIGHT_FIELD adds FIELD_ID; LAND_RIGHT_COMPONENT adds COMPONENT_OBS_NO and generic associate/interest links. A FIELD link, calculated interest or recorded partner role alone cannot be promoted into legal title or operatorship. Land-right evidence and explicit domain approvals still require implementation through their existing owners.

Financial owners also remain separate:

| Existing owner | Finding and required follow-up |
|---|---|
| [LeaseEconomicInterestService](../../Beep.OilandGas.ProductionAccounting/Services/LeaseEconomicInterestService.cs) | Owns OWNERSHIP_INTEREST / ROYALTY_INTEREST / DIVISION_ORDER economic fractions. Extend its authoritative company/resource boundary; do not calculate those fractions from this review response. |
| [JointInterestBillingService](../../Beep.OilandGas.ProductionAccounting/Services/JointInterestBillingService.cs) and [API registration](../../Beep.OilandGas.ApiService/Program.cs) | Still accept/default caller connectionName to PPDM39; registration supplies no module resolver. Lease allocation reads scan active ALLOCATION_RESULT then inspect RUN_TICKET.LEASE_ID. Statement GENERATED_BY and OPERATOR come from supplied userId, and absent period start falls back to periodEnd.AddMonths(-1). Company/module/resource/actor and explicit period/operator boundaries remain work in this existing owner. |
| [JointArrangementsService](../../Beep.OilandGas.Accounting/Services/JointArrangementsService.cs) | Owns cost postings with a PPDM39 connection path, supplied actor, default JOINT_ARRANGEMENT reference and fallback accounts. Audit and extend this owner rather than introduce another posting service. |

Next, consume the verified review API/SDK in the existing FIELD administration experience, with selected-company cancellation and honest empty/error states. Then implement explicit legal/operator/JV evidence and approval commands through canonical owners, and close the financial/domain/local-mode/gateway/cache gaps before provisioning and CA-04 period close. Unowned/old-binding grant provenance and ordinary grant/revoke history remain tracked separately. This audit identifies remaining work; it does not mark those services repaired or any phase complete.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
```

**1184 passed, 0 failed, 0 skipped**, adding 62 cases to the previous 1122; final duration 1 minute 31 seconds. [New review tests](../../Beep.OilandGas.Repository.Tests/CompanyFieldInterestReviewTests.cs) execute the actual reviewer, shared evidence reader, FIELD policy, persisted company authority, facade/controller and SDK/shared JSON through the existing in-process dispatcher. Coverage includes complete composite identities, foreign company exclusion, current/inactive/expired/missing records, exact vocabulary and date zones, reference/metadata drift, duplicate/missing raw numeric keys, provider predicate bypass/errors, decimal scale, response limits, revoked grants/entitlements/membership/bindings, final status changes and malformed SDK responses. All existing entitlement/admission/editor/policy tests remain in the complete focused gate after the shared-reader refactor.

Tests use SQLite application authority and a recording Beep datasource, not a live HTTP host or production PPDM database. Fixture vocabulary/interest records are test-only. No Razor source changed; Implementation 48's component compilation is historical, not a current review UI check. Full host/DI/options/OIDC/HTTP/browser/provider upgrades/concurrency and transactional snapshots remain unproven. Missing shared projects are not manufactured.

No schema migration, handwritten SQL, live datasource write or deployment occurs. Business/module schema work remains Beep MigrationManager; the approved existing Identity/access registry retains its EF owner. The reviewer adds no migration or persistent table. Root CA/FG phases and the broader approved plan remain open.
