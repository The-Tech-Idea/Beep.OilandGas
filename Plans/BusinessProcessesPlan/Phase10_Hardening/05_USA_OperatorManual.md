# Phase 10 — USA Operator Manual
## End-User Guide: Processes, Workflows, and Compliance for USA Jurisdiction

---

## USA Jurisdiction Overview

For fields with `JURISDICTION = 'USA'`, the system applies:
- **BSEE** regulations (offshore): 30 CFR Part 250, SEMS requirements
- **ONRR** production reporting: Form MMS-2014 monthly
- **EPA** GHG reporting: 40 CFR 98 Subpart W (annual)
- **OPA 90** emergency notification for oil spills
- **OSHA** HSE incident recordkeeping: OSHA 300 log

---

## Accessing the System

1. Navigate to `https://[your-deployment]/`
2. Sign in with your corporate credentials
3. Select your field from the Field Selector dropdown
4. The navigation sidebar shows all available modules for your role

---

## USA Daily Workflow

### Production Reporting

1. Navigate to **Production → Daily Entry**
2. Enter or confirm daily oil, gas, and water volumes for each wellbore
3. Volumes auto-populate `PDEN_VOL_SUMMARY` for the selected field
4. At month end, navigate to **Compliance → Reports → EIA-914** to generate and download the monthly CSV for submission to EIA

### ONRR Production Report

1. Navigate to **Compliance → Obligations**
2. Locate the auto-generated `ONRR_PRODUCTION_REPORT` obligation for the current month
3. Click **Generate Report** → system creates CSV mapped to Form MMS-2014 columns
4. Download and submit to ONRR eCommerce Portal
5. Click **Mark Submitted** and enter the ONRR confirmation number

### BSEE SEMS Annual Audit

1. Navigate to **Compliance → Obligations → BSEE_SEMS_ANNUAL_AUDIT**
2. The SEMS audit process (14 elements) auto-starts each year on January 1
3. For each element, navigate to the linked process and attach supporting documentation
4. Submit completed audit package via the **Submit to BSEE** workflow step

---

## Incident Reporting (USA)

### Reporting a Tier 1 PSE

1. Navigate to **HSE → Report Incident**
2. Select `Incident Type = PSE_TIER1`, set the date and location
3. System automatically creates a `BSEE_INCIDENT_REPORT` obligation (due within 24 hours)
4. Assign an investigator and proceed through the UNDER_INVESTIGATION state
5. Record immediate notification to NRC if oil spill ≥ 1 barrel:
   - Navigate to **HSE → [Incident] → Obligations**
   - Mark the `NRC_REPORT` obligation as submitted

### OSHA 300 Log Entry

1. After recording injury details in the incident, navigate to **HSE → OSHA 300 Log**
2. Confirm the auto-populated injury classification (LTI / RWC / MTC / FAC)
3. Print or export the log for on-site posting (required by 29 CFR 1904.44)

---

## Emergency Response (USA — OPA 90)

1. Navigate to **HSE → Emergency Response → Activate**
2. Select `Emergency Type = OIL_SPILL` and enter estimated spill volume in barrels
3. System auto-creates:
   - `NRC_REPORT` obligation (due Immediately)
   - `USCG_NOTIFY` obligation (due within 24 hours if > 10,000 gallons)
   - `BSEE_REPORT` obligation (due within 24 hours for offshore)
4. Record each agency contact using the **Log Agency Contact** button
5. Track containment, remediation, and restoration states through the ER workflow

---

## Role Summary (USA)

| Role | Access | Key Responsibilities |
|---|---|---|
| `ProcessOperator` | Field production + work orders | Daily data entry; WO execution |
| `SafetyOfficer` | All HSE pages | Incident reporting; HAZOP management; barrier review |
| `ComplianceOfficer` | All Compliance pages | ONRR/EPA submissions; obligation tracking |
| `Approver` | Gate review approvals | Field Development decisions; ER restoration sign-off |
| `Auditor` | Read-only all modules | Audit trail review; OSHA log verification |
| `Manager` | All + admin | Field activation; DR decisions |
