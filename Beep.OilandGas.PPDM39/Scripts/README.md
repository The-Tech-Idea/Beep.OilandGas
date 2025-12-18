# PPDM39 Database Scripts

This directory contains database creation scripts for the PPDM39 data model in multiple database formats.

## Available Databases

- **Oracle** - Oracle Database scripts (original format)
- **Sqlserver** - SQL Server scripts with batch installation script
- **MySQL** - MySQL scripts (see MySQL/README.md for details on consolidated vs split structure)
- **PostgreSQL** - PostgreSQL-compatible scripts
- **MariaDB** - MariaDB/MySQL-compatible scripts (split file structure)
- **SQLite** - SQLite-compatible scripts (with limitations)

## Script Structure

Each database folder contains the following script files (where applicable):

- **TAB.sql** - Creates all tables with columns (mandatory)
- **PK.sql** - Creates primary key constraints (mandatory)
- **CK.sql** - Creates check constraints (mandatory)
- **FK.sql** - Creates foreign key constraints (mandatory)
- **OUOM.sql** - Creates original units of measure foreign keys (mandatory)
- **UOM.sql** - Creates units of measure foreign keys (mandatory)
- **RQUAL.sql** - Creates ROW_QUALITY foreign keys (mandatory)
- **RSRC.sql** - Creates SOURCE foreign keys (mandatory)
- **TCM.sql** - Creates table comments (optional but recommended)
- **CCM.sql** - Creates column comments (optional but recommended)
- **SYN.sql** - Creates synonyms (optional)
- **GUID.sql** - Creates unique constraints on PPDM_GUID (optional)
- **install.sh** / **install.bat** - Installation script for automated setup

## Converting Scripts

To convert SQL Server scripts to PostgreSQL, MariaDB, or SQLite formats, use the provided Python conversion script:

```bash
python convert_scripts.py
```

This script will:
1. Read all SQL files from the `Sqlserver` directory
2. Convert them to the appropriate syntax for each target database
3. Save the converted files in the respective database directories

**Note:** The conversion script performs basic syntax conversions. You should review the converted scripts, especially for SQLite, as some features may require manual adjustment due to database limitations.

## Installation

Each database folder contains a README.md with specific installation instructions for that database. Refer to the appropriate README for:
- Database-specific requirements
- Installation steps
- Known limitations or differences from other databases

## License and Terms

Use of this data is subject to the Terms and Conditions outlined in the PPDM_TermsAndConditions.txt file (if present in the Sqlserver directory).
