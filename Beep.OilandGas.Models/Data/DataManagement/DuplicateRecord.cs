using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class DuplicateRecord : ModelEntityBase
    {
        private object RecordIdValue;

        public object RecordId

        {

            get { return this.RecordIdValue; }

            set { SetProperty(ref RecordIdValue, value); }

        }
        private object EntityValue;

        public object Entity

        {

            get { return this.EntityValue; }

            set { SetProperty(ref EntityValue, value); }

        }
        public Dictionary<string, object> FieldValues { get; set; } = new Dictionary<string, object>();
        private bool IsMasterValue;

        public bool IsMaster

        {

            get { return this.IsMasterValue; }

            set { SetProperty(ref IsMasterValue, value); }

        }
    }
}
