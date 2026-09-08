# Field cost allocation

The selected workflow calculates new allocations from active COST_TRANSACTION rows,
not previously stored allocation results. Dates are inclusive calendar dates; the
query uses an exclusive next-day upper bound. Operating and capital costs are
calculated separately using the existing Accounting CostAllocationService engine.

## Configuration

Configure each field under CostAllocation:Fields in the API configuration. Example
IDs must be replaced with active COST_CENTER IDs belonging to the selected field:

```json
{
  "CostAllocation": {
    "Fields": {
      "FIELD-ID": {
        "Centers": [
          { "CostCenterId": "SUPPORT-ID", "CostCenterType": "SUPPORT", "AllocationSequence": 1 },
          { "CostCenterId": "TARGET-A", "CostCenterType": "REVENUE", "AllocationBasisValue": 1, "ActivityUnits": 1 },
          { "CostCenterId": "TARGET-B", "CostCenterType": "REVENUE", "AllocationBasisValue": 3, "ActivityUnits": 3 }
        ],
        "ActivityBases": [
          { "CostCenterId": "SUPPORT-ID", "ActivityName": "Operations", "ActivityPercent": 1 }
        ]
      }
    }
  }
}
```

ActivityPercent is a fraction, not a percentage. For ABC, each support center's
activity fractions must total one. For step-down, explicitly order support centers.
Rules are deployment configuration for now, not an implemented administration UI.
Configuration must be reviewed by the organization's accounting owner before use.

Every source transaction must have an amount, a configured cost center, and exactly
one of IS_CAPITALIZED/IS_EXPENSED set to Y. Missing rules, invalid classification,
foreign centers, or unreconciled totals fail instead of returning misleading zeros.
Revenue centers retain their own direct costs in addition to incoming allocations.

## API behavior and remaining gates

POST api/accounting/cost/allocations/allocate currently calculates one calendar day.
The requested connection is passed through for both reading and persistence.
Request-supplied total overrides are no longer applied; the engine owns totals.
Audit identity uses the controller's existing principal resolver, not query userId.
The existing persistence path still saves one aggregate COST_ALLOCATION record.

Before production rollout: independently verify multiple-support reciprocal and
step-down fixtures, introduce durable per-center details/rule-version provenance,
add idempotency and transactional persistence, enforce field/connection authorization,
and exercise the hosted endpoint against the deployment database provider.
Current regression tests mock the datasource and run the real calculation engine;
they do not prove database filtering, endpoint authorization, or persistence behavior.
