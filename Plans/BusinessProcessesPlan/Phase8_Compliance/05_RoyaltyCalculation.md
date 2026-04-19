# Phase 8 — Royalty Calculation
## USA ONRR GOR Formula, Canada Alberta Crown Formula, Variance Reporting

---

## USA ONRR Royalty

### Gas-Oil Ratio (GOR) Approach for Condensate

Under ONRR rules, condensate on leases producing associated gas is classified by gas-oil ratio:

```
GOR = Gas Volume (MCF) / Oil+Condensate Volume (BBL)

If GOR >= 100,000 scf/bbl → product is GAS for royalty purposes
If GOR  < 100,000 scf/bbl → product is OIL/CONDENSATE for royalty purposes
```

### ONRR Royalty Calculation

```
Royalty = Royalty Rate × Gross Revenue(At Month Wellhead Value)

Where:
  Royalty Rate for federal onshore oil leases = 12.5% (statutory minimum)
  Royalty Rate for offshore oil leases = 18.75%
  Royalty Rate for gas = 12.5–18.75% per lease terms

Gross Revenue = Sales Volume × Reference Price (NYMEX monthly avg. if arms-length)
```

---

## IRoyaltyCalculationService Interface

```csharp
public interface IRoyaltyCalculationService
{
    Task<RoyaltySummary> CalculateUSARoyaltyAsync(
        string fieldId, int year, int month, string userId);

    Task<RoyaltySummary> CalculateAlbertaCrownRoyaltyAsync(
        string fieldId, int year, int quarter, string userId);

    Task<List<RoyaltyVariance>> GetVarianceHistoryAsync(
        string fieldId, int year);
}

public record RoyaltySummary(
    string FieldId, string Jurisdiction, string Period,
    double ProductionVolume, string ProductType, string VolumeUnit,
    double RoyaltyRate, double GrossRevenue, decimal DueRoyalty,
    decimal? PaidRoyalty, decimal? VarianceAmount);

public record RoyaltyVariance(
    string Period, string ObligationId, decimal DueRoyalty,
    decimal PaidRoyalty, decimal Variance, bool IsUnder);
```

---

## Canada — Alberta Crown Generic Royalty

### Modernized Royalty Framework (2017)

The Alberta MRF uses a sliding royalty rate schedule based on the monthly revenue per equivalent cubic metre of production:

```
1. Calculate monthly "r" value:
   r = WTI price (CAD/bbl) × 6.2898   (convert to CAD/m³)

2. Look up royalty rate in tier table:
   Pre-payout phase → r < $200/m³: rate = 5%
   Pre-payout phase → r ≥ $200/m³: rate = 5% + ((r - 200) × 0.0015)
   Post-payout → rates are higher; see AER MRF Tier Table

3. Royalty Due = Rate × (Gross Revenue − Prescribed Deductions)
   Prescribed Deductions = Operating cost per well tier × Production
```

### PPDM Storage for Alberta Crown Royalty

| Step | PPDM Column | Value |
|---|---|---|
| Record monthly r value | `OBLIG_PAYMENT.PAYMENT_NOTES` | JSON: `{"r_value": 245.3, "tier": "post_payout"}` |
| Record royalty rate used | `OBLIG_PAYMENT.PAYMENT_BASIS` | `"MRF_RATE_0.0775"` |
| Record due royalty | `OBLIG_PAYMENT.GROSS_AMT` | CAD amount |
| Record paid royalty | `OBLIG_PAYMENT.ACTUAL_AMT` | CAD amount |
| Record variance | `OBLIG_PAYMENT.VARIANCE_AMT` | Computed |

---

## Variance Reporting

If `VARIANCE_AMT > $500 (CAD/USD)` or `VARIANCE_AMT / GROSS_AMT > 5%`:

1. System sets `OBLIGATION.OBLIG_STATUS = 'VARIANCE_FLAGGED'`
2. Creates `NOTIFICATION` for `ComplianceOfficer` and `Manager`
3. Generates entry in audit log via `PPDMDataAccessAuditService`

---

## Royalty Dashboard Component

```razor
<!-- RoyaltyVariancePanel.razor -->
<MudTable Items="_variances" Dense="true">
    <HeaderContent>
        <MudTh>Period</MudTh><MudTh>Jurisdiction</MudTh>
        <MudTh>Due Royalty</MudTh><MudTh>Paid</MudTh>
        <MudTh>Variance</MudTh><MudTh>Status</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Period</MudTd>
        <MudTd>@context.Jurisdiction</MudTd>
        <MudTd>@context.DueRoyalty.ToString("C")</MudTd>
        <MudTd>@context.PaidRoyalty.ToString("C")</MudTd>
        <MudTd>
            <MudText Color="@(context.IsUnder ? Color.Error : Color.Success)">
                @(context.Variance >= 0 ? "+" : "")@context.Variance.ToString("C")
            </MudText>
        </MudTd>
        <MudTd><ProcessStateChip State="@context.Status" /></MudTd>
    </RowTemplate>
</MudTable>
```
