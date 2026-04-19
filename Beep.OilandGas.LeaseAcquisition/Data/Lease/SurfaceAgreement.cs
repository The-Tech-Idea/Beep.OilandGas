using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class SurfaceAgreement : ModelEntityBase
    {
        private string AgreementIdValue = string.Empty;

        public string AgreementId

        {

            get { return this.AgreementIdValue; }

            set { SetProperty(ref AgreementIdValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string AgreementTypeValue = string.Empty;

        public string AgreementType

        {

            get { return this.AgreementTypeValue; }

            set { SetProperty(ref AgreementTypeValue, value); }

        }
        private string? SurfaceOwnerIdValue;

        public string? SurfaceOwnerId

        {

            get { return this.SurfaceOwnerIdValue; }

            set { SetProperty(ref SurfaceOwnerIdValue, value); }

        }
        private string? SurfaceOwnerNameValue;

        public string? SurfaceOwnerName

        {

            get { return this.SurfaceOwnerNameValue; }

            set { SetProperty(ref SurfaceOwnerNameValue, value); }

        }
        private DateTime? AgreementDateValue;

        public DateTime? AgreementDate

        {

            get { return this.AgreementDateValue; }

            set { SetProperty(ref AgreementDateValue, value); }

        }
        private DateTime? EffectiveDateValue;

        public DateTime? EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
        private decimal CompensationAmountValue;

        public decimal CompensationAmount

        {

            get { return this.CompensationAmountValue; }

            set { SetProperty(ref CompensationAmountValue, value); }

        }
        private string? CompensationTypeValue;

        public string? CompensationType

        {

            get { return this.CompensationTypeValue; }

            set { SetProperty(ref CompensationTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }
}
