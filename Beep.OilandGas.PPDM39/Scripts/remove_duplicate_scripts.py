#!/usr/bin/env python3
"""
Script to remove duplicate SQL scripts from PPDM39/Scripts that already exist in Models/Scripts
"""

import os
from pathlib import Path
from typing import Set, List

# Script suffixes to remove (both .sql and .SQL for Oracle)
SCRIPT_SUFFIXES = [
    '_TAB.sql', '_TAB.SQL',
    '_PK.sql', '_PK.SQL',
    '_FK.sql', '_FK.SQL',
    '_IX.sql', '_IX.SQL',
    '_CK.sql', '_CK.SQL',
    '_OUOM.sql', '_OUOM.SQL',
    '_UOM.sql', '_UOM.SQL',
    '_RQUAL.sql', '_RQUAL.SQL',
    '_RSRC.sql', '_RSRC.SQL',
    '_TCM.sql', '_TCM.SQL',
    '_CCM.sql', '_CCM.SQL',
    '_SYN.sql', '_SYN.SQL',
    '_GUID.sql', '_GUID.SQL',
]

def get_entities_from_models_scripts(models_scripts_dir: Path) -> Set[str]:
    """Extract all entity names from Models/Scripts"""
    entities = set()
    
    # Check all database type folders
    db_folders = ['Sqlserver', 'SQLite', 'PostgreSQL', 'Oracle', 'MySQL', 'MariaDB']
    
    for db_folder in db_folders:
        db_dir = models_scripts_dir / db_folder
        if not db_dir.exists():
            continue
        
        # Find all _TAB.sql files recursively
        for tab_file in db_dir.rglob("*_TAB.sql"):
            entity_name = tab_file.stem.replace('_TAB', '')
            entities.add(entity_name)
    
    return entities

def find_matching_files(db_dir: Path, entity_name: str) -> List[Path]:
    """Find all script files matching an entity name"""
    matching_files = []
    found_files = set()  # Track found files to avoid duplicates
    
    for suffix in SCRIPT_SUFFIXES:
        # Try exact case first, then uppercase
        for pattern in [f"{entity_name}{suffix}", f"{entity_name.upper()}{suffix}"]:
            file_path = db_dir / pattern
            if file_path.exists() and str(file_path) not in found_files:
                matching_files.append(file_path)
                found_files.add(str(file_path))
    
    return matching_files

def main():
    """Main function to remove duplicate scripts"""
    # Get script directories
    ppdm39_scripts_dir = Path(__file__).parent
    workspace_root = ppdm39_scripts_dir.parent.parent
    models_scripts_dir = workspace_root / "Beep.OilandGas.Models" / "Scripts"
    
    if not models_scripts_dir.exists():
        print(f"Error: Models/Scripts directory not found: {models_scripts_dir}")
        return
    
    print("Removing duplicate SQL scripts from PPDM39/Scripts...")
    print(f"PPDM39 Scripts: {ppdm39_scripts_dir}")
    print(f"Models Scripts: {models_scripts_dir}\n")
    
    # Get all entities from Models/Scripts
    print("Extracting entity list from Models/Scripts...")
    entities_to_remove = get_entities_from_models_scripts(models_scripts_dir)
    print(f"Found {len(entities_to_remove)} entities in Models/Scripts\n")
    
    # Database type folders
    db_folders = ['Sqlserver', 'SQLite', 'PostgreSQL', 'Oracle', 'MySQL', 'MariaDB']
    
    total_deleted = 0
    total_entities_processed = 0
    
    for db_folder in db_folders:
        db_dir = ppdm39_scripts_dir / db_folder
        if not db_dir.exists():
            print(f"Skipping {db_folder} (folder does not exist)")
            continue
        
        print(f"Processing {db_folder}...")
        db_deleted = 0
        db_entities = 0
        
        for entity_name in sorted(entities_to_remove):
            matching_files = find_matching_files(db_dir, entity_name)
            
            if matching_files:
                db_entities += 1
                for file_path in matching_files:
                    try:
                        file_path.unlink()
                        db_deleted += 1
                        total_deleted += 1
                        print(f"  Deleted: {file_path.name}")
                    except Exception as e:
                        print(f"  Error deleting {file_path.name}: {e}")
        
        if db_entities > 0:
            print(f"  {db_folder}: Removed {db_deleted} files for {db_entities} entities\n")
        else:
            print(f"  {db_folder}: No matching files found\n")
        
        total_entities_processed += db_entities
    
    print(f"\nRemoval complete!")
    print(f"  Total entities processed: {total_entities_processed}")
    print(f"  Total files deleted: {total_deleted}")
    print(f"  Entities checked: {len(entities_to_remove)}")

if __name__ == "__main__":
    main()
