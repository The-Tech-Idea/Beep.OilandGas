using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for IAS 36 impairment testing at CGU/property level.
    /// </summary>
    public interface IImpairmentTestingService
    {
        Task<IMPAIRMENT_RECORD> EvaluateImpairmentAsync(
            string cguId,
            decimal carryingAmount,
            decimal valueInUse,
            decimal fairValueLessCosts,
            DateTime testDate,
            string userId,
            string cn = "PPDM39");
    }
}
