using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Unitization
{
    public class UnitOperationsSummary : ModelEntityBase
    {
        private string UnitAgreementIdValue;

        public string UnitAgreementId

        {

            get { return this.UnitAgreementIdValue; }

            set { SetProperty(ref UnitAgreementIdValue, value); }

        }
        private string UnitNameValue;

        public string UnitName

        {

            get { return this.UnitNameValue; }

            set { SetProperty(ref UnitNameValue, value); }

        }
        private int ParticipatingAreaCountValue;

        public int ParticipatingAreaCount

        {

            get { return this.ParticipatingAreaCountValue; }

            set { SetProperty(ref ParticipatingAreaCountValue, value); }

        }
        private int TractCountValue;

        public int TractCount

        {

            get { return this.TractCountValue; }

            set { SetProperty(ref TractCountValue, value); }

        }
        private decimal TotalParticipationPercentageValue;

        public decimal TotalParticipationPercentage

        {

            get { return this.TotalParticipationPercentageValue; }

            set { SetProperty(ref TotalParticipationPercentageValue, value); }

        }
        private decimal TotalWorkingInterestValue;

        public decimal TotalWorkingInterest

        {

            get { return this.TotalWorkingInterestValue; }

            set { SetProperty(ref TotalWorkingInterestValue, value); }

        }
        private decimal TotalNetRevenueInterestValue;

        public decimal TotalNetRevenueInterest

        {

            get { return this.TotalNetRevenueInterestValue; }

            set { SetProperty(ref TotalNetRevenueInterestValue, value); }

        }
        private DateTime? LastProductionDateValue;

        public DateTime? LastProductionDate

        {

            get { return this.LastProductionDateValue; }

            set { SetProperty(ref LastProductionDateValue, value); }

        }
        private decimal? TotalProductionVolumeValue;

        public decimal? TotalProductionVolume

        {

            get { return this.TotalProductionVolumeValue; }

            set { SetProperty(ref TotalProductionVolumeValue, value); }

        }
    }
}
