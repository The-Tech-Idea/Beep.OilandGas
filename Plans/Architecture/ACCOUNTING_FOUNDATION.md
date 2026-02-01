# Accounting System Foundation (Pre-Oil & Gas)

## CRITICAL REALIZATION
The ProductionAccounting module cannot exist without a solid FOUNDATIONAL accounting system.

Oil & Gas accounting is a SPECIALIZED LAYER on top of:
- General Ledger (GL)
- Accounts Receivable (AR)  
- Accounts Payable (AP)
- Purchasing
- Inventory
- Financial Reporting

---

## 1. GENERAL LEDGER (GL) - THE FOUNDATION

### Chart of Accounts (COA) Structure
```
1000-1999: ASSETS
  1000-1099: Cash & Equivalents
  1100-1199: Accounts Receivable
  1200-1299: Other Receivables
  1300-1399: Inventory
  1400-1499: Prepaid Expenses
  1500-1599: Property, Plant & Equipment (Oil & Gas)
  1600-1699: Accumulated Depreciation/Depletion
  1700-1799: Oil & Gas Leasehold
  1800-1899: Intangible Assets
  1900-1999: Other Assets

2000-2999: LIABILITIES
  2000-2099: Accounts Payable
  2100-2199: Accrued Expenses
  2200-2299: Accrued Royalties
  2300-2399: Other Payables
  2400-2499: Deferred Revenue
  2500-2599: Loan Payable - Current
  2600-2699: Other Current Liabilities
  2700-2799: Long-term Debt
  2800-2899: Deferred Tax Liability
  2900-2999: Other Long-term Liabilities

3000-3999: EQUITY
  3000-3099: Common Stock
  3100-3199: Retained Earnings
  3200-3299: Accumulated Other Comprehensive Income

4000-4999: REVENUE
  4000-4099: Oil Sales Revenue
  4100-4199: Natural Gas Revenue
  4200-4299: Other Oil & Gas Revenue
  4300-4399: Other Revenue

5000-5999: COST OF GOODS SOLD (COGS)
  5000-5099: Lease Operating Costs
  5100-5199: Depletion - Oil & Gas Properties
  5200-5299: Royalty Payments
  5300-5399: Production Taxes

6000-6999: OPERATING EXPENSES
  6000-6099: Salaries & Wages
  6100-6199: Office & Administrative
  6200-6299: Marketing & Sales
  6300-6399: Professional Fees
  6400-6499: Utilities & Rent
  6500-6599: Depreciation - Equipment
  6600-6699: Amortization - Intangibles
  6700-6799: Interest Expense
  6800-6899: Tax Expense
  6900-6999: Other Operating Expenses

7000-7999: OTHER INCOME/EXPENSE
  7000-7099: Gain/Loss on Asset Sales
  7100-7199: Investment Income
  7200-7299: Foreign Exchange Gains/Losses
  7300-7399: Other Non-operating Items
```

### GL Account Master (Table: GL_ACCOUNT_MASTER)
```csharp
public class GLAccountMaster
{
    public string ACCOUNT_NUMBER { get; set; }           // e.g., "4001"
    public string ACCOUNT_NAME { get; set; }            // "Crude Oil Sales"
    public string ACCOUNT_TYPE { get; set; }            // "ASSET", "LIABILITY", "EQUITY", "REVENUE", "EXPENSE"
    public string NORMAL_BALANCE { get; set; }          // "DEBIT" or "CREDIT"
    public bool IS_ACTIVE { get; set; }
    public string DESCRIPTION { get; set; }
    public string COST_CENTER { get; set; }             // For cost allocation
    public string DEPARTMENT { get; set; }
    public decimal OPENING_BALANCE { get; set; }        // As of fiscal year start
    public DateTime CREATED_DATE { get; set; }
    public string CREATED_BY { get; set; }
}
```

### GL Entry (Table: GL_ENTRY)
```csharp
public class GLEntry
{
    public string GL_ENTRY_ID { get; set; }
    public string JOURNAL_ENTRY_ID { get; set; }        // FK to JOURNAL_ENTRY
    public string ACCOUNT_NUMBER { get; set; }          // FK to GL_ACCOUNT_MASTER
    public decimal DEBIT_AMOUNT { get; set; }           // If positive, this is debit
    public decimal CREDIT_AMOUNT { get; set; }          // If positive, this is credit
    public string DESCRIPTION { get; set; }
    public DateTime ENTRY_DATE { get; set; }
    public string POSTED_BY { get; set; }
    public string REFERENCE_ID { get; set; }            // Link to source (Invoice, PO, etc.)
    public string REFERENCE_TYPE { get; set; }          // "INVOICE", "PO", "PAYMENT", etc.
}
```

### Journal Entry (Table: JOURNAL_ENTRY)
```csharp
public class JournalEntry
{
    public string JOURNAL_ENTRY_ID { get; set; }
    public string ENTRY_NUMBER { get; set; }            // Sequential number
    public DateTime ENTRY_DATE { get; set; }
    public string DESCRIPTION { get; set; }
    public decimal TOTAL_DEBIT { get; set; }
    public decimal TOTAL_CREDIT { get; set; }
    public string STATUS { get; set; }                  // "DRAFT", "POSTED", "REVERSED"
    public string POSTED_BY { get; set; }
    public DateTime? POSTED_DATE { get; set; }
    public string REVERSING_ENTRY_ID { get; set; }      // If reversed, link to reversal
    public string NOTES { get; set; }
    
    // GL Entries (1 to Many)
    public List<GLEntry> GLEntries { get; set; }
}
```

### GL Posting Rules
```
RULE 1: Balance Equation
  Assets = Liabilities + Equity
  
RULE 2: Debit/Credit
  - Increase in Asset → Debit
  - Decrease in Asset → Credit
  - Increase in Liability → Credit
  - Decrease in Liability → Debit
  - Increase in Equity → Credit
  - Decrease in Equity → Debit
  - Revenue → Credit (increases equity)
  - Expense → Debit (decreases equity)
  
RULE 3: Entry Validation
  ∑(Debits) = ∑(Credits)  (within 0.01 tolerance)

RULE 4: Posting
  - Only POSTED journals affect GL balances
  - DRAFT journals do not affect balances
  - REVERSED entries are cleared with reversal entry
```

---

## 2. ACCOUNTS RECEIVABLE (AR) - INVOICING & COLLECTIONS

### Invoice (Table: INVOICE)
```csharp
public class Invoice
{
    public string INVOICE_ID { get; set; }
    public string INVOICE_NUMBER { get; set; }
    public string CUSTOMER_ID { get; set; }             // FK to CUSTOMER
    public string SALES_ORDER_ID { get; set; }          // FK to SALES_ORDER
    public DateTime INVOICE_DATE { get; set; }
    public DateTime DUE_DATE { get; set; }
    public string STATUS { get; set; }                  // "DRAFT", "ISSUED", "PAID", "OVERDUE", "WRITTEN_OFF"
    
    public decimal SUBTOTAL { get; set; }
    public decimal TAX_AMOUNT { get; set; }
    public decimal DISCOUNT_AMOUNT { get; set; }
    public decimal FREIGHT_AMOUNT { get; set; }
    public decimal TOTAL_AMOUNT { get; set; }
    
    public decimal AMOUNT_PAID { get; set; }
    public decimal AMOUNT_DUE { get; set; }
    
    public string TERMS { get; set; }                   // "NET30", "NET60", etc.
    public string CURRENCY_CODE { get; set; }
    public string NOTES { get; set; }
}
```

### Invoice Line Item (Table: INVOICE_LINE_ITEM)
```csharp
public class InvoiceLineItem
{
    public string LINE_ITEM_ID { get; set; }
    public string INVOICE_ID { get; set; }              // FK
    public int LINE_NUMBER { get; set; }
    public string PRODUCT_ID { get; set; }              // FK to PRODUCT
    public string DESCRIPTION { get; set; }
    public decimal QUANTITY { get; set; }
    public string UNIT_OF_MEASURE { get; set; }
    public decimal UNIT_PRICE { get; set; }
    public decimal EXTENDED_PRICE { get; set; }         // Qty × UnitPrice
    public decimal TAX_RATE { get; set; }
    public decimal TAX_AMOUNT { get; set; }
    public decimal LINE_TOTAL { get; set; }
    public string GL_ACCOUNT_NUMBER { get; set; }       // For revenue GL posting
}
```

### GL Posting from Invoice
```
When Invoice Status changes to "ISSUED":
  Debit:  1110 - Accounts Receivable              $X,XXX.XX
    Credit: 4001 - Oil Sales Revenue                          $X,XXX.XX
  
When Payment Received:
  Debit:  1000 - Cash                             $X,XXX.XX
    Credit: 1110 - Accounts Receivable                        $X,XXX.XX
```

### Aging Report
```
Current (0-30 days):     $100,000
31-60 days overdue:      $ 50,000
61-90 days overdue:      $ 25,000
Over 90 days overdue:    $ 10,000
                         ---------
Total AR:                $185,000
```

---

## 3. ACCOUNTS PAYABLE (AP) - VENDOR BILLS & PAYMENTS

### Vendor Bill (Table: VENDOR_BILL)
```csharp
public class VendorBill
{
    public string BILL_ID { get; set; }
    public string BILL_NUMBER { get; set; }
    public string VENDOR_ID { get; set; }               // FK to VENDOR
    public string PO_ID { get; set; }                   // FK to PURCHASE_ORDER
    public DateTime BILL_DATE { get; set; }
    public DateTime DUE_DATE { get; set; }
    public string STATUS { get; set; } 
