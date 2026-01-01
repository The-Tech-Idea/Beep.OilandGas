# Plan: Migrate Data types to inherit from `Entity`

Goal
- Ensure all model classes under `Beep.OilandGas.Models\Data` that are intended to be PPDM/Domain entities inherit from `Entity` (the base class used across generated PPDM model types).

Summary of findings (scanned `Beep.OilandGas.Models\Data`)
- Files containing public classes that do NOT currently inherit from `Entity` (top-level types):
  - `Beep.OilandGas.Models\Data\Trading\ExchangeStatement.cs`
    - `ExchangeStatement` (class)
    - `ExchangeSummary` (class)
    - `ExchangeNetPosition` (class)
    - `ExchangeTransaction` (class)
    - `ExchangeStatementGenerator` (static class) — NOTE: static helper, cannot inherit `Entity`.
  - `Beep.OilandGas.Models\Data\Accounting\SalesStatement.cs`
    - `SalesStatement` (class)
    - `SalesSummary` (class)
    - `VolumeDetail` (class)
    - `PricingDetail` (class)
  - `Beep.OilandGas.Models\Data\Accounting\SalesTransaction.cs`
    - `SalesTransaction` (class)
    - `DeliveryInformation` (class)
    - `ProductionMarketingCosts` (class)
    - `ProductionTax` (class)
    - `ProductionTaxType` (enum) — NOTE: enums cannot inherit `Entity`.

Notes about scope
- The scan focused on concrete C# files under `Beep.OilandGas.Models\Data` and its subfolders. Many other files in that folder already appear to be generated `partial class ... : Entity` and were not included here.
- Helper/DTO classes and enums (static classes, enums) are intentionally excluded from inheritance recommendations where that would be invalid.

Recommended migration approach (high level)
1. Decide which of the above classes should be domain/DB entities vs. plain DTO/helper types.
   - If a class is only used as an in-memory DTO or a reporting/model type (not persisted as a PPDM table), do NOT make it inherit `Entity`.
   - If a class represents a table/record (should participate in common column handling / data source operations), migrate it to inherit `Entity` and implement `IPPDMEntity` where appropriate.

2. Apply changes per file (example safe steps):
   - Add `using TheTechIdea.Beep.Editor;` to the file header (if not present).
   - Change class declaration from `public class SalesStatement` to `public partial class SalesStatement : Entity`.
   - Convert auto-properties to pattern used across other `Entity` classes if you want change notifications (`SetProperty` with backing fields) — optional but recommended for consistency.
   - If the class must implement PPDM optional properties (e.g., `ROW_QUALITY`, `PPDM_GUID`) add them to satisfy `IPPDMEntity` if you choose to implement that interface.
   - Run targeted build and unit tests after each file change.

3. Prefer minimal, incremental commits:
   - 1 commit per logical group (e.g., Trading models, Accounting models).
   - Keep diffs small to ease code review and testing.

4. Testing and verification
   - After each commit run `dotnet build Beep.OilandGas.Models` and any dependent projects.
   - Execute existing unit tests (if any) and run any smoke tests for services that use these types (e.g., AccountingService flows).
   - Manually verify that instances used by services still function: data-source Insert/Update paths should still accept the modified entities.

5. Rollout considerations
   - If these types are serialized (JSON) used by external clients, maintain property names and shapes; changing to `Entity` should not alter public properties, but if you convert to backing fields + `SetProperty` ensure JSON serializers still bind.
   - If you implement `IPPDMEntity` members (some are non-nullable), consider defaulting them to sensible values or making them nullable to avoid warning noise.

Files recommended for migration (proposed commits)
- commit-1: `Data/Trading/ExchangeStatement.cs`
  - Migrate `ExchangeStatement`, `ExchangeSummary`, `ExchangeNetPosition`, `ExchangeTransaction` (if these represent persisted entities). Leave `ExchangeStatementGenerator` unchanged.
- commit-2: `Data/Accounting/SalesStatement.cs`
  - Migrate `SalesStatement`. Consider keeping `SalesSummary`, `VolumeDetail`, `PricingDetail` as DTOs unless they map to persisted tables.
- commit-3: `Data/Accounting/SalesTransaction.cs`
  - Migrate `SalesTransaction` (if it represents a persisted object). Support classes (`DeliveryInformation`, `ProductionMarketingCosts`, `ProductionTax`) probably remain DTOs.

Suggested PR checklist
- Build passes for `Beep.OilandGas.Models` and dependent projects.
- No public API breaking changes (review serialized contract and property names).
- New/updated unit tests covering creation/persistence flows (if applicable).
- Reviewer confirms classes chosen for migration are intended to be persistent domain entities.

Risks and caveats
- Converting DTO-only types to inherit `Entity` can introduce unnecessary coupling and nullable/initialization warnings. Prefer to migrate only types used with data source operations.
- Changing classes to use `SetProperty` and backing fields will produce large diffs. Consider leaving public auto-properties intact and only add inheritance if `Entity` requires it.

Next actions (pick one)
- A. I will create the first patch changing `ExchangeStatement` and nested types to inherit `Entity` (incremental). I will run a targeted build and report results.
- B. I will produce concrete code changes for all listed files in a single PR (larger change).
- C. Stop here and let you review the plan; you will indicate which files to migrate.

---
Generated: planentitymigrate.md
