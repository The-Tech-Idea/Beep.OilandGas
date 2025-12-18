# SQLite Scripts for PPDM39

This directory contains SQLite-compatible SQL scripts for the PPDM39 database model.

## Important Notes

SQLite has limitations compared to other databases:
- **No ALTER TABLE ADD CONSTRAINT** support for adding constraints after table creation
- **No DATE type** - uses TEXT, INTEGER, or REAL
- **Limited foreign key support** - must be enabled with `PRAGMA foreign_keys = ON`
- **No CHECK constraints** added via ALTER TABLE
- Some advanced features may not be supported

**Warning:** These scripts may require significant manual adjustments due to SQLite's limitations. Consider reviewing each constraint and potentially combining creation statements.

## Script Files

- **TAB.sql** - Creates all tables with columns (may include inline constraints)
- **PK.sql** - Creates primary key constraints (may need to be inline with tables)
- **FK.sql** - Creates foreign key constraints (if supported)
- **CK.sql** - Creates check constraints (may need to be inline with tables)
- **OUOM.sql** - Creates original units of measure foreign keys
- **UOM.sql** - Creates units of measure foreign keys
- **RQUAL.sql** - Creates ROW_QUALITY foreign keys
- **RSRC.sql** - Creates SOURCE foreign keys
- **TCM.sql** - Not applicable (SQLite doesn't support table comments)
- **CCM.sql** - Not applicable (SQLite doesn't support column comments)
- **SYN.sql** - Not applicable (SQLite doesn't support synonyms)
- **GUID.sql** - Creates unique constraints on PPDM_GUID

## Installation

1. Create/open SQLite database:
   ```bash
   sqlite3 ppdm39.db
   ```

2. Enable foreign keys:
   ```sql
   PRAGMA foreign_keys = ON;
   ```

3. Run the scripts in order:
   ```bash
   sqlite3 ppdm39.db < TAB.sql
   sqlite3 ppdm39.db < PK.sql
   sqlite3 ppdm39.db < CK.sql
   sqlite3 ppdm39.db < FK.sql
   sqlite3 ppdm39.db < OUOM.sql
   sqlite3 ppdm39.db < UOM.sql
   sqlite3 ppdm39.db < RQUAL.sql
   sqlite3 ppdm39.db < RSRC.sql
   sqlite3 ppdm39.db < GUID.sql
   ```

## Differences from SQL Server

- Uses `TEXT` instead of `NVARCHAR`
- Uses `NUMERIC` or `INTEGER` for numbers
- Uses `TEXT` for dates (stored as ISO 8601 strings: "YYYY-MM-DD")
- No `GO` statements
- Limited constraint support - many constraints must be defined during table creation
- No stored procedures, triggers, or advanced features

## Generating Scripts

To generate these scripts from the SQL Server versions, run:

```bash
python convert_scripts.py
```

from the parent Scripts directory.

**Note:** You may need to manually review and adjust these scripts due to SQLite's limitations.
