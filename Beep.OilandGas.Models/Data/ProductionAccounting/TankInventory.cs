using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Request to create a tank inventory class
    /// </summary>
    public class CreateTankInventoryRequest : ModelEntityBase
    {
        [Required]
        public string TankBatteryId { get; set; } = string.Empty;
        
        [Required]
        public DateTime InventoryDate { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal OpeningInventory { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Receipts { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Deliveries { get; set; }
        
        public decimal Adjustments { get; set; }
        
        public decimal Shrinkage { get; set; }
        
        public decimal TheftLoss { get; set; }
        
        public decimal? ActualClosingInventory { get; set; }
    }

    /// <summary>
    /// Response for tank inventory
    /// </summary>
    public class TankInventoryResponse : ModelEntityBase
    {
        public string InventoryId { get; set; } = string.Empty;
        public string TankBatteryId { get; set; } = string.Empty;
        public DateTime InventoryDate { get; set; }
        public decimal OpeningInventory { get; set; }
        public decimal Receipts { get; set; }
        public decimal Deliveries { get; set; }
        public decimal Adjustments { get; set; }
        public decimal Shrinkage { get; set; }
        public decimal TheftLoss { get; set; }
        public decimal ClosingInventory { get; set; }
        public decimal? ActualClosingInventory { get; set; }
        public decimal? InventoryVariance { get; set; }
    }
}






