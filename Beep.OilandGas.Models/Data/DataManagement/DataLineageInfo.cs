using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class DataLineageInfo : ModelEntityBase
    {
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
        private string SourceSystemValue;

        public string SourceSystem

        {

            get { return this.SourceSystemValue; }

            set { SetProperty(ref SourceSystemValue, value); }

        }
        private string SourceTableValue;

        public string SourceTable

        {

            get { return this.SourceTableValue; }

            set { SetProperty(ref SourceTableValue, value); }

        }
        private object SourceEntityIdValue;

        public object SourceEntityId

        {

            get { return this.SourceEntityIdValue; }

            set { SetProperty(ref SourceEntityIdValue, value); }

        }
        private string TransformationTypeValue;

        public string TransformationType

        {

            get { return this.TransformationTypeValue; }

            set { SetProperty(ref TransformationTypeValue, value); }

        }
        private string TransformationDetailsValue;

        public string TransformationDetails

        {

            get { return this.TransformationDetailsValue; }

            set { SetProperty(ref TransformationDetailsValue, value); }

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
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
