using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Storage
{
    public class CreateServiceUnitRequest : ModelEntityBase
    {
        private string UnitNameValue;

        public string UnitName

        {

            get { return this.UnitNameValue; }

            set { SetProperty(ref UnitNameValue, value); }

        }
        private string UnitTypeValue;

        public string UnitType

        {

            get { return this.UnitTypeValue; }

            set { SetProperty(ref UnitTypeValue, value); }

        }
        private string LeaseIdValue;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string TankBatteryIdValue;

        public string TankBatteryId

        {

            get { return this.TankBatteryIdValue; }

            set { SetProperty(ref TankBatteryIdValue, value); }

        }
        private string OperatorBaIdValue;

        public string OperatorBaId

        {

            get { return this.OperatorBaIdValue; }

            set { SetProperty(ref OperatorBaIdValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
    }
}
