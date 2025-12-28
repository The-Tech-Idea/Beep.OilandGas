#!/usr/bin/env python3
"""
Script to consolidate PPDM seed data from PPDMCSVData.json into PPDMReferenceData.json
"""
import json
import os
from collections import defaultdict
from typing import Dict, List, Any, Set

def load_json(file_path: str) -> Dict:
    """Load JSON file"""
    with open(file_path, 'r', encoding='utf-8') as f:
        return json.load(f)

def save_json(file_path: str, data: Dict, indent: int = 2):
    """Save JSON file with formatting"""
    with open(file_path, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=indent, ensure_ascii=False)

def map_table_name(table_name: str) -> str:
    """Map R_* table names to RA_* table names"""
    if table_name.startswith('R_'):
        return 'RA_' + table_name[2:]
    return table_name

def extract_status_type_from_filename(file_name: str) -> str:
    """Extract STATUS_TYPE from filename (e.g., Life_Cycle_PPDM -> Life_Cycle)"""
    if '_PPDM' in file_name:
        parts = file_name.split('_PPDM')[0].split('_')
        if len(parts) > 1:
            return '_'.join(parts)
    return None

def convert_value_status_to_active_ind(value_status: str) -> str:
    """Convert VALUE_STATUS to ACTIVE_IND"""
    if not value_status:
        return "Y"
    value_status_lower = value_status.lower()
    if "adopted" in value_status_lower:
        return "Y"
    elif "deprecated" in value_status_lower:
        return "N"
    return "Y"

def determine_primary_field_name(table_name: str) -> str:
    """Determine the primary field name based on table name"""
    table_lower = table_name.lower()
    if 'status' in table_lower:
        return 'STATUS'
    elif 'type' in table_lower:
        if 'well_status_type' in table_lower:
            return 'STATUS_TYPE'
        elif 'cost_type' in table_lower:
            return 'COST_TYPE'
        elif 'completion_type' in table_lower:
            return 'COMPLETION_TYPE'
        elif 'completion_status_type' in table_lower:
            return 'COMPLETION_STATUS_TYPE'
        elif 'property_type' in table_lower:
            return 'PROPERTY_TYPE'
        elif 'lease_type' in table_lower:
            return 'LEASE_TYPE'
        elif 'production_method' in table_lower:
            return 'PRODUCTION_METHOD'
        elif 'production_status' in table_lower:
            return 'STATUS'
        elif 'production_type' in table_lower:
            return 'PRODUCTION_TYPE'
        elif 'equipment_type' in table_lower:
            return 'EQUIPMENT_TYPE'
        elif 'equipment_status' in table_lower:
            return 'STATUS'
        elif 'facility_type' in table_lower:
            return 'FACILITY_TYPE'
        elif 'drilling_method' in table_lower:
            return 'DRILLING_METHOD'
        elif 'drilling_type' in table_lower:
            return 'DRILLING_TYPE'
        elif 'completion_method' in table_lower:
            return 'COMPLETION_METHOD'
        elif 'reservoir_type' in table_lower:
            return 'RESERVOIR_TYPE'
        elif 'formation_type' in table_lower:
            return 'FORMATION_TYPE'
        elif 'lithology_type' in table_lower:
            return 'LITHOLOGY_TYPE'
        elif 'allocation_type' in table_lower:
            return 'ALLOCATION_TYPE'
        elif 'account_proc_type' in table_lower:
            return 'ACCOUNT_PROC_TYPE'
        elif 'allowable_expense' in table_lower:
            return 'ALLOWABLE_EXPENSE'
        elif 'activity_type' in table_lower:
            return 'ACTIVITY_TYPE'
        elif 'activity_set_type' in table_lower:
            return 'ACTIVITY_SET_TYPE'
        elif 'anl_method_set_type' in table_lower:
            return 'ANL_METHOD_SET_TYPE'
        elif 'anl_confidence_type' in table_lower:
            return 'ANL_CONFIDENCE_TYPE'
        elif 'anl_repeatability' in table_lower:
            return 'ANL_REPEATABILITY'
        return 'TYPE'
    elif 'method' in table_lower:
        return 'METHOD'
    elif 'uom' in table_lower or 'unit_of_measure' in table_lower:
        return 'UOM'
    elif 'row_quality' in table_lower:
        return 'ROW_QUALITY'
    elif 'source' in table_lower and 'account' not in table_lower:
        return 'SOURCE'
    elif 'accounting_method' in table_lower:
        return 'ACCOUNTING_METHOD'
    return 'NAME'

def convert_csv_row_to_json(row: List[str], headers: List[str], table_name: str, file_name: str) -> Dict[str, Any]:
    """Convert CSV row (array of strings) to JSON object"""
    if len(row) != len(headers):
        return None
    
    # Skip header rows or invalid rows
    if len(row) == 0 or (len(row) > 0 and row[0].startswith('(')):
        return None
    
    # Create mapping
    row_dict = dict(zip(headers, row))
    
    # Determine primary field name
    primary_field = determine_primary_field_name(table_name)
    
    # Extract STATUS_TYPE from filename if applicable
    status_type = extract_status_type_from_filename(file_name)
    
    # Build JSON object
    result = {}
    
    # Primary field (STATUS, TYPE, etc.)
    name1 = row_dict.get('NAME1', '').strip()
    if name1:
        result[primary_field] = name1
    
    # STATUS_TYPE (if applicable)
    if status_type and 'status' in table_name.lower():
        result['STATUS_TYPE'] = status_type
    
    # ALIAS fields
    alias = row_dict.get('ALIAS', '').strip()
    name4 = row_dict.get('NAME4', '').strip()
    
    if alias:
        result['ALIAS_ID'] = alias
        result['ALIAS_SHORT_NAME'] = alias
    elif name4:
        result['ALIAS_ID'] = name4
        result['ALIAS_SHORT_NAME'] = name4
    
    if name4:
        result['ALIAS_LONG_NAME'] = name4
    elif name1:
        result['ALIAS_LONG_NAME'] = name1
    
    if not result.get('ALIAS_SHORT_NAME'):
        result['ALIAS_SHORT_NAME'] = name1 if name1 else alias
    
    # ABBREVIATION
    name2 = row_dict.get('NAME2', '').strip()
    name3 = row_dict.get('NAME3', '').strip()
    if name2:
        result['ABBREVIATION'] = name2
    elif name3:
        result['ABBREVIATION'] = name3
    elif alias:
        # Use first few chars of alias as abbreviation
        result['ABBREVIATION'] = alias[:4].upper() if len(alias) > 4 else alias.upper()
    else:
        result['ABBREVIATION'] = name1[:4].upper() if len(name1) > 4 else name1.upper()
    
    # ACTIVE_IND
    value_status = row_dict.get('VALUE_STATUS', '').strip()
    result['ACTIVE_IND'] = convert_value_status_to_active_ind(value_status)
    
    # PREFERRED_IND and ORIGINAL_IND
    result['PREFERRED_IND'] = "Y"
    result['ORIGINAL_IND'] = "Y"
    
    # SOURCE
    source = row_dict.get('SOURCE', '').strip()
    if source:
        result['SOURCE'] = source
    else:
        result['SOURCE'] = "PPDM"
    
    # Only return if we have at least a primary field value
    if result.get(primary_field):
        return result
    
    return None

def process_csv_data(csv_data_path: str) -> Dict[str, List[Dict]]:
    """Process PPDMCSVData.json and convert to structured format"""
    csv_data = load_json(csv_data_path)
    
    # Group by mapped table name
    tables_data = defaultdict(list)
    
    for file_key, entry in csv_data.items():
        if not isinstance(entry, dict):
            continue
        
        table_name = entry.get('tableName', '')
        if not table_name:
            continue
        
        # Map table name
        mapped_table_name = map_table_name(table_name)
        
        headers = entry.get('headers', [])
        rows = entry.get('rows', [])
        file_name = entry.get('fileName', file_key)
        
        # Convert rows
        for row in rows:
            converted = convert_csv_row_to_json(row, headers, mapped_table_name, file_name)
            if converted:
                tables_data[mapped_table_name].append(converted)
    
    return dict(tables_data)

def get_existing_table_names(reference_data: Dict) -> Set[str]:
    """Get set of existing table names in PPDMReferenceData.json"""
    tables = reference_data.get('tables', [])
    return {table.get('tableName', '') for table in tables if table.get('tableName')}

def get_existing_data_keys(reference_data: Dict, table_name: str) -> Set[str]:
    """Get set of unique keys for existing data in a table (for duplicate detection)"""
    tables = reference_data.get('tables', [])
    for table in tables:
        if table.get('tableName') == table_name:
            data = table.get('data', [])
            # Create unique keys based on primary field
            keys = set()
            for item in data:
                # Use primary field value as key
                primary_field = determine_primary_field_name(table_name)
                primary_value = item.get(primary_field, '')
                status_type = item.get('STATUS_TYPE', '')
                if status_type:
                    keys.add(f"{status_type}:{primary_value}")
                else:
                    keys.add(primary_value)
            return keys
    return set()

def is_duplicate(new_item: Dict, existing_keys: Set[str], table_name: str) -> bool:
    """Check if new item is duplicate of existing data"""
    primary_field = determine_primary_field_name(table_name)
    primary_value = new_item.get(primary_field, '')
    status_type = new_item.get('STATUS_TYPE', '')
    
    if status_type:
        key = f"{status_type}:{primary_value}"
    else:
        key = primary_value
    
    return key in existing_keys

def consolidate_data(csv_data_path: str, reference_data_path: str, output_path: str):
    """Main consolidation function"""
    print("Loading CSV data...")
    csv_tables = process_csv_data(csv_data_path)
    print(f"Found {len(csv_tables)} tables in CSV data")
    
    print("Loading reference data...")
    reference_data = load_json(reference_data_path)
    existing_tables = get_existing_table_names(reference_data)
    print(f"Found {len(existing_tables)} existing tables in reference data")
    
    # Process each CSV table
    new_tables = []
    updated_tables = []
    
    for table_name, csv_items in csv_tables.items():
        if table_name in existing_tables:
            # Table exists, check for new data
            existing_keys = get_existing_data_keys(reference_data, table_name)
            new_items = [item for item in csv_items if not is_duplicate(item, existing_keys, table_name)]
            
            if new_items:
                # Find existing table and add new items
                for table in reference_data['tables']:
                    if table.get('tableName') == table_name:
                        table['data'].extend(new_items)
                        updated_tables.append(table_name)
                        print(f"  Added {len(new_items)} new items to {table_name}")
                        break
        else:
            # New table, add it
            new_table = {
                "tableName": table_name,
                "description": f"Reference data for {table_name}",
                "data": csv_items
            }
            reference_data['tables'].append(new_table)
            new_tables.append(table_name)
            print(f"  Added new table {table_name} with {len(csv_items)} items")
    
    # Update description
    reference_data['description'] = "PPDM standard reference tables (RA_* tables) with comprehensive standard values. This file contains consolidated seed data from PPDMCSVData.json and additional priority tables."
    
    print(f"\nSummary:")
    print(f"  New tables added: {len(new_tables)}")
    print(f"  Existing tables updated: {len(updated_tables)}")
    
    print(f"\nSaving to {output_path}...")
    save_json(output_path, reference_data)
    print("Done!")

if __name__ == '__main__':
    base_dir = os.path.dirname(os.path.abspath(__file__))
    csv_data_path = os.path.join(base_dir, 'Core', 'SeedData', 'PPDMCSVData.json')
    reference_data_path = os.path.join(base_dir, 'SeedData', 'Templates', 'PPDMReferenceData.json')
    output_path = reference_data_path  # Overwrite the original
    
    consolidate_data(csv_data_path, reference_data_path, output_path)

