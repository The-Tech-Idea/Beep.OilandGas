# Database Scripts Generation Guide

## Overview
This document provides the pattern for generating database scripts (TAB, PK, FK) for all accounting entity classes across all 6 database types: SQL Server, SQLite, PostgreSQL, Oracle, MySQL, and MariaDB.

## Script Pattern

### SQL Server Pattern
- **TAB.sql**: `CREATE TABLE` with `NVARCHAR`, `NUMERIC`, `DATE`, `DATETIME`
- **PK.sql**: `ALTER TABLE ADD CONSTRAINT PK_<TABLE> PRIMARY KEY (<ID_COLUMN>)`
- **FK.sql**: `ALTER TABLE ADD CONSTRAINT FK_<TABLE>_<REF> FOREIGN KEY (<FK_COLUMN>) REFERENCES <REF_TABLE>(<REF_ID>)`

### SQLite Pattern
- **TAB.sql**: `CREATE TABLE` with `TEXT`, `NUMERIC`, `DATE`, `DATETIME`
- **PK.sql**: Same as SQL Server
- **FK.sql**: Same as SQL Server

### PostgreSQL Pattern
- **TAB.sql**: `CREATE TABLE` with `VARCHAR`, `NUMERIC`, `DATE`, `TIMESTAMP`
- **PK.sql**: Same as SQL Server
- **FK.sql**: Same as SQL Server

### Oracle Pattern
- **TAB.sql**: `CREATE TABLE` with `VARCHAR2`, `NUMBER`, `DATE`
- **PK.sql**: Same as SQL Server
- **FK.sql**: Same as SQL Server

### MySQL Pattern
- **TAB.sql**: `CREATE TABLE` with `VARCHAR`, `NUMERIC`, `DATE`, `DATETIME`
- **PK.sql**: Same as SQL Server
- **FK.sql**: Same as SQL Server

### MariaDB Pattern
- **TAB.sql**: `CREATE TABLE` with `VARCHAR`, `DECIMAL`, `DATE`, `DATETIME`
- **PK.sql**: Same as SQL Server
- **FK.sql**: Same as SQL Server

## Standard PPDM Columns
All tables include these standard columns:
- `ACTIVE_IND` (String)
- `PPDM_GUID` (String)
- `REMARK` (String)
- `SOURCE` (String)
- `ROW_QUALITY` (String)
- `ROW_CREATED_BY` (String)
- `ROW_CREATED_DATE` (DateTime)
- `ROW_CHANGED_BY` (String)
- `ROW_CHANGED_DATE` (DateTime)
- `ROW_EFFECTIVE_DATE` (DateTime)
- `ROW_EXPIRY_DATE` (DateTime)
- `ROW_ID` (String)

## Entity Classes Requiring Scripts

### Revenue Accounting
- ✅ REVENUE_TRANSACTION (already has entity class)
- ✅ SALES_CONTRACT
- ✅ PRICE_INDEX
- ✅ REVENUE_ALLOCATION

### Cost Accounting
- ✅ COST_TRANSACTION (already has entity class)
- ✅ COST_ALLOCATION
- ✅ AFE (already has entity class)
- ✅ AFE_LINE_ITEM

### Financial Accounting
- ✅ AMORTIZATION_RECORD (already has entity class)
- ✅ IMPAIRMENT_RECORD (already has entity class)
- ✅ CEILING_TEST_CALCULATION

### Traditional Accounting
- ✅ GL_ACCOUNT (scripts created)
- ✅ GL_ENTRY (entity class exists)
- ✅ JOURNAL_ENTRY (entity class exists)
- ✅ JOURNAL_ENTRY_LINE (entity class exists)
- ✅ INVOICE (scripts created)
- ✅ INVOICE_LINE_ITEM (entity class exists)
- ✅ INVOICE_PAYMENT
- ✅ PURCHASE_ORDER (scripts created)
- ✅ PO_LINE_ITEM (entity class exists)
- ✅ PO_RECEIPT
- ✅ AP_INVOICE (entity class exists)
- ✅ AP_PAYMENT (entity class exists)
- ✅ AP_CREDIT_MEMO
- ✅ AR_INVOICE (entity class exists)
- ✅ AR_PAYMENT (entity class exists)
- ✅ AR_CREDIT_MEMO
- ✅ INVENTORY_ITEM (entity class exists)
- ✅ INVENTORY_TRANSACTION (entity class exists)
- ✅ INVENTORY_ADJUSTMENT
- ✅ INVENTORY_VALUATION

### Joint Venture
- ✅ JOINT_OPERATING_AGREEMENT (entity class exists)
- ✅ JOA_INTEREST
- ✅ JOINT_INTEREST_BILL (entity class exists)
- ✅ JOIB_LINE_ITEM
- ✅ JOIB_ALLOCATION

### Royalty
- ✅ ROYALTY_INTEREST (entity class exists)
- ✅ ROYALTY_OWNER
- ✅ ROYALTY_PAYMENT
- ✅ ROYALTY_PAYMENT_DETAIL

### Tax
- ✅ TAX_TRANSACTION (entity class exists)
- ✅ TAX_RETURN

## Script Generation Status

### Completed Scripts
- GL_ACCOUNT (all 6 database types)
- INVOICE (SQL Server only - pattern established)
- PURCHASE_ORDER (SQL Server only - pattern established)

### Remaining Scripts
All other entity classes listed above need scripts generated following the established pattern.

## Generation Instructions

1. For each entity class, create 3 script files per database type:
   - `<TABLE>_TAB.sql` - Table creation
   - `<TABLE>_PK.sql` - Primary key constraint
   - `<TABLE>_FK.sql` - Foreign key constraints

2. Place scripts in appropriate directories:
   - `Beep.OilandGas.PPDM39/Scripts/Sqlserver/`
   - `Beep.OilandGas.PPDM39/Scripts/SQLite/`
   - `Beep.OilandGas.PPDM39/Scripts/PostgreSQL/`
   - `Beep.OilandGas.PPDM39/Scripts/Oracle/`
   - `Beep.OilandGas.PPDM39/Scripts/MySQL/`
   - `Beep.OilandGas.PPDM39/Scripts/MariaDB/`

3. Follow the data type mappings:
   - String → NVARCHAR(40/255/4000) / TEXT / VARCHAR / VARCHAR2
   - Decimal? → NUMERIC(18,2) / NUMERIC / NUMBER(18,2) / DECIMAL(18,2)
   - DateTime? → DATETIME / DATE / TIMESTAMP

4. Include all standard PPDM columns in every table.

5. Add foreign key constraints for relationships:
   - Reference BUSINESS_ASSOCIATE for BA_ID columns
   - Reference PROPERTY for PROPERTY_ID columns
   - Reference WELL for WELL_ID columns
   - Reference parent tables for foreign keys

## Example: SALES_CONTRACT Scripts

### SQL Server - SALES_CONTRACT_TAB.sql
```sql
raiserror ('CREATING TABLE SALES_CONTRACT', 10,1) with nowait
CREATE TABLE SALES_CONTRACT
(
    SALES_CONTRACT_ID NVARCHAR(40) NOT NULL,
    CONTRACT_NUMBER NVARCHAR(40),
    BUYER_BA_ID NVARCHAR(40),
    SELLER_BA_ID NVARCHAR(40),
    EFFECTIVE_DATE DATE,
    EXPIRY_DATE DATE,
    COMMODITY_TYPE NVARCHAR(40),
    BASE_PRICE NUMERIC(18,2),
    PRICING_METHOD NVARCHAR(40),
    CURRENCY_CODE NVARCHAR(10),
    DESCRIPTION NVARCHAR(4000),
    ACTIVE_IND NVARCHAR(1),
    PPDM_GUID NVARCHAR(40),
    REMARK NVARCHAR(4000),
    SOURCE NVARCHAR(40),
    ROW_QUALITY NVARCHAR(40),
    ROW_CREATED_BY NVARCHAR(40),
    ROW_CREATED_DATE DATETIME,
    ROW_CHANGED_BY NVARCHAR(40),
    ROW_CHANGED_DATE DATETIME,
    ROW_EFFECTIVE_DATE DATETIME,
    ROW_EXPIRY_DATE DATETIME,
    ROW_ID NVARCHAR(40)
);
```

### SQL Server - SALES_CONTRACT_PK.sql
```sql
ALTER TABLE SALES_CONTRACT
ADD CONSTRAINT PK_SALES_CONTRACT PRIMARY KEY (SALES_CONTRACT_ID);
```

### SQL Server - SALES_CONTRACT_FK.sql
```sql
ALTER TABLE SALES_CONTRACT
ADD CONSTRAINT FK_SALES_CONTRACT_BUYER FOREIGN KEY (BUYER_BA_ID) REFERENCES BUSINESS_ASSOCIATE(BUSINESS_ASSOCIATE_ID);
ALTER TABLE SALES_CONTRACT
ADD CONSTRAINT FK_SALES_CONTRACT_SELLER FOREIGN KEY (SELLER_BA_ID) REFERENCES BUSINESS_ASSOCIATE(BUSINESS_ASSOCIATE_ID);
```

## Notes
- All scripts follow the same pattern across database types
- Only data type syntax differs between database systems
- Foreign key constraints should reference existing PPDM tables where applicable
- Use appropriate column sizes based on the entity class properties

