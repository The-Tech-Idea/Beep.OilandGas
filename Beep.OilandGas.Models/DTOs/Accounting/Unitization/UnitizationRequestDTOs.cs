using System;

namespace Beep.OilandGas.Models.DTOs.Accounting.Unitization
{
    /// <summary>
    /// Request DTO for creating a unit agreement
    /// </summary>
    public class CreateUnitAgreementRequest
    {
        public string UnitName { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public string UnitOperator { get; set; } = string.Empty;
    }
}



