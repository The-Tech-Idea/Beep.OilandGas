using System;

namespace Beep.OilandGas.Models.DTOs.Accounting.Royalty
{
    /// <summary>
    /// Request DTO for creating a royalty payment
    /// </summary>
    public class CreateRoyaltyPaymentRequest
    {
        public string RoyaltyOwnerId { get; set; } = string.Empty;
        public decimal RoyaltyAmount { get; set; }
        public decimal? RoyaltyInterest { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    /// <summary>
    /// Request DTO for calculating royalty
    /// </summary>
    public class CalculateRoyaltyRequest
    {
        public string? FieldId { get; set; }
        public string? PoolId { get; set; }
        public DateTime CalculationDate { get; set; }
        public decimal? OilRoyaltyRate { get; set; }
        public decimal? GasRoyaltyRate { get; set; }
        public decimal? OilPrice { get; set; }
        public decimal? GasPrice { get; set; }
    }
}



