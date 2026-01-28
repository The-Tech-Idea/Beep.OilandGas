using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class FieldDifference : ModelEntityBase
    {
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private object Source1ValueValue;

        public object Source1Value

        {

            get { return this.Source1ValueValue; }

            set { SetProperty(ref Source1ValueValue, value); }

        }
        private object Source2ValueValue;

        public object Source2Value

        {

            get { return this.Source2ValueValue; }

            set { SetProperty(ref Source2ValueValue, value); }

        }
        private string DifferenceTypeValue;

        public string DifferenceType

        {

            get { return this.DifferenceTypeValue; }

            set { SetProperty(ref DifferenceTypeValue, value); }

        } // Missing, Different, Extra
    }
}
