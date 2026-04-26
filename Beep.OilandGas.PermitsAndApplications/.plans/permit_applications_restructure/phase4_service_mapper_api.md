# Phase 4: Service, Mapper, and API Refactor

## Goals
- Refactor all services, mappers, and API endpoints to align with the new data model boundaries.
- Ensure all endpoints use canonical models for foundation data and extension tables for custom data.
- Remove any legacy or obsolete code paths.

## Steps
1. Audit all services and mappers for references to removed/renamed tables.
2. Refactor endpoints to use canonical models and extension tables appropriately.
3. Update API documentation and OpenAPI specs as needed.
4. Test all endpoints for correct data flow.
5. Document all changes and any remaining technical debt.

## Notes
- Use the FieldOrchestrator and PPDMGenericRepository patterns for all data access.
- Document any API contract changes.
