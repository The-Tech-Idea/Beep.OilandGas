# Beep.OilandGas.UserManagement

## Snapshot

- Category: Finance and support
- Scan depth: Medium
- Current role: user and permission infrastructure support
- Maturity signal: support library, not end-user workflow project

## Observed Structure

- Top-level folders: `DependencyInjection`, `Security`, `Services`
- Security contains permission requirement and handler types
- The project appears to support authorization plumbing more than UI-level user administration

## Representative Evidence

- Security: `Security/PermissionRequirement.cs`, `PermissionHandler.cs`
- API surfacing relationship: `Beep.OilandGas.ApiService/Controllers/AccessControl/AccessControlController.cs`

## Planning Notes

- Phase 10 access-control validation should include this project when route retirement or controller consolidation changes permission boundaries.
- The current repo scan does not show a dedicated web admin surface tied directly to this project yet.
