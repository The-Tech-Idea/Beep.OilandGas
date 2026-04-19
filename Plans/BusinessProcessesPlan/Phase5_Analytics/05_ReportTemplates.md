# Phase 5 — Report Templates
## PDF and Excel Export Definitions

---

## Reports Catalog

| Report ID | Name | Format | Trigger | Frequency |
|---|---|---|---|---|
| RPT-WO-01 | Work Order Summary | PDF + XLSX | Manual / weekly scheduler | Weekly |
| RPT-HSE-01 | HSE KPI Report | PDF | Manual / monthly | Monthly |
| RPT-COMP-01 | Compliance Obligation Status | PDF + XLSX | Manual / monthly | Monthly |
| RPT-PROD-01 | Monthly Production Summary | PDF + XLSX | Manual / monthly | Monthly |

---

## RPT-WO-01: Work Order Summary

### Columns (XLSX)

| Column | Source Field | Format |
|---|---|---|
| Instance ID | `ProcessInstanceResponse.InstanceId` | Text |
| Process Name | `ProcessInstanceResponse.ProcessId` | Text |
| State | `ProcessInstanceResponse.CurrentState` | Text |
| Entity | `ProcessInstanceResponse.EntityId` | Text |
| Jurisdiction | `ProcessInstanceResponse.Jurisdiction` | Text |
| Started | `ProcessInstanceResponse.StartDate` | `yyyy-MM-dd` |
| Completed | Latest complete audit record date | `yyyy-MM-dd` or blank |
| Duration (days) | Computed from Start→Complete | Number |
| On Time | `Completed ≤ ScheduledEnd` | Yes/No |
| Contractor | `PROJECT_STEP_BA.BA_ID` | Text |

### PDF Layout

```
Header: Field Name | Reporting Period | Generated timestamp
Title:  Work Order Summary Report

Section 1: KPI Summary (4 boxes in a row)
   [WO-01 Completion %]  [WO-02 On-Time %]  [WO-03 MTTC hrs]  [WO-04 Overdue count]

Section 2: Work Order Table (full list — columns as above)

Footer: Page n of N | Beep Oil & Gas | CONFIDENTIAL
```

---

## RPT-HSE-01: HSE KPI Report

### PDF Layout

```
Header: Field Name | Reporting Period | Generated timestamp
Title:  HSE Key Performance Indicators

Section 1: Exposure Basis
   Total exposure hours: {exposureHours}

Section 2: IOGP-Aligned KPIs (table)
   | KPI Name                 | Count | Rate     | Benchmark |
   | Tier 1 PSE               | n     | x.xx/MH  | IOGP 2022e|
   | Tier 2 PSE               | n     | x.xx/MH  | IOGP 2022e|
   | Total Recordable Injury  | n     | x.xx/MH  | OSHA 300  |
   | Near Miss Reporting Rate | x%    | —        | —         |

Section 3: Open Corrective Actions
   Table: Incident | CA Description | Responsible | Due Date | Status

Footer: Page n of N | CONFIDENTIAL
```

---

## Export Service Interface

```csharp
public interface IReportExportService
{
    Task<byte[]> GenerateWorkOrderSummaryPdfAsync(string fieldId, DateRangeFilter range);
    Task<byte[]> GenerateWorkOrderSummaryXlsxAsync(string fieldId, DateRangeFilter range);
    Task<byte[]> GenerateHSEKPIReportPdfAsync(string fieldId, DateRangeFilter range, double exposureHours);
    Task<byte[]> GenerateComplianceStatusXlsxAsync(string fieldId, DateRangeFilter range);
    Task<byte[]> GenerateProductionSummaryXlsxAsync(string fieldId, DateRangeFilter range);
}
```

### Recommended Libraries

| Format | Library | NuGet |
|---|---|---|
| PDF | QuestPDF (MIT) | `QuestPDF` |
| XLSX | ClosedXML | `ClosedXML` |

---

## API Endpoint for Export

```csharp
[HttpGet("reports/work-orders/pdf")]
[Produces("application/pdf")]
public async Task<FileResult> DownloadWOSummaryPdfAsync([FromQuery] int days = 90)
{
    var fieldId = _fieldOrchestrator.CurrentFieldId;
    var range   = new DateRangeFilter(DateTime.UtcNow.AddDays(-days), DateTime.UtcNow);
    var bytes   = await _exportService.GenerateWorkOrderSummaryPdfAsync(fieldId, range);

    return File(bytes, "application/pdf",
        $"WO-Summary-{fieldId}-{DateTime.UtcNow:yyyyMMdd}.pdf");
}
```

---

## Blazor Download Button

```razor
<MudButton OnClick="DownloadAsync" StartIcon="@Icons.Material.Filled.Download">
    Download XLSX
</MudButton>

@code {
    private async Task DownloadAsync()
    {
        var url = $"/api/field/current/analytics/reports/work-orders/xlsx?days={_days}";
        await JSRuntime.InvokeVoidAsync("open", url, "_blank");
    }
}
```
