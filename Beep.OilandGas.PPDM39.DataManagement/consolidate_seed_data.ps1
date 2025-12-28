# PowerShell script to consolidate PPDM seed data
$ErrorActionPreference = "Stop"

$baseDir = Split-Path -Parent $PSScriptRoot
$csvDataPath =  "C:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.PPDM39.DataManagement\Core\SeedData\PPDMCSVData.json"
$referenceDataPath = "C:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.PPDM39.DataManagement\SeedData\Templates\PPDMReferenceData.json"

Write-Host "Loading CSV data from: $csvDataPath"
$csvData = Get-Content $csvDataPath -Raw | ConvertFrom-Json

Write-Host "Loading reference data from: $referenceDataPath"
$referenceData = Get-Content $referenceDataPath -Raw | ConvertFrom-Json

# Process CSV data
$csvTables = @{}
$processedCount = 0

foreach ($entry in $csvData.PSObject.Properties) {
    $entryObj = $entry.Value
    $tableName = $entryObj.tableName
    if ([string]::IsNullOrEmpty($tableName)) { continue }
    
    # Map R_* to RA_*
    $mappedTableName = if ($tableName.StartsWith("R_")) { "RA_" + $tableName.Substring(2) } else { $tableName }
    
    $headers = $entryObj.headers
    $rows = $entryObj.rows
    $fileName = if ($entryObj.fileName) { $entryObj.fileName } else { $entry.Name }
    
    if (-not $headers -or -not $rows) { continue }
    
    $items = @()
    foreach ($row in $rows) {
        if ($row.Count -ne $headers.Count) { continue }
        if ($row[0] -match '^\(.*\)') { continue }  # Skip header rows
        
        # Convert row to object
        $rowDict = @{}
        for ($i = 0; $i -lt $headers.Count; $i++) {
            $rowDict[$headers[$i]] = if ($row[$i]) { $row[$i].Trim() } else { "" }
        }
        
        $name1 = $rowDict["NAME1"]
        if ([string]::IsNullOrWhiteSpace($name1)) { continue }
        
        # Determine primary field
        $primaryField = "STATUS"
        if ($mappedTableName -match "TYPE" -and $mappedTableName -notmatch "STATUS") {
            if ($mappedTableName -match "COST_TYPE") { $primaryField = "COST_TYPE" }
            elseif ($mappedTableName -match "COMPLETION_TYPE") { $primaryField = "COMPLETION_TYPE" }
            elseif ($mappedTableName -match "PROPERTY_TYPE") { $primaryField = "PROPERTY_TYPE" }
            elseif ($mappedTableName -match "LEASE_TYPE") { $primaryField = "LEASE_TYPE" }
            elseif ($mappedTableName -match "PRODUCTION_METHOD") { $primaryField = "PRODUCTION_METHOD" }
            elseif ($mappedTableName -match "PRODUCTION_TYPE") { $primaryField = "PRODUCTION_TYPE" }
            elseif ($mappedTableName -match "EQUIPMENT_TYPE") { $primaryField = "EQUIPMENT_TYPE" }
            elseif ($mappedTableName -match "FACILITY_TYPE") { $primaryField = "FACILITY_TYPE" }
            elseif ($mappedTableName -match "DRILLING_METHOD") { $primaryField = "DRILLING_METHOD" }
            elseif ($mappedTableName -match "DRILLING_TYPE") { $primaryField = "DRILLING_TYPE" }
            elseif ($mappedTableName -match "COMPLETION_METHOD") { $primaryField = "COMPLETION_METHOD" }
            elseif ($mappedTableName -match "RESERVOIR_TYPE") { $primaryField = "RESERVOIR_TYPE" }
            elseif ($mappedTableName -match "FORMATION_TYPE") { $primaryField = "FORMATION_TYPE" }
            elseif ($mappedTableName -match "LITHOLOGY_TYPE") { $primaryField = "LITHOLOGY_TYPE" }
            elseif ($mappedTableName -match "ALLOCATION_TYPE") { $primaryField = "ALLOCATION_TYPE" }
            elseif ($mappedTableName -match "ACCOUNT_PROC_TYPE") { $primaryField = "ACCOUNT_PROC_TYPE" }
            elseif ($mappedTableName -match "ALLOWABLE_EXPENSE") { $primaryField = "ALLOWABLE_EXPENSE" }
            elseif ($mappedTableName -match "ACTIVITY_TYPE") { $primaryField = "ACTIVITY_TYPE" }
            elseif ($mappedTableName -match "ACTIVITY_SET_TYPE") { $primaryField = "ACTIVITY_SET_TYPE" }
            elseif ($mappedTableName -match "ANL_METHOD_SET_TYPE") { $primaryField = "ANL_METHOD_SET_TYPE" }
            elseif ($mappedTableName -match "ANL_CONFIDENCE_TYPE") { $primaryField = "ANL_CONFIDENCE_TYPE" }
            elseif ($mappedTableName -match "ANL_REPEATABILITY") { $primaryField = "ANL_REPEATABILITY" }
            else { $primaryField = "TYPE" }
        }
        elseif ($mappedTableName -match "UOM" -or $mappedTableName -match "UNIT_OF_MEASURE") {
            $primaryField = "UOM"
        }
        elseif ($mappedTableName -match "ROW_QUALITY") {
            $primaryField = "ROW_QUALITY"
        }
        elseif ($mappedTableName -match "SOURCE" -and $mappedTableName -notmatch "ACCOUNT") {
            $primaryField = "SOURCE"
        }
        elseif ($mappedTableName -match "ACCOUNTING_METHOD") {
            $primaryField = "ACCOUNTING_METHOD"
        }
        
        # Extract STATUS_TYPE from filename
        $statusType = $null
        if ($fileName -match "_PPDM") {
            $parts = ($fileName -split "_PPDM")[0] -split "_"
            if ($parts.Count -gt 1) {
                $statusType = $parts -join "_"
            }
        }
        
        # Build item object
        $item = @{}
        $item[$primaryField] = $name1
        
        if ($statusType -and $mappedTableName -match "STATUS") {
            $item["STATUS_TYPE"] = $statusType
        }
        
        $alias = $rowDict["ALIAS"]
        $name4 = $rowDict["NAME4"]
        
        if ($alias) {
            $item["ALIAS_ID"] = $alias
            $item["ALIAS_SHORT_NAME"] = $alias
        }
        elseif ($name4) {
            $item["ALIAS_ID"] = $name4
            $item["ALIAS_SHORT_NAME"] = $name4
        }
        
        if ($name4) {
            $item["ALIAS_LONG_NAME"] = $name4
        }
        elseif ($name1) {
            $item["ALIAS_LONG_NAME"] = $name1
        }
        
        if (-not $item.ContainsKey("ALIAS_SHORT_NAME")) {
            $item["ALIAS_SHORT_NAME"] = $name1
        }
        
        # ABBREVIATION
        $name2 = $rowDict["NAME2"]
        $name3 = $rowDict["NAME3"]
        if ($name2) {
            $item["ABBREVIATION"] = $name2
        }
        elseif ($name3) {
            $item["ABBREVIATION"] = $name3
        }
        elseif ($alias) {
            $item["ABBREVIATION"] = if ($alias.Length -gt 4) { $alias.Substring(0, 4).ToUpper() } else { $alias.ToUpper() }
        }
        else {
            $item["ABBREVIATION"] = if ($name1.Length -gt 4) { $name1.Substring(0, 4).ToUpper() } else { $name1.ToUpper() }
        }
        
        # ACTIVE_IND
        $valueStatus = $rowDict["VALUE_STATUS"]
        if ($valueStatus -match "Adopted") {
            $item["ACTIVE_IND"] = "Y"
        }
        elseif ($valueStatus -match "Deprecated") {
            $item["ACTIVE_IND"] = "N"
        }
        else {
            $item["ACTIVE_IND"] = "Y"
        }
        
        $item["PREFERRED_IND"] = "Y"
        $item["ORIGINAL_IND"] = "Y"
        
        # SOURCE
        $source = $rowDict["SOURCE"]
        $item["SOURCE"] = if ([string]::IsNullOrWhiteSpace($source)) { "PPDM" } else { $source }
        
        $items += $item
        $processedCount++
    }
    
    if ($items.Count -gt 0) {
        if (-not $csvTables.ContainsKey($mappedTableName)) {
            $csvTables[$mappedTableName] = @()
        }
        $csvTables[$mappedTableName] += $items
    }
}

Write-Host "Found $($csvTables.Count) unique tables in CSV data"
Write-Host "Processed $processedCount items"

# Get existing table names
$existingTables = @{}
foreach ($table in $referenceData.tables) {
    $existingTables[$table.tableName] = $table
}

Write-Host "Found $($existingTables.Count) existing tables in reference data"

# Consolidate
$newTables = @()
$updatedTables = @()

foreach ($tableName in $csvTables.Keys) {
    $csvItems = $csvTables[$tableName]
    
    if ($existingTables.ContainsKey($tableName)) {
        # Add to existing table
        $existingTable = $existingTables[$tableName]
        $existingData = $existingTable.data
        
        # Get existing keys
        $existingKeys = @{}
        foreach ($item in $existingData) {
            $primaryField = if ($item.STATUS) { "STATUS" } elseif ($item.PSObject.Properties.Name -match "TYPE") { ($item.PSObject.Properties.Name | Where-Object { $_ -match "TYPE" })[0] } else { "NAME" }
            $primaryValue = $item.$primaryField
            $statusType = $item.STATUS_TYPE
            $key = if ($statusType) { "$statusType`:$primaryValue" } else { $primaryValue }
            if ($key) { $existingKeys[$key] = $true }
        }
        
        # Add new items
        $added = 0
        foreach ($newItem in $csvItems) {
            $primaryField = if ($newItem.STATUS) { "STATUS" } elseif ($newItem.PSObject.Properties.Name -match "TYPE") { ($newItem.PSObject.Properties.Name | Where-Object { $_ -match "TYPE" })[0] } else { "NAME" }
            $primaryValue = $newItem.$primaryField
            $statusType = $newItem.STATUS_TYPE
            $key = if ($statusType) { "$statusType`:$primaryValue" } else { $primaryValue }
            
            if ($key -and -not $existingKeys.ContainsKey($key)) {
                $existingData += $newItem
                $existingKeys[$key] = $true
                $added++
            }
        }
        
        if ($added -gt 0) {
            $updatedTables += $tableName
            Write-Host "  Added $added new items to $tableName"
        }
    }
    else {
        # Add new table
        $newTable = @{
            tableName = $tableName
            description = "Reference data for $tableName"
            data = $csvItems
        }
        $referenceData.tables += $newTable
        $newTables += $tableName
        Write-Host "  Added new table $tableName with $($csvItems.Count) items"
    }
}

# Update description
$referenceData.description = "PPDM standard reference tables (RA_* tables) with comprehensive standard values. This file contains consolidated seed data from PPDMCSVData.json and additional priority tables."

Write-Host "`nSummary:"
Write-Host "  New tables added: $($newTables.Count)"
Write-Host "  Existing tables updated: $($updatedTables.Count)"

# Save
$json = $referenceData | ConvertTo-Json -Depth 100
[System.IO.File]::WriteAllText($referenceDataPath, $json, [System.Text.Encoding]::UTF8)
Write-Host "`nSaved to $referenceDataPath"

