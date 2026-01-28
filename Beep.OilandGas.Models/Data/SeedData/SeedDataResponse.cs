using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeedDataResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private int TablesSeededValue;

        public int TablesSeeded

        {

            get { return this.TablesSeededValue; }

            set { SetProperty(ref TablesSeededValue, value); }

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
        private List<TableSeedResult> TableResultsValue = new List<TableSeedResult>();

        public List<TableSeedResult> TableResults

        {

            get { return this.TableResultsValue; }

            set { SetProperty(ref TableResultsValue, value); }

        }
        private List<string> ErrorsValue = new List<string>();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }
}
