#!/usr/bin/env python3
"""
Script to move SQL scripts from Scripts root to appropriate database type subfolders
"""

import shutil
from pathlib import Path

# Entity to project folder mapping
ENTITY_TO_FOLDER = {
    'CRUDE_OIL_INVENTORY': 'ProductionAccounting',
    'CRUDE_OIL_INVENTORY_TRANSACTION': 'ProductionAccounting',
    'PRODUCTION_COSTS': 'ProductionOperations',
    'PROVED_PROPERTY': 'Common',
    'UNPROVED_PROPERTY': 'Common',
    'DEVELOPMENT_COSTS': 'DevelopmentPlanning',
    'EXPLORATION_COSTS': 'ProspectIdentification',
    'RECEIVABLE': 'ProductionAccounting',
    'REGULATED_PRICE': 'Pricing',  # Pricing is a subfolder
}

def main():
    """Move SQL scripts from root to appropriate folders"""
    script_dir = Path(__file__).parent
    
    print("Moving SQL scripts from root to database type folders...")
    
    # Get all SQL files in root
    root_sql_files = [f for f in script_dir.iterdir() 
                     if f.is_file() and f.suffix == '.sql']
    
    if not root_sql_files:
        print("No SQL files found in root.")
        return
    
    print(f"Found {len(root_sql_files)} SQL files in root\n")
    
    # Database type folders
    db_folders = ['Sqlserver', 'SQLite', 'PostgreSQL', 'Oracle', 'MySQL', 'MariaDB']
    
    moved_count = 0
    skipped_count = 0
    
    for sql_file in root_sql_files:
        # Extract entity name (remove .sql extension)
        entity_name = sql_file.stem
        
        # Determine target folder
        target_folder = ENTITY_TO_FOLDER.get(entity_name)
        
        if not target_folder:
            print(f"  Skipping {sql_file.name} (not in mapping)")
            skipped_count += 1
            continue
        
        print(f"  Processing {sql_file.name} -> {target_folder}/")
        
        # Move to each database type folder
        for db_folder in db_folders:
            db_dir = script_dir / db_folder
            if not db_dir.exists():
                print(f"    Warning: {db_folder} folder does not exist")
                continue
            
            target_dir = db_dir / target_folder
            target_dir.mkdir(exist_ok=True)
            
            target_file = target_dir / sql_file.name
            
            if target_file.exists():
                print(f"    Skipping {db_folder}/{target_folder}/{sql_file.name} (already exists)")
                continue
            
            try:
                # Copy file to target directory
                shutil.copy2(str(sql_file), str(target_file))
                moved_count += 1
                print(f"    Copied to {db_folder}/{target_folder}/")
            except Exception as e:
                print(f"    Error copying to {db_folder}/{target_folder}/: {e}")
        
        # Remove original file after copying to all DB folders
        try:
            sql_file.unlink()
            print(f"  Removed original {sql_file.name}\n")
        except Exception as e:
            print(f"  Warning: Could not remove {sql_file.name}: {e}\n")
    
    print(f"Move complete!")
    print(f"  Files processed: {len(root_sql_files)}")
    print(f"  Files moved: {moved_count}")
    print(f"  Files skipped: {skipped_count}")

if __name__ == "__main__":
    main()
