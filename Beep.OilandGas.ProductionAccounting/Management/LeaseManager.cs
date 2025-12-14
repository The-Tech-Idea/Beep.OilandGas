using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Validation;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Management
{
    /// <summary>
    /// Manages lease agreements and operations.
    /// </summary>
    public class LeaseManager
    {
        private readonly Dictionary<string, LeaseAgreement> leases = new();
        private readonly Dictionary<string, OilSalesAgreement> salesAgreements = new();
        private readonly Dictionary<string, TransportationAgreement> transportationAgreements = new();

        /// <summary>
        /// Registers a new lease agreement.
        /// </summary>
        public void RegisterLease(LeaseAgreement lease)
        {
            if (lease == null)
                throw new ArgumentNullException(nameof(lease));

            if (string.IsNullOrEmpty(lease.LeaseId))
                throw new InvalidLeaseDataException(nameof(lease.LeaseId), "Lease ID cannot be null or empty.");

            LeaseValidator.Validate(lease);
            leases[lease.LeaseId] = lease;
        }

        /// <summary>
        /// Gets a lease by ID.
        /// </summary>
        public LeaseAgreement? GetLease(string leaseId)
        {
            return leases.TryGetValue(leaseId, out var lease) ? lease : null;
        }

        /// <summary>
        /// Gets all leases.
        /// </summary>
        public IEnumerable<LeaseAgreement> GetAllLeases()
        {
            return leases.Values;
        }

        /// <summary>
        /// Gets leases by type.
        /// </summary>
        public IEnumerable<LeaseAgreement> GetLeasesByType(LeaseType leaseType)
        {
            return leases.Values.Where(l => l.LeaseType == leaseType);
        }

        /// <summary>
        /// Gets active leases (not expired).
        /// </summary>
        public IEnumerable<LeaseAgreement> GetActiveLeases(DateTime asOfDate)
        {
            return leases.Values.Where(l =>
                l.EffectiveDate <= asOfDate &&
                (l.ExpirationDate == null || l.ExpirationDate >= asOfDate));
        }

        /// <summary>
        /// Checks if a lease is held by production.
        /// </summary>
        public bool IsHeldByProduction(string leaseId)
        {
            var lease = GetLease(leaseId);
            return lease?.Provisions.IsHeldByProduction ?? false;
        }

        /// <summary>
        /// Registers a sales agreement.
        /// </summary>
        public void RegisterSalesAgreement(OilSalesAgreement agreement)
        {
            if (agreement == null)
                throw new ArgumentNullException(nameof(agreement));

            if (string.IsNullOrEmpty(agreement.AgreementId))
                throw new InvalidLeaseDataException(nameof(agreement.AgreementId), "Agreement ID cannot be null or empty.");

            salesAgreements[agreement.AgreementId] = agreement;
        }

        /// <summary>
        /// Gets a sales agreement by ID.
        /// </summary>
        public OilSalesAgreement? GetSalesAgreement(string agreementId)
        {
            return salesAgreements.TryGetValue(agreementId, out var agreement) ? agreement : null;
        }

        /// <summary>
        /// Registers a transportation agreement.
        /// </summary>
        public void RegisterTransportationAgreement(TransportationAgreement agreement)
        {
            if (agreement == null)
                throw new ArgumentNullException(nameof(agreement));

            if (string.IsNullOrEmpty(agreement.AgreementId))
                throw new InvalidLeaseDataException(nameof(agreement.AgreementId), "Agreement ID cannot be null or empty.");

            transportationAgreements[agreement.AgreementId] = agreement;
        }

        /// <summary>
        /// Gets a transportation agreement by ID.
        /// </summary>
        public TransportationAgreement? GetTransportationAgreement(string agreementId)
        {
            return transportationAgreements.TryGetValue(agreementId, out var agreement) ? agreement : null;
        }

        /// <summary>
        /// Calculates total working interest for a lease.
        /// </summary>
        public decimal CalculateTotalWorkingInterest(string leaseId)
        {
            var lease = GetLease(leaseId);
            if (lease == null)
                return 0;

            if (lease is JointInterestLease jointLease)
            {
                return jointLease.Participants.Sum(p => p.WorkingInterest);
            }

            return lease.WorkingInterest;
        }

        /// <summary>
        /// Calculates total net revenue interest for a lease.
        /// </summary>
        public decimal CalculateTotalNetRevenueInterest(string leaseId)
        {
            var lease = GetLease(leaseId);
            if (lease == null)
                return 0;

            if (lease is JointInterestLease jointLease)
            {
                return jointLease.Participants.Sum(p => p.NetRevenueInterest);
            }

            return lease.NetRevenueInterest;
        }
    }
}

