using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class UnitAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        private string UnitIdValue = string.Empty;

        public string UnitId

        {

            get { return this.UnitIdValue; }

            set { SetProperty(ref UnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the unit name.
        /// </summary>
        private string UnitNameValue = string.Empty;

        public string UnitName

        {

            get { return this.UnitNameValue; }

            set { SetProperty(ref UnitNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the unit operator.
        /// </summary>
        private string UnitOperatorValue = string.Empty;

        public string UnitOperator

        {

            get { return this.UnitOperatorValue; }

            set { SetProperty(ref UnitOperatorValue, value); }

        }

        /// <summary>
        /// Gets or sets the participating area.
        /// </summary>
        private ParticipatingArea ParticipatingAreaValue = new();

        public ParticipatingArea ParticipatingArea

        {

            get { return this.ParticipatingAreaValue; }

            set { SetProperty(ref ParticipatingAreaValue, value); }

        }

        /// <summary>
        /// Gets or sets the terms and conditions.
        /// </summary>
        private string? TermsAndConditionsValue;

        public string? TermsAndConditions

        {

            get { return this.TermsAndConditionsValue; }

            set { SetProperty(ref TermsAndConditionsValue, value); }

        }
    }
}
