using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class StorageRequirementsResult : ModelEntityBase
    {
        private double CrudeOilStorageValue;

        public double CrudeOilStorage

        {

            get { return this.CrudeOilStorageValue; }

            set { SetProperty(ref CrudeOilStorageValue, value); }

        }
        private double CondensateStorageValue;

        public double CondensateStorage

        {

            get { return this.CondensateStorageValue; }

            set { SetProperty(ref CondensateStorageValue, value); }

        }
        private double GasStorageValue;

        public double GasStorage

        {

            get { return this.GasStorageValue; }

            set { SetProperty(ref GasStorageValue, value); }

        }
    }
}
