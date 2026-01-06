namespace Beep.OilandGas.Models.DTOs.Accounting
{
    public class InventoryItemDto
    {
        public string InventoryItemId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal? QuantityOnHand { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? TotalValue { get; set; }
        public string ValuationMethod { get; set; }
    }

    public class CreateInventoryItemRequest
    {
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal? UnitCost { get; set; }
        public string ValuationMethod { get; set; }
    }

    public class InventoryItemResponse : InventoryItemDto
    {
    }
}




