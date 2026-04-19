# Phase 10 — International Operator Manual
## End-User Guide: Processes, Workflows, and Compliance for International Jurisdiction

---

## International Jurisdiction Overview

For fields with `JURISDICTION = 'INTERNATIONAL'`, the system applies:
- **OSPAR Convention**: Offshore hydrocarbon releases in N Atlantic / North Sea
- **IMO MARPOL**: Vessel pollution control (FSOs, FPSOs, shuttle tankers)
- **EU ETS**: Greenhouse gas emissions trading (EU member state operations)
- **IOGP 2022e**: Voluntary HSE performance reporting

---

## Setting Up an International Field

Configuration required before first use:

- [ ] `FIELD.JURISDICTION = 'INTERNATIONAL'`
- [ ] `FIELD.COUNTRY_CODE` set to ISO 3166 alpha-2 (e.g., `'GB'` for UK North Sea, `'NO'` for Norway)
- [ ] Regulatory body added as `BUSINESS_ASSOCIATE` with `BA_TYPE='REGULATOR'`
- [ ] OSPAR area flag: `FIELD.IS_OSPAR_AREA = 'Y'` if in N Atlantic / North Sea
- [ ] EU member state: `FIELD.IS_EU_MEMBER_STATE = 'Y'` if applicable to EU ETS
- [ ] IMO vessel registrations: `EQUIPMENT WHERE EQUIP_TYPE='VESSEL'` linked to field

---

## Offshore Production Reporting

Most international jurisdictions use their own production reporting formats (UK EEMS, Norway PETRO, Netherlands EBN). The system provides raw production data export:

1. Navigate to **Production → Export → Custom CSV**
2. Select columns matching the regulatory authority's required format
3. Map PPDM field names to regulatory authority column names using the Column Mapping tool
4. Download CSV; submit via the regulator's online portal

---

## OSPAR Release Reporting

### Hydrocarbon Release to Sea

1. Navigate to **HSE → Report Incident**
2. Set `Jurisdiction = INTERNATIONAL`, `Incident Type = OFFSHORE_RELEASE`
3. If `FIELD.IS_OSPAR_AREA = 'Y'` and release volume > OSPAR threshold, system auto-creates:
   - `OSPAR_OFFSHORE_REPORT` obligation (due within 24 hours)
4. Record volume in barrels and substance type (crude oil / condensate / produced water)
5. Navigate to **HSE → [Incident] → Obligations → OSPAR_OFFSHORE_REPORT**
6. Mark submitted with OSPAR reference number

---

## EU ETS Compliance

### Annual Verified Emissions

1. Navigate to **Compliance → GHG Report**
2. Select year, Jurisdiction = `INTERNATIONAL (EU)`
3. Review total CO₂e from combustion and process sources
4. Engage an accredited verifier to sign off on emission figures
5. Submit verified emissions to national competent authority's EU ETS portal
6. Navigate to **Compliance → Obligations → EU_ETS_VERIFIED_EMISSIONS → Mark Submitted**

### Surrendering Allowances

1. Navigate to **Compliance → Royalty / Financial → EU ETS Allowances**
2. Enter number of EUAs (EU Allowances) to surrender for the reporting year
3. System records `OBLIG_PAYMENT` with `PAYMENT_TYPE='EU_ETS_ALLOWANCE'` and market value
4. Surrender via EU Registry (EU Transaction Log); enter registry reference in notes

---

## IOGP Annual HSE Report

1. Navigate to **Analytics → HSE KPIs**
2. Set date range to full calendar year
3. Export IOGP dataset (includes Tier 1/2 PSE counts, TRIR, LTIF, Fatalities, Exposure Hours)
4. Submit via IOGP member portal (voluntary; recommended for IOGP members)

---

## IMO MARPOL (Vessel Operations)

1. Navigate to **Equipment → Vessels**
2. Select the FSO / FPSO / shuttle tanker
3. Navigate to **Vessel → MARPOL Log**
4. Record bilge water discharges, oil record book entries
5. System creates `IMO_MARPOL_ANNEX1_REPORT` obligation if discharge > 15 ppm

---

## Role Summary (International)

| Role | Access | Key International Responsibilities |
|---|---|---|
| `ProcessOperator` | Field production | Daily volumes, wellhead data |
| `SafetyOfficer` | HSE | OSPAR release reports; HAZOP for offshore nodes |
| `ComplianceOfficer` | Compliance | EU ETS; OSPAR annual chemical returns; ETS allowances |
| `Approver` | Gate reviews | HSE case approvals (UK UKCS: Safety Case per PFEER) |
| `Auditor` | Read-only | IOGP report audit; verifier support |
| `Manager` | All + admin | Engagement with local competent authority (OGA/PARS/Petroleum Safety Authority) |
