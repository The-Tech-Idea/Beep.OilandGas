# Phase 6 — Cost Capture
## AFE Integration, Cost Code Lines, and Budget Variance Tracking

---

## PPDM Tables

| Table | Key Columns | Purpose |
|---|---|---|
| `FINANCE` | `FINANCE_ID`, `PROJECT_ID`, `BUDGET_AMT`, `ACTUAL_AMT`, `FINANCE_TYPE` | AFE / budget record |
| `FIN_COMPONENT` | `FINANCE_ID`, `COMP_CODE`, `BUDGET_AMT`, `ACTUAL_AMT`, `COMP_DESC` | Cost code line items |

---

## ICostCaptureService Interface

```csharp
public interface ICostCaptureService
{
    /// <summary>
    /// Create or update the AFE finance record for a work order.
    /// </summary>
    Task<FinanceRecord> UpsertAFEAsync(string instanceId, decimal budgetAmount, string userId);

    /// <summary>
    /// Add a cost code line item to a work order's AFE.
    /// </summary>
    Task<FinComponentRecord> AddCostLineAsync(string financeId, string compCode,
        decimal budgetAmt, string description, string userId);

    /// <summary>
    /// Record actual cost against a cost line (called when work step is completed).
    /// </summary>
    Task UpdateActualCostAsync(string financeId, string compCode, decimal actualAmt, string userId);

    /// <summary>
    /// Return variance summary: % over/under budget per cost code.
    /// </summary>
    Task<List<CostVarianceLine>> GetVarianceSummaryAsync(string instanceId);
}

public record CostVarianceLine(string CompCode, string Description,
    decimal BudgetAmt, decimal ActualAmt, decimal VariancePct);
```

---

## AFE Guard Rule

Work orders with total cost estimate > $50,000 USD require an AFE before transition to `IN_PROGRESS`:

```csharp
machine.Configure(PLANNED)
    .Permit(START_TRIGGER, IN_PROGRESS)
    .Guard(() => _costService.HasAFEAsync(instanceId).GetAwaiter().GetResult() ||
                 _costService.GetEstimatedCostAsync(instanceId).GetAwaiter().GetResult() <= 50_000m,
           "AFE (FINANCE record) required for WOs with estimated cost > $50,000");
```

---

## Cost Code Standard

Use USACE cost codes as default (override per jurisdiction):

| Code | Description | Typical WO Types |
|---|---|---|
| `01-LABOR` | Labor costs | All |
| `02-MATERIAL` | Parts and materials | All |
| `03-CONTRACTOR` | Contractor services | Preventive, Corrective, Turnaround |
| `04-EQUIPMENT` | Rental or owned equipment | Turnaround |
| `05-ENVIRONMENTAL` | Environmental compliance cost | Environmental |
| `06-REGULATORY` | Regulatory filing fees | Regulatory Inspection |
| `07-OTHER` | Miscellaneous | All |

---

## AFE Variance Tracking

```csharp
public async Task<List<CostVarianceLine>> GetVarianceSummaryAsync(string instanceId)
{
    var financeRepo  = BuildRepo("FINANCE");
    var financeRow   = (await financeRepo.GetAsync(ByProjectId(instanceId))).FirstOrDefault();
    if (financeRow == null) return [];

    var compRepo = BuildRepo("FIN_COMPONENT");
    var lines    = await compRepo.GetAsync(ByFinanceId(((dynamic)financeRow).FINANCE_ID));

    return lines
        .Select(l => (dynamic)l)
        .Select(l =>
        {
            decimal budget   = (decimal)(l.BUDGET_AMT ?? 0m);
            decimal actual   = (decimal)(l.ACTUAL_AMT ?? 0m);
            decimal variance = budget == 0 ? 0 : ((actual - budget) / budget) * 100;
            return new CostVarianceLine(
                (string)l.COMP_CODE,
                (string)l.COMP_DESC,
                budget, actual,
                Math.Round(variance, 2));
        })
        .ToList();
}
```

---

## Blazor Cost Summary Component

```razor
<!-- CostVariancePanel.razor -->
[Parameter] public string InstanceId { get; set; }

<MudTable Items="_lines" Dense="true">
    <HeaderContent>
        <MudTh>Code</MudTh>
        <MudTh>Description</MudTh>
        <MudTh>Budget</MudTh>
        <MudTh>Actual</MudTh>
        <MudTh>Variance</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.CompCode</MudTd>
        <MudTd>@context.Description</MudTd>
        <MudTd>@context.BudgetAmt.ToString("C")</MudTd>
        <MudTd>@context.ActualAmt.ToString("C")</MudTd>
        <MudTd>
            <MudText Color="@(context.VariancePct > 0 ? Color.Error : Color.Success)">
                @context.VariancePct.ToString("F1")%
            </MudText>
        </MudTd>
    </RowTemplate>
</MudTable>
```
