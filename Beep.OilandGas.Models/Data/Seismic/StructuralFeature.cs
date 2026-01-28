using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class StructuralFeature : ModelEntityBase
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public double? Depth { get; init; }
        public string? Description { get; init; }
    }
}
