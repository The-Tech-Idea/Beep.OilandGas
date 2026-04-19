# Phase 8 — USA Regulatory Requirements
## ONRR Form MMS-2014, BSEE SEMS (30 CFR 250), EPA 40 CFR 98

---

## USA Obligation Trigger Matrix

| Operational Event | Obligation Type | CFR / Form | Due Date | Auto-Create |
|---|---|---|---|---|
| Month-end production recorded | `ONRR_PRODUCTION_REPORT` | ONRR Form MMS-2014 | 15th of following month | Yes |
| Gas flaring > threshold | `EPA_GHG_REPORT_FLARE` | 40 CFR 98 Subpart W | Annual (March 31) | Yes |
| Offshore: any incident Tier 1 | `BSEE_INCIDENT_REPORT` | 30 CFR §250.188 | Within 24 hours | Yes (Phase 7) |
| Annual: offshore facilities | `BSEE_SEMS_ANNUAL_AUDIT` | 30 CFR §250.1920 | April 30 | Yes |
| Quarterly: royalty payment due | `ONRR_ROYALTY_PAYMENT` | ONRR compliance | Calendar quarter end | Yes |

---

## ONRR Form MMS-2014 — Production Report

Column mapping from PPDM to the ONRR report form:

| ONRR Field | PPDM Source | Column |
|---|---|---|
| Lease Number | `PRODUCTION_WELLBORE.CONTRACT_ID` | `CONTRACT_ID` |
| API Well Number | `WELL.API_WELL_NO` | `API_WELL_NO` |
| Product Code | `PDEN_VOL_SUMMARY.PROD_TYPE` | `PROD_TYPE` → mapped to ONRR product table |
| Quantity (MCF/BBL) | `PDEN_VOL_SUMMARY.VOLUME` | `PROD_VOLUME` |
| Begin Date | Report month start | — |
| End Date | Report month end | — |
| Disposition Code | `PDEN_VOL_DISPOSITION.DISP_TYPE` | → ONRR 5-digit disposition |

---

## BSEE SEMS (30 CFR Part 250) Requirements

The Safety and Environmental Management System obligation is tracked as an annual audit process:

```
ProcessId: BSEE-SEMS-AUDIT
Category:  COMPLIANCE_REPORT
Triggers: annual (scheduled) → INITIATED → IN_PROGRESS → REVIEWED → SUBMITTED → CLOSED
```

14 audit elements per SEMS (30 CFR §250.1920–.1932):

| Element | Section | Description |
|---|---|---|
| 1 | §250.1921 | Goals, objectives, and performance measures |
| 2 | §250.1922 | Management of change |
| 3 | §250.1923 | Operating procedures |
| 4 | §250.1924 | Safe work practices |
| 5 | §250.1925 | Training |
| 6 | §250.1926 | Mechanical integrity |
| 7 | §250.1927 | Pre-startup review |
| 8 | §250.1928 | Emergency response and control |
| 9 | §250.1929 | Investigation of incidents |
| 10 | §250.1930 | Audit (self-audit requirement) |
| 11 | §250.1931 | Records and documentation |
| 12 | §250.1932 | Stop work authority |
| 13 | §250.1933 | Ultimate work authority |
| 14 | §250.1934 | Employee participation plan |

---

## IComplianceService — USA Region Methods

```csharp
// Relevant USA-specific methods on IComplianceService
Task<string> CreateMMS2014ReportAsync(string fieldId, int year, int month, string userId);
Task<List<ObligationSummary>> GetUSAObligationsAsync(string fieldId, DateRangeFilter range);
Task ScheduleAnnualSEMSAuditAsync(string fieldId, int year, string userId);
```
