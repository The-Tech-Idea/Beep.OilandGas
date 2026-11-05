# Implementation 77: company-aware persona state and landing

Updated 2026-10-06. Supports persona-first delivery in [the company persona index](Company-Persona-Data-Workflows.md), CA-03/40 and FG-52. **Partial delivery: state is focused verified; Razor and host wiring are source-connected only. Role-to-workspace eligibility and the persona business journeys remain open.**

## Problem and approved guidance

Landing directly loaded a global preferred persona and navigated its route without a selected-company boundary. PersonaContextService only invalidated on authentication changes, allowing cached profile/catalog state to remain after company changes. Existing account/company, persona client, shared contracts and components are the owners to extend.

The user explicitly approved installed auth/UI/form-validation standards as substitute guidance for the missing project skills on 2026-10-06. The earlier missing-guidance question is resolved. Keep standard app-owned roles, API resource enforcement, MudBlazor and shared theme/CSS/JS conventions. No additional approval is needed for those substitutions.

## Implementation

- [PersonaContextService](../../Beep.OilandGas.Web/Services/PersonaContextService.cs) receives the existing CompanyClientContext and the same authoritative account resolver used by company selection. It clears on company changes, refuses requests without matching selected account/company, rejects foreign-account profiles and discards late publication after a company change, including away/back.
- The typed IPersonaClient boundary is implemented by the existing [PersonaClient](../../Beep.OilandGas.Web/Services/PersonaClient.cs). [Program](../../Beep.OilandGas.Web/Program.cs) aliases that same concrete instance and supplies PartyIdClaims.Find; no replacement transport, identity resolver or persona store is added.
- [Landing](../../Beep.OilandGas.Web/Pages/Landing.razor) uses shared persona state and the existing CompanySelector/PersonaSelector. It shows loading, no company, no active settings and retryable failure states. It checks account/company around awaits, uses the existing local-route validator, rejects unavailable/self-referential destinations and unsubscribes on disposal. It does not silently turn an invalid preferred route into a field dashboard redirect.
- [PersonaSelector](../../Beep.OilandGas.Web/Components/Shared/PersonaSelector.razor) reloads on company changes and retains ordinary state-change rendering without recursive publication reload. No new CSS/JavaScript or business schema path.

## Focused evidence

[Six state cases](../../Beep.OilandGas.Web.Auth.Tests/CompanyPersonaContextTests.cs) passed with zero failures/skips, terminal exit 0, 1.1695 seconds, through [Test-PersonaCompanyState.ps1](../../Scripts/Test-PersonaCompanyState.ps1). This compiles the actual production state/context sources against the actual compiled TheTechIdea.Data assembly and explicit typed-client/auth fixtures. It does not rebuild the live focused repository assembly.

Cases prove company-switch clear/reload, away/back late-result discard, missing/foreign company no request, foreign profile refusal and disposal unsubscribe. Existing HTTP persona tests were updated for constructor/account-company consistency but were not executed in the focused state gate. Log: `%TEMP%/oilgas-persona-company-state-final.log`.

The initial transport-focused harness could not compile the current shared diagnostics/identity dependencies from the available older binaries. No missing project was cloned and no production dependency shim was introduced. The isolated state check uses an explicit client interface fixture; it does not substitute for HTTP/client/identity integration evidence.

## Next visible delivery gates

1. Resolve authoritative role-to-persona eligibility in the active app repository/API owner; do not infer it from global preference, role-name resemblance or the separate Beep persona store. Use current company-approved standard roles and resources.
2. Connect eligible landing choices and multi-duty selection to that API review. Verify direct route/API denial independently.
3. Compile and render the actual Landing, CompanySelector and PersonaSelector, exercise company switch during load/save, busy/retry and sign-out, and verify full DI/OIDC/browser behavior. Source-only Razor changes are not a delivered screen.
4. Deliver the accountant landing -> source invoice/allocation/journal -> independent approval/post -> recorded outcome -> KPI drill-down, then the operator/engineer-to-maintenance handoff.

No persona, feature, phase or complete implementation goal closes. Beep MigrationManager remains the business schema owner; no handwritten SQL, business EF lane, role grants or live migration was added.
