using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class ComplianceRequest : ModelEntityBase
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
        private string ComplianceTypeValue = string.Empty;

        public string ComplianceType

        {

            get { return this.ComplianceTypeValue; }

            set { SetProperty(ref ComplianceTypeValue, value); }

        }
        private DateTime ComplianceDateValue;

        public DateTime ComplianceDate

        {

            get { return this.ComplianceDateValue; }

            set { SetProperty(ref ComplianceDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? ComplianceData { get; set; }
    }
}
