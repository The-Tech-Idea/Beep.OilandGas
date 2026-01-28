using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ParticipatingArea : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the participating area identifier.
        /// </summary>
        private string ParticipatingAreaIdValue = string.Empty;

        public string ParticipatingAreaId

        {

            get { return this.ParticipatingAreaIdValue; }

            set { SetProperty(ref ParticipatingAreaIdValue, value); }

        }

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
        /// Gets or sets the participating area name.
        /// </summary>
        private string ParticipatingAreaNameValue = string.Empty;

        public string ParticipatingAreaName

        {

            get { return this.ParticipatingAreaNameValue; }

            set { SetProperty(ref ParticipatingAreaNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the tracts included in this area.
        /// </summary>
        private List<TractParticipation> TractsValue = new();

        public List<TractParticipation> Tracts

        {

            get { return this.TractsValue; }

            set { SetProperty(ref TractsValue, value); }

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
        /// Gets the total participation percentage.
        /// </summary>
        public decimal TotalParticipation => Tracts.Sum(t => t.ParticipationPercentage);
    }
}
