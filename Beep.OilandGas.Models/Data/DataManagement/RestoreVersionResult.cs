using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class RestoreVersionResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private object RestoredEntityValue;

        public object RestoredEntity

        {

            get { return this.RestoredEntityValue; }

            set { SetProperty(ref RestoredEntityValue, value); }

        }
        private int RestoredVersionNumberValue;

        public int RestoredVersionNumber

        {

            get { return this.RestoredVersionNumberValue; }

            set { SetProperty(ref RestoredVersionNumberValue, value); }

        }
        private string MessageValue;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
    }
}
