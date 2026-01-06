using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    // NOTE: ExchangeContractDto, ExchangePartyDto, ExchangeTermsDto, ExchangeDeliveryPointDto, 
    // ExchangePricingTermsDto, and ExchangeCommitmentDto are defined in TradingModelsDto.cs
    // This file contains request classes and additional DTOs for trading operations.

    /// <summary>
    /// DTO for exchange reconciliation
    /// </summary>
    public class ExchangeReconciliationDto
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
    public class CreateExchangeContractRequest
    {
        [Required]
        public string ContractId { get; set; } = string.Empty;
        [Required]
        public string ContractName { get; set; } = string.Empty;
        [Required]
        public ExchangeContractType ContractType { get; set; }
        [Required]
        public List<ExchangePartyDto> Parties { get; set; } = new();
        [Required]
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public ExchangeTermsDto? Terms { get; set; }
        public List<ExchangeDeliveryPointDto>? DeliveryPoints { get; set; }
        public ExchangePricingTermsDto? PricingTerms { get; set; }
    }

    /// <summary>
    /// Request to reconcile exchange
    /// </summary>
    public class ReconcileExchangeRequest
    {
        [Required]
        public DateTime ReconciliationDate { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public string? CounterpartyStatementId { get; set; }
    }
}




