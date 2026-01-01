#!/usr/bin/env python3
"""
Script to reorganize SQL scripts into subfolders matching the Data folder structure
"""

import os
import shutil
from pathlib import Path
from typing import Optional

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

def get_table_name_from_file(file_path: Path) -> str:
    """Extract table name from entity file name"""
    # Remove .cs extension and get the class name
    return file_path.stem

def main():
    """Main function to reorganize scripts"""
    # Paths
    script_dir = Path(__file__).parent
    data_dir = script_dir.parent / "Data"
    scripts_dir = script_dir
    
    if not data_dir.exists():
        print(f"Error: Data directory not found: {data_dir}")
        return
    
    # Database types
    db_types = ['Sqlserver', 'SQLite', 'PostgreSQL', 'Oracle', 'MySQL', 'MariaDB']
    
    # Get all Entity class files
    entity_files = list(data_dir.rglob("*.cs"))
    entity_files = [f for f in entity_files if f.name != "OwnershipTree.cs" 
                    and not f.name.startswith("Sales") 
                    and not f.name.startswith("Exchange")]
    
    print(f"Found {len(entity_files)} entity files")
    print("Reorganizing scripts into subfolders...")
    
    moved_count = 0
    skipped_count = 0
    
    # Process each entity
    for entity_file in entity_files:
        table_name = get_table_name_from_file(entity_file)
        
        # Determine subfolder for this entity
        subfolder = get_entity_subfolder(entity_file, data_dir)
        
        if not subfolder:
            # Entity is in root, scripts should stay in root
            continue
        
        # Process each database type
        for db_type in db_types:
            db_dir = scripts_dir / db_type
            
            if not db_dir.exists():
                continue
            
            # Script file names
            tab_file = db_dir / f"{table_name}_TAB.sql"
            pk_file = db_dir / f"{table_name}_PK.sql"
            fk_file = db_dir / f"{table_name}_FK.sql"
            
            # Create subfolder
            target_dir = db_dir / subfolder
            target_dir.mkdir(parents=True, exist_ok=True)
            
            # Move files if they exist in root
            for script_file in [tab_file, pk_file, fk_file]:
                if script_file.exists():
                    target_file = target_dir / script_file.name
                    if not target_file.exists():
                        shutil.move(str(script_file), str(target_file))
                        moved_count += 1
                        print(f"  Moved {db_type}/{script_file.name} to {db_type}/{subfolder}/")
                    else:
                        # Target exists in subfolder, remove duplicate from root
                        script_file.unlink()
                        skipped_count += 1
                        print(f"  Removed duplicate {db_type}/{script_file.name} (exists in {subfolder}/)")
    
    print(f"\nReorganization complete!")
    print(f"  Moved: {moved_count} files")
    print(f"  Skipped (already exists): {skipped_count} files")

if __name__ == "__main__":
    main()
