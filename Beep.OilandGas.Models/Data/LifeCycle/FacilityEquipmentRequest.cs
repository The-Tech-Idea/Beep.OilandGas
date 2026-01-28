using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class FacilityEquipmentRequest : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string EquipmentTypeValue = string.Empty;

        public string EquipmentType

        {

            get { return this.EquipmentTypeValue; }

            set { SetProperty(ref EquipmentTypeValue, value); }

        }
        private string? EquipmentNameValue;

        public string? EquipmentName

        {

            get { return this.EquipmentNameValue; }

            set { SetProperty(ref EquipmentNameValue, value); }

        }
        private string? ManufacturerValue;

        public string? Manufacturer

        {

            get { return this.ManufacturerValue; }

            set { SetProperty(ref ManufacturerValue, value); }

        }
        private string? ModelValue;

        public string? Model

        {

            get { return this.ModelValue; }

            set { SetProperty(ref ModelValue, value); }

        }
        private DateTime? InstallationDateValue;

        public DateTime? InstallationDate

        {

            get { return this.InstallationDateValue; }

            set { SetProperty(ref InstallationDateValue, value); }

        }
        public Dictionary<string, object>? EquipmentData { get; set; }
    }
}
