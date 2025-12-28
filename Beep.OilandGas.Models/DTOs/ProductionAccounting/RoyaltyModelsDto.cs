//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
//{
//    /// <summary>
//    /// Payment method enumeration.
//    /// </summary>
//    public enum PaymentMethod
//    {
//        /// <summary>
//        /// Check payment.
//        /// </summary>
//        Check,

//        /// <summary>
//        /// Wire transfer.
//        /// </summary>
//        WireTransfer,

//        /// <summary>
//        /// ACH (Automated Clearing House).
//        /// </summary>
//        ACH,

//        /// <summary>
//        /// Direct deposit.
//        /// </summary>
//        DirectDeposit
//    }

//    /// <summary>
//    /// Payment status enumeration.
//    /// </summary>
//    public enum PaymentStatus
//    {
//        /// <summary>
//        /// Pending payment.
//        /// </summary>
//        Pending,

//        /// <summary>
//        /// Paid.
//        /// </summary>
//        Paid,

//        /// <summary>
//        /// Suspended.
//        /// </summary>
//        Suspended,

//        /// <summary>
//        /// Cancelled.
//        /// </summary>
//        Cancelled
//    }

//    /// <summary>
//    /// Tax withholding type enumeration.
//    /// </summary>
//    public enum TaxWithholdingType
//    {
//        /// <summary>
//        /// Invalid tax ID withholding.
//        /// </summary>
//        InvalidTaxId,

//        /// <summary>
//        /// Out of state withholding.
//        /// </summary>
//        OutOfState,

//        /// <summary>
//        /// Alien withholding.
//        /// </summary>
//        Alien,

//        /// <summary>
//        /// Backup withholding.
//        /// </summary>
//        BackupWithholding
//    }

//    /// <summary>
//    /// Represents a royalty interest (DTO for calculations/reporting).
//    /// </summary>
//    public class RoyaltyInterestDto
//    {
//        /// <summary>
//        /// Gets or sets the royalty interest identifier.
//        /// </summary>
//        public string RoyaltyInterestId { get; set; } = string.Empty;

//        /// <summary>
//        /// Gets or sets the royalty owner identifier.
//        /// </summary>
//        public string RoyaltyOwnerId { get; set; } = string.Empty;

//        /// <summary>
//        /// Gets or sets the property or lease identifier.
//        /// </summary>
//        public string PropertyOrLeaseId { get; set; } = string.Empty;

//        /// <summary>
//        /// Gets or sets the royalty interest percentage (decimal, 0-1).
//        /// </summary>
//        public decimal InterestPercentage { get; set; }

//        /// <summary>
//        /// Gets or sets the effective start date.
//        /// </summary>
//        public DateTime EffectiveStartDate { get; set; }

//        /// <summary>
//        /// Gets or sets the effective end date.
//        /// </summary>
//        public DateTime? EffectiveEndDate { get; set; }

//        /// <summary>
//        /// Gets or sets the division order reference.
//        /// </summary>
//        public string? DivisionOrderId { get; set; }
//    }

//    /// <summary>
//    /// Represents royalty calculation data (DTO for calculations/reporting).
//    /// </summary>
//    public class RoyaltyCalculationDto
//    {
//        /// <summary>
//        /// Gets or sets the calculation identifier.
//        /// </summary>
//        public string CalculationId { get; set; } = string.Empty;

//        /// <summary>
//        /// Gets or sets the calculation date.
//        /// </summary>
//        public DateTime CalculationDate { get; set; }

//        /// <summary>
//        /// Gets or sets the property or lease identifier.
//        /// </summary>
//        public string PropertyOrLeaseId { get; set; } = string.Empty;

//        /// <summary>
//        /// Gets or sets the gross revenue.
//        /// </summary>
//        public decimal GrossRevenue { get; set; }

//        /// <summary>
//        /// Gets or sets the deductions.
//        /// </summary>
//        public RoyaltyDeductionsDto Deductions { get; set; } = new();

//        /// <summary>
//        /// Gets the net revenue.
//        /// </summary>
//        public decimal NetRevenue => GrossRevenue - Deductions.TotalDeductions;

//        /// <summary>
//        /// Gets or sets the royalty interest.
//        /// </summary>
//        public decimal RoyaltyInterest { get; set; }

//        /// <summary>
//        /// Gets the royalty amount.
//        /// </summary>
//        public decimal RoyaltyAmount => NetRevenue * RoyaltyInterest;
//    }

//    /// <summary>
//    /// Represents royalty deductions.
//    /// </summary>
//    public class RoyaltyDeductionsDto
//    {
//        /// <summary>
//        /// Gets or sets the production taxes.
//        /// </summary>
//        public decimal ProductionTaxes { get; set; }

//        /// <summary>
//        /// Gets or sets the transportation costs.
//        /// </summary>
//        public decimal TransportationCosts { get; set; }

//        /// <summary>
//        /// Gets or sets the processing costs.
//        /// </summary>
//        public decimal ProcessingCosts { get; set; }

//        /// <summary>
//        /// Gets or sets the marketing costs.
//        /// </summary>
//        public decimal MarketingCosts { get; set; }

//        /// <summary>
//        /// Gets or sets other deductions.
//        /// </summary>
//        public decimal OtherDeductions { get; set; }

//        /// <summary>
//        /// Gets the total deductions.
//        /// </summary>
//        public decimal TotalDeductions =>
//            ProductionTaxes +
//            TransportationCosts +
//            ProcessingCosts +
//            MarketingCosts +
//            OtherDeductions;
//    }

//    /// <summary>
//    /// Represents a royalty payment (DTO for calculations/reporting).
//    /// </summary>
//    public class RoyaltyPaymentDto
//    {
//        /// <summary>
//        /// Gets or sets the payment identifier.
//        /// </summary>
//        public string PaymentId { get; set; } = string.Empty;

//        /// <summary>
//        /// Gets or sets the royalty owner identifier.
//        /// </summary>
//        public string RoyaltyOwnerId { get; set; } = string.Empty;

//        /// <summary>
//        /// Gets or sets the property or lease identifier.
//        /// </summary>
//        public string PropertyOrLeaseId { get; set; } = string.Empty;

//        /// <summary>
//        /// Gets or sets the payment period start date.
//        /// </summary>
//        public DateTime PaymentPeriodStart { get; set; }

//        /// <summary>
//        /// Gets or sets the payment period end date.
//        /// </summary>
//        public DateTime PaymentPeriodEnd { get; set; }

//        /// <summary>
//        /// Gets or sets the royalty amount.
//        /// </summary>
//        public decimal RoyaltyAmount { get; set; }

//        /// <summary>
//        /// Gets or sets the payment date.
//        /// </summary>
//        public DateTime PaymentDate { get; set; }

//        /// <summary>
//        /// Gets or sets the payment method.
//        /// </summary>
//        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Check;

//        /// <summary>
//        /// Gets or sets the check number (if applicable).
//        /// </summary>
//        public string? CheckNumber { get; set; }

//        /// <summary>
//        /// Gets or sets the status.
//        /// </summary>
//        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

//        /// <summary>
//        /// Gets or sets any tax withholdings.
//        /// </summary>
//        public List<TaxWithholdingDto> TaxWithholdings { get; set; } = new();

//        /// <summary>
//        /// Gets the net payment amount (after withholdings).
//        /// </summary>
//        public decimal NetPaymentAmount => RoyaltyAmount - TaxWithholdings.Sum(t => t.Amount);
//    }

//    /// <summary>
//    /// Represents tax withholding.
//    /// </summary>
//    public class TaxWithholdingDto
//    {
//        /// <summary>
//        /// Gets or sets the withholding identifier.
//        /// </summary>
//        public string WithholdingId { get; set; } = string.Empty;

//        /// <summary>
//        /// Gets or sets the withholding type.
//        /// </summary>
//        public TaxWithholdingType WithholdingType { get; set; }

//        /// <summary>
//        /// Gets or sets the withholding rate (percentage).
//        /// </summary>
//        public decimal WithholdingRate { get; set; }

//        /// <summary>
//        /// Gets or sets the withholding amount.
//        /// </summary>
//        public decimal Amount { get; set; }

//        /// <summary>
//        /// Gets or sets the reason for withholding.
//        /// </summary>
//        public string? Reason { get; set; }
//    }
//}

