using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    // NOTE: ExchangeContract, ExchangeParty, ExchangeTerms, ExchangeDeliveryPoint, 
    // ExchangePricingTerms, and ExchangeCommitment are defined in TradingModelsDto.cs
    // This file contains request classes and additional DTOs for trading operations.

    /// <summary>
    /// DTO for exchange reconciliation
    /// </summary>
    public class ExchangeReconciliation : ModelEntityBase
    {
        public string ReconciliationId { get; set; } = string.Empty;
        public string ContractId { get; set; } = string.Empty;
        public DateTime ReconciliationDate { get; set; }
        public decimal ExpectedVolume { get; set; }
        public decimal ActualVolume { get; set; }
        public decimal Variance { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to create exchange contract
    /// </summary>
    public class CreateExchangeContractRequest : ModelEntityBase
    {
        [Required]
        public string ContractId { get; set; } = string.Empty;
        [Required]
        public string ContractName { get; set; } = string.Empty;
        [Required]
        public ExchangeContractType ContractType { get; set; }
        [Required]
        public List<ExchangeParty> Parties { get; set; } = new();
        [Required]
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public ExchangeTerms? Terms { get; set; }
        public List<ExchangeDeliveryPoint>? DeliveryPoints { get; set; }
        public ExchangePricingTerms? PricingTerms { get; set; }
    }

    /// <summary>
    /// Request to reconcile exchange
    /// </summary>
    public class ReconcileExchangeRequest : ModelEntityBase
    {
        [Required]
        public DateTime ReconciliationDate { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public string? CounterpartyStatementId { get; set; }
    }
}





