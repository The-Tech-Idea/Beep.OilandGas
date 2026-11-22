# ENG-05 — Integrity status board

| Field | Value |
|---|---|
| Stage | STG-16 — Well engineering toolboxes |
| Area | Engineering |
| Status | Gate met - see [evidence](../../evidence/STG-16-ENG-05/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Integrity` |
| Desktop project | `Modules/Engineering/IntegrityBoard/` |
| External packages | `TheTechIdea.OilGas.Integrity` |
| Depends on | ENG-04 |
| Blocks | — |
| Web reference | `OilandGasTools/docs/customer/integrity.html` (field board) |

## 1. Purpose

One screen that answers "which wells are at risk right now", worst first, with the category and the
reason.

## 2. Source components

| Source | Reuse |
|---|---|
| `Integrity/IntegrityStatus.cs` | `Assess(WellModel, inputs) → WellIntegrity` |
| `Integrity/FieldIntegrity.cs` | `Counts`, `WorstFirst`, `ToCsv` |
| `Integrity/IntegrityBoardView.cs` | the board visual |
| `Integrity/CorrosionTrack.cs` | wall-loss context |

## 3. Screens

`Engineering → Integrity board`

- Category summary: Green / Yellow / Orange / Red counts for the field.
- Board: one cell per well, colour by category, sorted worst first, click for detail.
- Well detail drawer: category, contributing findings (metal loss, casing pressure, barrier gaps),
  last assessed date.
- Export CSV (`FieldIntegrity.ToCsv`).
- Filter: field, facility, category, last assessed age.

## 4. Data and contracts

`OgWellIntegrity { WellId, Category, Findings[], AssessedAt }`
Category comes from `IntegrityStatus`, never computed in the view model.

## 5. States

| State | Behaviour |
|---|---|
| Well not assessed | shown as "unassessed", distinct from green |
| Stale assessment | age badge; filtered by age |
| Partial inputs | category describes which inputs were missing |
| No wells | empty state with a filter action |
| Failed read | failed state; board cleared rather than stale |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `integrity.read` | no |
| Export | `data.export` | no |

## 7. Acceptance Criteria

- AC-1 Unassessed wells are visually distinct from green.
- AC-2 Worst-first ordering is stable and documented.
- AC-3 Category counts equal the number of wells in each category.
- AC-4 A finding names which input produced it.
- AC-5 CSV export matches the board.
- AC-6 A failed read never shows all-green.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ENG-05"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-16-ENG-05/observed-evidence.json).

**The category is never the desktop's.** Every well is `IntegrityStatus.Assess` over its **barrier set**, the **annulus
findings** and the **failing joints** — and the gateway asserts the outcomes that rule produces: Green for an intact
two-envelope set, Orange for a pressure above the MAWOP, Red for no barrier set, Yellow for a corroded tubing string.

| What the board shows | Recorded in |
|---|---|
| Well, name, field | `WELL` |
| Barrier envelopes and elements | the added `WELL_BARRIER` |
| Casing-pressure inputs | `WELL_ANNULUS` + `CASING_PRESSURE_TEST` (ENG-04) |
| Wall-loss inputs | `MFC_RESULT` + `MFC_JOINT` (ENG-03) |
| Category, reasons, findings, last assessed | the added `INTEGRITY_ASSESSMENT` |

**The inputs are rebuilt, not re-implemented.** The annulus findings come from running the owner's own classifier over the stored
readings with the MAWOP the test was judged against; the joints come from the stored re-rating columns. So the board and the
casing-pressure and metal-loss screens cannot disagree about what the evidence was.

**AC-1 is a wording rule, not a colour.** A well with no recorded assessment says **"unassessed" before its category**, so the two
can never be read as one — and the unit test asserts the category word never appears beside "unassessed" in that state. The
category is still shown, because it is what the owner says right now; the label says it has not been signed off.

**AC-6 is structural.** A failed read returns no board, and the page empties the owner's view, so a failure cannot leave a green
board on screen.

**The open question is followed rather than answered around.** `IntegrityBoardView` is a `ViewSurface`, so the page hosts it in
the shared `SkiaViewSurfaceHost` and fills its `Wells` from the assessment: the tiles, their colours and their order are the
tools'.

**A gap was closed in the owning module.** The category is read from a barrier set, and neither repository recorded one — every
well would have been uniformly red. `WELL_BARRIER` and `INTEGRITY_ASSESSMENT` now do, and each assessment keeps the reasons,
findings and envelope states it was read from.

**A rule surfaced by the fixture.** The owner flags **an element shared by both envelopes** as "a single failure may lead to leak
to surface" — so a set listing the same casing in both envelopes is **Orange by design**. The fixture gives each envelope its own
targets, which is the design the rule describes.

**Not built, and stated as gaps:** a configured staleness threshold (the age is shown and the filter supplies the threshold); a
write command in the feature's own table (it lists only View and Export, yet "last assessed" needs one, so **integrity.assess**
was added and named); verification or ageing of a barrier element (the set is read as recorded); carrying an older failing joint
forward once a newer caliper run exists; a slide-out detail drawer (the detail is a text block beside the grid); and paging, which
the board reads whole.

## 9. Open questions and risks

- `IntegrityBoardView` is a `ViewSurface`; reuse it inside `SkiaViewSurfaceHost` rather than
  re-drawing cells in WPF.
