using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Unproved property accounting service for acquisition, impairment, and reclassification.
    /// </summary>
    public interface IUnprovedPropertyService
    {
        Task<ACCOUNTING_COST> RecordUnprovedAcquisitionAsync(
            string propertyId,
            decimal cost,
            DateTime acquisitionDate,
            string userId,
            string cn = "PPDM39");

        Task<LEASEHOLD_CARRYING_GROUP> CreateCarryingGroupAsync(
            string propertyId,
            string groupName,
            DateTime effectiveDate,
            DateTime? expiryDate,
            string userId,
            string cn = "PPDM39");

        Task<LEASE_OPTION> RecordLeaseOptionAsync(
            string leaseId,
            DateTime optionDate,
            DateTime optionExpiryDate,
            decimal bonusAmount,
            string userId,
            string cn = "PPDM39");

        Task<DELAY_RENTAL> RecordDelayRentalAsync(
            string leaseId,
            DateTime rentalDate,
            decimal amount,
            DateTime? nextDueDate,
            string userId,
            string cn = "PPDM39");

        Task<List<LEASE_EXPIRY_EVENT>> EvaluateExpiriesAsync(
            DateTime asOfDate,
            string userId,
            string cn = "PPDM39");

        Task<ACCOUNTING_COST?> TestImpairmentAsync(
            string propertyId,
            DateTime asOfDate,
            string userId,
            string cn = "PPDM39");

        Task<bool> ReclassifyToProvedAsync(
            string propertyId,
            DateTime effectiveDate,
            string userId,
            string cn = "PPDM39");
    }
}
