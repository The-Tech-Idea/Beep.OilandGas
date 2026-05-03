# ProductionOperations Migration Notes

## Canonical Source of Truth
- Production/facility operation persistence is canonicalized on `ProductionOperationsService` and `FacilityManagementService`.
- `ProductionManagementService` remains compatibility-oriented for legacy orchestration and transition support.

## Compatibility Boundaries
- Compatibility API routes in `ProductionOperationsController` remain available while canonical routes are preferred.
- Lifecycle integration remains explicit where `PPDMProductionService` performs internal orchestration.

## Deprecation/Cutover Checklist
- Remove semantic overlap where legacy routes duplicate canonical writes.
- Keep compatibility mapping logic isolated and documented.
- Ensure tests cover both canonical and compatibility behavior during transition.
