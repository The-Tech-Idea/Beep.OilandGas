# Phase 9 — SAP ERP Integration
## SAP PM Work Orders → PROJECT / EQUIPMENT_MAINTAIN; FIN_COMPONENT Cost Sync

---

## ISapErpAdapter Interface

```csharp
public interface ISapErpAdapter
{
    Task<SapSyncResult> SyncWorkOrderAsync(
        string sapWoNumber, string fieldId, string userId);

    Task<SapSyncResult> SyncCostsAsync(
        string sapWoNumber, string userId);

    Task<List<SapWorkOrderSummary>> GetOpenWorkOrdersAsync(
        string plantCode, string fieldId);
}

public record SapSyncResult(
    bool Success, string? ProjectId, int CostRowsWritten, string? ErrorMessage);

public record SapWorkOrderSummary(
    string SapWoNumber, string Description, string WoType,
    string Status, DateTime? BasicFinishDate, decimal? TotalCost);
```

---

## SAP PM Work Order → PPDM PROJECT Mapping

| SAP PM Field | PPDM Column | Table | Notes |
|---|---|---|---|
| Order Number | `PROJECT.SAP_WO_NUMBER` (ext col) | `PROJECT` | Used for dedup |
| Short text | `PROJECT.PROJECT_NAME` | | |
| Order type (PM01/PM02/PM03) | `PROJECT.PROJECT_TYPE` | | PM01=Corrective, PM02=Preventive, PM03=Inspection |
| Plant | `PROJECT.FIELD_ID` | | Maps plant code → FIELD_ID |
| System status (CRTD/REL/TECO) | `PROJECT.PROJECT_STATUS` | | TECO=COMPLETED, REL=IN_PROGRESS |
| Planner group | `PROJECT_STEP_BA.BA_ID` | `PROJECT_STEP_BA` | Responsible planner |
| Basic start / finish | `PROJECT.PLAN_START_DATE` / `PLAN_END_DATE` | | |
| Actual start / finish | `PROJECT.ACT_START_DATE` / `ACT_END_DATE` | | |
| Equipment number | `EQUIPMENT_MAINTAIN.EQUIP_ID` | `EQUIPMENT_MAINTAIN` | FK to EQUIPMENT |

---

## SAP PM → EQUIPMENT_MAINTAIN Mapping

| SAP PM Field | PPDM Column |
|---|---|
| Equipment Number | `EQUIPMENT_MAINTAIN.EQUIP_ID` |
| Order Number | `EQUIPMENT_MAINTAIN.PROJECT_ID` |
| Maintenance type | `EQUIPMENT_MAINTAIN.MAINTAIN_TYPE` |
| Scheduled date | `EQUIPMENT_MAINTAIN.MAINT_DATE` |
| Completion date | `EQUIPMENT_MAINTAIN.ACT_COMPLETE_DATE` |

---

## SAP Cost Elements → FIN_COMPONENT Mapping

SAP CO actual costs are pulled per WO and written as `FIN_COMPONENT` rows:

| SAP CO Field | PPDM Column | Notes |
|---|---|---|
| Cost element number | `FIN_COMPONENT.COST_ELEMENT` | |
| Cost element name | `FIN_COMPONENT.COMPONENT_DESC` | |
| Actual cost (WO attribute) | `FIN_COMPONENT.ACTUAL_COST` | Local currency |
| Plan cost | `FIN_COMPONENT.PLAN_COST` | |
| Currency | `FIN_COMPONENT.COST_CURRENCY` | SAP `WAERS` field |
| Controlling area | `FIN_COMPONENT.CONTROLLING_AREA` | |
| Year / Period | `FIN_COMPONENT.COST_YEAR`, `COST_PERIOD` | |

---

## Integration Protocol

SAP integration uses the **SAP REST API** (SAP Gateway OData v4 / BTP API) rather than RFC BAPI calls, for firewall compatibility:

```json
"Integrations": {
    "SAP": {
        "BaseUrl": "https://sap-gateway.company.com/sap/opu/odata4/SAP_PM",
        "ApiKey": "{{env:SAP_API_KEY}}",
        "PlantCode": "1000",
        "SyncIntervalMinutes": 60,
        "CircuitBreakerThreshold": 5
    }
}
```

---

## Sync Trigger Options

| Trigger | Description |
|---|---|
| Scheduled (hourly) | Pull all WOs modified in last 1 hour from SAP |
| On-demand via API | `POST /api/integrations/sap/sync-wo/{sapWoNumber}` triggered by operator |
| Webhook (optional, Phase 10) | SAP sends change notification via RFC `Z_PPDM_WO_CHANGED` |

---

## PPDM Project Status Mapping

| SAP System Status | PPDM Project Status | Transition |
|---|---|---|
| `CRTD` (Created) | `PENDING` | None |
| `REL` (Released) | `IN_PROGRESS` | Process state machine if linked |
| `PCNF` (Partially Confirmed) | `IN_PROGRESS` | — |
| `CNF` (Confirmed) | `IN_REVIEW` | — |
| `TECO` (Technically Completed) | `COMPLETED` | Closes process instance if linked |
| `CLSD` (Closed) | `CLOSED` | — |
