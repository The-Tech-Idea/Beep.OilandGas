using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Unitization
{
    public class CreateTractParticipationRequest : ModelEntityBase
    {
        private string ParticipatingAreaIdValue;

        public string ParticipatingAreaId

        {

            get { return this.ParticipatingAreaIdValue; }

            set { SetProperty(ref ParticipatingAreaIdValue, value); }

        }
        private string UnitAgreementIdValue;

        public string UnitAgreementId

        {

            get { return this.UnitAgreementIdValue; }

            set { SetProperty(ref UnitAgreementIdValue, value); }

        }
        private string TractIdValue;

        public string TractId

        {

            get { return this.TractIdValue; }

            set { SetProperty(ref TractIdValue, value); }

        }
        private decimal ParticipationPercentageValue;

        public decimal ParticipationPercentage

        {

            get { return this.ParticipationPercentageValue; }

            set { SetProperty(ref ParticipationPercentageValue, value); }

        }
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private decimal? TractAcreageValue;

        public decimal? TractAcreage

        {

            get { return this.TractAcreageValue; }

            set { SetProperty(ref TractAcreageValue, value); }

        }
    }
}
