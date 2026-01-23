using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Imbalance
{
    public class CreateProductionAvailRequest : ModelEntityBase
    {
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private DateTime AvailDateValue;

        public DateTime AvailDate

        {

            get { return this.AvailDateValue; }

            set { SetProperty(ref AvailDateValue, value); }

        }
        private decimal EstimatedVolumeValue;

        public decimal EstimatedVolume

        {

            get { return this.EstimatedVolumeValue; }

            set { SetProperty(ref EstimatedVolumeValue, value); }

        }
        private decimal? AvailableForDeliveryValue;

        public decimal? AvailableForDelivery

        {

            get { return this.AvailableForDeliveryValue; }

            set { SetProperty(ref AvailableForDeliveryValue, value); }

        }
    }

    public class CreateNominationRequest : ModelEntityBase
    {
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private decimal NominatedVolumeValue;

        public decimal NominatedVolume

        {

            get { return this.NominatedVolumeValue; }

            set { SetProperty(ref NominatedVolumeValue, value); }

        }
        private List<string> DeliveryPointsValue = new();

        public List<string> DeliveryPoints

        {

            get { return this.DeliveryPointsValue; }

            set { SetProperty(ref DeliveryPointsValue, value); }

        }
    }

    public class CreateActualDeliveryRequest : ModelEntityBase
    {
        private string NominationIdValue;

        public string NominationId

        {

            get { return this.NominationIdValue; }

            set { SetProperty(ref NominationIdValue, value); }

        }
        private DateTime DeliveryDateValue;

        public DateTime DeliveryDate

        {

            get { return this.DeliveryDateValue; }

            set { SetProperty(ref DeliveryDateValue, value); }

        }
        private decimal ActualVolumeValue;

        public decimal ActualVolume

        {

            get { return this.ActualVolumeValue; }

            set { SetProperty(ref ActualVolumeValue, value); }

        }
        private string DeliveryPointValue;

        public string DeliveryPoint

        {

            get { return this.DeliveryPointValue; }

            set { SetProperty(ref DeliveryPointValue, value); }

        }
        private string AllocationMethodValue;

        public string AllocationMethod

        {

            get { return this.AllocationMethodValue; }

            set { SetProperty(ref AllocationMethodValue, value); }

        }
        private string RunTicketNumberValue;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
    }

    public class ImbalanceReconciliationResult : ModelEntityBase
    {
        private string ReconciliationIdValue;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private string ImbalanceIdValue;

        public string ImbalanceId

        {

            get { return this.ImbalanceIdValue; }

            set { SetProperty(ref ImbalanceIdValue, value); }

        }
        private decimal ImbalanceBeforeValue;

        public decimal ImbalanceBefore

        {

            get { return this.ImbalanceBeforeValue; }

            set { SetProperty(ref ImbalanceBeforeValue, value); }

        }
        private decimal ImbalanceAfterValue;

        public decimal ImbalanceAfter

        {

            get { return this.ImbalanceAfterValue; }

            set { SetProperty(ref ImbalanceAfterValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
        private DateTime ReconciliationDateValue = DateTime.UtcNow;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private string ReconciledByValue;

        public string ReconciledBy

        {

            get { return this.ReconciledByValue; }

            set { SetProperty(ref ReconciledByValue, value); }

        }
    }

    public class ImbalanceSettlementResult : ModelEntityBase
    {
        private string SettlementIdValue;

        public string SettlementId

        {

            get { return this.SettlementIdValue; }

            set { SetProperty(ref SettlementIdValue, value); }

        }
        private string ImbalanceIdValue;

        public string ImbalanceId

        {

            get { return this.ImbalanceIdValue; }

            set { SetProperty(ref ImbalanceIdValue, value); }

        }
        private DateTime SettlementDateValue;

        public DateTime SettlementDate

        {

            get { return this.SettlementDateValue; }

            set { SetProperty(ref SettlementDateValue, value); }

        }
        private decimal SettlementAmountValue;

        public decimal SettlementAmount

        {

            get { return this.SettlementAmountValue; }

            set { SetProperty(ref SettlementAmountValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string SettledByValue;

        public string SettledBy

        {

            get { return this.SettledByValue; }

            set { SetProperty(ref SettledByValue, value); }

        }
    }

    public class ImbalanceSummary : ModelEntityBase
    {
        private string PeriodStartValue;

        public string PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private string PeriodEndValue;

        public string PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private decimal TotalNominatedVolumeValue;

        public decimal TotalNominatedVolume

        {

            get { return this.TotalNominatedVolumeValue; }

            set { SetProperty(ref TotalNominatedVolumeValue, value); }

        }
        private decimal TotalActualVolumeValue;

        public decimal TotalActualVolume

        {

            get { return this.TotalActualVolumeValue; }

            set { SetProperty(ref TotalActualVolumeValue, value); }

        }
        private decimal TotalImbalanceAmountValue;

        public decimal TotalImbalanceAmount

        {

            get { return this.TotalImbalanceAmountValue; }

            set { SetProperty(ref TotalImbalanceAmountValue, value); }

        }
        private int ImbalanceCountValue;

        public int ImbalanceCount

        {

            get { return this.ImbalanceCountValue; }

            set { SetProperty(ref ImbalanceCountValue, value); }

        }
        private int BalancedCountValue;

        public int BalancedCount

        {

            get { return this.BalancedCountValue; }

            set { SetProperty(ref BalancedCountValue, value); }

        }
        private int OverDeliveredCountValue;

        public int OverDeliveredCount

        {

            get { return this.OverDeliveredCountValue; }

            set { SetProperty(ref OverDeliveredCountValue, value); }

        }
        private int UnderDeliveredCountValue;

        public int UnderDeliveredCount

        {

            get { return this.UnderDeliveredCountValue; }

            set { SetProperty(ref UnderDeliveredCountValue, value); }

        }
    }
}








