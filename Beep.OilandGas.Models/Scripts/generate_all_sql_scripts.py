#!/usr/bin/env python3
"""
Script to generate SQL scripts for all Entity classes in Beep.OilandGas.Models/Data
This script parses C# Entity classes and generates TAB, PK, FK scripts for all 6 database types.
"""

import os
import re
from pathlib import Path
from typing import Dict, List, Tuple, Optional

# Database type configurations
DB_TYPES = {
    'Sqlserver': {
        'string': 'NVARCHAR',
        'datetime': 'DATETIME',
        'decimal': 'NUMERIC(18,2)',
        'decimal6': 'NUMERIC(18,6)',
        'int': 'INT',
        'bigint': 'BIGINT',
        'bool': 'BIT',
        'guid': 'NVARCHAR(38)',
        'quote_open': '',
        'quote_close': '',
        'header': "raiserror ('CREATING TABLE {table}', 10,1) with nowait\n"
    },
    'SQLite': {
        'string': 'TEXT',
        'datetime': 'DATETIME',
        'decimal': 'REAL',
        'decimal6': 'REAL',
        'int': 'INTEGER',
        'bigint': 'INTEGER',
        'bool': 'INTEGER',
        'guid': 'TEXT',
        'quote_open': '',
        'quote_close': '',
        'header': '',
        'no_length_for_text': True  # SQLite TEXT doesn't use length
    },
    'PostgreSQL': {
        'string': 'VARCHAR',
        'datetime': 'TIMESTAMP',
        'decimal': 'NUMERIC(18,2)',
        'decimal6': 'NUMERIC(18,6)',
        'int': 'INTEGER',
        'bigint': 'BIGINT',
        'bool': 'BOOLEAN',
        'guid': 'VARCHAR(38)',
        'quote_open': '"',
        'quote_close': '"',
        'header': ''
    },
    'Oracle': {
        'string': 'VARCHAR2',
        'datetime': 'DATE',
        'decimal': 'NUMBER(18,2)',
        'decimal6': 'NUMBER(18,6)',
        'int': 'NUMBER(10)',
        'bigint': 'NUMBER(19)',
        'bool': 'NUMBER(1)',
        'guid': 'VARCHAR2(38)',
        'quote_open': '"',
        'quote_close': '"',
        'header': ''
    },
    'MySQL': {
        'string': 'VARCHAR',
        'datetime': 'DATETIME',
        'decimal': 'DECIMAL(18,2)',
        'decimal6': 'DECIMAL(18,6)',
        'int': 'INT',
        'bigint': 'BIGINT',
        'bool': 'TINYINT(1)',
        'guid': 'VARCHAR(38)',
        'quote_open': '`',
        'quote_close': '`',
        'header': ''
    },
    'MariaDB': {
        'string': 'VARCHAR',
        'datetime': 'DATETIME',
        'decimal': 'DECIMAL(18,2)',
        'decimal6': 'DECIMAL(18,6)',
        'int': 'INT',
        'bigint': 'BIGINT',
        'bool': 'TINYINT(1)',
        'guid': 'VARCHAR(38)',
        'quote_open': '`',
        'quote_close': '`',
        'header': ''
    }
}

def map_csharp_type_to_sql(csharp_type: str, db_type: str, column_name: str) -> str:
    """Map C# type to SQL type for specific database"""
    db_config = DB_TYPES[db_type]
    
    # Determine length for string types
    length = get_string_length(column_name)
    
    # Map types
    if 'String' in csharp_type or 'string' in csharp_type.lower():
        # SQLite TEXT doesn't use length specification
        if db_type == 'SQLite':
            return 'TEXT'
        if length:
            return f"{db_config['string']}({length})"
        return f"{db_config['string']}(255)"
    elif 'DateTime' in csharp_type:
        return db_config['datetime']
    elif 'Decimal' in csharp_type:
        # Check if it's a percentage or precision field
        if 'PERCENTAGE' in column_name or 'RATIO' in column_name:
            return db_config['decimal6']
        return db_config['decimal']
    elif 'Int' in csharp_type and '64' not in csharp_type:
        return db_config['int']
    elif 'Int64' in csharp_type or 'Long' in csharp_type:
        return db_config['bigint']
    elif 'Boolean' in csharp_type or 'bool' in csharp_type.lower():
        return db_config['bool']
    elif 'Guid' in csharp_type:
        return db_config['guid']
    
    # Default to string
    return f"{db_config['string']}(255)"

def get_string_length(column_name: str) -> Optional[int]:
    """Determine appropriate string length based on column name"""
    if column_name.endswith('_ID') or 'GUID' in column_name:
        return 40
    if 'GUID' in column_name:
        return 38
    if 'DESCRIPTION' in column_name or 'REMARK' in column_name or 'NOTES' in column_name:
        return 2000
    if 'NAME' in column_name:
        return 255
    if column_name == 'ACTIVE_IND':
        return 1
    if 'STATUS' in column_name or 'TYPE' in column_name:
        return 50
    if 'SOURCE' in column_name or 'QUALITY' in column_name:
        return 40
    if 'CREATED_BY' in column_name or 'CHANGED_BY' in column_name:
        return 30
    return None

def parse_entity_file(file_path: Path) -> Tuple[str, List[Dict]]:
    """Parse C# Entity file and extract table name and properties"""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Extract class name (table name)
    class_match = re.search(r'public\s+(partial\s+)?class\s+(\w+)\s*:\s*Entity', content)
    if not class_match:
        return None, []
    
    table_name = class_match.group(2)
    
    # Extract properties by finding public properties and their corresponding private fields
    properties = []
    
    # Find all public property declarations: public Type PropertyName
    # Look for the property declaration line, then find the corresponding private field
    prop_pattern = r'public\s+(System\.)?(\w+\??)\s+(\w+)\s*\{'
    
    for match in re.finditer(prop_pattern, content):
        prop_name = match.group(3)
        
        # Skip Value properties (internal fields)
        if prop_name.endswith('Value'):
            continue
        
        # Find the corresponding private field to determine type and nullability
        # Pattern: private System.Type? PropertyNameValue;
        # Try with System. prefix first, then without
        field_patterns = [
            rf'private\s+System\.(\w+\??)\s+{re.escape(prop_name)}Value\s*;',
            rf'private\s+(\w+\??)\s+{re.escape(prop_name)}Value\s*;'
        ]
        
        field_match = None
        for pattern in field_patterns:
            field_match = re.search(pattern, content)
            if field_match:
                break
        
        if field_match:
            field_type = field_match.group(1)
            prop_type = field_type.replace('?', '').replace('System.', '')
            
            # Determine nullability:
            # - String types are nullable (reference types in C#)
            # - Value types with ? are nullable
            # - Value types without ? are NOT nullable
            # - Primary key is never nullable
            is_pk = prop_name == f"{table_name}_ID"
            if is_pk:
                is_nullable = False
            elif prop_type in ['String', 'string']:
                is_nullable = True  # String is nullable
            else:
                is_nullable = '?' in field_type  # Value types: nullable if has ?
            
            properties.append({
                'name': prop_name,
                'type': prop_type,
                'nullable': is_nullable
            })
        else:
            # If we can't find the field, try to infer from property type
            prop_type = match.group(2).replace('?', '').replace('System.', '')
            is_pk = prop_name == f"{table_name}_ID"
            if is_pk:
                is_nullable = False
            elif prop_type in ['String', 'string']:
                is_nullable = True
            else:
                is_nullable = '?' in match.group(2)
            properties.append({
                'name': prop_name,
                'type': prop_type,
                'nullable': is_nullable
            })
    
    return table_name, properties

def generate_tab_script(table_name: str, properties: List[Dict], db_type: str) -> str:
    """Generate TAB.sql script"""
    db_config = DB_TYPES[db_type]
    lines = []
    
    # Header
    if db_config['header']:
        lines.append(db_config['header'].format(table=table_name))
    
    # CREATE TABLE
    quote_open = db_config['quote_open']
    quote_close = db_config['quote_close']
    lines.append(f"CREATE TABLE {quote_open}{table_name}{quote_close}")
    lines.append("(")
    
    # Columns
    for i, prop in enumerate(properties):
        prop_name = prop['name']
        prop_type = prop['type']
        is_nullable = prop['nullable']
        
        # Determine if NOT NULL
        # Primary key is always NOT NULL
        # String types are nullable (reference types in C#)
        # Value types without ? are NOT NULL, with ? are nullable
        is_pk = prop_name == f"{table_name}_ID"
        if is_pk:
            not_null = " NOT NULL"
        elif prop_type in ['String', 'string']:
            not_null = ""  # String is nullable in SQL (reference type in C#)
        elif not is_nullable and prop_type not in ['String', 'string']:
            # Non-nullable value types (Decimal, DateTime, Int, etc. without ?)
            not_null = " NOT NULL"
        else:
            not_null = ""  # Nullable value type (with ?)
        
        # Map type
        sql_type = map_csharp_type_to_sql(prop_type, db_type, prop_name)
        
        # Default value for ACTIVE_IND
        default = ""
        if prop_name == 'ACTIVE_IND' and db_type != 'SQLite':
            default = " DEFAULT 'Y'"
        
        comma = "," if i < len(properties) - 1 else ""
        lines.append(f"    {quote_open}{prop_name}{quote_close} {sql_type}{not_null}{default}{comma}")
    
    lines.append(");")
    lines.append("")
    
    return "\n".join(lines)

def generate_pk_script(table_name: str, db_type: str) -> str:
    """Generate PK.sql script"""
    db_config = DB_TYPES[db_type]
    quote_open = db_config['quote_open']
    quote_close = db_config['quote_close']
    
    if db_type == 'SQLite':
        return "-- SQLite doesn't support ALTER TABLE ADD CONSTRAINT for primary keys\n-- Primary key is defined in CREATE TABLE statement\n-- This file is kept for consistency with other database types\n"
    
    lines = []
    lines.append(f"ALTER TABLE {quote_open}{table_name}{quote_close}")
    lines.append(f"ADD CONSTRAINT PK_{table_name} PRIMARY KEY ({quote_open}{table_name}_ID{quote_close});")
    lines.append("")
    
    return "\n".join(lines)

def generate_fk_script(table_name: str, properties: List[Dict], db_type: str) -> str:
    """Generate FK.sql script"""
    db_config = DB_TYPES[db_type]
    quote_open = db_config['quote_open']
    quote_close = db_config['quote_close']
    
    # Find foreign key properties (end with _ID but not the primary key)
    fk_properties = [p for p in properties 
                     if p['name'].endswith('_ID') 
                     and p['name'] != f"{table_name}_ID"
                     and p['name'] not in ['ROW_ID', 'PPDM_GUID']]
    
    if db_type == 'SQLite':
        return "-- SQLite foreign keys are enabled via PRAGMA foreign_keys = ON\n-- Foreign key constraints are defined in CREATE TABLE statement\n-- This file is kept for consistency with other database types\n"
    
    if not fk_properties:
        return f"-- {table_name} table has no foreign key constraints\n"
    
    lines = []
    for fk_prop in fk_properties:
        fk_name = fk_prop['name']
        # Infer referenced table from FK name (remove _ID suffix)
        ref_table = fk_name[:-3] if fk_name.endswith('_ID') else fk_name
        ref_column = f"{ref_table}_ID"
        
        lines.append(f"ALTER TABLE {quote_open}{table_name}{quote_close}")
        lines.append(f"ADD CONSTRAINT FK_{table_name}_{ref_table} FOREIGN KEY ({quote_open}{fk_name}{quote_close}) REFERENCES {quote_open}{ref_table}{quote_close}({quote_open}{ref_column}{quote_close});")
        lines.append("")
    
    return "\n".join(lines)

def get_entity_subfolder(entity_file: Path, data_dir: Path) -> Optional[str]:
    """Determine the subfolder for an entity based on its location in Data folder"""
    try:
        # Get relative path from Data directory
        relative_path = entity_file.relative_to(data_dir)
        
        # If entity is in a subfolder (not root), return the subfolder name
        if len(relative_path.parts) > 1:
            # First part is the subfolder name
            subfolder = relative_path.parts[0]
            # Capitalize first letter to match folder names
            return subfolder.capitalize()
        
        # Entity is in root Data folder
        return None
    except:
        return None

def main():
    """Main function to generate all SQL scripts"""
    # Paths
    script_dir = Path(__file__).parent
    data_dir = script_dir.parent / "Data"
    scripts_dir = script_dir
    
    if not data_dir.exists():
        print(f"Error: Data directory not found: {data_dir}")
        return
    
    # Get all Entity class files
    entity_files = list(data_dir.rglob("*.cs"))
    entity_files = [f for f in entity_files if f.name != "OwnershipTree.cs" 
                    and not f.name.startswith("Sales") 
                    and not f.name.startswith("Exchange")]
    
    print(f"Found {len(entity_files)} entity files")
    
    # Process each entity
    for entity_file in entity_files:
        table_name, properties = parse_entity_file(entity_file)
        if not table_name or not properties:
            print(f"Warning: Could not parse {entity_file.name}")
            continue
        
        # Determine subfolder for this entity
        subfolder = get_entity_subfolder(entity_file, data_dir)
        print(f"Processing: {table_name}" + (f" (in {subfolder}/)" if subfolder else " (root)"))
        
        # Generate scripts for each database type
        for db_type in DB_TYPES.keys():
            db_dir = scripts_dir / db_type
            
            # Create subfolder if entity belongs to one
            if subfolder:
                target_dir = db_dir / subfolder
                target_dir.mkdir(parents=True, exist_ok=True)
            else:
                target_dir = db_dir
                target_dir.mkdir(exist_ok=True)
            
            # Check if scripts already exist
            tab_file = target_dir / f"{table_name}_TAB.sql"
            pk_file = target_dir / f"{table_name}_PK.sql"
            fk_file = target_dir / f"{table_name}_FK.sql"
            
            # Generate TAB (always regenerate to ensure correct location)
            tab_script = generate_tab_script(table_name, properties, db_type)
            tab_file.write_text(tab_script, encoding='utf-8')
            
            # Generate PK
            pk_script = generate_pk_script(table_name, db_type)
            pk_file.write_text(pk_script, encoding='utf-8')
            
            # Generate FK
            fk_script = generate_fk_script(table_name, properties, db_type)
            fk_file.write_text(fk_script, encoding='utf-8')
    
    print("Script generation complete!")

if __name__ == "__main__":
    main()
