# 07 — PermitsAndApplications: Best-Practice Audit & Revision Plan

## SP-A — Domain Audit Findings

### O&G Standards Applied
| Standard | Scope |
|---|---|
| PPDM 3.9 `APPLICATION` / `APPLIC_STATUS` / `BA_PERMIT` | Master permit entities |
| BSEE 30 CFR §250.410 | APD (Application for Permit to Drill) |
| EPA UIC 40 CFR Part 144 | Injection well permit classes I–VI |
| Clean Water Act §402 NPDES | Environmental / wastewater permits |
| NEPA 40 CFR Part 1501 | Environmental review type |
| BOEM offshore permit | Offshore / lease area applications |
| Railroad Commission of Texas | State permit reference number conventions |

### Audit Findings

1. **Duplicate auto-property blocks in sub-table classes** — `DRILLING_PERMIT_APPLICATION`, `ENVIRONMENTAL_PERMIT_APPLICATION`, and `INJECTION_PERMIT_APPLICATION` each had a bottom block of duplicate auto-properties (APPLICATION_TYPE, STATUS, COUNTRY, etc.) that conflicted with the `ModelEntityBase` `SetProperty` pattern and violated the table-class scalar-only rule for `ATTACHMENTS`/`AREAS`/`COMPONENTS` collections.

2. **Inherited column shadow redeclarations** — `PERMIT_APPLICATION` redeclared `EFFECTIVE_DATE` and `EXPIRY_DATE` as `new` hiding the `ModelEntityBase` definitions; removed.

3. **Collection properties on table classes** — `PERMIT_APPLICATION` had `List<APPLICATION_ATTACHMENT> ATTACHMENTS` and `List<APPLICATION_AREA> Areas` auto-properties; removed (these are loaded via separate repository queries).

4. **`object` typed fields** — Sub-tables had `public object COMPONENTS`, `public object PROPOSED_DEPTH` public fields; removed.

5. **Missing O&G identifier fields** — `PERMIT_APPLICATION` lacked `FIELD_ID`, `PERMIT_NUMBER`, `LEASE_ID`; added with full backing-field pattern.

6. **Missing unit-of-measure fields** — `DRILLING_PERMIT_APPLICATION` lacked `PROPOSED_DEPTH_OUOM` and `BOND_AMOUNT_CURRENCY`; added (`PROPOSED_DEPTH_OUOM` defaults `"ft"`, `BOND_AMOUNT_CURRENCY` defaults `"USD"`).

7. **Missing EPA UIC classification** — `INJECTION_PERMIT_APPLICATION` lacked `INJECTION_WELL_CLASS` (Class I–VI), `INJECTION_PRESSURE_OUOM`, `CONFINING_ZONE_DESC`; added.

8. **Missing NEPA / NPDES fields** — `ENVIRONMENTAL_PERMIT_APPLICATION` lacked `NEPA_REVIEW_TYPE`, `EPA_NPDES_PERMIT_NUMBER`, `WASTE_VOLUME_OUOM_STR`; added.

9. **`PERMIT_STATUS_HISTORY.STATUS` typed as enum** — Changed from `PermitApplicationStatus?` to `string?` per PPDM string-code pattern.

10. **Enum types in table classes (known tech debt)** — `PERMIT_APPLICATION` and the three specialised sub-tables carry enum-typed properties (`PermitApplicationType`, `PermitApplicationStatus`, `Country`, `StateProvince`, `RegulatoryAuthority`). These are deeply integrated into existing services and mapper; left as-is and documented. Enums are co-located in `Beep.OilandGas.Models.Data.PermitsAndApplications` namespace.

---

## SP-B — File Changes

| File | Change |
|---|---|
| `Data/PermitsAndApplications/Tables/PERMIT_APPLICATION.cs` | Removed `new EFFECTIVE_DATE`/`EXPIRY_DATE` shadows; removed `List<APPLICATION_ATTACHMENT> ATTACHMENTS` and `List<APPLICATION_AREA> Areas`; removed `object Components`; added `FIELD_ID`, `PERMIT_NUMBER`, `LEASE_ID` |
| `Data/PermitsAndApplications/Tables/DRILLING_PERMIT_APPLICATION.cs` | Removed duplicate bottom auto-property block; added `PROPOSED_DEPTH_OUOM`, `WELL_TYPE`, `BOND_AMOUNT_CURRENCY`, `BOND_AMOUNT`; restored shared header fields as proper backing-field properties |
| `Data/PermitsAndApplications/Tables/ENVIRONMENTAL_PERMIT_APPLICATION.cs` | Removed duplicate bottom auto-property block; removed `object PROPOSED_DEPTH` field; added `NEPA_REVIEW_TYPE`, `EPA_NPDES_PERMIT_NUMBER`, `WASTE_VOLUME_OUOM_STR`; restored shared header fields |
| `Data/PermitsAndApplications/Tables/INJECTION_PERMIT_APPLICATION.cs` | Removed duplicate bottom auto-property block; added `INJECTION_WELL_CLASS`, `INJECTION_PRESSURE_OUOM`, `CONFINING_ZONE_DESC`; restored shared header fields |
| `Data/PermitsAndApplications/Tables/PERMIT_STATUS_HISTORY.cs` | Changed `STATUS` type from `PermitApplicationStatus?` to `string?` |
| `Services/PermitStatusHistoryService.cs` | Updated `STATUS` assignment to `nextStatusEnum.ToString()` |
| `Services/PermitApplicationWorkflowService.cs` | Updated `STATUS` assignment to `status?.ToString()` |
| `Services/PermitApplicationLifecycleService.cs` | Updated `STATUS` assignment to `status?.ToString()` |

---

## SP-C — EntityTypes Registry

`Modules/PermitsModule.cs` — **unchanged** — registers all 17 table types:

```
APPLICATION_ATTACHMENT, APPLICATION_AREA, APPLICATION_AUTHORITY_CONTACT,
APPLICATION_DOCUMENT_REQUIREMENT, APPLICATION_FEE, APPLICATION_INSPECTION,
APPLICATION_MILESTONE, APPLICATION_NOTE, APPLICATION_NOTIFICATION,
APPLICATION_REGULATORY_CONDITION, BOND_REQUIREMENT, COMPLIANCE_REQUIREMENT,
DRILLING_PERMIT_APPLICATION, ENVIRONMENTAL_PERMIT_APPLICATION,
INJECTION_PERMIT_APPLICATION, PERMIT_APPLICATION, PERMIT_STATUS_HISTORY
```

All 17 types confirmed in `IReadOnlyList<Type> EntityTypes`.

---

## Build Result

```
dotnet build Beep.OilandGas.PermitsAndApplications/Beep.OilandGas.PermitsAndApplications.csproj -v q
→ 0 Error(s)   0 Warning(s)   ✓
```
