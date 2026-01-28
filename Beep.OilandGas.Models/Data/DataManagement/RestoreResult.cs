using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class RestoreResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int RestoredRecordCountValue;

        public int RestoredRecordCount

        {

            get { return this.RestoredRecordCountValue; }

            set { SetProperty(ref RestoredRecordCountValue, value); }

        }
        private List<object> RestoredRecordIdsValue = new List<object>();

        public List<object> RestoredRecordIds

        {

            get { return this.RestoredRecordIdsValue; }

            set { SetProperty(ref RestoredRecordIdsValue, value); }

        }
        private List<string> MessagesValue = new List<string>();

        public List<string> Messages

        {

            get { return this.MessagesValue; }

            set { SetProperty(ref MessagesValue, value); }

        }
    }
}
