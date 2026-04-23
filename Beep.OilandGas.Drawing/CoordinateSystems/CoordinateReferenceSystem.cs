using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Drawing.CoordinateSystems
{
    /// <summary>
    /// Identifies the authority behind a CRS definition.
    /// </summary>
    public enum CoordinateAuthority
    {
        Unknown,
        Custom,
        EPSG,
        OGC,
        PPDM,
        Energistics
    }

    /// <summary>
    /// Represents the general shape of a coordinate reference definition.
    /// </summary>
    public enum CoordinateReferenceSystemKind
    {
        Depth,
        Geographic,
        Projected,
        Section,
        Time,
        Custom
    }

    /// <summary>
    /// Represents a typed coordinate reference system used by scenes and adapters.
    /// </summary>
    public sealed class CoordinateReferenceSystem
    {
        private readonly List<CoordinateAxisDefinition> axes;

        /// <summary>
        /// Gets the stable CRS identifier.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the authority that owns the identifier.
        /// </summary>
        public CoordinateAuthority Authority { get; }

        /// <summary>
        /// Gets the CRS kind.
        /// </summary>
        public CoordinateReferenceSystemKind Kind { get; }

        /// <summary>
        /// Gets the axis definitions in declared order.
        /// </summary>
        public IReadOnlyList<CoordinateAxisDefinition> Axes => axes.AsReadOnly();

        /// <summary>
        /// Gets whether the CRS supports plan-view mapping.
        /// </summary>
        public bool SupportsPlanView => Kind == CoordinateReferenceSystemKind.Geographic || Kind == CoordinateReferenceSystemKind.Projected;

        /// <summary>
        /// Gets whether the CRS includes depth or section semantics.
        /// </summary>
        public bool SupportsSectionOrDepthView => axes.Any(axis => axis.Kind == CoordinateAxisKind.Depth || axis.Kind == CoordinateAxisKind.MeasuredDepth || axis.Kind == CoordinateAxisKind.SectionDistance);

        public CoordinateReferenceSystem(
            string identifier,
            string name,
            CoordinateAuthority authority,
            CoordinateReferenceSystemKind kind,
            IEnumerable<CoordinateAxisDefinition> axisDefinitions)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("CRS identifier is required.", nameof(identifier));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("CRS name is required.", nameof(name));
            if (axisDefinitions == null)
                throw new ArgumentNullException(nameof(axisDefinitions));

            axes = axisDefinitions.Where(axis => axis != null).ToList();
            if (axes.Count == 0)
                throw new ArgumentException("At least one axis definition is required.", nameof(axisDefinitions));

            Identifier = identifier.Trim();
            Name = name.Trim();
            Authority = authority;
            Kind = kind;
        }

        /// <summary>
        /// Creates a local depth CRS.
        /// </summary>
        public static CoordinateReferenceSystem CreateDepth(string unitCode = "ft", string identifier = "LOCAL:DEPTH", string name = "Local Depth")
        {
            return new CoordinateReferenceSystem(
                identifier,
                name,
                CoordinateAuthority.Custom,
                CoordinateReferenceSystemKind.Depth,
                new[]
                {
                    new CoordinateAxisDefinition(CoordinateAxisKind.Depth, "Depth", MeasurementUnit.FromCode(unitCode), isInverted: true)
                });
        }

        /// <summary>
        /// Creates an OGC CRS84 geographic CRS.
        /// </summary>
        public static CoordinateReferenceSystem CreateGeographicCrs84()
        {
            return new CoordinateReferenceSystem(
                "OGC:CRS84",
                "WGS 84 / CRS84",
                CoordinateAuthority.OGC,
                CoordinateReferenceSystemKind.Geographic,
                new[]
                {
                    new CoordinateAxisDefinition(CoordinateAxisKind.Longitude, "Longitude", MeasurementUnit.DecimalDegrees),
                    new CoordinateAxisDefinition(CoordinateAxisKind.Latitude, "Latitude", MeasurementUnit.DecimalDegrees)
                });
        }

        /// <summary>
        /// Creates a projected CRS definition using the supplied identifier and linear unit.
        /// </summary>
        public static CoordinateReferenceSystem CreateProjected(string identifier, string name, string unitCode = "m", CoordinateAuthority authority = CoordinateAuthority.EPSG)
        {
            return new CoordinateReferenceSystem(
                identifier,
                name,
                authority,
                CoordinateReferenceSystemKind.Projected,
                new[]
                {
                    new CoordinateAxisDefinition(CoordinateAxisKind.Easting, "Easting", MeasurementUnit.FromCode(unitCode)),
                    new CoordinateAxisDefinition(CoordinateAxisKind.Northing, "Northing", MeasurementUnit.FromCode(unitCode))
                });
        }

        /// <summary>
        /// Creates a section CRS using distance and depth axes.
        /// </summary>
        public static CoordinateReferenceSystem CreateSection(string distanceUnitCode = "ft", string depthUnitCode = "ft", string identifier = "LOCAL:SECTION", string name = "Local Section")
        {
            return new CoordinateReferenceSystem(
                identifier,
                name,
                CoordinateAuthority.Custom,
                CoordinateReferenceSystemKind.Section,
                new[]
                {
                    new CoordinateAxisDefinition(CoordinateAxisKind.SectionDistance, "Section Distance", MeasurementUnit.FromCode(distanceUnitCode)),
                    new CoordinateAxisDefinition(CoordinateAxisKind.Depth, "Depth", MeasurementUnit.FromCode(depthUnitCode), isInverted: true)
                });
        }

        /// <summary>
        /// Creates a time-axis CRS for future chart and time-domain scenes.
        /// </summary>
        public static CoordinateReferenceSystem CreateTime(string unitCode = "d", string identifier = "LOCAL:TIME", string name = "Local Time")
        {
            return new CoordinateReferenceSystem(
                identifier,
                name,
                CoordinateAuthority.Custom,
                CoordinateReferenceSystemKind.Time,
                new[]
                {
                    new CoordinateAxisDefinition(CoordinateAxisKind.Time, "Time", MeasurementUnit.FromCode(unitCode))
                });
        }
    }
}