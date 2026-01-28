using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class CreateDemoDatabaseRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required(ErrorMessage = "UserId is required")]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private string SeedDataOptionValue = "reference-sample";


        public string SeedDataOption


        {


            get { return this.SeedDataOptionValue; }


            set { SetProperty(ref SeedDataOptionValue, value); }


        } // none, reference-only, reference-sample, full-demo
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        } // Optional, will be auto-generated if not provided
    }
}
