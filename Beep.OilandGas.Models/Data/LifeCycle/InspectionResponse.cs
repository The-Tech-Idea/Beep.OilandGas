using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class InspectionResponse : ModelEntityBase
    {
        private string InspectionIdValue = string.Empty;

        public string InspectionId

        {

            get { return this.InspectionIdValue; }

            set { SetProperty(ref InspectionIdValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string InspectionTypeValue = string.Empty;

        public string InspectionType

        {

            get { return this.InspectionTypeValue; }

            set { SetProperty(ref InspectionTypeValue, value); }

        }
        private DateTime InspectionDateValue;

        public DateTime InspectionDate

        {

            get { return this.InspectionDateValue; }

            set { SetProperty(ref InspectionDateValue, value); }

        }
        private string? InspectorValue;

        public string? Inspector

        {

            get { return this.InspectorValue; }

            set { SetProperty(ref InspectorValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? InspectionData { get; set; }
    }
}
