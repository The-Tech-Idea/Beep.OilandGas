using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.ProductionAccounting;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Trading
{
    /// <summary>
    /// Request to create exchange contract
    /// </summary>
    public class CreateExchangeContractRequest : ModelEntityBase
    {
        private string ContractIdValue = string.Empty;

        [Required]
        public string ContractId

        {

            get { return this.ContractIdValue; }

            set { SetProperty(ref ContractIdValue, value); }

        }
        private string ContractNameValue = string.Empty;

        [Required]
        public string ContractName

        {

            get { return this.ContractNameValue; }

            set { SetProperty(ref ContractNameValue, value); }

        }
        private ExchangeContractType ContractTypeValue;

        [Required]
        public ExchangeContractType ContractType

        {

            get { return this.ContractTypeValue; }

            set { SetProperty(ref ContractTypeValue, value); }

        }
        private List<ExchangeParty> PartiesValue = new();

        [Required]
        public List<ExchangeParty> Parties

        {

            get { return this.PartiesValue; }

            set { SetProperty(ref PartiesValue, value); }

        }
        private DateTime EffectiveDateValue;

        [Required]
        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
        private ExchangeTerms? TermsValue;

        public ExchangeTerms? Terms

        {

            get { return this.TermsValue; }

            set { SetProperty(ref TermsValue, value); }

        }
        private List<ExchangeDeliveryPoint>? DeliveryPointsValue;

        public List<ExchangeDeliveryPoint>? DeliveryPoints

        {

            get { return this.DeliveryPointsValue; }

            set { SetProperty(ref DeliveryPointsValue, value); }

        }
        private ExchangePricingTerms? PricingTermsValue;

        public ExchangePricingTerms? PricingTerms

        {

            get { return this.PricingTermsValue; }

            set { SetProperty(ref PricingTermsValue, value); }

        }
    }

    public class CreateExchangeCommitmentRequest : ModelEntityBase
    {
        private string ExchangeContractIdValue;

        public string ExchangeContractId

        {

            get { return this.ExchangeContractIdValue; }

            set { SetProperty(ref ExchangeContractIdValue, value); }

        }
        private string CommitmentTypeValue;

        public string CommitmentType

        {

            get { return this.CommitmentTypeValue; }

            set { SetProperty(ref CommitmentTypeValue, value); }

        }
        private decimal CommittedVolumeValue;

        public decimal CommittedVolume

        {

            get { return this.CommittedVolumeValue; }

            set { SetProperty(ref CommittedVolumeValue, value); }

        }
        private DateTime DeliveryPeriodStartValue;

        public DateTime DeliveryPeriodStart

        {

            get { return this.DeliveryPeriodStartValue; }

            set { SetProperty(ref DeliveryPeriodStartValue, value); }

        }
        private DateTime DeliveryPeriodEndValue;

        public DateTime DeliveryPeriodEnd

        {

            get { return this.DeliveryPeriodEndValue; }

            set { SetProperty(ref DeliveryPeriodEndValue, value); }

        }
    }

    public class CreateExchangeTransactionRequest : ModelEntityBase
    {
        private string ExchangeContractIdValue;

        public string ExchangeContractId

        {

            get { return this.ExchangeContractIdValue; }

            set { SetProperty(ref ExchangeContractIdValue, value); }

        }
        private DateTime TransactionDateValue;

        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private decimal ReceiptVolumeValue;

        public decimal ReceiptVolume

        {

            get { return this.ReceiptVolumeValue; }

            set { SetProperty(ref ReceiptVolumeValue, value); }

        }
        private decimal ReceiptPriceValue;

        public decimal ReceiptPrice

        {

            get { return this.ReceiptPriceValue; }

            set { SetProperty(ref ReceiptPriceValue, value); }

        }
        private string ReceiptLocationValue;

        public string ReceiptLocation

        {

            get { return this.ReceiptLocationValue; }

            set { SetProperty(ref ReceiptLocationValue, value); }

        }
        private decimal DeliveryVolumeValue;

        public decimal DeliveryVolume

        {

            get { return this.DeliveryVolumeValue; }

            set { SetProperty(ref DeliveryVolumeValue, value); }

        }
        private decimal DeliveryPriceValue;

        public decimal DeliveryPrice

        {

            get { return this.DeliveryPriceValue; }

            set { SetProperty(ref DeliveryPriceValue, value); }

        }
        private string DeliveryLocationValue;

        public string DeliveryLocation

        {

            get { return this.DeliveryLocationValue; }

            set { SetProperty(ref DeliveryLocationValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class ExchangeSettlementResult : ModelEntityBase
    {
        private string ExchangeContractIdValue;

        public string ExchangeContractId

        {

            get { return this.ExchangeContractIdValue; }

            set { SetProperty(ref ExchangeContractIdValue, value); }

        }
        private DateTime SettlementDateValue;

        public DateTime SettlementDate

        {

            get { return this.SettlementDateValue; }

            set { SetProperty(ref SettlementDateValue, value); }

        }
        private decimal TotalReceiptValueValue;

        public decimal TotalReceiptValue

        {

            get { return this.TotalReceiptValueValue; }

            set { SetProperty(ref TotalReceiptValueValue, value); }

        }
        private decimal TotalDeliveryValueValue;

        public decimal TotalDeliveryValue

        {

            get { return this.TotalDeliveryValueValue; }

            set { SetProperty(ref TotalDeliveryValueValue, value); }

        }
        private decimal NetSettlementAmountValue;

        public decimal NetSettlementAmount

        {

            get { return this.NetSettlementAmountValue; }

            set { SetProperty(ref NetSettlementAmountValue, value); }

        }
        private int TransactionCountValue;

        public int TransactionCount

        {

            get { return this.TransactionCountValue; }

            set { SetProperty(ref TransactionCountValue, value); }

        }
        private bool IsSettledValue;

        public bool IsSettled

        {

            get { return this.IsSettledValue; }

            set { SetProperty(ref IsSettledValue, value); }

        }
    }

    /// <summary>
    /// DTO for exchange reconciliation
    /// </summary>
    public class ExchangeReconciliation : ModelEntityBase
    {
        private string ReconciliationIdValue = string.Empty;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private string ContractIdValue = string.Empty;

        public string ContractId

        {

            get { return this.ContractIdValue; }

            set { SetProperty(ref ContractIdValue, value); }

        }
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private decimal ExpectedVolumeValue;

        public decimal ExpectedVolume

        {

            get { return this.ExpectedVolumeValue; }

            set { SetProperty(ref ExpectedVolumeValue, value); }

        }
        private decimal ActualVolumeValue;

        public decimal ActualVolume

        {

            get { return this.ActualVolumeValue; }

            set { SetProperty(ref ActualVolumeValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    public class ExchangeReconciliationResult : ModelEntityBase
    {
        private string ExchangeContractIdValue;

        public string ExchangeContractId

        {

            get { return this.ExchangeContractIdValue; }

            set { SetProperty(ref ExchangeContractIdValue, value); }

        }
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private decimal BookReceiptVolumeValue;

        public decimal BookReceiptVolume

        {

            get { return this.BookReceiptVolumeValue; }

            set { SetProperty(ref BookReceiptVolumeValue, value); }

        }
        private decimal BookDeliveryVolumeValue;

        public decimal BookDeliveryVolume

        {

            get { return this.BookDeliveryVolumeValue; }

            set { SetProperty(ref BookDeliveryVolumeValue, value); }

        }
        private decimal PhysicalReceiptVolumeValue;

        public decimal PhysicalReceiptVolume

        {

            get { return this.PhysicalReceiptVolumeValue; }

            set { SetProperty(ref PhysicalReceiptVolumeValue, value); }

        }
        private decimal PhysicalDeliveryVolumeValue;

        public decimal PhysicalDeliveryVolume

        {

            get { return this.PhysicalDeliveryVolumeValue; }

            set { SetProperty(ref PhysicalDeliveryVolumeValue, value); }

        }
        private decimal ReceiptVarianceValue;

        public decimal ReceiptVariance

        {

            get { return this.ReceiptVarianceValue; }

            set { SetProperty(ref ReceiptVarianceValue, value); }

        }
        private decimal DeliveryVarianceValue;

        public decimal DeliveryVariance

        {

            get { return this.DeliveryVarianceValue; }

            set { SetProperty(ref DeliveryVarianceValue, value); }

        }
        private bool RequiresAdjustmentValue;

        public bool RequiresAdjustment

        {

            get { return this.RequiresAdjustmentValue; }

            set { SetProperty(ref RequiresAdjustmentValue, value); }

        }
    }

    /// <summary>
    /// Request to reconcile exchange
    /// </summary>
    public class ReconcileExchangeRequest : ModelEntityBase
    {
        private DateTime ReconciliationDateValue;

        [Required]
        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private DateTime? PeriodStartValue;

        public DateTime? PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime? PeriodEndValue;

        public DateTime? PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private string? CounterpartyStatementIdValue;

        public string? CounterpartyStatementId

        {

            get { return this.CounterpartyStatementIdValue; }

            set { SetProperty(ref CounterpartyStatementIdValue, value); }

        }
    }
}








