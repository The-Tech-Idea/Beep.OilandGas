# Beep.OilandGas.ProspectIdentification

## Snapshot

- Category: Operations
- Scan depth: Heavy
- Current role: exploration and prospect-management domain slice
- Maturity signal: materially present service layer with matching API and web surfaces

## Observed Structure

- Top-level folders: `Core`, `Data`, `Services`
- Services include `ProspectIdentificationService`, `ProspectEvaluationService`, and `SeismicAnalysisService`
- The project includes its own domain data area under `Data`

## Representative Evidence

- Service contracts and implementations: `Services/IProspectIdentificationService.cs`, `Services/ProspectIdentificationService.cs`, `Services/ProspectEvaluationService.cs`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Operations/ProspectIdentificationController.cs`
- Web surfacing: `Beep.OilandGas.Web/Pages/PPDM39/Exploration/Prospects.razor`, `ProspectDetail.razor`, `ProspectBoard.razor`

## Planning Notes

- This is one of the stronger operational slices and should anchor early phase 8 integration work.
- `Controllers/Operations/ProspectIdentificationController.cs` now also carries legacy `Beep.OilandGas.Client` compatibility routes under `/api/prospect/*`, mapping `PROSPECT*` payloads onto `IProspectIdentificationService` instead of growing a second prospect API surface.
- The legacy `PROSPECT_PORTFOLIO` contract still does not encode prospect membership, so the `/api/prospect/rank` shim intentionally ranks the available prospect set with default reserves/risk weights and returns the top candidate rather than inventing a parallel portfolio store.
- Future planning should focus more on handoffs into development and economics than on basic slice creation.
