# W11 Schema Contract - UserManagement Models

> Phase: 11 Identity, Persona, and Access Governance  
> Purpose: authoritative contract for table and primary-key shape used by migration-manager schema generation.  
> Scope: `Beep.OilandGas.UserManagement/Models/**` entities introduced in W11-03/W11-04.

---

## Contract Rules

1. Every data model in this contract inherits `ModelEntityBase`.
2. Each entity has exactly one explicit primary-key property ending with `_ID`.
3. Column/property names are uppercase with underscore separators.
4. Complex payloads are stored as scalar JSON string columns (`*_JSON`) instead of list/dictionary columns.
5. `*_IND` columns are string flags (`Y/N`) for schema portability.

---

## Canonical Table and Key Map

| Entity Class | Target Table Name | Primary Key Column | JSON Columns | Notes |
|------|--------------------|--------------------|-------------|-------|
| AppUser | APP_USER | USER_ID | - | lifecycle and lock state columns standardized |
| AppRole | APP_ROLE | ROLE_ID | - | role category and sensitivity flags |
| AppPermission | APP_PERMISSION | PERMISSION_ID | - | policy key stored in `PERMISSION_KEY` |
| AppUserRole | APP_USER_ROLE | USER_ROLE_ID | - | effective-date window for assignment |
| AppRolePermission | APP_ROLE_PERMISSION | ROLE_PERMISSION_ID | - | role-permission grant source in `SOURCE_SYSTEM` |
| UserPersonaProfile | USER_PERSONA_PROFILE | PROFILE_ID | PREFERENCES_JSON, EFFECTIVE_ACCESS_CONTEXT_JSON | persona defaults and route/context preferences |
| PersonaDefinition | PERSONA_DEFINITION | PERSONA_ID | - | seeded persona catalog |
| PersonaViewPreference | PERSONA_VIEW_PREFERENCE | PREFERENCE_ID | - | per-view resolved preference values |
| UserScopeAssignment | USER_SCOPE_ASSIGNMENT | ASSIGNMENT_ID | - | scoped grants by type/value |
| UserAssetAccess | USER_ASSET_ACCESS | ACCESS_ID | - | asset-level access and expiry |
| OrganizationScope | ORGANIZATION_SCOPE | ORG_SCOPE_ID | - | hierarchy metadata |
| UserAccessAuditEvent | USER_ACCESS_AUDIT_EVENT | EVENT_ID | DETAILS_JSON | immutable access-event log |
| UserProfileAuditEvent | USER_PROFILE_AUDIT_EVENT | EVENT_ID | BEFORE_JSON, AFTER_JSON | immutable profile-change history |
| AuthorizationDecisionTrace | AUTHORIZATION_DECISION_TRACE | TRACE_ID | CONTEXT_JSON | policy engine decision trace |
| SetupWizardLog | SETUP_WIZARD_LOG | LOG_ID | DETAILS_JSON | setup/bootstrap audit telemetry |
| ApplyRowFiltersRequest | APPLY_ROW_FILTERS_REQUEST | APPLY_ROW_FILTERS_REQUEST_ID | EXISTING_FILTERS_JSON | request persistence only if enabled |
| CheckDatabaseAccessRequest | CHECK_DATABASE_ACCESS_REQUEST | CHECK_DATABASE_ACCESS_REQUEST_ID | - | request persistence only if enabled |
| CheckDataSourceAccessRequest | CHECK_DATASOURCE_ACCESS_REQUEST | CHECK_DATASOURCE_ACCESS_REQUEST_ID | - | request persistence only if enabled |
| CheckPermissionRequest | CHECK_PERMISSION_REQUEST | CHECK_PERMISSION_REQUEST_ID | - | request persistence only if enabled |
| CheckPermissionAnyRequest | CHECK_PERMISSION_ANY_REQUEST | CHECK_PERMISSION_ANY_REQUEST_ID | PERMISSIONS_JSON | request persistence only if enabled |
| CheckPermissionAllRequest | CHECK_PERMISSION_ALL_REQUEST | CHECK_PERMISSION_ALL_REQUEST_ID | PERMISSIONS_JSON | request persistence only if enabled |
| CheckRoleRequest | CHECK_ROLE_REQUEST | CHECK_ROLE_REQUEST_ID | - | request persistence only if enabled |
| CheckRowAccessRequest | CHECK_ROW_ACCESS_REQUEST | CHECK_ROW_ACCESS_REQUEST_ID | ENTITY_DATA_JSON | request persistence only if enabled |
| CheckSourceAccessRequest | CHECK_SOURCE_ACCESS_REQUEST | CHECK_SOURCE_ACCESS_REQUEST_ID | - | request persistence only if enabled |

---

## Migration-Manager Validation Checklist

1. Confirm each class resolves one `_ID` key column in generated metadata.
2. Confirm no generated columns use nested collection or dictionary types.
3. Confirm all JSON payload columns are generated as nullable text columns.
4. Confirm all `*_IND` flags are generated as string columns with default values.
5. Confirm table naming follows this document for deterministic migration scripts.

---

## W11 Exit Gate Dependency

W11-03 cannot be marked complete until this contract is validated against generated schema output and any mismatches are corrected in model classes or explicit mapping configuration.

Evidence workflow reference: `MigrationManager-Evidence-Checklist.md`
