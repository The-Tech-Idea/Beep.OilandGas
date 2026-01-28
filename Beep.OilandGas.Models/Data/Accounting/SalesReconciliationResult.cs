using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class SalesReconciliationResult : ModelEntityBase
    {
        private string ReconciliationIdValue;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private decimal TotalProductionVolumeValue;

        public decimal TotalProductionVolume

        {

            get { return this.TotalProductionVolumeValue; }

            set { SetProperty(ref TotalProductionVolumeValue, value); }

        }
        private decimal TotalSalesVolumeValue;

        public decimal TotalSalesVolume

        {

            get { return this.TotalSalesVolumeValue; }

            set { SetProperty(ref TotalSalesVolumeValue, value); }

        }
        private decimal VolumeDifferenceValue;

        public decimal VolumeDifference

        {

            get { return this.VolumeDifferenceValue; }

            set { SetProperty(ref VolumeDifferenceValue, value); }

        }
        private decimal TotalProductionRevenueValue;

        public decimal TotalProductionRevenue

        {

            get { return this.TotalProductionRevenueValue; }

            set { SetProperty(ref TotalProductionRevenueValue, value); }

        }
        private decimal TotalSalesRevenueValue;

        public decimal TotalSalesRevenue

        {

            get { return this.TotalSalesRevenueValue; }

            set { SetProperty(ref TotalSalesRevenueValue, value); }

        }
        private decimal RevenueDifferenceValue;

        public decimal RevenueDifference

        {

            get { return this.RevenueDifferenceValue; }

            set { SetProperty(ref RevenueDifferenceValue, value); }

        }
        private List<SalesReconciliationIssue> IssuesValue = new List<SalesReconciliationIssue>();

        public List<SalesReconciliationIssue> Issues

        {

            get { return this.IssuesValue; }

            set { SetProperty(ref IssuesValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
    }
}
