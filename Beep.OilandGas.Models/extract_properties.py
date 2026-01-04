import re
import os
from pathlib import Path

def extract_properties(file_path):
    """Extract public properties from a C# class file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Find the class definition
        class_match = re.search(r'public\s+class\s+(\w+)', content)
        if not class_match:
            return None, []
        
        class_name = class_match.group(1)
        
        # Extract namespace
        namespace_match = re.search(r'namespace\s+([^\s{]+)', content)
        namespace = namespace_match.group(1) if namespace_match else "Unknown"
        
        # Extract public properties (handling both auto-properties and full properties)
        # Pattern: public Type PropertyName { get; set; } or public Type? PropertyName { get; set; }
        property_pattern = r'public\s+([\w\.<>,\s\[\]?]+)\s+(\w+)\s*\{[^}]*get[^}]*set[^}]*\}'
        properties = []
        
        for match in re.finditer(property_pattern, content):
            prop_type = match.group(1).strip()
            prop_name = match.group(2).strip()
            properties.append((prop_name, prop_type))
        
        return {
            'class_name': class_name,
            'namespace': namespace,
            'file_path': str(file_path),
            'properties': properties
        }
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
        return None

def normalize_type(type_str):
    """Normalize type string for comparison"""
    # Remove nullable marker
    type_str = type_str.replace('?', '').strip()
    # Normalize common types
    type_str = type_str.replace('System.', '')
    type_str = type_str.replace('string', 'String')
    type_str = type_str.replace('int', 'Int32')
    type_str = type_str.replace('decimal', 'Decimal')
    type_str = type_str.replace('DateTime', 'DateTime')
    type_str = type_str.replace('bool', 'Boolean')
    return type_str.strip()

def compare_classes(dto_info, actual_info, exact_match=True):
    """Compare two class definitions"""
    if not dto_info or not actual_info:
        return False
    
    dto_props = {name: normalize_type(typ) for name, typ in dto_info['properties']}
    actual_props = {name: normalize_type(typ) for name, typ in actual_info['properties']}
    
    if exact_match:
        # Exact match: same number of properties and all match
        if len(dto_props) != len(actual_props):
            return False
        
        for prop_name, prop_type in dto_props.items():
            if prop_name not in actual_props:
                return False
            if actual_props[prop_name] != prop_type:
                return False
        return True
    else:
        # Subset match: all DTO properties exist in actual class
        if len(dto_props) == 0:
            return False
        
        for prop_name, prop_type in dto_props.items():
            if prop_name not in actual_props:
                return False
            if actual_props[prop_name] != prop_type:
                return False
        return True

# Main execution
dto_files = list(Path('DTOs').rglob('*.cs'))
models_files = list(Path('Models').rglob('*.cs'))
data_files = list(Path('Data').rglob('*.cs'))

print(f"Found {len(dto_files)} DTO files, {len(models_files)} Models files, {len(data_files)} Data files")

duplicates = []

for dto_file in dto_files:
    dto_info = extract_properties(dto_file)
    if not dto_info:
        continue
    
    dto_name = dto_info['class_name']
    if not dto_name.endswith('Dto') and not dto_name.endswith('DTO'):
        continue
    
    base_name = dto_name[:-3] if dto_name.endswith('Dto') else dto_name[:-3]
    
    # Search in Models
    for model_file in models_files:
        actual_info = extract_properties(model_file)
        if actual_info and isinstance(actual_info, dict) and actual_info.get('class_name') == base_name:
            if compare_classes(dto_info, actual_info, exact_match=True):
                duplicates.append({
                    'dto': dto_info,
                    'actual': actual_info,
                    'location': 'Models',
                    'match_type': 'exact'
                })
    
    # Search in Data
    for data_file in data_files:
        actual_info = extract_properties(data_file)
        if actual_info and isinstance(actual_info, dict) and actual_info.get('class_name') == base_name:
            if compare_classes(dto_info, actual_info, exact_match=True):
                duplicates.append({
                    'dto': dto_info,
                    'actual': actual_info,
                    'location': 'Data',
                    'match_type': 'exact'
                })

print(f"\nFound {len(duplicates)} duplicate(s):")
for dup in duplicates:
    print(f"\nDTO: {dup['dto']['class_name']} ({dup['dto']['file_path']})")
    print(f"Actual: {dup['actual']['class_name']} ({dup['actual']['file_path']})")
    print(f"Location: {dup['location']}")
    print(f"Properties match: {len(dup['dto']['properties'])} properties")
