# Beep.OilandGas.DevelopmentPlanning

## Snapshot

- Category: Operations
- Scan depth: Heavy
- Current role: development-planning and FDP domain slice
- Maturity signal: partial slice with service, DTO, API, and web presence

## Observed Structure

- Top-level folders: `Core`, `Data`, `Services`
- Services include `DevelopmentPlanService` and `DevelopmentPlanService.Advanced`
- Data folder contains development-plan and well-plan DTOs such as `DevelopmentPlan.cs`, `FacilityPlan.cs`, and `WellPlan.cs`

## Representative Evidence

- Services: `Services/IDevelopmentPlanService.cs`, `Services/DevelopmentPlanService.cs`, `Services/DevelopmentPlanService.Advanced.cs`
- Data contracts: `Data/DevelopmentPlan.cs`, `Data/FacilityPlan.cs`, `Data/WellPlan.cs`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Field/DevelopmentController.cs`
- Web surfacing: `Beep.OilandGas.Web/Pages/PPDM39/Development/DevDashboard.razor`, `FDPWizard.razor`, `WellDesign.razor`

## Planning Notes

- The slice is real, but still needs orchestration growth around planning sequences, facilities, and drilling handoffs.
- Phase 8 should treat this project as partial, not complete.
