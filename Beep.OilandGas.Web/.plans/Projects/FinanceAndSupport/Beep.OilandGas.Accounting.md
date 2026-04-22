# Beep.OilandGas.Accounting

## Snapshot

- Category: Finance and support
- Scan depth: Heavy
- Current role: general accounting, finance, and reporting service layer
- Maturity signal: very broad service surface with clear API and web exposure

## Observed Structure

- Top-level folders: `Constants`, `Core`, `Data`, `Models`, `Services`, `plans`
- Service layer is broad and includes GL, AP, AR, budgeting, inventory, tax, reporting, reconciliation, presentation, and IFRS/GAAP-oriented service areas
- The project is broader than oil-and-gas-specific accounting; it looks like a general finance platform adapted to the solution

## Representative Evidence

- Services: `Services/GLAccountService.cs`, `JournalEntryService.cs`, `APInvoiceService.cs`, `ARService.cs`, `FinancialStatementService.cs`, `DashboardService.cs`, `CostAllocationService.cs`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Accounting/`
- Web surfacing: `Beep.OilandGas.Web/Pages/PPDM39/Accounting/AccountingDashboard.razor`, `CostAllocation.razor`

## Planning Notes

- This project is mature enough to support phase 9 finance integration, but the web/API plans should avoid assuming every service area has coherent end-to-end workflows.
- Production-accounting-specific work should remain separated from this project even when the UI converges.
