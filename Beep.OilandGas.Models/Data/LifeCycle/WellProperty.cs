using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class WellProperty : ModelEntityBase
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string DataType { get; set; } = "String";
    }
}
