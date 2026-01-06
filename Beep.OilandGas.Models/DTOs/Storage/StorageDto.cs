using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Storage
{
    public class CreateStorageFacilityRequest
    {
        public string FacilityName { get; set; }
        public string FacilityType { get; set; }
        public string Location { get; set; }
        public decimal Capacity { get; set; }
    }

    public class CreateTankBatteryRequest
    {
        public string BatteryName { get; set; }
        public string StorageFacilityId { get; set; }
        public string LeaseId { get; set; }
    }

    public class CreateServiceUnitRequest
    {
        public string UnitName { get; set; }
        public string UnitType { get; set; }
        public string LeaseId { get; set; }
        public string TankBatteryId { get; set; }
        public string OperatorBaId { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    public class CreateLACTUnitRequest
    {
        public string LactName { get; set; }
        public string ServiceUnitId { get; set; }
        public string MeterType { get; set; }
        public decimal MaximumFlowRate { get; set; }
        public decimal MeterFactor { get; set; } = 1.0m;
    }

    public class StorageCapacitySummary
    {
        public string StorageFacilityId { get; set; }
        public string FacilityName { get; set; }
        public decimal TotalCapacity { get; set; }
        public decimal CurrentInventory { get; set; }
        public decimal AvailableCapacity { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public int TankBatteryCount { get; set; }
    }

    public class StorageUtilizationReport
    {
        public string StorageFacilityId { get; set; }
        public DateTime AsOfDate { get; set; }
        public decimal TotalCapacity { get; set; }
        public decimal CurrentInventory { get; set; }
        public decimal AvailableCapacity { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public List<StorageUtilizationDetail> Details { get; set; } = new List<StorageUtilizationDetail>();
    }

    public class StorageUtilizationDetail
    {
        public string TankBatteryId { get; set; }
        public string BatteryName { get; set; }
        public decimal Capacity { get; set; }
        public decimal CurrentInventory { get; set; }
        public decimal UtilizationPercentage { get; set; }
    }
}




