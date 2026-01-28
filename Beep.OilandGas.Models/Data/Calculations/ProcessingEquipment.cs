using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProcessingEquipment : ModelEntityBase
    {
        private string EquipmentTypeValue;

        public string EquipmentType

        {

            get { return this.EquipmentTypeValue; }

            set { SetProperty(ref EquipmentTypeValue, value); }

        }
        private string SizeValue;

        public string Size

        {

            get { return this.SizeValue; }

            set { SetProperty(ref SizeValue, value); }

        }
        private double CapacityValue;

        public double Capacity

        {

            get { return this.CapacityValue; }

            set { SetProperty(ref CapacityValue, value); }

        }
    }
}
