using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for lease economic interest and ownership validation.
    /// </summary>
    public interface ILeaseEconomicInterestService
    {
        Task<List<OWNERSHIP_INTEREST>> GetOwnershipInterestsAsync(
            string propertyOrLeaseId,
            DateTime? asOfDate = null,
            string cn = "PPDM39");

        Task<List<ROYALTY_INTEREST>> GetRoyaltyInterestsAsync(
            string propertyOrLeaseId,
            DateTime? asOfDate = null,
            string cn = "PPDM39");

        Task<bool> ValidateEconomicInterestsAsync(
            string propertyOrLeaseId,
            DateTime? asOfDate = null,
            string cn = "PPDM39");
    }
}
