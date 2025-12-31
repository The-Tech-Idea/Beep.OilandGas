using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Agreement;
using Beep.OilandGas.Models.DTOs.Lease;
using Beep.OilandGas.Models.DTOs.Agreement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for lease and agreement operations.
    /// </summary>
    public interface ILeaseService
    {
        /// <summary>
        /// Registers a fee mineral lease.
        /// </summary>
        Task<FEE_MINERAL_LEASE> RegisterFeeMineralLeaseAsync(CreateFeeMineralLeaseRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Registers a government lease.
        /// </summary>
        Task<GOVERNMENT_LEASE> RegisterGovernmentLeaseAsync(CreateGovernmentLeaseRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a fee mineral lease by ID.
        /// </summary>
        Task<FEE_MINERAL_LEASE?> GetFeeMineralLeaseAsync(string leaseId, string? connectionName = null);
        
        /// <summary>
        /// Gets leases by property.
        /// </summary>
        Task<List<FEE_MINERAL_LEASE>> GetLeasesByPropertyAsync(string propertyId, string? connectionName = null);
        
        /// <summary>
        /// Registers a sales agreement.
        /// </summary>
        Task<SALES_AGREEMENT> RegisterSalesAgreementAsync(CreateSalesAgreementRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Registers a transportation agreement.
        /// </summary>
        Task<TRANSPORTATION_AGREEMENT> RegisterTransportationAgreementAsync(CreateTransportationAgreementRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Renews a lease.
        /// </summary>
        Task<LeaseRenewalResult> RenewLeaseAsync(string leaseId, LeaseRenewalRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets leases expiring within a date range.
        /// </summary>
        Task<List<LeaseExpirationAlert>> GetLeasesExpiringAsync(DateTime? expirationDate, string? connectionName = null);
        
        /// <summary>
        /// Gets lease summary.
        /// </summary>
        Task<LeaseSummary> GetLeaseSummaryAsync(string leaseId, string? connectionName = null);
    }
}

