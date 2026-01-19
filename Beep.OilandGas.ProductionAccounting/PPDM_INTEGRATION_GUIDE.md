# PPDM Integration Guide for ProductionAccounting

## Overview

This guide explains how to use existing PPDM tables in the ProductionAccounting system instead of creating duplicate tables.

## Existing PPDM Tables to Leverage

### BUSINESS_ASSOCIATE
**Use for:** Vendors, Customers, Suppliers, Contractors, Royalty Owners, All Parties

**Fields:**
- `BUSINESS_ASSOCIATE_ID` - Primary key
- `BA_TYPE` - Type (VENDOR, CUSTOMER, SUPPLIER, etc.)
- `BA_NAME` - Name of the business associate
- `BA_SHORT_NAME` - Short name

**Usage:**
```csharp
// Get vendor repository
var vendorRepo = PPDMTableMapping.GetPPDMRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(BUSINESS_ASSOCIATE),
    PPDMTableMapping.TableNames.BusinessAssociate,
    connectionName);
```

### CONTRACT
**Use for:** Sales Contracts, Purchase Contracts, Service Contracts, Transportation Contracts, Processing Contracts

**Fields:**
- `CONTRACT_ID` - Primary key
- `CONTRACT_TYPE` - Type of contract
- `CONTRACT_DATE` - Contract date
- `BUSINESS_ASSOCIATE_ID` - Party to the contract

**Usage:**
- Sales contracts -> Use CONTRACT with CONTRACT_TYPE = "SALES"
- Purchase contracts -> Use CONTRACT with CONTRACT_TYPE = "PURCHASE"
- Service contracts -> Use CONTRACT with CONTRACT_TYPE = "SERVICE"

### FINANCE
**Use for:** Financial Transactions, Revenue Transactions (may enhance), Cost Transactions (may enhance)

**Fields:**
- `FINANCE_ID` - Primary key
- `FIN_TYPE` - Type of financial transaction
- `AMOUNT` - Transaction amount
- `TRANSACTION_DATE` - Transaction date

### OBLIGATION
**Use for:** Payment Obligations, Tax Obligations, Royalty Obligations

**Fields:**
- `OBLIGATION_ID` - Primary key
- `OBLIGATION_TYPE` - Type of obligation
- `OBLIGATION_AMOUNT` - Amount of obligation
- `DUE_DATE` - Due date

### OBLIG_PAYMENT
**Use for:** Payment Records (for all types of payments)

**Fields:**
- `OBLIG_PAYMENT_ID` - Primary key
- `OBLIGATION_ID` - Related obligation
- `PAYMENT_AMOUNT` - Payment amount
- `PAYMENT_DATE` - Payment date

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

## Mapping Strategy

### Vendors and Customers
- **DO NOT CREATE:** VENDOR, CUSTOMER tables
- **USE:** BUSINESS_ASSOCIATE with BA_TYPE = "VENDOR" or "CUSTOMER"

### Contracts
- **DO NOT CREATE:** SALES_CONTRACT, PURCHASE_CONTRACT tables
- **USE:** CONTRACT with appropriate CONTRACT_TYPE

### Payments
- **DO NOT CREATE:** AP_PAYMENT, AR_PAYMENT (separate tables)
- **USE:** OBLIG_PAYMENT linked to OBLIGATION

### Property/Land
- **DO NOT CREATE:** PROPERTY table (if LAND_RIGHT covers it)
- **USE:** LAND_RIGHT for property rights

## New Tables Created

The following tables are NOT in PPDM and have been created as new entity classes:

### General Ledger
- GL_ACCOUNT
- GL_ENTRY
- JOURNAL_ENTRY
- JOURNAL_ENTRY_LINE

### Invoice
- INVOICE
- INVOICE_LINE_ITEM
- INVOICE_PAYMENT

### Purchase Order
- PURCHASE_ORDER
- PO_LINE_ITEM
- PO_RECEIPT

### Accounts Payable
- AP_INVOICE
- AP_PAYMENT
- AP_CREDIT_MEMO

### Accounts Receivable
- AR_INVOICE
- AR_PAYMENT
- AR_CREDIT_MEMO

### Inventory
- INVENTORY_ITEM
- INVENTORY_TRANSACTION
- INVENTORY_ADJUSTMENT
- INVENTORY_VALUATION

### Revenue Accounting
- REVENUE_TRANSACTION
- SALES_CONTRACT (if not using CONTRACT)
- PRICE_INDEX
- REVENUE_ALLOCATION

### Cost Accounting
- COST_TRANSACTION
- COST_ALLOCATION
- AFE
- AFE_LINE_ITEM
- COST_CENTER

### Financial Accounting
- AMORTIZATION_RECORD
- IMPAIRMENT_RECORD
- CEILING_TEST_CALCULATION

## Best Practices

1. **Always check PPDM first** - Before creating a new table, check if PPDM has an equivalent
2. **Use junction tables** - For many-to-many relationships, use PPDM junction tables (e.g., APPLIC_AREA)
3. **Leverage BUSINESS_ASSOCIATE** - Use for all parties (vendors, customers, contractors, etc.)
4. **Use CONTRACT for all contracts** - Don't create separate contract tables
5. **Use OBLIGATION/OBLIG_PAYMENT** - For all payment obligations and payments

## Example: Creating a Vendor Invoice

```csharp
// 1. Get or create vendor (using BUSINESS_ASSOCIATE)
var vendorRepo = PPDMTableMapping.GetPPDMRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(BUSINESS_ASSOCIATE),
    PPDMTableMapping.TableNames.BusinessAssociate,
    connectionName);

// 2. Create AP invoice (new table)
var apInvoice = new AP_INVOICE
{
    AP_INVOICE_ID = Guid.NewGuid().ToString(),
    VENDOR_BA_ID = vendorId, // Reference to BUSINESS_ASSOCIATE
    INVOICE_NUMBER = "INV-001",
    // ... other fields
};

// 3. Create payment obligation (using OBLIGATION)
var obligationRepo = PPDMTableMapping.GetPPDMRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(OBLIGATION),
    PPDMTableMapping.TableNames.Obligation,
    connectionName);

var obligation = new OBLIGATION
{
    OBLIGATION_ID = Guid.NewGuid().ToString(),
    OBLIGATION_TYPE = "AP_INVOICE",
    OBLIGATION_AMOUNT = apInvoice.TOTAL_AMOUNT,
    // ... other fields
};
```

