using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class ShiftHandoverRequest : ModelEntityBase
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
        private DateTime HandoverDateValue;

        public DateTime HandoverDate

        {

            get { return this.HandoverDateValue; }

            set { SetProperty(ref HandoverDateValue, value); }

        }
        private string FromShiftValue = string.Empty;

        public string FromShift

        {

            get { return this.FromShiftValue; }

            set { SetProperty(ref FromShiftValue, value); }

        }
        private string ToShiftValue = string.Empty;

        public string ToShift

        {

            get { return this.ToShiftValue; }

            set { SetProperty(ref ToShiftValue, value); }

        }
        private string? HandoverNotesValue;

        public string? HandoverNotes

        {

            get { return this.HandoverNotesValue; }

            set { SetProperty(ref HandoverNotesValue, value); }

        }
        public Dictionary<string, object>? HandoverData { get; set; }
    }
}
