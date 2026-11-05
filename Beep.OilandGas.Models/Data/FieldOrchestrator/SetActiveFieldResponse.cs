using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SetActiveFieldResponse : ModelEntityBase
    {
        [System.Text.Json.Serialization.JsonRequired]
        public Beep.OilandGas.Models.Data.Field.FieldSelectionReview? Context { get; set; }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }
}
