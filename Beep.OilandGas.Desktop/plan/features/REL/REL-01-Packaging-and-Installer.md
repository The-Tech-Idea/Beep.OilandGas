# REL-01 — Packaging and installer

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Release |
| Status | Gate met for what a build can prove - see [evidence](../../evidence/STG-18-REL-01/observed-evidence.json); AC-1 (a clean Windows VM) is the owner’s step |
| Owner projects | none |
| Desktop project | `installer/`, `src/Beep.OilandGas.Desktop/app.manifest` |
| External packages | none |
| Depends on | REL-02, REL-03, REL-04 |
| Blocks | REL-05 |
| Web reference | ERP packaging pattern |

## 1. Purpose

Produce an installer a customer can run on a clean Windows machine, with every native dependency
present and every file in a writable, uninstallable location.

## 2. Source components

| Source | Reuse |
|---|---|
| ERP `app.manifest`, `ErpPaths` | per-user data root, no writes beside the executable |
| ERP `--restore` and `--schema-apply` verbs | post-install and repair actions |
| `OilandGasTools` package assets | `WMM2025.COF`, country presets, symbol catalogs, lithology standards, environment template/schema |
| `Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf` | native renderer |
| `SkiaSharp` native assets | `libSkiaSharp` |

## 3. Screens

Installer wizard: welcome · licence · install location · shortcuts · prerequisites (WebView2 is not
required; note any that are) · install · finish with "start the application".

Also: an "installation health" page in the app (`Settings → Diagnostics`) listing expected assets and
their presence.

## 4. Data and contracts

Per-user data root `%LOCALAPPDATA%\TheTechIdea\BeepOilGas` (see `OgPaths`). The install directory
holds only binaries and read-only resources.

Required runtime assets checklist (verified by the health page):

| Asset | Source |
|---|---|
| `ConnectionDrivers/` assemblies | packaged |
| `libSkiaSharp.dll` | SkiaSharp package |
| `wkhtmltopdf` binary | permissions PDF |
| Country presets (62) | embedded in `Wellbore.Data` |
| Environment template and schema | embedded |
| Symbol catalogs (235) | embedded in `WellSchematic` |
| Lithology standards | embedded in `Wellbore` |
| `WMM2025.COF` | embedded in `Wellbore.Data` |
| Mapping profiles (csv, ppdm39, edm, template) | embedded in `Wellbore.Loaders` |

## 5. States

| State | Behaviour |
|---|---|
| Missing runtime asset | health page lists it as missing with the consequence |
| Install over an existing version | offers repair or upgrade, never silent data loss |
| Uninstall | offers to keep or remove the data root, defaulting to keep |
| Non-admin install | supported; no writes outside the install and data root |

## 6. Commands and permissions

Installer only. The app exposes `Settings → Diagnostics → Verify installation`.

## 7. Acceptance Criteria

- AC-1 A clean Windows VM installs, launches, and completes the setup wizard.
- AC-2 No file is written beside the executable, inside the install folder, under `%ProgramData%` or
  in the roaming profile during normal use — including the failure store, the rolling log, the
  wellbore environment and the local database.
- AC-3 Every required asset is present and reported by the health page.
- AC-4 Uninstall removes program files and leaves the per-user data root by default; removing the
  data root requires an explicit choice that names the failure store and the logs it contains.
- AC-5 An upgrade preserves the data root and the setup state.
- AC-6 A non-admin user can install and run.

## 8. Verification

Install on a clean VM, run `--smoke`, and attach both the install log and the smoke result.

Blind spot: a clean VM is required; a developer machine is not evidence.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-18-REL-01/observed-evidence.json).

| | |
|---|---|
| Program files | `%LOCALAPPDATA%\Programs\TheTechIdea\BeepOilGas` |
| Writable data | `%LOCALAPPDATA%\TheTechIdea\BeepOilGas` (see `OgPaths`) |
| Health page | `Administration → Installation`, and `Verify installation` writes `Diagnostics\installation-report.json` |

**AC-3 wants a list of assets to verify; this build embeds most of them.** The **WMM2025 coefficient file is a manifest resource** the engine
streams, and the **country presets, symbol catalogs, mapping profiles and lithology standards are compiled in** — so each is checked
**through the API that consumes it** rather than as a file that never ships. The page prints which assets are embedded and which are files.

**The `ConnectionDrivers` folder does not exist.** The driver assemblies are **compiled in and registered from
`OgBundledDrivers.AssemblyAnchors`**, so the asset checks that the anchors resolve — the thing that has to be true for the local database to
open.

**`LithologyStandards.Load("FGDC")` returns null by design** — FGDC's numbers are part of the library — so a first check reported the
standards **MISSING on a complete installation**. It now asks for what is genuinely loadable (the GOST layer) plus the library's own
definitions, and a unit test pins both facts.

**The permits renderer is not shipped**, so it is an **optional** asset reported as "not installed". Requiring it would have made every
installation unhealthy for a feature that is not in the build.

**AC-2 is checked as an asset of its own.** A data root **inside the install folder** fails with the reason — an uninstall would then remove
customer data with the program — and every path `OgPaths` resolves is asserted under the data root and outside the install folder.

**AC-4 refuses six different wrong confirmations** (empty, "yes", "REMOVE", another path, the bare path) with the **failure store and the
logs named in the sentence**, then removes the tree with the exact words and leaves a sibling folder untouched. `uninstall.ps1` prints the
same names and keeps the root by default.

**AC-5 writes nothing.** The plan reports fresh, repair or upgrade from a record under the data root, and `setup.json`'s **bytes and
timestamp are unchanged** afterwards.

**Two honest admissions.** The Start-menu shortcut is **one write outside the per-user folders** — in `%APPDATA%`, at install time only,
removed by the uninstall, and skippable with `-NoShortcut` — stated because a first script sentence claimed otherwise and the guard test
caught the contradiction. And **AC-1 (a clean Windows VM) is the owner's step**, which the feature itself says a developer machine cannot
evidence; this record says what the build contributes and no more.

**Not built, and stated as gaps:** an MSI or MSIX package (this is a per-user PowerShell installer, with no Add/Remove Programs entry, no
upgrade code and no per-machine option); code signing, so SmartScreen warns; removal of individual parts of the data root; a migration in
the upgrade plan (that is the module-setup runner's job — the plan only guarantees it will not be what loses the data); and a per-machine
or desktop shortcut.

## 9. Open questions and risks

- Code signing certificate availability is unknown; without it, SmartScreen warnings will appear.
- `wkhtmltopdf` licensing and redistribution must be checked before shipping.
