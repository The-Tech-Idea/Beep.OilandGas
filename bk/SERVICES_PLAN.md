# Production Accounting Services Layer - Implementation Plan

## Overview

The Services layer implements business logic for production accounting operations using the PPDM39 data model from `Models/Data/ProductionAccounting`.

**Key Principle**: Direct entity operations - no DTOs

---

## Services Architecture

### 1. Main Orchestrator Service

**File**: `Services/ProductionAccountingService.cs`

**Responsibility**: Orchestrate all production accounting operations

**Interface**:
```csharp
public interface IProductionAccountingService
{
    // Run Ticket Operations
    Task<RUN_TICKET> CreateRunTicketAsync(RUN_TICKET ticket, string userId, string? connectionName = null);
    Task<RUN_TICKET?> GetRunTicketAsync(string ticketId, string? connectionName = null);
    Task<List<RUN_TICKET>> GetRunTicketsByWellAsync(string wellId, string? connectionName = null);
    Task<RUN_TICKET> UpdateRunTicketAsync(RUN_TICKET ticket, string userId, string? connectionName = null);
    Task<bool> DeleteRunTicketAsync(string ticketId, string userId, string? connectionName = null);
    
    // Daily Operations
    Task<PRODUCTION_REPORT_SUMMARY> GenerateDailyProductionAsync(DateTime date, string fieldId, string? connectionName = null);
    Task<(List<RUN_TICKET> tickets, List<MEASUREMENT_RECORD> measurements)> ReconcileDailyProductionAsync(DateTime date, string fieldId, string userId, string? connectionName = null);
    
    // Period Closing
    Task<List<ValidationResult>> ValidateMonthEndDataAsync(DateTime monthEnd, string fieldId, string? connectionName = null);
    Task<OPERATIONAL_REPORT> GenerateMonthEndReportAsync(DateTime monthEnd, string fieldId, string? connectionName = null);
}
```

**Implementation Notes**:
- Delegates to specialized services (Allocation, Royalty, Revenue)
- Coordinates workflows across multiple services
- Maintains data consistency

---

### 2. Allocation Service

**File**: `Services/AllocationService.cs`

**Responsibility**: Production allocation from wells to leases/tracts

**Entities Used**:
- RUN_TICKET (input)
- WELL_ALLOCATION_DATA (input)
- ALLOCATION_RESULT (output)
- ALLOCATION_DETAIL (output)
- ALLOCATION_METHOD (enum)

**Interface**:
```csharp
public interface IAllocationService
{
    // Allocation Calculations
    Task<ALLOCATION_RESULT> AllocateProductionAsync(
        RUN_TICKET runTicket, 
        List<WELL_ALLOCATION_DATA> wells,
        ALLOCATION_METHOD method,
        string userId, 
        string? connectionName = null);
    
    Task<ALLOCATION_RESULT> AllocateToLeasesAsync(
        ALLOCATION_RESULT allocation,
        List<LEASE_ALLOCATION_DATA> leases,
        string userId,
        string? connectionName = null);
    
    Task<ALLOCATION_RESULT> AllocateToTractsAsync(
        ALLOCATION_RESULT allocation,
        List<TRACT_ALLOCATION_DATA> tracts,
        string userId,
        string? connectionName = null);
    
    // Validation
    Task<bool> ValidateAllocationAsync(ALLOCATION_RESULT allocation, string? connectionName = null);
    Task<List<ALLOCATION_DETAIL>> GetAllocationDetailsAsync(string allocationId, string? connectionName = null);
    
    // History
    Task<List<ALLOCATION_RESULT>> GetAllocationHistoryAsync(string runTicketId, string? connectionName = null);
    Task<ALLOCATION_RESULT?> GetLatestAllocationAsync(string runTicketId, string? connectionName = null);
}
```

**Implementation Notes**:
- Use AllocationEngine for complex calculations
- Support multiple allocation methods (Pro-rata, Equation, Volumetric, Yield)
- Validate sum of allocations = 100%
- Track allocation changes for audit trail

---

### 3. Royalty Service

**File**: `Services/RoyaltyService.cs`

**Responsibility**: Calculate and manage royalty payments

**Entities Used**:
- ALLOCATION_RESULT (input)
- ROYALTY_INTEREST (input)
- ROYALTY_CALCULATION (working)
- ROYALTY_PAYMENT (output)
- ROYALTY_DEDUCTIONS (output)

**Interface**:
```csharp
public interface IRoyaltyService
{
    // Royalty Calculations
    Task<ROYALTY_CALCULATION> CalculateRoyaltyAsync(
        ALLOCATION_RESULT allocation,
        List<ROYALTY_INTEREST> interests,
        string userId,
        string? connectionName = null);
    
    Task<ROYALTY_PAYMENT> CreateRoyaltyPaymentAsync(
        ROYALTY_CALCULATION calculation,
        string recipientId,
        DateTime paymentDate,
        string userId,
        string? connectionName = null);
    
    // Deductions
    Task<ROYALTY_DEDUCTIONS> ApplyDeductionsAsync(
        ROYALTY_CALCULATION calculation,
        decimal severanceTax,
        decimal adValoremTax,
        decimal transportationCost,
        string userId,
        string? connectionName = null);
    
    // Reconciliation
    Task<(decimal calculated, decimal paid, decimal variance)> ReconcileRoyaltyAsync(
        string wellId,
        DateTime fromDate,
        DateTime toDate,
        string? connectionName = null);
    
    // Statements
    Task<JOINT_INTEREST_STATEMENT> GenerateJIBStatementAsync(
        string fieldId,
        DateTime periodEnd,
        string? connectionName = null);
    
    Task<List<ROYALTY_PAYMENT>> GetPaymentHistoryAsync(
        string recipientId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Calculate net revenue: Sales - Costs - Deductions
- Apply royalty rates and interest calculations
- Handle Joint Interest Billing (JIB) for partner interests
- Reconcile with actual payments received

---

### 4. Measurement Service

**File**: `Services/MeasurementService.cs`

**Responsibility**: Manage and validate measurement data

**Entities Used**:
- RUN_TICKET (input)
- MEASUREMENT_RECORD (working)
- MEASUREMENT_ACCURACY (standards)
- MEASUREMENT_CORRECTIONS (adjustments)

**Interface**:
```csharp
public interface IMeasurementService
{
    // Record Measurements
    Task<MEASUREMENT_RECORD> RecordMeasurementAsync(
        MEASUREMENT_RECORD measurement,
        string userId,
        string? connectionName = null);
    
    // Validation
    Task<(bool isValid, List<string> issues)> ValidateMeasurementAsync(
        MEASUREMENT_RECORD measurement,
        MEASUREMENT_ACCURACY standard,
        string? connectionName = null);
    
    Task<bool> ValidateMeasurementAccuracyAsync(
        MEASUREMENT_RECORD measurement,
        string? connectionName = null);
    
    // Corrections
    Task<MEASUREMENT_CORRECTIONS> ApplyMeasurementCorrectionAsync(
        string measurementId,
        decimal correctionFactor,
        string reason,
        string userId,
        string? connectionName = null);
    
    // LACT (Lease Automatic Custody Transfer)
    Task<MEASUREMENT_RECORD> ProcessLACTUnitAsync(
        MEASUREMENT_RECORD measurement,
        string userId,
        string? connectionName = null);
    
    // Reports
    Task<MEASUREMENT_REPORT_SUMMARY> GenerateMeasurementReportAsync(
        string wellId,
        DateTime fromDate,
        DateTime toDate,
        string? connectionName = null);
    
    Task<List<MEASUREMENT_RECORD>> GetMeasurementHistoryAsync(
        string wellId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Validate against MEASUREMENT_ACCURACY standards
- Support multiple measurement methods (manual, automatic, LACT)
- Track measurement corrections and adjustments
- Ensure measurement consistency across wells

---

### 5. Pricing Service

**File**: `Services/PricingService.cs`

**Responsibility**: Manage pricing and price indices

**Entities Used**:
- PRICE_INDEX (reference prices)
- REGULATED_PRICE (regulated pricing)
- SALES_TRANSACTION (actual sales)
- EXCHANGE_TERMS (price adjustments)

**Interface**:
```csharp
public interface IPricingService
{
    // Price Index Management
    Task<PRICE_INDEX> CreatePriceIndexAsync(
        PRICE_INDEX priceIndex,
        string userId,
        string? connectionName = null);
    
    Task<PRICE_INDEX?> GetCurrentPriceAsync(
        string indexCode,
        string? connectionName = null);
    
    Task<List<PRICE_INDEX>> GetPriceHistoryAsync(
        string indexCode,
        DateTime fromDate,
        DateTime toDate,
        string? connectionName = null);
    
    // Pricing Calculations
    Task<decimal> CalculateNetPriceAsync(
        RUN_TICKET runTicket,
        SALES_CONTRACT salesContract,
        string? connectionName = null);
    
    Task<SALES_TRANSACTION> ApplyPricingAsync(
        RUN_TICKET runTicket,
        PRICE_INDEX priceIndex,
        string userId,
        string? connectionName = null);
    
    // Exchange Management
    Task<decimal> ApplyExchangeTermsAsync(
        decimal basePrice,
        EXCHANGE_TERMS terms,
        string? connectionName = null);
    
    // Price Adjustments
    Task<SALES_TRANSACTION> AdjustPriceAsync(
        string salesTransactionId,
        decimal adjustmentAmount,
        string reason,
        string userId,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Maintain historical price indices
- Calculate net prices (gross - adjustments - transportation)
- Handle price adjustments retroactively
- Support multiple pricing methods (fixed, index-based, market)

---

### 6. Revenue Service

**File**: `Services/RevenueService.cs`

**Responsibility**: Recognize revenue per ASC 606

**Entities Used**:
- SALES_TRANSACTION (transactions)
- INVOICE (billing)
- REVENUE_TRANSACTION (recognition)
- JOURNAL_ENTRY (GL posting)

**Interface**:
```csharp
public interface IRevenueService
{
    // Revenue Recognition
    Task<REVENUE_TRANSACTION> RecognizeRevenueAsync(
        SALES_TRANSACTION transaction,
        DateTime recognitionDate,
        string userId,
        string? connectionName = null);
    
    // Invoicing
    Task<INVOICE> CreateInvoiceAsync(
        List<SALES_TRANSACTION> transactions,
        string customerId,
        DateTime invoiceDate,
        string userId,
        string? connectionName = null);
    
    Task<bool> ReconcileInvoiceAsync(
        string invoiceId,
        decimal paidAmount,
        DateTime paymentDate,
        string userId,
        string? connectionName = null);
    
    // Revenue Reporting
    Task<decimal> GetTotalRevenueAsync(
        DateTime fromDate,
        DateTime toDate,
        string? wellId = null,
        string? connectionName = null);
    
    Task<List<REVENUE_TRANSACTION>> GetRevenueHistoryAsync(
        string wellId,
        DateTime fromDate,
        DateTime toDate,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Follow ASC 606 revenue recognition principles
- Match revenue to performance obligations (delivery)
- Handle returns and adjustments
- Reconcile with actual cash receipts

---

### 7. Inventory Service

**File**: `Services/InventoryService.cs`

**Responsibility**: Manage tank inventory and valuations

**Entities Used**:
- TANK_INVENTORY (inventory)
- INVENTORY_TRANSACTION (movement)
- INVENTORY_ADJUSTMENT (corrections)
- INVENTORY_VALUATION (pricing)

**Interface**:
```csharp
public interface IInventoryService
{
    // Inventory Tracking
    Task<TANK_INVENTORY> RecordInventoryAsync(
        TANK_INVENTORY inventory,
        string userId,
        string? connectionName = null);
    
    Task<decimal> GetCurrentInventoryAsync(
        string tankId,
        DateTime? asOfDate = null,
        string? connectionName = null);
    
    // Adjustments
    Task<INVENTORY_ADJUSTMENT> AdjustInventoryAsync(
        string tankId,
        decimal adjustmentQuantity,
        string reason,
        string userId,
        string? connectionName = null);
    
    // Valuations
    Task<INVENTORY_VALUATION> ValueInventoryAsync(
        string tankId,
        DateTime valuationDate,
        decimal pricePerBarrel,
        string userId,
        string? connectionName = null);
    
    Task<(decimal quantity, decimal value)> GetInventoryValueAsync(
        string tankId,
        DateTime asOfDate,
        string? connectionName = null);
    
    // Reports
    Task<INVENTORY_REPORT_SUMMARY> GenerateInventoryReportAsync(
        DateTime asOfDate,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Track inventory by tank/storage location
- Support FIFO, LIFO, weighted average valuation methods
- Reconcile physical counts with recorded amounts
- Generate month-end inventory reports

---

## Service Implementation Standards

### Dependency Injection

```csharp
// In Program.cs
builder.Services.AddScoped<IAllocationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<AllocationService>();
    
    return new AllocationService(
        editor, commonColumnHandler, defaults, metadata, 
        "DefaultConnection", logger);
});
```

### Error Handling

```csharp
public class AllocationException : Exception
{
    public AllocationException(string message) : base(message) { }
    public AllocationException(string message, Exception inner) : base(message, inner) { }
}
```

### Validation Pattern

```csharp
private async Task<(bool isValid, List<string> errors)> ValidateAllocationAsync(
    ALLOCATION_RESULT allocation, string? connectionName = null)
{
    var errors = new List<string>();
    
    if (allocation == null)
        errors.Add("Allocation cannot be null");
    
    if (string.IsNullOrEmpty(allocation.ALLOCATION_RESULT_ID))
        errors.Add("Allocation ID is required");
    
    var detailSum = allocation.Details?.Sum(d => d.ALLOCATION_PERCENT) ?? 0;
    if (Math.Abs(detailSum - 100m) > 0.01m)
        errors.Add($"Allocation percentages must sum to 100%, got {detailSum}");
    
    return (errors.Count == 0, errors);
}
```

---

## Service Implementation Sequence

1. **ProductionAccountingService** - Main orchestrator
2. **MeasurementService** - Data input validation
3. **AllocationService** - Core business logic
4. **PricingService** - Price management
5. **RoyaltyService** - Royalty calculations
6. **RevenueService** - Revenue recognition
7. **InventoryService** - Inventory management

---

## Testing Strategy

- Unit tests for each service method
- Integration tests with real data
- Scenario tests (daily, monthly, yearly close)
- Performance tests for large data sets

---

**Document Version**: 1.0  
**Status**: Ready for Implementation
