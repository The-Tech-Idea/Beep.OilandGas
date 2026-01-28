using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class CheckDriverRequest : ModelEntityBase
    {
        private string DatabaseTypeValue = string.Empty;

        [Required(ErrorMessage = "DatabaseType is required")]
        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        }
    }
}
