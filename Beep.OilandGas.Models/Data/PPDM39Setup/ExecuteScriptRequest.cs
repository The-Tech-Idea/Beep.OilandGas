using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class ExecuteScriptRequest : ModelEntityBase
    {
        private ConnectionProperties ConnectionValue = null!;

        [Required(ErrorMessage = "Connection is required")]
        public ConnectionProperties Connection

        {

            get { return this.ConnectionValue; }

            set { SetProperty(ref ConnectionValue, value); }

        }

        private string ScriptNameValue = string.Empty;


        [Required(ErrorMessage = "ScriptName is required")]
        public string ScriptName


        {


            get { return this.ScriptNameValue; }


            set { SetProperty(ref ScriptNameValue, value); }


        }
    }
}
