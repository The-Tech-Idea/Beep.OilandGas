# ProductionAccounting Module Enhancement Plans Index

## Overview

This document indexes all module enhancement plans. Each module has a detailed PLAN.md file in its directory.

## Plan Status

### Plans Created (All 28 Modules)

1. **Allocation** - `Allocation/PLAN.md`
2. **Production** - `Production/PLAN.md`
3. **Financial/SuccessfulEfforts** - `Financial/SuccessfulEfforts/PLAN.md`
4. **Royalty** - `Royalty/PLAN.md`
5. **GeneralLedger** - `GeneralLedger/PLAN.md`
6. **Pricing** - `Pricing/PLAN.md`
7. **Financial/FullCost** - `Financial/FullCost/PLAN.md`
8. **Financial/Amortization** - `Financial/Amortization/PLAN.md`
9. **Invoice** - `Invoice/PLAN.md`
10. **PurchaseOrder** - `PurchaseOrder/PLAN.md`
11. **AccountsPayable** - `AccountsPayable/PLAN.md`
12. **AccountsReceivable** - `AccountsReceivable/PLAN.md`
13. **Inventory** - `Inventory/PLAN.md`
14. **Trading** - `Trading/PLAN.md`
15. **Storage** - `Storage/PLAN.md`
16. **Ownership** - `Ownership/PLAN.md`
17. **Unitization** - `Unitization/PLAN.md`
18. **Imbalance** - `Imbalance/PLAN.md`
19. **Measurement** - `Measurement/PLAN.md`
20. **Management** - `Management/PLAN.md`
21. **Reporting** - `Reporting/PLAN.md`
22. **Analytics** - `Analytics/PLAN.md`
23. **Validation** - `Validation/PLAN.md`
24. **Calculations** - `Calculations/PLAN.md`
25. **Export** - `Export/PLAN.md`
26. **Rendering** - `Rendering/PLAN.md`
27. **Services** - `Services/PLAN.md`
28. **Accounting** - `Accounting/PLAN.md`

## Common Enhancement Patterns

All plans follow this structure:

1. **Current State Analysis** - What exists, what's missing
2. **Entity/DTO Migration** - Classes to move to Beep.OilandGas.Models
3. **Service Class Creation** - Create service with IDataSource and connectionName
4. **Database Integration** - Use PPDMGenericRepository
5. **Missing Workflows** - Industry-standard workflows to add
6. **Database Scripts** - Scripts needed for new tables
7. **Implementation Steps** - Step-by-step guide

## Key Requirements for All Modules

1. **Service Class Pattern**: Each module must have a service class with:
   - Constructor: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
   - Interface in Beep.OilandGas.PPDM39/Core/Interfaces/
   - Implementation using PPDMGenericRepository

2. **Entity Location**: All entity classes in `Beep.OilandGas.Models/Data/{Module}/`

3. **DTO Location**: All DTOs in `Beep.OilandGas.Models/DTOs/{Module}/`

4. **No Dictionary Conversions**: Use entities directly, remove all Convert*ToDictionary methods

5. **Database Scripts**: Create TAB/PK/FK scripts for all new tables (6 database types)

## Implementation Priority

### Phase 1 (Critical - Financial & Core Operations)
- Financial/SuccessfulEfforts
- Financial/FullCost
- Financial/Amortization
- GeneralLedger
- Production
- Allocation

### Phase 2 (Traditional Accounting)
- Invoice
- PurchaseOrder
- AccountsPayable
- AccountsReceivable
- Inventory

### Phase 3 (Operational Accounting)
- Royalty
- Pricing
- Trading
- Storage
- Ownership
- Unitization
- Imbalance

### Phase 4 (Supporting Modules)
- Measurement
- Management
- Reporting
- Analytics
- Validation
- Calculations
- Export
- Rendering
- Services
- Accounting

