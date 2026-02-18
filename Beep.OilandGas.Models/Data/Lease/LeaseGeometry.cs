using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class LeaseGeometry
    {
        public string LeaseId { get; set; } = string.Empty;
        public string GeometryType { get; set; } = "Polygon";
        public List<GeoPoint> Coordinates { get; set; } = new List<GeoPoint>();
        public double Area { get; set; }
        public string Unit { get; set; } = "Acres";
    }

    public class GeoPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Elevation { get; set; }
    }
}
