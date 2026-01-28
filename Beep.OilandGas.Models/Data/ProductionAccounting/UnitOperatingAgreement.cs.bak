using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class UnitOperatingAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the operating agreement identifier.
        /// </summary>
        private string OperatingAgreementIdValue = string.Empty;

        public string OperatingAgreementId

        {

            get { return this.OperatingAgreementIdValue; }

            set { SetProperty(ref OperatingAgreementIdValue, value); }

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
        /// Gets or sets the participants.
        /// </summary>
        private List<UnitParticipant> ParticipantsValue = new();

        public List<UnitParticipant> Participants

        {

            get { return this.ParticipantsValue; }

            set { SetProperty(ref ParticipantsValue, value); }

        }

        /// <summary>
        /// Gets or sets the voting rights provisions.
        /// </summary>
        private VotingRights VotingRightsValue = new();

        public VotingRights VotingRights

        {

            get { return this.VotingRightsValue; }

            set { SetProperty(ref VotingRightsValue, value); }

        }

        /// <summary>
        /// Gets or sets the cost sharing provisions.
        /// </summary>
        private CostSharing CostSharingValue = new();

        public CostSharing CostSharing

        {

            get { return this.CostSharingValue; }

            set { SetProperty(ref CostSharingValue, value); }

        }

        /// <summary>
        /// Gets or sets the revenue sharing provisions.
        /// </summary>
        private RevenueSharing RevenueSharingValue = new();

        public RevenueSharing RevenueSharing

        {

            get { return this.RevenueSharingValue; }

            set { SetProperty(ref RevenueSharingValue, value); }

        }
    }
}
