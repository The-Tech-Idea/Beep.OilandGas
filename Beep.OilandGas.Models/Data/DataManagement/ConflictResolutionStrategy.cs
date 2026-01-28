using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public enum ConflictResolutionStrategy
    {
        SourceWins,
        TargetWins,
        Manual,
        Merge,
        Skip
    }
}
