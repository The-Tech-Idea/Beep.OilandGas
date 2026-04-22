# Beep.OilandGas.LeaseAcquisition

## Snapshot

- Category: Operations
- Scan depth: Heavy
- Current role: lease acquisition, rights, due diligence, and lease lifecycle domain slice
- Maturity signal: materially present and decomposed service layer

## Observed Structure

- Top-level folders: `Core`, `Data`, `Services`
- Data is split into `Lease` and `LeaseManagement`
- Service layer is decomposed into multiple partials covering rights, negotiation, due diligence, lifecycle, financial, and stakeholders

## Representative Evidence

- Core service: `Services/LeaseAcquisitionService.cs`
- Partial service areas: `Services/LeaseAcquisitionService.DueDiligence.cs`, `LeaseAcquisitionService.Rights.cs`, `LeaseAcquisitionService.Negotiation.cs`, `LeaseAcquisitionService.Financial.cs`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Operations/LeaseAcquisitionController.cs`
- Web surfacing: `Beep.OilandGas.Web/Pages/PPDM39/Operations/LeaseAcquisition.razor`

## Planning Notes

- This is one of the stronger phase-8 operational slices.
- The main planning need is cleaner linkage to development, compliance, and economics rather than basic service creation.
