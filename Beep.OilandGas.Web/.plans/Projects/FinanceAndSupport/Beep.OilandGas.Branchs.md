# Beep.OilandGas.Branchs

## Snapshot

- Category: Finance and support
- Scan depth: Medium
- Current role: business-process and category/tree mapping support
- Maturity signal: support library with taxonomy/navigation concerns

## Observed Structure

- Top-level folders: `BusinessProcess`, `Data`, `GFX`, `PPDM39`
- Root files include `PPDM39Categories.cs` and `PPDM39TableMapping.cs`
- Business-process folder contains category and node types

## Representative Evidence

- Business process taxonomy: `BusinessProcess/BusinessProcessCategories.cs`, `BusinessProcessNode.cs`, `BusinessProcessRootNode.cs`
- PPDM mapping: `PPDM39Categories.cs`, `PPDM39TableMapping.cs`

## Planning Notes

- This project matters to navigation, taxonomy, and grouping decisions in the web modernization plan.
- Phase 6 and 9 should use it as a supporting taxonomy source rather than introduce parallel category systems in the UI plan.
