using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class MaskingRule : ModelEntityBase
    {
        private MaskingType TypeValue;

        public MaskingType Type

        {

            get { return this.TypeValue; }

            set { SetProperty(ref TypeValue, value); }

        }
        private string MaskingValueValue;

        public string MaskingValue

        {

            get { return this.MaskingValueValue; }

            set { SetProperty(ref MaskingValueValue, value); }

        }
        private bool PreserveFormatValue = true;

        public bool PreserveFormat

        {

            get { return this.PreserveFormatValue; }

            set { SetProperty(ref PreserveFormatValue, value); }

        }
        private int? PreserveLengthValue;

        public int? PreserveLength

        {

            get { return this.PreserveLengthValue; }

            set { SetProperty(ref PreserveLengthValue, value); }

        }
    }
}
