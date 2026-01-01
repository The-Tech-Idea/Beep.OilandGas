#!/usr/bin/env python3
"""
Script to move SQL scripts from PPDM39/Scripts to Models/Scripts for new entities
"""

import shutil
from pathlib import Path

# Entity to folder mapping
ENTITY_TO_FOLDER = {
    # Security entities
    'USER_ASSET_ACCESS': 'Security',
    'ORGANIZATION_HIERARCHY_CONFIG': 'Security',
    'USER_PROFILE': 'Security',
    'SETUP_WIZARD_LOG': 'Security',
    # Common reference tables
    'R_ANOMALY_TYPE': 'Common',
    'R_DISCOVERY_TYPE': 'Common',
    'R_ECONOMIC_METRIC': 'Common',
    'R_INSPECTION_TYPE': 'Common',
    'R_PROSPECT_STATUS': 'Common',
    'R_PROSPECT_TYPE': 'Common',
    'R_TRAP_TYPE': 'Common',
}

# Script suffixes to move
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

def main():
    """Move scripts from PPDM39 to Models"""
    ppdm39_scripts_dir = Path(__file__).parent
    workspace_root = ppdm39_scripts_dir.parent.parent
    models_scripts_dir = workspace_root / "Beep.OilandGas.Models" / "Scripts"
    
    if not models_scripts_dir.exists():
        print(f"Error: Models/Scripts directory not found: {models_scripts_dir}")
        return
    
    print("Moving SQL scripts from PPDM39/Scripts to Models/Scripts...")
    print(f"PPDM39 Scripts: {ppdm39_scripts_dir}")
    print(f"Models Scripts: {models_scripts_dir}\n")
    
    # Database type folders
    db_folders = ['Sqlserver', 'SQLite', 'PostgreSQL', 'Oracle', 'MySQL', 'MariaDB']
    
    total_moved = 0
    
    for db_folder in db_folders:
        ppdm39_db_dir = ppdm39_scripts_dir / db_folder
        if not ppdm39_db_dir.exists():
            continue
        
        models_db_dir = models_scripts_dir / db_folder
        if not models_db_dir.exists():
            continue
        
        print(f"Processing {db_folder}...")
        db_moved = 0
        
        for entity_name, target_folder in ENTITY_TO_FOLDER.items():
            target_dir = models_db_dir / target_folder
            target_dir.mkdir(parents=True, exist_ok=True)
            
            for suffix in SCRIPT_SUFFIXES:
                # Try both exact case and uppercase
                for pattern in [f"{entity_name}{suffix}", f"{entity_name.upper()}{suffix}"]:
                    source_file = ppdm39_db_dir / pattern
                    if source_file.exists():
                        target_file = target_dir / pattern
                        
                        if target_file.exists():
                            # Remove from PPDM39 if already in Models
                            try:
                                source_file.unlink()
                                print(f"  Removed duplicate: {pattern}")
                            except:
                                pass
                        else:
                            # Move to Models
                            try:
                                shutil.move(str(source_file), str(target_file))
                                db_moved += 1
                                total_moved += 1
                                print(f"  Moved: {pattern} -> {target_folder}/")
                            except Exception as e:
                                print(f"  Error moving {pattern}: {e}")
        
        if db_moved > 0:
            print(f"  {db_folder}: Moved {db_moved} files\n")
        else:
            print(f"  {db_folder}: No files to move\n")
    
    print(f"\nMove complete!")
    print(f"  Total files moved: {total_moved}")

if __name__ == "__main__":
    main()
