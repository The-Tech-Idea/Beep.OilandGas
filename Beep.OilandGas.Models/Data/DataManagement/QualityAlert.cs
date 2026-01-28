using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class QualityAlert : ModelEntityBase
    {
        private string AlertIdValue;

        public string AlertId

        {

            get { return this.AlertIdValue; }

            set { SetProperty(ref AlertIdValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private QualityAlertSeverity SeverityValue;

        public QualityAlertSeverity Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        }
        private string AlertMessageValue;

        public string AlertMessage

        {

            get { return this.AlertMessageValue; }

            set { SetProperty(ref AlertMessageValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private bool IsResolvedValue;

        public bool IsResolved

        {

            get { return this.IsResolvedValue; }

            set { SetProperty(ref IsResolvedValue, value); }

        }
        private DateTime? ResolvedDateValue;

        public DateTime? ResolvedDate

        {

            get { return this.ResolvedDateValue; }

            set { SetProperty(ref ResolvedDateValue, value); }

        }

        public object SEVERITY { get; set; }
    }
}
