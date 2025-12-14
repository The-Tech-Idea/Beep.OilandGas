# PPDM39 Data Seeding

This folder contains tools for converting Excel seed data files to CSV format and generating seed data for PPDM39 tables.

## Excel to CSV Conversion

### Prerequisites
- Microsoft Excel must be installed on your system
- PowerShell (included with Windows)

### Usage

Run the PowerShell script to convert all Excel files in the PPDM_DATA folder to CSV:

```powershell
.\ConvertExcelToCsv.ps1
```

Or specify custom folders:

```powershell
.\ConvertExcelToCsv.ps1 -SourceFolder "C:\Path\To\Excel\Files" -OutputFolder "C:\Path\To\CSV\Output"
```

### What it does

1. Scans the source folder for all `.xlsx` files
2. Opens each Excel file using Excel COM automation
3. Converts each worksheet to CSV format
4. Saves CSV files to the output folder
5. Handles multiple worksheets by creating separate CSV files (e.g., `Well_Status_Type_Sheet1.csv`)

### Output

CSV files will be saved to:
- Default: `C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\PPDM_DATA\CSV`
- Or the folder specified by `-OutputFolder` parameter

### Well Status Files

The following Excel files are particularly important for well status seeding:

- `Well_Status_Type_PPDM_20220225.xlsx` → `R_WELL_STATUS` table
- `Business_Intention_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Business Intention")
- `Business_Interest_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Business Interest")
- `Life_Cycle_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Business Life Cycle Phase")
- `Outcome_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Outcome")
- `Role_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Role")
- `Play_Type_PPDM_20220425.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Play Type")
- `Well_Structure_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Well Structure")
- `Fluid_Direction_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Fluid Direction")
- `Trajectory_Type_PPDM_20220421.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Trajectory Type")
- `Wellbore_Status_PPDM_20220610.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Wellbore Status")
- `Well_Status_PPDM_20220225.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Well Status")
- `Fluid_Type_PPDM_20220419.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Fluid Type")
- `Well_Reporting_Class_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Well Reporting Class")
- `Lahee_Class_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Lahee Class")
- `Operatorship_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Operatorship")

## Next Steps

After converting Excel files to CSV:

1. Review the CSV files to ensure data quality
2. Create SQL INSERT scripts or C# seed data classes from the CSV files
3. Use the seed data to populate `R_WELL_STATUS` and other reference tables
4. Ensure all STATUS_TYPE values match the default well status types defined in `PPDM39DefaultsRepository`

## Notes

- Excel files may contain multiple worksheets - each will be converted to a separate CSV
- The script preserves the original Excel file structure
- CSV files use UTF-8 encoding
- Date formats are preserved from Excel

