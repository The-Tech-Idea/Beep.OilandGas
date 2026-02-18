using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data.FieldOrchestrator;

namespace Beep.OilandGas.Models.Data
{
    public class FieldOrchestratorResponse : ModelEntityBase
    {
        private Field? FieldValue;

        public Field? Field

        {

            get { return this.FieldValue; }

            set { SetProperty(ref FieldValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? FieldNameValue;

        public string? FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
    }
}
