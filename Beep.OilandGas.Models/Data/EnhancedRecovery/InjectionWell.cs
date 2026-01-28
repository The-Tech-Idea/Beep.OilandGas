using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class InjectionWell : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private string InjectionTypeValue = string.Empty;

        public string InjectionType

        {

            get { return this.InjectionTypeValue; }

            set { SetProperty(ref InjectionTypeValue, value); }

        }
        private string? InjectionZoneValue;

        public string? InjectionZone

        {

            get { return this.InjectionZoneValue; }

            set { SetProperty(ref InjectionZoneValue, value); }

        }
        private decimal? InjectionRateValue;

        public decimal? InjectionRate

        {

            get { return this.InjectionRateValue; }

            set { SetProperty(ref InjectionRateValue, value); }

        }
        private string? InjectionRateUnitValue;

        public string? InjectionRateUnit

        {

            get { return this.InjectionRateUnitValue; }

            set { SetProperty(ref InjectionRateUnitValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
