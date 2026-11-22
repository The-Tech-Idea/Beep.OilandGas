# REL-05 — Release acceptance gates

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Release |
| Status | Gate met - see [evidence](../../evidence/STG-18-REL-05/observed-evidence.json); the checklist itself reads **NOT READY TO SHIP** on this build, with the outstanding items recorded |
| Owner projects | none |
| Desktop project | all |
| External packages | none |
| Depends on | every stage |
| Blocks | release |
| Web reference | `MASTER-TODO-TRACKER.md` evidence discipline |

## 1. Purpose

Define exactly what "ready to ship" means, and refuse to claim it on the strength of a build that
compiled.

## 2. Source components

The gates below summarise what every stage document already requires.

## 3. Screens

- `Settings → Diagnostics → Release acceptance`: the checklist with live pass/fail where it can be
  computed, and manual attestations with who and when.
- Evidence pack export: zips `plan/evidence/**` with a manifest.

## 4. Data and contracts

`OgReleaseAcceptance { Item, State (Pass / Fail / NotRun / Waived), Evidence, AttestedBy, AttestedAt }`

Waivers require an approver name, a reason and an expiry — the same discipline the repository
tracker already uses.

## 5. States

| State | Behaviour |
|---|---|
| Not run | never displayed as pass |
| Waived | shown with the approver and expiry |
| Expired waiver | reverts to fail |
| Evidence link broken | fail with the missing path |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View checklist | any user |
| Attest an item | `release.attest` |
| Waive an item | `release.waive` (different person) |

## 7. Acceptance criteria

The release checklist — every item must be `Pass` or a valid waiver:

**Build and code**
1. Solution builds with 0 errors and no new warnings.
2. No `BEEP0001..0004` diagnostic, `CS8509` clean.
3. No `catch` without a report or rethrow (guard test).
4. No hardcoded colour or font in `Modules/**` (guard test).
5. No user-visible literal outside resources (guard test).
6. Every catalog route has exactly one view case and an icon (guard tests).
7. No `PackageReference` carries a version (central package management guard).

**Behaviour**
8. `--smoke` writes `result: ok` and `views-constructed: True`.
9. Every feature's acceptance criteria have a passing test or a recorded manual result.
10. Every page implements empty / loading / failed / denied / unsaved and each was seen.
11. Every page is in `OgModuleCatalog` and refuses without entitlement or role.
12. Every write command follows the retained-original pattern (ADR-010).
13. Every page's user-visible failures arrive as toasts from the one pipeline, each with a reference
    and no exception text.
14. Every writing command and every refusal produced exactly one audit row.
15. `IOgToastService` is injected only into `OgNotificationHub` (guard).
16. The startup `MessageBox` is the only `MessageBox` outside `Dialogs/` (guard).
17. `CatchReportingTests` finds zero logger-only or empty catches across the solution.

**Data and safety**
18. No screen opens a database connection outside `.Data` (guard test).
19. No screen writes into a closed period.
20. No screen defaults a jurisdiction.
21. No failed read renders as zero.
22. No licence expiry path corrupts data.

**Quality**
23. REL-03 accessibility checklist complete with the screen-reader name and version recorded.
24. Toast is keyboard reachable, announced by a screen reader and never the only carrier of state.
25. REL-04 budgets measured and within limits.
26. Localization complete for the shipped cultures; RTL verified on the five key pages.

**Delivery**
27. Installer tested on a clean VM; install log and smoke result attached.
28. Upgrade preserves the data root and setup state.
29. Privacy and licence notices present and matching `Beep.OilandGas.Web/Pages/Legal/*`.
30. Failure-record retention is configured and the privacy notice covers failure and audit records.

## 8. Verification

```
dotnet build src/Beep.OilandGas.Desktop.slnx -c Release
dotnet run --project src/Beep.OilandGas.Desktop -- --smoke
dotnet run --project src/Beep.OilandGas.Desktop -- --benchmark
```

Then walk the checklist and attach evidence for each item.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-18-REL-05/observed-evidence.json).

**Thirty items in five areas.** Five are **computed from artefacts that actually exist**, and twenty-five are **attested with evidence** —
because a clean Windows VM and a screen reader cannot be observed from inside a running application.

| Computed item | Where it is read from |
|---|---|
| 07 no `PackageReference` carries a version | the checkout's project files |
| 08 `result: ok` and `views-constructed: True` | `<data root>/smoke-result.txt` |
| 18 no `IDMEEditor` outside `.Data` | the checkout's screen sources |
| 23 accessibility counters | the same smoke result: unnamed, untabbable and skin contrast |
| 25 budgets within their limits | the same smoke result: `performance-over-budget` |

**On this build the checklist reads NOT READY TO SHIP on its own numbers.** Item 23 computes **66 skin contrast failures** and stays in the
blockers; the smoke and budget items pass; the four source and artefact items pass. Nothing was softened into "not run".

**Nothing unproven is a pass.** A missing smoke result is `NotRun` with the command to run; an artefact that cannot be read is reported and the
item stays `NotRun`; a result line that says failed, or is absent, is `Fail`.

**The evidence rule is one rule.** A first version decided "is this a file?" in the state check but treated every evidence string as a path in
the pack's missing-link report, so a manual observation appeared as a missing file. `OgReleaseRules.LooksLikePath` now answers for both, and
the gateway test catches any future disagreement.

**A broken link names the path** and passes once the file is there. **A waiver needs another person, a reason and a future expiry**, and an
**expired waiver applies as `Fail`** with the expiry and the approver in the sentence.

**The guard tests read every word.** Writing "catch" and the toast type's name into the checklist's own item text made two source guards fail
on **prose** — the guards are right to be literal, so the two items (and two keys) were reworded instead.

## Outstanding acceptance items

These are recorded, not hidden. They are the reason the checklist does not read READY:

| Item | What is outstanding |
|---|---|
| 27 / REL-01 AC-1 | the installer has **not been run on a clean Windows VM**; the feature says a developer machine is not evidence |
| 23 / REL-03 AC-2, AC-3, AC-6 | a **named screen reader** has not been recorded against the five key journeys, and the **66 skin contrast failures** remain |
| 09 for INT-02 AC-2, INT-03 AC-1 | the **Energistics XSDs** and the **OSDU JSON schemas** are not in this repository, so those criteria cannot be demonstrated |
| 26 | localization is complete for the two shipped cultures, but the **RTL pass on the five key pages** is a manual observation |
| 27 to 30 | delivery: the clean VM, the legal notices matched to the web pages, and the retention configuration |

**Not built, and stated as gaps:** the checklist **does not block packaging** (it reports; the release process gates); attestations have **no
history** and cannot be withdrawn, only replaced; the source scans are **textual** (a version set through a property would pass item 7); the
pack collects **JSON evidence only**; and the waiver expiry uses the **machine clock**.

## 9. Open questions and risks

- Thirty items is a large attestation surface. Split it into "must" (1–30) and "should"
  (localization breadth) before release so scope cannot be quietly expanded.
