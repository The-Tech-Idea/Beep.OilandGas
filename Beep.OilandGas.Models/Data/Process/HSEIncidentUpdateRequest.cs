using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class HSEIncidentUpdateRequest : ModelEntityBase
    {
        private string StatusValue = string.Empty;

        public string Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }

        private string RcaSummaryValue = string.Empty;

        /// <summary>Root Cause Analysis summary (maps to HSE_INCIDENT_CAUSE)</summary>
        public string RcaSummary
        {
            get { return this.RcaSummaryValue; }
            set { SetProperty(ref RcaSummaryValue, value); }
        }

        private string NotesValue = string.Empty;

        public string Notes
        {
            get { return this.NotesValue; }
            set { SetProperty(ref NotesValue, value); }
        }

        private string AssignedToUserIdValue = string.Empty;

        public string AssignedToUserId
        {
            get { return this.AssignedToUserIdValue; }
            set { SetProperty(ref AssignedToUserIdValue, value); }
        }

        private List<string> CorrectiveActionsValue = new();

        public List<string> CorrectiveActions
        {
            get { return this.CorrectiveActionsValue; }
            set { SetProperty(ref CorrectiveActionsValue, value); }
        }
    }

    public class CorrectiveActionCloseRequest : ModelEntityBase
    {
        private string NotesValue = string.Empty;

        public string Notes
        {
            get { return this.NotesValue; }
            set { SetProperty(ref NotesValue, value); }
        }
    }

    public class IncidentCloseRequest : ModelEntityBase
    {
        private string ReasonValue = string.Empty;

        public string Reason
        {
            get { return this.ReasonValue; }
            set { SetProperty(ref ReasonValue, value); }
        }
    }
}
