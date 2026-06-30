# AGENTS.md

## Project Overview

TheTechIdeaWeb is a **.NET 10 Aspire-orchestrated** solution with multiple Blazor Server applications, a shared IdentityServer, and REST APIs. All Blazor apps share theming/branding infrastructure via `TheTechIdeaWeb.ThemeBranding`.

## Canonical System Plan

### 1) API-First Data Access (Mandatory)

- **TheTechIdeaWeb.ApiService is the only server allowed to access application databases for client-facing features.**
- All client apps (TheTechIdeaWeb.Web, TemplateApp, BeepDiA, Events apps, and future clients) must read/write business data through ApiService endpoints.
- No client app may open direct SQL/EF connections to domain databases.

### 2) Data Ownership (Mandatory)

- **TheTechIdea.Data is the single source of truth for domain data.**
- All EF Core entities, DTOs, enums, and data contracts for shared business modules live in TheTechIdea.Data.
- ApiService can define service-layer orchestration, validators, and interfaces, but must not duplicate domain models already owned by TheTechIdea.Data.

### 3) BeepDiA Administrative Boundary

- **BeepDiA is the full admin implementation surface** for management use cases (products, offers, catalog configuration, and other table-backed admin workflows).
- BeepDiA performs admin operations by calling ApiService admin/resource endpoints.
- TheTechIdeaWeb.Web can expose business-facing and selected admin experiences, but core operational management workflows should be implemented in BeepDiA.

### 4) Identity Boundary (Authentication vs Authorization)

- **Beep.Foundation.IdentityServer is authentication-only.**
- IdentityServer is responsible for user/company sign-in, token issuance, and client authentication trust.
- **Authorization (roles, permissions, policy decisions) belongs to the CLIENT APP** and uses **standard ASP.NET Core roles** (`[Authorize(Roles = "...")]`, `<AuthorizeView Roles="...">`, `User.IsInRole(...)`).
- Client app roles are sourced from the **client app's own RBAC tables** (e.g. `AspNetUserRoles` for TheTechIdeaWeb apps, `PlatformAdmins` for Beep.EventsRegistration) — NOT from IdentityServer's `role` claim.
- Do not move business authorization logic into IdentityServer, and do not move it to ApiService either. The client app owns authorization; the API enforces authorization for its own endpoints independently.

### 4a) Standard ASP.NET Roles Pattern (Mandatory)

Each client app implements the **canonical bridge** between IdentityServer's auth-only OIDC tokens and ASP.NET Core's standard role-based authorization:

1. **IdentityServer** issues OIDC tokens WITHOUT a `role` scope/claim (authentication only).
2. **The client app's API** (e.g. `MeController.GetMe` for TheTechIdeaWeb apps, `AuthEndpoints.GetPlatformAdminStatus` for Beep.EventsRegistration) is the read-only gateway that resolves the current user's authoritative role set from the client app's own RBAC tables.
3. **An `IClaimsTransformation` in the client app** calls the API on first `[Authorize]` evaluation and synthesizes a standard ASP.NET role claim (`ClaimTypes.Role = "<role-name>"`) onto the `ClaimsPrincipal`. This is the standard ASP.NET Core pattern for bridging external auth + in-app roles. Use a marker claim to avoid repeat upstream calls per request. Fail-closed on errors.
4. **Pages and components** use the standard ASP.NET mechanisms directly:
   - `@attribute [Authorize(Roles = "Admin")]` for page-level route guards
   - `<AuthorizeView Roles="Admin">` for component-level conditional rendering
   - `@inject AuthenticationStateProvider` and `User.IsInRole("Admin")` for imperative checks
5. **The client app's own API** independently enforces role checks on its protected endpoints (e.g. `[Authorize(Roles = "Admin")]` on `AdminController`). This is defense-in-depth: the client app's UI gates the user, and the client app's API gate the data.

This pattern keeps IdentityServer auth-only, keeps authorization in the client app using standard ASP.NET roles, and keeps the role source-of-truth in the client app's own RBAC tables. Do NOT invent a custom non-standard authorization mechanism; the standard ASP.NET roles pattern is the correct approach.

## Key Architecture Rules

- **Blazor frontends never access databases directly** — use typed API clients
- **ApiService is the canonical data gateway for all client apps**
- **TheTechIdea.Data owns shared entities/DTOs/enums for EF Core and API contracts**
- **IdentityServer handles authentication only; client apps handle authorization using standard ASP.NET Core roles**
- **Each client app implements the canonical IClaimsTransformation bridge** so IdentityServer's auth-only tokens work with standard `[Authorize(Roles=...)]` guards
- **MudBlazor first** — prefer MudBlazor components and theme system over custom HTML/CSS
- **Theme-driven CSS only** — all custom CSS must reference `--mud-palette-*` tokens, never hardcoded hex
- **One theme source per experience root** — no page-local `new MudTheme` palettes
- **BrandingConfig is the single source of truth** for colors, typography, logos

## End-to-End Workflow

1. Define or update entity/DTO/enum in TheTechIdea.Data.
2. Apply EF Core migration from the API data layer setup.
3. Implement/extend ApiService endpoint + service logic using TheTechIdea.Data models.
4. Enforce authorization in ApiService (role/permission checks sourced from data model).
5. Consume endpoint from clients through typed HTTP clients.
6. Implement admin workflows primarily in BeepDiA for product/offer/management operations.
7. Keep IdentityServer focused on authentication flows and token issuance.

## Implementation Guardrails

- No duplicate business DTO/entity classes across client projects.
- No business authorization policy embedded in IdentityServer UI/server logic.
- No direct DB access from Blazor projects.
- Prefer additive API versioning when changing externally consumed contracts.
- For admin features that alter persistent business state, implement API-first then consume from BeepDiA/Web.
- **CSS in ONE file** — all custom CSS goes in `wwwroot/css/app.css`. No `<style>` blocks in `.razor` files.
- **JS in ONE file** — all custom JavaScript goes in `wwwroot/js/site.js`. No `<script>` blocks in `.razor` files.
- **All colors via `--mud-palette-*` tokens** — never hardcode hex in `Style=""` attributes. Use MudBlazor `Color` props, `terminal-*` CSS classes, or `var(--mud-palette-*)` references.

## Skills

Before modifying code in these areas, read the corresponding skill:

| Area | Skill |
|------|-------|
| IdentityServer UI | `.github/Skills/identityserver-mudblazor-ui/SKILL.md` |
| Web app auth & branding | `.github/Skills/beep-web/SKILL.md` |
| Theme & branding | `.github/Skills/theming-branding/SKILL.md` |
| MudBlazor API reference | `.github/Skills/mudblazor-api/SKILL.md` |
| Terminal UI design (TermilUI) | `.github/Skills/termilui/SKILL.md` |
| UI/UX design guidance | `.github/Skills/ui-ux-pro-max/SKILL.md` |
| Data management (BeepDM) | `.github/Skills/beep-dm/SKILL.md` |

## Theme & Branding

- All theme classes live in `TheTechIdeaWeb.ThemeBranding` (shared library with MudBlazor reference)
- Use `BrandingConfigMudThemeMapper.CreateMudTheme()` for ALL BrandingConfig→MudTheme mapping
- Use `BrandingConfig.GetRtlClone()` for RTL font adjustments
- Use `BrandingConfigLoader.LoadFromFile()` for loading theme JSON
- CSS must use `--mud-palette-*` variables, not custom `--primary`/`--surface`
- No `<style>` blocks in Razor that inject custom `:root` variables

## Building & Running

```powershell
dotnet run --project TheTechIdeaWeb.AppHost
```

## Documentation

- Full architecture: `.github/copilot-instructions.md`
- Theme how-to: `TheTechIdeaWeb.ThemeBranding/HOWTO.md`
- IdentityServer: `Beep.Foundation.IdentityServer/docs/`
- Data architecture rules: `docs/data/ARCHITECTURE_RULES.md`
- Identity and web architecture: `docs/identity-and-web/01-ARCHITECTURE.md
