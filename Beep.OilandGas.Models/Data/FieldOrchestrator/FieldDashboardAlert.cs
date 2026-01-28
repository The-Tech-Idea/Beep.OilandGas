using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FieldDashboardAlert : ModelEntityBase
    {
        private string AlertIdValue = string.Empty;

        public string AlertId

        {

            get { return this.AlertIdValue; }

            set { SetProperty(ref AlertIdValue, value); }

        }
        private string AlertTypeValue = string.Empty;

        public string AlertType

        {

            get { return this.AlertTypeValue; }

            set { SetProperty(ref AlertTypeValue, value); }

        } // "info", "warning", "error", "success"
        private string TitleValue = string.Empty;

        public string Title

        {

            get { return this.TitleValue; }

            set { SetProperty(ref TitleValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? PhaseValue;

        public string? Phase

        {

            get { return this.PhaseValue; }

            set { SetProperty(ref PhaseValue, value); }

        }
        private DateTime AlertDateValue;

        public DateTime AlertDate

        {

            get { return this.AlertDateValue; }

            set { SetProperty(ref AlertDateValue, value); }

        }
        private bool IsActiveValue = true;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
    }
}
