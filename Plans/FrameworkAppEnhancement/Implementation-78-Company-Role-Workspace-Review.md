# Implementation 78: company role workspace review

Updated 2026-10-06. Supports [company persona workflows](Company-Persona-Data-Workflows.md). Partial: focused state and SQLite store verified; API, host and Razor wiring remain source-only. No persona journey or phase closes.

## User journey and ownership

After choosing a company, the user sees active workspaces mapped to that company's current approved app roles. A global preferred persona is displayed only if it is eligible in that company. An empty mapping yields an explicit administrator-contact state. Workspace choice changes navigation preferences, never roles, permissions or resource rights. Standard ASP.NET authorization and independently enforced API resource checks remain mandatory.

The existing RepositoryPersonaService, AppPersona catalog/profile/audit, AspNetRoleClaims, RepositoryAccessService and typed PersonaClient remain the owners. `workspace-persona` metadata on existing roles maps persona codes to navigation choices. There is no new table, permission mechanism, inferred role-name mapping, auto-seeding, SQL or business EF migration. Business schema continues through Beep MigrationManager; this extends the existing identity/profile repository owner.

## Source changes

- Data-owned CompanyPersonaWorkspaceReview carries company, actor, authoritative roles, active choices and evidence revision. RoleWorkspaceMappingRequest requires an explicit JSON Enabled value.
- Existing persona owner captures current company access and mapping/catalog evidence twice, refusing changes during review. The revision binds actor/company/roles/mappings/catalog. This is scoped review evidence, not a durable transaction snapshot guarantee.
- Mapping changes require current repository Administrator authority before and after an existing identity-store Serializable transaction. Company-only Administrator is insufficient under the canonical access owner. Enable/disable and audit persist together; unchanged replay adds no duplicate. Inactive enable and duplicate mappings refuse. No grants are created.
- Existing PersonasController adds current-company review and independently guarded mapping routes. PersonaClient consumes the review. No replacement persona store or transport is introduced.
- PersonaContextService validates company, actor, exact roles, revision and active unique choices before publication. A fresh review must match the loaded revision before saving a preference; late company changes cancel publication. Save readback must match the requested account and persona.
- PersonaSelector queues company reloads during busy operations and rejects late navigation after notification reconnect. Landing explains absent company-role assignments. These Razor changes require rendered checks.

## Focused evidence

Actual shared Data sources compiled in an isolated temporary output against actual existing PPDM/DataManagementModels assemblies: zero warnings/errors. No live test output was replaced.

- [CompanyPersonaContextTests](../../Beep.OilandGas.Web.Auth.Tests/CompanyPersonaContextTests.cs): **17 passed, zero failed/skipped, terminal exit 0, 1.2128 seconds**. Actual state/context sources with explicit typed-client/auth fixtures; scope/actor/role/revision/duplicate/inactive refusal, stale eligibility before save, company switching during review, mismatched save readback and previous company-state cases. Log `%TEMP%/oilgas-persona-role-state.log`.
- [RoleWorkspaceTests](../../Beep.OilandGas.Repository.Tests/RoleWorkspaceTests.cs) plus existing RepositoryPersonaTests: **14 passed, zero failed/skipped, 3.8194 seconds**. Actual repository services, mappings and SQLite context, with explicit authority fixture. Covers map/replay/remove/revision/audit, unassigned roles, missing membership, non-admin denial, closing revocation rollback, inactive persona and duplicate mapping refusal. Log `%TEMP%/oilgas-workspace-store.log`. This does not certify canonical membership resolution through an HTTP host or provider concurrency.
- Repeat scoped checks with [state script](../../Scripts/Test-PersonaCompanyState.ps1) and [store script](../../Scripts/Test-PersonaWorkspaceStore.ps1), passing an actual current compiled Data assembly via `-DataAssemblyPath`. Missing production dependencies are not replaced.

Existing HTTP persona fixtures were updated but remain unexecuted. API/controller, DI, standard role transformation, Razor compilation/rendering, accessibility, browser/OIDC, actual provider transactions and concurrent configuration writers remain open.

## Next visible delivery

1. Connect mapping review/edit to the existing administrative role/persona screen owner; no parallel admin surface. Verify canonical approved membership, company-only administrator denial and actual HTTP gates.
2. Render landing/selectors and exercise company switch during load/save/notification reconnect, empty assignments, multiple eligible duties, busy/error/retry and sign-out. Independently deny unauthorized direct routes/API actions.
3. Connect accountant landing to assigned source records and an actual source transaction, independent approval/post, recorded readback and KPI drill-down. Then production/operator-to-maintenance.

The user-approved installed auth/UI/form-validation guidance substitutes for the missing project skills. No additional permission request is needed.
