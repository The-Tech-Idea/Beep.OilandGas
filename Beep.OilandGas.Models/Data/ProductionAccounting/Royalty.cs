using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// DTO for royalty interest
    /// </summary>
    public class RoyaltyInterest : ModelEntityBase
    {
        public string RoyaltyInterestId { get; set; } = string.Empty;
        public string RoyaltyOwnerId { get; set; } = string.Empty;
        public string PropertyOrLeaseId { get; set; } = string.Empty;
        public decimal InterestPercentage { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? DivisionOrderId { get; set; }
    }

    /// <summary>
    /// DTO for royalty calculation
    /// </summary>
    public class RoyaltyCalculation : ModelEntityBase
    {
        public string CalculationId { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        public string PropertyOrLeaseId { get; set; } = string.Empty;
        public decimal GrossRevenue { get; set; }
        public RoyaltyDeductions? Deductions { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal RoyaltyInterest { get; set; }
        public decimal RoyaltyAmount { get; set; }
    }

    /// <summary>
    /// DTO for royalty deductions
    /// </summary>
    public class RoyaltyDeductions : ModelEntityBase
    {
        public decimal ProductionTaxes { get; set; }
        public decimal TransportationCosts { get; set; }
        public decimal ProcessingCosts { get; set; }
        public decimal MarketingCosts { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }
    }

    /// <summary>
    /// DTO for royalty payment
    /// </summary>
    public class RoyaltyPayment : ModelEntityBase
    {
        public string PaymentId { get; set; } = string.Empty;
        public string RoyaltyOwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal RoyaltyAmount { get; set; }
        public decimal TaxWithholdings { get; set; }
        public decimal NetPayment { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CheckNumber { get; set; }
        public DateTime? PaymentProcessedDate { get; set; }
    }

    /// <summary>
    /// DTO for royalty statement
    /// </summary>
    public class RoyaltyStatement : ModelEntityBase
    {
        public string StatementId { get; set; } = string.Empty;
        public string RoyaltyOwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime StatementPeriodStart { get; set; }
        public DateTime StatementPeriodEnd { get; set; }
        public ProductionSummary? ProductionSummary { get; set; }
        public RevenueSummary? RevenueSummary { get; set; }
        public DeductionsSummary? DeductionsSummary { get; set; }
        public decimal TotalRoyaltyAmount { get; set; }
    }

    /// <summary>
    /// DTO for production summary
    /// </summary>
    public class ProductionSummary : ModelEntityBase
    {
        public decimal GrossOilVolume { get; set; }
        public decimal GrossGasVolume { get; set; }
        public decimal NetOilVolume { get; set; }
        public decimal NetGasVolume { get; set; }
    }

    /// <summary>
    /// DTO for revenue summary
    /// </summary>
    public class RevenueSummary : ModelEntityBase
    {
        public decimal GrossRevenue { get; set; }
        public decimal NetRevenue { get; set; }
    }

    /// <summary>
    /// DTO for deductions summary
    /// </summary>
    public class DeductionsSummary : ModelEntityBase
    {
        public decimal TotalDeductions { get; set; }
        public decimal ProductionTaxes { get; set; }
        public decimal TransportationCosts { get; set; }
        public decimal ProcessingCosts { get; set; }
        public decimal MarketingCosts { get; set; }
    }

    /// <summary>
    /// DTO for 1099 form information
    /// </summary>
    public class Form1099Info : ModelEntityBase
    {
        public string TaxId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public int TaxYear { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalWithholdings { get; set; }
        public string? Address { get; set; }
    }

    /// <summary>
    /// Request to calculate royalty
    /// </summary>
    public class CalculateRoyaltyRequest : ModelEntityBase
    {
        [Required]
        public string PropertyOrLeaseId { get; set; } = string.Empty;
        [Required]
        public string RoyaltyOwnerId { get; set; } = string.Empty;
        [Required]
        public decimal GrossRevenue { get; set; }
        public RoyaltyDeductions? Deductions { get; set; }
        [Required]
        [Range(0, 1)]
        public decimal RoyaltyInterest { get; set; }
        [Required]
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Request to create royalty payment
    /// </summary>
    public class CreateRoyaltyPaymentRequest : ModelEntityBase
    {
        [Required]
        public string RoyaltyOwnerId { get; set; } = string.Empty;
        [Required]
        public decimal RoyaltyAmount { get; set; }
        public decimal? TaxWithholdings { get; set; }
        [Required]
        public DateTime PaymentDate { get; set; }
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}





