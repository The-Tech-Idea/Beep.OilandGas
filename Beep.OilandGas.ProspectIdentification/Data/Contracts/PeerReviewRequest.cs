using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PeerReviewRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string ReviewerValue = string.Empty;

        public string Reviewer

        {

            get { return this.ReviewerValue; }

            set { SetProperty(ref ReviewerValue, value); }

        }
        private string ReviewTypeValue = "Technical";

        public string ReviewType

        {

            get { return this.ReviewTypeValue; }

            set { SetProperty(ref ReviewTypeValue, value); }

        }
    }
}
