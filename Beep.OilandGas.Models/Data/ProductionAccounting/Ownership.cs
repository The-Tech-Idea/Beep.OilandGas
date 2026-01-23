using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    // NOTE: DivisionOrder, OwnerInformation, and OwnershipInterest are defined in OwnershipModelsDto.cs
    // This file contains additional DTOs and request classes for ownership operations.

    /// <summary>
    /// DTO for ownership tree node
    /// </summary>
    public class OwnershipTreeNode : ModelEntityBase
    {
        private string NodeIdValue = string.Empty;

        public string NodeId

        {

            get { return this.NodeIdValue; }

            set { SetProperty(ref NodeIdValue, value); }

        }
        private string NodeNameValue = string.Empty;

        public string NodeName

        {

            get { return this.NodeNameValue; }

            set { SetProperty(ref NodeNameValue, value); }

        }
        private string NodeTypeValue = string.Empty;

        public string NodeType

        {

            get { return this.NodeTypeValue; }

            set { SetProperty(ref NodeTypeValue, value); }

        }
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private List<OwnershipTreeNode> ChildrenValue = new();

        public List<OwnershipTreeNode> Children

        {

            get { return this.ChildrenValue; }

            set { SetProperty(ref ChildrenValue, value); }

        }
    }

    /// <summary>
    /// Request to register ownership interest
    /// </summary>
    public class RegisterOwnershipInterestRequest : ModelEntityBase
    {
        private string PropertyOrLeaseIdValue = string.Empty;

        [Required]
        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }
        private OwnerInformation OwnerValue = new();

        [Required]
        public OwnerInformation Owner

        {

            get { return this.OwnerValue; }

            set { SetProperty(ref OwnerValue, value); }

        }
        private decimal WorkingInterestValue;

        [Required]
        [Range(0, 1)]
        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal NetRevenueInterestValue;

        [Required]
        [Range(0, 1)]
        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private decimal? RoyaltyInterestValue;

        [Range(0, 1)]
        public decimal? RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }
        private decimal? OverridingRoyaltyInterestValue;

        [Range(0, 1)]
        public decimal? OverridingRoyaltyInterest

        {

            get { return this.OverridingRoyaltyInterestValue; }

            set { SetProperty(ref OverridingRoyaltyInterestValue, value); }

        }
        private DateTime EffectiveDateValue;

        [Required]
        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
    }
}








