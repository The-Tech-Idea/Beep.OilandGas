# Production Accounting Module - Real Business Logic Implementation Guide

## OBJECTIVE
Implement REAL FASB ASC 932, ASC 606, and COPAS compliant production accounting logic.

---

## 1. ROYALTY CALCULATION (FASB ASC 932 + COPAS)

### Real Formula
```
ROYALTY_PAYMENT = NET_REVENUE × ROYALTY_RATE

Where:
  NET_REVENUE = GROSS_REVENUE - DEDUCTIONS
  GROSS_REVENUE = Volume_BBL × Price_per_BBL
  DEDUCTIONS = Sum of:
    - Transportation Costs (typically 5-10% of gross)
    - Ad Valorem Tax (2-4% depending on state)
    - Severance Tax (variable by state: TX=4.6%, OK=7%, LA=12.5%)
    - Processing Fees (if applicable)
  
  ROYALTY_RATE = Interest % (varies by interest type):
    - Mineral Royalty: 12.5% (1/8th royalty - most common)
    - Overriding Royalty Interest (ORI): 2-5%
    - Net Profit Interest (NPI): 10-20%

Example:
  Production: 1,000 BBL
  Price: $75/BBL
  Gross Revenue: 1,000 × $75 = $75,000
  
  Deductions:
    - Transportation (8%): $6,000
    - Ad Valorem (2%): $1,500
    - Severance (4.6%): $3,450
    Total Deductions: $10,950
  
  Net Revenue: $75,000 - $10,950 = $64,050
  Royalty Rate: 12.5%
  Royalty Payment: $64,050 × 0.125 = $8,006.25
```

### Implementation Requirements
1. Query ALLOCATION_DETAIL for volume
2. Get commodity price from PRICE_INDEX (by date, commodity type)
3. Query DEDUCTIONS table for each cost type (or calculate %)
4. Create ROYALTY_DEDUCTIONS record (one per lease/period)
5. Create ROYALTY_CALCULATION record
6. Update ROYALTY_PAYMENT status (Pending → Accrued → Paid)

---

## 2. ALLOCATION (Pro Rata, Equation, Volumetric, Yield)

### Pro Rata Method (Most Common)
```
Allocation_i = (Interest_%_i / Total_Interest_%) × Total_Production

Example (3-party lease):
  Total Production: 1,000 BBL
  
  Operator (75% WI):      1,000 × (0.75 / 1.00) = 750 BBL
  Working Interest #1 (15% WI): 1,000 × (0.15 / 1.00) = 150 BBL
  Working Interest #2 (10% WI): 1,000 × (0.10 / 1.00) = 100 BBL
  Total: 1,000 BBL ✓
```

### Equation Method
Custom formula-based allocation. Example:
```
If Production > 200 BBL/day:
  Allocation = Production × Interest% × 1.2  (bonus for high volume)
Else:
  Allocation = Production × Interest%
```

### Volumetric Method
Based on reserve volumes, not percentage interests.
```
Allocation_i = (Reserves_i / Total_Reserves) × Total_Production

Example:
  Well A Reserves: 50,000 BOE
  Well B Reserves: 30,000 BOE
  Total Reserves: 80,000 BOE
  
  Total Production: 1,000 BBL
  
  Well A: (50,000/80,000) × 1,000 = 625 BBL
  Well B: (30,000/80,000) × 1,000 = 375 BBL
```

### Yield Method
Based on expected yield from reserves.
```
Allocation_i = (Expected_Yield_i / Total_Expected_Yield) × Total_Production
```

### Validation
- Sum of allocations = Total production (within 0.01% tolerance)
- Each allocation ≥ 0
- Each allocation ≤ Total production
- Handle imbalances: Over/under production vs. allocated

---

## 3. SUCCESSFUL EFFORTS METHOD (FASB ASC 932)

### Decision Tree
```
Is the well SUCCESSFUL?
├─ YES (producing or expected to produce)
│  ├─ CAPITALIZE the drilling costs
│  ├─ Add to Proved Property, Plant & Equipment (PP&E)
│  └─ Begin depreciation (UOP method)
│
└─ NO (dry hole or plugged & abandoned)
   ├─ EXPENSE the drilling costs immediately
   └─ Recognize loss on P&L
```

### Implementation
```csharp
public async Task<bool> IsWellSuccessfulAsync(string wellId, DateTime evaluationDate)
{
    // Check if well has PRODUCTION
    var production = await GetProductionAsync(wellId, evaluationDate);
    if (production > 0) return true;
    
    // Check if well has PROVED RESERVES
    var reserves = await GetProvedReservesAsync(wellId);
    if (reserves > 0) return true;
    
    // Check DRY HOLE indicator
    var wellStatus = await GetWellStatusAsync(wellId);
    if (wellStatus == "DRY HOLE" || wellStatus == "PLUGGED AND ABANDONED")
        return false;
    
    return false;  // Default to unsuccessful if no evidence
}
```

### Journal Entries

**For Successful Well:**
```
Debit:  Oil & Gas PP&E - Proved Properties         $XXX,XXX
  Credit: Cash / Accounts Payable                          $XXX,XXX
(Capitalize drilling cost)

Later, as well produces:
Debit:  Depletion Expense                          $XX,XXX
  Credit: Accumulated Depletion - Oil & Gas PP&E         $XX,XXX
(UOP depletion: see Amortization section)
```

**For Unsuccessful Well (Dry Hole):**
```
Debit:  Dry Hole Expense (P&L)                     $XXX,XXX
  Credit: Cash / Accounts Payable                        $XXX,XXX
(Immediate expense recognition)
```

---

## 4. FULL COST METHOD + SEC CEILING TEST (FASB ASC 932)

### Methodology
```
1. Capitalize ALL exploration & development costs in COST CENTER (country level)
2. Accumulate by cost center:
   - Equipment & facilities
   - Seismic & geological
   - Drilling & completion
   - Lease & land costs

3. Pool depletion by cost center
4. Apply SEC CEILING TEST quarterly:
   
   If Net Book Value (NBV) > Fair Value → IMPAIRMENT
```

### SEC Ceiling Test Formula
```
NBV = Capitalized Costs - Accumulated Depletion
Fair Value = PV of Future Cash Flows from Proved Reserves

If NBV > Fair Value:
  Impairment Charge = NBV - Fair Value
  Debit: Impairment Expense (P&L)
    Credit: Accumulated Depletion / Capitalized Costs

Example:
  Capitalized Costs (Mexico):    $50,000,000
  Less: Accumulated Depletion:  ($30,000,000)
  NBV:                           $20,000,000
  
  PV of proved reserves (10% discount rate):  $15,000,000
  
  Impairment Required:  $20,000,000 - $15,000,000 = $5,000,000
```

### Implementation Steps
```csharp
public async Task PerformCeilingTestAsync(string costCenter, DateTime testDate)
{
    // 1. Calculate NBV
    decimal capitalizedCosts = await GetCapitalizedCostsAsync(costCenter);
    decimal accumulatedDepletion = await GetAccumulatedDepletionAsync(costCenter);
    decimal nbv = capitalizedCosts - accumulatedDepletion;
    
    // 2. Calculate Fair Value (PV of proved reserves)
    var provedReserves = await GetProvedReservesAsync(costCenter);
    decimal discountRate = 0.10m;  // 10% per SEC
    decimal fairValue = CalculatePresentValue(provedReserves, discountRate);
    
    // 3. Test & Impair if needed
    if (nbv > fairValue)
    {
        decimal impairment = nbv - fairValue;
        await RecordImpairmentAsync(costCenter, impairment, testDate);
    }
}
```

---

## 5. REVENUE RECOGNITION (FASB ASC 606)

### Core Principle
"Recognize revenue when control of product transfers to customer"

### 5-Step Model
```
1. IDENTIFY THE CONTRACT
   - Customer purchase agreement
   - Specifies volume, quality, delivery point

2. IDENTIFY PERFORMANCE OBLIGATIONS
   - Delivery of oil/gas = 1 performance obligation
   - Delivery at wellhead vs. pipeline vs. sales point?

3. DETERMINE TRANSACTION PRICE
   - Gross Revenue = Volume × Price
   - Adjust for:
     - Volume discounts
     - Quality adjustments (API gravity, sulfur content)
     - Take-or-pay adjustments

4. ALLOCATE TRANSACTION PRICE
   - If multiple products (oil + gas), allocate by relative sales price
   - If multiple delivery points, allocate by point

5. RECOGNIZE REVENUE
   - When control transfers (typically at delivery point)
   - Per ASC 606: "When customer obtains control"
   - Status flow: Deferred → Recognized → Billed → Collected
```

### Implementation
```
For each ALLOCATION_DETAIL:
  Volume = 1,000 BBL
  Price = $75/BBL (fetch from PRICE_INDEX)
  Interest% = 100%
  
  Gross Revenue = 1,000 × $75 = $75,000
  
  Create REVENUE_ALLOCATION:
    REVENUE_TRANSACTION_ID = unique_id
    INTEREST_OWNER_BA_ID = operator_id
    INTEREST_PERCENTAGE = 100%
    ALLOCATED_AMOUNT = $75,000
    REVENUE_STATUS = "Recognized"
    
  Create GL entries:
    Debit:  Accounts Receivable     $75,000
    Credit: Oil & Gas Revenue              $75,000
```

---

## 6. GENERAL LEDGER (DOUBLE-ENTRY ACCOUNTING)

### Golden Rule
```
DEBITS = CREDITS (within tolerance: 0.01%)

Every transaction has:
  - At least one debit
  - At least one credit
  - Sum of debits = Sum of credits
```

### Standard GL Accounts
