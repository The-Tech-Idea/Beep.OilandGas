# Phase 8 — International Regulatory Requirements
## OSPAR, IOGP Guidelines, IMO MARPOL, EU ETS, Jurisdiction Routing

---

## Jurisdiction Routing Logic

The `Jurisdiction` column on `HSE_INCIDENT` and `OBLIGATION` drives which regulatory notification rules apply:

```csharp
public static class JurisdictionRules
{
    public static IEnumerable<string> GetRequiredObligationTypes(
        string jurisdiction, string operationalEvent)
    {
        return (jurisdiction, operationalEvent) switch
        {
            ("USA", "SPILL") => new[] { "NRC_REPORT", "BSEE_REPORT" },
            ("CANADA", "SPILL") => new[] { "AER_SPILL_REPORT", "ECCC_REPORTABLE_RELEASE" },
            ("INTERNATIONAL", "SPILL") => new[] { "OSPAR_OFFSHORE_REPORT" },
            ("INTERNATIONAL", "VESSEL_DISCHARGE") => new[] { "IMO_MARPOL_REPORT" },
            ("EU", "GHG_THRESHOLD") => new[] { "EU_ETS_REPORT" },
            _ => Enumerable.Empty<string>()
        };
    }
}
```

---

## OSPAR Convention (Offshore Chemicals and GHG)

Applies to offshore facilities in the OSPAR Maritime Area (N Atlantic + North Sea):

| Threshold | Obligation Type | Deadline | Reference |
|---|---|---|---|
| Hydrocarbon release > 1 m³ to sea | `OSPAR_OFFSHORE_REPORT` | 24 hours | OSPAR Decision 2000/1 |
| Annual chemical discharge > threshold | `OSPAR_CHEM_REPORT` | March 31 | OSPAR Recommendation 2003/5 |
| Annual CO2 stored offshore | `OSPAR_CCS_REPORT` | March 31 | OSPAR Decision 2007/2 |

PPDM mapping: `OBLIGATION.OBLIG_TYPE = 'OSPAR_OFFSHORE_REPORT'`, `JURISDICTION_CODE = 'OSPAR'`

---

## IMO MARPOL (Marine Pollution)

Applies to vessels (FSOs, FPSOs, shuttle tankers) operated by the field:

| Annex | Subject | Threshold | Obligation |
|---|---|---|---|
| Annex I | Oil bilge discharge | > 15 ppm | `IMO_MARPOL_ANNEX1_REPORT` |
| Annex VI | Air pollution / NOx/SOx | Operating in ECA | `IMO_MARPOL_ANNEX6_LOG` |
| Annex VI | GHG — CII rating | Annual | `IMO_CII_ANNUAL_REPORT` |

PPDM: Store vessel as `EQUIPMENT` with `EQUIP_TYPE='VESSEL'`; `MARPOL` emissions stored in `PDEN_VOL_DISPOSITION` with `DISP_TYPE='MARPOL_DISCHARGE'`.

---

## EU Emissions Trading System (EU ETS)

For facilities in EU member states, Phase III and IV:

| Trigger | Obligation | Deadline |
|---|---|---|
| Annual: verified emissions > 2,500 t CO2e | `EU_ETS_VERIFIED_EMISSIONS` | March 31 |
| Annual: surrender allowances | `EU_ETS_ALLOWANCE_SURRENDER` | April 30 |
| Change in capacity | `EU_ETS_MONITORING_PLAN_UPDATE` | 6 weeks before change |

Emission allowances are stored as `OBLIG_PAYMENT` with `PAYMENT_TYPE='EU_ETS_ALLOWANCE'`:

| Column | Value |
|---|---|
| `PAYMENT_CURRENCY` | EUA (EU Allowance unit code) |
| `PAYMENT_QUANTITY` | Number of allowances |
| `GROSS_AMT` | Market value at submission date |

---

## IOGP Guidelines

IOGP 2022e reporting applies globally for members. Required annual data sets:

| Dataset | Basis | Stored In |
|---|---|---|
| PSE Tier 1/2 counts | `HSE_INCIDENT` WHERE `INCIDENT_TIER IN (1,2)` | Phase 7 |
| Exposure hours | `PDEN_SOURCE WHERE SOURCE_TYPE='EXPOSURE_HOURS'` | Phase 7 |
| Fatalities and LTI | `HSE_INCIDENT_BA WHERE INJURY_TYPE IN ('FATALITY','LTI')` | Phase 7 |
| GHG emissions (t CO2e) | Calculated per Phase 8 GHG module | Phase 8 |

---

## International Obligation PPDM Mapping

| Column | Value |
|---|---|
| `OBLIGATION.JURISDICTION_CODE` | `'OSPAR'` / `'IMO'` / `'EU_ETS'` / `'IOGP'` |
| `OBLIGATION.OBLIG_TYPE` | Per table above |
| `OBLIGATION.REGULATOR_BA_ID` | FK to BUSINESS_ASSOCIATE representing the regulator |
| `OBLIGATION.REPORTING_PERIOD_START` | Period start for the obligation |
| `OBLIGATION.REPORTING_PERIOD_END` | Period end |
