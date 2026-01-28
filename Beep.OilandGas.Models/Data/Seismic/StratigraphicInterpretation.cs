using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class StratigraphicInterpretation : ModelEntityBase
    {
        public string? Summary { get; init; }
        public List<string>? Layers { get; init; }
    }
}
