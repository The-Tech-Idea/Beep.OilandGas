using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ScriptGenerationRequest : ModelEntityBase
    {
        private List<string>? EntityNamesValue;

        public List<string>? EntityNames

        {

            get { return this.EntityNamesValue; }

            set { SetProperty(ref EntityNamesValue, value); }

        }
        private List<string>? DatabaseTypesValue;

        public List<string>? DatabaseTypes

        {

            get { return this.DatabaseTypesValue; }

            set { SetProperty(ref DatabaseTypesValue, value); }

        }
        private string? OutputPathValue;

        public string? OutputPath

        {

            get { return this.OutputPathValue; }

            set { SetProperty(ref OutputPathValue, value); }

        }
        private List<string>? ScriptTypesValue;

        public List<string>? ScriptTypes

        {

            get { return this.ScriptTypesValue; }

            set { SetProperty(ref ScriptTypesValue, value); }

        } // TAB, PK, FK, IX, etc.
        private bool SaveToFileValue = true;

        public bool SaveToFile

        {

            get { return this.SaveToFileValue; }

            set { SetProperty(ref SaveToFileValue, value); }

        }
    }
}
