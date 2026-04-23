using System.Collections.Generic;
using Beep.OilandGas.Drawing.CoordinateSystems;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Identifies the asset type represented on a field map.
    /// </summary>
    public enum FieldMapAssetKind
    {
        Unknown,
        Field,
        Pool,
        Lease,
        LandRight,
        ProtectedArea,
        HazardZone,
        Facility,
        Well,
        Seismic2D,
        Seismic3D,
        Pipeline,
        SurfaceSystem
    }

    /// <summary>
    /// Identifies a surface connection rendered on a field map.
    /// </summary>
    public enum FieldMapConnectionKind
    {
        Unknown,
        Flowline,
        GatheringLine,
        ExportLine,
        Utility,
        FacilityLink
    }

    /// <summary>
    /// Represents a polygonal or boundary asset on a field map.
    /// </summary>
    public sealed class FieldMapAreaAsset
    {
        /// <summary>
        /// Gets or sets the asset identifier.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Gets or sets the asset display name.
        /// </summary>
        public string AssetName { get; set; }

        /// <summary>
        /// Gets or sets the asset kind.
        /// </summary>
        public FieldMapAssetKind AssetKind { get; set; } = FieldMapAssetKind.Unknown;

        /// <summary>
        /// Gets or sets the boundary points.
        /// </summary>
        public List<Point3D> BoundaryPoints { get; set; } = new List<Point3D>();

        /// <summary>
        /// Gets or sets an optional fill color override as a hex value.
        /// </summary>
        public string FillColorCode { get; set; }

        /// <summary>
        /// Gets or sets an optional stroke color override as a hex value.
        /// </summary>
        public string StrokeColorCode { get; set; }

        /// <summary>
        /// Gets or sets asset metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents a point or path asset on a field map.
    /// </summary>
    public sealed class FieldMapPointAsset
    {
        /// <summary>
        /// Gets or sets the asset identifier.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Gets or sets the asset display name.
        /// </summary>
        public string AssetName { get; set; }

        /// <summary>
        /// Gets or sets the asset kind.
        /// </summary>
        public FieldMapAssetKind AssetKind { get; set; } = FieldMapAssetKind.Unknown;

        /// <summary>
        /// Gets or sets the primary asset location.
        /// </summary>
        public Point3D Location { get; set; }

        /// <summary>
        /// Gets or sets optional path points for wells, pipelines, or surface systems.
        /// </summary>
        public List<Point3D> PathPoints { get; set; } = new List<Point3D>();

        /// <summary>
        /// Gets or sets the status or role.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets an optional color override as a hex value.
        /// </summary>
        public string ColorCode { get; set; }

        /// <summary>
        /// Gets or sets asset metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents a surface-system connection between wells, facilities, or other map assets.
    /// </summary>
    public sealed class FieldMapConnectionAsset
    {
        /// <summary>
        /// Gets or sets the connection identifier.
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the connection display name.
        /// </summary>
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the connection kind.
        /// </summary>
        public FieldMapConnectionKind ConnectionKind { get; set; } = FieldMapConnectionKind.Unknown;

        /// <summary>
        /// Gets or sets the source asset identifier when known.
        /// </summary>
        public string FromAssetId { get; set; }

        /// <summary>
        /// Gets or sets the target asset identifier when known.
        /// </summary>
        public string ToAssetId { get; set; }

        /// <summary>
        /// Gets or sets the connection vertices.
        /// </summary>
        public List<Point3D> Vertices { get; set; } = new List<Point3D>();

        /// <summary>
        /// Gets or sets an optional color override as a hex value.
        /// </summary>
        public string ColorCode { get; set; }

        /// <summary>
        /// Gets or sets metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents a full field map with typed areas, point assets, and a map CRS.
    /// </summary>
    public sealed class FieldMapData
    {
        /// <summary>
        /// Gets or sets the map name.
        /// </summary>
        public string MapName { get; set; }

        /// <summary>
        /// Gets or sets the coordinate reference system for the map.
        /// </summary>
        public CoordinateReferenceSystem CoordinateReferenceSystem { get; set; }

        /// <summary>
        /// Gets or sets the polygon and boundary assets.
        /// </summary>
        public List<FieldMapAreaAsset> AreaAssets { get; set; } = new List<FieldMapAreaAsset>();

        /// <summary>
        /// Gets or sets the point and path assets.
        /// </summary>
        public List<FieldMapPointAsset> PointAssets { get; set; } = new List<FieldMapPointAsset>();

        /// <summary>
        /// Gets or sets explicit connection assets for surface networks.
        /// </summary>
        public List<FieldMapConnectionAsset> ConnectionAssets { get; set; } = new List<FieldMapConnectionAsset>();

        /// <summary>
        /// Gets or sets optional map bounds.
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}