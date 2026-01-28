using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeedDataValidationRequest : ModelEntityBase
    {
        private string CategoryValue = string.Empty;

        public string Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
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
    }
}
