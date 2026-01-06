using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.OilandGas.Models
{
    public class WellData
    {
        public int ID { get; set; }
        public string UWI { get; set; }
        public string UBHI { get; set; }
        public WellData()
        { GuidID = Guid.NewGuid().ToString(); }
        public string GuidID { get; set; }

        public List<WellData_Borehole> BoreHoles { get; set;}=new List<WellData_Borehole>();
      


    }
}




