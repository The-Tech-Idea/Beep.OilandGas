using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Represents a straight section cut through a reservoir map.
    /// </summary>
    public sealed class ReservoirSectionLine
    {
        /// <summary>
        /// Gets or sets the section name.
        /// </summary>
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the section start point.
        /// </summary>
        public Point3D Start { get; set; }

        /// <summary>
        /// Gets or sets the section end point.
        /// </summary>
        public Point3D End { get; set; }

        /// <summary>
        /// Gets the section length in map units.
        /// </summary>
        public double Length
        {
            get
            {
                if (Start == null || End == null)
                    return 0;

                var deltaX = End.X - Start.X;
                var deltaY = End.Y - Start.Y;
                return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
            }
        }
    }

    /// <summary>
    /// Represents a sampled surface profile along a section line.
    /// </summary>
    public sealed class ReservoirSectionProfile
    {
        /// <summary>
        /// Gets or sets the source surface identifier.
        /// </summary>
        public string SurfaceId { get; set; }

        /// <summary>
        /// Gets or sets the profile display name.
        /// </summary>
        public string SurfaceName { get; set; }

        /// <summary>
        /// Gets or sets the surface kind.
        /// </summary>
        public ReservoirSurfaceKind SurfaceKind { get; set; }

        /// <summary>
        /// Gets or sets the value unit.
        /// </summary>
        public string ValueUnit { get; set; }

        /// <summary>
        /// Gets or sets the sampled section points.
        /// </summary>
        public List<ReservoirSectionSample> Samples { get; set; } = new List<ReservoirSectionSample>();
    }

    /// <summary>
    /// Represents a single distance and depth sample along a section.
    /// </summary>
    public sealed class ReservoirSectionSample
    {
        /// <summary>
        /// Gets or sets the distance along the section.
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Gets or sets the sampled depth.
        /// </summary>
        public double Depth { get; set; }
    }

    /// <summary>
    /// Represents a well marker projected onto a section.
    /// </summary>
    public sealed class ReservoirWellSectionMarker
    {
        /// <summary>
        /// Gets or sets the well identifier.
        /// </summary>
        public string WellId { get; set; }

        /// <summary>
        /// Gets or sets the well label.
        /// </summary>
        public string WellName { get; set; }

        /// <summary>
        /// Gets or sets the UWI.
        /// </summary>
        public string Uwi { get; set; }

        /// <summary>
        /// Gets or sets the distance along the section where the well projects.
        /// </summary>
        public double DistanceAlongSection { get; set; }

        /// <summary>
        /// Gets or sets the plotted well depth.
        /// </summary>
        public double Depth { get; set; }

        /// <summary>
        /// Gets or sets the perpendicular map offset from the section line.
        /// </summary>
        public double OffsetFromSection { get; set; }

        /// <summary>
        /// Gets or sets an optional color override.
        /// </summary>
        public string ColorCode { get; set; }
    }

    /// <summary>
    /// Represents extracted section-view data derived from reservoir surfaces and well locations.
    /// </summary>
    public sealed class ReservoirCrossSectionData
    {
        /// <summary>
        /// Gets or sets the section line used to generate the view.
        /// </summary>
        public ReservoirSectionLine SectionLine { get; set; }

        /// <summary>
        /// Gets or sets the sampled section profiles.
        /// </summary>
        public List<ReservoirSectionProfile> Profiles { get; set; } = new List<ReservoirSectionProfile>();

        /// <summary>
        /// Gets or sets the projected well markers.
        /// </summary>
        public List<ReservoirWellSectionMarker> WellMarkers { get; set; } = new List<ReservoirWellSectionMarker>();

        /// <summary>
        /// Gets or sets fluid contacts to render in the section.
        /// </summary>
        public FluidContacts FluidContacts { get; set; }

        /// <summary>
        /// Gets or sets the extracted section bounds using distance as X and depth as Z semantics.
        /// </summary>
        public BoundingBox Bounds { get; set; }
    }
}