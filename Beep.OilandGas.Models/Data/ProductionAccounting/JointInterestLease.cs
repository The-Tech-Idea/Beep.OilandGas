using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class JointInterestLease : LeaseAgreement
    {
        /// <summary>
        /// Gets or sets the operator company.
        /// </summary>
        private string OperatorValue = string.Empty;

        public string Operator

        {

            get { return this.OperatorValue; }

            set { SetProperty(ref OperatorValue, value); }

        }

        /// <summary>
        /// Gets or sets the non-operator participants.
        /// </summary>
        private List<JointInterestParticipant> ParticipantsValue = new();

        public List<JointInterestParticipant> Participants

        {

            get { return this.ParticipantsValue; }

            set { SetProperty(ref ParticipantsValue, value); }

        }

        /// <summary>
        /// Gets or sets the joint operating agreement reference.
        /// </summary>
        private string JointOperatingAgreementIdValue = string.Empty;

        public string JointOperatingAgreementId

        {

            get { return this.JointOperatingAgreementIdValue; }

            set { SetProperty(ref JointOperatingAgreementIdValue, value); }

        }

        public JointInterestLease()
        {
            LeaseType = LeaseType.JointInterest;
        }
    }
}
