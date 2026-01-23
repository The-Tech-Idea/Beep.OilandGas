using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting.Revenue
{
    /// <summary>
    /// Request DTO for creating a revenue transaction
    /// </summary>
    public class CreateRevenueTransactionRequest : ModelEntityBase
    {
        private decimal RevenueAmountValue;

        public decimal RevenueAmount

        {

            get { return this.RevenueAmountValue; }

            set { SetProperty(ref RevenueAmountValue, value); }

        }
        private DateTime? TransactionDateValue;

        public DateTime? TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string? RunTicketNumberValue;

        public string? RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for revenue allocation
    /// </summary>
    public class RevenueAllocationRequest : ModelEntityBase
    {
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private decimal TotalRevenueValue;

        public decimal TotalRevenue

        {

            get { return this.TotalRevenueValue; }

            set { SetProperty(ref TotalRevenueValue, value); }

        }
        private DateTime AllocationDateValue;

        public DateTime AllocationDate

        {

            get { return this.AllocationDateValue; }

            set { SetProperty(ref AllocationDateValue, value); }

        }
        private string? RevenueTransactionIdValue;

        public string? RevenueTransactionId

        {

            get { return this.RevenueTransactionIdValue; }

            set { SetProperty(ref RevenueTransactionIdValue, value); }

        }
        private string? AllocationMethodValue;

        public string? AllocationMethod

        {

            get { return this.AllocationMethodValue; }

            set { SetProperty(ref AllocationMethodValue, value); }

        }
        private List<WorkingInterestRequest>? WorkingInterestsValue;

        public List<WorkingInterestRequest>? WorkingInterests

        {

            get { return this.WorkingInterestsValue; }

            set { SetProperty(ref WorkingInterestsValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for working interest
    /// </summary>
    public class WorkingInterestRequest : ModelEntityBase
    {
        private string OwnerIdValue = string.Empty;

        public string OwnerId

        {

            get { return this.OwnerIdValue; }

            set { SetProperty(ref OwnerIdValue, value); }

        }
        private decimal InterestPercentageValue;

        public decimal InterestPercentage

        {

            get { return this.InterestPercentageValue; }

            set { SetProperty(ref InterestPercentageValue, value); }

        }
    }
}







