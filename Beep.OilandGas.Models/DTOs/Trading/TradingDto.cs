using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Trading
{
    public class CreateExchangeContractRequest
    {
        public string ContractNumber { get; set; }
        public string CounterpartyBaId { get; set; }
        public string ContractType { get; set; }
        public string CommodityType { get; set; }
        public DateTime ContractDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Description { get; set; }
    }

    public class CreateExchangeCommitmentRequest
    {
        public string ExchangeContractId { get; set; }
        public string CommitmentType { get; set; }
        public decimal CommittedVolume { get; set; }
        public DateTime DeliveryPeriodStart { get; set; }
        public DateTime DeliveryPeriodEnd { get; set; }
    }

    public class CreateExchangeTransactionRequest
    {
        public string ExchangeContractId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal ReceiptVolume { get; set; }
        public decimal ReceiptPrice { get; set; }
        public string ReceiptLocation { get; set; }
        public decimal DeliveryVolume { get; set; }
        public decimal DeliveryPrice { get; set; }
        public string DeliveryLocation { get; set; }
        public string Description { get; set; }
    }

    public class ExchangeSettlementResult
    {
        public string ExchangeContractId { get; set; }
        public DateTime SettlementDate { get; set; }
        public decimal TotalReceiptValue { get; set; }
        public decimal TotalDeliveryValue { get; set; }
        public decimal NetSettlementAmount { get; set; }
        public int TransactionCount { get; set; }
        public bool IsSettled { get; set; }
    }

    public class ExchangeReconciliationResult
    {
        public string ExchangeContractId { get; set; }
        public DateTime ReconciliationDate { get; set; }
        public decimal BookReceiptVolume { get; set; }
        public decimal BookDeliveryVolume { get; set; }
        public decimal PhysicalReceiptVolume { get; set; }
        public decimal PhysicalDeliveryVolume { get; set; }
        public decimal ReceiptVariance { get; set; }
        public decimal DeliveryVariance { get; set; }
        public bool RequiresAdjustment { get; set; }
    }
}

