# Beep.OilandGas.ProductionAccounting

## Snapshot

- Category: Finance and support
- Scan depth: Heavy
- Current role: oil-and-gas-specific accounting layer for allocations, royalties, JIB, reserves, revenue, and related workflows
- Maturity signal: broad specialized service layer with visible UI/API surfaces

## Observed Structure

- Top-level folders: `Data`, `Services`
- Services include allocation, authorization workflow, JIB, reserves, revenue, royalty, full cost, successful efforts, production tax, imbalance, and decommissioning-related accounting
- The project is narrower than general accounting but deeper in petroleum-specific finance logic

## Representative Evidence

- Services: `Services/ProductionAccountingService.cs`, `AllocationService.cs`, `JointInterestBillingService.cs`, `ReserveAccountingService.cs`, `RoyaltyService.cs`, `RevenueService.cs`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Accounting/Production/`, `Controllers/Accounting/Royalty/`, `Controllers/Accounting/Revenue/`
- Web surfacing: `Beep.OilandGas.Web/Pages/PPDM39/Accounting/ProductionAccounting.razor`, `Royalties.razor`, `VolumeReconciliation.razor`

## Planning Notes

- This project is a major phase-9 dependency and should be treated as a first-class vertical, not as a sub-feature of generic accounting.
- The project already suggests enough business breadth that the main risk is workflow fragmentation across UI/controller families.
