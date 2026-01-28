using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpHistory : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private DateTime DateRecordValue;

        public DateTime DateRecord

        {

            get { return this.DateRecordValue; }

            set { SetProperty(ref DateRecordValue, value); }

        }
        private string EventDescriptionValue = string.Empty;

        public string EventDescription

        {

            get { return this.EventDescriptionValue; }

            set { SetProperty(ref EventDescriptionValue, value); }

        }
    }
}
