using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Accounting.Exceptions;

namespace Beep.OilandGas.Accounting.JointVenture.WorkingInterest
{
    /// <summary>
    /// Manages ownership, division orders, and transfers.
    /// </summary>
    public class OwnershipManager
    {
        private readonly Dictionary<string, DivisionOrder> divisionOrders = new();
        private readonly Dictionary<string, TransferOrder> transferOrders = new();
        private readonly Dictionary<string, OwnershipInterest> ownershipInterests = new();
        private readonly Dictionary<string, OwnershipTree> ownershipTrees = new();

        /// <summary>
        /// Creates a division order.
        /// </summary>
        public DivisionOrder CreateDivisionOrder(
            string propertyOrLeaseId,
            OwnerInformation owner,
            decimal workingInterest,
            decimal netRevenueInterest,
            DateTime effectiveDate)
        {
            if (string.IsNullOrEmpty(propertyOrLeaseId))
                throw new ArgumentException("Property or lease ID cannot be null or empty.", nameof(propertyOrLeaseId));

            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            if (workingInterest < 0 || workingInterest > 1)
                throw new ArgumentException("Working interest must be between 0 and 1.", nameof(workingInterest));

            if (netRevenueInterest < 0 || netRevenueInterest > 1)
                throw new ArgumentException("Net revenue interest must be between 0 and 1.", nameof(netRevenueInterest));

            if (netRevenueInterest > workingInterest)
                throw new ArgumentException("Net revenue interest cannot exceed working interest.", nameof(netRevenueInterest));

            var divisionOrder = new DivisionOrder
            {
                DivisionOrderId = Guid.NewGuid().ToString(),
                PropertyOrLeaseId = propertyOrLeaseId,
                Owner = owner,
                WorkingInterest = workingInterest,
                NetRevenueInterest = netRevenueInterest,
                EffectiveDate = effectiveDate,
                Status = DivisionOrderStatus.Pending
            };

            divisionOrders[divisionOrder.DivisionOrderId] = divisionOrder;
            return divisionOrder;
        }

        /// <summary>
        /// Approves a division order.
        /// </summary>
        public void ApproveDivisionOrder(string divisionOrderId, string approvedBy)
        {
            if (!divisionOrders.TryGetValue(divisionOrderId, out var divisionOrder))
                throw new ArgumentException($"Division order {divisionOrderId} not found.", nameof(divisionOrderId));

            divisionOrder.Status = DivisionOrderStatus.Approved;
            divisionOrder.ApprovalDate = DateTime.Now;
            divisionOrder.ApprovedBy = approvedBy;

            // Create or update ownership interest
            var ownershipInterest = new OwnershipInterest
            {
                OwnershipId = Guid.NewGuid().ToString(),
                OwnerId = divisionOrder.Owner.OwnerId,
                PropertyOrLeaseId = divisionOrder.PropertyOrLeaseId,
                WorkingInterest = divisionOrder.WorkingInterest,
                NetRevenueInterest = divisionOrder.NetRevenueInterest,
                RoyaltyInterest = divisionOrder.RoyaltyInterest,
                OverridingRoyaltyInterest = divisionOrder.OverridingRoyaltyInterest,
                EffectiveStartDate = divisionOrder.EffectiveDate,
                EffectiveEndDate = divisionOrder.ExpirationDate,
                DivisionOrderId = divisionOrderId
            };

            ownershipInterests[ownershipInterest.OwnershipId] = ownershipInterest;
        }

        /// <summary>
        /// Creates a transfer order.
        /// </summary>
        public TransferOrder CreateTransferOrder(
            string propertyOrLeaseId,
            OwnerInformation fromOwner,
            OwnerInformation toOwner,
            decimal interestTransferred,
            DateTime effectiveDate)
        {
            if (string.IsNullOrEmpty(propertyOrLeaseId))
                throw new ArgumentException("Property or lease ID cannot be null or empty.", nameof(propertyOrLeaseId));

            if (interestTransferred < 0 || interestTransferred > 1)
                throw new ArgumentException("Interest transferred must be between 0 and 1.", nameof(interestTransferred));

            var transferOrder = new TransferOrder
            {
                TransferOrderId = Guid.NewGuid().ToString(),
                PropertyOrLeaseId = propertyOrLeaseId,
                FromOwner = fromOwner,
                ToOwner = toOwner,
                InterestTransferred = interestTransferred,
                EffectiveDate = effectiveDate,
                IsApproved = false
            };

            transferOrders[transferOrder.TransferOrderId] = transferOrder;
            return transferOrder;
        }

        /// <summary>
        /// Approves a transfer order and updates ownership.
        /// </summary>
        public void ApproveTransferOrder(string transferOrderId, string approvedBy)
        {
            if (!transferOrders.TryGetValue(transferOrderId, out var transferOrder))
                throw new ArgumentException($"Transfer order {transferOrderId} not found.", nameof(transferOrderId));

            transferOrder.IsApproved = true;
            transferOrder.ApprovalDate = DateTime.Now;
            transferOrder.ApprovedBy = approvedBy;

            // Update ownership interests
            var fromInterests = ownershipInterests.Values
                .Where(o => o.PropertyOrLeaseId == transferOrder.PropertyOrLeaseId &&
                           o.OwnerId == transferOrder.FromOwner.OwnerId &&
                           (o.EffectiveEndDate == null || o.EffectiveEndDate >= transferOrder.EffectiveDate))
                .ToList();

            foreach (var interest in fromInterests)
            {
                // End the from owner's interest
                interest.EffectiveEndDate = transferOrder.EffectiveDate;

                // Create new interest for to owner
                var newInterest = new OwnershipInterest
                {
                    OwnershipId = Guid.NewGuid().ToString(),
                    OwnerId = transferOrder.ToOwner.OwnerId,
                    PropertyOrLeaseId = transferOrder.PropertyOrLeaseId,
                    WorkingInterest = interest.WorkingInterest * transferOrder.InterestTransferred,
                    NetRevenueInterest = interest.NetRevenueInterest * transferOrder.InterestTransferred,
                    EffectiveStartDate = transferOrder.EffectiveDate
                };

                ownershipInterests[newInterest.OwnershipId] = newInterest;
            }
        }

        /// <summary>
        /// Gets ownership interests for a property or lease.
        /// </summary>
        public IEnumerable<OwnershipInterest> GetOwnershipInterests(string propertyOrLeaseId, DateTime asOfDate)
        {
            return ownershipInterests.Values
                .Where(o => o.PropertyOrLeaseId == propertyOrLeaseId &&
                           o.EffectiveStartDate <= asOfDate &&
                           (o.EffectiveEndDate == null || o.EffectiveEndDate >= asOfDate));
        }

        /// <summary>
        /// Calculates total working interest for a property or lease.
        /// </summary>
        public decimal CalculateTotalWorkingInterest(string propertyOrLeaseId, DateTime asOfDate)
        {
            return GetOwnershipInterests(propertyOrLeaseId, asOfDate)
                .Sum(o => o.WorkingInterest);
        }

        /// <summary>
        /// Calculates total net revenue interest for a property or lease.
        /// </summary>
        public decimal CalculateTotalNetRevenueInterest(string propertyOrLeaseId, DateTime asOfDate)
        {
            return GetOwnershipInterests(propertyOrLeaseId, asOfDate)
                .Sum(o => o.NetRevenueInterest);
        }

        /// <summary>
        /// Gets a division order by ID.
        /// </summary>
        public DivisionOrder? GetDivisionOrder(string divisionOrderId)
        {
            return divisionOrders.TryGetValue(divisionOrderId, out var order) ? order : null;
        }

        /// <summary>
        /// Gets a transfer order by ID.
        /// </summary>
        public TransferOrder? GetTransferOrder(string transferOrderId)
        {
            return transferOrders.TryGetValue(transferOrderId, out var order) ? order : null;
        }
    }
}

