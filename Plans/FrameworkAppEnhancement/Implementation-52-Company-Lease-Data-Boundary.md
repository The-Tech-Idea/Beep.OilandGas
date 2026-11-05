# Implementation 52 — Company lease data boundary

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/10/11/12/20. Status: canonical lease acquisition service implemented with focused evidence; multi-record command completion, advanced domain decisions and full runtime/provider acceptance remain open. No phase closure.

Follow-up: [Implementation 53](Implementation-53-Lease-Transaction-and-Readback.md) supersedes the sequential-write fixture with owned Beep transaction/rollback/readback orchestration. Use its remaining-work list and the root tracker for current driver conformance, durable retry/history concurrency and command acceptance gates. The 1302-check evidence below remains historical to this implementation.

## Problem and ownership

The existing lease summary path joined LAND_RIGHT by ID without its subtype, used AREA_ID as a FIELD association and called a fabricated opportunity recommendation to populate stored status. Its optional connection resolver allowed a global default. Creation discarded owner/economic terms and omitted explicit company/FIELD evidence; status updates did not consistently carry complete keys or recheck current resource authority during repository preparation.

Extend the existing LeaseAcquisitionService and its canonical ILeaseAcquisitionService methods. The [actual constructor partial](../../Beep.OilandGas.LeaseAcquisition/Services/LeaseAcquisitionService.Context.cs) makes company access mandatory; both API interfaces still resolve that same owner. No second lease repository, command store or compatibility path is introduced. Move the existing interface, LeaseSummary, CreateLeaseAcquisition and UpdateLeaseStatusRequest to TheTechIdea.Data, preserving their namespaces and removing their original definitions.

The [API company lease gate](../../Beep.OilandGas.ApiService/Services/PPDMCompanyLeaseDataAccess.cs) uses the existing CompanyModuleConnection, CompanyPPDMEvidenceReader, FIELD row policy and PPDMGenericRepository. Its [shared contract](../../TheTechIdea.Data/OilGas/CompanyLeaseDataAccess.cs) belongs to Data. Business/module schema ownership remains Beep MigrationManager; the existing approved app Identity/access registry remains its EF owner. This change adds no schema, SQL, live migration or live datasource write.

## Reviewed meaning and exact evidence

Each company's existing PPDMFieldEligibilityConfiguration must explicitly provide LeaseLandRightSubtype, LeaseCompanyComponentType and LeaseStatusType, alongside its reviewed TimeZoneId. No production codes or defaults are seeded. Require exact canonical metadata primary keys and association/reference relationships; require active/current R_LAND_COMPONENT_TYPE and agreement/status reference records when used. Existing companies with no reviewed lease configuration receive an explicit conflict when using this workflow.

| Record | Complete key |
|---|---|
| LAND_RIGHT / LAND_AGREEMENT | LAND_RIGHT_SUBTYPE + LAND_RIGHT_ID |
| LAND_STATUS | LAND_RIGHT_SUBTYPE + LAND_RIGHT_ID + STATUS_TYPE + LAND_RIGHT_STATUS + STATUS_SEQ_NO |
| LAND_RIGHT_FIELD | LAND_RIGHT_SUBTYPE + LAND_RIGHT_ID + FIELD_ID |
| LAND_RIGHT_COMPONENT | LAND_RIGHT_SUBTYPE + LAND_RIGHT_ID + COMPONENT_OBS_NO |

Read only exact FIELD or lease requests with supported named filters. LAND_RIGHT_COMPONENT must match the selected company's BUSINESS_ASSOCIATE_ID, reviewed component code/subtype, active indicator and effective window. A unique current LAND_RIGHT_FIELD link establishes the recorded FIELD relationship; actual current FIELD entitlement/app access is independently required. A multi-FIELD association cannot be flattened into the existing single-FIELD summary. Complete duplicate/missing/fractional keys, ignored provider predicates, dangling links, ambiguous status and invalid term units are errors.

Evaluation now returns persisted facts. Current recorded LAND_STATUS determines operational status; missing status stays absent, and future/inactive/expired history cannot replace a current record. ACTIVE_IND does not imply approval. The summary carries subtype and reviewed date zone. CALCULATED_INTEREST is returned under its canonical name, without percent conversion or a working-interest claim; WorkingInterest is nullable and absent until its financial owner provides evidence. Creation requires an explicit CalculatedInterest, including a valid explicit zero, and rejects supplied working/net/royalty interests or owner/government terms that this core registration cannot persist through their canonical workflows.

The company component proves an application data association only. It does not establish title, mineral/surface ownership, operatorship, billing rights, JV participation or a legal approval. LAND_TITLE.TITLE_HOLDER is a free string with no reviewed business-associate relationship; neither it nor recorded percentages can supply those decisions.

## Mutation boundary

Creation uses the authenticated app account and current company Administrator plus actual FIELD WRITE access. It writes the existing LAND_RIGHT, LAND_AGREEMENT, LAND_STATUS, LAND_RIGHT_FIELD and LAND_RIGHT_COMPONENT shapes with complete identities and explicit requested dates. No LAND_TITLE or operator/financial record is synthesized. Both existing controllers use standard Administrator mutation guards, while the service independently verifies live app authority.

Status changes require canonical uppercase codes and current company association/FIELD links. The existing repository reads previous records by complete keys, prepares common columns with the actual actor and invokes a command-bound row policy before each provider write. That policy snapshots every key part before preparation and refuses subtype/ID/sequence/observation/company/FIELD changes, missing actors, revoked resource authority, changed interpretation or references. Update preparation also re-reads current company association/FIELD evidence. Status history appends a new sequence using current recorded term evidence, preserving future rows without treating their term as current.

Before returning evidence, the service re-reads associations and FIELD links and rechecks FIELD/current-company/binding authority. Mid-operation revocation or reparenting cannot retain an old result merely because the original objects remain in memory.

## Consumer adjustment

The existing lease page displays the raw recorded calculated-interest value and asks for an explicit nullable value with a required MudBlazor numeric input. It sends ACTIVE rather than the rejected mixed-case status alias. No CSS, JavaScript or theme source is added. The remaining page/client state, default agreement selection, role/context guards, typed command envelopes/readback and current-field endpoint assertion are still open; this small adjustment is not a completed lease experience.

## Verification

The [focused production-source project](../../Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj) includes the actual shared constructor/canonical service partials and API gate. The [lease cases](../../Beep.OilandGas.Repository.Tests/CompanyLeaseDataTests.cs) exercise real SQLite app membership/grant/role authority, existing FIELD entitlement/policy, canonical metadata and a recording Beep IDataSource. Existing LeaseRoutingTests require the mandatory company gate. They do not stub an advanced recommendation into the canonical owner.

Final focused gate: **1302 passed, 0 failed, 0 skipped**, adding 67 cases to the previous 1235 (65 lease cases and two source-linked routing cases). The final source run took 2 minutes 26 seconds. All prior focused authority/client/administration cases remain green. The two exact edited Razor fragments compile against actual MudBlazor 9.7.0 with zero warnings/errors using [Test-LeaseWebMarkup.ps1](../../Scripts/Test-LeaseWebMarkup.ps1); this is isolated markup compilation, not full-page/services/auth/browser proof. The prior Implementation 51 static rendering evidence remains historical.

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger "console;verbosity=minimal"
./Scripts/Test-LeaseWebMarkup.ps1
git diff --check
```

Logs: `%TEMP%/oilgas-company-lease-focused.log`, `%TEMP%/oilgas-lease-markup.log`. A module build probe also fails in existing PPDM39.DataManagement dependencies on missing diagnostics/Beep editor/migration types; no dependency substitute or unrelated repair is introduced. Full host/DI/controller/auth/HTTP/client assemblies and real-provider behavior remain unverified.

## Required next work

1. Complete multi-record writes through the existing Beep transaction/command owner: atomicity, expected revision/history concurrency, idempotency, provider errors/rollback and confirmed readback. Current inserts/updates are sequential; a late refusal/failure can leave earlier records written. The recording fixture proves refusal before the affected write, not rollback or physical persistence.
2. Resolve canonical LAND_AREA parent/foreign-key requirements and explicit area identity through the existing contract/setup owners. The metadata relationship uses a four-part area key; FIELD_ID must never be substituted for AREA_ID. The five recorded creation writes do not prove real-provider FK/order acceptance.
3. Repair the actual lease SDK routes/local-mode branches and Web command envelopes, stale-result behavior, selected-company/standard-role guards and current-field controller assertion. Exercise actual HTTP and consumer readback using the same shared contracts.
4. Replace fabricated advanced prospect/title/rights/compliance/financial outputs through the existing advanced methods and canonical workflow owners. The canonical EvaluateLeaseAsync no longer invokes those fabricated methods; the other advanced paths are not repaired or validated here.
5. Continue the already inventoried LeaseEconomicInterestService, JointInterestBillingService and JointArrangementsService connection/resource/actor/reference/default work. Keep their existing economic/posting owners. Provisioning, historical/unowned authority, remaining domain/local-mode/cache and the broader CA phases remain open before period close/release promotion.
