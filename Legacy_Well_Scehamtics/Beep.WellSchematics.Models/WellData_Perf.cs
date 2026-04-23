using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.WellSchematics
{
    // Well Perferation
    public class WellData_Perf
    {
        public int ID { get; set; }
        public string UWI { get; set; }
        public string UBHI { get; set; }
        public int BoreHoleIndex { get; set; }
        public WellData_Perf()
        { GuidID = Guid.NewGuid().ToString(); }
        public string GuidID { get; set; }
        public float TopDepth { get; set; }
        public float BottomDepth { get; set; }
        public string CompletionCode { get; set; }
        public float ShotsPerUOM { get; set; }
        public string PerfType { get; set; }
        public float ShotDensity { get; set; }
        public float ShotSize { get; set; }
        public float ShotDepth { get; set; }
        public float ShotThickness { get; set; }
        public float ShotLength { get; set; }
        public float ShotWidth { get; set; }
    }
}
