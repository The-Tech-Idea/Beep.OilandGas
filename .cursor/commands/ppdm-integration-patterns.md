# PPDM Integration Patterns

## Overview

This document outlines patterns for integrating with existing PPDM tables versus creating new tables, following PPDM standards and best practices.

## Core Principle

**Always check PPDM first** - Before creating a new table, check if PPDM has an equivalent table that can be used.

## Existing PPDM Tables to Leverage

### BUSINESS_ASSOCIATE

**Use for:** Vendors, Customers, Suppliers, Contractors, Royalty Owners, All Parties

**Fields:**
- `BUSINESS_ASSOCIATE_ID` - Primary key
- `BA_TYPE` - Type (VENDOR, CUSTOMER, SUPPLIER, etc.)
- `BA_NAME` - Name of the business associate
- `BA_SHORT_NAME` - Short name

**Usage Pattern:**

```csharp
// Get vendor repository
var vendorRepo = PPDMTableMapping.GetPPDMRepository(
    editor, 
    commonColumnHandler, 
    defaults, 
    metadata,
    typeof(BUSINESS_ASSOCIATE),
    PPDMTableMapping.TableNames.BusinessAssociate,
    connectionName);

// Create vendor (using BUSINESS_ASSOCIATE)
var vendor = new BUSINESS_ASSOCIATE
{
    BUSINESS_ASSOCIATE_ID = defaults.FormatIdForTable("BUSINESS_ASSOCIATE", Guid.NewGuid().ToString()),
    BA_TYPE = "VENDOR",
    BA_NAME = "ABC Drilling Company",
    BA_SHORT_NAME = "ABC Drilling",
    ACTIVE_IND = "Y",
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow
};
await vendorRepo.InsertAsync(vendor, userId);
```

**DO NOT CREATE:** Separate VENDOR, CUSTOMER tables  
**USE:** BUSINESS_ASSOCIATE with BA_TYPE = "VENDOR" or "CUSTOMER"

### CONTRACT

**Use for:** Sales Contracts, Purchase Contracts, Service Contracts, Transportation Contracts, Processing Contracts

**Fields:**
- `CONTRACT_ID` - Primary key
- `CONTRACT_TYPE` - Type of contract
- `CONTRACT_DATE` - Contract date
- `BUSINESS_ASSOCIATE_ID` - Party to the contract

**Usage Pattern:**

```csharp
// Sales contract → Use CONTRACT with CONTRACT_TYPE = "SALES"
var salesContract = new CONTRACT
{
    CONTRACT_ID = defaults.FormatIdForTable("CONTRACT", Guid.NewGuid().ToString()),
    CONTRACT_TYPE = "SALES",
    CONTRACT_DATE = DateTime.UtcNow,
    BUSINESS_ASSOCIATE_ID = buyerId, // Reference to BUSINESS_ASSOCIATE
    ACTIVE_IND = "Y"
};

// Purchase contract → Use CONTRACT with CONTRACT_TYPE = "PURCHASE"
var purchaseContract = new CONTRACT
{
    CONTRACT_ID = defaults.FormatIdForTable("CONTRACT", Guid.NewGuid().ToString()),
    CONTRACT_TYPE = "PURCHASE",
    CONTRACT_DATE = DateTime.UtcNow,
    BUSINESS_ASSOCIATE_ID = vendorId,
    ACTIVE_IND = "Y"
};
```

**DO NOT CREATE:** SALES_CONTRACT, PURCHASE_CONTRACT tables  
**USE:** CONTRACT with appropriate CONTRACT_TYPE

### FINANCE

**Use for:** Financial Transactions, Revenue Transactions (may enhance), Cost Transactions (may enhance)

**Fields:**
- `FINANCE_ID` - Primary key
- `FIN_TYPE` - Type of financial transaction
- `AMOUNT` - Transaction amount
- `TRANSACTION_DATE` - Transaction date

**Usage Pattern:**

```csharp
var financeRepo = PPDMTableMapping.GetPPDMRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(FINANCE),
    PPDMTableMapping.TableNames.Finance,
    connectionName);

var financeTransaction = new FINANCE
{
    FINANCE_ID = defaults.FormatIdForTable("FINANCE", Guid.NewGuid().ToString()),
    FIN_TYPE = "REVENUE",
    AMOUNT = 100000m,
    TRANSACTION_DATE = DateTime.UtcNow,
    ACTIVE_IND = "Y"
};
```

### OBLIGATION

**Use for:** Payment Obligations, Tax Obligations, Royalty Obligations

**Fields:**
- `OBLIGATION_ID` - Primary key
- `OBLIGATION_TYPE` - Type of obligation
- `OBLIGATION_AMOUNT` - Amount of obligation
- `DUE_DATE` - Due date

**Usage Pattern:**

```csharp
var obligationRepo = PPDMTableMapping.GetPPDMRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(OBLIGATION),
    PPDMTableMapping.TableNames.Obligation,
    connectionName);

var obligation = new OBLIGATION
{
    OBLIGATION_ID = defaults.FormatIdForTable("OBLIGATION", Guid.NewGuid().ToString()),
    OBLIGATION_TYPE = "AP_INVOICE",
    OBLIGATION_AMOUNT = 50000m,
    DUE_DATE = DateTime.UtcNow.AddDays(30),
    ACTIVE_IND = "Y"
};
```

### OBLIG_PAYMENT

**Use for:** Payment Records (for all types of payments)

**Fields:**
- `OBLIG_PAYMENT_ID` - Primary key
- `OBLIGATION_ID` - Related obligation
- `PAYMENT_AMOUNT` - Payment amount
- `PAYMENT_DATE` - Payment date

**Usage Pattern:**

```csharp
// DO NOT CREATE: AP_PAYMENT, AR_PAYMENT (separate tables)
// USE: OBLIG_PAYMENT linked to OBLIGATION

var paymentRepo = PPDMTableMapping.GetPPDMRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(OBLIG_PAYMENT),
    PPDMTableMapping.TableNames.ObligPayment,
    connectionName);

var payment = new OBLIG_PAYMENT
{
    OBLIG_PAYMENT_ID = defaults.FormatIdForTable("OBLIG_PAYMENT", Guid.NewGuid().ToString()),
    OBLIGATION_ID = obligation.OBLIGATION_ID,
    PAYMENT_AMOUNT = 50000m,
    PAYMENT_DATE = DateTime.UtcNow,
    ACTIVE_IND = "Y"
};
```

### LAND_RIGHT

**Use for:** Property Rights, Lease Rights, Mineral Rights

**Fields:**
- `LAND_RIGHT_ID` - Primary key
- `LAND_RIGHT_TYPE` - Type of right
- `PROPERTY_ID` - Related property

### EQUIPMENT

**Use for:** Equipment Inventory

**Fields:**
- `EQUIPMENT_ID` - Primary key
- `EQUIPMENT_TYPE` - Type of equipment
- `DESCRIPTION` - Equipment description

## New Tables Created (Not in PPDM)

The following tables are NOT in PPDM and have been created as new entity classes:

### General Ledger
- `GL_ACCOUNT`
- `GL_ENTRY`
- `JOURNAL_ENTRY`
- `JOURNAL_ENTRY_LINE`

### Invoice
- `INVOICE`
- `INVOICE_LINE_ITEM`
- `INVOICE_PAYMENT`

### Purchase Order
- `PURCHASE_ORDER`
- `PO_LINE_ITEM`
- `PO_RECEIPT`

### Accounts Payable
- `AP_INVOICE`
- `AP_PAYMENT`
- `AP_CREDIT_MEMO`

### Accounts Receivable
- `AR_INVOICE`
- `AR_PAYMENT`
- `AR_CREDIT_MEMO`

### Inventory
- `INVENTORY_ITEM`
- `INVENTORY_TRANSACTION`
- `INVENTORY_ADJUSTMENT`
- `INVENTORY_VALUATION`

### Revenue Accounting
- `REVENUE_TRANSACTION`
- `PRICE_INDEX`
- `REVENUE_ALLOCATION`

### Cost Accounting
- `COST_TRANSACTION`
- `COST_ALLOCATION`
- `AFE`
- `AFE_LINE_ITEM`
- `COST_CENTER`

### Financial Accounting
- `AMORTIZATION_RECORD`
- `IMPAIRMENT_RECORD`
- `CEILING_TEST_CALCULATION`

### Royalty
- `ROYALTY_INTEREST`
- `ROYALTY_OWNER`
- `ROYALTY_PAYMENT`
- `ROYALTY_PAYMENT_DETAIL`

## Complete Example: Creating a Vendor Invoice

```csharp
// 1. Get or create vendor (using BUSINESS_ASSOCIATE)
var vendorRepo = PPDMTableMapping.GetPPDMRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(BUSINESS_ASSOCIATE),
    PPDMTableMapping.TableNames.BusinessAssociate,
    connectionName);

var vendor = await vendorRepo.GetByIdAsync(vendorId);
if (vendor == null)
{
    vendor = new BUSINESS_ASSOCIATE
    {
        BUSINESS_ASSOCIATE_ID = defaults.FormatIdForTable("BUSINESS_ASSOCIATE", Guid.NewGuid().ToString()),
        BA_TYPE = "VENDOR",
        BA_NAME = "ABC Drilling Company",
        ACTIVE_IND = "Y",
        ROW_CREATED_BY = userId,
        ROW_CREATED_DATE = DateTime.UtcNow
    };
    await vendorRepo.InsertAsync(vendor, userId);
}

// 2. Create AP invoice (new table - not in PPDM)
var apInvoice = new AP_INVOICE
{
    AP_INVOICE_ID = defaults.FormatIdForTable("AP_INVOICE", Guid.NewGuid().ToString()),
    VENDOR_BA_ID = vendor.BUSINESS_ASSOCIATE_ID, // Reference to BUSINESS_ASSOCIATE
    INVOICE_NUMBER = "INV-001",
    INVOICE_DATE = DateTime.UtcNow,
    TOTAL_AMOUNT = 50000m,
    ACTIVE_IND = "Y",
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow
};

// 3. Create payment obligation (using OBLIGATION)
var obligationRepo = PPDMTableMapping.GetPPDMRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(OBLIGATION),
    PPDMTableMapping.TableNames.Obligation,
    connectionName);

var obligation = new OBLIGATION
{
    OBLIGATION_ID = defaults.FormatIdForTable("OBLIGATION", Guid.NewGuid().ToString()),
    OBLIGATION_TYPE = "AP_INVOICE",
    OBLIGATION_AMOUNT = apInvoice.TOTAL_AMOUNT,
    DUE_DATE = DateTime.UtcNow.AddDays(30),
    ACTIVE_IND = "Y",
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow
};
await obligationRepo.InsertAsync(obligation, userId);

// 4. Create payment (using OBLIG_PAYMENT)
var paymentRepo = PPDMTableMapping.GetPPDMRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(OBLIG_PAYMENT),
    PPDMTableMapping.TableNames.ObligPayment,
    connectionName);

var payment = new OBLIG_PAYMENT
{
    OBLIG_PAYMENT_ID = defaults.FormatIdForTable("OBLIG_PAYMENT", Guid.NewGuid().ToString()),
    OBLIGATION_ID = obligation.OBLIGATION_ID,
    PAYMENT_AMOUNT = apInvoice.TOTAL_AMOUNT,
    PAYMENT_DATE = DateTime.UtcNow,
    ACTIVE_IND = "Y",
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow
};
await paymentRepo.InsertAsync(payment, userId);
```

## Standard PPDM Columns

All tables (both PPDM and new) should include these standard columns:

- `ACTIVE_IND` (String) - Active indicator
- `PPDM_GUID` (String) - PPDM GUID
- `REMARK` (String) - Remarks
- `SOURCE` (String) - Data source
- `ROW_QUALITY` (String) - Row quality indicator
- `ROW_CREATED_BY` (String) - Creator user ID
- `ROW_CREATED_DATE` (DateTime) - Creation date
- `ROW_CHANGED_BY` (String) - Last modifier user ID
- `ROW_CHANGED_DATE` (DateTime) - Last modification date
- `ROW_EFFECTIVE_DATE` (DateTime) - Effective date
- `ROW_EXPIRY_DATE` (DateTime) - Expiry date
- `ROW_ID` (String) - Row identifier

## Best Practices

1. **Always check PPDM first** - Before creating a new table, check if PPDM has an equivalent
2. **Use junction tables** - For many-to-many relationships, use PPDM junction tables (e.g., APPLIC_AREA)
3. **Leverage BUSINESS_ASSOCIATE** - Use for all parties (vendors, customers, contractors, etc.)
4. **Use CONTRACT for all contracts** - Don't create separate contract tables
5. **Use OBLIGATION/OBLIG_PAYMENT** - For all payment obligations and payments
6. **Include standard audit columns** - All tables must include standard PPDM audit columns
7. **Use PPDMTableMapping** - Use PPDMTableMapping.GetPPDMRepository() to get repositories for PPDM tables
8. **Format IDs correctly** - Use `defaults.FormatIdForTable(tableName, id)` for all ID formatting

## References

- See `Beep.OilandGas.ProductionAccounting/PPDM_INTEGRATION_GUIDE.md` for complete integration guide
- See PPDM documentation for complete list of available tables
- See `Beep.OilandGas.ProductionAccounting/ARCHITECTURE_AND_INTEGRATION.md` for architecture patterns

