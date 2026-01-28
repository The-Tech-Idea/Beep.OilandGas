using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class MaskingStrategy : ModelEntityBase
    {
        public Dictionary<string, MaskingRule> FieldRules { get; set; } = new Dictionary<string, MaskingRule>();
        private bool PreserveFormatValue = true;

        public bool PreserveFormat

        {

            get { return this.PreserveFormatValue; }

            set { SetProperty(ref PreserveFormatValue, value); }

        }
        private string DefaultMaskingValueValue = "***";

        public string DefaultMaskingValue

        {

            get { return this.DefaultMaskingValueValue; }

            set { SetProperty(ref DefaultMaskingValueValue, value); }

        }
    }
}
