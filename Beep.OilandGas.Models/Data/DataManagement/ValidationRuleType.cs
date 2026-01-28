using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public enum ValidationRuleType
    {
        Required,
        MaxLength,
        MinLength,
        Range,
        Pattern,
        Custom,
        ForeignKey,
        Unique,
        DateRange,
        Format,
        BusinessRule
    }
}
