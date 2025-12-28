using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.OilandGas.Models
{
    //borehole tubing
    public  class WellData_Tubing
    {
        public int ID { get; set; }
        public string UWI { get; set; }
        public string UBHI { get; set; }
        public int BoreHoleIndex { get; set; }
        public WellData_Tubing()
        { GuidID = Guid.NewGuid().ToString(); }
        public string GuidID { get; set; }
        public float TopDepth { get; set; }
        public float BottomDepth { get; set; }
        public float Diameter { get; set; }
        public int TubeIndex { get; set; }
        public int TubeType { get; set; }
        public List<WellData_Equip> Equip { get; set; } = new List<WellData_Equip>();
        public List<(float x, float y)> StoredPoints { get; set; } = new List<(float x, float y)>();
    }
}

