# MySQL Scripts for PPDM39

This directory is for MySQL-specific scripts for the PPDM39 database model.

## Note on Existing MySQL Script

There is currently a consolidated MySQL script at the root of the Scripts directory:
- **ppdm39mysql.sql** - A single file containing all table definitions, primary keys, foreign keys, and constraints

This script is structured differently from the SQL Server and Oracle versions, which are split into multiple files (TAB.sql, PK.sql, FK.sql, etc.).

## Options

### Option 1: Use the Consolidated Script
The existing `ppdm39mysql.sql` file can be used as-is:

```bash
mysql -u root -p ppdm39 < ../ppdm39mysql.sql
```

### Option 2: Use MariaDB Scripts
MySQL and MariaDB are highly compatible. You can use the scripts in the `MariaDB` directory, which follow the same structure as SQL Server and Oracle:

```bash
cd ../MariaDB
mysql -u root -p ppdm39 < TAB.sql
mysql -u root -p ppdm39 < PK.sql
# ... etc
```

### Option 3: Split the Consolidated Script
If you prefer to split `ppdm39mysql.sql` into separate files (TAB.sql, PK.sql, FK.sql, etc.) to match the structure of other databases, you can:

1. Use a script to parse and split the consolidated file
2. Or manually extract sections based on:
   - `CREATE TABLE` statements → TAB.sql
   - `PRIMARY KEY` definitions → PK.sql
   - `FOREIGN KEY` definitions → FK.sql
   - etc.

## Differences from Other Databases

- Uses `VARCHAR` instead of `NVARCHAR`
- Uses `DECIMAL` for numeric types
- Uses `datetime` for date/time values
- Table names are in UPPERCASE by default
- Constraints can be inline or added via ALTER TABLE

## Installation

If using the consolidated script:

```bash
mysql -u root -p -e "CREATE DATABASE ppdm39 CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
mysql -u root -p ppdm39 < ../ppdm39mysql.sql
```

If using MariaDB scripts (recommended for consistency):

See the MariaDB/README.md for installation instructions.
