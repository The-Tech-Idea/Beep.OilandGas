# Pricing Module Enhancement Plan

## Current State Analysis

### Existing Files
- `PricingManager.cs` - Manager class with IDataSource integration, uses Dictionary conversions
- `PriceIndexManager.cs` - Manager class with IDataSource integration
- `RegulatedPricing.cs` - Regulated pricing manager
- `RUN_TICKET_VALUATION.cs` - Static valuation engine
- `PricingModels.cs` - Contains RUN_TICKET_VALUATION, QualityAdjustments, LocationAdjustments, TimeAdjustments

### Issues Identified
1. **Dictionary Conversions**: PricingManager uses ConvertValuationToDictionary (should use entities directly)
2. **Models in Wrong Location**: PricingModels.cs classes should be in Beep.OilandGas.Models
3. **No Service Interface**: Missing IPricingService interface
4. **Missing Workflows**: No price index history tracking, no pricing reconciliation, no pricing approval workflow

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Pricing/`:**
- `RUN_TICKET_VALUATION` → `RUN_TICKET_VALUATION` (entity class with PPDM audit columns)
- `PriceIndex` → `PRICE_INDEX` (entity class)
- `RegulatedPrice` → `REGULATED_PRICE` (entity class)

**Keep as DTOs in `Beep.OilandGas.Models/DTOs/Pricing/`:**
- `QualityAdjustments` (value object)
- `LocationAdjustments` (value object)
- `TimeAdjustments` (value object)

**Keep in ProductionAccounting:**
- `RUN_TICKET_VALUATIONEngine` static methods (calculation logic)
- `PricingMethod` enum (business logic)

## Service Class Creation

### New Service: PricingService

**Location**: `Beep.OilandGas.ProductionAccounting/Pricing/PricingService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IPricingService.cs`

```csharp
public interface IPricingService
{
    Task<RUN_TICKET_VALUATION> ValueRunTicketAsync(
        ValueRunTicketRequest request,
        string userId,
        string? connectionName = null);
    
    Task<RUN_TICKET_VALUATION?> GetValuationAsync(string valuationId, string? connectionName = null);
    Task<List<RUN_TICKET_VALUATION>> GetValuationsByRunTicketAsync(string runTicketNumber, string? connectionName = null);
    
    // Price Index Management
    Task<PRICE_INDEX> CreatePriceIndexAsync(CreatePriceIndexRequest request, string userId, string? connectionName = null);
    Task<PRICE_INDEX?> GetLatestPriceAsync(string indexName, string? connectionName = null);
    Task<List<PRICE_INDEX>> GetPriceHistoryAsync(string indexName, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    
    // Missing workflows
    Task<PricingReconciliationResult> ReconcilePricingAsync(PricingReconciliationRequest request, string userId, string? connectionName = null);
    Task<List<PricingApproval>> GetPricingApprovalsAsync(string? connectionName = null);
    Task<PricingApprovalResult> ApprovePricingAsync(string valuationId, string approverId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Removes Dictionary conversions
- Uses entities directly

## Database Integration

### Tables Required

**RUN_TICKET_VALUATION**:
- VALUATION_ID (PK)
- RUN_TICKET_NUMBER (FK to RUN_TICKET)
- VALUATION_DATE
- BASE_PRICE
- ADJUSTED_PRICE
- NET_VOLUME
- TOTAL_VALUE
- PRICING_METHOD
- Quality, Location, Time adjustment fields
- Standard PPDM audit columns

**PRICE_INDEX**:
- PRICE_INDEX_ID (PK)
- INDEX_NAME
- COMMODITY_TYPE
- PRICE_DATE
- PRICE_VALUE
- CURRENCY_CODE
- Standard PPDM audit columns

**REGULATED_PRICE**:
- REGULATED_PRICE_ID (PK)
- REGULATORY_AUTHORITY
- EFFECTIVE_DATE
- PRICE_VALUE
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("RUN_TICKET_VALUATION");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Pricing.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "RUN_TICKET_VALUATION");
```

## Missing Workflows

### 1. Price Index History Tracking
- Track price index changes over time
- Price index trend analysis
- Price index forecasting
- Price index reporting

### 2. Pricing Reconciliation
- Reconcile pricing vs contracts
- Reconcile pricing vs sales
- Identify pricing discrepancies
- Generate reconciliation reports

### 3. Pricing Approval Workflow
- Require approval for pricing changes
- Track approval status
- Maintain approval history
- Pricing approval notifications

### 4. Pricing Rules Engine
- Define pricing rules by contract
- Apply pricing rules automatically
- Pricing rule validation
- Pricing rule reporting

### 5. Differential Management
- Manage location differentials
- Manage quality differentials
- Manage time differentials
- Differential history tracking

### 6. Pricing Reporting
- Pricing summary reports
- Pricing detail reports
- Price index reports
- Pricing variance reports

## Database Scripts

### Scripts to Create

**For RUN_TICKET_VALUATION, PRICE_INDEX, REGULATED_PRICE**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql` (FKs to RUN_TICKET)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Pricing/`
2. Add standard PPDM audit columns
3. Map all properties from PricingModels

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Pricing/`
2. Create DTOs for missing workflows

### Step 3: Create Service Interface
1. Create `IPricingService` interface
2. Define all service methods

### Step 4: Refactor PricingManager to PricingService
1. Rename PricingManager.cs to PricingService.cs
2. Update to implement IPricingService
3. Remove Dictionary conversions
4. Use entities directly with PPDMGenericRepository
5. Add missing workflow methods

### Step 5: Refactor PriceIndexManager
1. Update to use PPDMGenericRepository
2. Remove Dictionary conversions
3. Use PRICE_INDEX entity directly

### Step 6: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)
2. Place scripts in appropriate directories

### Step 7: Implement Missing Workflows
1. Implement price index history tracking
2. Implement pricing reconciliation
3. Implement pricing approval workflow
4. Implement pricing rules engine
5. Implement differential management
6. Enhance pricing reporting

## Testing Requirements

1. Test run ticket valuation
2. Test price index management
3. Test regulated pricing
4. Test pricing reconciliation
5. Test pricing approval workflow

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Production module (for RunTicket)

