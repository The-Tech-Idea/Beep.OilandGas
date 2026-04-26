using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public enum ProspectStatus
    {
        Unknown = 0,
        Identified,
        Evaluated,
        Approved,
        Rejected
    }
}
