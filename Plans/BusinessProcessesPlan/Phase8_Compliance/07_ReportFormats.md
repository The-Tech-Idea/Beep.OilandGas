# Phase 8 — Report Formats
## EIA-914, OGOR, AER ST-39 Column Mappings and Export Templates

---

## EIA-914 Monthly Natural Gas Production Report

Monthly gas production for US operators reported to EIA.

| EIA-914 Field | Width | PPDM Column | Table |
|---|---|---|---|
| OPERATOR_CODE | 6 | `BA_ID` of operator | `BUSINESS_ASSOCIATE` |
| STATE_CODE | 2 | `SURFACE_STATE` | `WELL` |
| COUNTY_CODE | 3 | `SURFACE_COUNTY` | `WELL` |
| PRODUCT_TYPE | 1 | `G`=gas, `L`=NGL, `O`=condensate | Derived |
| PRODUCTION_VOLUME (MMCF) | 9.2 | `PROD_VOLUME` (convert MSCF÷1000) | `PDEN_VOL_SUMMARY` |
| REPORT_MONTH | YYYYMM | Report period | — |

---

## OGOR — Oil and Gas Operations Report (BSEE)

Monthly report for OCS operators. Three-part form: OGOR-A (oil), OGOR-B (gas), OGOR-C (water and injected fluids).

### OGOR-A — Oil Production

| OGOR-A Field | Description | PPDM Column |
|---|---|---|
| LEASE_NUMBER | OCS lease identifier | `CONTRACT.CONTRACT_ID` |
| COMPLEX_ID | Facility complex code | `FACILITY.FACILITY_CODE` |
| API_WELL_NO | 14-digit API well number | `WELL.API_WELL_NO` |
| OIL_PROD (Bbls) | Oil produced | `PDEN_VOL_SUMMARY.PROD_VOLUME WHERE PROD_TYPE='OIL'` |
| OIL_SOLD (Bbls) | Oil sold | `PDEN_VOL_DISPOSITION.DISP_VOL WHERE DISP_TYPE='SALES'` |
| WATER_PRODUCED | Produced water | `PDEN_VOL_SUMMARY.PROD_VOLUME WHERE PROD_TYPE='WATER'` |
| GOR (scf/bbl) | Calculated | Gas prod (MCF×1000) ÷ Oil prod |

### OGOR-B — Gas Production

| OGOR-B Field | Description | PPDM Column |
|---|---|---|
| GAS_PROD (MCF) | Gas produced | `PDEN_VOL_SUMMARY WHERE PROD_TYPE='GAS'` |
| GAS_FLARED (MCF) | Gas flared | `PDEN_VOL_DISPOSITION WHERE DISP_TYPE='FLARE'` |
| GAS_VENTED (MCF) | Gas vented | `PDEN_VOL_DISPOSITION WHERE DISP_TYPE='VENT'` |
| GAS_FUEL (MCF) | Gas used as fuel | `PDEN_VOL_DISPOSITION WHERE DISP_TYPE='FUEL'` |
| GAS_SOLD (MCF) | Sales gas | `PDEN_VOL_DISPOSITION WHERE DISP_TYPE='SALES'` |

---

## AER ST-39 — Alberta Monthly Oil Production Report

CSV format submitted via Petrinex or AER eSubmission portal.

| Column # | Field | PPDM Source | Conversion |
|---|---|---|---|
| 1 | Unique Well ID (UWI) | `WELL.UWI` | No conversion; 16 chars |
| 2 | Status Code | `WELL.WELL_STATUS` | Map to AER 2-char code |
| 3 | Monthly Oil Production (m³) | `PDEN_VOL_SUMMARY.PROD_VOLUME` WHERE `PROD_TYPE='OIL'` | Multiply BBL × 0.158987 |
| 4 | Monthly Water Production (m³) | Same table `PROD_TYPE='WATER'` | Same conversion |
| 5 | Hours Producing | `PDEN_VOL_SUMMARY.HOURS_ON` | Integer |
| 6 | Method of Lifting | `WELL.LIFT_TYPE` | AER code mapping |
| 7 | Report Period (YYYYMM) | Obligation period | — |

---

## IReportExportService (Extended)

```csharp
// Added to existing IReportExportService from Phase 5
Task<byte[]> ExportEIA914CsvAsync(string fieldId, int year, int month);
Task<byte[]> ExportOGORAsync(string fieldId, int year, int month, string part);  // A, B, or C
Task<byte[]> ExportAERST39CsvAsync(string fieldId, int year, int month);
Task<byte[]> ExportAERST60CsvAsync(string fieldId, int year, int month);
```

---

## API Endpoints

```
GET  /api/compliance/reports/eia914/{fieldId}/{year}/{month}    → byte[] CSV
GET  /api/compliance/reports/ogor/{fieldId}/{year}/{month}/{part} → byte[] CSV
GET  /api/compliance/reports/aer-st39/{fieldId}/{year}/{month}  → byte[] CSV
GET  /api/compliance/reports/aer-st60/{fieldId}/{year}/{month}  → byte[] CSV
```

All require `[Authorize(Roles = "ComplianceOfficer,Manager")]`.

---

## Blazor Download Button Pattern

```razor
<MudButton OnClick="DownloadEIA914"
           StartIcon="@Icons.Material.Filled.FileDownload"
           Variant="Variant.Outlined">
    EIA-914 CSV
</MudButton>

@code {
    async Task DownloadEIA914()
    {
        var bytes = await ApiClient.GetBytesAsync(
            $"/api/compliance/reports/eia914/{FieldId}/{Year}/{Month}");
        await JsRuntime.InvokeVoidAsync("downloadFile",
            $"EIA914-{FieldId}-{Year}{Month:D2}.csv", bytes);
    }
}
```
