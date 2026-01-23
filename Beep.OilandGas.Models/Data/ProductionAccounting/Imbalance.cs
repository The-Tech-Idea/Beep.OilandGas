using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    // NOTE: Nomination, ActualDelivery, and ImbalanceAdjustment are defined in ImbalanceModelsDto.cs
    // This file contains request classes for imbalance operations.

    /// <summary>
    /// DTO for production avail
    /// </summary>
    public class ProductionAvail : ModelEntityBase
    {
        private string AvailIdValue = string.Empty;

        public string AvailId

        {

            get { return this.AvailIdValue; }

            set { SetProperty(ref AvailIdValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

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
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    /// <summary>
    /// DTO for imbalance
    /// </summary>
    public class Imbalance : ModelEntityBase
    {
        private string ImbalanceIdValue = string.Empty;

        public string ImbalanceId

        {

            get { return this.ImbalanceIdValue; }

            set { SetProperty(ref ImbalanceIdValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime ImbalanceDateValue;

        public DateTime ImbalanceDate

        {

            get { return this.ImbalanceDateValue; }

            set { SetProperty(ref ImbalanceDateValue, value); }

        }
        private decimal ProductionAvailValue;

        public decimal ProductionAvail

        {

            get { return this.ProductionAvailValue; }

            set { SetProperty(ref ProductionAvailValue, value); }

        }
        private decimal NominatedVolumeValue;

        public decimal NominatedVolume

        {

            get { return this.NominatedVolumeValue; }

            set { SetProperty(ref NominatedVolumeValue, value); }

        }
        private decimal ActualDeliveredValue;

        public decimal ActualDelivered

        {

            get { return this.ActualDeliveredValue; }

            set { SetProperty(ref ActualDeliveredValue, value); }

        }
        private decimal ImbalanceVolumeValue;

        public decimal ImbalanceVolume

        {

            get { return this.ImbalanceVolumeValue; }

            set { SetProperty(ref ImbalanceVolumeValue, value); }

        }
        private string ImbalanceTypeValue = string.Empty;

        public string ImbalanceType

        {

            get { return this.ImbalanceTypeValue; }

            set { SetProperty(ref ImbalanceTypeValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// Request to reconcile imbalance
    /// </summary>
    public class ReconcileImbalanceRequest : ModelEntityBase
    {
        private string ImbalanceIdValue = string.Empty;

        [Required]
        public string ImbalanceId

        {

            get { return this.ImbalanceIdValue; }

            set { SetProperty(ref ImbalanceIdValue, value); }

        }
        private List<ImbalanceAdjustment> AdjustmentsValue = new();

        [Required]
        public List<ImbalanceAdjustment> Adjustments

        {

            get { return this.AdjustmentsValue; }

            set { SetProperty(ref AdjustmentsValue, value); }

        }
        private string ReconciledByValue = string.Empty;

        [Required]
        public string ReconciledBy

        {

            get { return this.ReconciledByValue; }

            set { SetProperty(ref ReconciledByValue, value); }

        }
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}








