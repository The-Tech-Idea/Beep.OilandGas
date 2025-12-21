#!/usr/bin/env python3
"""
PPDM39 Oracle Script Converter
Converts SQL Server scripts to Oracle format
"""

import os
import re
import sys
from pathlib import Path

# Base directory
BASE_DIR = Path(__file__).parent
SQLSERVER_DIR = BASE_DIR / "Sqlserver"
ORACLE_DIR = BASE_DIR / "Oracle"

def convert_sqlserver_to_oracle(content):
    """Convert SQL Server syntax to Oracle syntax"""
    # Convert raiserror to PROMPT
    content = re.sub(
        r"raiserror\s*\(['\"](.*?)['\"].*?\)",
        lambda m: f"PROMPT {m.group(1)}",
        content,
        flags=re.IGNORECASE
    )
    
    # Convert NVARCHAR to VARCHAR2
    content = re.sub(r'\bNVARCHAR\b', 'VARCHAR2', content, flags=re.IGNORECASE)
    
    # Convert NUMERIC to NUMBER
    content = re.sub(r'\bNUMERIC\b', 'NUMBER', content, flags=re.IGNORECASE)
    
    # DATE stays DATE in Oracle
    
    # Remove GO statements
    content = re.sub(r'^\s*GO\s*$', '', content, flags=re.MULTILINE | re.IGNORECASE)
    
    return content

def process_file(source_file, target_dir):
    """Convert a single file from SQL Server to Oracle format"""
    target_file = target_dir / f"{source_file.stem}.SQL"
    
    print(f"Converting {source_file.name} -> {target_file.name} (Oracle)")
    
    try:
        with open(source_file, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
        
        converted_content = convert_sqlserver_to_oracle(content)
        
        # Add header comment
        header = f"""-- PPDM39 Oracle DDL Script
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
    
    # Create target directory
    ORACLE_DIR.mkdir(exist_ok=True)
    
    # Get all SQL files from SQL Server directory
    sql_files = list(SQLSERVER_DIR.glob("*.sql"))
    
    if not sql_files:
        print(f"No SQL files found in {SQLSERVER_DIR}")
        return 1
    
    # Filter to only new files (LEAD, PLAY, PROSPECT, etc.)
    prospect_files = [f for f in sql_files if any(prefix in f.name.upper() for prefix in 
        ['LEAD_', 'PLAY_', 'PROSPECT_', 'EXPLORATION_', 'R_PROSPECT', 'R_LEAD', 'R_PLAY', 
         'R_TRAP', 'R_DISCOVERY', 'R_WORKFLOW', 'R_ECONOMIC', 'R_RISK'])]
    
    if not prospect_files:
        print("No prospect-related files to convert")
        return 0
    
    print(f"Found {len(prospect_files)} prospect-related SQL files to convert\n")
    
    success = 0
    failed = 0
    
    for sql_file in prospect_files:
        if process_file(sql_file, ORACLE_DIR):
            success += 1
        else:
            failed += 1
    
    print(f"\nConversion Summary: {success} successful, {failed} failed")
    return 0

if __name__ == '__main__':
    sys.exit(main())

