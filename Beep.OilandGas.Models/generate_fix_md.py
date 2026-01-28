import os

def main():
    report_path = r"c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\duplicate_report.txt"
    output_path = r"c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Models\Data\duplicatefix.md"
    
    with open(report_path, 'r') as f:
        lines = f.readlines()
        
    classes = {}
    current_class = None
    
    parsing_classes = False
    for line in lines:
        line = line.strip()
        if "Duplicate Class Definitions" in line:
            parsing_classes = True
            continue
        if not parsing_classes:
            continue
            
        if line.startswith("- **"):
            current_class = line.replace("- **", "").replace("**", "")
            classes[current_class] = []
        elif line.startswith("- `") and current_class:
            path = line.replace("- `", "").replace("`", "")
            classes[current_class].append(path)

    with open(output_path, 'w') as f:
        f.write("# Duplicate Class Resolution Plan\n\n")
        f.write(f"**Total Duplicates Found**: {len(classes)}\n\n")
        f.write("## Strategy\n")
        f.write("1. **Identify Master**: Choose the most relevant namespace/folder (e.g., `Data` vs `ProductionAccounting`).\n")
        f.write("2. **Merge Properties**: Consolidate unique properties to the master file.\n")
        f.write("3. **Update References**: Fix usages in the code.\n")
        f.write("4. **Delete Duplicate**: Remove the redundant file.\n\n")
        f.write("## Duplicate Candidates\n")
        
        for cls, paths in sorted(classes.items()):
            f.write(f"### {cls}\n")
            for p in paths:
                f.write(f"- `{p}`\n")
            f.write("- **Action**: [ ] Select Master / Merge / Delete\n\n")

if __name__ == "__main__":
    main()
