using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.DataLoaders;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.Builders
{
    internal static class ReservoirVisualizationResolver
    {
        public static ReservoirData ResolveReservoirData(
            ref ReservoirData reservoirData,
            IReservoirLoader reservoirLoader,
            string dataSource,
            DataLoaderType? loaderType,
            string reservoirIdentifier,
            ReservoirLoadConfiguration loadConfiguration)
        {
            if (reservoirData != null)
                return reservoirData;

            var effectiveLoader = reservoirLoader;
            if (effectiveLoader == null && !string.IsNullOrWhiteSpace(dataSource) && loaderType.HasValue)
            {
                effectiveLoader = DataLoaderFactory.CreateReservoirLoader(dataSource, loaderType.Value);
            }

            if (effectiveLoader == null)
                return null;

            var identifier = reservoirIdentifier;
            if (string.IsNullOrWhiteSpace(identifier))
            {
                identifier = effectiveLoader.GetAvailableIdentifiers().FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(identifier))
                throw new InvalidOperationException("No reservoir identifier was provided and the loader did not return any available reservoirs.");

            var result = effectiveLoader.LoadReservoirWithResult(identifier, loadConfiguration);
            if (result == null || !result.Success || result.Data == null)
            {
                var message = result == null
                    ? "Unknown error occurred while loading reservoir data."
                    : string.Join("; ", result.Errors);
                throw new InvalidOperationException($"Failed to load reservoir data: {message}");
            }

            reservoirData = result.Data;
            return reservoirData;
        }

        public static ReservoirSurfaceData ResolveSurface(ReservoirData reservoir, ReservoirSurfaceData explicitSurface, string surfaceIdentifier)
        {
            if (explicitSurface != null)
                return explicitSurface;

            if (reservoir == null)
                return null;

            if (!string.IsNullOrWhiteSpace(surfaceIdentifier))
            {
                var selectedSurface = (reservoir.Surfaces ?? Enumerable.Empty<ReservoirSurfaceData>())
                    .FirstOrDefault(surface =>
                        string.Equals(surface.SurfaceId, surfaceIdentifier, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(surface.SurfaceName, surfaceIdentifier, StringComparison.OrdinalIgnoreCase));

                if (selectedSurface != null)
                    return selectedSurface;
            }

            var candidateSurfaces = (reservoir.Surfaces ?? Enumerable.Empty<ReservoirSurfaceData>())
                .Where(surface => surface?.Points != null && surface.Points.Count >= 3)
                .ToList();

            var preferredSurface = candidateSurfaces.FirstOrDefault(surface => surface.SurfaceKind != ReservoirSurfaceKind.Fault);
            if (preferredSurface != null)
                return preferredSurface;

            var firstSurface = candidateSurfaces.FirstOrDefault();
            if (firstSurface != null)
                return firstSurface;

            var firstGrid = (reservoir.Grids ?? Enumerable.Empty<ReservoirGridData>())
                .FirstOrDefault(grid => grid?.Nodes != null && grid.Nodes.Count >= 3);

            return firstGrid == null ? null : CreateSurfaceFromGrid(firstGrid);
        }

        public static IReadOnlyList<ReservoirSurfaceData> ResolveFaultSurfaces(
            ReservoirData reservoir,
            ReservoirSurfaceData baseSurface,
            IEnumerable<ReservoirSurfaceData> explicitFaultSurfaces = null)
        {
            var selectedFaults = (explicitFaultSurfaces ?? Enumerable.Empty<ReservoirSurfaceData>())
                .Where(surface => surface?.Points != null && surface.Points.Count >= 2)
                .ToList();

            if (selectedFaults.Count > 0)
                return selectedFaults;

            if (reservoir?.Surfaces == null || reservoir.Surfaces.Count == 0)
                return Array.Empty<ReservoirSurfaceData>();

            return reservoir.Surfaces
                .Where(surface => surface?.Points != null && surface.Points.Count >= 2)
                .Where(surface => surface.SurfaceKind == ReservoirSurfaceKind.Fault)
                .Where(surface => !MatchesSurface(surface, baseSurface))
                .ToList();
        }

        private static ReservoirSurfaceData CreateSurfaceFromGrid(ReservoirGridData grid)
        {
            return new ReservoirSurfaceData
            {
                SurfaceId = grid.GridId,
                SurfaceName = grid.GridName,
                SurfaceKind = ReservoirSurfaceKind.GridDerived,
                SourceRepresentationType = grid.GridKind.ToString(),
                SourceGridId = grid.GridId,
                Points = grid.Nodes.Select(node => node.Position).Where(point => point != null).ToList(),
                BoundingBox = grid.BoundingBox
            };
        }

        private static bool MatchesSurface(ReservoirSurfaceData left, ReservoirSurfaceData right)
        {
            if (left == null || right == null)
                return false;

            if (!string.IsNullOrWhiteSpace(left.SurfaceId) && !string.IsNullOrWhiteSpace(right.SurfaceId))
            {
                return string.Equals(left.SurfaceId, right.SurfaceId, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(left.SurfaceName, right.SurfaceName, StringComparison.OrdinalIgnoreCase);
        }
    }
}