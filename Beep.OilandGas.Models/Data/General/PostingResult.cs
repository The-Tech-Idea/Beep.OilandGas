using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.General
{
    public class PostingResult : ModelEntityBase
    {
        private int EntriesPostedValue;

        public int EntriesPosted

        {

            get { return this.EntriesPostedValue; }

            set { SetProperty(ref EntriesPostedValue, value); }

        }
        private int EntriesFailedValue;

        public int EntriesFailed

        {

            get { return this.EntriesFailedValue; }

            set { SetProperty(ref EntriesFailedValue, value); }

        }
        private List<string> ErrorsValue = new();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }
}


