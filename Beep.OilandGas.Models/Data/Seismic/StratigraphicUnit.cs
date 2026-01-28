using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class StratigraphicUnit : ModelEntityBase
    {
        public string? UnitId { get; init; }
        public string? Name { get; init; }
        public double? TopDepth { get; init; }
        public double? BaseDepth { get; init; }
        public string? Description { get; init; }
    }
}
