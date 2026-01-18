# Beep.OilandGas Production Accounting - Architecture Rewrite Plan

## Executive Summary

**Goal**: Rewrite the entire `Beep.OilandGas.ProductionAccounting` project to follow PPDM39 data model standards and oil & gas accounting best practices.

**Key Principle**: Use **Data Classes Only** - No DTOs. All classes inherit from `Entity` and implement `IPPDMEntity`, following the pattern in `Beep.OilandGas.Models.Data`.

**Timeline**: Phase-based implementation

---

## Architecture Principles

### 1. Data Model Authority
- **Single Source of Truth**: `Beep.OilandGas.Models.Data.ProductionAccounting`
- All entity definitions in this folder
- All services operate on these entities
- No DTO translation layers

### 2. Service Architecture
- **Repository Pattern**: Use `PPDMGenericRepository` for all CRUD
- **Service Layer**: Business logic classes in `ProductionAccounting/Services/`
- **No DTOs**: Services accept and return Data entities directly
- **Async-First**: All database operations are async

### 3. Oil & Gas Accounting Standards
- **PPDM39 Compliance**: Follow Petroleum Data Management guidelines
- **FASB ASC 932**: Extractive Activities - Oil & Gas (accounting rules)
- **Revenue Recognition**: ASC 606 compliant
- **Full Cost Method**: Track all costs to successful wells
- **Successful Efforts Method**: Write off unsuccessful wells immediately

### 4. Entity Relationships (PPDM39)

```
WELL -> RUN_TICKET -> ALLOCATION -> ROYALTY_PAYMENT
  |       |              |
  |       +-> MEASUREMENT_RECORD
  |       |
  |       +-> JOURNAL_ENTRY
  |
  +-> DIVISION_ORDER (Lease Agreement)
  |
  +-> WELL_ALLOCATION_DATA

RUN_TICKET -> INVENTORY (Tank, Storage)
  |
  +-> PRICE_INDEX -> REVENUE_TRANSACTION
  |
  +-> SALES_CONTRACT -> INVOICE

ALLOCATION_RESULT -> ALLOCATION_DETAIL -> ROYALTY_CALCULATION
```

---

## Project Structure

```
Beep.OilandGas.ProductionAccounting/
├── Services/
│   ├── ProductionAccountingService.cs (Main orchestrator)
│   ├── AllocationService.cs (Allocation engine)
│   ├── RoyaltyService.cs (Royalty calculations)
│   ├── RevenueService.cs (Revenue recognition)
│   ├── MeasurementService.cs (Measurement management)
│   └── PricingService.cs (Price indexing)
├── Accounting/
│   ├── FullCostService.cs (Full cost method)
│   ├── SuccessfulEffortsService.cs (Successful efforts method)
│   ├── AmortizationService.cs (Depletion/depreciation)
│   ├── JournalEntryService.cs (General ledger posting)
│   └── PeriodClosingService.cs (Month-end close)
├── Allocation/
│   ├── AllocationEngine.cs (Core allocation logic)
│   ├── AllocationMethod.cs (Pro-rata, Equation, etc.)
│   └── AllocationValidator.cs
├── Inventory/
│   ├── InventoryService.cs (Tank inventory tracking)
│   ├── InventoryAdjustment.cs (Inventory corrections)
│   └── InventoryValuation.cs (FIFO/LIFO/Weighted avg)
├── Measurement/
│   ├── MeasurementValidator.cs (Accuracy standards)
│   ├── MeasurementCorrections.cs (Adjustment logic)
│   └── LAC TService.cs (Lease Automatic Custody Transfer)
├── Royalty/
│   ├── RoyaltyCalculator.cs (Interest & payment calcs)
│   ├── RoyaltyReconciliation.cs (Imbalance handling)
│   └── RoyaltyReporting.cs (Partner statements)
├── Reporting/
│   ├── ProductionReport.cs
│   ├── FinancialReport.cs
│   ├── ComplianceReport.cs (Governmental/SEC)
│   └── PartnerStatement.cs
├── Validation/
│   ├── DataQualityValidator.cs
│   ├── BusinessRuleValidator.cs
│   └── ComplianceValidator.cs
├── Constants/
│   ├── AccountingMethods.cs
│   ├── AllocationMethods.cs
│   └── RoyaltyTypes.cs
└── Exceptions/
    ├── ProductionAccountingException.cs
    ├── AllocationException.cs
    └── RoyaltyException.cs
```

---

## Data Model Requirements

### Required Entity Classes (in Models/Data/ProductionAccounting)

All inherit from `Entity` and implement `IPPDMEntity`:

#### Production Tracking
- ✅ RUN_TICKET
- ✅ MEASUREMENT_RECORD
- ✅ TANK_INVENTORY
- ✅ PRODUCTION_ALLOCATION
- ✅ WELL_ALLOCATION_DATA
- ✅ LEASE_ALLOCATION_DATA
- ✅ TRACT_ALLOCATION_DATA

#### Allocation & Royalty
- ✅ ALLOCATION_RESULT
- ✅ ALLOCATION_DETAIL
- ✅ ROYALTY_CALCULATION
- ✅ ROYALTY_PAYMENT
- ✅ ROYALTY_DEDUCTIONS
- ✅ ROYALTY_INTEREST
- ✅ JIB_PARTICIPANT (Joint Interest Billing)
- ✅ JIB_CHARGE
- ✅ JIB_CREDIT
- ✅ JOINT_INTEREST_STATEMENT

#### Financial
- ✅ SALES_TRANSACTION
- ✅ INVOICE
- ✅ JOURNAL_ENTRY
- ✅ GL_ENTRY
- ✅ GL_ACCOUNT
- ✅ ACCOUNTING_COST
- ✅ ACCOUNTING_METHOD (Full Cost vs Successful Efforts)
- ✅ AMORTIZATION_RECORD

#### Pricing & Exchange
- ✅ PRICE_INDEX
- ✅ REGULATED_PRICE
- ✅ EXCHANGE_CONTRACT
- ✅ EXCHANGE_TERMS
- ✅ SALES_CONTRACT

#### Agreements & Leases
- ✅ DIVISION_ORDER (Lease Agreement)
- ✅ JOINT_OPERATING_AGREEMENT
- ✅ UNIT_AGREEMENT
- ✅ OPERATIONAL_REPORT

#### Inventory & Storage
- ✅ TANK_INVENTORY
- ✅ INVENTORY_TRANSACTION
- ✅ INVENTORY_ADJUSTMENT
- ✅ INVENTORY_VALUATION
- ✅ CRUDE_OIL_INVENTORY

#### Measurement Standards
- ✅ MEASUREMENT_ACCURACY
- ✅ MEASUREMENT_CORRECTIONS
- ✅ MEASUREMENT_REPORT_SUMMARY

#### Imbalance Management
- ✅ IMBALANCE_ADJUSTMENT
- ✅ IMBALANCE_RECONCILIATION
- ✅ OIL_IMBALANCE
- ✅ IMBALANCE_STATEMENT

#### Reporting
- ✅ OPERATIONAL_REPORT
- ✅ GOVERNMENTAL_REPORT
- ✅ LEASE_REPORT
- ✅ PRODUCTION_REPORT_SUMMARY
- ✅ COST_REPORT_SUMMARY
- ✅ INVENTORY_REPORT_SUMMARY

---

## Service Interface Standards

All services must follow this pattern:

```csharp
namespace Beep.OilandGas.ProductionAccounting.{SubDomain}
{
    public interface I{Domain}Service
    {
        // Create
        Task<TEntity> CreateAsync(TEntity entity, string userId, string? connectionName = null);
        
        // Read
        Task<TEntity?> GetByIdAsync(string id, string? connectionName = null);
        Task<List<TEntity>> GetAllAsync(string? connectionName = null);
        Task<List<TEntity>> GetByFilterAsync(List<AppFilter> filters, string? connectionName = null);
        
        // Update
        Task<TEntity> UpdateAsync(TEntity entity, string userId, string? connectionName = null);
        
        // Delete
        Task<bool> DeleteAsync(string id, string userId, string? connectionName = null);
        
        // Business Logic
        Task<BusinessResult> PerformBusinessOperationAsync(InputData data, string userId, string? connectionName = null);
    }
}
```

---

## Implementation Phases

### Phase 1: Data Model Completion (Week 1)
- [ ] Verify all required entities exist in Models/Data/ProductionAccounting
- [ ] Create missing entities following SALES_TRANSACTION pattern
- [ ] Add XML documentation to all entity properties
- [ ] Create entity relationship diagrams

### Phase 2: Service Layer Foundation (Week 2)
- [ ] Create base service interfaces (IProductionAccountingService, etc.)
- [ ] Implement repository pattern for each entity
- [ ] Create validation services for business rules
- [ ] Setup dependency injection in Program.cs

### Phase 3: Core Services Implementation (Weeks 3-4)
- [ ] Allocation Service
- [ ] Measurement Service
- [ ] Inventory Service
- [ ] Pricing Service

### Phase 4: Accounting Services (Weeks 5-6)
- [ ] Revenue Recognition Service
- [ ] Royalty Calculation Service
- [ ] Journal Entry Service
- [ ] Period Closing Service

### Phase 5: Advanced Features (Weeks 7-8)
- [ ] Imbalance Management
- [ ] Report Generation
- [ ] Compliance Reporting
- [ ] Partner Statements

### Phase 6: Testing & Optimization (Week 9)
- [ ] Unit tests for all services
- [ ] Integration tests
- [ ] Performance optimization
- [ ] Documentation

---

## Naming Conventions

### Services
- `I{Domain}Service` - Interface (e.g., IAllocationService)
- `{Domain}Service` - Implementation (e.g., AllocationService)
- `{Domain}Engine` - Complex logic (e.g., AllocationEngine)
- `{Domain}Calculator` - Calculations (e.g., RoyaltyCalculator)
- `{Domain}Validator` - Validation (e.g., ProductionValidator)

### Entities
- ALL CAPS with underscores (e.g., RUN_TICKET, ALLOCATION_RESULT)
- PPDM39 naming standards
- Properties also ALL CAPS

### Methods
- `CreateAsync` - Create new entity
- `GetByIdAsync` - Retrieve single entity
- `GetAllAsync` - Retrieve all entities
- `UpdateAsync` - Update entity
- `DeleteAsync` - Delete entity
- `PerformAsync` - Business operation

---

## Best Practices

### 1. Repository Usage
```csharp
var metadata = await _metadata.GetTableMetadataAsync("RUN_TICKET");
var repo = new PPDMGenericRepository(
    _editor, _commonColumnHandler, _defaults, _metadata,
    typeof(RUN_TICKET), _connectionName, "RUN_TICKET");

var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId },
    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
};

var tickets = await repo.GetAsync(filters);
```

### 2. Validation
- Validate inputs before database operations
- Check business rules (e.g., sum of allocation = 100%)
- Validate entity relationships exist
- Check for duplicate records

### 3. Logging
- Log all financial transactions
- Log allocation calculations
- Log user actions with timestamps
- Include operation IDs for traceability

### 4. Error Handling
- Create custom exceptions for business errors
- Log exceptions with full context
- Return meaningful error messages
- Never expose database errors to users

### 5. Async Operations
- All database calls are async
- Use `async Task` not `async void`
- Properly handle CancellationToken
- No blocking calls

---

## API Endpoint Structure

```
/api/production/
├── /run-tickets
│   ├── GET /{id}
│   ├── POST
│   ├── PUT /{id}
│   └── DELETE /{id}
├── /allocations
│   ├── GET /{id}
│   ├── POST (Calculate)
│   └── PUT /{id} (Update)
├── /royalties
│   ├── GET /{id}
│   ├── POST (Calculate)
│   └── GET /statements/{partyId}
├── /measurements
│   ├── GET /{id}
│   ├── POST (Record)
│   ├── PUT /{id}
│   └── POST /{id}/validate
├── /inventory
│   ├── GET
│   ├── POST (Adjust)
│   └── GET /valuations
└── /reports
    ├── /production/{wellId}
    ├── /financial/{period}
    └── /compliance/{jurisdictionId}
```

---

## SQL Database Schema Assumptions

All tables follow PPDM39 standard:
- Primary Key: Entity ID (string)
- Common Columns: ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE, ACTIVE_IND
- Foreign Keys: *_ID columns to related entities
- Indexes: On commonly filtered columns (WELL_ID, FIELD_ID, ACTIVE_IND)

---

## Success Criteria

- [ ] Zero compilation errors
- [ ] All services follow standard interface pattern
- [ ] No DTO classes - only Data entities
- [ ] All business logic in service layer
- [ ] Unit test coverage > 80%
- [ ] API endpoints fully documented with Swagger
- [ ] PPDM39 compliance verified
- [ ] Oil & gas accounting standards implemented

---

## References

1. **PPDM39 Standard**: Petroleum Data Management v3.9
2. **FASB ASC 932**: Accounting Standards for Oil & Gas
3. **FASB ASC 606**: Revenue from Contracts with Customers
4. **SEC Guidance**: Oil & Gas Reserves and Disclosure
5. **API Guide**: American Petroleum Institute standards
6. **Project CLAUDE.md**: Beep.OilandGas architecture guidelines

---

## Rollout Plan

1. **Code Review**: Architecture review with team
2. **Spike**: Implement one complete service (e.g., Allocation)
3. **Feedback**: Adjust patterns based on spike results
4. **Full Build**: Implement remaining services
5. **Testing**: Comprehensive test suite
6. **Documentation**: Complete API documentation
7. **Deployment**: Staged rollout

---

**Document Version**: 1.0  
**Last Updated**: January 2026  
**Status**: Planning Phase
