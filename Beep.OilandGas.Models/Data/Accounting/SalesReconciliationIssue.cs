using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class SalesReconciliationIssue : ModelEntityBase
    {
        private string IssueTypeValue;

        public string IssueType

        {

            get { return this.IssueTypeValue; }

            set { SetProperty(ref IssueTypeValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string ReferenceIdValue;

        public string ReferenceId

        {

            get { return this.ReferenceIdValue; }

            set { SetProperty(ref ReferenceIdValue, value); }

        }
        private decimal? AmountValue;

        public decimal? Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private decimal? VolumeValue;

        public decimal? Volume

        {

            get { return this.VolumeValue; }

            set { SetProperty(ref VolumeValue, value); }

        }
    }
}
