using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class MergeResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private object MergedEntityValue;

        public object MergedEntity

        {

            get { return this.MergedEntityValue; }

            set { SetProperty(ref MergedEntityValue, value); }

        }
        private List<object> MergedRecordIdsValue = new List<object>();

        public List<object> MergedRecordIds

        {

            get { return this.MergedRecordIdsValue; }

            set { SetProperty(ref MergedRecordIdsValue, value); }

        }
        private List<string> MessagesValue = new List<string>();

        public List<string> Messages

        {

            get { return this.MessagesValue; }

            set { SetProperty(ref MessagesValue, value); }

        }
    }
}
