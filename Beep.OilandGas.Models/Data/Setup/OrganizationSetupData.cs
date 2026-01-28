using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Setup
{
    public class OrganizationSetupData : ModelEntityBase
    {
        private string BusinessAssociateIdValue = string.Empty;

        [Required]
        public string BusinessAssociateId

        {

            get { return this.BusinessAssociateIdValue; }

            set { SetProperty(ref BusinessAssociateIdValue, value); }

        }

        private string OrganizationIdValue = string.Empty;

        [Required]
        public string OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }

        private int OrganizationSeqNoValue = 1;

        [Required]
        public int OrganizationSeqNo

        {

            get { return this.OrganizationSeqNoValue; }

            set { SetProperty(ref OrganizationSeqNoValue, value); }

        }

        private string OrganizationNameValue = string.Empty;

        [Required]
        public string OrganizationName

        {

            get { return this.OrganizationNameValue; }

            set { SetProperty(ref OrganizationNameValue, value); }

        }

        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }

        private string? OrganizationTypeValue;

        public string? OrganizationType

        {

            get { return this.OrganizationTypeValue; }

            set { SetProperty(ref OrganizationTypeValue, value); }

        }

        private string? AreaIdValue;

        public string? AreaId

        {

            get { return this.AreaIdValue; }

            set { SetProperty(ref AreaIdValue, value); }

        }

        private string? AreaTypeValue;

        public string? AreaType

        {

            get { return this.AreaTypeValue; }

            set { SetProperty(ref AreaTypeValue, value); }

        }

        private DateTime? CreatedDateValue;

        public DateTime? CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }

        private DateTime? EffectiveDateValue;

        public DateTime? EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        private DateTime? ExpiryDateValue;

        public DateTime? ExpiryDate

        {

            get { return this.ExpiryDateValue; }

            set { SetProperty(ref ExpiryDateValue, value); }

        }

        private string ActiveIndValue = "Y";

        public string ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? PPDMGuidValue;

        public string? PPDMGuid

        {

            get { return this.PPDMGuidValue; }

            set { SetProperty(ref PPDMGuidValue, value); }

        }
    }
}
