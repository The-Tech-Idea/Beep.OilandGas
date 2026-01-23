using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting.Unitization
{
    /// <summary>
    /// Request DTO for creating a unit agreement
    /// </summary>
    public class CreateUnitAgreementRequest : ModelEntityBase
    {
        private string UnitNameValue = string.Empty;

        public string UnitName

        {

            get { return this.UnitNameValue; }

            set { SetProperty(ref UnitNameValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private string UnitOperatorValue = string.Empty;

        public string UnitOperator

        {

            get { return this.UnitOperatorValue; }

            set { SetProperty(ref UnitOperatorValue, value); }

        }
    }
}







