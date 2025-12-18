# PostgreSQL Scripts for PPDM39

This directory contains PostgreSQL-compatible SQL scripts for the PPDM39 database model.

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

1. Create a PostgreSQL database:
   ```sql
   CREATE DATABASE ppdm39;
   ```

2. Connect to the database:
   ```sql
   \c ppdm39
   ```

3. Run the scripts in order:
   ```bash
   psql -d ppdm39 -f TAB.sql
   psql -d ppdm39 -f PK.sql
   psql -d ppdm39 -f CK.sql
   psql -d ppdm39 -f FK.sql
   psql -d ppdm39 -f OUOM.sql
   psql -d ppdm39 -f UOM.sql
   psql -d ppdm39 -f RQUAL.sql
   psql -d ppdm39 -f RSRC.sql
   psql -d ppdm39 -f TCM.sql  # Optional
   psql -d ppdm39 -f CCM.sql  # Optional
   psql -d ppdm39 -f SYN.sql  # Optional
   psql -d ppdm39 -f GUID.sql # Optional
   ```

## Differences from SQL Server

- Uses `VARCHAR` instead of `NVARCHAR`
- Uses `NUMERIC` (same as SQL Server)
- Uses `DATE` type
- Uses `COMMENT ON` for comments instead of `sp_addextendedproperty`
- Uses double quotes `"` for identifiers instead of square brackets `[]`
- No `GO` statements (use `psql` to run scripts)

## Generating Scripts

To generate these scripts from the SQL Server versions, run:

```bash
python convert_scripts.py
```

from the parent Scripts directory.
