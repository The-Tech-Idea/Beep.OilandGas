using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Represents a sales statement.
    /// </summary>
    public partial class SalesStatement : ModelEntityBase
    {
        private System.String StatementIdValue;
        public System.String StatementId
        {
            get { return this.StatementIdValue; }
            set { SetProperty(ref StatementIdValue, value); }
        }

        private System.DateTime StatementPeriodStartValue;
        public System.DateTime StatementPeriodStart
        {
            get { return this.StatementPeriodStartValue; }
            set { SetProperty(ref StatementPeriodStartValue, value); }
        }

        private System.DateTime StatementPeriodEndValue;
        public System.DateTime StatementPeriodEnd
        {
            get { return this.StatementPeriodEndValue; }
            set { SetProperty(ref StatementPeriodEndValue, value); }
        }

        private System.String? PropertyOrLeaseIdValue;
        public System.String? PropertyOrLeaseId
        {
            get { return this.PropertyOrLeaseIdValue; }
            set { SetProperty(ref PropertyOrLeaseIdValue, value); }
        }

        private SalesSummary SummaryValue = new SalesSummary();
        public SalesSummary Summary
        {
            get { return this.SummaryValue; }
            set { SetProperty(ref SummaryValue, value); }
        }

        private List<VolumeDetail> VolumeDetailsValue = new List<VolumeDetail>();
        public List<VolumeDetail> VolumeDetails
        {
            get { return this.VolumeDetailsValue; }
            set { SetProperty(ref VolumeDetailsValue, value); }
        }

        private List<PricingDetail> PricingDetailsValue = new List<PricingDetail>();
        public List<PricingDetail> PricingDetails
        {
            get { return this.PricingDetailsValue; }
            set { SetProperty(ref PricingDetailsValue, value); }
        }

        private List<SalesTransaction> TransactionsValue = new List<SalesTransaction>();
        public List<SalesTransaction> Transactions
        {
            get { return this.TransactionsValue; }
            set { SetProperty(ref TransactionsValue, value); }
        }
    }

    /// <summary>
    /// Represents a sales summary.
    /// </summary>
    public partial class SalesSummary : ModelEntityBase
    {
        private System.Decimal TotalNetVolumeValue;
        public System.Decimal TotalNetVolume
        {
            get { return this.TotalNetVolumeValue; }
            set { SetProperty(ref TotalNetVolumeValue, value); }
        }

        private System.Decimal AveragePricePerBarrelValue;
        public System.Decimal AveragePricePerBarrel
        {
            get { return this.AveragePricePerBarrelValue; }
            set { SetProperty(ref AveragePricePerBarrelValue, value); }
        }

        private System.Decimal TotalGrossRevenueValue;
        public System.Decimal TotalGrossRevenue
        {
            get { return this.TotalGrossRevenueValue; }
            set { SetProperty(ref TotalGrossRevenueValue, value); }
        }

        private System.Decimal TotalCostsValue;
        public System.Decimal TotalCosts
        {
            get { return this.TotalCostsValue; }
            set { SetProperty(ref TotalCostsValue, value); }
        }

        private System.Decimal TotalTaxesValue;
        public System.Decimal TotalTaxes
        {
            get { return this.TotalTaxesValue; }
            set { SetProperty(ref TotalTaxesValue, value); }
        }

        public System.Decimal TotalNetRevenue => TotalGrossRevenue - TotalCosts - TotalTaxes;

        private System.Int32 TransactionCountValue;
        public System.Int32 TransactionCount
        {
            get { return this.TransactionCountValue; }
            set { SetProperty(ref TransactionCountValue, value); }
        }
    }

    /// <summary>
    /// Represents volume details.
    /// </summary>
    public partial class VolumeDetail : ModelEntityBase
    {
        private System.DateTime DateValue;
        public System.DateTime Date
        {
            get { return this.DateValue; }
            set { SetProperty(ref DateValue, value); }
        }

        private System.Decimal NetVolumeValue;
        public System.Decimal NetVolume
        {
            get { return this.NetVolumeValue; }
            set { SetProperty(ref NetVolumeValue, value); }
        }

        private System.String? RunTicketNumberValue;
        public System.String? RunTicketNumber
        {
            get { return this.RunTicketNumberValue; }
            set { SetProperty(ref RunTicketNumberValue, value); }
        }
    }

    /// <summary>
    /// Represents pricing details.
    /// </summary>
    public partial class PricingDetail : ModelEntityBase
    {
        private System.DateTime DateValue;
        public System.DateTime Date
        {
            get { return this.DateValue; }
            set { SetProperty(ref DateValue, value); }
        }

        private System.Decimal PricePerBarrelValue;
        public System.Decimal PricePerBarrel
        {
            get { return this.PricePerBarrelValue; }
            set { SetProperty(ref PricePerBarrelValue, value); }
        }

        private System.String PricingMethodValue = string.Empty;
        public System.String PricingMethod
        {
            get { return this.PricingMethodValue; }
            set { SetProperty(ref PricingMethodValue, value); }
        }

        private System.String? PriceIndexValue;
        public System.String? PriceIndex
        {
            get { return this.PriceIndexValue; }
            set { SetProperty(ref PriceIndexValue, value); }
        }
    }
}




