# Database Script Generation

## Overview

This document outlines patterns for generating database scripts (TAB, PK, FK) for all entity classes across 6 database types: SQL Server, SQLite, PostgreSQL, Oracle, MySQL, and MariaDB.

## Script Pattern

### SQL Server Pattern

**TAB.sql**: `CREATE TABLE` with `NVARCHAR`, `NUMERIC`, `DATE`, `DATETIME`

```sql
raiserror ('CREATING TABLE TABLE_NAME', 10,1) with nowait
CREATE TABLE TABLE_NAME
(
    TABLE_ID NVARCHAR(40) NOT NULL,
    COLUMN_NAME NVARCHAR(255),
    AMOUNT NUMERIC(18,2),
    TRANSACTION_DATE DATE,
    CREATED_DATE DATETIME,
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

**PK.sql**: `ALTER TABLE ADD CONSTRAINT PK_<TABLE> PRIMARY KEY (<ID_COLUMN>)`

```sql
ALTER TABLE TABLE_NAME
ADD CONSTRAINT PK_TABLE_NAME PRIMARY KEY (TABLE_ID);
```

**FK.sql**: `ALTER TABLE ADD CONSTRAINT FK_<TABLE>_<REF> FOREIGN KEY (<FK_COLUMN>) REFERENCES <REF_TABLE>(<REF_ID>)`

```sql
ALTER TABLE TABLE_NAME
ADD CONSTRAINT FK_TABLE_NAME_REF FOREIGN KEY (REF_ID) REFERENCES REF_TABLE(REF_TABLE_ID);
```

### SQLite Pattern

**TAB.sql**: `CREATE TABLE` with `TEXT`, `NUMERIC`, `DATE`, `DATETIME`

```sql
CREATE TABLE TABLE_NAME
(
    TABLE_ID TEXT NOT NULL,
    COLUMN_NAME TEXT,
    AMOUNT NUMERIC(18,2),
    TRANSACTION_DATE DATE,
    CREATED_DATE DATETIME,
    ACTIVE_IND TEXT,
    PPDM_GUID TEXT,
    REMARK TEXT,
    SOURCE TEXT,
    ROW_QUALITY TEXT,
    ROW_CREATED_BY TEXT,
    ROW_CREATED_DATE DATETIME,
    ROW_CHANGED_BY TEXT,
    ROW_CHANGED_DATE DATETIME,
    ROW_EFFECTIVE_DATE DATETIME,
    ROW_EXPIRY_DATE DATETIME,
    ROW_ID TEXT
);
```

**PK.sql**: Same as SQL Server  
**FK.sql**: Same as SQL Server

### PostgreSQL Pattern

**TAB.sql**: `CREATE TABLE` with `VARCHAR`, `NUMERIC`, `DATE`, `TIMESTAMP`

```sql
CREATE TABLE TABLE_NAME
(
    TABLE_ID VARCHAR(40) NOT NULL,
    COLUMN_NAME VARCHAR(255),
    AMOUNT NUMERIC(18,2),
    TRANSACTION_DATE DATE,
    CREATED_DATE TIMESTAMP,
    ACTIVE_IND VARCHAR(1),
    PPDM_GUID VARCHAR(40),
    REMARK VARCHAR(4000),
    SOURCE VARCHAR(40),
    ROW_QUALITY VARCHAR(40),
    ROW_CREATED_BY VARCHAR(40),
    ROW_CREATED_DATE TIMESTAMP,
    ROW_CHANGED_BY VARCHAR(40),
    ROW_CHANGED_DATE TIMESTAMP,
    ROW_EFFECTIVE_DATE TIMESTAMP,
    ROW_EXPIRY_DATE TIMESTAMP,
    ROW_ID VARCHAR(40)
);
```

**PK.sql**: Same as SQL Server  
**FK.sql**: Same as SQL Server

### Oracle Pattern

**TAB.sql**: `CREATE TABLE` with `VARCHAR2`, `NUMBER`, `DATE`

```sql
CREATE TABLE TABLE_NAME
(
    TABLE_ID VARCHAR2(40) NOT NULL,
    COLUMN_NAME VARCHAR2(255),
    AMOUNT NUMBER(18,2),
    TRANSACTION_DATE DATE,
    CREATED_DATE DATE,
    ACTIVE_IND VARCHAR2(1),
    PPDM_GUID VARCHAR2(40),
    REMARK VARCHAR2(4000),
    SOURCE VARCHAR2(40),
    ROW_QUALITY VARCHAR2(40),
    ROW_CREATED_BY VARCHAR2(40),
    ROW_CREATED_DATE DATE,
    ROW_CHANGED_BY VARCHAR2(40),
    ROW_CHANGED_DATE DATE,
    ROW_EFFECTIVE_DATE DATE,
    ROW_EXPIRY_DATE DATE,
    ROW_ID VARCHAR2(40)
);
```

**PK.sql**: Same as SQL Server  
**FK.sql**: Same as SQL Server

### MySQL Pattern

**TAB.sql**: `CREATE TABLE` with `VARCHAR`, `NUMERIC`, `DATE`, `DATETIME`

```sql
CREATE TABLE TABLE_NAME
(
    TABLE_ID VARCHAR(40) NOT NULL,
    COLUMN_NAME VARCHAR(255),
    AMOUNT NUMERIC(18,2),
    TRANSACTION_DATE DATE,
    CREATED_DATE DATETIME,
    ACTIVE_IND VARCHAR(1),
    PPDM_GUID VARCHAR(40),
    REMARK VARCHAR(4000),
    SOURCE VARCHAR(40),
    ROW_QUALITY VARCHAR(40),
    ROW_CREATED_BY VARCHAR(40),
    ROW_CREATED_DATE DATETIME,
    ROW_CHANGED_BY VARCHAR(40),
    ROW_CHANGED_DATE DATETIME,
    ROW_EFFECTIVE_DATE DATETIME,
    ROW_EXPIRY_DATE DATETIME,
    ROW_ID VARCHAR(40)
);
```

**PK.sql**: Same as SQL Server  
**FK.sql**: Same as SQL Server

### MariaDB Pattern

**TAB.sql**: `CREATE TABLE` with `VARCHAR`, `DECIMAL`, `DATE`, `DATETIME`

```sql
CREATE TABLE TABLE_NAME
(
    TABLE_ID VARCHAR(40) NOT NULL,
    COLUMN_NAME VARCHAR(255),
    AMOUNT DECIMAL(18,2),
    TRANSACTION_DATE DATE,
    CREATED_DATE DATETIME,
    ACTIVE_IND VARCHAR(1),
    PPDM_GUID VARCHAR(40),
    REMARK VARCHAR(4000),
    SOURCE VARCHAR(40),
    ROW_QUALITY VARCHAR(40),
    ROW_CREATED_BY VARCHAR(40),
    ROW_CREATED_DATE DATETIME,
    ROW_CHANGED_BY VARCHAR(40),
    ROW_CHANGED_DATE DATETIME,
    ROW_EFFECTIVE_DATE DATETIME,
    ROW_EXPIRY_DATE DATETIME,
    ROW_ID VARCHAR(40)
);
```

**PK.sql**: Same as SQL Server  
**FK.sql**: Same as SQL Server

## Standard PPDM Columns

All tables must include these standard columns:

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

## Data Type Mappings

### String Types
- **SQL Server**: `NVARCHAR(40/255/4000)`
- **SQLite**: `TEXT`
- **PostgreSQL**: `VARCHAR(40/255/4000)`
- **Oracle**: `VARCHAR2(40/255/4000)`
- **MySQL**: `VARCHAR(40/255/4000)`
- **MariaDB**: `VARCHAR(40/255/4000)`

### Decimal Types
- **SQL Server**: `NUMERIC(18,2)`
- **SQLite**: `NUMERIC(18,2)`
- **PostgreSQL**: `NUMERIC(18,2)`
- **Oracle**: `NUMBER(18,2)`
- **MySQL**: `NUMERIC(18,2)`
- **MariaDB**: `DECIMAL(18,2)`

### Date/Time Types
- **SQL Server**: `DATE`, `DATETIME`
- **SQLite**: `DATE`, `DATETIME`
- **PostgreSQL**: `DATE`, `TIMESTAMP`
- **Oracle**: `DATE`
- **MySQL**: `DATE`, `DATETIME`
- **MariaDB**: `DATE`, `DATETIME`

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

## Script File Organization

Place scripts in appropriate directories:

- `Beep.OilandGas.PPDM39/Scripts/Sqlserver/`
- `Beep.OilandGas.PPDM39/Scripts/SQLite/`
- `Beep.OilandGas.PPDM39/Scripts/PostgreSQL/`
- `Beep.OilandGas.PPDM39/Scripts/Oracle/`
- `Beep.OilandGas.PPDM39/Scripts/MySQL/`
- `Beep.OilandGas.PPDM39/Scripts/MariaDB/`

For each entity class, create 3 script files per database type:
- `<TABLE>_TAB.sql` - Table creation
- `<TABLE>_PK.sql` - Primary key constraint
- `<TABLE>_FK.sql` - Foreign key constraints

## Foreign Key References

Common foreign key patterns:

- Reference `BUSINESS_ASSOCIATE` for `BA_ID` columns
- Reference `PROPERTY` for `PROPERTY_ID` columns
- Reference `WELL` for `WELL_ID` columns
- Reference `FIELD` for `FIELD_ID` columns
- Reference parent tables for foreign keys

## Generation Instructions

1. For each entity class, create 3 script files per database type
2. Follow the data type mappings above
3. Include all standard PPDM columns in every table
4. Add foreign key constraints for relationships
5. Use appropriate column sizes based on the entity class properties

## Key Principles

1. **Consistency**: All scripts follow the same pattern across database types
2. **Standard Columns**: All tables include standard PPDM audit columns
3. **Data Type Mapping**: Only data type syntax differs between database systems
4. **Foreign Keys**: Reference existing PPDM tables where applicable
5. **Column Sizes**: Use appropriate column sizes based on entity properties

## References

- See `Beep.OilandGas.ProductionAccounting/DATABASE_SCRIPTS_GENERATION_GUIDE.md` for complete guide
- See PPDM documentation for table structures
- See entity classes for property types and sizes

