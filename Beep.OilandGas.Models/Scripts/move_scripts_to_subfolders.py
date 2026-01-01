#!/usr/bin/env python3
"""
Script to move existing SQL scripts to subfolders based on entity file locations
"""

import shutil
from pathlib import Path
from typing import Optional

def get_entity_subfolder(entity_file: Path, data_dir: Path) -> Optional[str]:
    """Determine the subfolder for an entity based on its location in Data folder"""
    try:
        relative_path = entity_file.relative_to(data_dir)
        if len(relative_path.parts) > 1:
            subfolder = relative_path.parts[0]
            return subfolder.capitalize()
        return None
    except:
        return None

def get_table_name_from_file(file_path: Path) -> str:
    """Extract table name from entity file name"""
    return file_path.stem

def main():
    """Main function to move scripts to subfolders"""
    script_dir = Path(__file__).parent
    data_dir = script_dir.parent / "Data"
    scripts_dir = script_dir
    
    if not data_dir.exists():
        print(f"Error: Data directory not found: {data_dir}")
        return
    
    db_types = ['Sqlserver', 'SQLite', 'PostgreSQL', 'Oracle', 'MySQL', 'MariaDB']
    
    # Get all Entity class files
    entity_files = list(data_dir.rglob("*.cs"))
    entity_files = [f for f in entity_files if f.name != "OwnershipTree.cs" 
                    and not f.name.startswith("Sales") 
                    and not f.name.startswith("Exchange")]
    
    print(f"Found {len(entity_files)} entity files")
    print("Moving scripts to subfolders...\n")
    
    moved_count = 0
    removed_count = 0
    
    for entity_file in entity_files:
        table_name = get_table_name_from_file(entity_file)
        subfolder = get_entity_subfolder(entity_file, data_dir)
        
        if not subfolder:
            continue  # Root entities stay in root
        
        for db_type in db_types:
            db_dir = scripts_dir / db_type
            if not db_dir.exists():
                continue
            
            # Script files in root
            tab_file = db_dir / f"{table_name}_TAB.sql"
            pk_file = db_dir / f"{table_name}_PK.sql"
            fk_file = db_dir / f"{table_name}_FK.sql"
            
            # Target subfolder
            target_dir = db_dir / subfolder
            target_dir.mkdir(parents=True, exist_ok=True)
            
            # Move each script file
            for script_file in [tab_file, pk_file, fk_file]:
                if script_file.exists():
                    target_file = target_dir / script_file.name
                    if target_file.exists():
                        # Target exists, remove duplicate from root
                        script_file.unlink()
                        removed_count += 1
                        print(f"  Removed duplicate: {db_type}/{script_file.name}")
                    else:
                        # Move to subfolder
                        shutil.move(str(script_file), str(target_file))
                        moved_count += 1
                        print(f"  Moved: {db_type}/{script_file.name} -> {db_type}/{subfolder}/")
    
    print(f"\nComplete!")
    print(f"  Moved: {moved_count} files")
    print(f"  Removed duplicates: {removed_count} files")

if __name__ == "__main__":
    main()
