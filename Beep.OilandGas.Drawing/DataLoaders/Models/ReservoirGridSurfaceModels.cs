using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Defines the structural type of a reservoir grid representation.
    /// </summary>
    public enum ReservoirGridKind
    {
        Unknown,
        Structured2D,
        Structured3D,
        CornerPoint3D,
        Unstructured2D,
        Unstructured3D
    }

    /// <summary>
    /// Defines the semantic type of a reservoir surface representation.
    /// </summary>
    public enum ReservoirSurfaceKind
    {
        Unknown,
        Horizon,
        Fault,
        Property,
        Structure,
        Isochore,
        GridDerived
    }

    /// <summary>
    /// Represents a reservoir grid that can be used for gridding, contouring, and map extraction.
    /// </summary>
    public class ReservoirGridData
    {
        /// <summary>
        /// Gets or sets the grid identifier.
        /// </summary>
        public string GridId { get; set; }

        /// <summary>
        /// Gets or sets the grid name.
        /// </summary>
        public string GridName { get; set; }

        /// <summary>
        /// Gets or sets the grid kind.
        /// </summary>
        public ReservoirGridKind GridKind { get; set; } = ReservoirGridKind.Unknown;

        /// <summary>
        /// Gets or sets the structured column count when known.
        /// </summary>
        public int? ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the structured row count when known.
        /// </summary>
        public int? RowCount { get; set; }

        /// <summary>
        /// Gets or sets the structured layer count when known.
        /// </summary>
        public int? LayerCount { get; set; }

        /// <summary>
        /// Gets or sets the sampled grid nodes.
        /// </summary>
        public List<ReservoirGridNode> Nodes { get; set; } = new List<ReservoirGridNode>();

        /// <summary>
        /// Gets or sets the grid bounding box.
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets representation metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents a sampled node within a reservoir grid.
    /// </summary>
    public class ReservoirGridNode
    {
        /// <summary>
        /// Gets or sets the zero-based node index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the structured I index when known.
        /// </summary>
        public int? I { get; set; }

        /// <summary>
        /// Gets or sets the structured J index when known.
        /// </summary>
        public int? J { get; set; }

        /// <summary>
        /// Gets or sets the structured K index when known.
        /// </summary>
        public int? K { get; set; }

        /// <summary>
        /// Gets or sets the node position.
        /// </summary>
        public Point3D Position { get; set; }
    }

    /// <summary>
    /// Represents a surface that can be contoured or overlaid on reservoir maps.
    /// </summary>
    public class ReservoirSurfaceData
    {
        /// <summary>
        /// Gets or sets the surface identifier.
        /// </summary>
        public string SurfaceId { get; set; }

        /// <summary>
        /// Gets or sets the surface name.
        /// </summary>
        public string SurfaceName { get; set; }

        /// <summary>
        /// Gets or sets the surface kind.
        /// </summary>
        public ReservoirSurfaceKind SurfaceKind { get; set; } = ReservoirSurfaceKind.Unknown;

        /// <summary>
        /// Gets or sets the source representation type from the upstream file.
        /// </summary>
        public string SourceRepresentationType { get; set; }

        /// <summary>
        /// Gets or sets the associated grid identifier when the surface is grid-derived.
        /// </summary>
        public string SourceGridId { get; set; }

        /// <summary>
        /// Gets or sets the property name when the surface represents a property map.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the declared value unit when available.
        /// </summary>
        public string ValueUnit { get; set; }

        /// <summary>
        /// Gets or sets the sampled surface points.
        /// </summary>
        public List<Point3D> Points { get; set; } = new List<Point3D>();

        /// <summary>
        /// Gets or sets the surface bounding box.
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets representation metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}