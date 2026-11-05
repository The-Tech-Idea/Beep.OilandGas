using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FieldListItem : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? CurrentPhaseValue;

        public string? CurrentPhase

        {

            get { return this.CurrentPhaseValue; }

            set { SetProperty(ref CurrentPhaseValue, value); }

        }
        private DateTime? LastModifiedDateValue;

        public DateTime? LastModifiedDate

        {

            get { return this.LastModifiedDateValue; }

            set { SetProperty(ref LastModifiedDateValue, value); }

        }
    }
}
