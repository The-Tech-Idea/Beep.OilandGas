using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FieldTimelineEvent : ModelEntityBase
    {
        private string EventIdValue = string.Empty;

        public string EventId

        {

            get { return this.EventIdValue; }

            set { SetProperty(ref EventIdValue, value); }

        }
        private string EventTypeValue = string.Empty;

        public string EventType

        {

            get { return this.EventTypeValue; }

            set { SetProperty(ref EventTypeValue, value); }

        } // Exploration, Development, Production, Decommissioning
        private string EventDescriptionValue = string.Empty;

        public string EventDescription

        {

            get { return this.EventDescriptionValue; }

            set { SetProperty(ref EventDescriptionValue, value); }

        }
        private DateTime EventDateValue;

        public DateTime EventDate

        {

            get { return this.EventDateValue; }

            set { SetProperty(ref EventDateValue, value); }

        }
        private string? EntityTypeValue;

        public string? EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        } // PROSPECT, WELL, FACILITY, etc.
        private string? EntityIdValue;

        public string? EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string? EntityNameValue;

        public string? EntityName

        {

            get { return this.EntityNameValue; }

            set { SetProperty(ref EntityNameValue, value); }

        }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}
