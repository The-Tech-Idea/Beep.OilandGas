using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.WellSchematics
{
    // Well Casing
    public class WellData_Casing
    {
       
        public int ID { get; set; }
        public string UWI { get; set; }
        public string UBHI { get; set; }
        public int BoreHoleIndex { get; set; }
        public WellData_Casing()
        { GuidID = Guid.NewGuid().ToString(); }
        public string GuidID { get; set; }
        public float Depth { get; set; }
        public float Diameter { get; set; }
        public float TopDepth { get; set; }
        public float BottomDepth { get; set; }
        public string CasingType { get; set; }
        public float INNER_DIAMETER { get; set; }
        public float OUTER_DIAMETER { get; set; }
        // Points on the outer and inner curves of the wellbore
        public Dictionary<int, List<(float x, float y)>> outerCasingPoints { get; set; } = new Dictionary<int, List<(float x, float y)>>();
        public Dictionary<int, List<(float x, float y)>> innerCasingPoints { get; set; } = new Dictionary<int, List<(float x, float y)>>();

    }
}
