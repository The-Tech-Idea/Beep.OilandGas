# STG-12 — Decommissioning

| Field | Value |
|---|---|
| Stage | STG-12 |
| Features | 3 (DEC-01 … DEC-03) |
| Depends on | STG-05, STG-11 |
| Status | Gate met - DEC-01, DEC-02 and DEC-03 gate met ([DEC-01](../evidence/STG-12-DEC-01/observed-evidence.json), [DEC-02](../evidence/STG-12-DEC-02/observed-evidence.json), [DEC-03](../evidence/STG-12-DEC-03/observed-evidence.json)); exit-gate items **SG-12-1..SG-12-4 met**. The exit gate holds: a well can be planned for P&A with barriers and cost, and a facility decommissioning programme is recorded with environmental restoration and a versioned cost estimate. |
| Exit gate | A well can be planned for P&A with barriers and cost, and a facility decommissioning programme can be recorded with environmental restoration |

---

## 1. Objective

The end of the lifecycle. Decommissioning is heavily regulated and financial, so this stage
combines barrier evidence (from `OilandGasTools` in STG-16), cost estimation, approval and
restoration records.

---

## 2. Scope

**In**

- Decommissioning workspace: portfolio of wells and facilities by state, cost and schedule.
- Well P&A: programme, barrier verification, plug placement, cost, approval, execution record.
- Facility decommissioning: scope, cost, environmental restoration, final status.
- Cost estimation with escalation and contingency.

**Out**

- Physical operation scheduling (work orders are WRK).
- Regulatory submission.

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.Decommissioning` | `IFieldDecommissioningService`, `FieldDecommissioningService.Analysis`, contracts `WellAbandonment*`, `FacilityDecommissioning*`, `EnvironmentalRestoration*`, `DecommissioningCost*`; tables `DECOMMISSIONING_STATUS`, `ABANDONMENT_STATUS`, `R_DECOMMISSIONING_REFERENCE_CODE`, `ENVIRONMENTAL_RESTORATION` |
| `Beep.OilandGas.LifeCycle` | `Decommissioning*Service`, `FieldLifecycleService`, `DecommissioningProcesses` |
| `Beep.OilandGas.ProductionOperations` | `IFacilityManagementService` for facility identity |
| `TheTechIdea.OilGas.WellSchematic` | `Abandonment.Track`, `BarrierTemplates.PermanentAbandonment` (STG-16 dependency) |
| `Beep.OilandGas.Client` | `.Operations`, `.LifeCycle` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| DEC-01 | Decommissioning workspace | [DEC-01](../features/DEC/DEC-01-Decommissioning-Workspace.md) |
| DEC-02 | Well plug and abandonment | [DEC-02](../features/DEC/DEC-02-Well-Plug-and-Abandonment.md) |
| DEC-03 | Facility decommissioning and cost estimation | [DEC-03](../features/DEC/DEC-03-Facility-Decommissioning-and-Cost-Estimation.md) |

---

## 5. New code introduced

`.Modules/Decommissioning` — three pairs. Hosts `WellSchematicView` with the abandonment track
from `WellTools`.

---

## 6. Architecture notes

- **Barrier evidence is required, not asserted.** A plug cannot be recorded as placed without an
  evidence record; the screenshot of the schematic is not evidence.
- **Cost is `decimal` and versioned.** A cost revision creates a new record.
- **Environmental restoration is a separate record** with its own completion state.
- **Facility selection is revalidated** before and after any write, same as HSE.

---

## 7. Dependencies and sequencing

DEC-01 → DEC-02 and DEC-03. DEC-02 needs `VSB-05`/`VSB-08` for the schematic view; until then it
shows the textual barrier table only.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-12-1 | Workspace counts come from records; a failed read is not zero | test `DEC-01/T02` |
| SG-12-2 | P&A record refuses without barrier evidence | test `DEC-02/T03` |
| SG-12-3 | Cost revision does not mutate the previous version | test `DEC-03/T02` |
| SG-12-4 | Restoration state is independent of facility decommissioning state | test `DEC-03/T04` |

---

## 9. Test plan

`DEC-01/T02`, `DEC-02/T03`, `DEC-03/T02`, `DEC-03/T04`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| Barrier rules differ by jurisdiction | Rules come from `BarrierTemplates` and configuration, not from desktop code |
| No regulator submission path exists | State as a limitation; do not simulate success |
