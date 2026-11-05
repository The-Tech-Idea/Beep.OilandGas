# Implementation 37: account and profile authority

Date: 2026-10-05. Owners: CA-03/05/06 and FG-01/11. Extends Implementation 36. Status: focused persistence, service and direct controller verification; complete HTTP/auth-owner/DI, client and provider concurrency gates remain open.

## Problem and resulting behavior

The repository user service relied on its controller for account authority. Profile overrides accepted incoming Administrator claims, which can represent a selected company's role rather than repository administration. Primary-role selection checked raw Identity memberships, allowing a display preference to name a role assigned outside the current company. Last-administrator account checks counted company assignments as repository administrators.

RepositoryRequestAccount now reuses the API's existing resolved-account delegate, authenticated HTTP principal, RepositoryAccessService and RepositoryRoleAssignmentService's current repository administration check. Every operation requires a currently active resolved account. Incoming role claims cannot grant repository authority. The permission store from Implementation 36 now consumes this same gate; its duplicate account-resolution/check code is removed. This introduces no claim parser, role store or replacement authorization system. IdentityServer remains authentication-only, and standard ASP.NET guards remain on the API.

RepositoryUserService independently checks self or current repository authority for account reads, name updates, deletion status and soft deletion. Directory/username lookup and global role mutations require repository administration. General updates that specify IsActive also require repository administration, including no-op activation values. Controlled self-deactivation derives its subject from the signed-in account and uses the existing transaction and concurrency checks. RepositoryAccountController consumes that operation. Last-administrator checks use current unscoped repository memberships, the existing role normalizer and TimeProvider; company-only administrators do not make the last repository administrator safe to disable. Historical account/role records remain intact.

## Company-scoped profiles and shared contracts

The existing profile service independently checks self or current repository authority, requires the actor to match the validated selected-company context and refreshes company grants before profile operations. Role reads use the existing authorization reader's current selected-company assignments. An organization query argument cannot replace that validated scope. Profiles work before a PPDM database is provisioned; they do not resolve or open a business datasource.

Primary-role selection accepts only a currently granted role in the selected company, through the existing normalized Identity role definition. Repository-only, other-company and unassigned roles cannot be selected. A revoked/expired/pending or removed assignment cannot authorize a new selection. A stored preference disappears from the displayed primary role when the selected company's current grants no longer include it. Ordinary accounts may choose an already assigned display role; this never creates an Identity membership or permission. The primary-role action retains the controller's authentication guard and uses the fresh account gate rather than requiring an Administrator role for a preference change.

All profile controller routes use that same account gate before delegating to profile storage. A repository administrator can override another account's shared profile without an incoming Administrator claim; a company-only or forged principal role cannot. Login recording derives the actor from the account gate and the service restricts it to self, including for administrators. Profile creation and changes record that actor and UTC values from TimeProvider.

UserProfile and its existing preference, primary-role and layout request DTOs move unchanged from Models into TheTechIdea.Data. Their namespaces, wire shapes and model base remain unchanged; no duplicate definitions or forwarding types are added. The actual Models, Data and PPDM project builds verify their consumers in the focused graph. Other shared domain contracts remain subject to the wider ownership audit.

Profiles retain one account-owned APP_USER_EXT record. Preferences/layout/primary-role metadata are shared account preferences; current company roles determine whether the primary-role preference is displayed. This does not introduce company-specific preference persistence. The broader company-experience plan must explicitly decide whether that additional behavior is required before adding schema.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false
```

**377 passed, zero failed/skipped**: previous 324 plus 37 account/profile persistence/service cases and 16 direct profile-controller cases. Production sources use actual Identity managers, EF stores, SQLite foreign keys, current company access and the existing authorization reader. Controller tests use the real account gate and explicitly resolved fixture accounts; a strict profile mock proves refused actions cannot reach profile storage. The service cases use the actual profile implementation and a strict unused business editor. The fixtures do not implement or replace the absent auth-owner libraries.

Coverage includes self versus repository/company authority; forged role claims; directory and global mutation refusal; active-account freshness and unresolved/anonymous/missing accounts; activation restrictions; controlled self-deactivation and last repository administrator preservation; stale update versions; actor/UTC audit; profile reads/preferences/layout/login; selected-company roles and primary-role validation; grant lifecycle and removed Identity membership; company switching and stale actor/context refusal; repository override without incoming roles; controller refusal after repository authority revocation; and unchanged role membership counts. Existing company/client/hub/queue/migration/PPDM row tests remain green.

The older API identity/registration/permission/profile/role-extension fixtures are updated for the shared request gate and actual subjects/actors. LocalDB/direct-action expectations use existing typed refusal contracts. Those wider host/LocalDB fixtures are source updates, not new executed evidence. The focused run does not execute middleware, token validation, complete Program DI, browser clients or a live database provider. Missing analyzer warnings remain outside this focused gate. `git diff --check` passes.

## Remaining work

Verify the complete HTTP/auth-owner/DI/client journeys and supported-provider concurrency, including authority revocation racing an account/profile write and simultaneous last-administrator removal/deactivation. Fresh checks and existing transactions are scoped evidence, not proof of atomic authority across all concurrent operations. Profile preference updates retain the existing unversioned behavior; concurrent preference-write handling remains an explicit gap.

Next audit legacy JWT/field administration and remaining table/domain ownership/reference policies, authoritative asset assignment/existence and shared connection/cache paths. Physical database identity/provisioning, full company isolation, authoritative period close, finance/engineering/operations/regional features and release acceptance remain open under their existing owners.

Business/module schema work remains on Beep Migration; existing Identity/access persistence uses EF. This slice adds no schema migration, handwritten SQL or live database mutation. No company phase or release is complete.
