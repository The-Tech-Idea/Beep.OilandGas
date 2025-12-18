#!/usr/bin/env python3
"""
PPDM39 Database Script Converter
Converts SQL Server scripts to PostgreSQL, MariaDB, and SQLite formats
"""

import os
import re
import sys
from pathlib import Path

# Base directory
BASE_DIR = Path(__file__).parent
SQLSERVER_DIR = BASE_DIR / "Sqlserver"
POSTGRESQL_DIR = BASE_DIR / "PostgreSQL"
MARIADB_DIR = BASE_DIR / "MariaDB"
SQLITE_DIR = BASE_DIR / "SQLite"

# Mapping dictionaries for different databases
TYPE_MAPPING = {
    'postgresql': {
        'NVARCHAR': 'VARCHAR',
        'NUMERIC': 'NUMERIC',
        'DATE': 'DATE',
        'raiserror': '--',
        'with nowait': ''
    },
    'mariadb': {
        'NVARCHAR': 'VARCHAR',
        'NUMERIC': 'DECIMAL',
        'DATE': 'DATE',
        'raiserror': '--',
        'with nowait': ''
    },
    'sqlite': {
        'NVARCHAR': 'TEXT',
        'NUMERIC': 'NUMERIC',
        'DATE': 'TEXT',  # SQLite doesn't have a native DATE type
        'raiserror': '--',
        'with nowait': ''
    }
}

def convert_sp_addextendedproperty(content, target_db):
    """Convert SQL Server sp_addextendedproperty to target database comment syntax"""
    
    def parse_sp_call(line):
        """Parse a sp_addextendedproperty call and extract description, table, and column"""
        # Pattern: execute sp_addextendedproperty 'Description','<desc>','USER','dbo','TABLE','<table>', 'COLUMN', '<column>';
        # Handle SQL Server escaped quotes ('') in the description
        
        # Find the description part (between 'Description', and the next ,'USER',)
        desc_match = re.search(r"'Description'\s*,\s*'(.+?)'\s*,\s*'USER'", line, re.IGNORECASE)
        if not desc_match:
            return None, None, None
        
        desc = desc_match.group(1)
        # Unescape SQL Server doubled quotes
        desc = desc.replace("''", "'")
        
        # Find table name
        table_match = re.search(r"'TABLE'\s*,\s*'(\w+)'", line, re.IGNORECASE)
        if not table_match:
            return None, None, None
        
        table = table_match.group(1)
        
        # Check if there's a COLUMN part
        column_match = re.search(r"'COLUMN'\s*,\s*'(\w+)'", line, re.IGNORECASE)
        column = column_match.group(1) if column_match else None
        
        return desc, table, column
    
    def convert_line(line):
        """Convert a single line containing sp_addextendedproperty"""
        if 'sp_addextendedproperty' not in line.lower():
            return line
        
        desc, table, column = parse_sp_call(line)
        if desc is None:
            return line  # Couldn't parse, return original
        
        # Escape single quotes for SQL (double them)
        desc_escaped = desc.replace("'", "''")
        
        if target_db == 'postgresql':
            if column:
                return f"COMMENT ON COLUMN {table}.{column} IS '{desc_escaped}';"
            else:
                return f"COMMENT ON TABLE {table} IS '{desc_escaped}';"
        
        elif target_db == 'mariadb':
            # MySQL/MariaDB column comments require full column definition, which we don't have
            # Table comments can be added, but it's easier to just comment these out
            # Users can manually add comments during table creation if needed
            if column:
                return f"-- COMMENT ON COLUMN {table}.{column}: {desc} (MySQL/MariaDB requires column definition to add comments)"
            else:
                # Escape for MySQL string
                desc_mysql = desc.replace("'", "\\'")
                return f"-- COMMENT ON TABLE {table}: {desc} (Can be added with: ALTER TABLE {table} COMMENT = '{desc_mysql}')"
        
        elif target_db == 'sqlite':
            # SQLite doesn't support comments - convert to SQL comments
            if column:
                return f"-- SQLite doesn't support column comments: {table}.{column}: {desc}"
            else:
                return f"-- SQLite doesn't support table comments: {table}: {desc}"
        
        return line
    
    # Process line by line
    lines = content.split('\n')
    converted_lines = []
    for line in lines:
        converted_lines.append(convert_line(line))
    
    return '\n'.join(converted_lines)

def convert_sqlserver_to_target(content, target_db):
    """Convert SQL Server syntax to target database syntax"""
    mapping = TYPE_MAPPING[target_db]
    
    # Convert sp_addextendedproperty first (before other conversions)
    content = convert_sp_addextendedproperty(content, target_db)
    
    # Convert data types
    for old_type, new_type in mapping.items():
        if old_type in ['NVARCHAR', 'NUMERIC', 'DATE']:
            # Use word boundaries to avoid partial matches
            content = re.sub(r'\b' + old_type + r'\b', new_type, content, flags=re.IGNORECASE)
        elif old_type == 'raiserror':
            # Convert raiserror statements to comments
            content = re.sub(
                r"raiserror\s*\(['\"](.*?)['\"].*?\)",
                lambda m: f"-- {m.group(1)}",
                content,
                flags=re.IGNORECASE
            )
        elif old_type == 'with nowait':
            content = re.sub(r'\s+with\s+nowait', '', content, flags=re.IGNORECASE)
    
    # PostgreSQL specific conversions
    if target_db == 'postgresql':
        # Remove GO statements
        content = re.sub(r'^\s*GO\s*$', '', content, flags=re.MULTILINE | re.IGNORECASE)
        # Convert square brackets to double quotes for identifiers
        content = re.sub(r'\[([^\]]+)\]', r'"\1"', content)
    
    # MariaDB specific conversions
    elif target_db == 'mariadb':
        # Remove GO statements
        content = re.sub(r'^\s*GO\s*$', '', content, flags=re.MULTILINE | re.IGNORECASE)
        # Convert square brackets (MariaDB doesn't support them)
        content = re.sub(r'\[([^\]]+)\]', r'`\1`', content)
        # Note: MariaDB supports DATE type, so we don't convert it to DATETIME
        # The original conversion was incorrect - DATE should remain DATE
    
    # SQLite specific conversions
    elif target_db == 'sqlite':
        # Remove GO statements
        content = re.sub(r'^\s*GO\s*$', '', content, flags=re.MULTILINE | re.IGNORECASE)
        # SQLite doesn't support ALTER TABLE ADD CONSTRAINT in all cases
        # This is a simplified conversion - complex constraints may need manual review
        pass
    
    return content

def process_file(source_file, target_dir, target_db, file_suffix=''):
    """Convert a single file from SQL Server to target database format"""
    target_file = target_dir / f"{source_file.stem}{file_suffix}.sql"
    
    print(f"Converting {source_file.name} -> {target_file.name} ({target_db})")
    
    try:
        with open(source_file, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
        
        converted_content = convert_sqlserver_to_target(content, target_db)
        
        # Add header comment
        header = f"""-- PPDM39 {target_db.upper()} DDL Script
-- Converted from SQL Server script: {source_file.name}
-- This script is for the PPDM39 model

"""
        
        with open(target_file, 'w', encoding='utf-8') as f:
            f.write(header + converted_content)
        
        return True
    except Exception as e:
        print(f"Error converting {source_file.name}: {e}")
        return False

def main():
    """Main conversion function"""
    if not SQLSERVER_DIR.exists():
        print(f"Error: SQL Server directory not found: {SQLSERVER_DIR}")
        return 1
    
    # Create target directories
    for target_dir in [POSTGRESQL_DIR, MARIADB_DIR, SQLITE_DIR]:
        target_dir.mkdir(exist_ok=True)
    
    # Get all SQL files from SQL Server directory
    sql_files = list(SQLSERVER_DIR.glob("*.sql"))
    
    if not sql_files:
        print(f"No SQL files found in {SQLSERVER_DIR}")
        return 1
    
    print(f"Found {len(sql_files)} SQL files to convert\n")
    
    # Process each file for each target database
    results = {db: {'success': 0, 'failed': 0} for db in ['postgresql', 'mariadb', 'sqlite']}
    
    for sql_file in sql_files:
        # Skip install scripts and other non-DDL files
        if sql_file.name.lower() in ['install.bat', 'changes.txt']:
            continue
        
        for target_db in ['postgresql', 'mariadb', 'sqlite']:
            target_dir_map = {
                'postgresql': POSTGRESQL_DIR,
                'mariadb': MARIADB_DIR,
                'sqlite': SQLITE_DIR
            }
            
            if process_file(sql_file, target_dir_map[target_db], target_db):
                results[target_db]['success'] += 1
            else:
                results[target_db]['failed'] += 1
    
    # Print summary
    print("\n" + "="*60)
    print("Conversion Summary")
    print("="*60)
    for db, stats in results.items():
        print(f"{db.upper()}: {stats['success']} successful, {stats['failed']} failed")
    
    print("\nNote: Please review the converted scripts as some database-specific")
    print("features may require manual adjustment (e.g., constraints, indexes).")
    
    return 0

if __name__ == '__main__':
    sys.exit(main())
