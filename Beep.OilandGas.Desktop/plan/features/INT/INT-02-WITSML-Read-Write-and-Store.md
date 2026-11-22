# INT-02 — WITSML read, write and store

| Field | Value |
|---|---|
| Stage | STG-17 — Data interop, mapping and 3D |
| Area | Interop |
| Status | Gate met for what a build can prove - AC-1, AC-3, AC-4, AC-5 and AC-6 met, and the Store tab now **queries and pulls**; AC-2 is **structural conformance only** (namespace, uuid, schemaVersion, references) because the Energistics XSDs are not shipped, so schema validation remains the owner's item. See [evidence](../../evidence/STG-17-INT-02/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Loaders` |
| Desktop project | `Modules/Interop/Witsml/` |
| External packages | `TheTechIdea.OilGas.Wellbore.Loaders` |
| Depends on | VSB-02 |
| Blocks | — |
| Web reference | `OilandGasTools/docs/customer/data.html`, `developer/architecture.html` |

## 1. Purpose

Exchange well data with WITSML: read 1.3.1.1 / 1.4.1.1 / 2.0 files, write 1.x, export 2.1, and pull
from a WITSML store.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore.Loaders/Witsml/WitsmlReader.cs` | `LoadFile`, `Load(xml)`, `Load(documents, entity)` |
| `Wellbore.Loaders/Witsml/WitsmlWriter.cs` | `Trajectory`, `FormationMarkers`, `Logs` |
| `Wellbore.Loaders/Witsml/Witsml21Export.cs` | `Export(well, warnings)` |
| `Wellbore.Loaders/Witsml/WitsmlStoreClient.cs` | SOAP store client with timeout/retry from `live.*` |
| `Wellbore.Loaders/Witsml/WitsmlQueries.cs` | query templates |
| `Wellbore.Loaders/Witsml/WitsmlGrowingLogSource.cs` | growing log (deferred live screen) |

## 3. Screens

`Data → WITSML`

Tabs:

- **Import** — file(s) or store connection; entity type (trajectory, markers, log); preview; commit.
- **Export** — well, version (1.x XML or 2.1 files), entity selection, output folder.
- **Store** — endpoint, version, credentials (stored securely), query, results, pull into a well.
- **Growing log** — deferred: shown as disabled with the reason.

## 4. Data and contracts

`OgWitsmlImport { Source, Version, Entity, Warnings[], RowsImported }`
Store credentials go to `OgLocalCredentialStore` (DPAPI); they must never appear in logs or the UI.

## 5. States

| State | Behaviour |
|---|---|
| Unsupported version | refused with the version found |
| Time-indexed log | reported as unsupported rather than misread |
| Store unreachable | failed state with the endpoint and reason, no credentials in the message |
| Authentication failed | distinct from unreachable |
| Partial import | counts plus per-entity outcome |
| Export folder not writable | refused with the path |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Import | `interop.witsml.write` | yes |
| Export | `interop.witsml.read` | file |
| Store pull | `interop.witsml.store` | yes |

## 7. Acceptance Criteria

- AC-1 A 1.4.1.1 trajectory and marker file round-trips through the writer.
- AC-2 A 2.1 export produces files that validate against the shipped schemas.
- AC-3 A time-indexed log is reported as unsupported.
- AC-4 Store credentials never appear in logs, audit or error text.
- AC-5 Store unreachable and authentication failure are distinguishable.
- AC-6 A partial import reports which entities succeeded.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~INT-02"
```

### Delivered 2026-11-09 - the Store tab pulls, and AC-2 got the strongest check it can have without the schemas

**The pull was the missing half of the feature.** The page could check a store but not use one. `OgWitsmlWorkspace.PullAsync` now queries one
entity of one wellbore through the tools' `GetFromStoreAsync`, with a query built per entity by `OgWitsmlQueries` (UIDs **escaped rather than
trusted**), and reads the reply with **the same reader a file uses** — so a store pull and a file import print the same counts, and the pull
fills the same facts grid and station table.

| State | Behaviour |
|---|---|
| Pull succeeded | the entity sentence and the counts, from the returned document |
| Store answered with nothing | `Ok`, no import, "the store returned no trajectory for that UID" — an empty answer, not a failure |
| 401 / 403 | `AuthenticationFailed`, never "unreachable" — the same classifier the check uses |
| Nothing answered | `Unreachable`, with the endpoint and what to try next |
| Version this build does not read | refused naming the version found and the three that are read |
| Export folder not writable | refused naming the path, probed with a real write before any file |

**Two stated states existed only in the plan; both are now real.** `LoadChecked` refuses a document whose declared version is not 1.3.1.1,
1.4.1.1 or 2.0 rather than parsing it under semantics nobody checked. `EnsureFolder` probes the export folder with an actual write and refuses
naming the path.

**AC-2 was not dressed up.** `Conform21` adds the checks a build can make without the Energistics XSDs: every file parses, its root is in an
energyml `witsmlv2` or `commonv2` namespace, it carries a **uuid** and a **schemaVersion that matches that namespace**, names are unique, and
**every object reference resolves to a uuid defined in the set**. The page prints the result **with the sentence saying it is not schema
validation**. The check is not vacuous: replacing the Wellbore uuid the Trajectory references **fails the Trajectory and names the uuid**, and a
namespace that disagrees with its own schemaVersion fails too.

**The growing log is now visible rather than absent** — shown on the page, disabled, with its reason (a live subscription; this client polls
with `GetFromStore`), which is what the screen specification asks for.

**Still the owner's, recorded rather than hidden:** the **Energistics XSDs** for AC-2's validation, and a **real WITSML server** — the pull was
exercised against simulated SOAP handlers, and WS-Security is unsupported by the tools' client, so only basic authentication over https is proven.
Store credentials remain **session-only**: the DPAPI store the plan assumes does not exist, and ADR-014's one-way derivation for sign-in cannot be
reused for an outbound password.

## 9. Open questions and risks

- WITSML store is SOAP; some providers require WS-Security. Confirm what `WitsmlStoreClient`
  supports before promising connectivity.

### Delivered 2026-11-06

- **Route `witsml`** (en + ar): file + **Example / Preview / Import**, a facts grid, a station table,
  **Export 1.x / Export 2.1**, and a store row (endpoint, user, password, **Check store**) whose state text
  says which of the four outcomes it was.
- `OgWitsmlWorkspace` wraps the tools: `Load`/`LoadFile`, `WriteTrajectory`/`WriteMarkers`/`WriteLogs`,
  `Export21`, `CheckStoreAsync`, `Classify`, plus three deterministic 1.4.1.1 example documents.
- **AC-1** a 1.4.1.1 trajectory (3 stations, radians/metres) and markers (2 with depth, one skipped) load,
  are written by the tools' writer, and reload with the same depths and angles. The marker round-trip
  confirmed the reader prefers the **sampled** depth (2612.5 m) over the prognosed one, and the writer
  carries that same depth.
- **AC-2 is partial.** The 2.1 export produces Well/Wellbore/Trajectory/log files that are **well-formed XML
  in the energyml WITSML 2.1 namespace**, but this repository ships **no Energistics XSDs**, so validation
  against "the shipped schemas" cannot be performed. The tools' own test says schema validation runs in a
  separate CI script against the Energistics schemas; that step remains outside this build.
- **AC-3** a time-indexed log is reported: `log 'Surface time log': index type 'date time' is time-based - not
  a depth log, skipped`, while the depth log in the same file loads.
- **AC-4** credentials are **session only** and never appear in a result sentence or a failure row; tests
  assert a real password and user name are absent from both. The plan's DPAPI credential store does not exist
  (ADR-014 chose a one-way derivation for sign-in, which cannot be reused for an outbound password), so
  persistence is recorded as open rather than invented.
- **AC-5** `Unreachable`, `AuthenticationFailed` and `Refused` are distinct, each with the next step in its
  sentence; a 401 is never described as unreachable.
- **AC-6** the import reports each entity by count (`N station(s) · N marker(s) · N depth log(s)`) and sets
  `IsPartial` when a warning says an entity was skipped or time-based.

### Still open

- **AC-2's schema validation** needs the Energistics XSDs checked in (or a CI step), which is a licensing and
  build-asset decision, not a code one.
- **Store credentials are not persisted.** Re-entering them each session is the current behaviour.
- **Growing log** is not offered; the tools' `WitsmlGrowingLogSource` needs a live store to be meaningful.
- **WS-Security** is unsupported by the tools' client; only basic authentication over https works.
- The store check was exercised against simulated handlers, not a real WITSML server.