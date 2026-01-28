using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class EntityStateHistoryResponse : ModelEntityBase
    {
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
        private string CurrentStateValue = string.Empty;

        public string CurrentState

        {

            get { return this.CurrentStateValue; }

            set { SetProperty(ref CurrentStateValue, value); }

        }
        private List<StateHistoryEntry> HistoryValue = new List<StateHistoryEntry>();

        public List<StateHistoryEntry> History

        {

            get { return this.HistoryValue; }

            set { SetProperty(ref HistoryValue, value); }

        }
    }
}
