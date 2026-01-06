using System;

namespace Beep.OilandGas.Models.DTOs.Accounting.Production
{
    /// <summary>
    /// Request DTO for creating tank inventory
    /// </summary>
    public class CreateTankInventoryRequest
    {
        public string TankBatteryId { get; set; } = string.Empty;
        public DateTime? InventoryDate { get; set; }
        public decimal OpeningInventory { get; set; }
        public decimal Receipts { get; set; }
        public decimal Deliveries { get; set; }
        public decimal? ActualClosingInventory { get; set; }
    }
}



