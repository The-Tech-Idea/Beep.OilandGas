# Phase 8 — GHG Reporting
## EPA 40 CFR 98 (Subparts W/C/NN), ECCC NIR Appendix A, Emission Factor Lookup

---

## GHG Reporting Framework

| Jurisdiction | Framework | Key Subpart / Protocol | PPDM Volume Basis |
|---|---|---|---|
| USA | EPA 40 CFR 98 Subpart W | Petroleum and natural gas systems | `PDEN_VOL_DISPOSITION` |
| USA | EPA 40 CFR 98 Subpart C | General stationary combustion | `EQUIPMENT.FUEL_CONSUMPTION` |
| USA | EPA 40 CFR 98 Subpart NN | Suppliers of natural gas / LNG | `GAS_SALES` |
| Canada | ECCC NIR Appendix A | Oil and gas industry protocol | `PDEN_VOL_DISPOSITION` |
| International | IOGP GHG reporting | Voluntary; align with GHG Protocol | Same sources |

---

## Emission Factor Lookup Table

Stored as a configuration file `EmissionFactors.json` deployed with the API:

```json
{
  "factors": [
    { "sourceCategory": "FLARE_GAS", "jurisdiction": "USA", "regulation": "40CFR98_W",
      "factor": 1.9959, "unit": "t_CO2e_per_MSCF" },
    { "sourceCategory": "VENTED_GAS", "jurisdiction": "USA", "regulation": "40CFR98_W",
      "factor": 0.05306, "unit": "t_CO2e_per_MSCF" },
    { "sourceCategory": "DIESEL_COMBUSTION", "jurisdiction": "USA", "regulation": "40CFR98_C",
      "factor": 0.002785, "unit": "t_CO2e_per_litre" },
    { "sourceCategory": "FLARE_GAS", "jurisdiction": "CANADA", "regulation": "ECCC_NIR",
      "factor": 2.18, "unit": "t_CO2e_per_1000m3" },
    { "sourceCategory": "VENTED_GAS", "jurisdiction": "CANADA", "regulation": "ECCC_NIR",
      "factor": 1.22, "unit": "t_CO2e_per_1000m3" }
  ]
}
```

---

## IGHGReportingService Interface

```csharp
public interface IGHGReportingService
{
    Task<GHGEmissionReport> GenerateAnnualReportAsync(
        string fieldId, int year, string jurisdiction, string userId);

    Task<List<EmissionSourceLine>> GetEmissionSourcesAsync(
        string fieldId, int year);

    Task<double> GetTotalEmissionsAsync(
        string fieldId, int year, string? jurisdiction = null);
}

public record GHGEmissionReport(
    string FieldId, int Year, string Jurisdiction,
    double TotalCO2e, string Units,
    List<EmissionSourceLine> Sources,
    DateTime GeneratedAt);

public record EmissionSourceLine(
    string SourceCategory, string Regulation,
    double ActivityVolume, string VolumeUnit,
    double EmissionFactor, string FactorUnit,
    double EmissionsCO2e);
```

---

## GHG Calculation Flow

```csharp
// GHGReportingService.GenerateAnnualReportAsync
var flareVolume = await GetFlareVolumeAsync(fieldId, year);   // MSCF/1000m³
var ventVolume  = await GetVentVolumeAsync(fieldId, year);
var dieselConsumption = await GetDieselVolumeAsync(fieldId, year);  // Litres

var flareFactor = _factorLookup.Get("FLARE_GAS", jurisdiction);
var ventFactor  = _factorLookup.Get("VENTED_GAS", jurisdiction);
var dieselFactor = _factorLookup.Get("DIESEL_COMBUSTION", jurisdiction);

var sources = new List<EmissionSourceLine>
{
    new("FLARE_GAS", flareFactor.Regulation, flareVolume, flareFactor.VolumeUnit,
        flareFactor.Factor, flareFactor.Unit, flareVolume * flareFactor.Factor),
    new("VENTED_GAS", ventFactor.Regulation, ventVolume, ventFactor.VolumeUnit,
        ventFactor.Factor, ventFactor.Unit, ventVolume * ventFactor.Factor),
    new("DIESEL_COMBUSTION", dieselFactor.Regulation, dieselConsumption, "litres",
        dieselFactor.Factor, dieselFactor.Unit, dieselConsumption * dieselFactor.Factor)
};

double totalCO2e = sources.Sum(s => s.EmissionsCO2e);
```

---

## PPDM Storage for GHG Data

| PPDM Column | Table | GHG Meaning |
|---|---|---|
| `DISP_VOL` | `PDEN_VOL_DISPOSITION` | Activity volume (flare/vent) |
| `DISP_TYPE` | `PDEN_VOL_DISPOSITION` | `'FLARE'` or `'VENT'` |
| `FUEL_CONSUMPTION` | `EQUIPMENT` | Diesel/fuel consumed (litres) |
| `EQUIP_TYPE` | `EQUIPMENT` | `'COMBUSTION_ENGINE'` / `'FLARE'` / `'SEPARATOR'` |
| `PPDM_GUID` of obligation | `OBLIGATION` | Where `OBLIG_TYPE='EPA_GHG_REPORT_FLARE'` |

---

## GHG Report UI (Blazor)

```razor
<!-- GHGReportPage.razor -->
@page "/ppdm39/compliance/ghg"

<MudSelect @bind-Value="_year" Label="Reporting Year">
    @for (int y = DateTime.Today.Year - 3; y <= DateTime.Today.Year; y++)
    { <MudSelectItem Value="y">@y</MudSelectItem> }
</MudSelect>
<MudSelect @bind-Value="_jurisdiction" Label="Jurisdiction">
    <MudSelectItem Value='"USA"'>USA (40 CFR 98)</MudSelectItem>
    <MudSelectItem Value='"CANADA"'>Canada (ECCC NIR)</MudSelectItem>
</MudSelect>

<MudButton OnClick="GenerateReport" Variant="Variant.Filled">Generate Report</MudButton>

@if (_report != null)
{
    <MudText Typo="Typo.h6">Total: @_report.TotalCO2e.ToString("N1") t CO2e</MudText>
    <MudTable Items="_report.Sources" Dense="true">
        <!-- Source Category | Volume | Factor | Emissions (t CO2e) -->
    </MudTable>
    <MudButton OnClick="ExportCsv" StartIcon="@Icons.Material.Filled.Download">Export CSV</MudButton>
}
```
