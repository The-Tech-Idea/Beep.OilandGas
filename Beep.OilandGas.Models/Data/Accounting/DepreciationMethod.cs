using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public enum DepreciationMethod
    {
        StraightLine,
        DoubleDeclining,
        UnitsOfProduction,
        MACRS
    }
}
