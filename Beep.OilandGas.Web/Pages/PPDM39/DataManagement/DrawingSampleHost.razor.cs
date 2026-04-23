using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.Export;
using Beep.OilandGas.Drawing.Interaction;
using Beep.OilandGas.Drawing.Measurements;
using Beep.OilandGas.Drawing.Samples;
using Beep.OilandGas.Drawing.Scenes;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using SkiaSharp;

namespace Beep.OilandGas.Web.Pages.PPDM39.DataManagement
{
    public partial class DrawingSampleHost : IDisposable
    {
        private readonly IReadOnlyList<SampleInteractionMode> _interactionModes = Enum.GetValues<SampleInteractionMode>();
        private readonly IReadOnlyList<AnnotationStylePreset> _annotationStylePresets = Enum.GetValues<AnnotationStylePreset>();
        private IReadOnlyList<DrawingSampleScene> _sampleScenes = Array.Empty<DrawingSampleScene>();
        private DrawingEngine? _engine;
        private string _selectedSampleName = string.Empty;
        private string _renderedImageDataUrl = string.Empty;
        private SampleInteractionMode _mode = SampleInteractionMode.Inspect;
        private AnnotationStylePreset _annotationStylePreset = AnnotationStylePreset.Auto;
        private bool _isExporting;
        private bool _isPointerDown;
        private bool _isPanning;
        private bool _suppressCanvasClick;
        private SKPoint _pointerDownPoint;
        private SKPoint _lastPointerPoint;
        private Severity _statusSeverity = Severity.Info;
        private string _statusMessage = "Select a sample and click the canvas to inspect a feature or create a measurement.";
        private readonly List<SKPoint> _draftScreenPoints = new();

        [Inject] private IClientFileExportService ClientFileExportService { get; set; } = default!;

        private DrawingSampleScene? CurrentSample => _sampleScenes.FirstOrDefault(scene => string.Equals(scene.Name, _selectedSampleName, StringComparison.OrdinalIgnoreCase));
        private IReadOnlyList<DrawingSampleExportAction> CurrentSampleSupplementalExports => CurrentSample?.SupplementalExports ?? Array.Empty<DrawingSampleExportAction>();
        private bool CanInteract => _engine?.ActiveScene != null;
        private bool CanCompleteAreaMeasurement => CanInteract && _mode == SampleInteractionMode.Area && _draftScreenPoints.Count >= 3;
        private bool CanExport => _engine != null && !_isExporting;
        private bool IsCanvasPanning => _isPanning;
        private SceneSelectionAnnotation? CurrentSelection => _engine?.ActiveScene?.InteractionState.Selections.LastOrDefault();
        private IReadOnlyList<SceneMeasurementAnnotation> RecordedMeasurements => _engine?.ActiveScene?.InteractionState.Measurements ?? Array.Empty<SceneMeasurementAnnotation>();

        private string CurrentSceneKindLabel => _engine?.ActiveScene?.Kind.ToString() ?? "Render Only";
        private string CurrentSceneReferenceLabel => _engine?.ActiveScene?.CoordinateReferenceSystem?.Identifier ?? "No scene CRS";
        private string CurrentCanvasSizeLabel => _engine is null ? "Canvas unavailable" : $"{_engine.Width} x {_engine.Height}";
        private string CurrentViewportPanLabel => _engine is null ? "Pan 0.0, 0.0" : $"Pan {_engine.Viewport.PanX:0.0}, {_engine.Viewport.PanY:0.0}";
        private AnnotationStylePreset ResolvedAnnotationStylePreset => ResolveEffectiveAnnotationStylePreset();
        private string CurrentAnnotationStylePresetLabel => $"Annotations: {GetAnnotationStylePresetLabel(ResolvedAnnotationStylePreset)}";
        private string CurrentAnnotationStyleSummary => _annotationStylePreset == AnnotationStylePreset.Auto
            ? $"Auto follows the active scene and is currently using the {GetAnnotationStylePresetLabel(ResolvedAnnotationStylePreset)} preset."
            : $"{GetAnnotationStylePresetLabel(_annotationStylePreset)} adjusts the scene-owned persisted annotation rendering without changing the draft overlay.";
        private string CanvasInstruction => _mode switch
        {
            SampleInteractionMode.Inspect when CanInteract => "Inspect mode: click a visible feature to record a typed selection.",
            SampleInteractionMode.Distance when CanInteract => _draftScreenPoints.Count == 0
                ? "Distance mode: click the first point. The second click records a persisted distance annotation."
                : "Distance mode: click the second point to record the current measurement.",
            SampleInteractionMode.Area when CanInteract => _draftScreenPoints.Count < 3
                ? "Area mode: click at least three vertices, then use Complete Area to persist the polygon measurement."
                : "Area mode: add more vertices or use Complete Area to persist the current polygon measurement.",
            _ => "This sample currently renders without typed interaction metadata. Switch to Field Map, Reservoir Contour, or Reservoir Cross Section for interaction." 
        };

        private MeasurementOverlayModel? DraftOverlay => BuildDraftOverlay();

        protected override void OnInitialized()
        {
            _sampleScenes = DrawingSampleGallery.GetStandardScenes();
            if (_sampleScenes.Count > 0)
                SelectSample(_sampleScenes[0].Name);
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }

        private void NavigateBackToHub()
        {
            NavigationManager.NavigateTo("/ppdm39/data-management");
        }

        private void SelectSample(string sampleName)
        {
            if (string.IsNullOrWhiteSpace(sampleName))
                return;

            _selectedSampleName = sampleName;
            _mode = SampleInteractionMode.Inspect;
            _draftScreenPoints.Clear();

            var engine = DrawingSampleGallery.CreateStandardScene(sampleName);
            _engine?.Dispose();
            _engine = engine;
            ApplyAnnotationStylePreset();
            RenderCurrentImage();

            if (_engine.ActiveScene == null)
            {
                SetStatus("Loaded render-only sample. Interaction is available on samples that attach typed scenes.", Severity.Warning);
            }
            else
            {
                SetStatus($"Loaded {FormatSceneName(sampleName)}. Inspect, distance, and area workflows are available for this sample.", Severity.Success);
            }
        }

        private void SetMode(SampleInteractionMode mode)
        {
            _mode = mode;
            _draftScreenPoints.Clear();

            if (!CanInteract)
            {
                SetStatus("This sample does not yet expose typed scene metadata for interaction.", Severity.Warning);
                return;
            }

            SetStatus(CanvasInstruction, Severity.Info);
        }

        private void SetAnnotationStylePreset(AnnotationStylePreset preset)
        {
            _annotationStylePreset = preset;

            if (_engine?.ActiveScene == null)
            {
                SetStatus($"Selected the {GetAnnotationStylePresetLabel(preset)} annotation preset. It will apply when a typed scene is active.", Severity.Info);
                return;
            }

            ApplyAnnotationStylePreset();
            RenderCurrentImage();
            SetStatus($"Applied the {GetAnnotationStylePresetLabel(ResolvedAnnotationStylePreset)} annotation preset.", Severity.Info);
        }

        private void HandleCanvasClick(MouseEventArgs args)
        {
            if (_engine == null)
                return;

            if (_suppressCanvasClick)
            {
                _suppressCanvasClick = false;
                return;
            }

            var screenPoint = new SKPoint((float)args.OffsetX, (float)args.OffsetY);
            if (screenPoint.X < 0 || screenPoint.Y < 0 || screenPoint.X > _engine.Width || screenPoint.Y > _engine.Height)
                return;

            if (!CanInteract)
            {
                SetStatus("This sample renders correctly, but interaction requires a typed scene contract.", Severity.Warning);
                return;
            }

            switch (_mode)
            {
                case SampleInteractionMode.Inspect:
                    HandleInspectClick(screenPoint);
                    break;

                case SampleInteractionMode.Distance:
                    HandleDistanceClick(screenPoint);
                    break;

                case SampleInteractionMode.Area:
                    HandleAreaClick(screenPoint);
                    break;
            }
        }

        private void HandleCanvasPointerDown(MouseEventArgs args)
        {
            if (_engine == null || args.Button != 0)
                return;

            _isPointerDown = true;
            _isPanning = false;
            _suppressCanvasClick = false;
            _pointerDownPoint = new SKPoint((float)args.OffsetX, (float)args.OffsetY);
            _lastPointerPoint = _pointerDownPoint;
        }

        private void HandleCanvasPointerMove(MouseEventArgs args)
        {
            if (_engine == null || !_isPointerDown)
                return;

            var currentPoint = new SKPoint((float)args.OffsetX, (float)args.OffsetY);
            var deltaFromOrigin = new SKPoint(currentPoint.X - _pointerDownPoint.X, currentPoint.Y - _pointerDownPoint.Y);
            if (!_isPanning && Math.Abs(deltaFromOrigin.X) < 4f && Math.Abs(deltaFromOrigin.Y) < 4f)
                return;

            _isPanning = true;

            var delta = new SKPoint(currentPoint.X - _lastPointerPoint.X, currentPoint.Y - _lastPointerPoint.Y);
            if (delta.X == 0f && delta.Y == 0f)
                return;

            _engine.Pan(delta.X, delta.Y);
            _lastPointerPoint = currentPoint;
            RenderCurrentImage();
            SetStatus($"Panning viewport. {CurrentViewportPanLabel}.", Severity.Info);
        }

        private void HandleCanvasPointerUp()
        {
            if (!_isPointerDown)
                return;

            _isPointerDown = false;
            _suppressCanvasClick = _isPanning;

            if (_isPanning)
            {
                _isPanning = false;
                SetStatus($"Viewport updated. {CurrentViewportPanLabel}.", Severity.Success);
            }
        }

        private void HandleCanvasPointerLeave()
        {
            if (!_isPointerDown)
                return;

            _isPointerDown = false;
            _isPanning = false;
            _suppressCanvasClick = false;
        }

        private void HandleInspectClick(SKPoint screenPoint)
        {
            var hit = _engine!.HitTest(screenPoint, screenTolerance: 8f);
            if (hit == null)
            {
                _engine.ActiveScene!.InteractionState.ClearSelections();
                RenderCurrentImage();
                SetStatus("No interactive feature was resolved at that point.", Severity.Info);
                return;
            }

            _engine.RecordSelection(hit, replaceExisting: true);
            RenderCurrentImage();
            SetStatus($"Selected {hit.FeatureKind}: {hit.FeatureLabel}.", Severity.Success);
        }

        private void HandleDistanceClick(SKPoint screenPoint)
        {
            _draftScreenPoints.Add(screenPoint);
            if (_draftScreenPoints.Count < 2)
            {
                SetStatus("Distance mode: click the second point to complete the measurement.", Severity.Info);
                return;
            }

            var measurement = _engine!.MeasureDistance(_draftScreenPoints[0], _draftScreenPoints[1]);
            var label = $"Distance {RecordedMeasurements.Count + 1}";
            _engine.RecordMeasurement(measurement, _draftScreenPoints.ToArray(), label);
            _draftScreenPoints.Clear();
            RenderCurrentImage();
            SetStatus($"Recorded {label}: {measurement.DisplayText}.", Severity.Success);
        }

        private void HandleAreaClick(SKPoint screenPoint)
        {
            _draftScreenPoints.Add(screenPoint);
            SetStatus($"Area draft now has {_draftScreenPoints.Count} point(s). Use Complete Area when ready.", Severity.Info);
        }

        private void CompleteAreaMeasurement()
        {
            if (!CanCompleteAreaMeasurement || _engine == null)
                return;

            var measurement = _engine.MeasureArea(_draftScreenPoints.ToArray());
            var label = $"Area {RecordedMeasurements.Count + 1}";
            _engine.RecordMeasurement(measurement, _draftScreenPoints.ToArray(), label);
            _draftScreenPoints.Clear();
            RenderCurrentImage();
            SetStatus($"Recorded {label}: {measurement.DisplayText}.", Severity.Success);
        }

        private void ClearDraft()
        {
            _draftScreenPoints.Clear();
            SetStatus("Cleared the in-progress interaction draft.", Severity.Info);
        }

        private void ClearSelections()
        {
            if (_engine?.ActiveScene == null)
                return;

            _engine.ActiveScene.InteractionState.ClearSelections();
            RenderCurrentImage();
            SetStatus("Cleared persisted selections for the current scene.", Severity.Info);
        }

        private void RemoveCurrentSelection()
        {
            if (_engine?.ActiveScene == null || CurrentSelection == null)
                return;

            if (_engine.ActiveScene.InteractionState.RemoveSelection(CurrentSelection.AnnotationId))
            {
                RenderCurrentImage();
                SetStatus("Removed the current persisted selection.", Severity.Info);
            }
        }

        private void ClearMeasurements()
        {
            if (_engine?.ActiveScene == null)
                return;

            _engine.ActiveScene.InteractionState.ClearMeasurements();
            _draftScreenPoints.Clear();
            RenderCurrentImage();
            SetStatus("Cleared persisted measurements for the current scene.", Severity.Info);
        }

        private void RemoveMeasurement(string annotationId)
        {
            if (_engine?.ActiveScene == null)
                return;

            if (_engine.ActiveScene.InteractionState.RemoveMeasurement(annotationId))
            {
                RenderCurrentImage();
                SetStatus("Removed a persisted measurement.", Severity.Info);
            }
        }

        private void ZoomIn()
        {
            if (_engine == null)
                return;

            _engine.SetZoom(_engine.Viewport.Zoom * 1.2f);
            RenderCurrentImage();
            SetStatus($"Zoom updated to {_engine.Viewport.Zoom:0.00}x.", Severity.Info);
        }

        private void ZoomOut()
        {
            if (_engine == null)
                return;

            _engine.SetZoom(_engine.Viewport.Zoom / 1.2f);
            RenderCurrentImage();
            SetStatus($"Zoom updated to {_engine.Viewport.Zoom:0.00}x.", Severity.Info);
        }

        private void ResetView()
        {
            if (_engine == null)
                return;

            if (_engine.ActiveScene != null)
            {
                _engine.UseScene(_engine.ActiveScene);
                RenderCurrentImage();
                SetStatus("Reset the viewport to the active scene defaults.", Severity.Info);
                return;
            }

            _engine.ResetViewport();
            RenderCurrentImage();
            SetStatus("Reset the viewport to the default canvas origin because no typed scene is attached.", Severity.Info);
        }

        private Task ExportPngAsync()
        {
            return ExportCurrentSceneAsync(
                extension: "png",
                contentType: "image/png",
                export: engine =>
                {
                    using var image = engine.RenderToImage();
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    return data.ToArray();
                });
        }

        private Task ExportSvgAsync()
        {
            return ExportCurrentSceneAsync(
                extension: "svg",
                contentType: "image/svg+xml",
                export: engine => Encoding.UTF8.GetBytes(SvgExporter.ExportToString(engine)));
        }

        private Task ExportPdfAsync()
        {
            return ExportCurrentSceneAsync(
                extension: "pdf",
                contentType: "application/pdf",
                export: engine =>
                {
                    var filePath = Path.Combine(Path.GetTempPath(), $"beep-drawing-export-{Guid.NewGuid():N}.pdf");
                    try
                    {
                        PdfExporter.Export(engine, filePath);
                        return File.ReadAllBytes(filePath);
                    }
                    finally
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                });
        }

        private async Task ExportSupplementalArtifactAsync(DrawingSampleExportAction exportAction)
        {
            if (exportAction == null || _engine == null || _isExporting)
                return;

            var fileName = BuildArtifactFileName(exportAction.FileNameSuffix);
            _isExporting = true;
            SetStatus($"Preparing {exportAction.Label} for {FormatSceneName(_selectedSampleName)}.", Severity.Info);

            try
            {
                await ClientFileExportService.DownloadBytesAsync(fileName, exportAction.Export(_engine), exportAction.ContentType);
                SetStatus($"Exported {fileName} from the current drawing workflow.", Severity.Success);
            }
            catch (Exception ex)
            {
                SetStatus($"Failed to export {exportAction.Label}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isExporting = false;
            }
        }

        private void RenderCurrentImage()
        {
            if (_engine == null)
            {
                _renderedImageDataUrl = string.Empty;
                return;
            }

            using var image = _engine.RenderToImage();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            _renderedImageDataUrl = "data:image/png;base64," + Convert.ToBase64String(data.ToArray());
        }

        private async Task ExportCurrentSceneAsync(string extension, string contentType, Func<DrawingEngine, byte[]> export)
        {
            if (_engine == null || _isExporting)
                return;

            var fileName = BuildExportFileName(extension);
            _isExporting = true;
            SetStatus($"Preparing {extension.ToUpperInvariant()} export for {FormatSceneName(_selectedSampleName)}.", Severity.Info);

            try
            {
                var bytes = export(_engine);
                await ClientFileExportService.DownloadBytesAsync(fileName, bytes, contentType);
                SetStatus($"Exported {fileName} from the current drawing workflow.", Severity.Success);
            }
            catch (Exception ex)
            {
                SetStatus($"Failed to export {extension.ToUpperInvariant()}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isExporting = false;
            }
        }

        private string BuildExportFileName(string extension)
        {
            var sampleName = string.IsNullOrWhiteSpace(_selectedSampleName)
                ? "drawing-scene"
                : _selectedSampleName.Replace(' ', '_');

            return $"{sampleName}-{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
        }

        private string BuildArtifactFileName(string suffix)
        {
            var sampleName = string.IsNullOrWhiteSpace(_selectedSampleName)
                ? "drawing-scene"
                : _selectedSampleName.Replace(' ', '_');

            return $"{sampleName}-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";
        }

        private string GetSupplementalExportIcon(DrawingSampleExportAction exportAction)
        {
            if (exportAction == null)
                return Icons.Material.Filled.Attachment;

            if (string.Equals(exportAction.ContentType, "application/zip", StringComparison.OrdinalIgnoreCase))
                return Icons.Material.Filled.Public;

            if (string.Equals(exportAction.ContentType, "application/geo+json", StringComparison.OrdinalIgnoreCase))
                return Icons.Material.Filled.Map;

            return Icons.Material.Filled.Attachment;
        }

        private Color GetSupplementalExportColor(DrawingSampleExportAction exportAction)
        {
            if (exportAction == null)
                return Color.Secondary;

            if (string.Equals(exportAction.ContentType, "application/zip", StringComparison.OrdinalIgnoreCase))
                return Color.Primary;

            if (string.Equals(exportAction.ContentType, "application/geo+json", StringComparison.OrdinalIgnoreCase))
                return Color.Secondary;

            return Color.Default;
        }

        private void SetStatus(string message, Severity severity)
        {
            _statusMessage = message;
            _statusSeverity = severity;
        }

        private void ApplyAnnotationStylePreset()
        {
            if (_engine?.ActiveScene == null)
                return;

            var style = _engine.ActiveScene.InteractionState.RenderStyle;
            CopyStyle(style, new SceneInteractionRenderStyle());

            switch (ResolveEffectiveAnnotationStylePreset())
            {
                case AnnotationStylePreset.Map:
                    style.SelectionColor = new SKColor(0, 121, 107);
                    style.MeasurementLineColor = new SKColor(2, 119, 189);
                    style.MeasurementAreaColor = new SKColor(239, 108, 0);
                    style.MeasurementAreaFillAlpha = 68;
                    style.SelectionDashPattern = new[] { 10f, 6f };
                    style.SelectionRingRadius = 13f;
                    style.LabelTextSize = 12f;
                    break;

                case AnnotationStylePreset.Log:
                    style.SelectionColor = new SKColor(13, 71, 161);
                    style.MeasurementLineColor = new SKColor(0, 96, 100);
                    style.MeasurementAreaColor = new SKColor(198, 40, 40);
                    style.SelectionDashPattern = new[] { 3f, 3f };
                    style.SelectionRingRadius = 9f;
                    style.SelectionCoreRadius = 4f;
                    style.MeasurementStrokeWidth = 2.5f;
                    style.MeasurementVertexRadius = 3.5f;
                    style.LabelTextSize = 11.5f;
                    style.SelectionLabelOffsetX = 10f;
                    style.SelectionLabelOffsetY = -10f;
                    style.MeasurementLabelOffsetX = 6f;
                    style.MeasurementLabelOffsetY = -6f;
                    break;

                case AnnotationStylePreset.Schematic:
                    style.SelectionColor = new SKColor(191, 54, 12);
                    style.MeasurementLineColor = new SKColor(0, 105, 92);
                    style.MeasurementAreaColor = new SKColor(255, 143, 0);
                    style.MeasurementAreaFillAlpha = 60;
                    style.SelectionDashPattern = new[] { 4f, 3f };
                    style.SelectionRingRadius = 12f;
                    style.MeasurementVertexRadius = 5f;
                    style.LabelTextSize = 12.5f;
                    style.SelectionLabelOffsetX = 18f;
                    style.MeasurementLabelOffsetX = 10f;
                    break;

                case AnnotationStylePreset.Hidden:
                    style.IsVisible = false;
                    break;
            }
        }

        private AnnotationStylePreset ResolveEffectiveAnnotationStylePreset()
        {
            if (_annotationStylePreset != AnnotationStylePreset.Auto)
                return _annotationStylePreset;

            if (_engine?.ActiveScene == null)
                return AnnotationStylePreset.Default;

            if (_selectedSampleName.Contains("WellLog", StringComparison.OrdinalIgnoreCase))
                return AnnotationStylePreset.Log;

            if (_selectedSampleName.Contains("WellSchematic", StringComparison.OrdinalIgnoreCase))
                return AnnotationStylePreset.Schematic;

            return _engine.ActiveScene.Kind == DrawingSceneKind.Map
                ? AnnotationStylePreset.Map
                : AnnotationStylePreset.Default;
        }

        private static void CopyStyle(SceneInteractionRenderStyle target, SceneInteractionRenderStyle source)
        {
            target.IsVisible = source.IsVisible;
            target.SelectionColor = source.SelectionColor;
            target.SelectionStrokeWidth = source.SelectionStrokeWidth;
            target.SelectionRingRadius = source.SelectionRingRadius;
            target.SelectionCoreRadius = source.SelectionCoreRadius;
            target.SelectionDashPattern = source.SelectionDashPattern?.ToArray() ?? Array.Empty<float>();
            target.ShowSelectionLabels = source.ShowSelectionLabels;
            target.SelectionLabelOffsetX = source.SelectionLabelOffsetX;
            target.SelectionLabelOffsetY = source.SelectionLabelOffsetY;
            target.MeasurementLineColor = source.MeasurementLineColor;
            target.MeasurementAreaColor = source.MeasurementAreaColor;
            target.MeasurementAreaFillAlpha = source.MeasurementAreaFillAlpha;
            target.MeasurementStrokeWidth = source.MeasurementStrokeWidth;
            target.MeasurementVertexRadius = source.MeasurementVertexRadius;
            target.MeasurementVertexHaloColor = source.MeasurementVertexHaloColor;
            target.MeasurementVertexHaloStrokeWidth = source.MeasurementVertexHaloStrokeWidth;
            target.ShowMeasurementLabels = source.ShowMeasurementLabels;
            target.MeasurementLabelOffsetX = source.MeasurementLabelOffsetX;
            target.MeasurementLabelOffsetY = source.MeasurementLabelOffsetY;
            target.LabelFillColor = source.LabelFillColor;
            target.LabelStrokeColor = source.LabelStrokeColor;
            target.LabelTextSize = source.LabelTextSize;
            target.LabelStrokeWidth = source.LabelStrokeWidth;
        }

        private Variant GetSampleVariant(string sampleName)
        {
            return string.Equals(sampleName, _selectedSampleName, StringComparison.OrdinalIgnoreCase) ? Variant.Filled : Variant.Outlined;
        }

        private Color GetSampleColor(string sampleName)
        {
            return string.Equals(sampleName, _selectedSampleName, StringComparison.OrdinalIgnoreCase) ? Color.Primary : Color.Default;
        }

        private Variant GetModeVariant(SampleInteractionMode mode)
        {
            return _mode == mode ? Variant.Filled : Variant.Outlined;
        }

        private Color GetModeColor(SampleInteractionMode mode)
        {
            return mode switch
            {
                SampleInteractionMode.Inspect => _mode == mode ? Color.Secondary : Color.Default,
                SampleInteractionMode.Distance => _mode == mode ? Color.Info : Color.Default,
                SampleInteractionMode.Area => _mode == mode ? Color.Warning : Color.Default,
                _ => Color.Default
            };
        }

        private static string GetAnnotationStylePresetLabel(AnnotationStylePreset preset)
        {
            return preset switch
            {
                AnnotationStylePreset.Auto => "Auto",
                AnnotationStylePreset.Default => "Default",
                AnnotationStylePreset.Map => "Map",
                AnnotationStylePreset.Log => "Log",
                AnnotationStylePreset.Schematic => "Schematic",
                AnnotationStylePreset.Hidden => "Hidden",
                _ => preset.ToString()
            };
        }

        private static Color GetAnnotationStylePresetColor(AnnotationStylePreset preset)
        {
            return preset switch
            {
                AnnotationStylePreset.Auto => Color.Info,
                AnnotationStylePreset.Default => Color.Default,
                AnnotationStylePreset.Map => Color.Success,
                AnnotationStylePreset.Log => Color.Primary,
                AnnotationStylePreset.Schematic => Color.Warning,
                AnnotationStylePreset.Hidden => Color.Dark,
                _ => Color.Default
            };
        }

        private static string GetModeLabel(SampleInteractionMode mode)
        {
            return mode switch
            {
                SampleInteractionMode.Inspect => "Inspect",
                SampleInteractionMode.Distance => "Measure Distance",
                SampleInteractionMode.Area => "Measure Area",
                _ => mode.ToString()
            };
        }

        private static string FormatSceneName(string sceneName)
        {
            return string.IsNullOrWhiteSpace(sceneName)
                ? string.Empty
                : sceneName.Replace('_', ' ');
        }

        private string GetCanvasStageClass()
        {
            return IsCanvasPanning
                ? "drawing-sample-host-stage drawing-sample-host-stage-panning"
                : "drawing-sample-host-stage";
        }

        private MeasurementOverlayModel? BuildDraftOverlay()
        {
            if (_engine == null || _draftScreenPoints.Count == 0)
                return null;

            var draftPoints = _draftScreenPoints
                .Select(point => new OverlayPoint(point.X, point.Y))
                .ToList();

            var labelAnchor = GetLabelAnchor(draftPoints, _mode == SampleInteractionMode.Area);
            return new MeasurementOverlayModel(
                Points: draftPoints,
                PointsAttribute: string.Join(' ', draftPoints.Select(point => point.ToSvgPoint())),
                LabelX: labelAnchor.X,
                LabelY: labelAnchor.Y,
                DisplayText: _mode == SampleInteractionMode.Area ? $"Area draft ({draftPoints.Count})" : $"Distance draft ({draftPoints.Count})",
                IsPolygon: _mode == SampleInteractionMode.Area && draftPoints.Count >= 3);
        }

        private static OverlayPoint GetLabelAnchor(IReadOnlyList<OverlayPoint> points, bool polygon)
        {
            if (points.Count == 0)
                return new OverlayPoint(0, 0);

            if (polygon)
            {
                return new OverlayPoint(points.Average(point => point.X), points.Average(point => point.Y));
            }

            return points[points.Count / 2];
        }

        private enum SampleInteractionMode
        {
            Inspect,
            Distance,
            Area
        }

        private enum AnnotationStylePreset
        {
            Auto,
            Default,
            Map,
            Log,
            Schematic,
            Hidden
        }

        private sealed record OverlayPoint(float X, float Y)
        {
            public string ToSvgPoint() => $"{X:0.###},{Y:0.###}";
        }

        private sealed record MeasurementOverlayModel(
            IReadOnlyList<OverlayPoint> Points,
            string PointsAttribute,
            float LabelX,
            float LabelY,
            string DisplayText,
            bool IsPolygon);
    }
}