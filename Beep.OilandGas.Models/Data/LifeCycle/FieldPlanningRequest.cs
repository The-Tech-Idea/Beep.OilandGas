using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class FieldPlanningRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string PlanningTypeValue = string.Empty;

        public string PlanningType

        {

            get { return this.PlanningTypeValue; }

            set { SetProperty(ref PlanningTypeValue, value); }

        } // EXPLORATION, DEVELOPMENT, PRODUCTION, etc.
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? TargetStartDateValue;

        public DateTime? TargetStartDate

        {

            get { return this.TargetStartDateValue; }

            set { SetProperty(ref TargetStartDateValue, value); }

        }
        private DateTime? TargetEndDateValue;

        public DateTime? TargetEndDate

        {

            get { return this.TargetEndDateValue; }

            set { SetProperty(ref TargetEndDateValue, value); }

        }
        public Dictionary<string, object>? PlanningData { get; set; }
    }
}
