using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for revenue recognition (ASC 606).
    /// </summary>
    public interface IRevenueService
    {
        Task<REVENUE_ALLOCATION> RecognizeRevenueAsync(ALLOCATION_DETAIL allocation, string userId, string cn = "PPDM39");
        Task<bool> ValidateAsync(REVENUE_ALLOCATION allocation, string cn = "PPDM39");
    }
}
