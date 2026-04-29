# ProductionAccounting Master Tracker

## Phase Rollup

- [x] Phase 0 - Baseline context + inventory
- [x] Phase 0.1 - Behavior map (implemented vs compatibility vs placeholder)
- [x] Phase 1 - Interface and API surface normalization plan
- [x] Phase 2 - Data access + seed strategy definition
- [ ] Phase 3 - Service hardening and consistency implementation (includes **Phase 3b** below)
- [x] Phase 4 - API alignment and DI cleanup
- [x] Phase 5 - Tests, docs, and cleanup

## Current Snapshot

- `ProductionAccountingService` is the orchestrator with real cross-service workflow (`run ticket -> allocation -> royalty -> revenue -> GL -> close`).
- `ProductionAccountingService.ControllerFacade` exposes many compatibility managers with mixed persistence and in-memory fallbacks.
- `ProductionAccountingModuleSetup` now seeds project-owned production-accounting reference sets through `R_PRODUCTION_ACCOUNTING_REFERENCE_CODE`.
- API usage is currently concentrated in `ApiService/Controllers/Field/AccountingController.cs` plus accounting-domain controllers.
- There is strong functional breadth but uneven operational maturity across validation, cancellation, and persistence consistency.

## Recent completions (reference alignment)

- [x] `PeriodClosingService`: royalty close GL line uses `PeriodCloseJournalLineDescriptionPhrases.CloseRoyaltyExpense` (no stray literal).
- [x] `ProductionTaxRemarkKeys`: `PROPERTY_ID`, `JURISDICTION`; `ProductionTaxService` uses them plus `ProductionTaxNotesPhrases` for standard `NOTES` text.
- [x] `DEFERRED_TAX_BALANCE.NOTES` uses `ProductionTaxNotesPhrases.DeferredTaxFromDepletionTiming`; `UnprovedPropertyService` + `ReserveDisclosureService` wired to `UnprovedPropertyDescriptionPhrases` / `ReserveDisclosureNotesPhrases`; checklist documents code anchors.
- [x] Interpolated `DESCRIPTION` patterns in `InventoryLcmService`, `TakeOrPayService`, `RevenueService`, `InventoryService`, `DrillingScenarioAccountingService` use `ProductionAccountingDescriptionPhrases` format strings + invariant `string.Format`.
- [x] `JointInterestBillingService` JIB fallbacks → `JointInterestBillingDescriptionPhrases`; tax depletion `NOTES` → `TaxDepletionAtRateFormat` + invariant format; drilling unknown scenario + `LeasingService` default currency aligned to constants.
- [x] `TakeOrPayService` uses `RevenueTypeCodes.TakeOrPay` + `TakeOrPayObligationParseKeys`; `RevenueService` / `RoyaltyService` use `CommodityPricingFallbackDefaults` for missing `PRICE_INDEX` values.
- [x] `RoyaltyService` deduction netting uses `RoyaltyDeductionCostTypeCodes` + seed rows; `SuccessfulEffortsService.ValidateAsync` accepts those `COST_TYPE` values.
- [x] **Phase 3b step 1** — `AfeService`: `AfeStatusCodes` + `AFE_STATUS` seed rows (`DRAFT`, `APPROVED`); variance / active filters unchanged (already used seeded codes).
- [x] **Phase 3b step 2** — `AllocationService`: reject unknown methods before engine; pro-rata percentage rule uses case-insensitive method match (aligned with `AllocationMethods` / seed).
- [x] **Phase 3b step 3** — `AmortizationService`: fixed depletion fallback logic; `AmortizationCalculationFallbacks`, `AmortizationMethods.AllSeeded` validation, `AmortizationRecordRemarkKeys` for field remark.
- [x] **Phase 3b step 4** — `AssetSwapService`: thin service; added `AccountingSourceModuleCodes.AssetSwap` + seed row; class doc for future GL provenance.
- [x] **Phase 3b step 5** — `AuthorizationWorkflowService`: `AFE.STATUS` checks/assignments use `AfeStatusCodes.Approved` (aligned with `AFE_STATUS` seed / `AfeService`).
- [x] **Phase 3b step 6** — `BorrowingCostCapitalizationService`: default `COST_TYPE` to `CostTypes.Development` when blank; doc links seeded `COST_CATEGORY` / `COST_TYPE`.
- [x] **Phase 3b step 7** — `ContractPerformanceService`: already on `ContractPerformanceStatusCodes` + defaults; docs reference `CONTRACT_OBLIGATION_STATUS` seed; `ContractPerformanceStatusCodes` summary aligned.
- [x] **Phase 3b step 8** — `DecommissioningService`: `AssetRetirementObligationStatusCodes` + `ASSET_RETIREMENT_OBLIGATION_STATUS` seed; default/preserve `STATUS` on insert/update.
- [x] **Phase 3b step 9** — `EmissionsTradingService`: already used `EmissionsObligationStatusCodes` / `SettlementOutcomeCodes` + defaults; class + module SeedScope docs tied to seed sets.
- [x] **Phase 3b step 10** — `ExplorationEvaluationService`: default `COST_TYPE` to `CostTypes.Exploration` when blank; class doc + module SeedScope list `CostTypes` / `CostCategories`.
- [x] **Phase 3b step 11** — `FinancialInstrumentsService`: `FinancialInstrumentTypeCodes` / `FinancialInstrumentStatusCodes` / `FinancialInstrumentMeasurementCodes` + seeds; defaults + row audit on insert vs update for `FINANCIAL_INSTRUMENT`.
- [x] **Phase 3b step 12** — `FullCostService`: `FullCostCalculationFallbacks`, `FullCostRecordedCostDefaults`, `ImpairmentRecordReasonPhrases.SecCeilingTestImpairment`; class doc ties ASC 932 / UOP / ceiling impairment types.
- [x] **Phase 3b step 13** — `FunctionalCurrencyService`: `ProductionAccountingAuditActors.System` replaces literal <c>ROW_CREATED_BY</c>; class doc ties `AccountingCurrencyCodes` / `FUNCTIONAL_CURRENCY_CODE` seed.
- [x] **Phase 3b step 14** — `ImbalanceService`: `ImbalanceDescriptionPhrases` + invariant `REASON`; `ImbalanceAdjustmentTypeCodes.AllSeeded` + case-insensitive validation/aggregation; class doc references `SettlementOutcomeCodes`.
- [x] **Phase 3b step 15** — `ImpairmentTestingService`: already on `ImpairmentRecordTypeCodes` / `ImpairmentEvaluationReasonCodes`; added `AllSeeded` on both code classes; class doc ties IAS 36 row shape to seed sets.
- [x] **Phase 3b step 16** — `LeaseEconomicInterestService`: `LeaseEconomicInterestValidation` + `LeaseEconomicInterestFractionRules`; class doc for `ApprovalWorkflowStatusCodes` / WI–NRI validation.
- [x] **Phase 3b step 17** — `MeasurementService`: `MeasurementStandardCodes`, `MeasurementVolumeRules`, `MeasurementMethodValidation`; `LegacyMeasurementMethodCodes.AllSeeded`; validate unknown `MEASUREMENT_METHOD`.
- [x] **Phase 3b step 18** — `PricingService`: `CommodityPricingFallbackDefaults` for missing/null `PRICE_INDEX`; `PriceIndexCommodityTypeCodes.AllSeeded` / `IsSeededCommodityType` + trim; class doc.
- [x] **Phase 3b step 19** — `ProductionAccountingService.AfeQueries`: `ACTIVE_IND` via `_defaults` on AFE + line items; optional `status` filter documented for `AfeStatusCodes` / `AFE_STATUS` seed.
- [x] **Phase 3b step 20** — `ProductionAccountingService.Compatibility`: `BoeConversionFactors.StandardMcfPerOilBarrelEquivalent` replaces magic `6m` in BOE helpers; BOE method XML docs.
- [x] **Phase 3b step 21** — `ProductionAccountingService.ControllerFacade`: `ProductionAccountingAuditActors.System` replaces literal system user id; `CreateRunTicket` net oil / BSW scale uses `MeasurementVolumeRules`.
- [x] **Phase 3b step 22** — `ProductionSharingService`: `PsaEntitlementCalculationDefaults` for cost-recovery and profit-split fallbacks; `NormalizePercent` uses `LeaseEconomicInterestFractionRules` / `LeaseEconomicInterestValidation`; module SeedScope lists PSA defaults.
- [x] **Phase 3b step 23** — `ReportingService`: `FinancialReportGlRollupAccountPrefixes`, `FinancialReportTaxVariantPlaceholders`, `ReportingServiceExceptionMessages`, `ReportingRemarkFormats` + invariant operational `REMARK`; module SeedScope lists reporting rollup constants.
- [x] **Phase 3b step 24** — `ReserveAccountingService`: `ReserveAccountingServiceExceptionMessages`, `ReserveCashflowPresentValueDefaults`; discount/tax rate percent scaling via `LeaseEconomicInterestFractionRules` / `LeaseEconomicInterestValidation`; module SeedScope.
- [x] **Phase 3b step 25** — `RoyaltyDisputeService`: `RoyaltyDisputeServiceExceptionMessages`, `RoyaltyDisputeMessageFormats` + invariant not-found text; class doc ties `RoyaltyDisputeStatusCodes`.

## Phase 3b — Reference / seed alignment (**complete** — one-file passes 1–25)

**Goal for each step:** Remove magic strings that belong in `Constants/` (`WorkflowReferenceCodes`, `CostTypes`, `JournalEntryTypeCodes`, `ProductionAccountingDescriptionPhrases`, `RoyaltyDeductionCostTypeCodes`, etc.), keep in sync with `ProductionAccountingReferenceCodeSeed` / `ProductionAccountingModuleSetup`, use `IPPDM39DefaultsRepository` for `ACTIVE_IND` and boolean-style columns where touched.

**Standard checklist (repeat per file):**

1. Grep the file for quoted tokens (`STATUS`, `TYPE`, `METHOD`, `DESCRIPTION =`, `FilterValue = "Y"` / `"N"`, currency codes, etc.).
2. Map each to an existing constant or add a constant + **one** `yield return` in `ProductionAccountingReferenceCodeSeed.GetConstantsBackedRows()` (or the appropriate seed section) when it is a new LOV/reference family.
3. Prefer `_defaults.GetActiveIndicatorYes()` / `GetActiveIndicatorNo()` over `"Y"` / `"N"` on filters and entity flags.
4. Run: `dotnet build Beep.OilandGas.ProductionAccounting\Beep.OilandGas.ProductionAccounting.csproj` (no new warnings).

**Execution order (do not batch multiple services in a single commit unless trivial):**

| Step | Target | Notes |
| ---: | --- | --- |
| 1 | `Services/AfeService.cs` | AFE status / phase literals → constants + seed if missing. |
| 2 | `Services/AllocationService.cs` | Allocation method/status; align with `AllocationMethods` / `AllocationStatus` / seed. |
| 3 | `Services/AmortizationService.cs` | `AmortizationMethods`, cost/amortization type strings. |
| 4 | `Services/AssetSwapService.cs` | Transaction / recognition status codes. |
| 5 | `Services/AuthorizationWorkflowService.cs` | `ApprovalWorkflowStatusCodes` (and related). |
| 6 | `Services/BorrowingCostCapitalizationService.cs` | `CostTypes` / `CostCategories`; capitalize flags via defaults. |
| 7 | `Services/ContractPerformanceService.cs` | `ContractPerformanceStatusCodes`; obligation types. |
| 8 | `Services/DecommissioningService.cs` | ARO / liability status or type literals. |
| 9 | `Services/EmissionsTradingService.cs` | `EmissionsObligationStatusCodes`, `SettlementOutcomeCodes`. |
| 10 | `Services/ExplorationEvaluationService.cs` | Exploration and evaluation cost categories / types — finish sweep. |
| 11 | `Services/FinancialInstrumentsService.cs` | Instrument type / measurement / IFRS 9 tokens. |
| 12 | `Services/FullCostService.cs` | Ceiling / amortization / cost pool codes. |
| 13 | `Services/FunctionalCurrencyService.cs` | `AccountingCurrencyCodes`; `ROW_CREATED_BY` system actor if hard-coded. |
| 14 | `Services/ImbalanceService.cs` | `ImbalanceAdjustmentTypeCodes`, `SettlementOutcomeCodes`. |
| 15 | `Services/ImpairmentTestingService.cs` | `ImpairmentRecordTypeCodes`, `ImpairmentEvaluationReasonCodes`. |
| 16 | `Services/LeaseEconomicInterestService.cs` | Interest type / WI-NRI codes; lease economic enums. |
| 17 | `Services/MeasurementService.cs` | `LegacyMeasurementMethodCodes` or enum-backed measurement sets. |
| 18 | `Services/PricingService.cs` | `PriceIndexCommodityTypeCodes`, `CommodityPricingFallbackDefaults` where relevant. |
| 19 | `Services/ProductionAccountingService.AfeQueries.cs` | Same rules as `AfeService`; keep queries aligned with constants used by AFE APIs. |
| 20 | `Services/ProductionAccountingService.Compatibility.cs` | Compatibility shims: replace embedded status strings with constants; avoid widening behavior. |
| 21 | `Services/ProductionAccountingService.ControllerFacade.cs` | Large surface — last before sharing/reporting; request DTO strings, report types (`GeneratedReportTypeCodes`), currencies. |
| 22 | `Services/ProductionSharingService.cs` | PSA cost / revenue split type codes. |
| 23 | `Services/ReportingService.cs` | Report service type, schedule frequency/status. |
| 24 | `Services/ReserveAccountingService.cs` | Reserve / cashflow / booking status literals. |
| 25 | `Services/RoyaltyDisputeService.cs` | `RoyaltyDisputeStatusCodes`; resolution narrative → phrases if persisted. |

**Progress (check when step complete):** [x] 1 [x] 2 [x] 3 [x] 4 [x] 5 [x] 6 [x] 7 [x] 8 [x] 9 [x] 10 [x] 11 [x] 12 [x] 13 [x] 14 [x] 15 [x] 16 [x] 17 [x] 18 [x] 19 [x] 20 [x] 21 [x] 22 [x] 23 [x] 24 [x] 25 [x]

## Phase 4 progress (API / DI)

- [x] **`Beep.OilandGas.ApiService/Program.cs`** — Register `Beep.OilandGas.Models.Core.Interfaces.IReportingService` → `ReportingService` (after `IJointInterestBillingService` registration so JIB dependency resolves); register `IRoyaltyDisputeService` → `RoyaltyDisputeService`. Factory pattern matches existing ProductionAccounting services (`IDMEEditor`, `ICommonColumnHandler`, `IPPDM39DefaultsRepository`, `IPPDMMetadataRepository`, loggers).
- [x] **`Beep.OilandGas.ApiService/Program.cs`** — Register additional ProductionAccounting interfaces in `Models.Core.Interfaces`: `IAfeService`, `IProductionTaxService`, `ITakeOrPayService`, `IContractPerformanceService`, `ICopasOverheadService`, `IInternalControlService`, `IUnprovedPropertyService`, `IDrillingScenarioAccountingService` using the same DI factory pattern and existing optional dependency wiring.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Reporting/ReportingServiceController.cs`** — Add service-backed async reporting endpoints (`operational`, `financial`, `royalty-statement`, `jib-statement`, schedule, distribution, history) that call `IReportingService` directly with claim-based user id fallback.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Royalty/RoyaltyDisputeController.cs`** + **`ResolveRoyaltyDisputeRequest.cs`** — Add service-backed dispute endpoints (`create`, `resolve`, `list`) aligned with `IRoyaltyDisputeService`; keep one class per file.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Reporting/ReportingController.cs`** — Modernize legacy route (`api/accounting/reporting`) to delegate operational/financial/royalty/JIB/schedule/distribution/history actions to `IReportingService` while retaining existing lease-report manager path for compatibility.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Tax/ProductionTaxController.cs`** — Add service-backed production tax endpoint (`POST api/accounting/tax/production/calculate`) calling `IProductionTaxService.CalculateProductionTaxesAsync` with claim-based user fallback and explicit `connectionName` pass-through.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Cost/AFEController.cs`** — Refactor legacy AFE controller to inject/use `IAfeService` for create/approve and add service-backed endpoints for line-items, cost recording, and variance reports; keep existing list/get + lease-compat style shape where needed.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Royalty/RoyaltyController.cs`** + **`RecordRoyaltyPaymentFromCalculationRequest.cs`** — Keep legacy royalty endpoints intact, add explicit service-backed endpoints (`service/calculate`, `service/calculations/{id}`, `service/calculations/by-allocation/{allocationId}`, `service/payments`) powered by `IRoyaltyService` for staged client migration.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Cost/CostAllocationController.cs`** + **`AllocateProductionRequest.cs`** — Keep existing cost allocation endpoint intact, add additive service-backed allocation endpoints (`service/allocate`, `service/{id}`, `service/{id}/details`, `service/{id}/reverse`) powered by `IAllocationService` with claim-based user fallback.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Production/RunTicketController.cs`** — Keep legacy run-ticket endpoints intact, add additive service-backed orchestrator endpoints (`service/process-cycle`, `service/accounting-status/{fieldId}`, `service/revenue-transactions/{fieldId}`) powered by `IProductionAccountingService`.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Revenue/RevenueTransactionController.cs`** — Keep legacy revenue transaction create path intact, add additive service-backed revenue endpoints (`service/recognize`, `service/validate`) powered by `IRevenueService`; fix missing `Models.Core.Interfaces` namespace import in controller.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Traditional/InventoryController.cs`** + request payload files (`UpdateTankInventoryRequest`, `CalculateInventoryValuationRequest`, `GenerateInventoryReconciliationReportRequest`) — Keep legacy traditional inventory endpoints intact; add additive service-backed inventory endpoints (`service/{tankId}/update`, `service/{tankId}`, `service/validate`, `service/{inventoryItemId}/valuation`, `service/{inventoryItemId}/reconciliation-report`) powered by `IInventoryService`; resolve type namespace imports for `TANK_INVENTORY` / valuation/report models.
- [x] **`Beep.OilandGas.ApiService/Controllers/Accounting/Storage/StorageController.cs`** + request payload files (`StorageTankInventoryUpdateRequest`, `StorageInventoryValuationRequest`, `StorageReconciliationReportRequest`) — Keep legacy storage-manager endpoints intact; add additive service-backed storage inventory endpoints (`service/tanks/{tankId}/update`, `service/tanks/{tankId}`, `service/inventory/{inventoryItemId}/valuation`, `service/inventory/{inventoryItemId}/reconciliation-report`) powered by `IInventoryService` with claim-based user fallback.

**Verification:** `dotnet build Beep.OilandGas.ApiService\Beep.OilandGas.ApiService.csproj` (latest run: 0 errors, 0 warnings).

## Active TODOs

- [x] Reduce sync-over-async repository calls in compatibility manager paths where persistent behavior is still synchronous.
- [ ] Ensure cancellation and logging consistency for long-running service paths.
- [ ] Execute a runtime module setup re-run validation (same DB/session) and capture evidence of no duplicate/unsafe side effects.

## Verification Criteria

- Build compiles with no new `ProductionAccounting` warnings/errors.
- Module setup remains idempotent and safe to re-run.
- Each active API area has at least one happy path and one guard/failure test.
- Compatibility layers are clearly marked as active, staged, or fallback-only.

*Last updated: 2026-04-28 — Completed sync-over-async convergence for compatibility callers (all compatibility paths now funnel through the shared sync bridge, with only the two intentional bridge internals containing blocking waits) and advanced long-running cancellation hardening by adding explicit `OperationCanceledException` logging/rethrow behavior in `ProductionAccountingService` orchestration/aggregate helpers, `PeriodClosingService` orchestration/reconciliation/update paths, `JointInterestBillingService` allocation/statement/validation paths, `InventoryService` update/retrieval/validation paths, `RevenueService` recognition/validation/price-lookup paths, `ImbalanceService` reconciliation/outstanding-balance/validation paths, and `MeasurementService` validation; `ProductionAccounting` build and focused ProductionAccounting tests remain green.*

