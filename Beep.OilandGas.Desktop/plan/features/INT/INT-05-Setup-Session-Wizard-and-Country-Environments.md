# INT-05 — Setup session wizard and country environments

| Field | Value |
|---|---|
| Stage | STG-17 — Data interop, mapping and 3D |
| Area | Interop |
| Status | Gate met - see [evidence](../../evidence/STG-17-INT-05/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Setup`, `TheTechIdea.OilGas.Wellbore.Data` |
| Desktop project | `Modules/Interop/SetupWizard/` |
| External packages | `TheTechIdea.OilGas.Wellbore.Setup` |
| Depends on | INT-04, VSB-02 |
| Blocks | INT-06, FND-07 (step 5) |
| Web reference | `OilandGasTools/docs/customer/onboarding.html`, `countries.html`, `configuring.html`; `tools/setup-app/*` (WinForms reference) |

## 1. Purpose

Stand up a customer in twelve steps: country, data sources, coverage, codes, symbols, stratigraphy,
lithology, logs, tubulars, surveys, verification and handover — using the tools' own setup engine.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore.Setup/SetupSession.cs` | `Create`, `Open`, `Set`, `Get`, `Effective`, `SetDataSource`, `SetUnit`, `SetUnitAlias`, `SetMnemonicAlias`, `MapCode`, `SetComponentSymbol`, `AddTubular`, `AddToolModelFile`, `ImportWorksheet`, `Coverage`, `Suggestions`, `Accept`, `CandidatesFor`, `CanonicalValues`, `Verify`, `Validate`, `Steps`, `Preview`, `TestSource`, `CaptureCase`, `Handover` |
| `Wellbore.Setup/DataSourcePlugins.cs` | `FromFolder`, `FromAssemblies`, `Register`, `Factories`, `Problems` |
| `Wellbore.Setup/MappingSuggestions.cs` | `MappingSuggestion`, `MappingCandidate`, `IMappingSuggester` |
| `Wellbore.Data/Configuration/CountryPresets.cs` | 62 presets |
| `Wellbore.Data/Configuration/EnvironmentReport.cs` | `Flatten`, `Diff` |
| `tools/setup-app/Pages/*` | reference screens (Company, Sources, Resolve, Stratigraphy, Other, Preview) |

## 3. Screens

`Settings → Wellbore setup` wizard (`WizardHost`), steps mirroring `SetupSession.Steps()`:

1. **Country** — 62 presets with a preview of what the preset sets.
2. **Company** — name, code, lexicon, locale.
3. **Data sources** — add/test/remove, with `TestSource` results.
4. **Coverage** — `Coverage(name)` per source.
5. **Codes and values** — `MapCode`, `CandidatesFor`, `CanonicalValues`.
6. **Equipment and symbols** — `SetComponentSymbol`, tubulars.
7. **Stratigraphy** — lexicon layers and picks.
8. **Lithology** — standard selection.
9. **Logs** — curve families, mnemonic aliases.
10. **Tubulars and tool models**.
11. **Surveys** — north reference, magnetic model.
12. **Verify and hand over** — `Verify`, `Validate`, `Preview`, `CaptureCase`, `Handover`.

## 4. Data and contracts

`OgSetupStep { Key, Title, State (NotStarted / InProgress / Done / Blocked), BlockingReason }`
The wizard writes only to the environment file(s), never to the database.

## 5. States

| State | Behaviour |
|---|---|
| Step blocked | reason shown; later steps can still be visited read-only |
| Source test fails | recorded with the failure, step stays blocked |
| Suggestion | shown as a proposal with accept/reject; never auto-applied |
| Import worksheet partially invalid | accepted rows applied, rejected rows listed |
| Handover without verify | refused with the outstanding checks |
| Save failure | previous environment retained |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run setup | `settings.wellbore.setup` | environment files |
| Apply suggestion | `settings.wellbore.setup` | environment files |
| Handover | `settings.wellbore.handover` | archive |

## 7. Acceptance Criteria

- AC-1 A country preset produces an environment that `SetupSession.Validate()` accepts.
- AC-2 A data source can be tested and its result recorded.
- AC-3 Mapping suggestions are never applied without explicit acceptance.
- AC-4 Coverage is shown per source, with an explicit "unknown" when it cannot be computed.
- AC-5 Handover is refused while verification has outstanding failures.
- AC-6 A failed save leaves the previous environment intact.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~INT-05"
```

Port assertions from `OilandGasTools/tests/TheTechIdea.OilGas.Tests/SetupSessionTests.cs`,
`CountryPresetTests.cs`, `EnvironmentConformanceTests.cs`.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-17-INT-05/observed-evidence.json).

**The wizard is a view over the engine, not a second implementation.** The steps, the source tests, the coverage, the proposals and the
verification are `SetupSession`'s own. The page edits **`<data root>/Wellbore/environment.json`** — the file the application's
`OgWellboreLifetime` loads — and then asks the host to **re-read it**, so a change is the running configuration rather than a copy of
it.

| What the page shows | Where it comes from |
|---|---|
| Steps, states and what to do next | `SetupSession.Steps()` |
| Source test: loaded or not, counts, warnings | `SetupSession.TestSource` |
| Coverage per source | `SetupSession.Coverage(name)` |
| Proposals | `SetupSession.Suggestions(source)` |
| Verification results | `SetupSession.Verify()` |
| The hand-over archive | `SetupSession.Handover(folder)` |

**AC-3 is enforced at two points.** Computing the proposals **writes nothing at all** (asserted byte for byte), and accepting
**re-reads the engine's current proposals** and refuses one it no longer offers — so what is applied is what the engine proposed, not
what a screen remembered.

**AC-5 does not trust the screen.** The hand-over **runs the verification itself** and refuses with the failing case named; a unit case
expecting 99 m for 1 ft is what proves it. Only then does it write the archive.

**AC-6 is structural.** A session is **opened per command and never held**, because the engine writes on every change — a session that
survived a failed write would carry a document the file does not contain. The test makes the file read-only and checks it is
**byte-identical** afterwards.

**A real bug was caught by the test.** The first `LoadAsync` built the workspace **without opening the file**, so the page would have
reported "no environment yet" over an existing one; it surfaced when the company name came back null after a reload.

**Two vocabulary decisions are recorded rather than smoothed.** The engine has **three step states, not the plan's four**, so
`InProgress` is the page's and is never fabricated from a rule; and it reports **eleven steps, not twelve**, because its *Country and
company* step carries Company and Coverage is a per-source computation rather than a step.

**A finding worth keeping:** a CSV source whose table file is missing reads as an **empty table**, so its coverage is a clean zero and
would hide the failure — the unknown case is a source kind the environment cannot open.

**The actor goes to the audit trail**, because neither the environment file nor the engine's other files has a field for one.

**Not built, and stated as gaps:** the dedicated editors for stratigraphy, lithology, logs, units, tubulars and component symbols (the
path-and-value editor reaches any setting the engine can set, and the proposal/accept command covers the units, mnemonics, codes and
lexicon areas it proposes for); selecting between several company files; a person's own checks in the hand-over; paging.

## 9. Open questions and risks

- The reference implementation is a WinForms app; the WPF wizard must reproduce its *behaviour*, not
  its layout.
- 62 country presets: ship them as embedded resources and verify they are present after packaging.
