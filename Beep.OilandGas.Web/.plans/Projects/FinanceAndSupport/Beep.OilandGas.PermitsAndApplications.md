# Beep.OilandGas.PermitsAndApplications

## Snapshot

- Category: Finance and support
- Scan depth: Heavy
- Current role: permit workflow, attachment, compliance-check, and validation support layer
- Maturity signal: service-rich and validation-rich, but underexposed compared with the size of the project

## Observed Structure

- Top-level folders: `Configs`, `Constants`, `Core`, `Data`, `DataMapping`, `Forms`, `Services`, `Validation`
- Validation includes a rule engine, field-value resolution, request/result models, and a `Rules` subfolder
- Services include lifecycle, workflow, attachment, compliance check/reporting, and status transition logic

## Representative Evidence

- Services: `Services/PermitApplicationLifecycleService.cs`, `PermitApplicationWorkflowService.cs`, `PermitComplianceCheckService.cs`, `PermitComplianceReportService.cs`
- Validation: `Validation/PermitValidationEngine.cs`, `PermitValidationRulesFactory.cs`, `Rules/`
- Current surfacing signal: `Beep.OilandGas.ApiService/Controllers/Compliance/ComplianceController.cs`, `Beep.OilandGas.Web/Pages/PPDM39/Compliance/ComplianceDashboard.razor`

## Planning Notes

- Phase 9 should treat this project as an under-surfaced capability area.
- The web/API plans need dedicated permit surfaces instead of relying only on broad compliance pages.
