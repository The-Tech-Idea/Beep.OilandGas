using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class GateReviewSubmitRequest : ModelEntityBase
    {
        private string GateProcessIdValue = string.Empty;

        public string GateProcessId
        {
            get { return this.GateProcessIdValue; }
            set { SetProperty(ref GateProcessIdValue, value); }
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

        private string CommentsValue = string.Empty;

        public string Comments
        {
            get { return this.CommentsValue; }
            set { SetProperty(ref CommentsValue, value); }
        }

        private List<string> DocumentIdsValue = new();

        public List<string> DocumentIds
        {
            get { return this.DocumentIdsValue; }
            set { SetProperty(ref DocumentIdsValue, value); }
        }
    }
}
