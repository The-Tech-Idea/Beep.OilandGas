using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.WellSchematics
{
    // Well Equipment
    public class WellData_Equip
    {
        public int ID { get; set; }
        public string UWI { get; set; }
        public string UBHI { get; set; }
        public int BoreHoleIndex { get; set; }
        public WellData_Equip()
        { GuidID = Guid.NewGuid().ToString(); }
        public string GuidID { get; set; }
        public float TopDepth { get; set; }
        public float BottomDepth { get; set; }
        public float Diameter { get; set; }
        public int TubeIndex { get; set; }
        public string EquipmentType { get; set; }
        public string EquipmentSvg { get; set; }
        public string ToolTipText { get; set; }
    }
}
