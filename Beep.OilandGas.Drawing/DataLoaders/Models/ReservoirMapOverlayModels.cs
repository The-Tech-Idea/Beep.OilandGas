using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Represents a well location or simple well path on a plan-view reservoir map.
    /// </summary>
    public sealed class ReservoirWellMapPoint
    {
        /// <summary>
        /// Gets or sets the well identifier.
        /// </summary>
        public string WellId { get; set; }

        /// <summary>
        /// Gets or sets the well display name.
        /// </summary>
        public string WellName { get; set; }

        /// <summary>
        /// Gets or sets the well UWI.
        /// </summary>
        public string Uwi { get; set; }

        /// <summary>
        /// Gets or sets the surface location.
        /// </summary>
        public Point3D SurfaceLocation { get; set; }

        /// <summary>
        /// Gets or sets optional trajectory points that should be rendered in map view.
        /// </summary>
        public List<Point3D> TrajectoryPoints { get; set; } = new List<Point3D>();

        /// <summary>
        /// Gets or sets the well status or role.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets an optional color override as a hex value.
        /// </summary>
        public string ColorCode { get; set; }

        /// <summary>
        /// Gets or sets additional metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}