using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
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
}
