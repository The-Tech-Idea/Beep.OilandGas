import os
import subprocess
import re
from collections import defaultdict

def get_git_files(root_dir):
    try:
        # Run git ls-files to get tracked files
        result = subprocess.run(['git', 'ls-files'], cwd=root_dir, capture_output=True, text=True, errors='ignore')
        if result.returncode != 0:
            print("Error running git ls-files")
            return []
        return result.stdout.splitlines()
    except Exception as e:
        print(f"Exception running git ls-files: {e}")
        return []

def find_case_collsions(files):
    # Map lowercase path -> list of actual paths
    lowermap = defaultdict(list)
    for f in files:
        if f.endswith('.cs') and 'Beep.OilandGas.Models/Data' in f.replace('\\', '/'):
             # Normalize to forward slashes for comparison
             f_norm = f.replace('\\', '/')
             # Get relative path from repo root or just use full path if unique enough
             lowermap[f_norm.lower()].append(f_norm)
    
    collisions = []
    for lower, paths in lowermap.items():
        if len(paths) > 1:
            collisions.append(paths)
    return collisions

def scan_classes(root_dir):
    class_map = defaultdict(list)
    
    for dirpath, dirnames, filenames in os.walk(root_dir):
        for filename in filenames:
            if filename.endswith(".cs"):
                full_path = os.path.join(dirpath, filename)
                try:
                    with open(full_path, 'r', encoding='utf-8-sig') as f:
                        content = f.read()
                    
                    # Regex to find class definitions
                    # Matches "class MyClass" optionally preceded by modifiers
                    matches = re.findall(r'^\s*(?:public|private|internal|protected|sealed|partial|abstract|\s)*\s+class\s+(\w+)', content, re.MULTILINE)
                    for class_name in matches:
                        class_map[class_name].append(full_path)
                except Exception as e:
                    print(f"Error reading {full_path}: {e}")
    
    duplicates = []
    for class_name, paths in class_map.items():
        if len(paths) > 1:
            duplicates.append((class_name, paths))
            
    return duplicates

def main():
    repo_root = r"c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models"
    data_dir = os.path.join(repo_root, "Data")
    
    print("# Duplicate Analysis Report")
    print("\n## Case-Insensitive File Collisions (Windows Issues)")
    print("Files that differ only by case (git tracks both, but Windows sees one):")
    
    git_files = get_git_files(repo_root)
    # Filter git files to only those inside Data folder for relevance
    data_files = [f for f in git_files if "Data/" in f.replace('\\', '/')]
    
    # We need a robust collision check. 
    # Use a dictionary: lowercase_full_path -> [list of actual tracking paths]
    path_map = defaultdict(list)
    for f in data_files:
        path_map[f.lower()].append(f)
        
    collisions_found = False
    for lower, originals in path_map.items():
        if len(set(originals)) > 1: # Use set to avoid duplicates if git returns same
             print(f"- **Collision Group**:")
             for path in originals:
                 print(f"  - `{path}`")
             collisions_found = True
             
    if not collisions_found:
        print("\nNo file name case-collisions found in git index.")

    print("\n## Duplicate Class Definitions")
    print("Classes defined in multiple files:")
    
    dup_classes = scan_classes(data_dir)
    if not dup_classes:
        print("\nNo duplicate class definitions found.")
        
    for class_name, paths in sorted(dup_classes):
        print(f"- **{class_name}**")
        for p in paths:
            # Make path relative to Data for cleaner output
            rel_path = os.path.relpath(p, repo_root)
            print(f"  - `{rel_path}`")

if __name__ == "__main__":
    main()
