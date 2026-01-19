# Allocation & Royalty Management - Implementation Plan

## Overview

Implements production allocation and joint interest accounting per COPAS standards:
- **Production Allocation**: Distribute well production across wells, leases, tracts
- **Royalty Calculation**: Calculate payments to mineral interest owners
- **Joint Interest Accounting**: Multi-party cost and revenue sharing
- **Imbalance Management**: Handle over/under-production situations

---

## Allocation Domain

### Allocation Engine

**File**: `Allocation/AllocationEngine.cs`

**Responsibility**: Core allocation calculation engine

**Allocation Methods**:
1. **Pro Rata**: Allocation = (Well Interest x Total Production) / 100
2. **Equation**: Custom formula-based allocation
3. **Volumetric**: Volume-based allocation (MCF, BBL)
4. **Yield**: Well performance-based allocation

**Interface**:
```csharp
public interface IAllocationEngine
{
    // Pro Rata Allocation
    Task<List<(string wellId, decimal allocated)>> AllocateProRataAsync(
        List<WELL_ALLOCATION_DATA> wells,
        decimal totalVolume,
        string? connectionName = null);

    // Equation-Based Allocation
    Task<List<(string wellId, decimal allocated)>> AllocateByEquationAsync(
        string allocationFormula,
        List<WELL_ALLOCATION_DATA> wells,
        decimal totalVolume,
        string? connectionName = null);

    // Validate Allocation
    Task<(bool isValid, decimal totalAllocated, decimal variance)> ValidateAllocationAsync(
        List<(string wellId, decimal allocated)> allocations,
        decimal totalVolume,
        string? connectionName = null);

    // Get Allocation Method Capabilities
    Task<List<string>> GetSupportedAllocationMethodsAsync();
}
```

---

### Allocation Service

**File**: `Allocation/AllocationService.cs`

**Responsibility**: Orchestrate allocation operations

**Entities**:
- RUN_TICKET (production source)
- WELL_ALLOCATION_DATA (well details)
- ALLOCATION_RESULT (final allocation)
- ALLOCATION_DETAIL (line items)

**Interface**:
```csharp
public interface IAllocationService
{
    // Primary Allocation
    Task<ALLOCATION_RESULT> AllocateProductionAsync(
        RUN_TICKET runTicket,
        List<WELL_ALLOCATION_DATA> wells,
        ALLOCATION_METHOD method,
        string userId,
        string? connectionName = null);

    // Lease-Level Allocation
    Task<ALLOCATION_RESULT> AllocateToLeasesAsync(
        ALLOCATION_RESULT wellAllocation,
        List<LEASE_ALLOCATION_DATA> leases,
        string userId,
        string? connectionName = null);

    // Tract-Level Allocation
    Task<ALLOCATION_RESULT> AllocateToTractsAsync(
        ALLOCATION_RESULT leaseAllocation,
        List<TRACT_ALLOCATION_DATA> tracts,
        string userId,
        string? connectionName = null);

    // Validation & Reports
    Task<(bool isValid, List<string> issues)> ValidateAllocationAsync(
        ALLOCATION_RESULT allocation,
        string? connectionName = null);

    Task<ALLOCATION_REPORT_SUMMARY> GetAllocationSummaryAsync(
        DateTime fromDate,
        DateTime toDate,
        string? wellId = null,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Validate allocation percentages sum to 100%
- Track allocation method used
- Support allocation adjustments
- Maintain allocation history
- Generate allocation reports

---

## Royalty Management Domain

### Royalty Calculation Service

**File**: `Royalty/RoyaltyService.cs`

**Responsibility**: Calculate royalty payments; handle mineral interest allocation

**Entities**:
- ALLOCATION_RESULT (source data)
- ROYALTY_INTEREST (interest definitions)
- ROYALTY_CALCULATION (calculations)
- ROYALTY_PAYMENT (payments)
- ROYALTY_DEDUCTIONS (deductions)

**Interface**:
```csharp
public interface IRoyaltyService
{
    // Royalty Calculation
    Task<ROYALTY_CALCULATION> CalculateRoyaltyAsync(
        ALLOCATION_RESULT allocation,
        List<ROYALTY_INTEREST> interests,
        PRICE_INDEX price,
        string userId,
        string? connectionName = null);

    // Net Revenue Calculation
    Task<(decimal gross, decimal costs, decimal deductions, decimal netRevenue)>
        CalculateNetRevenueAsync(
            SALES_TRANSACTION sales,
            List<ROYALTY_DEDUCTIONS> deductions,
            string? connectionName = null);

    // Royalty Payment Creation
    Task<ROYALTY_PAYMENT> CreateRoyaltyPaymentAsync(
        ROYALTY_CALCULATION calculation,
        string recipientId,
        DateTime paymentDate,
        string userId,
        string? connectionName = null);

    // Working Interest Accounting
    Task<COST_ALLOCATION> AllocateCostsToInterestsAsync(
        List<ACCOUNTING_COST> costs,
        List<OWNERSHIP_INTEREST> interests,
        string userId,
        string? connectionName = null);

    // Reconciliation
    Task<(decimal calculated, decimal paid, decimal variance)> ReconcileRoyaltyAsync(
        string wellId,
        DateTime fromDate,
        DateTime toDate,
        string? connectionName = null);

    // Reports
    Task<List<ROYALTY_PAYMENT>> GetPaymentHistoryAsync(
        string recipientId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Royalty = (Net Revenue x Royalty Rate)
- Net Revenue = (Gross Revenue - Transportation - Ad Valorem Tax)
- Collect and apply severance taxes
- Handle revenue deductions per lease terms
- Track royalty interest ownership changes

---

### Joint Interest Billing (JIB) Service

**File**: `Royalty/JointInterestBillingService.cs`

**Responsibility**: COPAS-compliant joint interest accounting

**Entities**:
- OWNERSHIP_INTEREST (interest tracking)
- JIB_PARTICIPANT (party information)
- JIB_CHARGE (charges/costs)
- JIB_CREDIT (revenue sharing)
- JOINT_INTEREST_STATEMENT (statement)
- JIB_COST_ALLOCATION (cost allocation)

**Interface**:
```csharp
public interface IJointInterestBillingService
{
    // Interest Management
    Task<OWNERSHIP_INTEREST> RecordOwnershipInterestAsync(
        string wellId,
        string partyId,
        decimal workingInterest,  // operator's interest
        decimal netRoyaltyInterest,
        string userId,
        string? connectionName = null);

    // Cost Allocation (COPAS AG)
    Task<List<JIB_CHARGE>> AllocateCostsAsync(
        string wellId,
        DateTime month,
        List<ACCOUNTING_COST> costs,
        List<OWNERSHIP_INTEREST> interests,
        string userId,
        string? connectionName = null);

    // Revenue Sharing
    Task<List<JIB_CREDIT>> ShareRevenueAsync(
        string wellId,
        SALES_TRANSACTION sales,
        List<OWNERSHIP_INTEREST> interests,
        string userId,
        string? connectionName = null);

    // Overhead Allocation (per COPAS standards)
    Task<decimal> CalculateOverheadAsync(
        string wellId,
        DateTime month,
        decimal baseAmount,
        decimal overheadRate,
        string? connectionName = null);

    // JIB Statement Generation
    Task<JOINT_INTEREST_STATEMENT> GenerateStatementAsync(
        string wellId,
        DateTime statementPeriodEnd,
        string? connectionName = null);

    // Settlement & Payment
    Task<ROYALTY_PAYMENT> SettleJIBStatementAsync(
        string statementId,
        string userId,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Each party's share = (Interest x Net Revenue) - (Interest x Costs & Deductions)
- Track operator's burden (non-consent costs)
- Apply overhead per COPAS AG standards
- Generate monthly statements per lease/well
- Support settlement documentation

---

### Imbalance Management Service

**File**: `Royalty/ImbalanceService.cs`

**Responsibility**: Manage over/under-production situations

**Entities**:
- IMBALANCE_ADJUSTMENT (imbalance records)
- IMBALANCE_RECONCILIATION (settlement)
- OIL_IMBALANCE (inventory difference)
- IMBALANCE_STATEMENT (reporting)

**Interface**:
```csharp
public interface IImbalanceService
{
    // Imbalance Detection
    Task<OIL_IMBALANCE> DetectImbalanceAsync(
        string wellId,
        DateTime month,
        decimal producedVolume,
        decimal soldVolume,
        string? connectionName = null);

    // Imbalance Recording
    Task<IMBALANCE_ADJUSTMENT> RecordImbalanceAsync(
        string wellId,
        decimal quantity,
        string reason,  // "Over-produced", "Under-produced"
        string userId,
        string? connectionName = null);

    // Imbalance Settlement
    Task<IMBALANCE_RECONCILIATION> SettleImbalanceAsync(
        string wellId,
        DateTime settleDate,
        decimal settledQuantity,
        decimal settlePrice,
        string userId,
        string? connectionName = null);

    // Reports
    Task<List<OIL_IMBALANCE>> GetActiveImbalancesAsync(
        DateTime? asOfDate = null,
        string? connectionName = null);

    Task<IMBALANCE_STATEMENT> GenerateImbalanceReportAsync(
        DateTime asOfDate,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Track cumulative imbalance by well
- Calculate monetary settlement value
- Handle imbalance reserves (liability)
- Reconcile to final settlement
- Report under "liability" until settled

---

## Joint Interest Structure

```
Well Production 100 BBL

Working Interests:
  Operator:     75%
  Partner 1:    20%
  Partner 2:     5%

Net Royalty Interests:
  Lessor 1:     12.5%
  Lessor 2:      7.5%

Allocation:
  Operator's share:   75 BBL
  Partner 1's share:  20 BBL
  Partner 2's share:   5 BBL

Revenue ($1,000):
  Gross Revenue:    $1,000
  Less: Costs        ($200)  [allocated by interest]
  Less: Royalties    ($200)  [12.5% + 7.5% to lessors]
  Net to Operator:   $600   (75% of remainder)
  Partner 1:         $160   (20% of remainder)
  Partner 2:          $40   (5% of remainder)
```

---

## Calculation Examples

### Pro Rata Allocation
```
Total Production: 1,000 BBL

Well A (75% interest):  750 BBL
Well B (20% interest):  200 BBL
Well C (5% interest):    50 BBL
Total:                1,000 BBL
```

### Royalty Calculation
```
Gross Revenue:              $10,000
Less: Transportation        ($1,000)
Less: Ad Valorem Tax        ($100)
Net Revenue:                $8,900

Royalty Rate: 12.5%
Royalty Payment:            $1,112.50
```

### Net Profit Interest
```
Revenue:                    $10,000
Less: Costs                 ($2,000)
Less: Royalties             ($1,000)
Profit Interest Base:       $7,000
NPI Rate: 10%
NPI Payment:                $700
```

---

## Validation Rules

1. **Allocation Validation**:
   - Sum of allocations = Total production within 0.01%
   - All allocations positive
   - No allocation exceeds 100%

2. **Royalty Validation**:
   - Royalty <= Net Revenue
   - Working Interest <= 100%
   - All royalty rates > 0%

3. **JIB Validation**:
   - Total interests = 100%
   - Costs allocated by interest
   - Revenue shared by interest

4. **Imbalance Validation**:
   - Imbalance = |Produced - Sold|
   - Settlement <= Imbalance quantity
   - Settlement price reasonable

---

## Reports

1. **Allocation Report**: Production by well/lease/tract
2. **Royalty Statement**: Payments by recipient
3. **JIB Statement**: Monthly statement per well
4. **Imbalance Report**: Active imbalances and reserves
5. **Reconciliation Report**: Allocated vs Sold

---

**Document Version**: 1.0
**Status**: Ready for Implementation
