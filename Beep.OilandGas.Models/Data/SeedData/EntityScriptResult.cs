using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class EntityScriptResult : ModelEntityBase
    {
        private string EntityNameValue = string.Empty;

        public string EntityName

        {

            get { return this.EntityNameValue; }

            set { SetProperty(ref EntityNameValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int ScriptsGeneratedValue;

        public int ScriptsGenerated

        {

            get { return this.ScriptsGeneratedValue; }

            set { SetProperty(ref ScriptsGeneratedValue, value); }

        }
        private List<ScriptResult> ScriptsValue = new List<ScriptResult>();

        public List<ScriptResult> Scripts

        {

            get { return this.ScriptsValue; }

            set { SetProperty(ref ScriptsValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }
}
