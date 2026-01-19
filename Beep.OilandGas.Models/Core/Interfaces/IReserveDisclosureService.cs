using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for IFRS reserve and resource disclosure packages.
    /// </summary>
    public interface IReserveDisclosureService
    {
        Task<RESERVE_DISCLOSURE_PACKAGE> BuildDisclosureAsync(
            string propertyId,
            DateTime? asOfDate = null,
            string cn = "PPDM39");
    }
}
