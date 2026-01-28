using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class TestConnectionRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }
}
