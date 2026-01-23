using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// DTO for royalty interest
    /// </summary>
    public class RoyaltyInterest : ModelEntityBase
    {
        private string RoyaltyInterestIdValue = string.Empty;

        public string RoyaltyInterestId

        {

            get { return this.RoyaltyInterestIdValue; }

            set { SetProperty(ref RoyaltyInterestIdValue, value); }

        }
        private string RoyaltyOwnerIdValue = string.Empty;

        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private string PropertyOrLeaseIdValue = string.Empty;

        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }
        private decimal InterestPercentageValue;

        public decimal InterestPercentage

        {

            get { return this.InterestPercentageValue; }

            set { SetProperty(ref InterestPercentageValue, value); }

        }
        private DateTime EffectiveStartDateValue;

        public DateTime EffectiveStartDate

        {

            get { return this.EffectiveStartDateValue; }

            set { SetProperty(ref EffectiveStartDateValue, value); }

        }
        private DateTime? EffectiveEndDateValue;

        public DateTime? EffectiveEndDate

        {

            get { return this.EffectiveEndDateValue; }

            set { SetProperty(ref EffectiveEndDateValue, value); }

        }
        private string? DivisionOrderIdValue;

        public string? DivisionOrderId

        {

            get { return this.DivisionOrderIdValue; }

            set { SetProperty(ref DivisionOrderIdValue, value); }

        }
    }

    /// <summary>
    /// DTO for royalty calculation
    /// </summary>
    public class RoyaltyCalculation : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        private string PropertyOrLeaseIdValue = string.Empty;

        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }
        private decimal GrossRevenueValue;

        public decimal GrossRevenue

        {

            get { return this.GrossRevenueValue; }

            set { SetProperty(ref GrossRevenueValue, value); }

        }
        private RoyaltyDeductions? DeductionsValue;

        public RoyaltyDeductions? Deductions

        {

            get { return this.DeductionsValue; }

            set { SetProperty(ref DeductionsValue, value); }

        }
        private decimal NetRevenueValue;

        public decimal NetRevenue

        {

            get { return this.NetRevenueValue; }

            set { SetProperty(ref NetRevenueValue, value); }

        }
        private decimal RoyaltyInterestValue;

        public decimal RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }
        private decimal RoyaltyAmountValue;

        public decimal RoyaltyAmount

        {

            get { return this.RoyaltyAmountValue; }

            set { SetProperty(ref RoyaltyAmountValue, value); }

        }
    }

    /// <summary>
    /// DTO for royalty deductions
    /// </summary>
    public class RoyaltyDeductions : ModelEntityBase
    {
        private decimal ProductionTaxesValue;

        public decimal ProductionTaxes

        {

            get { return this.ProductionTaxesValue; }

            set { SetProperty(ref ProductionTaxesValue, value); }

        }
        private decimal TransportationCostsValue;

        public decimal TransportationCosts

        {

            get { return this.TransportationCostsValue; }

            set { SetProperty(ref TransportationCostsValue, value); }

        }
        private decimal ProcessingCostsValue;

        public decimal ProcessingCosts

        {

            get { return this.ProcessingCostsValue; }

            set { SetProperty(ref ProcessingCostsValue, value); }

        }
        private decimal MarketingCostsValue;

        public decimal MarketingCosts

        {

            get { return this.MarketingCostsValue; }

            set { SetProperty(ref MarketingCostsValue, value); }

        }
        private decimal OtherDeductionsValue;

        public decimal OtherDeductions

        {

            get { return this.OtherDeductionsValue; }

            set { SetProperty(ref OtherDeductionsValue, value); }

        }
        private decimal TotalDeductionsValue;

        public decimal TotalDeductions

        {

            get { return this.TotalDeductionsValue; }

            set { SetProperty(ref TotalDeductionsValue, value); }

        }
    }

    /// <summary>
    /// DTO for royalty payment
    /// </summary>
    public class RoyaltyPayment : ModelEntityBase
    {
        private string PaymentIdValue = string.Empty;

        public string PaymentId

        {

            get { return this.PaymentIdValue; }

            set { SetProperty(ref PaymentIdValue, value); }

        }
        private string RoyaltyOwnerIdValue = string.Empty;

        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }
        private DateTime PaymentDateValue;

        public DateTime PaymentDate

        {

            get { return this.PaymentDateValue; }

            set { SetProperty(ref PaymentDateValue, value); }

        }
        private decimal RoyaltyAmountValue;

        public decimal RoyaltyAmount

        {

            get { return this.RoyaltyAmountValue; }

            set { SetProperty(ref RoyaltyAmountValue, value); }

        }
        private decimal TaxWithholdingsValue;

        public decimal TaxWithholdings

        {

            get { return this.TaxWithholdingsValue; }

            set { SetProperty(ref TaxWithholdingsValue, value); }

        }
        private decimal NetPaymentValue;

        public decimal NetPayment

        {

            get { return this.NetPaymentValue; }

            set { SetProperty(ref NetPaymentValue, value); }

        }
        private string PaymentMethodValue = string.Empty;

        public string PaymentMethod

        {

            get { return this.PaymentMethodValue; }

            set { SetProperty(ref PaymentMethodValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? CheckNumberValue;

        public string? CheckNumber

        {

            get { return this.CheckNumberValue; }

            set { SetProperty(ref CheckNumberValue, value); }

        }
        private DateTime? PaymentProcessedDateValue;

        public DateTime? PaymentProcessedDate

        {

            get { return this.PaymentProcessedDateValue; }

            set { SetProperty(ref PaymentProcessedDateValue, value); }

        }
    }

    /// <summary>
    /// DTO for royalty statement
    /// </summary>
    public class RoyaltyStatement : ModelEntityBase
    {
        private string StatementIdValue = string.Empty;

        public string StatementId

        {

            get { return this.StatementIdValue; }

            set { SetProperty(ref StatementIdValue, value); }

        }
        private string RoyaltyOwnerIdValue = string.Empty;

        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }
        private DateTime StatementPeriodStartValue;

        public DateTime StatementPeriodStart

        {

            get { return this.StatementPeriodStartValue; }

            set { SetProperty(ref StatementPeriodStartValue, value); }

        }
        private DateTime StatementPeriodEndValue;

        public DateTime StatementPeriodEnd

        {

            get { return this.StatementPeriodEndValue; }

            set { SetProperty(ref StatementPeriodEndValue, value); }

        }
        private ProductionSummary? ProductionSummaryValue;

        public ProductionSummary? ProductionSummary

        {

            get { return this.ProductionSummaryValue; }

            set { SetProperty(ref ProductionSummaryValue, value); }

        }
        private RevenueSummary? RevenueSummaryValue;

        public RevenueSummary? RevenueSummary

        {

            get { return this.RevenueSummaryValue; }

            set { SetProperty(ref RevenueSummaryValue, value); }

        }
        private DeductionsSummary? DeductionsSummaryValue;

        public DeductionsSummary? DeductionsSummary

        {

            get { return this.DeductionsSummaryValue; }

            set { SetProperty(ref DeductionsSummaryValue, value); }

        }
        private decimal TotalRoyaltyAmountValue;

        public decimal TotalRoyaltyAmount

        {

            get { return this.TotalRoyaltyAmountValue; }

            set { SetProperty(ref TotalRoyaltyAmountValue, value); }

        }
    }

    /// <summary>
    /// DTO for production summary
    /// </summary>
    public class ProductionSummary : ModelEntityBase
    {
        private decimal GrossOilVolumeValue;

        public decimal GrossOilVolume

        {

            get { return this.GrossOilVolumeValue; }

            set { SetProperty(ref GrossOilVolumeValue, value); }

        }
        private decimal GrossGasVolumeValue;

        public decimal GrossGasVolume

        {

            get { return this.GrossGasVolumeValue; }

            set { SetProperty(ref GrossGasVolumeValue, value); }

        }
        private decimal NetOilVolumeValue;

        public decimal NetOilVolume

        {

            get { return this.NetOilVolumeValue; }

            set { SetProperty(ref NetOilVolumeValue, value); }

        }
        private decimal NetGasVolumeValue;

        public decimal NetGasVolume

        {

            get { return this.NetGasVolumeValue; }

            set { SetProperty(ref NetGasVolumeValue, value); }

        }
    }

    /// <summary>
    /// DTO for revenue summary
    /// </summary>
    public class RevenueSummary : ModelEntityBase
    {
        private decimal GrossRevenueValue;

        public decimal GrossRevenue

        {

            get { return this.GrossRevenueValue; }

            set { SetProperty(ref GrossRevenueValue, value); }

        }
        private decimal NetRevenueValue;

        public decimal NetRevenue

        {

            get { return this.NetRevenueValue; }

            set { SetProperty(ref NetRevenueValue, value); }

        }
    }

    /// <summary>
    /// DTO for deductions summary
    /// </summary>
    public class DeductionsSummary : ModelEntityBase
    {
        private decimal TotalDeductionsValue;

        public decimal TotalDeductions

        {

            get { return this.TotalDeductionsValue; }

            set { SetProperty(ref TotalDeductionsValue, value); }

        }
        private decimal ProductionTaxesValue;

        public decimal ProductionTaxes

        {

            get { return this.ProductionTaxesValue; }

            set { SetProperty(ref ProductionTaxesValue, value); }

        }
        private decimal TransportationCostsValue;

        public decimal TransportationCosts

        {

            get { return this.TransportationCostsValue; }

            set { SetProperty(ref TransportationCostsValue, value); }

        }
        private decimal ProcessingCostsValue;

        public decimal ProcessingCosts

        {

            get { return this.ProcessingCostsValue; }

            set { SetProperty(ref ProcessingCostsValue, value); }

        }
        private decimal MarketingCostsValue;

        public decimal MarketingCosts

        {

            get { return this.MarketingCostsValue; }

            set { SetProperty(ref MarketingCostsValue, value); }

        }
    }

    /// <summary>
    /// DTO for 1099 form information
    /// </summary>
    public class Form1099Info : ModelEntityBase
    {
        private string TaxIdValue = string.Empty;

        public string TaxId

        {

            get { return this.TaxIdValue; }

            set { SetProperty(ref TaxIdValue, value); }

        }
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }
        private int TaxYearValue;

        public int TaxYear

        {

            get { return this.TaxYearValue; }

            set { SetProperty(ref TaxYearValue, value); }

        }
        private decimal TotalPaymentsValue;

        public decimal TotalPayments

        {

            get { return this.TotalPaymentsValue; }

            set { SetProperty(ref TotalPaymentsValue, value); }

        }
        private decimal TotalWithholdingsValue;

        public decimal TotalWithholdings

        {

            get { return this.TotalWithholdingsValue; }

            set { SetProperty(ref TotalWithholdingsValue, value); }

        }
        private string? AddressValue;

        public string? Address

        {

            get { return this.AddressValue; }

            set { SetProperty(ref AddressValue, value); }

        }
    }

    /// <summary>
    /// Request to calculate royalty
    /// </summary>
    public class CalculateRoyaltyRequest : ModelEntityBase
    {
        private string PropertyOrLeaseIdValue = string.Empty;

        [Required]
        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }
        private string RoyaltyOwnerIdValue = string.Empty;

        [Required]
        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private decimal GrossRevenueValue;

        [Required]
        public decimal GrossRevenue

        {

            get { return this.GrossRevenueValue; }

            set { SetProperty(ref GrossRevenueValue, value); }

        }
        private RoyaltyDeductions? DeductionsValue;

        public RoyaltyDeductions? Deductions

        {

            get { return this.DeductionsValue; }

            set { SetProperty(ref DeductionsValue, value); }

        }
        private decimal RoyaltyInterestValue;

        [Required]
        [Range(0, 1)]
        public decimal RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }
        private DateTime CalculationDateValue;

        [Required]
        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
    }

    /// <summary>
    /// Request to create royalty payment
    /// </summary>
    public class CreateRoyaltyPaymentRequest : ModelEntityBase
    {
        private string RoyaltyOwnerIdValue = string.Empty;

        [Required]
        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private decimal RoyaltyAmountValue;

        [Required]
        public decimal RoyaltyAmount

        {

            get { return this.RoyaltyAmountValue; }

            set { SetProperty(ref RoyaltyAmountValue, value); }

        }
        private decimal? TaxWithholdingsValue;

        public decimal? TaxWithholdings

        {

            get { return this.TaxWithholdingsValue; }

            set { SetProperty(ref TaxWithholdingsValue, value); }

        }
        private DateTime PaymentDateValue;

        [Required]
        public DateTime PaymentDate

        {

            get { return this.PaymentDateValue; }

            set { SetProperty(ref PaymentDateValue, value); }

        }
        private string PaymentMethodValue = string.Empty;

        [Required]
        public string PaymentMethod

        {

            get { return this.PaymentMethodValue; }

            set { SetProperty(ref PaymentMethodValue, value); }

        }
    }
}








