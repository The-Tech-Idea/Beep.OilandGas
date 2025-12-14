# PowerShell script to generate seed data for PPDM39 tables from CSV files
# Specifically generates R_WELL_STATUS seed data from well status facet CSV files
# Usage: .\GeneratePPDMSeedData.ps1 [csvFolder] [outputFolder]

param(
    [string]$CsvFolder = "C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\PPDM_DATA\CSV",
    [string]$OutputFolder = "C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\PPDM_DATA\SeedData"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PPDM39 Seed Data Generator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Mapping of CSV file names to STATUS_TYPE values for R_WELL_STATUS
$statusTypeMapping = @{
    "Life_Cycle" = "Business Life Cycle Phase"
    "Role" = "Role"
    "Business_Interest" = "Business Interest"
    "Business_Intention" = "Business Intention"
    "Outcome" = "Outcome"
    "Lahee_Class" = "Lahee Class"
    "Play_Type" = "Play Type"
    "Well_Structure" = "Well Structure"
    "Trajectory_Type" = "Trajectory Type"
    "Fluid_Direction" = "Fluid Direction"
    "Well_Reporting_Class" = "Well Reporting Class"
    "Fluid_Type" = "Fluid Type"
    "Wellbore_Status" = "Wellbore Status"
    "Well_Status" = "Well Status"
    "Operatorship" = "Operatorship"
}

# Validate CSV folder
if (-not (Test-Path $CsvFolder)) {
    Write-Host "ERROR: CSV folder not found: $CsvFolder" -ForegroundColor Red
    exit 1
}

# Create output folder if it doesn't exist
if (-not (Test-Path $OutputFolder)) {
    New-Item -ItemType Directory -Path $OutputFolder -Force | Out-Null
    Write-Host "Created output folder: $OutputFolder" -ForegroundColor Green
}

# Get all CSV files
$csvFiles = Get-ChildItem -Path $CsvFolder -Filter "*.csv" -File

if ($csvFiles.Count -eq 0) {
    Write-Host "No CSV files found in: $CsvFolder" -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($csvFiles.Count) CSV files to process" -ForegroundColor Green
Write-Host ""

$allSeedData = @()
$processedCount = 0
$skippedCount = 0

foreach ($csvFile in $csvFiles) {
    $fileName = $csvFile.BaseName
    
    # Extract STATUS_TYPE from filename
    $statusType = $null
    foreach ($key in $statusTypeMapping.Keys) {
        if ($fileName -like "*$key*") {
            $statusType = $statusTypeMapping[$key]
            break
        }
    }
    
    if ($null -eq $statusType) {
        Write-Host "Skipping: $fileName (not a well status facet file)" -ForegroundColor Gray
        $skippedCount++
        continue
    }
    
    Write-Host "Processing: $fileName -> STATUS_TYPE: $statusType" -ForegroundColor Cyan
    
    try {
        # Read CSV file (skip first 3 rows: logo, disclaimer, copyright)
        $csvContent = Get-Content $csvFile.FullName -Encoding UTF8
        
        if ($csvContent.Count -lt 4) {
            Write-Host "  ⚠ File has less than 4 rows, skipping..." -ForegroundColor Yellow
            $skippedCount++
            continue
        }
        
        # Get header row (row 4, index 3 after skipping first 3 rows)
        # CSV structure: Row 1-3 (logo, disclaimer, copyright), Row 4 (header), Row 5+ (data)
        $headerRow = $csvContent[3] -split ','
        
        # Find column indices
        $name1Index = -1
        $definitionIndex = -1
        $valueStatusIndex = -1
        $sourceIndex = -1
        $resourceIndex = -1
        
        for ($i = 0; $i -lt $headerRow.Count; $i++) {
            $col = $headerRow[$i].Trim('"')
            if ($col -eq "NAME1") { $name1Index = $i }
            elseif ($col -eq "DEFINITION") { $definitionIndex = $i }
            elseif ($col -eq "VALUE_STATUS") { $valueStatusIndex = $i }
            elseif ($col -eq "SOURCE") { $sourceIndex = $i }
            elseif ($col -eq "RESOURCE") { $resourceIndex = $i }
        }
        
        if ($name1Index -eq -1) {
            Write-Host "  ⚠ NAME1 column not found, skipping..." -ForegroundColor Yellow
            $skippedCount++
            continue
        }
        
        # Process data rows (starting from row 5, index 4)
        $recordCount = 0
        for ($rowIndex = 4; $rowIndex -lt $csvContent.Count; $rowIndex++) {
            $row = $csvContent[$rowIndex]
            
            # Skip empty rows
            if ([string]::IsNullOrWhiteSpace($row) -or $row.Trim() -eq ",") {
                continue
            }
            
            # Parse CSV row (handle quoted values)
            $values = @()
            $currentValue = ""
            $inQuotes = $false
            
            foreach ($char in $row.ToCharArray()) {
                if ($char -eq '"') {
                    $inQuotes = -not $inQuotes
                }
                elseif ($char -eq ',' -and -not $inQuotes) {
                    $values += $currentValue
                    $currentValue = ""
                }
                else {
                    $currentValue += $char
                }
            }
            $values += $currentValue  # Add last value
            
            # Extract values
            $status = if ($values.Count -gt $name1Index) { $values[$name1Index].Trim('"').Trim() } else { "" }
            $definition = if ($values.Count -gt $definitionIndex -and $definitionIndex -ge 0) { $values[$definitionIndex].Trim('"').Trim() } else { "" }
            $valueStatus = if ($values.Count -gt $valueStatusIndex -and $valueStatusIndex -ge 0) { $values[$valueStatusIndex].Trim('"').Trim() } else { "Adopted" }
            $source = if ($values.Count -gt $sourceIndex -and $sourceIndex -ge 0) { $values[$sourceIndex].Trim('"').Trim() } else { "PPDM" }
            $resource = if ($values.Count -gt $resourceIndex -and $resourceIndex -ge 0) { $values[$resourceIndex].Trim('"').Trim() } else { "" }
            
            # Skip if status is empty
            if ([string]::IsNullOrWhiteSpace($status)) {
                continue
            }
            
            # Create STATUS_ID (format: STATUS_TYPE,STATUS)
            $statusId = "$statusType,$status"
            
            # Generate PPDM_GUID
            $ppdmGuid = [System.Guid]::NewGuid().ToString().ToUpper()
            
            # Create seed data record
            $seedRecord = @{
                STATUS_ID = $statusId
                STATUS_TYPE = $statusType
                STATUS = $status
                LONG_NAME = $status
                SHORT_NAME = if ($status.Length -gt 20) { $status.Substring(0, 20) } else { $status }
                DESCRIPTION = $definition
                VALUE_STATUS = $valueStatus
                SOURCE = $source
                RESOURCE = $resource
                ACTIVE_IND = "Y"
                PPDM_GUID = $ppdmGuid
                EFFECTIVE_DATE = (Get-Date).ToString("yyyy-MM-dd")
                EXPIRY_DATE = "1900-01-01"  # DateTime.MinValue equivalent
            }
            
            $allSeedData += $seedRecord
            $recordCount++
        }
        
        Write-Host "  ✓ Processed $recordCount records" -ForegroundColor Green
        $processedCount++
    }
    catch {
        Write-Host "  ✗ Error processing $fileName : $($_.Exception.Message)" -ForegroundColor Red
        $skippedCount++
    }
    
    Write-Host ""
}

# Generate SQL INSERT statements
$sqlFile = Join-Path $OutputFolder "R_WELL_STATUS_SeedData.sql"
$sqlContent = New-Object System.Text.StringBuilder
$sqlContent.AppendLine("-- Auto-generated seed data for R_WELL_STATUS table") | Out-Null
$sqlContent.AppendLine("-- Generated from CSV files in PPDM_DATA/CSV folder") | Out-Null
$sqlContent.AppendLine("-- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')") | Out-Null
$sqlContent.AppendLine("-- Total records: $($allSeedData.Count)") | Out-Null
$sqlContent.AppendLine("") | Out-Null
$sqlContent.AppendLine("SET NOCOUNT ON;") | Out-Null
$sqlContent.AppendLine("GO") | Out-Null
$sqlContent.AppendLine("") | Out-Null

foreach ($record in $allSeedData) {
    $statusId = $record.STATUS_ID.Replace("'", "''")
    $statusType = $record.STATUS_TYPE.Replace("'", "''")
    $status = $record.STATUS.Replace("'", "''")
    $longName = $record.LONG_NAME.Replace("'", "''")
    $shortName = $record.SHORT_NAME.Replace("'", "''")
    $description = $record.DESCRIPTION.Replace("'", "''")
    $valueStatus = $record.VALUE_STATUS.Replace("'", "''")
    $source = $record.SOURCE.Replace("'", "''")
    $resource = $record.RESOURCE.Replace("'", "''")
    
    $shortName = $record.SHORT_NAME.Replace("'", "''")
    $ppdmGuid = $record.PPDM_GUID
    $effectiveDate = $record.EFFECTIVE_DATE
    $expiryDate = $record.EXPIRY_DATE
    
    $sqlContent.AppendLine("INSERT INTO R_WELL_STATUS (STATUS_ID, STATUS_TYPE, STATUS, LONG_NAME, SHORT_NAME, DESCRIPTION, VALUE_STATUS, SOURCE, RESOURCE, ACTIVE_IND, PPDM_GUID, EFFECTIVE_DATE, EXPIRY_DATE, ROW_CREATED_BY, ROW_CREATED_DATE)") | Out-Null
    $sqlContent.AppendLine("VALUES (") | Out-Null
    $sqlContent.AppendLine("    '$statusId',") | Out-Null
    $sqlContent.AppendLine("    '$statusType',") | Out-Null
    $sqlContent.AppendLine("    '$status',") | Out-Null
    $sqlContent.AppendLine("    '$longName',") | Out-Null
    $sqlContent.AppendLine("    '$shortName',") | Out-Null
    $sqlContent.AppendLine("    '$description',") | Out-Null
    $sqlContent.AppendLine("    '$valueStatus',") | Out-Null
    $sqlContent.AppendLine("    '$source',") | Out-Null
    $sqlContent.AppendLine("    '$resource',") | Out-Null
    $sqlContent.AppendLine("    '$($record.ACTIVE_IND)',") | Out-Null
    $sqlContent.AppendLine("    '$ppdmGuid',") | Out-Null
    $sqlContent.AppendLine("    '$effectiveDate',") | Out-Null
    $sqlContent.AppendLine("    '$expiryDate',") | Out-Null
    $sqlContent.AppendLine("    'SYSTEM',") | Out-Null
    $sqlContent.AppendLine("    GETDATE()") | Out-Null
    $sqlContent.AppendLine(");") | Out-Null
    $sqlContent.AppendLine("") | Out-Null
}

[System.IO.File]::WriteAllText($sqlFile, $sqlContent.ToString(), [System.Text.Encoding]::UTF8)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Generation Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Processed files: $processedCount" -ForegroundColor Green
Write-Host "  Skipped files: $skippedCount" -ForegroundColor Yellow
Write-Host "  Total records: $($allSeedData.Count)" -ForegroundColor Green
Write-Host ""
Write-Host "Output:" -ForegroundColor Cyan
Write-Host "  SQL file: $sqlFile" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review the generated SQL file" -ForegroundColor White
Write-Host "  2. Execute against your PPDM39 database" -ForegroundColor White
Write-Host "  3. Verify data in R_WELL_STATUS table" -ForegroundColor White
Write-Host ""

