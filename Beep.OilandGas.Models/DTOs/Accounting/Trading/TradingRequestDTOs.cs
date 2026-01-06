using System;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;

namespace Beep.OilandGas.Models.DTOs.Accounting.Trading
{
    /// <summary>
    /// Request DTO for creating an exchange contract
    /// </summary>
    public class CreateExchangeContractRequest
    {
        public string ContractId { get; set; } = string.Empty;
        public string ContractName { get; set; } = string.Empty;
        public ExchangeContractType ContractType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}



