using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class TableSeedResult : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int RecordsInsertedValue;

        public int RecordsInserted

        {

            get { return this.RecordsInsertedValue; }

            set { SetProperty(ref RecordsInsertedValue, value); }

        }
        private int RecordsSkippedValue;

        public int RecordsSkipped

        {

            get { return this.RecordsSkippedValue; }

            set { SetProperty(ref RecordsSkippedValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }
}
