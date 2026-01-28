using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class VersionSnapshot : ModelEntityBase
    {
        private int VersionNumberValue;

        public int VersionNumber

        {

            get { return this.VersionNumberValue; }

            set { SetProperty(ref VersionNumberValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private object EntityIdValue;

        public object EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private object EntityDataValue;

        public object EntityData

        {

            get { return this.EntityDataValue; }

            set { SetProperty(ref EntityDataValue, value); }

        }
        private string VersionLabelValue;

        public string VersionLabel

        {

            get { return this.VersionLabelValue; }

            set { SetProperty(ref VersionLabelValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private string CreatedByValue;

        public string CreatedBy

        {

            get { return this.CreatedByValue; }

            set { SetProperty(ref CreatedByValue, value); }

        }
        private string ChangeDescriptionValue;

        public string ChangeDescription

        {

            get { return this.ChangeDescriptionValue; }

            set { SetProperty(ref ChangeDescriptionValue, value); }

        }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
