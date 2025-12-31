# Best Practices

## Overview

This document outlines best practices for oil and gas accounting, data management, and system development following FASB, SEC, and industry standards.

## FASB Compliance Patterns

### Successful Efforts Method (FASB Statement No. 19)

**Property Acquisition:**
- ✅ **Capitalize** all acquisition costs as unproved property
- ✅ Track by property ID with acquisition date and cost
- ✅ Reclassify to proved property when reserves are discovered
- ✅ Test for impairment annually on unproved properties

**Exploration Costs:**
- ✅ **Expense** geological and geophysical (G&G) costs as incurred
- ✅ **Capitalize** exploratory drilling costs if well finds proved reserves
- ✅ **Expense** exploratory drilling costs if well is dry hole
- ✅ Maintain detailed records of exploration activities

**Development Costs:**
- ✅ **Capitalize** all development costs (drilling, completion, facilities)
- ✅ Track by property and well
- ✅ Link to AFE (Authorization for Expenditure) for approval tracking

**Production Costs:**
- ✅ **Expense** all production costs (lifting costs) as incurred
- ✅ Include operating expenses, maintenance, workovers
- ✅ Track by property and well for cost allocation

**Amortization:**
- ✅ Use **units-of-production method** based on proved reserves
- ✅ Calculate amortization rate: Net Capitalized Costs / Total Proved Reserves BOE
- ✅ Apply rate to production for period
- ✅ Update reserves annually

### Full Cost Method (Alternative)

**Cost Capitalization:**
- ✅ **Capitalize** all exploration and development costs
- ✅ Organize by cost center (country, region, or field)
- ✅ No distinction between successful and unsuccessful exploration

**Ceiling Test:**
- ✅ Perform **annually** to ensure capitalized costs don't exceed discounted future net cash flows
- ✅ Calculate: Net Capitalized Costs vs. Discounted Future Net Cash Flows
- ✅ Record impairment if ceiling exceeded
- ✅ Use appropriate discount rate (typically 10%)

## Revenue Recognition

### Point of Sale
- ✅ Recognize revenue when **title transfers** (typically at wellhead or delivery point)
- ✅ Use run tickets or LACT unit measurements for volume
- ✅ Apply pricing at time of sale (index-based with differentials)

### Price Determination
- ✅ Use **index-based pricing** (WTI, Brent, Henry Hub)
- ✅ Apply **differentials** for quality, location, transportation
- ✅ Document pricing methodology in contracts
- ✅ Maintain price index history

### Volume Measurement
- ✅ Use **certified measurement** (run tickets, LACT units, meters)
- ✅ Apply **BSW (Basic Sediment and Water)** adjustments
- ✅ Convert to **standard conditions** (temperature, pressure)
- ✅ Maintain measurement audit trail

### Revenue Allocation
- ✅ Allocate revenue to **working interests** based on ownership
- ✅ Deduct **royalties** before allocation
- ✅ Apply **production sharing** agreements if applicable
- ✅ Maintain allocation audit trail

## Cost Management

### AFE (Authorization for Expenditure) Management
- ✅ Require **AFE approval** before capital expenditures
- ✅ Track **budget vs. actual** costs
- ✅ Monitor **variance** and require approval for overruns
- ✅ Link all costs to AFE for approval tracking

### Cost Centers
- ✅ Organize costs by **property**, **well**, **field**, or **region**
- ✅ Enable cost allocation and reporting
- ✅ Support both Successful Efforts and Full Cost methods

### Cost Allocation
- ✅ Allocate **shared costs** (facilities, overhead) equitably
- ✅ Use **volume-based**, **revenue-based**, or **equity-based** allocation
- ✅ Document allocation methodology
- ✅ Maintain allocation audit trail

## Production Accounting

### Run Ticket Management
- ✅ Create run ticket from **certified measurement**
- ✅ Include **BSW**, **temperature**, **API gravity**
- ✅ Link to **lease**, **well**, **tank battery**
- ✅ Track **disposition** (sale, transfer, inventory)

### Production Allocation
- ✅ Allocate production to **working interests** accurately
- ✅ Use **measured volumes** from run tickets
- ✅ Apply **allocation factors** (ownership percentages)
- ✅ Handle **imbalances** and adjustments

### Tank Inventory Management
- ✅ Maintain **opening inventory**, **receipts**, **deliveries**, **closing inventory**
- ✅ Reconcile **book inventory** vs. **actual inventory**
- ✅ Track **inventory adjustments** and reasons
- ✅ Calculate **inventory valuation** (FIFO, LIFO, weighted average)

## Royalty Management

### Royalty Calculation
- ✅ Calculate royalty based on **gross revenue** (before deductions)
- ✅ Apply **royalty interest percentage** from lease or contract
- ✅ Deduct **royalty** before allocating to working interests
- ✅ Support **different royalty types** (mineral, overriding, production payment)

### Royalty Payment
- ✅ Pay royalties **monthly** or **quarterly** per lease terms
- ✅ Provide **royalty statements** with production and revenue details
- ✅ Track **payment status** (pending, paid, overdue)
- ✅ Maintain **payment history**

## Joint Interest Billing (JIB)

### Cost Sharing
- ✅ Automate **cost allocation** among working interest owners
- ✅ Bill **non-operated** costs to working interest owners
- ✅ Include **overhead** and **administrative** charges
- ✅ Provide **detailed line items** for all costs

### Timely Billing
- ✅ Bill within **60-90 days** of cost incurrence
- ✅ Provide **approval workflow** for non-operated costs
- ✅ Track **payment status** and **aging**
- ✅ Reconcile with **operator statements**

## Internal Controls

### Segregation of Duties
- ✅ Separate **production**, **accounting**, and **approval** functions
- ✅ Require **dual approval** for capital expenditures
- ✅ Restrict **system access** based on role
- ✅ Maintain **audit logs** for all transactions

### Authorization
- ✅ Require **AFE approval** before capital expenditures
- ✅ Require **approval** for non-operated costs
- ✅ Require **approval** for revenue adjustments
- ✅ Maintain **approval workflow** and history

### Reconciliation
- ✅ Reconcile **production volumes** (run tickets vs. sales)
- ✅ Reconcile **revenue** (sales vs. cash receipts)
- ✅ Reconcile **costs** (invoices vs. payments)
- ✅ Reconcile **inventory** (book vs. actual)
- ✅ Perform **monthly reconciliations**

## Data Integrity

### PPDM Compliance
- ✅ Use **PPDM standard tables** where possible
- ✅ Follow **PPDM naming conventions**
- ✅ Include **standard audit columns** (ROW_CREATED_BY, ROW_CREATED_DATE, etc.)
- ✅ Maintain **referential integrity** with foreign keys

### Data Validation
- ✅ Validate **all inputs** (volumes, prices, costs)
- ✅ Enforce **business rules** (e.g., volumes cannot be negative)
- ✅ Check **data quality** (e.g., BSW cannot exceed 100%)
- ✅ Implement **data validation** at entry point

### Version Control
- ✅ Track **changes** to critical data (ownership, contracts)
- ✅ Maintain **version history** for contracts and agreements
- ✅ Implement **effective dating** for time-based data
- ✅ Support **audit queries** for historical data

## Audit Trail Patterns

### Standard Audit Columns

All entities must include:
- `ROW_CREATED_BY` (String) - Creator user ID
- `ROW_CREATED_DATE` (DateTime) - Creation date
- `ROW_CHANGED_BY` (String) - Last modifier user ID
- `ROW_CHANGED_DATE` (DateTime) - Last modification date
- `ROW_EFFECTIVE_DATE` (DateTime) - Effective date
- `ROW_EXPIRY_DATE` (DateTime) - Expiry date
- `ACTIVE_IND` (String) - Active indicator

### Audit Trail Implementation

```csharp
// Set audit columns when creating entity
var entity = new WELL
{
    WELL_ID = defaults.FormatIdForTable("WELL", Guid.NewGuid().ToString()),
    WELL_NAME = "Well-001",
    ACTIVE_IND = "Y",
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow,
    ROW_CHANGED_BY = userId,
    ROW_CHANGED_DATE = DateTime.UtcNow
};

// Update audit columns when modifying entity
entity.WELL_NAME = "Well-001-Updated";
entity.ROW_CHANGED_BY = userId;
entity.ROW_CHANGED_DATE = DateTime.UtcNow;
```

## Reporting and Compliance

### Financial Reports
- ✅ Generate **Income Statement** (revenue, costs, amortization)
- ✅ Generate **Balance Sheet** (assets, liabilities, equity)
- ✅ Generate **Cash Flow Statement** (operating, investing, financing)
- ✅ Include **FASB disclosures** (reserves, costs, production)

### Operational Reports
- ✅ Generate **Production Reports** (volumes by property, well, field)
- ✅ Generate **Revenue Reports** (revenue by property, product, customer)
- ✅ Generate **Cost Reports** (costs by property, well, cost center)
- ✅ Generate **AFE Reports** (budget vs. actual, variance analysis)

### Compliance Reports
- ✅ Generate **SEC Reports** (10-K, 10-Q disclosures)
- ✅ Generate **Tax Reports** (federal, state, local)
- ✅ Generate **Royalty Statements** (for royalty owners)
- ✅ Generate **JIB Statements** (for working interest owners)

## Key Principles Summary

1. **FASB Compliance**: Follow Successful Efforts or Full Cost method consistently
2. **Revenue Recognition**: Recognize at point of sale with proper documentation
3. **Cost Management**: Track all costs against AFEs and cost centers
4. **Internal Controls**: Segregate duties, require approvals, maintain audit trails
5. **Data Integrity**: Validate inputs, maintain referential integrity, track changes
6. **Reconciliation**: Regular reconciliation of production, revenue, costs, inventory
7. **Documentation**: Maintain complete audit trail and supporting documentation
8. **Compliance**: Generate required reports for SEC, tax, and regulatory compliance

## References

- See `Beep.OilandGas.ProductionAccounting/BEST_PRACTICES_OIL_GAS_ACCOUNTING.md` for complete best practices
- See `Beep.OilandGas.ProductionAccounting/ARCHITECTURE_AND_INTEGRATION.md` for integration patterns
- See FASB Statement No. 19 for Successful Efforts method
- See SEC Rule 4-10 for proved reserves definition

