using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class RiglessWorkoverRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string WorkoverSubTypeValue = string.Empty;

        public string WorkoverSubType

        {

            get { return this.WorkoverSubTypeValue; }

            set { SetProperty(ref WorkoverSubTypeValue, value); }

        } // WIRELINE, COILED_TUBING, SNUBBING, TESTING, STIMULATION, CLEANOUT
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        public Dictionary<string, object>? WorkoverData { get; set; }
    }
}
