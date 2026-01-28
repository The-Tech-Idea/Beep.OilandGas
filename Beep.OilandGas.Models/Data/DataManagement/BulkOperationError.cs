using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class BulkOperationError : ModelEntityBase
    {
        private int RecordIndexValue;

        public int RecordIndex

        {

            get { return this.RecordIndexValue; }

            set { SetProperty(ref RecordIndexValue, value); }

        }
        private object RecordIdValue;

        public object RecordId

        {

            get { return this.RecordIdValue; }

            set { SetProperty(ref RecordIdValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string ErrorTypeValue;

        public string ErrorType

        {

            get { return this.ErrorTypeValue; }

            set { SetProperty(ref ErrorTypeValue, value); }

        }
    }
}
