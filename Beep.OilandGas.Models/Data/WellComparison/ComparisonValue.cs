using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellComparison
{
    public class ComparisonValue : ModelEntityBase
    {
        public string FieldName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string DataType { get; set; } = "String";
        public bool IsMatch { get; set; }
    }
}
