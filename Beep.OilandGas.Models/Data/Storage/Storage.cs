using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Storage
{
    public class CreateStorageFacilityRequest : ModelEntityBase
    {
        private string FacilityNameValue;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private string FacilityTypeValue;

        public string FacilityType

        {

            get { return this.FacilityTypeValue; }

            set { SetProperty(ref FacilityTypeValue, value); }

        }
        private string LocationValue;

        public string Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
        private decimal CapacityValue;

        public decimal Capacity

        {

            get { return this.CapacityValue; }

            set { SetProperty(ref CapacityValue, value); }

        }
    }

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

    public class CreateLACTUnitRequest : ModelEntityBase
    {
        private string LactNameValue;

        public string LactName

        {

            get { return this.LactNameValue; }

            set { SetProperty(ref LactNameValue, value); }

        }
        private string ServiceUnitIdValue;

        public string ServiceUnitId

        {

            get { return this.ServiceUnitIdValue; }

            set { SetProperty(ref ServiceUnitIdValue, value); }

        }
        private string MeterTypeValue;

        public string MeterType

        {

            get { return this.MeterTypeValue; }

            set { SetProperty(ref MeterTypeValue, value); }

        }
        private decimal MaximumFlowRateValue;

        public decimal MaximumFlowRate

        {

            get { return this.MaximumFlowRateValue; }

            set { SetProperty(ref MaximumFlowRateValue, value); }

        }
        private decimal MeterFactorValue = 1.0m;

        public decimal MeterFactor

        {

            get { return this.MeterFactorValue; }

            set { SetProperty(ref MeterFactorValue, value); }

        }
    }

    public class StorageCapacitySummary : ModelEntityBase
    {
        private string StorageFacilityIdValue;

        public string StorageFacilityId

        {

            get { return this.StorageFacilityIdValue; }

            set { SetProperty(ref StorageFacilityIdValue, value); }

        }
        private string FacilityNameValue;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private decimal TotalCapacityValue;

        public decimal TotalCapacity

        {

            get { return this.TotalCapacityValue; }

            set { SetProperty(ref TotalCapacityValue, value); }

        }
        private decimal CurrentInventoryValue;

        public decimal CurrentInventory

        {

            get { return this.CurrentInventoryValue; }

            set { SetProperty(ref CurrentInventoryValue, value); }

        }
        private decimal AvailableCapacityValue;

        public decimal AvailableCapacity

        {

            get { return this.AvailableCapacityValue; }

            set { SetProperty(ref AvailableCapacityValue, value); }

        }
        private decimal UtilizationPercentageValue;

        public decimal UtilizationPercentage

        {

            get { return this.UtilizationPercentageValue; }

            set { SetProperty(ref UtilizationPercentageValue, value); }

        }
        private int TankBatteryCountValue;

        public int TankBatteryCount

        {

            get { return this.TankBatteryCountValue; }

            set { SetProperty(ref TankBatteryCountValue, value); }

        }
    }

    public class StorageUtilizationReport : ModelEntityBase
    {
        private string StorageFacilityIdValue;

        public string StorageFacilityId

        {

            get { return this.StorageFacilityIdValue; }

            set { SetProperty(ref StorageFacilityIdValue, value); }

        }
        private DateTime AsOfDateValue;

        public DateTime AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
        private decimal TotalCapacityValue;

        public decimal TotalCapacity

        {

            get { return this.TotalCapacityValue; }

            set { SetProperty(ref TotalCapacityValue, value); }

        }
        private decimal CurrentInventoryValue;

        public decimal CurrentInventory

        {

            get { return this.CurrentInventoryValue; }

            set { SetProperty(ref CurrentInventoryValue, value); }

        }
        private decimal AvailableCapacityValue;

        public decimal AvailableCapacity

        {

            get { return this.AvailableCapacityValue; }

            set { SetProperty(ref AvailableCapacityValue, value); }

        }
        private decimal UtilizationPercentageValue;

        public decimal UtilizationPercentage

        {

            get { return this.UtilizationPercentageValue; }

            set { SetProperty(ref UtilizationPercentageValue, value); }

        }
        private List<StorageUtilizationDetail> DetailsValue = new List<StorageUtilizationDetail>();

        public List<StorageUtilizationDetail> Details

        {

            get { return this.DetailsValue; }

            set { SetProperty(ref DetailsValue, value); }

        }
    }

    public class StorageUtilizationDetail : ModelEntityBase
    {
        private string TankBatteryIdValue;

        public string TankBatteryId

        {

            get { return this.TankBatteryIdValue; }

            set { SetProperty(ref TankBatteryIdValue, value); }

        }
        private string BatteryNameValue;

        public string BatteryName

        {

            get { return this.BatteryNameValue; }

            set { SetProperty(ref BatteryNameValue, value); }

        }
        private decimal CapacityValue;

        public decimal Capacity

        {

            get { return this.CapacityValue; }

            set { SetProperty(ref CapacityValue, value); }

        }
        private decimal CurrentInventoryValue;

        public decimal CurrentInventory

        {

            get { return this.CurrentInventoryValue; }

            set { SetProperty(ref CurrentInventoryValue, value); }

        }
        private decimal UtilizationPercentageValue;

        public decimal UtilizationPercentage

        {

            get { return this.UtilizationPercentageValue; }

            set { SetProperty(ref UtilizationPercentageValue, value); }

        }
    }
}








