# PowerShell script to convert Excel files to CSV
# Requires Microsoft Excel to be installed
# Usage: .\ConvertExcelToCsv.ps1 [sourceFolder] [outputFolder]

param(
    [string]$SourceFolder = "C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\PPDM_DATA",
    [string]$OutputFolder = "C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\PPDM_DATA\CSV"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PPDM39 Excel to CSV Converter" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Validate source folder
if (-not (Test-Path $SourceFolder)) {
    Write-Host "ERROR: Source folder not found: $SourceFolder" -ForegroundColor Red
    exit 1
}

# Create output folder if it doesn't exist
if (-not (Test-Path $OutputFolder)) {
    New-Item -ItemType Directory -Path $OutputFolder -Force | Out-Null
    Write-Host "Created output folder: $OutputFolder" -ForegroundColor Green
}

# Get all Excel files
$excelFiles = Get-ChildItem -Path $SourceFolder -Filter "*.xlsx" -File

if ($excelFiles.Count -eq 0) {
    Write-Host "No Excel files found in: $SourceFolder" -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($excelFiles.Count) Excel files to convert" -ForegroundColor Green
Write-Host ""

# Try to create Excel COM object
try {
    Write-Host "Starting Excel..." -ForegroundColor Yellow
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    $excel.EnableEvents = $false
    Write-Host "Excel started successfully" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "ERROR: Could not start Excel. Make sure Microsoft Excel is installed." -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Helper function to export worksheet to CSV manually
function ExportWorksheetToCsvManually {
    param(
        [object]$Worksheet,
        [string]$CsvPath
    )
    
    $usedRange = $Worksheet.UsedRange
    if ($null -eq $usedRange) {
        throw "Worksheet has no data"
    }
    
    $rows = $usedRange.Rows.Count
    $cols = $usedRange.Columns.Count
    
    if ($rows -eq 0 -or $cols -eq 0) {
        throw "Worksheet has no data"
    }
    
    # Always skip first 3 rows (logo, disclaimer, copyright) - start from row 4
    $skipRows = 3
    $dataStartRow = $skipRows + 1  # Row 4 (skip rows 1, 2, 3)
    $dataRows = $rows - $skipRows
    
    if ($dataRows -le 0) {
        throw "No data rows after skipping first 3 rows. Total rows: $rows"
    }
    
    Write-Host "    Starting from row $dataStartRow (skipping first 3 rows)" -ForegroundColor Gray
    
    $csvContent = New-Object System.Text.StringBuilder
    
    # Write each row starting from dataStartRow (skip first 3 rows: logo, disclaimer, copyright)
    for ($row = $dataStartRow; $row -le $rows; $row++) {
        $rowValues = @()
        for ($col = 1; $col -le $cols; $col++) {
            $cell = $Worksheet.Cells.Item($row, $col)
            $value = $cell.Value2
            
            if ($null -eq $value) {
                $rowValues += ""
            }
            elseif ($value -is [DateTime]) {
                $rowValues += $value.ToString("yyyy-MM-dd HH:mm:ss")
            }
            else {
                # Escape quotes and wrap in quotes if contains comma, quote, or newline
                $strValue = $value.ToString()
                if ($strValue -match '[,"\r\n]') {
                    $strValue = $strValue.Replace('"', '""')
                    $rowValues += """$strValue"""
                }
                else {
                    $rowValues += $strValue
                }
            }
        }
        [void]$csvContent.AppendLine(($rowValues -join ","))
    }
    
    # Write to file
    [System.IO.File]::WriteAllText($CsvPath, $csvContent.ToString(), [System.Text.Encoding]::UTF8)
}

$successCount = 0
$errorCount = 0
$skippedCount = 0

foreach ($file in $excelFiles) {
    $fileName = $file.BaseName
    $filePath = $file.FullName
    $csvPath = Join-Path $OutputFolder "$fileName.csv"
    
    Write-Host "Processing: $fileName" -ForegroundColor Cyan
    
    try {
        # Open the workbook
        $workbook = $excel.Workbooks.Open($filePath, $false, $true) # Read-only, no update links
        
        # Process each worksheet
        $worksheetCount = $workbook.Worksheets.Count
        
        # Always look for "All Values" sheet (required)
        $worksheetsToProcess = @()
        $allValuesSheet = $null
        
        Write-Host "  Searching for 'All Values' sheet among $worksheetCount worksheets..." -ForegroundColor Gray
        
        for ($i = 1; $i -le $worksheetCount; $i++) {
            $ws = $workbook.Worksheets.Item($i)
            $wsName = $ws.Name.Trim()
            
            # Try multiple matching patterns for "All Values"
            if ($wsName -eq "All Values" -or 
                $wsName -eq "All values" -or 
                $wsName -eq "ALL VALUES" -or
                $wsName -match "^All\s+Values$" -or
                $wsName -match "^All\s+values$" -or
                $wsName -match "^ALL\s+VALUES$" -or
                $wsName -like "*All*Values*" -or
                $wsName -like "*all*values*") {
                $allValuesSheet = $ws
                Write-Host "  Found 'All Values' sheet: '$wsName'" -ForegroundColor Green
                break
            }
        }
        
        if ($null -ne $allValuesSheet) {
            Write-Host "  Processing only 'All Values' sheet (skipping first 3 rows)" -ForegroundColor Yellow
            $worksheetsToProcess = @($allValuesSheet)
        }
        else {
            Write-Host "  ⚠ No 'All Values' sheet found. Available sheets:" -ForegroundColor Yellow
            for ($i = 1; $i -le $worksheetCount; $i++) {
                $ws = $workbook.Worksheets.Item($i)
                Write-Host "    - '$($ws.Name)'" -ForegroundColor Gray
            }
            Write-Host "  ⚠ Skipping this file - 'All Values' sheet is required" -ForegroundColor Red
            $skippedCount++
            $workbook.Close($false)
            continue
        }
        
        # Process each worksheet
        foreach ($worksheet in $worksheetsToProcess) {
            $sheetName = $worksheet.Name
            
            # Check if worksheet has data
            $usedRange = $worksheet.UsedRange
            if ($null -eq $usedRange) {
                Write-Host "  ⚠ Worksheet '$sheetName' is empty, skipping..." -ForegroundColor Yellow
                continue
            }
            
            # Get actual data range
            $lastRow = $usedRange.Rows.Count
            $lastCol = $usedRange.Columns.Count
            
            if ($lastRow -eq 0 -or $lastCol -eq 0) {
                Write-Host "  ⚠ Worksheet '$sheetName' has no data, skipping..." -ForegroundColor Yellow
                continue
            }
            
            Write-Host "  Worksheet '$sheetName': $lastRow rows x $lastCol columns" -ForegroundColor Gray
            
            # Determine output file path
            # If we filtered to "All Values", use the base filename without sheet suffix
            $isAllValues = $sheetName -eq "All Values" -or 
                          $sheetName -eq "All values" -or 
                          $sheetName -eq "ALL VALUES" -or
                          $sheetName -match "^All\s+Values$" -or
                          $sheetName -match "^All\s+values$" -or
                          $sheetName -like "*All*Values*" -or
                          $sheetName -like "*all*values*"
            
            if ($worksheetCount -eq 1 -or ($worksheetCount -gt 1 -and $isAllValues)) {
                $sheetCsvPath = $csvPath
            }
            else {
                # Clean sheet name for filename (remove invalid characters)
                $cleanSheetName = $sheetName -replace '[\\/:*?"<>|]', '_'
                $sheetCsvPath = Join-Path $OutputFolder "${fileName}_${cleanSheetName}.csv"
            }
            
            # Convert worksheet to CSV
            try {
                # Clean the CSV path - ensure it's a valid file path
                $sheetCsvPath = [System.IO.Path]::GetFullPath($sheetCsvPath)
                
                # Ensure directory exists
                $csvDir = Split-Path $sheetCsvPath -Parent
                if (-not (Test-Path $csvDir)) {
                    New-Item -ItemType Directory -Path $csvDir -Force | Out-Null
                }
                
                # If we're only processing one worksheet (either single sheet or filtered to "All Values"),
                # use manual export method to skip first 3 rows
                if ($worksheetsToProcess.Count -eq 1) {
                    # Use manual export method to skip first 3 rows (logo, disclaimer, etc.)
                    Write-Host "  Using manual export method (skipping first 3 rows)..." -ForegroundColor Gray
                    ExportWorksheetToCsvManually $worksheet $sheetCsvPath
                }
                else {
                    # For multiple worksheets, create a temporary workbook with just this sheet
                    $tempWorkbook = $excel.Workbooks.Add()
                    
                    # Delete all default worksheets except the first one
                    while ($tempWorkbook.Worksheets.Count -gt 1) {
                        $tempWorkbook.Worksheets.Item($tempWorkbook.Worksheets.Count).Delete()
                    }
                    
                    # Get the first (and only) worksheet in temp workbook
                    $tempWorksheet = $tempWorkbook.Worksheets.Item(1)
                    
                    # Copy the source worksheet to the temp workbook
                    # This will insert the copied sheet BEFORE the tempWorksheet
                    $worksheet.Copy($tempWorksheet)
                    
                    # After Copy, the copied sheet is at index 1, original is at index 2
                    # Get the copied worksheet (now at index 1)
                    $copiedWorksheet = $tempWorkbook.Worksheets.Item(1)
                    
                    # Verify it's the copied one (should have different name or we can check)
                    # Delete the original default worksheet (now at index 2)
                    if ($tempWorkbook.Worksheets.Count -gt 1) {
                        try {
                            $tempWorkbook.Worksheets.Item(2).Delete()
                        }
                        catch {
                            # If deletion fails, try to identify which is which
                            # The copied one should have the original name or be at index 1
                            # Just keep both and use the first one
                        }
                    }
                    
                    # Ensure we have at least one worksheet
                    if ($tempWorkbook.Worksheets.Count -eq 0) {
                        throw "No worksheets in temp workbook after copy operation"
                    }
                    
                    # Use the first worksheet (should be our copied one)
                    $finalWorksheet = $tempWorkbook.Worksheets.Item(1)
                    $finalWorksheet.Name = $sheetName
                    
                    # Activate the worksheet (required for SaveAs)
                    $finalWorksheet.Activate()
                    
                    # Save as CSV format (6 = xlCSV)
                    $tempWorkbook.SaveAs($sheetCsvPath, 6, $null, $null, $false, $false, 1, 2)
                    $tempWorkbook.Close($false)
                }
                
                # Verify the CSV file was created and has content
                Start-Sleep -Milliseconds 500 # Give file system time to write
                
                if (Test-Path $sheetCsvPath) {
                    $csvSize = (Get-Item $sheetCsvPath).Length
                    if ($csvSize -gt 0) {
                        Write-Host "  ✓ Converted to: $(Split-Path $sheetCsvPath -Leaf) ($csvSize bytes)" -ForegroundColor Green
                        $successCount++
                    }
                    else {
                        Write-Host "  ✗ CSV file created but is empty" -ForegroundColor Red
                        $errorCount++
                    }
                }
                else {
                    Write-Host "  ✗ CSV file was not created" -ForegroundColor Red
                    $errorCount++
                }
            }
            catch {
                Write-Host "  ✗ Error saving worksheet '$sheetName': $($_.Exception.Message)" -ForegroundColor Red
                Write-Host "    Error Code: $($_.Exception.ErrorCode)" -ForegroundColor DarkGray
                Write-Host "    HRESULT: 0x$($_.Exception.HResult.ToString('X'))" -ForegroundColor DarkGray
                
                # Try alternative method: Export to CSV manually
                try {
                    Write-Host "    Attempting alternative export method..." -ForegroundColor Yellow
                    ExportWorksheetToCsvManually $worksheet $sheetCsvPath
                    Write-Host "    ✓ Alternative method succeeded" -ForegroundColor Green
                    $successCount++
                }
                catch {
                    Write-Host "    ✗ Alternative method also failed: $($_.Exception.Message)" -ForegroundColor Red
                    $errorCount++
                }
            }
        }
        
        # Close the workbook if it's still open (shouldn't be for single sheet, but just in case)
        if ($null -ne $workbook) {
            try {
                $workbook.Close($false)
            }
            catch {
                # Workbook might already be closed, ignore
            }
        }
    }
    catch {
        Write-Host "  ✗ Error converting $fileName : $($_.Exception.Message)" -ForegroundColor Red
        $errorCount++
    }
    
    Write-Host ""
}

# Clean up Excel
try {
    $excel.Quit()
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    Write-Host "Excel closed" -ForegroundColor Green
}
catch {
    Write-Host "Warning: Error closing Excel: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Conversion Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Successfully converted: $successCount" -ForegroundColor Green
Write-Host "  Errors: $errorCount" -ForegroundColor $(if ($errorCount -eq 0) { "Green" } else { "Red" })
Write-Host "  Skipped: $skippedCount" -ForegroundColor Yellow
Write-Host ""
Write-Host "CSV files saved to: $OutputFolder" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

