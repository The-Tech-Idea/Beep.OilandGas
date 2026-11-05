# Implementation 51 — Company FIELD interest administration

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/10/11/12/20. Status: implemented with focused state, exact component compilation and static page rendering evidence. Full host/auth/HTTP/browser/provider acceptance remains open; no phase closure.

## Problem and design

Implementation 50 has verified API/SDK review evidence, but the existing asset administration page has no consumer. Extend that page and its existing AssetGrantAdministrationState; keep the existing typed client, API facade, shared contracts and PPDM owners. Do not create another review service or persistence path.

One labeled exact FIELD ID input and a Review FIELD Interests button work independently of the subject-account grant form. Standard page Administrator and selected-company Administrator gates remain mandatory; the API independently requires the administrator's own FIELD access. No subject user ID is sent to the review endpoint. New company/account/field selections, another administration load and disposal cancel outstanding work and clear prior results. Only the captured current request can publish a response or an error.

The review displays company and FIELD provenance, server review time, the reviewed date zone, complete set sequence and component/partner observation keys, recorded type/role, raw gross/net percentage values and recorded breach/escrow/right-to-earn indicators. It groups canonical records by complete set identity instead of flattening them into another DTO. Read-only facts do not assign title, operator, billing or app rights. No percentage normalization, fabricated dates, indicator/role interpretation or legal approval action is added.

The [shared review contract and validator](../../TheTechIdea.Data/OilGas/FieldInterestReview.cs) carry the server's reviewed date zone and canonical typed invariants. The [SDK](../../Beep.OilandGas.Client/App/Services/AccessControl/AccessControlService.cs) and [existing state partial](../../Beep.OilandGas.Web/Services/AssetGrantAdministrationState.Interests.cs) call that same validator; raw JSON numeric-presence validation remains in the SDK. This avoids duplicating contract traversal while preventing malformed or foreign response display. The [API reviewer](../../Beep.OilandGas.ApiService/Services/PPDMCompanyFieldInterestReview.cs) returns the actual reviewed zone ID rather than deriving it in the browser.

The [existing page](../../Beep.OilandGas.Web/Pages/Admin/AccessControl/AssetAccess.razor) renders the review without a new component/service owner. It uses the existing state constructor/DI registration and shared pending request/generation/company snapshot lifetime. FIELD changes clear prior administrative results; starting another administration load cancels the previous load and clears its results. Company/account/subject changes clear the review and FIELD draft. Errors clear the previous review and allow an explicit retry; error explanation is rechecked against current scope too. A late completion cannot replace a newer result or clear its loading state. Grant editing remains disabled until its own direct-grant load succeeds.

## Component states and accessibility

Default/filling: exact ID input with helper text; review button enabled only for a valid ID/current company Administrator. Invalid input gets an inline text error. Loading: shared Busy state disables administration actions and a labeled progress indicator reports pending work. Success: canonical records and provenance. Empty: explicit no current recorded company interest, without an ownership conclusion. Errors: existing actionable error explanation and no old result. Hover/focus/pressed/disabled styling comes from the app's MudBlazor theme; no CSS/JavaScript/theme source is added.

Use MudBlazor controls and labeled responsive table regions/captions; keys and dates stay visible. Small screens can scroll each evidence table. Numeric/date values are displayed explicitly rather than interpreted as decisions. Keyboard, screen-reader, light/dark, RTL and browser layout acceptance will be recorded separately from focused state tests/component compilation.

## Files and verification

The existing [focused project](../../Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj) links exact state source. [Administration tests](../../Beep.OilandGas.Repository.Tests/CompanyFieldInterestAdministrationTests.cs) cover actor/company/role/FIELD/subject/disposal changes, cancellation, stale successes/errors, superseded reads, malformed composite identities/response limits, no-subject review, empty/full results, failed refresh/retry and an actual API/facade/SDK/state round trip. Four [additional SDK cases](../../Beep.OilandGas.Repository.Tests/CompanyFieldInterestReviewTests.cs) refuse missing/padded date zones, missing set type and default review time.

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
./Scripts/Test-HierarchyWebComponents.ps1 -RenderFieldInterests
```

**1235 passed, 0 failed, 0 skipped**, adding 51 cases to the previous 1184 (47 administration cases and four SDK cases). Final focused duration: 1 minute 55 seconds. Known missing analyzer/shared-project warnings remain; no shared projects are manufactured. The complete gate retains existing entitlement, admission, stored-grant, policy and hierarchy coverage after the validator/response extension.

The [component check script](../../Scripts/Test-HierarchyWebComponents.ps1) compiles all three exact-source access-control components against the actual focused production-state assemblies and MudBlazor 9.7.0, with zero warnings/errors. Its opt-in static HtmlRenderer mode then renders the actual AssetAccess page in five cases: English/French populated results, empty, error and loading. It checks unchanged raw percentages, encoded recorded text/indicators, complete set sequence, captions/header scopes/focusable regions, UTC/date-zone provenance, absence of invented dates/rows and explicit busy/error states. Temp output contains reproducible console evidence and rendered HTML. Fixture records remain test-only.

Static rendering directly instantiates the component and does not exercise route authorization, OIDC, real HTTP, DOM events, JavaScript, browser keyboard/screen-reader behavior or production PPDM providers. Focused API/SDK round trips use the existing in-process dispatcher and SQLite authority/recording Beep datasource. Full Web/API host/DI, localization/RTL, light/dark/responsive presentation, browser accessibility, provider snapshots/upgrades/concurrency and deployment configuration acceptance remain open. This is scoped proof, not promotion of overall foundation readiness.

No schema migration, handwritten SQL or live datasource mutation is required. Business/module schemas retain Beep MigrationManager; the existing approved Identity/access EF owner is unchanged. Legal/operator/JV approvals, land-right evidence, financial/domain boundary repairs and the broader plan remain open.

## Next work

Continue explicit legal/operator/JV domain decisions through canonical interest/land-right and existing workflow/command owners. Implementation 50 records the distinct economic-interest, billing and joint-arrangement owners and their remaining connection/resource/actor/default gaps. Repair those paths rather than add financial or title stores. Continue remaining local-mode/gateway/cache/provenance/provisioning boundaries before CA-04 period close; root CA/FG tasks retain their owners and remain partial.
