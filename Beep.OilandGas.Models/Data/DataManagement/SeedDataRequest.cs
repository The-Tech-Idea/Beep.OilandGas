using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class SeedDataRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName

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
        private bool SkipExistingValue = true;

        public bool SkipExisting

        {

            get { return this.SkipExistingValue; }

            set { SetProperty(ref SkipExistingValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
