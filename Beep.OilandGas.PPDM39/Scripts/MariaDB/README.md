# MariaDB Scripts for PPDM39

This directory contains MariaDB-compatible SQL scripts for the PPDM39 database model.

## Script Files

- **TAB.sql** - Creates all tables with columns
- **PK.sql** - Creates primary key constraints
- **FK.sql** - Creates foreign key constraints
- **CK.sql** - Creates check constraints
- **OUOM.sql** - Creates original units of measure foreign keys
- **UOM.sql** - Creates units of measure foreign keys
- **RQUAL.sql** - Creates ROW_QUALITY foreign keys
- **RSRC.sql** - Creates SOURCE foreign keys
- **TCM.sql** - Creates table comments (optional)
- **CCM.sql** - Creates column comments (optional)
- **SYN.sql** - Creates synonyms (optional)
- **GUID.sql** - Creates unique constraints on PPDM_GUID (optional)

## Installation

1. Create a MariaDB database:
   ```sql
   CREATE DATABASE ppdm39;
   ```

2. Use the database:
   ```sql
   USE ppdm39;
   ```

3. Run the scripts in order:
   ```bash
   mysql -u root -p ppdm39 < TAB.sql
   mysql -u root -p ppdm39 < PK.sql
   mysql -u root -p ppdm39 < CK.sql
   mysql -u root -p ppdm39 < FK.sql
   mysql -u root -p ppdm39 < OUOM.sql
   mysql -u root -p ppdm39 < UOM.sql
   mysql -u root -p ppdm39 < RQUAL.sql
   mysql -u root -p ppdm39 < RSRC.sql
   mysql -u root -p ppdm39 < TCM.sql  # Optional
   mysql -u root -p ppdm39 < CCM.sql  # Optional
   mysql -u root -p ppdm39 < SYN.sql  # Optional
   mysql -u root -p ppdm39 < GUID.sql # Optional
   ```

## Differences from SQL Server

- Uses `VARCHAR` instead of `NVARCHAR`
- Uses `DECIMAL` instead of `NUMERIC`
- Uses `DATE` or `DATETIME` types
- Uses backticks `` ` `` for identifiers instead of square brackets `[]`
- No `GO` statements (use `mysql` command-line tool to run scripts)

## Generating Scripts

To generate these scripts from the SQL Server versions, run:

```bash
python convert_scripts.py
```

from the parent Scripts directory.
