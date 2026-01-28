using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ScriptResult : ModelEntityBase
    {
        private string ScriptTypeValue = string.Empty;

        public string ScriptType

        {

            get { return this.ScriptTypeValue; }

            set { SetProperty(ref ScriptTypeValue, value); }

        } // TAB, PK, FK, IX
        private string DatabaseTypeValue = string.Empty;

        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        }
        private string? FilePathValue;

        public string? FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        private int ScriptLengthValue;

        public int ScriptLength

        {

            get { return this.ScriptLengthValue; }

            set { SetProperty(ref ScriptLengthValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }
}
