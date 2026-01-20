using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// DTO for sales transaction
    /// </summary>
    public class SalesTransaction : ModelEntityBase
    {
        public string TransactionId { get; set; } = string.Empty;
        public string? SalesAgreementId { get; set; }
        public string RunTicketNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Purchaser { get; set; } = string.Empty;
        public decimal NetVolume { get; set; }
        public decimal PricePerBarrel { get; set; }
        public decimal TotalValue { get; set; }
        public DeliveryInformation? Delivery { get; set; }
        public ProductionMarketingCosts? Costs { get; set; }
        public List<ProductionTax> Taxes { get; set; } = new();
        public decimal NetRevenue { get; set; }
    }

    /// <summary>
    /// DTO for delivery information
    /// </summary>
    public class DeliveryInformation : ModelEntityBase
    {
        public DateTime DeliveryDate { get; set; }
        public string DeliveryPoint { get; set; } = string.Empty;
        public string DeliveryMethod { get; set; } = string.Empty;
        public bool TitleTransferredAtDeliveryPoint { get; set; } = true;
    }

    /// <summary>
    /// DTO for production and marketing costs
    /// </summary>
    public class ProductionMarketingCosts : ModelEntityBase
    {
        public decimal LiftingCosts { get; set; }
        public decimal TransportationCosts { get; set; }
        public decimal ProcessingCosts { get; set; }
        public decimal MarketingCosts { get; set; }
        public decimal OtherCosts { get; set; }
        public decimal TotalCosts { get; set; }
    }

    /// <summary>
    /// DTO for production tax
    /// </summary>
    public class ProductionTax : ModelEntityBase
    {
        public string TaxType { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// DTO for receivable
    /// </summary>
    public class Receivable : ModelEntityBase
    {
        public string ReceivableId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Purchaser { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for inventory
    /// </summary>
    public class Inventory : ModelEntityBase
    {
        public string InventoryId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string? TankBatteryId { get; set; }
        public DateTime InventoryDate { get; set; }
        public decimal GrossVolume { get; set; }
        public decimal BSWVolume { get; set; }
        public decimal NetVolume { get; set; }
        public decimal? ApiGravity { get; set; }
    }

    /// <summary>
    /// Request to create sales transaction
    /// </summary>
    public class CreateSalesTransactionRequest : ModelEntityBase
    {
        [Required]
        public string RunTicketNumber { get; set; } = string.Empty;
        public string? SalesAgreementId { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        [Required]
        public string Purchaser { get; set; } = string.Empty;
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal NetVolume { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal PricePerBarrel { get; set; }
        public DeliveryInformation? Delivery { get; set; }
        public ProductionMarketingCosts? Costs { get; set; }
        public List<ProductionTax>? Taxes { get; set; }
    }
}





