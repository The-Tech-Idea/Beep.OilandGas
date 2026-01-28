using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ScriptGenerationResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private int ScriptsGeneratedValue;

        public int ScriptsGenerated

        {

            get { return this.ScriptsGeneratedValue; }

            set { SetProperty(ref ScriptsGeneratedValue, value); }

        }
        private int EntitiesProcessedValue;

        public int EntitiesProcessed

        {

            get { return this.EntitiesProcessedValue; }

            set { SetProperty(ref EntitiesProcessedValue, value); }

        }
        private int ErrorsValue;

        public int Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        private string? OutputPathValue;

        public string? OutputPath

        {

            get { return this.OutputPathValue; }

            set { SetProperty(ref OutputPathValue, value); }

        }
        private List<EntityScriptResult> EntityResultsValue = new List<EntityScriptResult>();

        public List<EntityScriptResult> EntityResults

        {

            get { return this.EntityResultsValue; }

            set { SetProperty(ref EntityResultsValue, value); }

        }
    }
}
