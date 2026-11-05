using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SetActiveFieldRequest : ModelEntityBase
    {
        [System.Text.Json.Serialization.JsonRequired]
        public TheTechIdea.Data.OilGas.FieldSelectionReview? Context { get; set; }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }
}
