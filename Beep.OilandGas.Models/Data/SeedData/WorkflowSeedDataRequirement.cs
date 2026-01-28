using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WorkflowSeedDataRequirement : ModelEntityBase
    {
        private string WorkflowNameValue = string.Empty;

        public string WorkflowName

        {

            get { return this.WorkflowNameValue; }

            set { SetProperty(ref WorkflowNameValue, value); }

        }
        private string WorkflowCategoryValue = string.Empty;

        public string WorkflowCategory

        {

            get { return this.WorkflowCategoryValue; }

            set { SetProperty(ref WorkflowCategoryValue, value); }

        }
        private List<string> RequiredTablesValue = new List<string>();

        public List<string> RequiredTables

        {

            get { return this.RequiredTablesValue; }

            set { SetProperty(ref RequiredTablesValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
