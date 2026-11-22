# STG-02 — Wells, wellbores and stratigraphy

| Field | Value |
|---|---|
| Stage | STG-02 |
| Features | 6 (WEL-01 … WEL-06) |
| Depends on | STG-01 |
| Status | Gate met - WEL-01..06 all met: well index, header, wellbores, status history, stratigraphy and comparison |
| Exit gate | A user can find a well, edit its header, see its wellbores, change its status with history, and compare two wells |

---

## 1. Objective

The well is the anchor entity of the whole product. Every later stage — exploration, drilling,
production, HSE, decommissioning — is scoped by company → field → well. This stage establishes
that anchor and the context model every other page reuses.

---

## 2. Scope

**In**

- Well search with field/status/type filters and full-text on name, UWI, API number.
- Well header editor: identity, classification, location, dates, operator, business associates.
- Wellbore list and editor, including surface location and parent/child relationships.
- Well status classification with an append-only history and reason.
- Stratigraphy browser: units, hierarchy, columns, well sections.
- Well comparison across selected properties.
- The shared **well context** control that later stages inherit.

**Out**

- Logs, surveys, completions (STG-15 / STG-16).
- Drilling programs (STG-04).
- Production volumes (STG-05).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.LifeCycle` | `WellManagementService`, `WellLifecycleService`, `ReservoirLifecycleService`, `WellComparisonService`, `WellManagement` process service |
| `Beep.OilandGas.LifeCycle/Services/DataMapping/WellMapper` | PPDM projection |
| `Beep.OilandGas.PPDM39.DataManagement` | `IWellStatusService`, `WellServices*` (8 partials), `PPDMGenericDataService` |
| `Beep.OilandGas.PPDM.Models` | `WELL`, `WELLBORE`, `WELL_XREF`, `STRAT_UNIT`, `STRAT_COLUMN`, `STRAT_WELL_SECTION`, `R_WELL_STATUS`, `BUSINESS_ASSOCIATE` |
| `Beep.OilandGas.Client` | `IBeepOilandGasApp.Well`, `.Field`, `.LifeCycle` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| WEL-01 | Well search and index | [WEL-01](../features/WEL/WEL-01-Well-Search-and-Index.md) |
| WEL-02 | Well header and business associates | [WEL-02](../features/WEL/WEL-02-Well-Header-and-Business-Associates.md) |
| WEL-03 | Wellbore management | [WEL-03](../features/WEL/WEL-03-Wellbore-Management.md) |
| WEL-04 | Well status classification and history | [WEL-04](../features/WEL/WEL-04-Well-Status-Classification-and-History.md) |
| WEL-05 | Stratigraphy browser | [WEL-05](../features/WEL/WEL-05-Stratigraphy-Browser.md) |
| WEL-06 | Well comparison | [WEL-06](../features/WEL/WEL-06-Well-Comparison.md) |

---

## 5. New code introduced

| Project | Key types |
|---|---|
| `.Modules/Wells` | `WellSearchViewModel/View`, `WellHeaderViewModel/View`, `WellboreViewModel/View`, `WellStatusViewModel/View`, `StratigraphyViewModel/View`, `WellCompareViewModel/View` |
| `.Controls` | `WellContextBar` (company · field · well), `HierarchyTree`, `DeltaGrid` for comparison |

---

## 6. Architecture notes

- **Context propagation.** The shell exposes a `WellContext` (company, field, well) that pages
  observe. Changing it cancels an in-flight load and clears the page — the same rule Web applies
  in its `*State.cs` classes.
- **Status is append-only.** A status change writes a new history row with actor, reason and
  timestamp. Edit-in-place is refused.
- **Well identity is not inferred.** If `WELL_XREF` gives more than one identity for the same
  well, the screen shows the conflict and refuses to pick one. Web has been burned by
  inferring identity from display names; do not repeat it.
- **Comparison is explicit.** `WellComparisonService` compares named, unit-annotated properties;
  the screen must not compare values in different units silently.

---

## 7. Dependencies and sequencing

1. WEL-01 (search) → WEL-02, WEL-03.
2. WEL-03 → WEL-04.
3. WEL-05 independent once search exists.
4. WEL-06 needs WEL-01 and WEL-02.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-02-1 | Search returns results for name, UWI and API number | test `WEL-01/T02` |
| SG-02-2 | 5 000 wells page and filter within budget (REL-04) | timing capture |
| SG-02-3 | Header edit round-trips and preserves untouched columns | test `WEL-02/T02` |
| SG-02-4 | Conflicting well identity is refused, not resolved by guessing | test `WEL-02/T04` |
| SG-02-5 | Wellbore list is scoped to the selected well only | test `WEL-03/T01` |
| SG-02-6 | Status change appends history and cannot be edited | test `WEL-04/T02` |
| SG-02-7 | Stratigraphy hierarchy renders 3 levels and links to well sections | screenshot |
| SG-02-8 | Comparison refuses mismatched units | test `WEL-06/T03` |
| SG-02-9 | Changing company/field clears the page and discards late responses | test `STG-02/T09` |

---

## 9. Test plan

| Test | What it proves |
|---|---|
| `WEL-01/T02` | Three identity search paths |
| `WEL-01/T05` | Empty result is distinct from failed read |
| `WEL-02/T02` | Header round-trip |
| `WEL-02/T04` | Ambiguous identity refused |
| `WEL-03/T01` | Wellbore scoping |
| `WEL-04/T02` | Append-only history |
| `WEL-05/T01` | Hierarchy ordering |
| `WEL-06/T03` | Unit mismatch refused |
| `STG-02/T09` | Context change invalidation |

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| `WellComparisonService` API shape unknown | Inspect `LifeCycle/Services/WellComparisonService.cs` before designing WEL-06 |
| Stratigraphy has four Web pages with overlapping behaviour | Consolidate into one browser with tabs; document the mapping |
| `R_WELL_STATUS` seed content varies by database | Read from the reference table; never hardcode status names |
