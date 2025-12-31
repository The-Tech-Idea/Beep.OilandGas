
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Validation
{
    /// <summary>
    /// Validates lease agreements and related data.
    /// </summary>
    public static class LeaseValidator
    {
        /// <summary>
        /// Validates a lease agreement.
        /// </summary>
        public static void Validate(LeaseAgreement lease)
        {
            if (lease == null)
                throw new ArgumentNullException(nameof(lease));

            if (string.IsNullOrEmpty(lease.LeaseId))
                throw new InvalidLeaseDataException(nameof(lease.LeaseId), "Lease ID cannot be null or empty.");

            ValidateInterest(lease.WorkingInterest, nameof(lease.WorkingInterest));
            ValidateInterest(lease.NetRevenueInterest, nameof(lease.NetRevenueInterest));
            ValidateInterest(lease.RoyaltyRate, nameof(lease.RoyaltyRate));

            if (lease.WorkingInterest < lease.NetRevenueInterest)
                throw new InvalidLeaseDataException(nameof(lease.NetRevenueInterest),
                    "Net revenue interest cannot exceed working interest.");

            if (lease.ExpirationDate.HasValue && lease.ExpirationDate < lease.EffectiveDate)
                throw new InvalidLeaseDataException(nameof(lease.ExpirationDate),
                    "Expiration date cannot be before effective date.");

            if (lease.PrimaryTermMonths < 0)
                throw new InvalidLeaseDataException(nameof(lease.PrimaryTermMonths),
                    "Primary term cannot be negative.");

            // Validate specific lease types
            if (lease is JointInterestLease jointLease)
            {
                ValidateJointInterestLease(jointLease);
            }
            else if (lease is NetProfitLease netProfitLease)
            {
                ValidateNetProfitLease(netProfitLease);
            }
        }

        /// <summary>
        /// Validates a joint interest lease.
        /// </summary>
        private static void ValidateJointInterestLease(JointInterestLease lease)
        {
            if (string.IsNullOrEmpty(lease.Operator))
                throw new InvalidLeaseDataException(nameof(lease.Operator), "Operator cannot be null or empty.");

            if (lease.Participants == null || lease.Participants.Count == 0)
                throw new InvalidLeaseDataException(nameof(lease.Participants), "Joint interest lease must have at least one participant.");

            decimal totalWorkingInterest = lease.Participants.Sum(p => p.WorkingInterest);
            if (Math.Abs(totalWorkingInterest - 1.0m) > 0.001m)
                throw new InvalidLeaseDataException(nameof(lease.Participants),
                    $"Total working interest must equal 1.0, but is {totalWorkingInterest}.");

            bool hasOperator = lease.Participants.Any(p => p.IsOperator);
            if (!hasOperator)
                throw new InvalidLeaseDataException(nameof(lease.Participants), "Joint interest lease must have one operator participant.");
        }

        /// <summary>
        /// Validates a net profit lease.
        /// </summary>
        private static void ValidateNetProfitLease(NetProfitLease lease)
        {
            ValidateInterest(lease.NetProfitInterest, nameof(lease.NetProfitInterest));
        }

        /// <summary>
        /// Validates an interest percentage.
        /// </summary>
        private static void ValidateInterest(decimal interest, string parameterName)
        {
            if (interest < 0 || interest > 1)
                throw new InvalidLeaseDataException(parameterName,
                    $"Interest must be between 0 and 1, but is {interest}.");
        }
    }
}
