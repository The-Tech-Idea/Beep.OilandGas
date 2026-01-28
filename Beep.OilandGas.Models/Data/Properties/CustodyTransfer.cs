using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class CustodyTransfer : ModelEntityBase
    {
        private DateTime TransferDateValue;

        public DateTime TransferDate

        {

            get { return this.TransferDateValue; }

            set { SetProperty(ref TransferDateValue, value); }

        }
        private string FromCustodianValue = string.Empty;

        public string FromCustodian

        {

            get { return this.FromCustodianValue; }

            set { SetProperty(ref FromCustodianValue, value); }

        }
        private string ToCustodianValue = string.Empty;

        public string ToCustodian

        {

            get { return this.ToCustodianValue; }

            set { SetProperty(ref ToCustodianValue, value); }

        }
        private string TransferReasonValue = string.Empty;

        public string TransferReason

        {

            get { return this.TransferReasonValue; }

            set { SetProperty(ref TransferReasonValue, value); }

        }
        private string DocumentationValue = string.Empty;

        public string Documentation

        {

            get { return this.DocumentationValue; }

            set { SetProperty(ref DocumentationValue, value); }

        }
        private string TransferConditionValue = string.Empty;

        public string TransferCondition

        {

            get { return this.TransferConditionValue; }

            set { SetProperty(ref TransferConditionValue, value); }

        }
    }
}
