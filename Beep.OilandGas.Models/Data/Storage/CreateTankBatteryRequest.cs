using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Storage
{
    public class CreateTankBatteryRequest : ModelEntityBase
    {
        private string BatteryNameValue;

        public string BatteryName

        {

            get { return this.BatteryNameValue; }

            set { SetProperty(ref BatteryNameValue, value); }

        }
        private string StorageFacilityIdValue;

        public string StorageFacilityId

        {

            get { return this.StorageFacilityIdValue; }

            set { SetProperty(ref StorageFacilityIdValue, value); }

        }
        private string LeaseIdValue;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
    }
}
