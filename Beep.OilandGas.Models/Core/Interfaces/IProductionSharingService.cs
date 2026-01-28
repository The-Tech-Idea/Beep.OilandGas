using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for Production Sharing Agreement (PSA) calculations.
    /// </summary>
    public interface IProductionSharingService
    {
        Task<PRODUCTION_SHARING_AGREEMENT> GetActiveAgreementAsync(
            string propertyId,
            DateTime asOfDate,
            string cn = "PPDM39");

        Task<PRODUCTION_SHARING_ENTITLEMENT> CalculateEntitlementAsync(
            ALLOCATION_DETAIL ALLOCATION_DETAIL,
            DateTime productionDate,
            string userId,
            string cn = "PPDM39");
    }
}
