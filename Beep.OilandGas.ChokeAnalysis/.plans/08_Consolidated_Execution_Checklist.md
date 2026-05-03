# Consolidated execution checklist (ChokeAnalysis)

Use this as a **single runbook** when implementing or reviewing choke work. Deep rationale and references: [07_Scenarios_Best_Practices_And_Industry_Reference.md](07_Scenarios_Best_Practices_And_Industry_Reference.md).

---

## A. Before coding

- [ ] Read [CLAUDE.md](../../CLAUDE.md) pre-edit checklist (Program.cs DI order if touching ApiService).
- [ ] Confirm scope: calculation-only vs PPDM path vs API (see phase 0).
- [ ] List **units** for this change (psia vs psig, °R, MMscf/d, 64ths in.) — document in README if new.

---

## B. Contracts and models (Phase 1)

- [ ] New public behavior → add to `IChokeAnalysisService` (`Beep.OilandGas.Models.Core.Interfaces`).
- [ ] Shared types → `Beep.OilandGas.Models` only; no `DTO` namespaces.
- [ ] Table vs projection: persisted entities remain scalar `ModelEntityBase` shapes.

---

## C. Calculation and validation (Phase 2)

- [ ] Regime: compute **critical vs subcritical** from \(k\), \(P_1\), \(P_2\) (absolute pressures); do not mix equation branches.
- [ ] Guard **division by zero** (area, \(\rho\), invalid ratios).
- [ ] Defaults (\(C_D\), \(Z\), assumed \(P_2\)) → log or validation warning where safety-sensitive.
- [ ] Multiphase: use correlation appropriate to **GLR band** and regime per [07 §4](07_Scenarios_Best_Practices_And_Industry_Reference.md).

---

## D. PPDM and wells (Phase 3)

- [ ] No raw `PPDMGenericRepository` for `WELL` / `WELL_STATUS` / `WELL_XREF` inside ChokeAnalysis — use **WellServices** upstream if reads are needed.
- [ ] Map `WELL_TEST_FLOW_MEAS`, pressures, tubulars explicitly; document fallbacks.

---

## E. API / orchestration (Phase 4)

- [ ] Choke HTTP path: `ICalculationService.PerformChokeAnalysisAsync` → `CalculationsController` — no duplicated choke logic outside `IChokeAnalysisService`.
- [ ] Align **field orchestrator** injection of `FieldId` with DCA/Nodal if request supports it.

---

## F. Tests (Phase 5)

- [ ] Single-phase gas: critical point, subcritical branch, edge near \(r \approx r_c\).
- [ ] Multiphase (if implemented): critical empirical vs subcritical mechanistic — separate tests.
- [ ] API: 200 / 400 paths for choke endpoint.

---

## G. Packaging and docs (Phase 6)

- [ ] `PackageReadmeFile` + packed `README.md` in `.csproj`.
- [ ] `dotnet pack` clean; README matches API.

---

## H. Sign-off

- [ ] `dotnet build` ChokeAnalysis + ApiService **0 errors**; target **0 warnings** on touched projects.
- [ ] Update [MASTER-TODO-TRACKER.md](../MASTER-TODO-TRACKER.md) phase status / next actions when applicable.
