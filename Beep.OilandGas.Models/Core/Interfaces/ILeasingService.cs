using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for IFRS 16 lease accounting.
    /// </summary>
    public interface ILeasingService
    {
        Task<LEASE_ACCOUNTING_ENTRY> RecognizeLeaseAsync(
            LEASE_CONTRACT leaseContract,
            IEnumerable<LEASE_PAYMENT> payments,
            DateTime commencementDate,
            string userId,
            string cn = "PPDM39");

        Task<LEASE_ACCOUNTING_ENTRY> RemeasureLeaseAsync(
            string leaseId,
            DateTime measurementDate,
            string userId,
            string cn = "PPDM39");
    }
}
