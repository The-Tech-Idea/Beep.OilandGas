# Beep.OilandGas.EnhancedRecovery

## Snapshot

- Category: Operations
- Scan depth: Medium
- Current role: enhanced recovery and EOR domain slice
- Maturity signal: thin and currently closer to a scaffolded slice than a mature workflow area

## Observed Structure

- Top-level folders: `Services`
- Service layer includes `EnhancedRecoveryService`, `EnhancedRecoveryService.Advanced`, and interface/model-core partials
- The project root does not show the richer data/DTO shape present in stronger operational slices

## Representative Evidence

- Services: `Services/EnhancedRecoveryService.cs`, `Services/EnhancedRecoveryService.Advanced.cs`, `Services/EnhancedRecoveryService.ModelsCoreImpl.cs`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Operations/EnhancedRecoveryController.cs`
- Web surfacing: `Beep.OilandGas.Web/Pages/PPDM39/Operations/EnhancedRecovery.razor`

## Planning Notes

- Phase 8 should treat this as a build-out slice, not as a completed integration surface.
- Explicit contracts and UI now exist for the pilot-economics handoff into `EconomicEvaluation.razor`; remaining planning work is approvals and any deeper reservoir workflow linkage.
