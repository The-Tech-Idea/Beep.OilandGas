# SETUP-01 — Deployment choice (machine)

**Document ID:** SETUP-01
**Status:** Built
**Actor:** Installer (whoever runs the product first on this computer)
**Level:** Machine

---

## 1. Purpose

Decide, once per computer, whether this installation keeps its own database file or uses a company database on a
server. The answer decides everything after it: where the company record lives, where accounts come from, and
whether a local file exists at all.

This stage records **only facts about the machine**. It must never ask for the company name, the company's calendar
or an administrator password — those belong to SETUP-03 and SETUP-04 and are created in the company database.

## 2. Actor

The installer. On a well-run site this is IT, not an end user; on a personal installation it is the same person who
will use the product.

## 3. Preconditions

- The application starts and the data root is writable (`OgPaths.DataRoot`, created at startup).
- The installation's database has no company record yet, or has none and no account (the probe reports it).
- No account has been created for this deployment yet (otherwise the gate goes to sign-in, SETUP-09).

## 4. Steps

1. The wizard shows the two deployments with the consequence of each written next to it:
   - **This workstation only** — a database file at `…\Data\Local\BeepOilGas.db`; the company record and settings
     live beside it on this computer.
   - **A company database** — a database on a server; the company record, the module set, the environment and the
     accounts live in that database for every user.
2. Personal preselected when there is no reason to prefer otherwise; a re-run preselects what was recorded before.
3. The choice is held in the wizard until Review; nothing is written at this step.
4. Review realises the choice as the connection and creates the company in the database (see §5).

## 5. Records written

Nothing at this step. The choice becomes the **connection** — a local database file (`IsFile = true`) or a server — and the
deployment is then read from that connection. No settings file is written:

| File | Field | Value |
|---|---|---|
| the installation connection | `IsFile` | `true` means personal; `false` means a company database |
| | `DataRoot` | the resolved data root |
| | `CompletedAtUtc` | when Review finished |

## 6. Failure states

| Code | When | What the person sees |
|---|---|---|
| `setup-not-writable` | the data root cannot be created or written | the path and the reason; the wizard does not open |
| — | the person cancels | nothing is written; the application exits 0; next start offers setup again |

## 7. Security requirements

- The choice is a fact, not a claim: the deployment recorded here is the deployment the gate and the sign-in use.
  It cannot be changed by editing a setting inside the application later (changing deployment is a re-install).
- No credential is collected at this stage, so nothing sensitive is at rest yet.
- The data root is the only writable location (ADR-013); nothing is written beside the executable.

## 8. UX requirements

- Each option states its consequence in one sentence; the personal option shows the exact file path.
- The choice is never a silent default: a company installation must be a deliberate act, because it creates
  company-wide records.
- The wizard's title and the first step name the product and the action ("Set up Beep Oil & Gas"), not a code.

## 9. Acceptance criteria

| # | Criterion |
|---|---|
| AC-1 | The step offers exactly two deployments and writes nothing until Review. |
| AC-2 | A re-run preselects the deployment and the connection read back from the installation's database. | **Met** — `FirstRunWindow.Prefill` reads `OgApplication.InstalledDatabase` (probe) and preselects `IsFile`. |
| AC-3 | Cancelling writes nothing and exits cleanly. |
| AC-4 | Only `Deployment` and `DataRoot` (machine facts) are written here; no company field is written. |
| AC-5 | Changing the deployment requires a re-install, not an in-app setting. |

## 10. What exists today

- `src/Beep.OilandGas.Desktop/Setup/FirstRunWindow.cs` — the two radio buttons, per-option copy, and the Review
  write of `OgSetupState`.
- `src/Beep.OilandGas.Desktop.Data/Setup/OgInstallationProbe.cs` — the deployment is read from the connection (`IsFile`), not from a recorded choice.
- `src/Beep.OilandGas.Desktop/App.xaml.cs` — the gate reads the recorded deployment.

Missing: nothing at this stage. The deployment is read back from the connection on a re-run, and the same fact
drives the startup gate (SETUP-12) and sign-in (SETUP-09).
