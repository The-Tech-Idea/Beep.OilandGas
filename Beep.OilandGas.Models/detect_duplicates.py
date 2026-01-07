import re
import os
import json
from pathlib import Path
from collections import defaultdict

def normalize_class_name(name):
    """Normalize class name for case-insensitive comparison"""
    # Remove Dto/DTO suffix
    name = re.sub(r'Dto$|DTO$', '', name, flags=re.IGNORECASE)
    # Convert UPPER_CASE to PascalCase equivalent
    if '_' in name:
        parts = name.lower().split('_')
        normalized = ''.join(word.capitalize() for word in parts)
        return normalized
    # Convert to lowercase for comparison
    return name.lower()

def extract_class_info(file_path):
    """Extract comprehensive class information from a C# file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Find all class definitions (including partial classes)
        class_pattern = r'public\s+(?:partial\s+)?class\s+(\w+)'
        class_matches = re.finditer(class_pattern, content)
        
        classes = []
        for class_match in class_matches:
            class_name = class_match.group(1)
            
            # Extract namespace
            namespace_match = re.search(r'namespace\s+([^\s{]+)', content)
            namespace = namespace_match.group(1) if namespace_match else "Unknown"
            
            # Extract public properties (handling both auto-properties and full properties with SetProperty)
            # Pattern 1: public Type PropertyName { get; set; }
            property_pattern1 = r'public\s+([\w\.<>,\s\[\]?]+)\s+(\w+)\s*\{[^}]*get[^}]*set[^}]*\}'
            # Pattern 2: Properties with SetProperty (Data folder style)
            property_pattern2 = r'private\s+([\w\.<>,\s\[\]?]+)\s+(\w+)Value;.*?public\s+([\w\.<>,\s\[\]?]+)\s+(\w+)\s*\{[^}]*get[^}]*SetProperty'
            
            properties = []
            
            # Extract auto-properties
            for match in re.finditer(property_pattern1, content, re.MULTILINE | re.DOTALL):
                prop_type = match.group(1).strip()
                prop_name = match.group(2).strip()
                # Skip PPDM standard columns for comparison
                if prop_name not in ['ACTIVE_IND', 'PPDM_GUID', 'REMARK', 'SOURCE', 
                                     'ROW_CREATED_DATE', 'ROW_CREATED_BY', 'ROW_CHANGED_DATE', 
                                     'ROW_CHANGED_BY', 'ROW_EFFECTIVE_DATE', 'ROW_EXPIRY_DATE', 
                                     'ROW_QUALITY', 'AREA_ID', 'AREA_TYPE', 'BUSINESS_ASSOCIATE_ID',
                                     'EFFECTIVE_DATE', 'EXPIRY_DATE']:
                    properties.append((prop_name, prop_type))
            
            # Extract SetProperty-style properties
            for match in re.finditer(property_pattern2, content, re.MULTILINE | re.DOTALL):
                prop_type = match.group(3).strip()
                prop_name = match.group(4).strip()
                # Skip PPDM standard columns
                if prop_name not in ['ACTIVE_IND', 'PPDM_GUID', 'REMARK', 'SOURCE', 
                                     'ROW_CREATED_DATE', 'ROW_CREATED_BY', 'ROW_CHANGED_DATE', 
                                     'ROW_CHANGED_BY', 'ROW_EFFECTIVE_DATE', 'ROW_EXPIRY_DATE', 
                                     'ROW_QUALITY', 'AREA_ID', 'AREA_TYPE', 'BUSINESS_ASSOCIATE_ID',
                                     'EFFECTIVE_DATE', 'EXPIRY_DATE']:
                    properties.append((prop_name, prop_type))
            
            # Check if it's a PPDM entity
            is_ppdm = 'IPPDMEntity' in content or 'Entity' in content
            is_dto = class_name.endswith('Dto') or class_name.endswith('DTO')
            
            classes.append({
                'class_name': class_name,
                'normalized_name': normalize_class_name(class_name),
                'namespace': namespace,
                'file_path': str(file_path),
                'properties': properties,
                'is_ppdm': is_ppdm,
                'is_dto': is_dto,
                'folder': 'Data' if '\\Data\\' in str(file_path) else ('Models' if '\\Models\\' in str(file_path) else 'DTOs')
            })
        
        return classes
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
        return []

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
    type_str = type_str.replace('double', 'Double')
    type_str = type_str.replace('float', 'Single')
    # Remove generic parameters for comparison
    type_str = re.sub(r'<.*?>', '', type_str)
    # Remove List<> wrapper
    type_str = type_str.replace('List', '').replace('IEnumerable', '').strip()
    return type_str.strip()

def normalize_property_name(name):
    """Normalize property name for comparison (UPPER_CASE to PascalCase)"""
    if '_' in name:
        parts = name.lower().split('_')
        return ''.join(word.capitalize() for word in parts)
    return name

def compare_properties(props1, props2):
    """Compare two property lists, accounting for case differences"""
    # Normalize both property lists
    normalized1 = {}
    for name, typ in props1:
        norm_name = normalize_property_name(name)
        norm_type = normalize_type(typ)
        normalized1[norm_name] = norm_type
    
    normalized2 = {}
    for name, typ in props2:
        norm_name = normalize_property_name(name)
        norm_type = normalize_type(typ)
        normalized2[norm_name] = norm_type
    
    # Find common properties
    common = set(normalized1.keys()) & set(normalized2.keys())
    only1 = set(normalized1.keys()) - set(normalized2.keys())
    only2 = set(normalized2.keys()) - set(normalized1.keys())
    
    # Check type matches for common properties
    type_matches = sum(1 for prop in common if normalized1[prop] == normalized2[prop])
    type_mismatches = common - {prop for prop in common if normalized1[prop] == normalized2[prop]}
    
    return {
        'common_count': len(common),
        'only_in_first': len(only1),
        'only_in_second': len(only2),
        'type_matches': type_matches,
        'type_mismatches': list(type_mismatches),
        'similarity': len(common) / max(len(normalized1), len(normalized2)) if max(len(normalized1), len(normalized2)) > 0 else 0
    }

def classify_duplicate(class1, class2, comparison):
    """Classify the type of duplicate"""
    total_props1 = len(class1['properties'])
    total_props2 = len(class2['properties'])
    common = comparison['common_count']
    
    # True duplicate: same number of properties, all match
    if total_props1 == total_props2 == common and comparison['type_matches'] == common:
        return 'true_duplicate'
    
    # Structural duplicate: same core properties but different metadata
    if common >= min(total_props1, total_props2) * 0.8 and comparison['type_matches'] >= common * 0.9:
        return 'structural_duplicate'
    
    # Semantic duplicate: same concept but different representation
    if common >= min(total_props1, total_props2) * 0.5:
        return 'semantic_duplicate'
    
    # False positive: similar name but different structure
    return 'false_positive'

# Main execution
print("Scanning for duplicate classes...")

data_files = list(Path('Data').rglob('*.cs'))
models_files = list(Path('Models').rglob('*.cs'))
dto_files = list(Path('DTOs').rglob('*.cs'))

print(f"Found {len(data_files)} Data files, {len(models_files)} Models files, {len(dto_files)} DTO files")

# Extract all class information
all_classes = []
for file_list, folder_name in [(data_files, 'Data'), (models_files, 'Models'), (dto_files, 'DTOs')]:
    for file_path in file_list:
        classes = extract_class_info(file_path)
        all_classes.extend(classes)

print(f"Extracted {len(all_classes)} classes")

# Group classes by normalized name
classes_by_name = defaultdict(list)
for cls in all_classes:
    classes_by_name[cls['normalized_name']].append(cls)

# Find potential duplicates
duplicates = []
for normalized_name, classes in classes_by_name.items():
    if len(classes) > 1:
        # Compare all pairs
        for i in range(len(classes)):
            for j in range(i + 1, len(classes)):
                class1 = classes[i]
                class2 = classes[j]
                
                # Skip if same file
                if class1['file_path'] == class2['file_path']:
                    continue
                
                comparison = compare_properties(class1['properties'], class2['properties'])
                duplicate_type = classify_duplicate(class1, class2, comparison)
                
                if duplicate_type != 'false_positive':
                    duplicates.append({
                        'class1': class1,
                        'class2': class2,
                        'comparison': comparison,
                        'duplicate_type': duplicate_type
                    })

# Sort by duplicate type and similarity
duplicates.sort(key=lambda x: (
    {'true_duplicate': 0, 'structural_duplicate': 1, 'semantic_duplicate': 2}[x['duplicate_type']],
    -x['comparison']['similarity']
))

# Generate report
print(f"\n{'='*80}")
print(f"Found {len(duplicates)} potential duplicate pairs")
print(f"{'='*80}\n")

report_data = []
for dup in duplicates:
    c1 = dup['class1']
    c2 = dup['class2']
    comp = dup['comparison']
    
    print(f"Type: {dup['duplicate_type'].upper()}")
    print(f"  Class 1: {c1['class_name']} ({c1['folder']})")
    print(f"    File: {c1['file_path']}")
    print(f"    Properties: {len(c1['properties'])}")
    print(f"  Class 2: {c2['class_name']} ({c2['folder']})")
    print(f"    File: {c2['file_path']}")
    print(f"    Properties: {len(c2['properties'])}")
    print(f"  Comparison:")
    print(f"    Common properties: {comp['common_count']}")
    print(f"    Type matches: {comp['type_matches']}")
    print(f"    Similarity: {comp['similarity']:.2%}")
    print(f"    Only in {c1['class_name']}: {comp['only_in_first']}")
    print(f"    Only in {c2['class_name']}: {comp['only_in_second']}")
    print()
    
    report_data.append({
        'type': dup['duplicate_type'],
        'class1_name': c1['class_name'],
        'class1_folder': c1['folder'],
        'class1_file': c1['file_path'],
        'class1_props': len(c1['properties']),
        'class2_name': c2['class_name'],
        'class2_folder': c2['folder'],
        'class2_file': c2['file_path'],
        'class2_props': len(c2['properties']),
        'common_props': comp['common_count'],
        'type_matches': comp['type_matches'],
        'similarity': comp['similarity']
    })

# Save to JSON for further analysis
with open('duplicate_report.json', 'w', encoding='utf-8') as f:
    json.dump(report_data, f, indent=2)

print(f"Detailed report saved to duplicate_report.json")

