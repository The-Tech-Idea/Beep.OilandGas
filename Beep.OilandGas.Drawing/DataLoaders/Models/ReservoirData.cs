using System;
using System.Collections.Generic;
using Beep.OilandGas.Models;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Represents reservoir data including layers, facies, and fluid contacts.
    /// </summary>
    public class ReservoirData
    {
        /// <summary>
        /// Gets or sets the reservoir identifier.
        /// </summary>
        public string ReservoirId { get; set; }

        /// <summary>
        /// Gets or sets the reservoir name.
        /// </summary>
        public string ReservoirName { get; set; }

        /// <summary>
        /// Gets or sets the formation name.
        /// </summary>
        public string FormationName { get; set; }

        /// <summary>
        /// Gets or sets the reservoir layers.
        /// </summary>
        public List<LayerData> Layers { get; set; } = new List<LayerData>();

        /// <summary>
        /// Gets or sets the fluid contacts.
        /// </summary>
        public FluidContacts FluidContacts { get; set; }

        /// <summary>
        /// Gets or sets the reservoir properties.
        /// </summary>
        public ReservoirProperties Properties { get; set; }

        /// <summary>
        /// Gets or sets the coordinate system (e.g., UTM zone, WGS84).
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the bounding box (min/max X, Y, Z).
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents a geological layer within a reservoir.
    /// </summary>
    public class LayerData
    {
        /// <summary>
        /// Gets or sets the layer identifier.
        /// </summary>
        public string LayerId { get; set; }

        /// <summary>
        /// Gets or sets the layer name.
        /// </summary>
        public string LayerName { get; set; }

        /// <summary>
        /// Gets or sets the top depth (TVDSS - True Vertical Depth Sub Sea).
        /// </summary>
        public double TopDepth { get; set; }

        /// <summary>
        /// Gets or sets the bottom depth (TVDSS).
        /// </summary>
        public double BottomDepth { get; set; }

        /// <summary>
        /// Gets or sets the layer thickness.
        /// </summary>
        public double Thickness => Math.Abs(BottomDepth - TopDepth);

        /// <summary>
        /// Gets or sets the lithology type (e.g., Sandstone, Shale, Limestone, Dolomite).
        /// </summary>
        public string Lithology { get; set; }

        /// <summary>
        /// Gets or sets the facies type (e.g., Channel, Sheet Sand, Shale).
        /// </summary>
        public string Facies { get; set; }

        /// <summary>
        /// Gets or sets the porosity (fraction, 0-1).
        /// </summary>
        public double? Porosity { get; set; }

        /// <summary>
        /// Gets or sets the permeability (mD).
        /// </summary>
        public double? Permeability { get; set; }

        /// <summary>
        /// Gets or sets the water saturation (fraction, 0-1).
        /// </summary>
        public double? WaterSaturation { get; set; }

        /// <summary>
        /// Gets or sets the oil saturation (fraction, 0-1).
        /// </summary>
        public double? OilSaturation { get; set; }

        /// <summary>
        /// Gets or sets the gas saturation (fraction, 0-1).
        /// </summary>
        public double? GasSaturation { get; set; }

        /// <summary>
        /// Gets or sets the net-to-gross ratio (fraction, 0-1).
        /// </summary>
        public double? NetToGross { get; set; }

        /// <summary>
        /// Gets or sets the layer geometry (polygon points for 2D/3D representation).
        /// </summary>
        public List<Point3D> Geometry { get; set; } = new List<Point3D>();

        /// <summary>
        /// Gets or sets whether this is a pay zone (productive layer).
        /// </summary>
        public bool IsPayZone { get; set; }

        /// <summary>
        /// Gets or sets the layer color (for visualization).
        /// </summary>
        public string ColorCode { get; set; }

        /// <summary>
        /// Gets or sets the pattern type (for visualization).
        /// </summary>
        public string PatternType { get; set; }

        /// <summary>
        /// Gets or sets metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents fluid contacts in a reservoir.
    /// </summary>
    public class FluidContacts
    {
        /// <summary>
        /// Gets or sets the Free Water Level (FWL) depth (TVDSS).
        /// </summary>
        public double? FreeWaterLevel { get; set; }

        /// <summary>
        /// Gets or sets the Oil-Water Contact (OWC) depth (TVDSS).
        /// </summary>
        public double? OilWaterContact { get; set; }

        /// <summary>
        /// Gets or sets the Gas-Oil Contact (GOC) depth (TVDSS).
        /// </summary>
        public double? GasOilContact { get; set; }

        /// <summary>
        /// Gets or sets the Gas-Water Contact (GWC) depth (TVDSS).
        /// </summary>
        public double? GasWaterContact { get; set; }

        /// <summary>
        /// Gets or sets the contact date (for time-lapse analysis).
        /// </summary>
        public DateTime? ContactDate { get; set; }

        /// <summary>
        /// Gets or sets the contact source (e.g., "Log", "Pressure", "Production").
        /// </summary>
        public string Source { get; set; }
    }

    /// <summary>
    /// Represents reservoir properties.
    /// </summary>
    public class ReservoirProperties
    {
        /// <summary>
        /// Gets or sets the average porosity (fraction).
        /// </summary>
        public double? AveragePorosity { get; set; }

        /// <summary>
        /// Gets or sets the average permeability (mD).
        /// </summary>
        public double? AveragePermeability { get; set; }

        /// <summary>
        /// Gets or sets the original oil in place (OOIP) in barrels.
        /// </summary>
        public double? OriginalOilInPlace { get; set; }

        /// <summary>
        /// Gets or sets the original gas in place (OGIP) in MCF.
        /// </summary>
        public double? OriginalGasInPlace { get; set; }

        /// <summary>
        /// Gets or sets the reservoir temperature (degrees Fahrenheit or Celsius).
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the reservoir pressure (psi).
        /// </summary>
        public double? Pressure { get; set; }

        /// <summary>
        /// Gets or sets the reservoir drive mechanism (e.g., "Water Drive", "Gas Cap", "Solution Gas").
        /// </summary>
        public string DriveMechanism { get; set; }
    }

    /// <summary>
    /// Represents a 3D point.
    /// </summary>
    public class Point3D
    {
        /// <summary>
        /// Gets or sets the X coordinate.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the Z coordinate (depth).
        /// </summary>
        public double Z { get; set; }
    }

    /// <summary>
    /// Represents a bounding box.
    /// </summary>
    public class BoundingBox
    {
        /// <summary>
        /// Gets or sets the minimum X coordinate.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// Gets or sets the maximum X coordinate.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// Gets or sets the minimum Y coordinate.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// Gets or sets the maximum Y coordinate.
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Gets or sets the minimum Z coordinate (depth).
        /// </summary>
        public double MinZ { get; set; }

        /// <summary>
        /// Gets or sets the maximum Z coordinate (depth).
        /// </summary>
        public double MaxZ { get; set; }
    }
}

