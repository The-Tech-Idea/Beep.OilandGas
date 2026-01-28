using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeedDataRequest : ModelEntityBase
    {
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private List<string>? TableNamesValue;

        public List<string>? TableNames

        {

            get { return this.TableNamesValue; }

            set { SetProperty(ref TableNamesValue, value); }

        }
        private bool ValidateOnlyValue;

        public bool ValidateOnly

        {

            get { return this.ValidateOnlyValue; }

            set { SetProperty(ref ValidateOnlyValue, value); }

        }
        private bool SkipExistingValue = true;

        public bool SkipExisting

        {

            get { return this.SkipExistingValue; }

            set { SetProperty(ref SkipExistingValue, value); }

        }
        private string? UserIdValue = "SYSTEM";

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
