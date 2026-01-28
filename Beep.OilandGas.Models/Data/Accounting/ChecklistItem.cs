using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class ChecklistItem : ModelEntityBase
    {
        private System.String TaskValue = string.Empty;
        /// <summary>
        /// Gets or sets task.
        /// </summary>
        public System.String Task
        {
            get { return this.TaskValue; }
            set { SetProperty(ref TaskValue, value); }
        }

        private System.Boolean IsCompleteValue;
        /// <summary>
        /// Gets or sets completion flag.
        /// </summary>
        public System.Boolean IsComplete
        {
            get { return this.IsCompleteValue; }
            set { SetProperty(ref IsCompleteValue, value); }
        }

        private System.String DetailsValue = string.Empty;
        /// <summary>
        /// Gets or sets details.
        /// </summary>
        public System.String Details
        {
            get { return this.DetailsValue; }
            set { SetProperty(ref DetailsValue, value); }
        }
    }
}
