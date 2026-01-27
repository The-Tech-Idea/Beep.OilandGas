# Implementation Status

## Completed

- Process models in Beep.OilandGas.LifeCycle/Models/Processes/.
- Process services in Beep.OilandGas.LifeCycle/Services/Processes/.
- Process workflow tables in Beep.OilandGas.LifeCycle/Scripts/ProcessWorkflowTables.sql.
- Phase process services in Beep.OilandGas.LifeCycle/Services/<Phase>/Processes/.
- Entity lifecycle services in Beep.OilandGas.LifeCycle/Services/<Entity>Lifecycle/.
- PPDM entity status tables and models in Beep.OilandGas.PPDM39/Scripts/Sqlserver/ and Beep.OilandGas.PPDM.Models/39/.

## In progress / next steps

- Add or extend process DTOs in Beep.OilandGas.PPDM39/Core/DTOs/ProcessDTOs.cs if new APIs are needed.
- Integrate process services with phase services and FieldOrchestrator where required.
- Add integration tests for workflows and lifecycle transitions.

## Notes

- Process workflow tables are application-level (not PPDM standard).
- Entity status tables are PPDM standard tables.
