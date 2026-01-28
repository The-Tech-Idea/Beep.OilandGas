using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ReconciliationDifference : ModelEntityBase
    {
        private object RecordIdValue;

        public object RecordId

        {

            get { return this.RecordIdValue; }

            set { SetProperty(ref RecordIdValue, value); }

        }
        private List<FieldDifference> FieldDifferencesValue = new List<FieldDifference>();

        public List<FieldDifference> FieldDifferences

        {

            get { return this.FieldDifferencesValue; }

            set { SetProperty(ref FieldDifferencesValue, value); }

        }
        private string Source1ValueValue;

        public string Source1Value

        {

            get { return this.Source1ValueValue; }

            set { SetProperty(ref Source1ValueValue, value); }

        }
        private string Source2ValueValue;

        public string Source2Value

        {

            get { return this.Source2ValueValue; }

            set { SetProperty(ref Source2ValueValue, value); }

        }
    }
}
