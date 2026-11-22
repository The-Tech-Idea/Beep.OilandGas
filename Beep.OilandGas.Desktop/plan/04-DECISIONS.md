# Decisions — Beep.OilandGas.Desktop

**Document ID:** PLAN-04
**Version:** 1.0
**Created:** 2026-11-05

Each decision below is a **proposal with a probe**. A decision becomes `Accepted` only when the
probe has been run and its output recorded under `plan/evidence/<ADR-id>/`.

---

## ADR-001 — Unify SkiaSharp on 4.154.0 and retire `SkiaSharp.Svg`

**Status:** **Accepted (2026-11-05)** · **Blocks:** STG-15, STG-16, INT-07 · **Evidence:** [`evidence/ADR-001-002-skia`](evidence/ADR-001-002-skia/observed-evidence.json)

### Context

Three incompatible SkiaSharp declarations exist today:

| Group | SkiaSharp | SVG helper |
|---|---|---|
| `Beep.OilandGas.Models` | `4.154.0` | `SkiaSharp.Svg 1.60.0` |
| 15 calculation projects | `3.119.4` | `SkiaSharp.Svg 1.60.0` |
| `Beep.OilandGas.Drawing`, `.HeatMap` | `3.119.4` | `Svg.Skia 5.1.1` |
| `TheTechIdea.OilGas.*` | `4.150.1` | none |

`Directory.Build.props` already sets `NoWarn=$(NoWarn);NU1605`, so NuGet silently resolves the
highest version (`4.154.0`) for the whole graph. In-repo comments in `Drawing.csproj` and
`HeatMap.csproj` state that `SkiaSharp.Svg 1.60.0` calls SkiaSharp 1.x APIs and throws
`MissingMethodException` against SkiaSharp 3+.

### Decision

Unify on **`SkiaSharp 4.154.0`** across the desktop graph and replace every `SkiaSharp.Svg`
usage with `Svg.Skia` — matching what `Drawing` and `HeatMap` already did.

### Consequences

- The 15 calculation projects keep their `3.119.4` declaration; NuGet resolves 4.154.0. This is
  the current behaviour and already ships.
- Any project still compiling against `SkiaSharp.Svg` must be migrated. Locate with
  `grep -r "SkiaSharp.Svg" --include=*.cs --include=*.csproj`.
- This is a **change to source repositories**, so it needs its own scoped approval and must not
  be bundled into a desktop stage.

### Probe result — accepted

A `net10.0` probe with **SkiaSharp 4.154.0 + Svg.Skia 5.2.3** restored, referenced and ran: it parsed an
SVG and painted the exact expected pixel `#ff0b6e4f`. `Svg.Skia` is therefore the SVG path for the
desktop, and `SkiaSharp.Svg 1.60.0` is retired.

### Fallback

If a future SkiaSharp change breaks native asset loading, pin **3.119.4** everywhere and down-bump
`Beep.OilandGas.Models` and the `OilandGasTools` packages. That is a larger change; prefer 4.154.0.

---

## ADR-002 — WPF Skia control

**Status:** **Accepted with a change (2026-11-05)** · **Blocks:** VSB-01 · **Evidence:** [`evidence/ADR-001-002-skia`](evidence/ADR-001-002-skia/observed-evidence.json)

### Context

`OilandGasTools` needs a canvas on WPF. `SkiaSharp.Views.WPF` 4.154.0 **exists**, so it looked like the
obvious host.

### Probe result — the package is .NET Framework only

A `net10.0-windows` WPF project referencing `SkiaSharp.Views.WPF` 4.154.0 builds with 0 errors, but
NuGet reports **NU1701**: the package was restored with `.NETFramework 4.6.1–4.8.1` assets, not the
`net10.0-windows` target. It also pulls `OpenTK 3.3.1` and `OpenTK.GLWpfControl 3.3.0`, both
.NET Framework as well. So `SKElement` would run as a net4x WPF type inside a .NET 10 host.

### Decision

**Do not use `SkiaSharp.Views.WPF`.** Implement `SkiaViewSurfaceHost` as:

```
SkiaSharp 4.154.0  →  SKSurface / SKBitmap  →  byte[]  →  WriteableBitmap  →  Image
```

refreshed when `ViewSurface.Invalidated` fires, with input forwarded through `HandleMouse*`,
`HandleMouseWheel`, `HandleKey`, `HandlePinch` and `HandleTwoFingerPan` exactly as written in VSB-01.

### Probe result — the fallback works

A `net10.0-windows` probe with **SkiaSharp 4.154.0 only** rendered a surface and pushed the pixels into
a WPF `WriteableBitmap`:

```
skia: 4.154.0.0
bitmap: 64x64 Bgra32
fallback-host: ok
```

### Consequences

- No extra package, no net4x assembly, no OpenTK in the desktop.
- VSB-01 keeps its contract; only its implementation changes.
- `ViewSurface` draws into an `SKCanvas` from an `SKSurface` we own, which is also the path the PDF/SVG
  exporters already use.
- Cost to watch: a full-canvas pixel copy per redraw. that is the thing to measure in VSB-01 and REL-04.

---

## ADR-003 — Target framework

**Status:** **Accepted (2026-11-05)** · **Blocks:** STG-00 · **Evidence:** [`evidence/ADR-003`](evidence/ADR-003/observed-evidence.json)

### Context

- `TheTechIdea.Beep.WPF.Controls` targets `net9.0-windows`.
- All `Beep.OilandGas.*` target `net10.0`.
- All `TheTechIdea.OilGas.*` target `net8.0;net9.0`.

### Decision

`net10.0-windows` for `Host`, `Controls`, `Modules`, `WellTools`; `net10.0` for `Core` and `Data`.

### Probe result

A scratch `net10.0-windows` project referencing `TheTechIdea.Beep.WPF.Controls` **builds with 0 errors**,
and `TheTechIdea.Beep.WPF.Controls.Styling.BeepControlStyle` (63 design-system values) resolves.
`net10.0-windows` consumes the `net9.0-windows` and `net9.0` assets without a downgrade. Accepted.

### Finding: the controls library drags 9 278 CA1416 warnings

The reference pulls `TheTechIdea.Beep.Vis.Modules`, whose WinForms theme code raises `CA1416`
("only supported on windows") once per call site — 9 278 warnings on a clean build of that project.
They are not our code, but they would break the programme's **0-warning** gate the moment FND-04 wires
`BeepChrome`.

**Mitigation (required before FND-04):** set `NoWarn=$(NoWarn);CA1416` on the project that references
the controls library, with a one-line comment naming the owner, and keep `CA1416` enabled everywhere
else. Record it in FND-04 §9 rather than silently suppressing it solution-wide.

### Consequences

`Core` and `Data` stay OS-agnostic and testable without a desktop session.

---

## ADR-004 — Data access mode

**Status:** Proposed · **Blocks:** STG-00 · **Related:** G-03

### Context

`AGENTS.md` requires API-first data access for client apps. `Beep.OilandGas.Client` already
offers `Remote`, `Local` and `Auto`. `Beep.DoitAll.Erp.Wpf` uses a local BeepDM connection for
personal deployments and a company API for multi-site.

### Decision

Default `ServiceAccessMode.Auto`:

1. If a company API base URL **and** a usable token are configured → `Remote`.
2. Otherwise → `Local`, composed through `AddBeepOilandGasAppLocal` or, if that extension does
   not cover every service the desktop needs, `LocalServiceComposer`.

A user-visible indicator in the shell shows the active mode, and no page may change it silently.

### Consequences

- Remote mode needs an `IAuthenticationProvider` implementation (the Client throws without one);
  `OgExternalIdentityService` supplies it.
- Local mode is the offline/personal path and is what `--smoke` exercises.
- Health checks must distinguish "API unreachable" from "no data"; a failed remote read is not an
  empty result.

### Probe

Run a page's read against (a) a stub HTTP API returning one well and (b) a local SQLite PPDM
database. Both must produce the same DTO shape.

---

## ADR-005 — One `Modules` assembly with folder-per-area

**Status:** Proposed · **Blocks:** STG-00

### Context

The ERP reference puts ~40 view models and ~45 views in one assembly. This programme covers 130
features across 22 areas and will reach ~230 files.

### Decision

One `Beep.OilandGas.Desktop.Modules` assembly, folder per area, `Views/` + `ViewModels/` inside
each. Areas can be split into satellite assemblies later without changing namespaces if the
namespace is already `...Modules.<Area>`.

### Consequences

Faster iteration, one DI registration point, no cross-assembly friend-assembly leakage. Risk:
a single very large assembly at completion (~230 files). Accepted for 1.0; revisit at 1.1.

---

## ADR-006 — `HeatMap` module is deferred

**Status:** Proposed · **Blocks:** nothing · **Gap:** G-02

### Context

`Beep.OilandGas.HeatMap` registers `HEAT_MAP` and exposes `HeatMapService` +
`HeatMapRenderer`, but `Beep.OilandGas.Web` has no equivalent page — `Pages/PPDM39/Properties/HeatMap.razor`
shows a summary only and does not render the canvas.

### Decision

Defer the dedicated heat-map screen from 1.0. Surface the renderer instead through `INT-07`
(mapping), which already hosts a Skia canvas and can render a heat-map layer without a new page.

### Consequences

The module stays registered and seeded; no feature doc is created for it; the master tracker
records the deferral with this ADR as the reason.

---

## ADR-007 — Lease acquisition gets its own area

**Status:** Proposed · **Blocks:** STG-03 · **Gap:** G-01

### Context

`Beep.OilandGas.LeaseAcquisition` registers `LEASE_ACQUISITION` (order 45) and exposes
`ILeaseAcquisitionService`, `ILeaseAcquisitionAdvancedService` and `ILeaseManagementService`
across nine partials, plus an extension table set
(`LEASE_ACQUISITION`, `FEE_MINERAL_LEASE`, `GOVERNMENT_LEASE`, `NET_PROFIT_LEASE`,
`R_LEASE_REFERENCE_CODE`). The Web has one thin page (`Operations/LeaseAcquisition.razor`:
`/ppdm39/operations/leaseacquisition`). `Beep.OilandGas.Client` already exposes
`IBeepOilandGasApp.Lease`.

### Decision

Add a **`LSE` area** with three features rather than hiding lease work inside exploration:

| ID | Feature | Source |
|---|---|---|
| `LSE-01` | Lease portfolio and register | `LeaseManagementService`, `LEASE` |
| `LSE-02` | Acquisition pipeline and due diligence | `LeaseAcquisitionService` (`DueDiligence`, `Negotiation`, `Rights`, `Stakeholders`) |
| `LSE-03` | Lease financials and lifecycle | `LeaseAcquisitionService` (`Financial`, `Lifecycle`) |

Insert as **STG-03a** running in parallel with STG-03, with the same exit gate style.

### Consequences

Feature total becomes **130**. Master tracker and stage index must be updated in the same edit
that accepts this ADR. Until accepted, `Lease` remains an explicit gap and must not silently
disappear.

---

## ADR-008 — How the desktop consumes `OilandGasTools`

**Status:** **Accepted (2026-11-05)** · **Blocks:** STG-15 · **Evidence:** [`evidence/ADR-008`](evidence/ADR-008/observed-evidence.json)

### Context

`OilandGasTools` publishes 13 packages (`TheTechIdea.OilGas.*`, one version line via MinVer,
currently `1.0.1-alpha.0.N`). Its `api/PublicApi.txt` guard fails the build on any removal or
signature change. It is a **separate repository** at `..\The-Tech-Idea\OilandGasTools`.

### Probe result — the packages are not published

All 13 `TheTechIdea.OilGas.*` ids return **404** on nuget.org, none are in the local NuGet cache, and no
`.nupkg` exists under `LocalNugetFiles` or `OilandGasTools/artifacts`. The sibling checkout is present.

### Decision

**`ProjectReference` into the sibling checkout**, with the root as a property:

```xml
<OilGasToolsRoot Condition="'$(OilGasToolsRoot)' == ''">$(MSBuildThisFileDirectory)..\..\..\OilandGasTools</OilGasToolsRoot>
```

`WellTools` projects `$(OilGasToolsRoot)\src\TheTechIdea.OilGas.Wellbore\...`, and a `BeforeTargets`
check fails the build with an explicit message when the checkout is absent.

### Consequences

- The desktop build requires the `OilandGasTools` checkout; the error names `OilGasToolsRoot`.
- That repository's **MinVer** runs during our build and needs `git` on PATH. When git is unavailable,
  build with `-p:SkipMinVer=true`; versioning of the tools checkout is its own repository's concern.
- `SkiaSharp` is pinned to 4.154.0 at the desktop level (ADR-001), overriding the tools' 4.150.1.
- Moving to packages later means changing the reference in one file, because nothing else knows how the
  assembly arrives.

---

## ADR-009 — No new business EF migrations

**Status:** Proposed · **Blocks:** nothing

### Context

`Beep.OilandGas.Repository` owns identity/RBAC migrations for three providers. The `MASTER-TODO-TRACKER.md`
records that business schema remains entity-driven Beep Migration.

### Decision

The desktop creates **no** EF migration and no handwritten SQL for business tables. Schema comes
from `IModuleSetup` runs through `OgModuleSetupRunner`. Any desktop-only table (for example a
window layout preference) goes into a local file under the data root, not into the database.

### Consequences

Schema changes require a module setup change in the owning project, not a desktop change.

---

## ADR-010 — Retained-original write semantics

**Status:** Proposed · **Blocks:** ACC-0x, HSE-0x, WFL-0x

### Context

The Web applies a consistent pattern: before a write, the exact reviewed original is held
locally; if the acknowledgement is lost, the original is retained, a warning is shown, and the
user explicitly retries the *same* original. It never re-submits automatically and never shows a
new document as success.

### Decision

Every desktop write command follows the same three states: `Ready → Sending → Confirmed |
Unconfirmed(retained)`. The retained original is keyed by company + actor + database + context
(entity/field/well) so it cannot be applied to a different context.

### Consequences

`OgWriteAdmissionService` in the host provides the contract; each feature document names which
of its commands use it.

---

## ADR-011 — One diagnostics pipeline, one toast surface

**Status:** Proposed · **Blocks:** STG-00 · **Authority:** [`07-LOGGING-AUDIT-AND-NOTIFICATIONS.md`](07-LOGGING-AUDIT-AND-NOTIFICATIONS.md)

### Context

The standing rule for every TheTechIdea system is: no swallowed exception, one service receiving
logs, exceptions and audit messages, and a visible notification surface fed by that service. The
`Beep.DoitAll.Erp.Wpf` reference reaches only part of this — it has `ErpNotificationService` and
`ErpAuditLog`, but nothing forces every `catch` through them and no single visible toast surface.

The repository already supplies the contracts (`TheTechIdeaWeb.Diagnostics.Abstractions`:
`IFailureReporter`, `FailureSeverity`, `LastResortLog`, `FailureReferenceFormat`,
`ProcessFailureSubscription`), the durable store (`Beep.OilandGas.Diagnostics`:
`OilGasDiagnosticsDbContext`, `AddOilGasDiagnostics`), and the analyzers (`BEEP0001..0004`).

### Decision

Adopt the skill's contract verbatim and add exactly one desktop surface:

1. Every `catch` calls `IFailureReporter.ReportHandled(exception, operation, consequence, severity)`
   or rethrows. No exceptions to this.
2. `OgDiagnosticsPipeline` implements `IFailureReporter` and `IOgAuditService`, writes the durable
   store and the rolling log, and publishes to `OgNotificationHub`.
3. `OgNotificationHub` is the **only** component that injects `IOgToastService`.
4. `OgToastService` + `ToastHost` present the message. A failure toast always carries the reference
   and never exception text.
5. All three app-wide handlers route through `ProcessFailureSubscription`.

### Consequences

- A feature cannot show an error any way other than a toast and an inline `FailurePanel`.
- The single permitted `MessageBox` is the startup failure dialog (no window exists yet); it is
  registered as an approved exception in guard G-7.
- Replacing the store or the transport later changes one class, not 130 features.

---

## ADR-012 — Toast presentation contract

**Status:** Proposed · **Blocks:** STG-00 · **Related:** ADR-011

### Decision

| Source | Kind | Duration | Sticky | Actions |
|---|---|---|---|---|
| `Critical` | Failure | until dismissed | yes | Open failure detail |
| `Error` | Failure | 10 s | no | Copy reference |
| `Degraded` | Warning | 6 s | no | Open affected page |
| Refusal | Warning | 6 s | no | Open policy detail |
| Successful privileged write | Success | 3 s | no | Open the record |
| Informational | Info | 3 s | no | — |

Max 5 visible; identical key+reference within 5 s collapses with a ×n badge; hover and focus pause
auto-dismiss; `Ctrl+Shift+T` focuses the newest; `AutomationProperties.LiveSetting` is `Assertive`
for Failure and `Polite` otherwise; an icon is mandatory so colour is never the only signal.

### Consequences

Toast behaviour is a product decision, not a per-screen decision. A feature cannot choose its own
duration or placement.

---

## ADR-013 — Failure store target and location for a desktop application

**Status:** Proposed · **Blocks:** STG-00 · **Amended:** 2026-11-05

### Context

The skill's web wiring uses the app's own database through EF. The desktop has three deployment
modes (ADR-004) and may be entirely offline. A desktop install also differs from a web host in one
important way: the program folder is often `C:\Program Files\…`, where a standard user **cannot
write**, and anything written there survives uninstall as unmanaged garbage.

### Decision — provider

- **Personal / CompanyDatabase:** local SQLite at the path in the next section. Always available,
  always writable, independent of the business connection.
- **CompanyApi:** write locally **first**, then forward through
  `TheTechIdeaWeb.Diagnostics.Device` (`DeviceFailureReporter` + `FailureOutbox`) to the ApiService
  failure intake. A dropped acknowledgement must not lose the record.
- Retention: 90 days for `Degraded`/`Error`, 365 for `Critical`.

### Decision — location (hard rule)

```
%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Diagnostics\failures.db
```

```csharp
// OgPaths — the single resolver. No other code composes this path.
public static string DataRoot        { get; }   // %LOCALAPPDATA%\TheTechIdea\BeepOilGas
public static string DiagnosticsDir  { get; }   // DataRoot\Diagnostics
public static string FailureStorePath { get; }  // DiagnosticsDir\failures.db
```

**The failure store is never written:**

| Forbidden location | Why |
|---|---|
| Beside the executable (`AppContext.BaseDirectory`) | Read-only under `Program Files` for a standard user |
| The install folder or any subfolder of it | Destroyed or orphaned by uninstall/upgrade |
| `%ProgramData%` | Shared between users; a per-user failure record must not leak across accounts |
| `%APPDATA%` (roaming) | A local log store must not roam between machines |
| The repository or the `src\` tree | Build artifacts must not reach version control |
| The current working directory | Unpredictable; may be a network share or read-only |

The only permitted relocation is via `BEEP_OILGAS_DATA_ROOT`, and the store still resolves **under**
whatever root that variable names — never under the executable.

Directory is created with default per-user ACLs on first use. If it cannot be created, the pipeline
degrades to the rolling log plus `LastResortLog` and reports that fact once, rather than throwing.

### Consequences

- The failure store never depends on the business database, so a broken business connection cannot
  also destroy the evidence of why.
- Offline use is fully supported.
- Uninstall keeps the evidence unless the operator explicitly chooses to remove the data root
  (REL-01).
- `OgAdminFailuresView` reads local first and shows the forwarding state per record.
- Guard test `DataRootGuardTests` fails the build if any writable path resolves under
  `AppContext.BaseDirectory` or outside `OgPaths.DataRoot`.

### Open sub-task created by this ADR

`OilGasDiagnosticsDbContext` currently ships migrations for **SQL Server, PostgreSQL and Oracle
only** — there is no SQLite migration set. One of the following must be chosen before FND-06 is coded:

| Option | Trade-off |
|---|---|
| **A (recommended)** Add a SQLite migration set to `Beep.OilandGas.Diagnostics`, mirroring the three existing provider sets | Consistent with the other providers; one `dotnet ef migrations add -- --provider Sqlite`; touches the owning project |
| B `EnsureCreated()` for the local store only | No migration work; the store is append-only and disposable, so schema evolution is a delete-and-recreate with retention loss |
| C Keep the store on the business database in `CompanyDatabase` mode and SQLite only for `Personal` | Adds a second code path and two failure stores |

Record the chosen option in `PLAN-07` §9 before implementing FND-06.

---

## ADR-014 — How the desktop protects a local password

**Status:** **Accepted (2026-11-05)** · **Blocks:** FND-07 · **Evidence:** [`evidence/STG-00-FND-07/observed-evidence.json`](evidence/STG-00-FND-07/observed-evidence.json)

### Context

FND-07 AC-5 says "local credentials are encrypted with DPAPI and never written in plain text". DPAPI
protects *data*, not *passwords*: `ProtectedData.Protect` output can be decrypted by the same Windows
account (or machine), so a copied profile or a running process under that account can recover the
plaintext. The requirement's real intent is that a copied file cannot be turned back into a password.

### Decision

Store a **password derivation, never the password**:

| Property | Value |
|---|---|
| KDF | PBKDF2-HMAC-SHA256 (`Rfc2898DeriveBytes.Pbkdf2`) |
| Iterations | 210 000 |
| Salt | 16 random bytes per credential (`RandomNumberGenerator`) |
| Derived key | 32 bytes |
| Comparison | `CryptographicOperations.FixedTimeEquals` |
| File | `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\credentials.json` (ADR-013 root) |
| Contents | user name, display name, salt, hash, iterations, created-at — **no password field exists** |

### Why this is stronger than the AC's letter

A PBKDF2 derivation is one-way: even with full access to the file and the Windows account, an attacker
must mount an offline guessing attack costing 210 000 iterations per candidate. DPAPI ciphertext, by
contrast, is reversible by the account that wrote it — which on a shared or compromised workstation is
exactly the attacker.

### Consequences

- Offline password reset is impossible: a forgotten password means creating a new administrator through
  the repair path (AC-2), not recovering the old one. That is the correct trade for a credential.
- The `PasswordBox` value is converted to a derivation immediately and never stored, logged, audited or
  shown in a toast.
- Making the iteration count configurable is an REL decision (hardware moves on); the stored
  `Iterations` field means an old credential still verifies after a policy change.
- When IdentityServer is the identity provider (CompanyApi mode) this store is not used at all.

---

## ADR-015 — How a licence is authenticated

**Status:** **Accepted (2026-11-06)** · **Blocks:** REL-02, REL-01, REL-05 · **Evidence:** [`evidence/STG-18-REL-02/observed-evidence.json`](evidence/STG-18-REL-02/observed-evidence.json)

### Context

REL-02 must gate commercial modules on a licence file. A file the customer can edit is a suggestion, not a
licence, so the question is what makes it authoritative — and how the product behaves when it is not.

### Decision

| Property | Value |
|---|---|
| Shape | `{ "schema": 2, "keyId": "…", "payload": "<base64url>", "signature": "<base64url>" }` |
| Signed bytes | the decoded payload, **verbatim** — no re-serialisation |
| Algorithm | RSA-2048, SHA-256, PKCS#1 v1.5 |
| Trust anchor | public key(s) in an **embedded** resource (`Security/licence-keys.json`) |
| Unsigned file | refused (`licence-unsigned`); still parseable for diagnostics, grants nothing |
| Altered / unknown key | refused with `licence-signature-invalid` / `licence-key-unknown` |
| Enforcement | a persisted setting: `Permissive` (default) or `Enforced` |
| Expired past grace, enforced | reads continue, writes blocked; core never blocked |

### Why RSA and not HMAC

An HMAC would need the same secret in every installation to verify a licence — and anything that can verify a
licence can then mint one. A public-key signature lets the application verify without being able to issue.

### Why the payload is signed as exact bytes

Two JSON serializers that disagree on key order, spacing or number formatting would make a valid licence fail
verification intermittently. Signing the payload's bytes removes the serializer from the trust path; the
verifier parses only after the signature matches.

### Why enforcement is switchable

Commercial terms are undecided (§9). A permissive default lets the product run today without pretending the
licensing question is settled, while every state is still computed, shown and audited. Turning enforcement on
is one audited setting, not a rebuild.

### Consequences

- `--licence-check` changed meaning: it now **verifies** rather than describing the file's structure. Three
  headless tests that accepted unsigned files were rewritten, because the old behaviour would have let an
  edited file pass as a licence.
- The repository carries a **development** key pair for tests. The production public key must replace
  `Security/licence-keys.json` before release, and the private half must never be committed; a guard test
  fails the build if any file under `src` contains private-key material.
- Seat counts are read and displayed but not enforced until the user store tracks active sessions.

---

## ADR-016 — Referencing `Beep.OilandGas.Models` without reviving `SkiaSharp.Svg`

**Status:** **Accepted (2026-11-06)** · **Blocks:** RPT-03, RPT-01, PRD-01/05 · **Evidence:** [`evidence/STG-18-RPT-03/observed-evidence.json`](evidence/STG-18-RPT-03/observed-evidence.json)

### Context

RPT-03 requires `IRenderingService` and `RenderChartRequest`/`RenderChartResult`, which live in
`Beep.OilandGas.Models`. That project also declares `SkiaSharp.Svg 1.60.0` for one property it owns
(`ReservoirLayer.PatternOrTextureSvg`). ADR-001 retired `SkiaSharp.Svg` from the desktop because its 1.x
calls throw `MissingMethodException` against SkiaSharp 3+.

Referencing Models therefore reintroduced the package transitively — visible immediately as a
`SKSvg` ambiguity in the export test (`Svg.Skia.SKSvg` vs `SkiaSharp.SKSvg`).

### Decision

Reference `Beep.OilandGas.Models` for the chart **contracts**, and **exclude the retired package's assets** in
the consuming projects:

```xml
<PackageReference Include="SkiaSharp.Svg" ExcludeAssets="all" />
```

with `SkiaSharp.Svg` pinned in `Directory.Packages.props`. Models still compiles against it (its own project is
untouched); the desktop compiles against, and ships, **one** SkiaSharp (4.154.0).

### Consequences

- The `SKSvg` ambiguity disappears because `SkiaSharp.SKSvg` is no longer in the desktop's reference set.
- `SkiaSharp.Svg.dll` is no longer copied to the application output (verified), so the 1.x binding cannot load.
- The one Models property that returns `SKSvg` is never touched by the desktop. A guard test
  (`The_desktop_never_touches_the_retired_svg_helper`) fails the build if desktop source references
  `SkiaSharp.Extended.Svg`.
- Any future desktop feature that needs `ReservoirLayer.PatternOrTextureSvg` must migrate that Models helper
  to `Svg.Skia` first — it cannot simply call it.
- When Models itself migrates, the exclusion and this ADR can be removed.

---

## Decision log

| ADR | Title | Status | Date | Evidence |
|---|---|---|---|---|
| ADR-001 | SkiaSharp unification | Accepted | 2026-11-05 | [evidence](evidence/ADR-001-002-skia/observed-evidence.json) |
| ADR-002 | WPF Skia control — own WriteableBitmap host | Accepted | 2026-11-05 | [evidence](evidence/ADR-001-002-skia/observed-evidence.json) |
| ADR-003 | Target framework | Accepted | 2026-11-05 | [evidence](evidence/ADR-003/observed-evidence.json) |
| ADR-004 | Data access mode | Proposed | 2026-11-05 | — |
| ADR-005 | One Modules assembly | Proposed | 2026-11-05 | — |
| ADR-006 | HeatMap deferred | Proposed | 2026-11-05 | — |
| ADR-007 | Lease area | Proposed | 2026-11-05 | — |
| ADR-008 | OilandGasTools via NuGet | **Accepted change** — not published; ProjectReference into the sibling checkout with `OilGasToolsRoot` | 2026-11-05 | [evidence](evidence/ADR-008/observed-evidence.json) |
| ADR-009 | No business EF migrations | Proposed | 2026-11-05 | — |
| ADR-010 | Retained-original writes | Proposed | 2026-11-05 | — |
| ADR-011 | One diagnostics pipeline, one toast surface | Proposed | 2026-11-05 | — |
| ADR-012 | Toast presentation contract | Proposed | 2026-11-05 | — |
| ADR-013 | Failure store target and location | Proposed | 2026-11-05 | - |
| ADR-014 | Local password protection — PBKDF2-SHA256, never DPAPI, never plaintext | Accepted | 2026-11-05 | [evidence](evidence/STG-00-FND-07/observed-evidence.json) |
| ADR-015 | Licence authentication — RSA-2048/SHA-256 over the payload's exact bytes, embedded public key, enforcement switchable | Accepted | 2026-11-06 | [evidence](evidence/STG-18-REL-02/observed-evidence.json) |
| ADR-016 | Chart contracts from `Beep.OilandGas.Models` with `SkiaSharp.Svg` assets excluded — one SkiaSharp at runtime | Accepted | 2026-11-06 | [evidence](evidence/STG-18-RPT-03/observed-evidence.json) |
