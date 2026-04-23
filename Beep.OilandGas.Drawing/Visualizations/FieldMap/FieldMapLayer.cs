using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Interaction;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Primitives;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Visualizations.FieldMap
{
    /// <summary>
    /// Renders a plan-view field map with fields, pools, leases, wells, facilities, and surface networks.
    /// </summary>
    public sealed class FieldMapLayer : LayerBase, IInteractiveLayer
    {
        private readonly FieldMapData mapData;
        private readonly FieldMapConfiguration configuration;
        private readonly LegendRenderer legendRenderer = new LegendRenderer();

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldMapLayer"/> class.
        /// </summary>
        public FieldMapLayer(FieldMapData mapData, FieldMapConfiguration configuration = null)
            : base("Field Map")
        {
            this.mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            this.configuration = configuration ?? new FieldMapConfiguration();
        }

        /// <inheritdoc />
        protected override void RenderContent(SKCanvas canvas, Viewport viewport)
        {
            canvas.DrawColor(configuration.BackgroundColor);

            canvas.Save();
            canvas.SetMatrix(viewport.GetMatrix());

            try
            {
                if (configuration.ShowAreas)
                {
                    DrawAreaAssets(canvas);
                }

                if (configuration.ShowConnections)
                {
                    DrawConnectionAssets(canvas);
                }

                if (configuration.ShowPoints)
                {
                    DrawPointAssets(canvas);
                }
            }
            finally
            {
                canvas.Restore();
            }

            if (configuration.ShowLegend)
            {
                DrawLegend(canvas);
            }
        }

        /// <inheritdoc />
        public override SKRect GetBounds()
        {
            var points = EnumerateAllPoints().ToList();
            if (points.Count == 0)
                return SKRect.Empty;

            float minX = points.Min(point => point.X);
            float maxX = points.Max(point => point.X);
            float minY = points.Min(point => point.Y);
            float maxY = points.Max(point => point.Y);
            return new SKRect(minX, minY, maxX, maxY);
        }

        /// <inheritdoc />
        public LayerHitResult HitTest(SKPoint worldPoint, float worldTolerance)
        {
            if (configuration.ShowPoints)
            {
                var pointHit = HitTestPointAssets(worldPoint, worldTolerance);
                if (pointHit != null)
                    return pointHit;
            }

            if (configuration.ShowConnections)
            {
                var connectionHit = HitTestConnectionAssets(worldPoint, worldTolerance);
                if (connectionHit != null)
                    return connectionHit;
            }

            if (configuration.ShowAreas)
            {
                var areaHit = HitTestAreaAssets(worldPoint, worldTolerance);
                if (areaHit != null)
                    return areaHit;
            }

            return null;
        }

        private void DrawAreaAssets(SKCanvas canvas)
        {
            using var labelHaloPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = configuration.LabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f
            };

            using var labelPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = configuration.LabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Fill
            };

            foreach (var asset in mapData.AreaAssets.Where(asset => asset?.BoundaryPoints?.Count >= 3))
            {
                var polygon = GetValidPoints(asset.BoundaryPoints).ToList();
                if (polygon.Count < 3)
                    continue;

                var fillColor = ResolveAreaFillColor(asset);
                var strokeColor = ResolveAreaStrokeColor(asset);
                using var fillPaint = new SKPaint
                {
                    Color = fillColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };

                using var strokePaint = new SKPaint
                {
                    Color = strokeColor,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = configuration.AreaStrokeWidth,
                    IsAntialias = true
                };

                using var path = new SKPath();
                path.MoveTo(polygon[0]);
                for (int index = 1; index < polygon.Count; index++)
                {
                    path.LineTo(polygon[index]);
                }
                path.Close();

                canvas.DrawPath(path, fillPaint);
                canvas.DrawPath(path, strokePaint);

                if (configuration.ShowAreaLabels)
                {
                    var centroid = new SKPoint(polygon.Average(point => point.X), polygon.Average(point => point.Y));
                    var label = string.IsNullOrWhiteSpace(asset.AssetName) ? asset.AssetId : asset.AssetName;
                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        canvas.DrawText(label, centroid.X, centroid.Y, labelHaloPaint);
                        canvas.DrawText(label, centroid.X, centroid.Y, labelPaint);
                    }
                }
            }
        }

        private void DrawConnectionAssets(SKCanvas canvas)
        {
            using var labelHaloPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = configuration.LabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f
            };

            using var labelPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = configuration.LabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Fill
            };

            foreach (var connection in mapData.ConnectionAssets.Where(asset => asset?.Vertices?.Count >= 2))
            {
                var points = GetValidPoints(connection.Vertices).ToList();
                if (points.Count < 2)
                    continue;

                var color = ResolveConnectionColor(connection);
                using var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = configuration.ConnectionStrokeWidth,
                    IsAntialias = true,
                    PathEffect = CreateConnectionPathEffect(connection.ConnectionKind)
                };

                using var path = new SKPath();
                path.MoveTo(points[0]);
                for (int index = 1; index < points.Count; index++)
                {
                    path.LineTo(points[index]);
                }

                canvas.DrawPath(path, paint);
                DrawArrowHead(canvas, points[^2], points[^1], color);

                if (configuration.ShowConnectionLabels)
                {
                    var label = string.IsNullOrWhiteSpace(connection.ConnectionName) ? connection.ConnectionId : connection.ConnectionName;
                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        var labelPoint = points[points.Count / 2];
                        canvas.DrawText(label, labelPoint.X, labelPoint.Y - 4f, labelHaloPaint);
                        canvas.DrawText(label, labelPoint.X, labelPoint.Y - 4f, labelPaint);
                    }
                }
            }
        }

        private void DrawPointAssets(SKCanvas canvas)
        {
            using var labelHaloPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = configuration.LabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f
            };

            using var labelPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = configuration.LabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Style = SKPaintStyle.Fill
            };

            foreach (var asset in mapData.PointAssets.Where(asset => asset != null))
            {
                var pathPoints = new List<SKPoint>();
                if (asset.Location != null)
                {
                    pathPoints.AddRange(GetValidPoints(new[] { asset.Location }));
                }
                pathPoints.AddRange(GetValidPoints(asset.PathPoints));

                if (pathPoints.Count == 0)
                    continue;

                var color = ResolvePointColor(asset);
                bool drawMarker = ShouldDrawMarker(asset.AssetKind);
                var labelAnchor = drawMarker ? pathPoints[0] : pathPoints[pathPoints.Count / 2];
                if (configuration.ShowPaths && pathPoints.Count > 1)
                {
                    using var pathPaint = new SKPaint
                    {
                        Color = color.WithAlpha(190),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = configuration.PathStrokeWidth,
                        IsAntialias = true,
                        PathEffect = CreatePathEffect(asset.AssetKind)
                    };

                    using var path = new SKPath();
                    path.MoveTo(pathPoints[0]);
                    for (int index = 1; index < pathPoints.Count; index++)
                    {
                        path.LineTo(pathPoints[index]);
                    }

                    canvas.DrawPath(path, pathPaint);
                }

                if (drawMarker)
                {
                    var anchor = pathPoints[0];
                    using var fillPaint = new SKPaint
                    {
                        Color = color,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    };

                    using var outlinePaint = new SKPaint
                    {
                        Color = SKColors.White,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1.4f,
                        IsAntialias = true
                    };

                    DrawMarker(canvas, asset.AssetKind, anchor, fillPaint, outlinePaint);
                }

                if (configuration.ShowPointLabels)
                {
                    var label = string.IsNullOrWhiteSpace(asset.AssetName) ? asset.AssetId : asset.AssetName;
                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        float labelX = drawMarker ? labelAnchor.X + configuration.MarkerRadius + 4f : labelAnchor.X + 4f;
                        float labelY = drawMarker ? labelAnchor.Y - configuration.MarkerRadius : labelAnchor.Y - 4f;
                        canvas.DrawText(label, labelX, labelY, labelHaloPaint);
                        canvas.DrawText(label, labelX, labelY, labelPaint);
                    }
                }
            }
        }

        private void DrawLegend(SKCanvas canvas)
        {
            var items = BuildLegendItems();
            if (items.Count == 0)
                return;

            legendRenderer.Render(
                canvas,
                items,
                options: new LegendOptions { Anchor = configuration.LegendAnchor },
                title: configuration.LegendTitle);
        }

        private IReadOnlyList<LegendItem> BuildLegendItems()
        {
            var items = new List<LegendItem>();

            foreach (var kind in mapData.AreaAssets.Select(asset => asset.AssetKind).Distinct())
            {
                items.Add(new LegendItem
                {
                    Label = kind.ToString(),
                    SymbolKind = LegendSymbolKind.Fill,
                    FillColor = ResolveDefaultFillColor(kind).WithAlpha(configuration.AreaFillAlpha),
                    StrokeColor = ResolveDefaultStrokeColor(kind),
                    StrokeWidth = configuration.AreaStrokeWidth
                });
            }

            foreach (var kind in mapData.PointAssets.Select(asset => asset.AssetKind).Distinct())
            {
                if (items.Any(item => string.Equals(item.Label, kind.ToString(), StringComparison.OrdinalIgnoreCase)))
                    continue;

                items.Add(new LegendItem
                {
                    Label = kind.ToString(),
                    SymbolKind = UsesLineSymbol(kind) ? LegendSymbolKind.Line : LegendSymbolKind.Marker,
                    FillColor = ResolveDefaultPointColor(kind),
                    StrokeColor = UsesLineSymbol(kind) ? ResolveDefaultPointColor(kind) : SKColors.White,
                    StrokeWidth = UsesLineSymbol(kind) ? configuration.PathStrokeWidth : 1.2f
                });
            }

            foreach (var kind in mapData.ConnectionAssets.Select(asset => asset.ConnectionKind).Distinct())
            {
                items.Add(new LegendItem
                {
                    Label = kind.ToString(),
                    SymbolKind = LegendSymbolKind.Line,
                    FillColor = ResolveDefaultConnectionColor(kind),
                    StrokeColor = ResolveDefaultConnectionColor(kind),
                    StrokeWidth = configuration.ConnectionStrokeWidth
                });
            }

            return items;
        }

        private LayerHitResult HitTestPointAssets(SKPoint worldPoint, float worldTolerance)
        {
            foreach (var asset in mapData.PointAssets.Where(asset => asset != null).Reverse())
            {
                var pathPoints = new List<SKPoint>();
                if (asset.Location != null)
                    pathPoints.AddRange(GetValidPoints(new[] { asset.Location }));

                pathPoints.AddRange(GetValidPoints(asset.PathPoints));
                if (pathPoints.Count == 0)
                    continue;

                bool drawMarker = ShouldDrawMarker(asset.AssetKind);
                if (drawMarker)
                {
                    var anchor = pathPoints[0];
                    float markerDistance = SKPoint.Distance(worldPoint, anchor);
                    if (markerDistance <= configuration.MarkerRadius + worldTolerance)
                    {
                        return CreateHitResult(
                            asset.AssetId,
                            string.IsNullOrWhiteSpace(asset.AssetName) ? asset.AssetId : asset.AssetName,
                            asset.AssetKind.ToString(),
                            anchor,
                            markerDistance,
                            asset.Metadata);
                    }
                }

                if (configuration.ShowPaths && pathPoints.Count > 1)
                {
                    float pathDistance = HitTestGeometry.DistanceToPolyline(worldPoint, pathPoints);
                    if (pathDistance <= worldTolerance + (configuration.PathStrokeWidth * 0.5f))
                    {
                        return CreateHitResult(
                            asset.AssetId,
                            string.IsNullOrWhiteSpace(asset.AssetName) ? asset.AssetId : asset.AssetName,
                            asset.AssetKind.ToString(),
                            HitTestGeometry.ComputeMidpoint(pathPoints),
                            pathDistance,
                            asset.Metadata);
                    }
                }
            }

            return null;
        }

        private LayerHitResult HitTestConnectionAssets(SKPoint worldPoint, float worldTolerance)
        {
            foreach (var connection in mapData.ConnectionAssets.Where(asset => asset?.Vertices?.Count >= 2).Reverse())
            {
                var points = GetValidPoints(connection.Vertices).ToList();
                if (points.Count < 2)
                    continue;

                float distance = HitTestGeometry.DistanceToPolyline(worldPoint, points);
                if (distance <= worldTolerance + (configuration.ConnectionStrokeWidth * 0.5f))
                {
                    return CreateHitResult(
                        connection.ConnectionId,
                        string.IsNullOrWhiteSpace(connection.ConnectionName) ? connection.ConnectionId : connection.ConnectionName,
                        connection.ConnectionKind.ToString(),
                        HitTestGeometry.ComputeMidpoint(points),
                        distance,
                        connection.Metadata);
                }
            }

            return null;
        }

        private LayerHitResult HitTestAreaAssets(SKPoint worldPoint, float worldTolerance)
        {
            foreach (var asset in mapData.AreaAssets.Where(asset => asset?.BoundaryPoints?.Count >= 3).Reverse())
            {
                var polygon = GetValidPoints(asset.BoundaryPoints).ToList();
                if (polygon.Count < 3)
                    continue;

                bool contains = HitTestGeometry.ContainsPolygon(worldPoint, polygon);
                float edgeDistance = HitTestGeometry.DistanceToPolygonEdge(worldPoint, polygon);
                if (contains || edgeDistance <= worldTolerance + (configuration.AreaStrokeWidth * 0.5f))
                {
                    return CreateHitResult(
                        asset.AssetId,
                        string.IsNullOrWhiteSpace(asset.AssetName) ? asset.AssetId : asset.AssetName,
                        asset.AssetKind.ToString(),
                        HitTestGeometry.ComputeCentroid(polygon),
                        contains ? 0f : edgeDistance,
                        asset.Metadata);
                }
            }

            return null;
        }

        private LayerHitResult CreateHitResult(
            string featureId,
            string featureLabel,
            string featureKind,
            SKPoint worldAnchor,
            float distance,
            IReadOnlyDictionary<string, string> metadata)
        {
            return new LayerHitResult(
                Name,
                featureId,
                featureLabel,
                featureKind,
                worldAnchor,
                distance,
                metadata != null ? new Dictionary<string, string>(metadata) : new Dictionary<string, string>());
        }

        private IEnumerable<SKPoint> EnumerateAllPoints()
        {
            foreach (var point in mapData.AreaAssets.SelectMany(asset => asset?.BoundaryPoints ?? Enumerable.Empty<Point3D>()))
            {
                if (TryToPoint(point, out var mapPoint))
                    yield return mapPoint;
            }

            foreach (var point in mapData.PointAssets.SelectMany(asset => EnumeratePointAssetGeometry(asset)))
            {
                if (TryToPoint(point, out var mapPoint))
                    yield return mapPoint;
            }

            foreach (var point in mapData.ConnectionAssets.SelectMany(asset => asset?.Vertices ?? Enumerable.Empty<Point3D>()))
            {
                if (TryToPoint(point, out var mapPoint))
                    yield return mapPoint;
            }
        }

        private static IEnumerable<Point3D> EnumeratePointAssetGeometry(FieldMapPointAsset asset)
        {
            if (asset?.Location != null)
                yield return asset.Location;

            if (asset?.PathPoints == null)
                yield break;

            foreach (var point in asset.PathPoints)
            {
                if (point != null)
                    yield return point;
            }
        }

        private static IEnumerable<SKPoint> GetValidPoints(IEnumerable<Point3D> points)
        {
            if (points == null)
                yield break;

            foreach (var point in points)
            {
                if (TryToPoint(point, out var mapPoint))
                    yield return mapPoint;
            }
        }

        private static bool TryToPoint(Point3D point, out SKPoint mapPoint)
        {
            mapPoint = default;
            if (point == null || !double.IsFinite(point.X) || !double.IsFinite(point.Y))
                return false;

            mapPoint = new SKPoint((float)point.X, (float)point.Y);
            return true;
        }

        private void DrawMarker(SKCanvas canvas, FieldMapAssetKind kind, SKPoint anchor, SKPaint fillPaint, SKPaint outlinePaint)
        {
            float radius = configuration.MarkerRadius;
            switch (kind)
            {
                case FieldMapAssetKind.Facility:
                    var rect = new SKRect(anchor.X - radius, anchor.Y - radius, anchor.X + radius, anchor.Y + radius);
                    canvas.DrawRect(rect, fillPaint);
                    canvas.DrawRect(rect, outlinePaint);
                    break;

                case FieldMapAssetKind.Well:
                    canvas.DrawCircle(anchor, radius, fillPaint);
                    canvas.DrawCircle(anchor, radius, outlinePaint);
                    break;

                default:
                    using (var markerPath = new SKPath())
                    {
                        markerPath.MoveTo(anchor.X, anchor.Y - radius);
                        markerPath.LineTo(anchor.X + radius, anchor.Y);
                        markerPath.LineTo(anchor.X, anchor.Y + radius);
                        markerPath.LineTo(anchor.X - radius, anchor.Y);
                        markerPath.Close();
                        canvas.DrawPath(markerPath, fillPaint);
                        canvas.DrawPath(markerPath, outlinePaint);
                    }
                    break;
            }
        }

        private SKColor ResolveAreaFillColor(FieldMapAreaAsset asset)
        {
            if (TryParseColor(asset.FillColorCode, out var color))
                return color.WithAlpha(configuration.AreaFillAlpha);

            return ResolveDefaultFillColor(asset.AssetKind).WithAlpha(configuration.AreaFillAlpha);
        }

        private SKColor ResolveAreaStrokeColor(FieldMapAreaAsset asset)
        {
            if (TryParseColor(asset.StrokeColorCode, out var color))
                return color;

            return ResolveDefaultStrokeColor(asset.AssetKind);
        }

        private SKColor ResolvePointColor(FieldMapPointAsset asset)
        {
            if (TryParseColor(asset.ColorCode, out var color))
                return color;

            return ResolveDefaultPointColor(asset.AssetKind);
        }

        private SKColor ResolveConnectionColor(FieldMapConnectionAsset connection)
        {
            if (TryParseColor(connection.ColorCode, out var color))
                return color;

            return ResolveDefaultConnectionColor(connection.ConnectionKind);
        }

        private static bool TryParseColor(string colorCode, out SKColor color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(colorCode) || !colorCode.StartsWith("#", StringComparison.Ordinal))
                return false;

            try
            {
                color = SKColor.Parse(colorCode);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static SKColor ResolveDefaultFillColor(FieldMapAssetKind kind)
        {
            return kind switch
            {
                FieldMapAssetKind.Field => new SKColor(102, 170, 102),
                FieldMapAssetKind.Pool => new SKColor(246, 170, 77),
                FieldMapAssetKind.Lease => new SKColor(218, 196, 140),
                FieldMapAssetKind.LandRight => new SKColor(121, 134, 203),
                FieldMapAssetKind.ProtectedArea => new SKColor(129, 199, 132),
                FieldMapAssetKind.HazardZone => new SKColor(239, 154, 154),
                FieldMapAssetKind.Seismic3D => new SKColor(100, 181, 246),
                _ => new SKColor(176, 190, 197)
            };
        }

        private static SKColor ResolveDefaultStrokeColor(FieldMapAssetKind kind)
        {
            return kind switch
            {
                FieldMapAssetKind.Field => new SKColor(46, 125, 50),
                FieldMapAssetKind.Pool => new SKColor(239, 108, 0),
                FieldMapAssetKind.Lease => new SKColor(121, 85, 72),
                FieldMapAssetKind.LandRight => new SKColor(57, 73, 171),
                FieldMapAssetKind.ProtectedArea => new SKColor(56, 142, 60),
                FieldMapAssetKind.HazardZone => new SKColor(198, 40, 40),
                FieldMapAssetKind.Seismic3D => new SKColor(25, 118, 210),
                _ => new SKColor(84, 110, 122)
            };
        }

        private static SKColor ResolveDefaultPointColor(FieldMapAssetKind kind)
        {
            return kind switch
            {
                FieldMapAssetKind.Facility => new SKColor(30, 136, 229),
                FieldMapAssetKind.Well => new SKColor(38, 50, 56),
                FieldMapAssetKind.Seismic2D => new SKColor(94, 53, 177),
                FieldMapAssetKind.Pipeline => new SKColor(123, 31, 162),
                FieldMapAssetKind.SurfaceSystem => new SKColor(0, 121, 107),
                _ => new SKColor(96, 125, 139)
            };
        }

        private static SKColor ResolveDefaultConnectionColor(FieldMapConnectionKind kind)
        {
            return kind switch
            {
                FieldMapConnectionKind.Flowline => new SKColor(0, 121, 107),
                FieldMapConnectionKind.GatheringLine => new SKColor(25, 118, 210),
                FieldMapConnectionKind.ExportLine => new SKColor(230, 81, 0),
                FieldMapConnectionKind.Utility => new SKColor(123, 31, 162),
                FieldMapConnectionKind.FacilityLink => new SKColor(96, 125, 139),
                _ => new SKColor(84, 110, 122)
            };
        }

        private static bool ShouldDrawMarker(FieldMapAssetKind kind)
        {
            return !UsesLineSymbol(kind);
        }

        private static bool UsesLineSymbol(FieldMapAssetKind kind)
        {
            return kind == FieldMapAssetKind.Pipeline || kind == FieldMapAssetKind.Seismic2D;
        }

        private static SKPathEffect CreatePathEffect(FieldMapAssetKind kind)
        {
            return kind switch
            {
                FieldMapAssetKind.Seismic2D => SKPathEffect.CreateDash(new float[] { 5f, 4f }, 0),
                _ => null
            };
        }

        private static SKPathEffect CreateConnectionPathEffect(FieldMapConnectionKind kind)
        {
            return kind switch
            {
                FieldMapConnectionKind.Utility => SKPathEffect.CreateDash(new float[] { 4f, 4f }, 0),
                FieldMapConnectionKind.FacilityLink => SKPathEffect.CreateDash(new float[] { 10f, 5f }, 0),
                _ => null
            };
        }

        private static void DrawArrowHead(SKCanvas canvas, SKPoint from, SKPoint to, SKColor color)
        {
            var direction = new SKPoint(to.X - from.X, to.Y - from.Y);
            var length = (float)Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
            if (length <= 0.01f)
                return;

            var unit = new SKPoint(direction.X / length, direction.Y / length);
            var perp = new SKPoint(-unit.Y, unit.X);
            const float arrowLength = 8f;
            const float arrowWidth = 4f;

            var tip = to;
            var basePoint = new SKPoint(tip.X - (unit.X * arrowLength), tip.Y - (unit.Y * arrowLength));
            var left = new SKPoint(basePoint.X + (perp.X * arrowWidth), basePoint.Y + (perp.Y * arrowWidth));
            var right = new SKPoint(basePoint.X - (perp.X * arrowWidth), basePoint.Y - (perp.Y * arrowWidth));

            using var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var path = new SKPath();
            path.MoveTo(tip);
            path.LineTo(left);
            path.LineTo(right);
            path.Close();
            canvas.DrawPath(path, paint);
        }
    }
}