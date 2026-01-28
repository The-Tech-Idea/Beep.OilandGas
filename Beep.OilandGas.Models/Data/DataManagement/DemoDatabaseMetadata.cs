using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class DemoDatabaseMetadata : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string DatabasePathValue = string.Empty;

        public string DatabasePath

        {

            get { return this.DatabasePathValue; }

            set { SetProperty(ref DatabasePathValue, value); }

        }
        private string SeedDataOptionValue = string.Empty;

        public string SeedDataOption

        {

            get { return this.SeedDataOptionValue; }

            set { SetProperty(ref SeedDataOptionValue, value); }

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
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;
    }
}
