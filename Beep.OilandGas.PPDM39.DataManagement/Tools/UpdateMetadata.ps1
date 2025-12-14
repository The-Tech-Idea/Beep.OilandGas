# PowerShell script to update PPDM39Metadata.json with correct SubjectArea and Module mappings
# Based on PPDM 3.9 Roadmaps

param(
    [string]$JsonPath = "Core\Metadata\PPDM39Metadata.json"
)

$ErrorActionPreference = "Stop"

Write-Host "PPDM 3.9 Metadata Mapper" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
Write-Host ""

# Resolve full path
$fullPath = Resolve-Path $JsonPath -ErrorAction SilentlyContinue
if (-not $fullPath) {
    $fullPath = Join-Path $PSScriptRoot "..\Core\Metadata\PPDM39Metadata.json"
    $fullPath = Resolve-Path $fullPath
}

if (-not (Test-Path $fullPath)) {
    Write-Host "Error: File not found: $fullPath" -ForegroundColor Red
    exit 1
}

Write-Host "Loading metadata from: $fullPath" -ForegroundColor Yellow

# Load JSON
$jsonContent = Get-Content $fullPath -Raw
$metadata = $jsonContent | ConvertFrom-Json

Write-Host "Loaded $($metadata.PSObject.Properties.Count) tables" -ForegroundColor Green
Write-Host ""

# Define mappings based on table name patterns
function Get-Mapping {
    param([string]$TableName)
    
    $tableUpper = $TableName.ToUpper()
    
    # Support Modules
    if ($tableUpper -match '^AREA(_|$)') { return @("Support Modules", "Areas") }
    if ($tableUpper -match '^SP(_|COMPONENT|DESC|LINE|ZONE|POINT|POLYGON|BOUNDARY)') { return @("Support Modules", "Spatial Locations") }
    if ($tableUpper -match '^SP_PARCEL') { return @("Support Modules", "Spatial Parcels") }
    if ($tableUpper -match '^ENT(_|ITLEMENT)') { return @("Support Modules", "Entitlements") }
    if ($tableUpper -match '^FIN(_|ANCE)') { return @("Support Modules", "Finances") }
    if ($tableUpper -match '^RATE_') { return @("Support Modules", "Rate Schedules") }
    if ($tableUpper -match '^SOURCE_DOC') { return @("Support Modules", "Source Document Bibliography") }
    if ($tableUpper -match '^CAT_ADDITIVE') { return @("Support Modules", "Additives") }
    if ($tableUpper -match '^(EQUIPMENT|CAT_EQUIP|EQUIPMENT_MAINTAIN|EQUIPMENT_MAINT|EQUIPMENT_SPEC)') { return @("Support Modules", "Equipment") }
    if ($tableUpper -match '^CS_') { return @("Support Modules", "Coordinate Systems") }
    if ($tableUpper -match '^(BUSINESS_ASSOCIATE|BA_)') { return @("Support Modules", "Business Associates") }
    if ($tableUpper -match '^(SUBSTANCE|Z_PRODUCT)') { return @("Support Modules", "Products and Substances") }
    if ($tableUpper -match '^PROJECT') { return @("Support Modules", "Projects") }
    if ($tableUpper -match '^WORK_ORDER') { return @("Support Modules", "Work Orders") }
    
    # Data Management & Units of Measure
    if ($tableUpper -match '^PPDM_(SYSTEM|TABLE|PROCEDURE|METRIC|PROPERTY|AUDIT|QUALITY|MAP|RULE|GROUP|CONSTRAINT|COLUMN|SCHEMA|CODE|SW_|CHECK|DOMAIN|QUANTITY|EXCEPTION|OBJECT)') { 
        return @("Data Management & Units of Measure", "Data Management") 
    }
    if ($tableUpper -match '^PPDM_(MEASUREMENT_SYSTEM|UNIT_OF_MEASURE|UOM_|UNIT_CONVERSION|DATA_STORE)') { 
        return @("Data Management & Units of Measure", "Unit of Measure") 
    }
    if ($tableUpper -match '^PPDM_VOL_MEAS_') { return @("Data Management & Units of Measure", "Volume Conversions") }
    if ($tableUpper -match '^(R_|RA_)') { return @("Data Management & Units of Measure", "Reference Table Management") }
    
    # Stratigraphy, Lithology & Sample Analysis
    if ($tableUpper -match '^LITH') { return @("Stratigraphy, Lithology & Sample Analysis", "Lithology") }
    if ($tableUpper -match '^(ECOZONE|PALEO_CLIMATE)') { return @("Stratigraphy, Lithology & Sample Analysis", "Ecozones and Environments") }
    if ($tableUpper -match '^FOSSIL') { return @("Stratigraphy, Lithology & Sample Analysis", "Paleontology") }
    if ($tableUpper -match '^PALEO_') { return @("Stratigraphy, Lithology & Sample Analysis", "Interpretation") }
    if ($tableUpper -match '^SAMPLE') { return @("Stratigraphy, Lithology & Sample Analysis", "Sample Management") }
    if ($tableUpper -match '^ANL') { return @("Stratigraphy, Lithology & Sample Analysis", "Sample Analysis") }
    if ($tableUpper -match '^STRAT') { return @("Stratigraphy, Lithology & Sample Analysis", "Stratigraphy") }
    
    # Production & Reserves
    if ($tableUpper -match '^FIELD') { return @("Production & Reserves", "Fields") }
    if ($tableUpper -match '^POOL') { return @("Production & Reserves", "Pools") }
    if ($tableUpper -match '^PDEN') { return @("Production & Reserves", "Production Reporting") }
    if ($tableUpper -match '^(PROD_STRING|PR_STR_)') { return @("Production & Reserves", "Production Strings") }
    if ($tableUpper -match '^(PROD_LEASE_UNIT|PR_LSE_UNIT_)') { return @("Production & Reserves", "Production Lease Units") }
    if ($tableUpper -match '^SPACING_UNIT') { return @("Production & Reserves", "Spacing Units") }
    if ($tableUpper -match '^FACILITY') { return @("Production & Reserves", "Production Facilities") }
    if ($tableUpper -match '^REPORT_HIER') { return @("Production & Reserves", "Reporting Hierarchies") }
    if ($tableUpper -match '^(RESERVE|RESENT)') { return @("Production & Reserves", "Reserves Reporting") }
    
    # Wells
    if ($tableUpper -match '^(WELL_LOG|WELL_MUD)') { return @("Wells", "Well Logs") }
    if ($tableUpper -match '^LEGAL_') { return @("Wells", "Legal Locations") }
    if ($tableUpper -match '^(WELL|WELLBORE)') { return @("Wells", "Wells") }
    
    # Product Management & Classifications
    if ($tableUpper -match '^CLASS_') { return @("Product Management & Classifications", "Classification Systems") }
    if ($tableUpper -match '^RM_') { return @("Product Management & Classifications", "Product and Information Management") }
    
    # Seismic
    if ($tableUpper -match '^SEIS') { return @("Seismic", "Seismic") }
    
    # Support Facilities
    if ($tableUpper -match '^SF_') { return @("Support Facilities", "Support Facilities") }
    
    # Operations Support
    if ($tableUpper -match '^APPLIC') { return @("Operations Support", "Applications") }
    if ($tableUpper -match '^CONSULT') { return @("Operations Support", "Consultations") }
    if ($tableUpper -match '^CONSENT') { return @("Operations Support", "Consents") }
    if ($tableUpper -match '^NOTIF') { return @("Operations Support", "Notifications") }
    if ($tableUpper -match '^CONTEST') { return @("Operations Support", "Contests") }
    if ($tableUpper -match '^DISPUTE') { return @("Operations Support", "Disputes") }
    if ($tableUpper -match '^NEGOTIATION') { return @("Operations Support", "Negotiations") }
    if ($tableUpper -match '^HSE_') { return @("Operations Support", "Health Safety & Environment") }
    
    # Land & Legal Management
    if ($tableUpper -match '^LAND_RIGHT') { return @("Land & Legal Management", "Land Rights") }
    if ($tableUpper -match '^LAND') { return @("Land & Legal Management", "Land") }
    if ($tableUpper -match '^CONT') { return @("Land & Legal Management", "Contracts") }
    if ($tableUpper -match '^OBLIG') { return @("Land & Legal Management", "Obligations") }
    if ($tableUpper -match '^INSTRUMENT') { return @("Land & Legal Management", "Instruments") }
    if ($tableUpper -match '^(INTEREST_SET|INT_SET)') { return @("Land & Legal Management", "Interest Sets") }
    if ($tableUpper -match '^REST') { return @("Land & Legal Management", "Restrictions") }
    
    # Default
    return @("Support Modules", "General")
}

# Update metadata
$updated = 0
$total = $metadata.PSObject.Properties.Count

foreach ($prop in $metadata.PSObject.Properties) {
    $tableName = $prop.Name
    $tableMeta = $prop.Value
    
    $mapping = Get-Mapping -TableName $tableName
    $subjectArea = $mapping[0]
    $module = $mapping[1]
    
    if ($tableMeta.SubjectArea -ne $subjectArea -or $tableMeta.Module -ne $module) {
        $oldSubjectArea = $tableMeta.SubjectArea
        $oldModule = $tableMeta.Module
        
        $tableMeta.SubjectArea = $subjectArea
        $tableMeta.Module = $module
        if ([string]::IsNullOrWhiteSpace($tableMeta.OriginalModule)) {
            $tableMeta.OriginalModule = $oldModule
        }
        
        $updated++
        if ($updated % 100 -eq 0) {
            Write-Host "Updated $updated tables..." -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "Updated $updated out of $total tables" -ForegroundColor Green
Write-Host ""

# Save updated metadata
Write-Host "Saving updated metadata..." -ForegroundColor Yellow
$updatedJson = $metadata | ConvertTo-Json -Depth 100
$updatedJson | Set-Content $fullPath -Encoding UTF8

Write-Host "Metadata file updated successfully!" -ForegroundColor Green

