# Beep.OilandGas.Web Architecture And Oil & Gas Fit Assessment

Date: 2026-04-23

## 1) Review Scope And Method

This assessment reviews the Beep.OilandGas.Web application structure, key runtime paths, and domain implementation readiness for oil and gas operations.

Because the web project is large (224 Razor files and 77 C# files), the review used a full structural scan plus deep inspection of startup, routing, layout, navigation, service boundaries, setup workflow, and representative domain pages.

### Project size snapshot

- Razor files: 224
- C# files: 77
- Pages: 131
- Components: 88

### Domain page coverage snapshot (Pages/PPDM39)

- Accounting: 5
- BusinessProcess: 13
- Calculations: 12
- Compliance: 4
- Data: 3
- DataManagement: 4
- Decommissioning: 4
- Development: 6
- Economics: 2
- Exploration: 8
- HSE: 5
- Operations: 3
- Production: 11
- Properties: 1
- Pumps: 3
- Reservoir: 4
- Seismic: 3
- Setup: 2
- Stratigraphy: 4
- WellDetails: 5
- Wells: 3
- WorkOrder: 3

## 2) Current Application Structure

## 2.1 Runtime composition

The app is a Blazor Server app using MudBlazor with server-side interactive rendering.

- Host and startup: Beep.OilandGas.Web/Program.cs
- App shell: Beep.OilandGas.Web/App.razor
- Router entry: Beep.OilandGas.Web/Components/Routes.razor

The runtime currently maps routes through MainLayout.

## 2.2 Authentication and identity

Authentication is OpenID Connect + Cookie-based auth with token capture for API calls.

Evidence:

- OIDC configuration and scopes: Beep.OilandGas.Web/Program.cs:147
- offline_access scope: Beep.OilandGas.Web/Program.cs:160
- Auth provider registration: Beep.OilandGas.Web/Program.cs:110

## 2.3 UI framework and shell

MudBlazor is properly registered and used as the shell framework.

- Mud services: Beep.OilandGas.Web/Program.cs:45
- Theme providers and shell: Beep.OilandGas.Web/Shared/MainLayout.razor:8
- Process-centric menu model: Beep.OilandGas.Web/Shared/NavMenu.razor

## 2.4 Service and API boundary

The app uses a combination of typed domain clients and shared service facades.

- Typed clients registration block starts: Beep.OilandGas.Web/Program.cs:341
- Data management façade: Beep.OilandGas.Web/Services/DataManagementService.cs
- Domain clients (exploration, production, compliance, etc.) are registered as scoped services.

## 2.5 Setup and database onboarding flow

The app includes first-run setup and migration-governed schema creation with artifact handling in the setup flow.

- Setup wizard page: Beep.OilandGas.Web/Pages/PPDM39/Setup/DatabaseSetupWizard.razor
- First-run service registration: Beep.OilandGas.Web/Program.cs:376

## 3) Oil & Gas Product Fit Assessment

## 3.1 What is strong and aligned

The app is strongly aligned with petroleum workflows in breadth and navigation:

- End-to-end lifecycle domains are represented (Exploration, Development, Production, Reservoir, Economics, HSE, Decommissioning).
- Process-centric top navigation is present instead of pure table CRUD navigation.
- Field-aware context patterns are present in many pages and domain services.
- PPDM39 setup and governance features are built into onboarding and data admin flows.
- Work-order and compliance surfaces exist, which are critical for operational oil and gas organizations.

Verdict on functional direction: Correct direction for an oil and gas web platform.

## 3.2 What still blocks enterprise correctness

The app is close to a correct O&G structure, but not yet fully enterprise-correct due to security and consistency gaps listed in Findings.

### Fit score

- Domain/workflow fit: 8.5/10
- Architecture and platform fit: 7.5/10
- Enterprise readiness: 6.5/10
- Overall: 7.5/10

## 4) Findings (Ordered By Severity)

## High severity

1. Hardcoded OIDC client secret in web startup

- Evidence: Beep.OilandGas.Web/Program.cs:149
- Why it matters: Shipping secrets in source creates credential leakage risk and weakens identity security posture.
- Impact: Production security and compliance risk.

2. First-run gating logic exists in DefaultLayout, but router defaults to MainLayout

- Evidence: Beep.OilandGas.Web/Components/Routes.razor:4
- Evidence: Beep.OilandGas.Web/Components/Layout/DefaultLayout.razor:114
- Why it matters: Setup enforcement can diverge depending on which layout is active; this can bypass intended onboarding governance.
- Impact: Environment setup drift and inconsistent user entry behavior.

## Medium severity

3. UI guideline drift from no-inline-style rule

- Evidence examples:
	- Beep.OilandGas.Web/Components/Shared/PPDMMapView.razor:48
	- Beep.OilandGas.Web/Components/Reservoir/ReservesChart.razor:34
	- Beep.OilandGas.Web/Pages/PPDM39/Compliance/GHGReport.razor:12
- Why it matters: Reduces theming consistency, increases maintenance cost, and weakens dark/light mode consistency.
- Impact: Design system fragmentation and slower UX hardening.

4. Layout model duplication and potential ambiguity

- Evidence:
	- Beep.OilandGas.Web/Shared/MainLayout.razor
	- Beep.OilandGas.Web/Components/Layout/DefaultLayout.razor
	- Beep.OilandGas.Web/Components/Layout/RoleBasedLayout.razor
- Why it matters: Multiple layout strategies can drift and cause inconsistent auth/setup behavior.
- Impact: Higher regression risk when changing shell behavior.

5. Some dashboards show placeholders where production charting/data streams are expected

- Evidence: Beep.OilandGas.Web/Pages/PPDM39/Production/ProductionDashboard.razor:147
- Why it matters: Core operations dashboards in O&G need live trend and exception management for operational decision-making.
- Impact: Reduced operational utility for real-time teams.

## Low severity

6. Route and navigation complexity is high, making discoverability and consistency validation harder

- Evidence: Beep.OilandGas.Web/Shared/NavMenu.razor
- Why it matters: Large nav trees can produce dead routes, overlapping surfaces, and uneven UX quality over time.
- Impact: Usability and maintainability overhead.

## 5) Is This A Correct Web App For Oil And Gas Companies?

Short answer: Mostly yes in business direction and module coverage, not yet fully yes in enterprise hardening.

The app clearly models upstream and production lifecycle processes and includes oil-and-gas-specific capabilities (well operations, reservoir/economics, HSE, work orders, setup/governance). This is not a generic admin UI.

However, to be fully correct for enterprise oil and gas deployment, it must close security and governance consistency gaps first.

## 6) Recommended Next Actions (Priority Order)

1. Security hardening immediately

- Move OIDC client secret to secure configuration and secret manager.
- Remove hardcoded secret from source and rotate credentials.

2. Layout and setup-gate consolidation

- Choose one canonical runtime layout path for setup/auth gating.
- Ensure first-run guard is applied in the active default layout path.

3. UI governance cleanup

- Remove inline style attributes and replace with MudBlazor class-based utilities and component parameters.
- Keep all visual rules centralized in theme/components.

4. Operations dashboard completion

- Replace placeholder trend blocks with real data-backed charting on production-critical pages.

5. Navigation and route governance

- Add route inventory tests/checks to detect missing pages, duplicated concepts, or stale links.

## 7) Conclusion

Beep.OilandGas.Web is already a strong oil-and-gas-oriented platform foundation with broad lifecycle scope and process-centric UX intent.

It is on the correct product path for oil and gas companies, but should be considered pre-hardening for enterprise production until the high-severity security and layout-governance findings are addressed.

