# Phase 8 — Canada Regulatory Requirements
## AER ST-39/ST-60, CER/NEB Forms, ECCC NIR, Crown Royalty

---

## Canada Obligation Trigger Matrix

| Operational Event | Obligation Type | Form/Directive | Due Date | Auto-Create |
|---|---|---|---|---|
| Month-end Oil production (Alberta) | `AER_ST39_PRODUCTION` | AER ST-39 | 20th of following month | Yes |
| Month-end Gas production (Alberta) | `AER_ST60_PRODUCTION` | AER ST-60 | 20th of following month | Yes |
| Spill/release reported | `AER_SPILL_REPORT` | AER Directive 070 | Immediately | Yes (Phase 7) |
| Annual: GHG emissions | `ECCC_NIR_GHG` | ECCC NIR Appendix A | March 31 | Yes |
| Pipeline construction/new facility | `CER_PIPELINE_APPLICATION` | CER Part III | Pre-construction | Manual |
| Quarterly: Crown royalty | `AB_CROWN_ROYALTY` | Alberta Crown generic | Quarterly | Yes |

---

## AER ST-39 — Monthly Oil Production Report

| AER Field | PPDM Source | Notes |
|---|---|---|
| Unique Well Identifier (UWI) | `WELL.UWI` | 16-char Alberta UWI |
| Location | `WELL.SURFACE_LATITUDE` / `SURFACE_LONGITUDE` | Converted to DLS/NTS |
| Product Type | `PDEN_VOL_SUMMARY.PROD_TYPE` | Oil/Condensate/Water |
| Volume (m³) | `PDEN_VOL_SUMMARY.PROD_VOLUME` | Convert BBL → m³ (÷ 6.2898) |
| Hours Produced | `PDEN_VOL_SUMMARY.HOURS_ON` | |
| Method of Production | `WELL.LIFT_TYPE` | Flow/Pump/ESP |

---

## AER ST-60 — Monthly Gas Production Report

| AER Field | PPDM Source | Notes |
|---|---|---|
| UWI | `WELL.UWI` | |
| Raw Gas Volume (103m³) | `PDEN_VOL_SUMMARY.PROD_VOLUME` | Convert MSCF → 103m³ (× 0.02832) |
| Sales Gas Volume | `PDEN_VOL_DISPOSITION.DISP_VOL` | Where `DISP_TYPE='SALES'` |
| Fuel Gas | `PDEN_VOL_DISPOSITION.DISP_VOL` | Where `DISP_TYPE='FUEL'` |
| Flare / Vent | `PDEN_VOL_DISPOSITION.DISP_VOL` | Where `DISP_TYPE='FLARE'` or `'VENT'` |
| H2S Concentration (%) | `PDEN_VOL_SUMMARY.H2S_PERCENT` | |

---

## ECCC National Inventory Report (NIR) Protocol

The NIR emission factor approach for oil and gas (Appendix A):

| Source Category | Emission Factor Source | PPDM Volume Basis |
|---|---|---|
| Flaring (solution gas) | ECCC Table A4-7: 2.18 t CO2e/103m³ | `PDEN_VOL_DISPOSITION WHERE DISP_TYPE='FLARE'` |
| Venting (gas wells) | ECCC Table A4-11: 1.22 t CO2e/103m³ | `PDEN_VOL_DISPOSITION WHERE DISP_TYPE='VENT'` |
| Pneumatic devices (high-bleed) | ECCC Table A4-14: per device count | `EQUIPMENT WHERE EQUIP_TYPE='PNEUMATIC'` |
| Diesel combustion (engines) | ECCC Table A3-2: 0.0027 t CO2e/L | `EQUIPMENT.FUEL_CONSUMPTION` |

---

## Alberta Crown Royalty (Generic Formula)

The Alberta generic Crown royalty rate depends on netback price and production rate:

```
Royalty Rate (%) = base rate + ((price - base_price) × price_sensitivity)

Where:
  base rate = 5% for new wells; 25–40% for mature wells
  price_sensitivity from AER Modernized Royalty Framework 2017 tier tables

Due royalty = Royalty Rate × (Gross Revenue − Allowed Deductions)
```

Royalty calculations are stored as `OBLIG_PAYMENT` rows:

| Column | Value |
|---|---|
| `PAYMENT_TYPE` | `'AB_CROWN_ROYALTY'` |
| `PAYMENT_BASIS` | Royalty rate used |
| `GROSS_AMT` | Calculated due royalty in CAD |
| `ACTUAL_AMT` | Paid amount |
| `VARIANCE_AMT` | `GROSS_AMT - ACTUAL_AMT` |

---

## Provincial Variations

| Province | Key Form | Standard | Notes |
|---|---|---|---|
| Alberta | ST-39 / ST-60 | AER | Primary jurisdiction |
| British Columbia | PETRINEX | BCOGC | Petrinex web portal; similar columns to AER |
| Saskatchewan | PSP | SPMC | Saskatchewan Petroleum and Natural Gas Act |
| Manitoba | EIA | MGGB | Manitoba Oil and Gas Branch |
