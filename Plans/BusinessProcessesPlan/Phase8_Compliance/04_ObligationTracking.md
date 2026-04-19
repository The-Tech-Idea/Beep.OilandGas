# Phase 8 — Obligation Tracking
## OBLIGATION + OBLIG_PAYMENT Deep Dive, Escalation, and IComplianceService

---

## OBLIGATION Table — Full Column Mapping

| Column | Type | Purpose | Example |
|---|---|---|---|
| `OBLIGATION_ID` | VARCHAR | PK; formatted `OBL-{FIELD_ID}-{YYYYMMDD}-{SEQ}` | `OBL-PERMIAN-20250115-001` |
| `FIELD_ID` | VARCHAR | FK; partition key | `PERMIAN` |
| `OBLIG_TYPE` | VARCHAR | LOV from `R_OBLIG_TYPE` | `ONRR_PRODUCTION_REPORT` |
| `OBLIG_STATUS` | VARCHAR | State machine state | `PENDING`, `SUBMITTED`, `OVERDUE`, `WAIVED` |
| `DUE_DATE` | DATE | Regulatory deadline | `2025-02-15` |
| `FULFILL_DATE` | DATE | Actual submission date | Null until submitted |
| `JURISDICTION_CODE` | VARCHAR | `USA` / `CANADA` / `OSPAR` etc. | `USA` |
| `REGULATOR_BA_ID` | VARCHAR | FK to BUSINESS_ASSOCIATE | `ONRR-GOV` |
| `DESCRIPTION` | VARCHAR | Human-readable obligation description | |
| `REPORTING_PERIOD_START` | DATE | Period covered by this obligation | |
| `REPORTING_PERIOD_END` | DATE | | |
| `CREATED_BY_PROCESS` | VARCHAR | ProcessInstanceId that triggered creation | `PROCESS-HSE-123` |
| `ACTIVE_IND` | CHAR(1) | Auto `'Y'`; set to `'N'` on waiver | |
| `ROW_CREATED_BY` | VARCHAR | Auto via `CommonColumnHandler` | |

---

## OBLIG_PAYMENT Table — Financial Records

| Column | Type | Purpose |
|---|---|---|
| `PAYMENT_ID` | VARCHAR | PK |
| `OBLIGATION_ID` | VARCHAR | FK |
| `PAYMENT_TYPE` | VARCHAR | `ROYALTY`, `FINE`, `EU_ETS_ALLOWANCE`, etc. |
| `PAYMENT_DATE` | DATE | |
| `PAYMENT_CURRENCY` | VARCHAR | `USD`, `CAD`, `EUR`, `EUA` |
| `GROSS_AMT` | DECIMAL | Amount calculated / owed |
| `ACTUAL_AMT` | DECIMAL | Amount actually paid |
| `VARIANCE_AMT` | DECIMAL | `GROSS_AMT - ACTUAL_AMT` |
| `PAYMENT_NOTES` | VARCHAR | Reference number, receipt ID |

---

## IComplianceService Interface

```csharp
public interface IComplianceService
{
    Task<string> CreateObligationAsync(CreateObligationRequest request, string userId);

    Task MarkSubmittedAsync(string obligationId, DateTime submitDate, string userId);

    Task WaiveObligationAsync(string obligationId, string reason, string userId);

    Task<List<ObligationSummary>> GetUpcomingObligationsAsync(
        string fieldId, int daysAhead = 30);

    Task<List<ObligationSummary>> GetOverdueObligationsAsync(string fieldId);

    Task<ComplianceScoreCard> GetComplianceScoreAsync(string fieldId, int year);
}

public record CreateObligationRequest(
    string FieldId, string ObligType, DateTime DueDate,
    string Jurisdiction, string Description,
    DateTime? ReportingPeriodStart = null, DateTime? ReportingPeriodEnd = null,
    string? CreatedByProcess = null);

public record ObligationSummary(
    string ObligationId, string ObligType, string Status,
    DateTime DueDate, bool IsOverdue, int DaysUntilDue,
    string Jurisdiction, string? FulfillDate);

public record ComplianceScoreCard(
    int Year, int TotalObligations, int SubmittedOnTime, int SubmittedLate,
    int Overdue, int Waived, double OnTimeRate);
```

---

## Escalation Logic

Background `IHostedService` runs daily at 06:00 UTC:

```
1. Query OBLIGATION WHERE OBLIG_STATUS='PENDING' AND DUE_DATE < TODAY
   → Set OBLIG_STATUS='OVERDUE'
   → Create NOTIFICATION to ComplianceOfficer

2. Query OBLIGATION WHERE OBLIG_STATUS='PENDING'
   AND DUE_DATE BETWEEN TODAY+1 AND TODAY+7
   → Create reminder NOTIFICATION to ComplianceOfficer

3. Query OBLIG_PAYMENT WHERE VARIANCE_AMT > 0.01
   → Create variance alert NOTIFICATION to Accountant
```

---

## Compliance Dashboard (Blazor)

```razor
@page "/ppdm39/compliance/dashboard"

<!-- Compliance Score Card -->
<MudGrid>
    <MudItem xs="3">
        <KPICard Title="On-Time Rate" Value="@($"{_score?.OnTimeRate:F1}%")" 
                 Color="@OnTimeColor(_score?.OnTimeRate)" />
    </MudItem>
    <MudItem xs="3">
        <KPICard Title="Overdue" Value="@_overdue?.Count.ToString()" Color="Color.Error" />
    </MudItem>
    <MudItem xs="3">
        <KPICard Title="Due Next 30 Days" Value="@_upcoming?.Count.ToString()" />
    </MudItem>
</MudGrid>

<!-- Obligation grid with jurisdictions and due dates -->
<MudDataGrid Items="_obligations" Filterable="true" SortMode="SortMode.Multiple">
    <Columns>
        <PropertyColumn Property="x => x.ObligType" Title="Type" />
        <PropertyColumn Property="x => x.Jurisdiction" Title="Jurisdiction" />
        <PropertyColumn Property="x => x.DueDate.ToString("yyyy-MM-dd")" Title="Due" />
        <TemplateColumn Title="Status">
            <CellTemplate>
                <ProcessStateChip State="@context.Item.Status" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
</MudDataGrid>
```
