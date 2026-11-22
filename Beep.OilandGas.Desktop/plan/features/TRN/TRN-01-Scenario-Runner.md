# TRN-01 — Scenario runner

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Training |
| Status | Gate met - see [evidence](../../evidence/STG-18-TRN-01/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Training` |
| Desktop project | `Modules/Training/Scenarios/` |
| External packages | `TheTechIdea.OilGas.Training` |
| Depends on | VSB-03, VSB-05 |
| Blocks | TRN-02 |
| Web reference | `OilandGasTools/docs/customer/training.html` |

## 1. Purpose

Give a trainee a real well view and an exercise on it: read the display, answer, get scored, then see
the worked solution — without touching production data.

## 2. Source components

| Source | Reuse |
|---|---|
| `Training/ScenarioLibrary.cs` | `LoadFolder`, `LoadConfigured`, `Validate` |
| `Training/Scenario.cs` | `Load(path)`, JSON-authored definition |
| `Training/ScenarioScoring.cs` | `Score(scenario, answers) → ScenarioResult` |
| `Training/ScenarioRender.cs` | `Document(scenario, solution, warnings)`, `ToPng`, `ToSvg`, `FitToPage()` |
| `Training/MapExhibit.cs`, `MapExhibitRender.cs` | map-based exercises |
| `Training/TrainingToolbox.cs` | `Settings` (`ScenarioFolders`, `PassMark`, `AttemptPolicy`) |
| `Training/SamplePacks.cs` | built-in samples |

## 3. Screens

`Training → Scenarios`

- Scenario list grouped by course, with pass mark and attempt policy.
- Runner: the scenario's `WellboreDocument` in `SkiaViewSurfaceHost`, the question panel, an answer
  entry per required value (with units), submit.
- Result: score, which answers were right/wrong with the correct values, pass/fail.
- Solution view: `ScenarioRender` document shown alongside or after the attempt.
- Attempt history for the signed-in trainee.

## 4. Data and contracts

`OgScenarioResult { ScenarioId, Score, PassMark, Passed, Answers[] { Key, Given, Expected, Correct } }`
Scenarios are files, not database records. A broken scenario file is listed as invalid, not ignored.

## 5. States

| State | Behaviour |
|---|---|
| Invalid scenario file | listed with the validation problem |
| No scenario folder configured | runner disabled with the reason, sample pack offered |
| Incomplete answer | submit disabled with the missing keys |
| Unit mismatch in an answer | marked incorrect with the expected unit shown |
| Attempt policy exhausted | submission blocked with the policy stated |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run a scenario | `training.run` | no |
| Submit | `training.run` | results ledger (TRN-02) |
| Configure folders | `training.admin` | file |

## 7. Acceptance Criteria

- AC-1 A scenario loads, presents its document and accepts answers.
- AC-2 Scoring matches the scenario's expected values exactly, with units.
- AC-3 The solution view is available after submission, not before.
- AC-4 A broken scenario is listed with its validation error.
- AC-5 Training never reads or writes production well data.
- AC-6 Attempt policy is enforced and stated.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~TRN-01"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-18-TRN-01/observed-evidence.json).

**The exercise is the engine's file.** `ScenarioLibrary.LoadFolder` reads and validates the folder, `ScenarioScoring` marks,
`ScenarioRender` draws the exhibit and `ResultsLedger` records the attempt — so the marking shown is the ledger's **rescore of the
recorded attempt**, not a second call that could drift once a key is corrected.

| What the page shows | Where it comes from |
|---|---|
| The scenario list, grouped by course | `ScenarioLibrary` and `CourseLibrary` |
| The exercise | the scenario's `WellboreDocument` in the well view, or its map as an image |
| The questions | the scenario's own, without the key |
| The marking | `ScenarioScoring`, through `ResultsLedger.Rescore` |
| The attempt history and the counting attempt | `ResultsLedger.Counted` |
| The pass mark | the course's, else `training.passMark` |

**The first test run failed on a real scenario.** One shipped sample (`coords-axes`) is a **map exercise with no well**, and
`ScenarioRender.Document` throws *"scenario 'coords-axes' has no well"*. The runner now renders **the exhibit by kind** — a well as its
document in the well view, a map as `ScenarioRender.ToPng` shown as an image — which is the engine saying what an exercise is, rather
than a special case invented here.

**AC-3 is structural, not a flag.** The object handed to the page before an attempt **carries no outcomes at all**, so there is no
expected value in it to leak; the key, the explanations and the solution's exhibit arrive with the marking.

**AC-5 is structural too.** The service **takes no `IDataSource`**, and a source test fails if one is ever added, so training cannot
reach a well table.

**AC-6 needed a distinction recorded rather than a feature built.** `attemptPolicy` is **latest / best / first — which attempt
counts — and the engine has no attempt limit**, so the feature's "attempt policy exhausted" state **cannot arise**. The page states the
policy and marks the counting attempt in the history, instead of inventing a maximum the engine would not enforce.

**A pass mark lives in two places.** A course may override `training.passMark`, but `ScenarioScoring.Passed` only reads the setting, so
the run's verdict is computed from the course-aware mark — and that mark is what the ledger records, so the stored verdict and the
displayed one agree.

**Two smaller findings are recorded.** An `Attempt` has **no id** — its identity is trainee | scenario | time, which is what the history
uses — and re-scoring is **`ResultsLedger.Rescore`**, not a member of `ScenarioScoring`.

**Not built, and stated as gaps:** a per-question widget for each answer type (the entry is one line per question, `id = value`); a hint
button (the hint is carried but not offered, so the engine's hint penalty never arises); more than the first configured scenario
folder; an attempt limit (the engine has none); an interactive map exhibit (it is an image); a cross-trainee report and the course
roll-up, which are TRN-02's.

## 9. Open questions and risks

- `ScenarioRender.FitToPage()` needs a page size; reuse the `PrintPlanner` options for consistency
  with VSB-09.
