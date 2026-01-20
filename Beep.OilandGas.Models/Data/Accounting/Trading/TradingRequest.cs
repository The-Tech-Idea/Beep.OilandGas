using System;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Data.Accounting.Trading
{
    /// <summary>
    /// Request DTO for creating an exchange contract
    /// </summary>
    public class CreateExchangeContractRequest : ModelEntityBase
    {
        public string ContractId { get; set; } = string.Empty;
        public string ContractName { get; set; } = string.Empty;
        public ExchangeContractType ContractType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}




