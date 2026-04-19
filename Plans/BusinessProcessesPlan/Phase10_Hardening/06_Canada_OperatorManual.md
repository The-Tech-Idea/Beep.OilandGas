# Phase 10 — Canada Operator Manual
## End-User Guide: Processes, Workflows, and Compliance for Canada Jurisdiction

---

## Canada Jurisdiction Overview

For fields with `JURISDICTION = 'CANADA'`, the system applies:
- **AER** (Alberta Energy Regulator): ST-39 (oil), ST-60 (gas), Directive 070 (incidents)
- **BCOGC** (British Columbia): Petrinex monthly production
- **CER/NEB**: Pipeline applications and export authorizations
- **ECCC NIR**: Annual GHG national inventory report
- **Alberta Crown Royalty**: Modernized Royalty Framework (MRF 2017)

---

## Alberta Setup Checklist (New Field)

Before recording production, ensure:

- [ ] `WELL.UWI` is populated for all wells (16-char Alberta UWI)
- [ ] `WELL.SURFACE_COUNTRY = 'CA'` and `SURFACE_STATE = 'AB'`
- [ ] `FIELD.JURISDICTION = 'CANADA'`
- [ ] AER licence numbers entered in `CONTRACT.CONTRACT_ID` for each well licence
- [ ] WTI reference price source configured (used for MRF royalty calculation)

---

## Monthly Reporting Workflow

### AER ST-39 (Oil Production) — Due 20th of Following Month

1. Navigate to **Production → Monthly Summary**
2. Confirm oil volumes are entered for all Alberta wells in the reporting month
3. Navigate to **Compliance → Reports → AER ST-39**
4. Select year and month → click **Generate CSV**
5. Review volumes (check unit conversion: BBL × 0.158987 = m³)
6. Download and submit via Petrinex or AER eSubmission portal
7. Return to Compliance → Obligations → mark `AER_ST39_PRODUCTION` as submitted

### AER ST-60 (Gas Production) — Due 20th of Following Month

1. Same as ST-39 workflow; navigate to **Reports → AER ST-60**
2. Confirm gas, fuel, flare, vent, and sales volumes are classified correctly
   - `PDEN_VOL_DISPOSITION.DISP_TYPE` must be set per volume stream
3. H₂S percentage: update `PDEN_VOL_SUMMARY.H2S_PERCENT` before generating

---

## Alberta Crown Royalty

1. Navigate to **Compliance → Royalty → Alberta Crown**
2. Select reporting quarter
3. Enter or confirm WTI monthly average price (CAD/bbl) for each month in quarter
4. System calculates MRF royalty rate and due amount per well
5. Review the **Variance Panel**: any well with variance > $500 CAD is flagged in orange
6. Navigate to **Obligations → AB_CROWN_ROYALTY → Pay** and record the payment made via CATS (Crown Accountability and Tracking System)

---

## Incident Reporting (Canada — AER Directive 070)

### Spill/Release Reporting

1. Navigate to **HSE → Report Incident**
2. Set `Jurisdiction = CANADA`, incident type, and severity
3. System auto-creates:
   - `AER_SPILL_REPORT` obligation (due Immediately for Alberta)
   - `ECCC_REPORTABLE_RELEASE` (due within 2 hours if listed substance)
4. Assign investigator
5. Contact AER via spill line (1-800-222-6514); record call in **Log Agency Contact**
6. Navigate to **Obligations → AER_SPILL_REPORT → Mark Submitted** with AER incident number

---

## Annual GHG Reporting (ECCC NIR)

1. Navigate to **Compliance → GHG Report**
2. Select year and `Jurisdiction = CANADA`
3. System queries flare, vent, and combustion volumes and applies ECCC Appendix A factors
4. Review totals; export as PDF or CSV
5. Submit via ECCC's Single Window Information Manager (SWIM)
6. Mark `ECCC_NIR_GHG` obligation as submitted with SWIM reference number

---

## Role Summary (Canada)

| Role | Access | Key Canadian Responsibilities |
|---|---|---|
| `ProcessOperator` | Field production | Daily/monthly volumes; wellbore data |
| `SafetyOfficer` | HSE | AER Directive 070 spill reports; HAZOP |
| `ComplianceOfficer` | Compliance | AER ST-39/ST-60 submissions; royalty payments; ECCC NIR |
| `Approver` | Gate reviews | Field Development Plan AER approvals |
| `Auditor` | Read-only | Provincial audit support |
| `Manager` | All + admin | Engagement with AER Area Manager |
