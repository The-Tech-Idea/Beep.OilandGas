using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class CleanupDemoDatabasesResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int DeletedCountValue;

        public int DeletedCount

        {

            get { return this.DeletedCountValue; }

            set { SetProperty(ref DeletedCountValue, value); }

        }
        private List<string> DeletedDatabasesValue = new();

        public List<string> DeletedDatabases

        {

            get { return this.DeletedDatabasesValue; }

            set { SetProperty(ref DeletedDatabasesValue, value); }

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
    }
}
