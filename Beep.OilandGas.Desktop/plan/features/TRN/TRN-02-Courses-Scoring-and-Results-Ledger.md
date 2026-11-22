# TRN-02 — Courses, scoring and results ledger

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Training |
| Status | Gate met - see [evidence](../../evidence/STG-18-TRN-02/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Training` |
| Desktop project | `Modules/Training/Courses/` |
| External packages | `TheTechIdea.OilGas.Training` |
| Depends on | TRN-01 |
| Blocks | TRN-03 |
| Web reference | `OilandGasTools/docs/customer/training.html` |

## 1. Purpose

Organise scenarios into courses, apply the attempt policy, and keep a durable results ledger an
instructor can review.

## 2. Source components

| Source | Reuse |
|---|---|
| `Training/Course.cs` | `CourseLibrary`, `CoursePack`, `CourseResult` |
| `Training/Results.cs` | `ResultsLedger` |
| `Training/ScenarioScoring.cs` | per-scenario result |
| `Training/TrainingToolbox.cs` | `PassMark`, `AttemptPolicy` (latest / best / first) |

## 3. Screens

`Training → Courses`

- Course list: name, scenario count, pass mark, attempt policy.
- Course detail: scenario sequence with the trainee's progress.
- Results ledger: trainee, course, attempts, best score, latest score, date, pass state.
- Instructor view: cohort summary with pass rate and weakest scenarios.

## 4. Data and contracts

`OgCourseResult { CourseId, Trainee, Attempts, Best, Latest, Passed, At }`

## 5. States

| State | Behaviour |
|---|---|
| No attempts | "not attempted", distinct from failed |
| Attempt policy = latest | best is still shown for information but the latest decides |
| Policy = best | the latest attempt does not lower the recorded result |
| Policy = first | later attempts recorded but not counted |
| Ledger write failed | the attempt result is shown but marked "not recorded" |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Take a course | `training.run` | ledger |
| View results | `training.read` | no |
| View cohort | `training.admin` | no |

## 7. Acceptance Criteria

- AC-1 The attempt policy changes the recorded result exactly as documented.
- AC-2 "Not attempted" is distinct from "failed".
- AC-3 A ledger write failure is visible, not silent.
- AC-4 Scores are stored with the course and scenario versions.
- AC-5 Cohort pass rate equals passed ÷ attempted.
- AC-6 Results are per trainee and never overwrite another trainee's row.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~TRN-02"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-18-TRN-02/observed-evidence.json).

**The cataloguing, the counted attempt, the course verdict and the cohort are the engine's.** `ResultsLedger.Counted` applies the
attempt policy, `ResultsLedger.Course(trainee, course, scenarios)` produces one trainee's result, and the same function called once per
trainee produces the cohort. The page re-implements none of it.

| What the page shows | Where it comes from |
|---|---|
| Courses with their pass marks and the policy | `CourseLibrary` and `training.passMark` / `training.attemptPolicy` |
| The trainee's progress through a course | `ResultsLedger.Counted` per scenario |
| The trainee's result | `ResultsLedger.Course` |
| The cohort and its pass rate | `ResultsLedger.Course(course, scenarios)`, summed |
| The weakest scenarios | the recorded attempts' per-question results |
| The ledger's path and state | the file itself, probed |

**AC-4 had nowhere to put a version.** An `Attempt` records the course and scenario **ids** but no version, and neither a scenario file
nor a course file has a version property at all — so a score could not say which answer key it was earned against. `Attempt` was made
**partial** and two **nullable** members added in the owner module (`ScenarioVersion`, `CourseVersion`), stamped with a **SHA-256
fingerprint of the file's content**. A corrected key is then a **different version against the attempt that used the older one**, and a
ledger written before this still reads.

**AC-2 needed words, not numbers.** `CourseResult` scores an unattempted course as **zero** — exactly like a failure — and `Passed` is
null both when a course is incomplete and when there is no pass mark. The page derives **five distinct sentences** from *Attempted*,
*Completed* and *Passed* together rather than re-deriving the engine's numbers.

**AC-1 is the engine's, not the page's.** The plan describes the policy as a rule the page applies (*"policy = best: the latest attempt
does not lower the recorded result"*), but in the engine the policy **selects which attempt is counted** and everything follows. The
page states the policy and marks the counting attempt; the gateway proves the effect by recording a **worse second attempt** and
reading the counted score under `latest`, `best` and `first`.

**AC-3 is tested against a real refusal.** With the ledger's folder replaced by a file, the marking still comes back with
**`Recorded` false and the failure reference**, and the page prints **NOT RECORDED** — and the ledger's writability is probed **on
load**, so the page says so before an attempt is lost to it.

**The open question is answered from the engine.** *"`ResultsLedger` storage location is unclear; confirm it is file-based and record
the path."* The engine stores nothing itself — its own remark is that where it lives is the host's decision — so the path is
**`<data root>/Training/results.json`**, printed on the page beside whether the file exists and whether it can be read and written.

**The answer entry was moved into `OgTrainingRules`**, so the runner and the course page cannot disagree about what a trainee wrote.

**Not built, and stated as gaps:** a declared version the authors bump by hand (the version is a content fingerprint); choosing an
arbitrary scenario on this page (recording follows the course's order, and picking one is the scenarios page's job); a register of
trainees who have never attempted anything; a cohort filter by course or date; locking on the ledger; the engine's CSV ledger exports;
and splitting `training.read` from `training.admin` into two page gates.

## 9. Open questions and risks

- `ResultsLedger` storage location is unclear; confirm it is file-based and record the path.
