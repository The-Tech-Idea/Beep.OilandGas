# VSB-01 — SkiaSharp view host control

| Field | Value |
|---|---|
| Stage | STG-15 — Wellbore visual suite |
| Area | Visuals |
| Status | Gate met — render, mouse, wheel, keys, pinch/pan, accessibility, contained failure. See [evidence](../../evidence/STG-15-VSB-01/observed-evidence.json) | |
| Owner projects | `TheTechIdea.OilGas.Wellbore` (`Hosting.ViewSurface`) |
| Desktop project | `src/Beep.OilandGas.Desktop.Controls/SkiaViewSurfaceHost.cs` |
| External packages | `SkiaSharp`, `SkiaSharp.Views.WPF` |
| Depends on | FND-04 |
| Blocks | VSB-03 … VSB-09, ENG-01 … ENG-10, INT-07, INT-08, RPT-03 |
| Web reference | none — the Web renders to a data URL |

## 1. Purpose

The single WPF control that hosts any `OilandGasTools` view. Built once; used by every visual screen
and every chart in the product.

## 2. Source components

| Source | Reuse |
|---|---|
| `OilandGasTools/src/TheTechIdea.OilGas.Wellbore/Hosting/ViewSurface.cs` | the contract: `Draw`, `Invalidated`, `HandleMouse*`, `HandleMouseWheel`, `HandleKey`, `HandlePinch`, `HandleTwoFingerPan`, `AccessibleName`, `AccessibleChildren`, `SettingChanged` |
| `OilandGasTools/samples/WinFormsViewer/MainForm.cs` | the reference host wiring (paint, mouse, wheel, leave, key map) |
| `OilandGasTools/README.md` §"Hosting a view in a UI" | the canonical 12-line recipe including the WPF case |
| `Beep.OilandGas.Drawing/Core/DrawingEngine.cs` | alternative renderer for chart screens (`RenderToCanvas`) |

## 3. Screens / API

No page. The control:

```csharp
public sealed class SkiaViewSurfaceHost : ContentControl
{
    public static readonly DependencyProperty SurfaceProperty;   // ViewSurface?
    public bool ShowToolbar { get; set; }        // zoom, fit, export, settings
    public bool AutoFit { get; set; }
}
```

Behaviour:
- `PaintSurface` → set `Surface.Width/Height` from `e.Info` → `Surface.Draw(e.Surface.Canvas)`.
- `Surface.Invalidated` → `Dispatcher.BeginInvoke(InvalidateVisual)` (one pending redraw).
- Mouse down/move/up → `HandleMouseDown/Move/Up` with `Point`→`SKPoint`.
- `MouseWheel` → `HandleMouseWheel(point, delta)`.
- `PreviewKeyDown` → `ViewKeyMap` → `HandleKey`.
- `ManipulationDelta` → `HandlePinch` / `HandleTwoFingerPan`.
- `AutomationPeer` exposing `AccessibleName` and children as automation elements.
- Focusable, with a visible focus rectangle that does not paint over the canvas.

## 4. Data and contracts

`ViewKeyMap` maps WPF `Key` to `ViewKey`: Up, Down, Left, Right, PageUp, PageDown, Home, End, OemPlus,
OemMinus, Tab, Enter, Escape, Delete, Z, Y. Modifiers map to `ViewModifiers` (`Shift`, `Control`,
`Alt`).

## 5. States

| State | Behaviour |
|---|---|
| `Surface` null | neutral placeholder, not a black canvas |
| Surface throws during draw | failure panel with reference; the app does not crash |
| Zero-size surface | skip draw, do not divide by zero |
| Skin change | surfaces that read theme colours redraw |
| Non-UI thread invalidate | marshalled to the dispatcher |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| Zoom in/out/fit/reset | view only |
| Export | `data.export` |
| Change a published setting | persisted per user |

## 7. Acceptance criteria

- AC-1 A `ViewSurface` renders visible pixels.
- AC-2 Mouse press/move/release reach the surface with correct coordinates.
- AC-3 Wheel delta reaches the surface.
- AC-4 Every documented key maps to a `ViewKey`.
- AC-5 Pinch and two-finger pan reach the surface.
- AC-6 `Invalidated` causes exactly one redraw per event, on the UI thread.
- AC-7 An exception in `Draw` is reported and contained.
- AC-8 The control is keyboard focusable and exposes an accessibility name.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~VSB-01"
```

Plus a manual probe: host `WellboreView` with the tools' demo document and exercise zoom, pan,
cursor, marker add and export.

## 9. Open questions and risks

- **Blocked on ADR-002.** If `SkiaSharp.Views.WPF` is unavailable at the unified version, implement
  `HwndHost` + `SKSurface` manually; the contract above does not change.
- Thread affinity: create the surface on the UI thread only.
