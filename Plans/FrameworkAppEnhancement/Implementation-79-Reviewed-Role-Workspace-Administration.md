# Implementation 79: reviewed role workspace administration

Updated 2026-10-06. Extends [Implementation 78](Implementation-78-Company-Role-Workspace-Review.md) and [company administrator journey](Persona-12-Company-Administrator.md). Partial: actual identity repository and canonical company-role checks have focused evidence. Role administration screen/API/client wiring remains source-only; complete persona journeys remain open.

## Visible workflow

The existing [User Roles page](../../Beep.OilandGas.Web/Pages/Admin/AccessControl/UserRoles.razor) is the current role-management surface. Extend it with a role selector, assigned/unassigned workspace names, inactive indicators and explicit Assign/Remove buttons. Controls disable during an operation; loading progress, no catalog entries and retryable errors are visible. MudBlazor provides semantic buttons, labelled selectors and responsive wrapping; no custom CSS/JavaScript or new route is added. Keyboard, screen-reader, RTL and responsive runtime checks remain open.

The editor explains that mapping affects navigation wherever the app role is assigned and grants no data/actions. Existing role creation and user assignment remain separate operations. No names are guessed or auto-seeded. A company-only administrator is refused independently by the repository/API.

## Implementation and recorded outcome

- Data owns RoleWorkspaceReview and RoleWorkspaceChoice. The review includes actual role identity, catalog entries, mapped state and SHA256 evidence revision. The mapping request requires explicit Enabled and ExpectedRevision JSON members.
- RepositoryPersonaService reads current global role metadata and catalog twice with independent current repository Administrator checks. Duplicate/orphan mappings and oversized catalogs require reconciliation. No separate mapping store is added.
- Save checks the original revision inside the existing identity-store Serializable transaction before changing any mapping or audit. Changed catalog/role/mapping evidence refuses with no overwrite. Existing post-write administrator recheck and atomic audit remain. No new business EF or SQL/schema lane.
- PersonasController adds the independently guarded GET review and consumes the original revision on PUT. PersonaClient uses the same ApiClient transport and encoded path segments.
- The existing page reloads persisted assignments after a save and checks the exact requested role/code/state before displaying success. Failed or unconfirmed outcomes clear the editable review and instruct an explicit reload; writes are never automatically repeated.

## Verification

Focused store gate compiles actual repository/access/mapping sources against an actual current shared Data assembly and SQLite. New cases use the actual RepositoryAccessService for approved/pending/revoked company assignment eligibility and company-only Administrator denial. Other transactional/mapping cases use a declared authority fixture.

Run [Test-PersonaWorkspaceStore.ps1](../../Scripts/Test-PersonaWorkspaceStore.ps1) with `-DataAssemblyPath` to the actual current Data assembly. Final focused result: **20 passed, zero failed/skipped, terminal exit 0, 3.8616 seconds**. Log: `%TEMP%/oilgas-workspace-store-final.log`. Both JSON-required member cases passed. Earlier checkpoint: 18 passed, zero failed/skipped, terminal exit 0, 3.7975 seconds. This is SQLite/store evidence, not native HTTP/OIDC/provider or rendered UI proof.

## Next gates

Compile/render the real role administration and company landing components and exercise save/readback/reload, standard route authorization, independent native API gates, company switch and sign-out. Then prioritize accountant assigned work -> source transaction -> independent approval/post -> recorded reload -> dashboard drill-down. The accounting landing currently inventories process links; it does not yet deliver its planned daily finance queue.

No persona, CA/FG feature, phase or full implementation goal closes. Installed auth/UI/form guidance is already approved.
