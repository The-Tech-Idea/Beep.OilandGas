using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
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
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

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
    }
}
