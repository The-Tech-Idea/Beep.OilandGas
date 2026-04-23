using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class CreateDemoDatabaseResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string DatabasePathValue = string.Empty;

        public string DatabasePath

        {

            get { return this.DatabasePathValue; }

            set { SetProperty(ref DatabasePathValue, value); }

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
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private DateTime ExpiryDateValue;

        public DateTime ExpiryDate

        {

            get { return this.ExpiryDateValue; }

            set { SetProperty(ref ExpiryDateValue, value); }

        }

        private string? SchemaPlanIdValue;

        public string? SchemaPlanId

        {

            get { return this.SchemaPlanIdValue; }

            set { SetProperty(ref SchemaPlanIdValue, value); }

        }

        private string? SchemaExecutionTokenValue;

        public string? SchemaExecutionToken

        {

            get { return this.SchemaExecutionTokenValue; }

            set { SetProperty(ref SchemaExecutionTokenValue, value); }

        }
    }
}
