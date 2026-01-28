import os
import re

def count_classes(file_path):
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
    except:
        return 0
    
    # Simple regex to find class definitions, ignoring simple comments is hard without full parser, 
    # but strictly looking for "public class" or "internal class" or just "class" at start of line or after space
    # This is a heuristic.
    # Match "class ClassName" preceded by optional modifiers (public, private, internal, partial, etc.) and whitespace
    # and avoiding "class" in comments by simpler heuristic checks or just better regex
    # Using '^\s*' to match start of line with indentation
    matches = re.findall(r'^\s*(?:public|private|internal|protected|sealed|partial|abstract|\s)*\s+class\s+\w+', content, re.MULTILINE)
    return len(matches)

root_dir = r"c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Data"
multi_class_files = []
total_files = 0
total_classes = 0

for dirpath, dirnames, filenames in os.walk(root_dir):
    for filename in filenames:
        if filename.endswith(".cs"):
            total_files += 1
            file_path = os.path.join(dirpath, filename)
            count = count_classes(file_path)
            total_classes += count
            if count > 1:
                multi_class_files.append((file_path, count))

print(f"Scanned {total_files} C# files.")
print(f"Found {total_classes} total classes.")
print(f"Found {len(multi_class_files)} files with multiple classes:")
for f, c in multi_class_files:
    print(f"{f}: {c} classes")
