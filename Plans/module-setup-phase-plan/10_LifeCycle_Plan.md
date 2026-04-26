# Project 10 — Beep.OilandGas.LifeCycle
## O&G Best-Practice Audit & Revision Plan

**Purpose**: Orchestration layer that coordinates lifecycle workflows (Exploration → Development → Production → Decommissioning) and permit management for wells, facilities, and fields.  
**Architecture role**: Service/orchestrator only — no raw table classes; consumes PPDM39 table classes via injected repositories.

---

## Sub-phases

### SP-A · Compile Error Audit
**Status**: ✅ Complete

Identified all compile errors in `PermitManagementService.cs`:

| Error | Root Cause |
|-------|------------|
| `CREATED_DATE = mappedApplication.CREATED_DATE` (×3) | `ModelEntityBase` exposes `ROW_CREATED_DATE`, not `CREATED_DATE` |
| `ATTACHMENTS = mappedApplication.ATTACHMENTS` (×3) | Collection property removed from table classes during Project 07 audit |
| `AREAS = mappedApplication.Areas` (×3) | Collection property removed from table classes during Project 07 audit |
| `COMPONENTS = mappedApplication.Components` (×3) | Collection property removed from table classes during Project 07 audit |
| `WELL_UWI = application.WELL_UWI` (ENV block ×2) | `ENVIRONMENTAL_PERMIT_APPLICATION` lacked `WELL_UWI` field |
| `TARGET_FORMATION`, `PROPOSED_DEPTH`, `DRILLING_METHOD`, `SURFACE_OWNER_NOTIFIED` (ENV block) | Drilling-specific fields incorrectly applied to environmental permit class |
| `WELL_UWI = application.WELL_UWI` (INJECTION block ×2) | `INJECTION_PERMIT_APPLICATION` uses `INJECTION_WELL_UWI`; service used wrong field name |

---

### SP-B · Compile Error Fixes
**Status**: ✅ Complete

#### `PermitManagementService.cs`
- Changed all 3 occurrences of `CREATED_DATE = mappedApplication.CREATED_DATE` → `ROW_CREATED_DATE = mappedApplication.CREATED_DATE`
- Removed `ATTACHMENTS`, `AREAS`, `COMPONENTS` assignments from all 3 permit creation blocks (DRILLING, ENVIRONMENTAL, INJECTION)
- ENVIRONMENTAL block: removed `TARGET_FORMATION`, `PROPOSED_DEPTH`, `DRILLING_METHOD`, `SURFACE_OWNER_NOTIFIED` assignments (drilling-specific, invalid for environmental permits)
- ENVIRONMENTAL block: retained `WELL_UWI = application.WELL_UWI` (valid — links environmental assessment to a well)
- INJECTION block: changed `WELL_UWI = application.WELL_UWI` → `INJECTION_WELL_UWI = application.INJECTION_WELL_UWI` (class uses `INJECTION_WELL_UWI`, not generic `WELL_UWI`)

#### `ENVIRONMENTAL_PERMIT_APPLICATION.cs` (Beep.OilandGas.PermitsAndApplications)
- Added `WELL_UWI` scalar field (string) before `ENVIRONMENTAL_PERMIT_TYPE` — links an environmental permit to the relevant well UWI per PPDM 3.9 convention

---

### SP-C · Data Class Shape Review
**Status**: ✅ Complete (no violations found)

`Models/Processes/` contains 7 classes:
- `ProcessDefinition`, `ProcessInstance`, `ProcessStepDefinition`, `ProcessStepInstance`, `ProcessHistory`, `ProcessHistoryEntry`, `ProcessTransition`

All are **projection/workflow classes**:
- ✅ Contain `List<T>` and `Dictionary<K,V>` (permitted for projections)
- ✅ NOT passed to `InsertAsync`/`UpdateAsync`
- ✅ Not persisted via `PPDMGenericRepository`
- ✅ Represent in-memory workflow state, not DB table rows

No `ModuleSetupBase` required — LifeCycle is an orchestration project with no owned table types.

---

### SP-D · O&G Best-Practice Review
**Status**: ✅ Complete

| Area | Finding | Action |
|------|---------|--------|
| Permit workflow | Three permit types (APD, Environmental/NEPA, UIC Injection) aligned to US regulatory framework | No change needed |
| DRILLING_PERMIT_APPLICATION | Has all ERCB/MMS APD fields: `WELL_UWI`, `TARGET_FORMATION`, `PROPOSED_DEPTH`, `DRILLING_METHOD`, `SURFACE_OWNER_NOTIFIED_IND`, `SURFACE_OWNER_NOTIFICATION_DATE`, `ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND`, `PERMIT_TYPE`, `SPACING_UNIT` | ✅ Compliant |
| ENVIRONMENTAL_PERMIT_APPLICATION | Has NEPA/NPDES fields: `NEPA_REVIEW_TYPE`, `EPA_NPDES_PERMIT_NUMBER`, `DISPOSAL_METHOD`, `MONITORING_PLAN`, `NORM_INVOLVED_IND`, `ENVIRONMENTAL_IMPACT`, `WASTE_TYPE`, `WASTE_VOLUME` | ✅ Compliant after `WELL_UWI` add |
| INJECTION_PERMIT_APPLICATION | Has EPA UIC fields: `INJECTION_WELL_CLASS` (Class I–VI per 40 CFR 144), `IS_CO2_STORAGE_IND`, `IS_GAS_STORAGE_IND`, `IS_BRINE_MINING_IND`, `INJECTION_ZONE`, `INJECTION_FLUID`, `MAXIMUM_INJECTION_PRESSURE`, `MAXIMUM_INJECTION_RATE` | ✅ Compliant |
| FieldOrchestrator | Manages field context; permit endpoints filter by `FIELD_ID` | ✅ Correct |
| Logging | Structured `_logger?.LogInformation/LogError` with `{FieldId}`, `{WellId}` placeholders | ✅ Correct |

---

### SP-E · Build Validation
**Status**: ✅ Complete

```
dotnet build Beep.OilandGas.LifeCycle.csproj -v q
→ 0 Error(s), 0 Warning(s)

dotnet build Beep.OilandGas.PermitsAndApplications.csproj -v q
→ 0 Error(s), 0 Warning(s)
```

---

## Files Changed

| File | Change |
|------|--------|
| `Beep.OilandGas.LifeCycle/Services/Permits/PermitManagementService.cs` | Fixed 3× CREATED_DATE→ROW_CREATED_DATE; removed collection props; removed drilling fields from ENV block; fixed INJECTION WELL_UWI reference |
| `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/ENVIRONMENTAL_PERMIT_APPLICATION.cs` | Added `WELL_UWI` scalar field |

## Key O&G Standards Applied
- **APD (Application for Permit to Drill)**: Fields aligned to BLM Form 3160-3 / ERCB Directive 056 requirements
- **NEPA Environmental Permits**: `NEPA_REVIEW_TYPE` categorizes CE/EA/EIS per 40 CFR Part 1501
- **EPA UIC Injection Permits**: `INJECTION_WELL_CLASS` covers Classes I–VI per 40 CFR Part 144
- **PPDM 3.9 Compliance**: `ROW_CREATED_DATE`, `ROW_CHANGED_DATE`, `ACTIVE_IND`, `PPDM_GUID` via `ModelEntityBase`
