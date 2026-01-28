using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FieldTimeline : ModelEntityBase
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
        private List<FieldTimelineEvent> EventsValue = new List<FieldTimelineEvent>();

        public List<FieldTimelineEvent> Events

        {

            get { return this.EventsValue; }

            set { SetProperty(ref EventsValue, value); }

        }
        private DateTime? EarliestEventDateValue;

        public DateTime? EarliestEventDate

        {

            get { return this.EarliestEventDateValue; }

            set { SetProperty(ref EarliestEventDateValue, value); }

        }
        private DateTime? LatestEventDateValue;

        public DateTime? LatestEventDate

        {

            get { return this.LatestEventDateValue; }

            set { SetProperty(ref LatestEventDateValue, value); }

        }
        public Dictionary<string, int> EventCountsByPhase { get; set; } = new Dictionary<string, int>();
    }
}
