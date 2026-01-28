using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public enum MaskingType
    {
        FullMask,           // Replace with ***
        PartialMask,        // Show first/last N characters
        Hash,               // Hash the value
        Randomize,          // Random value of same type
        Nullify,            // Set to null
        Custom              // Custom masking logic
    }
}
