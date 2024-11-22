namespace Beep.OilandGas.Models
{
    public class WellData_Borehole
    {

        public int ID { get; set; }
        public string UWI { get; set; }
       
        public WellData_Borehole() { GuidID = Guid.NewGuid().ToString(); }
        public string GuidID { get; set; }
        public string UBHI { get;set; }
        public float TopDepth { get; set; }
        public float BottomDepth { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Diameter { get; set; }
        public int BoreHoleIndex { get; set; }
        public double x_left { get; set; }
        public double x_right { get; set; }
        public double Top_y { get; set;}
        public double Bottom_y { get; set; }
        public bool IsVertical { get; set; }=true;
        public float OuterDiameterOffset { get; set; }
        public List<WellData_Casing> Casing { get; set; } = new List<WellData_Casing>();
        public List<WellData_Equip> Equip { get; set; } = new List<WellData_Equip>();
        public List<WellData_Tubing> Tubing { get; set; } = new List<WellData_Tubing>();
        public List<WellData_Perf> Perforation { get; set; } = new List<WellData_Perf>();

        // Points on the outer and inner curves of the wellbore
        public Dictionary<int, List<(float x, float y)>> outerWellborePointsSide1 { get; set; } = new Dictionary<int, List<(float x, float y)>>();
        public Dictionary<int, List<(float x, float y)>> outerWellborePointsSide2 { get; set; } = new Dictionary<int, List<(float x, float y)>>();
        public Dictionary<int, List<(float x, float y)>> innerWellborePoints { get; set; } = new Dictionary<int, List<(float x, float y)>>();

     

    }
}
