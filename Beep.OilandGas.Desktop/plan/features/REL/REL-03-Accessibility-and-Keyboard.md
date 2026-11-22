# REL-03 — Accessibility and keyboard

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Release |
| Status | Gate met for what a build can prove - AC-3 now passes: the contrast guard closed the skin palette and the smoke run audits **66 skins with 0 failures** (was 66). AC-1, AC-4, AC-5 and AC-7 pass; AC-2 and AC-6 are met as far as a build can show, with the screen-reader and 200 % observations recorded as the owner’s. See [evidence](../../evidence/STG-18-REL-03/observed-evidence.json) |
| Owner projects | none |
| Desktop project | all |
| External packages | none |
| Depends on | every UI stage |
| Blocks | REL-01 |
| Web reference | ERP accessibility patterns; `OilandGasTools` `AccessibleItem`/`AccessibleChildren` |

## 1. Purpose

Make the product usable without a mouse, readable by a screen reader, and legible at 200 % scaling —
because engineering and field users often have neither a mouse nor good eyesight.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore/Hosting/ViewSurface.cs` | `AccessibleName`, `AccessibleDescription`, `AccessibleChildren`, `AccessibleItem` |
| `Wellbore/WellboreView.Accessibility.cs` | existing accessibility implementation |
| `Wellbore/WellboreView.Input.cs` | keyboard handling |
| `Wellbore/ViewKey.cs` | key model |
| WPF `AutomationPeer` | bridge to UI Automation |

## 3. Screens

Cross-cutting. Specific deliverables:

- Every page reachable by keyboard; logical tab order.
- Visible focus indicator on every interactive control, including the Skia host.
- Accessible names on buttons, fields, grids and pickers.
- Grid rows and cells exposed as automation elements.
- Charts and canvases expose a text alternative summarising the data.
- High-contrast skin usable; no information conveyed by colour alone.
- Zoom to 200 % without clipping.

## 4. Data and contracts

`SkiaViewSurfaceHost` exposes `AccessibleName`/`AccessibleChildren` through a custom `AutomationPeer`.
`ChartHost` exposes a `ChartSummary` automation property with the series names and value ranges.

## 5. States

| State | Behaviour |
|---|---|
| Focus outside the visible area | scrolled into view |
| Canvas focused | focus rectangle drawn around, not over, the canvas |
| Screen reader on a chart | announces the summary and the selected point |
| High contrast | theme tokens switch to the high-contrast palette |
| Zoom 200 % | layout reflows; no horizontal scroll for primary content |

## 6. Commands and permissions

Keyboard map (global): `Ctrl+K` command palette · `F6` cycle panes · `Ctrl+Tab` next page ·
`Alt+←/→` back/forward · `Esc` close dialog · `F1` context help.

## 7. Acceptance Criteria

- AC-1 Every interactive control is reachable by Tab and has an accessible name.
- AC-2 Focus is always visible.
- AC-3 Contrast meets WCAG 2.2 AA for text and UI components in every skin.
- AC-4 No state is conveyed by colour alone (icons or text accompany it).
- AC-5 Charts and canvases expose a text summary.
- AC-6 200 % scaling causes no clipping of primary content.
- AC-7 The command palette works without a mouse.

## 8. Verification

Manual checklist plus: automated colour-contrast computation over the theme tokens; keyboard-only walk
through the five most important pages.

Blind spot: a checklist is not a screen-reader test; record which reader and version was used.

### Delivered 2026-11-09 - the skin contrast failure closed

**The defect.** All 66 control-library skins failed the same pair: **`#F9AB00` on `#FFFBFE` = 1.89:1** where a status colour used as text and
component state needs **3:1**. The library resolves **one semantic palette for all 66 design systems** — its design systems change templates,
geometry and chrome, not colours — so the number was **one defect counted 66 times**, and one correction fixes all 66. That also explains the
earlier finding that the High-Contrast skin left the desktop's tokens byte-identical.

**The fix.** `OgSkins.Apply` now enforces the palette after the skin is applied: `OgThemeContrast.Enforce` moves **only** the colours that fail,
**as far as the threshold needs and no further**, keeping the hue (`#F9AB00 -> #C48700`), and writes them into one dictionary of the
application's own that sits last in the merge chain. A palette that already passes is returned **byte-identical**, so the guard cannot quietly
restyle a skin.

**A pair is the unit, not a foreground.** White text on `#F9AB00` cannot be lightened any further, so the **accent is darkened under the label**
instead — the label stays white and the amber becomes `#C48700`. The order matters too: the accent is settled first, then its label, against the
accent as it will finally be.

**Two of my own mistakes, caught by evidence.** A first version wrote into the application resource dictionary, which outranks every merged
one, so each skin after the first audited **the previous skin's adjustment** — 66 failures became **0 with nothing fixed** and the adjustments
list was **empty**. That empty list is what exposed it. And a foreground-only adjustment **looped** on white. The smoke run now prints
**every failure and every colour it moved**, so the next false pass is visible in the record.

| Measurement | Before | After |
|---|---|---|
| `skin-contrast-audited` | 66 | 66 |
| `skin-contrast-failures` | 66 | **0** |
| `skin-contrast-text-failures` | 66 | **0** |
| `skin-contrast-first-failure` | `AntDesign: #F9AB00 on #FFFBFE = 1.89:1` | `(none)` |
| `skin-contrast-adjustments` | not reported | 66 lines, one per skin |
| `accessibility-unnamed-count` | 0 | 0 |
| `accessibility-untabbable-count` | 0 | 0 |

**Still the owner's, and recorded rather than claimed:** a **named screen reader** against the five key journeys (AC-2, §8's blind spot) and
the **200 % scaling** observation (AC-6). The build shows the focus ring exists, that every control is named and tabbable, and that the layout
has no fixed widths for primary content; it cannot show either observation.

## 9. Open questions and risks

- WPF UI Automation on a Skia canvas is coarse. Accept a summary-level exposure and record the limit
  rather than claiming full canvas accessibility.

### Delivered 2026-11-06

**Naming (AC-1).** `OgAccessibilityAudit.UnnamedInteractive` walks a page's logical tree and reports every
interactive control UI Automation could not name — an explicit `AutomationProperties.Name`, a `LabeledBy`
reference, or (for a button/checkbox/tab) its own text. The first run found **45 unnamed controls** on 15
pages: every text box, combo box, password box and grid that relied on a neighbouring caption. All now carry a
name describing *what it holds* (for example `"Licence file path"`, `"Store password"`, `"Survey stations"`),
not a generic label. The smoke run and a test both assert the count is **0** across all 20 declared pages.

**Tab order (AC-1).** `OgAccessibilityAudit.NotInTabOrder` reports focusable controls excluded from Tab. The
first run found **14 read-only `DataGrid`s and one `TabControl`** — focusable by WPF but not tab stops, so a
keyboard user could not reach a grid at all. Each is now an explicit `IsTabStop = true`; the count is 0.

**Focus (AC-2).** `SkiaViewSurfaceHost` is a tab stop and draws an explicit focus ring: a 2 px border on the
host, so it frames the image rather than covering it, coloured from `BeepAccentBrush` by an `IsKeyboardFocused`
trigger (`IsFocusRingVisible` exposes the state for tests). It had relied on the framework's default focus
visual, which a restyled control is not guaranteed to have. Every other control uses the control library's own
focus visuals, which are its contract and are **not** audited here.

**Contrast (AC-3).** `OgContrast` implements WCAG 2.2 relative luminance and gives 21:1 for black on white and
exactly 1:1 for identical colours. `OgThemeContrast` audits nine pairs of the desktop's own tokens: body text on
the three surfaces at 4.5:1, and accent/danger/success/warning at 3:1. The shipped palette **passes all nine**;
an adverse palette fails three, so the audit is not vacuous. The smoke run prints the exact colours audited.

**The library's skins do not pass.** `AuditSkin` reads each control-library skin's own semantic brushes
(`BeepStyleOnSurfaceBrush`, `BeepStyleSurfaceBrush`, `BeepStyleAccentBrush`, `BeepStyleAccentForegroundBrush`,
the status brushes) after applying it and audits eight pairs. The smoke run applies **all 66** skins: **66 of
66 fail at least one pair**. The first is AntDesign: white `#FFFFFF` on accent `#F9AB00` = **1.89:1** where
button text needs 4.5:1; the same shape recurs across the set. This is a **control-library palette defect**, not
a desktop token problem: the desktop's own chrome tokens pass, and fixing the skins means changing the library
(or overriding every skin's accent foreground here, which would fight the brand). **AC-3 is therefore not met**,
and the finding is recorded with its numbers rather than hidden behind the passing chrome audit.

**Honest finding about skins:** switching to the **High-Contrast** skin leaves the desktop's nine chrome
tokens **byte-identical** (`accessibility-skins-change-tokens: False`). The desktop's chrome palette is
skin-independent, so "contrast in every skin" is the same palette 66 times for the chrome the product draws.
The **control library's per-skin template colours are not audited** — that is the remaining gap in AC-3.

**Not colour alone (AC-4).** A state is never carried by hue: the connection-health line now reads
`Health: OK` / `Health: attention needed` in the primary text colour with the state named, instead of turning
amber at 3.9:1. Cartesian charts gained a **legend** (swatch + series name) so oil and gas are distinguishable
without colour; pie and donut already had one.

**Text alternatives (AC-5).** `SkiaViewSurfaceHost` names the canvas from the surface's `AccessibleName` and
sets its help text to the description plus each accessible child's name and role. `ChartSurface` produces a
summary — type, series count and each series' value range (`line chart with 2 series: Oil 10 to 25; Gas 8 to
18.`) — or `… chart with no data.` when empty.

**Reflow (AC-6).** Every page's content is hosted in a `ScrollViewer` with vertical scrolling automatic and
**horizontal scrolling disabled**, so primary content must reflow rather than disappear sideways. A test now lays
**all 22 declared pages** out at **620 DIP** - the logical width of a 1240 px window at 200 % - and asserts
`ScrollableWidth <= 1`, with a guard that the viewport really is 400-620 DIP so the test cannot pass on an empty
tree. (The first version of that test *did* pass vacuously with `ViewportWidth == 0`; the guard caught it.) No
real 200 % display was inspected.

**Keyboard (AC-7).** `Ctrl+K` opens a command palette (`OgCommandPalette` filter/movement is pure and tested:
case-insensitive, every term must match, wrap-around), `Ctrl+Tab` / `Ctrl+Shift+Tab` move pages, `F6` cycles
navigation ↔ page, `Alt+Left` walks back through visited pages, `F1` opens the shortcut list, `Esc` closes.
A source guard asserts the shortcut list matches what the shell actually binds, so the reference cannot
promise a key nobody handles.

### A defect the screenshot found

The smoke run reported `charts-exported: True` while the written PNG said **"No data"**: `RenderEmpty()` had
set the empty-series switch and `RenderType` did not clear it, so the export was of the placeholder. Fixed in
the page (`RenderType` clears the switch and rejects an unknown type), and pinned by three assertions — the
request carries **2** series before export, the PNG is **> 5 KB** rather than the ~2.9 KB placeholder, and a
test exports a data chart and an empty chart and requires different bytes. Reading the image is what exposed it;
the earlier signature-only check was too weak.

### Still open

- **AC-2 and AC-6 are verified structurally, not visually.** The focus ring's geometry and trigger are
  tested; no rendered focus state was inspected, and no page was rendered on a real 200 % display.
- **AC-3's remaining gap is the control library's per-skin colours.** The desktop's own tokens are audited and
  pass; the 66 skins restyle the library's templates with colours this audit does not read.
- **No screen-reader run.** The plan's §8 blind spot stands: no NVDA/JAWS/Narrator session has been recorded,
  and which reader and version was used is the thing an accessibility claim needs.
- **No keyboard-only walk-through** of the five most important pages by a person; the map is implemented and
  its bindings are guarded, but the human pass in §8 has not happened.
- Canvas automation is summary-level only: a screen reader hears the chart's summary, not its points.