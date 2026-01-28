using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Validation
{
    public class ValidationIssue : ModelEntityBase
    {
        private string FieldValue;

        public string Field

        {

            get { return this.FieldValue; }

            set { SetProperty(ref FieldValue, value); }

        }
        private string MessageValue;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
    }
}
