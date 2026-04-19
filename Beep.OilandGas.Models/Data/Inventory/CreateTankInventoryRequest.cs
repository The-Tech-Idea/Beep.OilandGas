using System;

namespace Beep.OilandGas.Models.Data.Inventory
{
    public class CreateTankInventoryRequest : ModelEntityBase
    {
        private decimal OpeningInventoryValue;
        private decimal ReceiptsValue;
        private decimal DeliveriesValue;
        private decimal ActualClosingInventoryValue;

        public string TankId { get; set; } = string.Empty;
        public string FacilityId { get; set; } = string.Empty;
        public decimal Volume { get; set; }
        public string VolumeUnit { get; set; } = "BBL";
        public decimal? Temperature { get; set; }
        public decimal? Pressure { get; set; }
        public DateTime? MeasurementDate { get; set; }
        public string ProductType { get; set; } = string.Empty;

        public string TankBatteryId
        {
            get => TankId;
            set => TankId = value;
        }

        public DateTime? InventoryDate
        {
            get => MeasurementDate;
            set => MeasurementDate = value;
        }

        public decimal OpeningInventory
        {
            get => OpeningInventoryValue;
            set => SetProperty(ref OpeningInventoryValue, value);
        }

        public decimal Receipts
        {
            get => ReceiptsValue;
            set => SetProperty(ref ReceiptsValue, value);
        }

        public decimal Deliveries
        {
            get => DeliveriesValue;
            set => SetProperty(ref DeliveriesValue, value);
        }

        public decimal ActualClosingInventory
        {
            get => ActualClosingInventoryValue;
            set => SetProperty(ref ActualClosingInventoryValue, value);
        }
    }
}
