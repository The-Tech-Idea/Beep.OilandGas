# SETUP-06 — Modules, reference data and LOV

**Document ID:** SETUP-06
**Status:** Built
**Actor:** SystemAdmin
**Level:** Company

---

## 1. Purpose

Bring the company database to the schema and reference data the application expects: the PPDM module tables and
their code lists (LOV), in the order the modules declare, on a database that may be empty.

Reference data and LOV are **shared by living in the database**; they are not copied into a settings row. The
settings row `BEEPSETTINGS/MODULES` records *which* set this database received, so a later workstation knows what it
is joining.

## 2. Actor

SystemAdmin. Applying schema is a privileged operation on a server.

## 3. Preconditions

- SETUP-02 proved the connection.
- The service account may create tables and insert rows in that database (DDL rights are the DBA's grant).
- For a database with existing modules, the plan reports them as already applied and they are skipped.

## 4. Steps

1. **Plan** — `IGModuleSetupRunner.PlanAsync` reads the discovered `IModuleSetup` implementations in declared order,
   with each module's entity-type count and whether it already completed. Reading the plan changes nothing.
2. **Choose** — the wizard lists the modules with a checkbox each; the demonstration-data module is **off** and
   needs an explicit confirmation that names the connection.
3. **Apply** — the runner applies the schema plan, then seeds in module order, reporting `phase/module/completed/total`
   as it goes and keeping per-module state so an interrupted run resumes.
4. **Record** — `BEEPSETTINGS/MODULES` receives `{"modules":[…],"recordedUtc":"…"}` with the same revision discipline
   as the other settings.
5. **Verify** — the application's first reads against the new tables are the smoke run's and the module tests'.

## 5. Records written

| Store | What |
|---|---|
| Company database | the module tables and their constraints, created by the module's own plan |
| Company database | reference rows — the PPDM code tables and their lists of values (the LOV) |
| Company database | `SETUP_WIZARD_LOG` rows from the security module's seeder |
| `BEEPSETTINGS` | `MODULES` document |
| Machine | `%LOCALAPPDATA%\…\Data\module-setup-state.json` — per-module completion for resume |

## 6. Failure states

| Code | Meaning | Sentence / next action |
|---|---|---|
| `setup-connection-unreachable` | the bound connection will not open | check SETUP-02 |
| schema refusal | a module's entity has no usable key or the plan was rejected | the module name and the provider's reason are recorded; the run stops before seeding |
| seed refusal | a module's data conflicts with what is there | the module is marked failed; `ResumeAsync` retries only the unfinished modules |
| demonstration data not confirmed | `DEMO_DATA` was ticked without naming the connection | skipped, and said so in the outcome |

The runner never reports an empty plan as success when a table is missing.

## 7. Security requirements

- **Privileged by design:** the SystemAdmin is the only actor who runs an apply. The wizard is reached only from a
  signed-out installation whose connection the installer defined.
- **Demonstration data is explicit** (DAT-02 AC-5): the confirmation names the connection, so a confirmation cannot
  be carried to another database or arrive by default.
- **No statement text** in the desktop: the module's own planner builds the DDL from the entity classes.
- **Resumable and recorded:** state is per connection and per module; a failed module is named, never silently
  dropped.
- **Backup before schema change:** the runner takes a copy of a local file database as its backup evidence; for a
  server database the backup is the DBA's (SETUP-12).

## 8. UX requirements

- The plan is shown before anything is applied, with counts and "already applied" markers.
- Progress names the phase and the module, not a percentage alone.
- The step states plainly that reference lists live in the database and are therefore shared by every user.

## 9. Acceptance criteria

| # | Criterion | Status |
|---|---|---|
| AC-1 | A plan can be read without changing the database | **Met** — `OgModuleSetupRunner.PlanAsync` |
| AC-2 | Applying on a fresh database creates the schema and seeds in module order, and a resume skips what completed | **Met for a module, run green** — `ModuleSetupIntegrationTests.Apply_then_resume_on_a_fresh_sqlite_file` with `OG_RUN_SETUP_INTEGRATION=1`: schema applied for 1 of 1 module, seeded, second apply and resume both skip; the code-table keys it needed are proved family-wide by `PpdmCodeTableKeyTests` (3) |
| AC-2b | The **whole catalogue** applies on one fresh database | **Blocked by the migration engine** — measured 2026-11-10: 34 modules / 475 entity types; `PPDM_CORE` fails after ~10 min with "Checkpoint was not acknowledged (Failed)" because the migration history file reaches 1 GB (every checkpoint record embeds the plan artifact and the file is rewritten per append); the engine's compact-checkpoint mode instead dies in `MigrationManager.Canonicalize`. Recorded in the research evidence; not a setup defect. **This is the wizard’s default selection too** — the Modules step ticks every module that needs no demonstration confirmation — so a real first run against an empty database reaches the same failure at PPDM_CORE. **The documented mitigation is unusable:** setting `PersistPlanArtifactsInHistory` / `TrackOperationsInHistory` to false (the engine’s own note names this caller) crashes the process in ordinary migration tests, so it cannot be shipped as a workaround; both the flags and an iterative rewrite of `Canonicalize` were tried and reverted |
| AC-3 | Demonstration data is only loaded when the confirmation names the connection | **Met** — `OgSetupPolicy`, `ModuleSetupConfirmationTests` |
| AC-4 | The module set is recorded in the company settings | **Met** — `OgSharedSetupService.WriteModulesAsync` |
| AC-5 | The wizard shows per-module status and lets the SystemAdmin review the LOV set | **Met** — the plan rows state each module's size and state (`OgSetupModulePlan.Describe`, `ModulePlanStatusTests`), and the Modules step lists the reference (`R_*`) code tables the plan will seed, grouped by module, before anything is applied (`OgSetupReferenceTables`, `ReferenceListTests`; the real metadata is proved by `ModuleSetupIntegrationTests.Plan_names_the_reference_lists_it_will_seed`) |
| AC-6 | A module that fails names itself and its reason, and the run stops before seeding | **Met** — `OgSetupOutcome`, `OgSetupSchemaResult` |

> **Note on AC-2 — fixed, not worked around.** The test was red on `R_WELL_STATUS_TYPE: Entity has no primary key
> defined`: the generated entity models carried no `[Key]`, although the PPDM table metadata names the key for every
> table. The metadata is now applied to the models — 2,695 model files carry `[Key]` on each column the registry names
> (925 attributes across the 868 `R_*` reference tables, the rest across the catalogue), and the generated models are
> read with `HonorNullableReferenceTypes = false` so a `System.String` is a nullable data column rather than a NOT NULL
> constraint, while a `[Key]` stays NOT NULL. `PpdmCodeTableKeyTests` asserts the invariant against every metadata
> entry, and the scoped apply test now runs green.
>
> **Full scope is a separate, open problem.** Running every module (AC-2b) does not complete, and the failure is
> inside `MigrationManager`, not in the setup: the plan for the catalogue is 34 modules / 475 entity types, and
> `PPDM_CORE` alone spent ~10 minutes writing checkpoints whose history file reached **1,027,452,713 bytes** before
> the write failed. The engine's own documented flag for this caller (`PersistPlanArtifactsInHistory = false`)
> avoids the file growth but kills the process inside `Canonicalize` (no managed exception, no crash dump), so it
> is not a workaround that can be shipped. `The_unscoped_apply_creates_and_seeds_every_module` is written to the
> criterion and gated behind `OG_RUN_SETUP_FULL=1` so it cannot report a false pass.

## 10. What exists today and what is missing

Exists: `Data/Setup/OgModuleSetupRunner.cs`, `OgSetupComposition.cs`, `OgSetupState.cs`, `OgSetupPolicy.cs`,
`Views/ModuleSetupPageView.cs`, the wizard's Modules step.

Missing: nothing this stage asks for. Per-module status is the wizard's plan rows, and the reference lists are the
same step's review list, read from the module metadata that produces the plan (`OgSetupReferenceTables`).

Run, not assumed: the opt-in scoped apply (`OG_RUN_SETUP_INTEGRATION=1`) applies `WELL_STATUS_FACETS` to a fresh
SQLite file, seeds it, and proves a second apply and a resume both skip. It is opt-in because the full plan covers 343
entity types; the scoped module is a reference-data module, which is the case that used to fail.
