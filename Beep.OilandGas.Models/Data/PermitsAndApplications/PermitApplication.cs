using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class PermitApplication : ModelEntityBase
    {
        private string ApplicationIdValue = string.Empty;
        public string ApplicationId
        {
            get { return this.ApplicationIdValue; }
            set { SetProperty(ref ApplicationIdValue, value); }
        }
        private PermitApplicationType ApplicationTypeValue = PermitApplicationType.Other;
        public PermitApplicationType ApplicationType
        {
            get { return this.ApplicationTypeValue; }
            set { SetProperty(ref ApplicationTypeValue, value); }
        }
        private PermitApplicationStatus StatusValue = PermitApplicationStatus.Draft;
        public PermitApplicationStatus Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }
        private Country CountryValue = Country.Other;
        public Country Country
        {
            get { return this.CountryValue; }
            set { SetProperty(ref CountryValue, value); }
        }
        private StateProvince StateProvinceValue = StateProvince.Other;
        public StateProvince StateProvince
        {
            get { return this.StateProvinceValue; }
            set { SetProperty(ref StateProvinceValue, value); }
        }
        private RegulatoryAuthority RegulatoryAuthorityValue = RegulatoryAuthority.Other;
        public RegulatoryAuthority RegulatoryAuthority
        {
            get { return this.RegulatoryAuthorityValue; }
            set { SetProperty(ref RegulatoryAuthorityValue, value); }
        }
        private DateTime? CreatedDateValue;
        public DateTime? CreatedDate
        {
            get { return this.CreatedDateValue; }
            set { SetProperty(ref CreatedDateValue, value); }
        }
        private DateTime? SubmittedDateValue;
        public DateTime? SubmittedDate
        {
            get { return this.SubmittedDateValue; }
            set { SetProperty(ref SubmittedDateValue, value); }
        }
        private DateTime? ReceivedDateValue;
        public DateTime? ReceivedDate
        {
            get { return this.ReceivedDateValue; }
            set { SetProperty(ref ReceivedDateValue, value); }
        }
        private DateTime? DecisionDateValue;
        public DateTime? DecisionDate
        {
            get { return this.DecisionDateValue; }
            set { SetProperty(ref DecisionDateValue, value); }
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
        private string? DecisionValue;
        public string? Decision
        {
            get { return this.DecisionValue; }
            set { SetProperty(ref DecisionValue, value); }
        }
        private string? ReferenceNumberValue;
        public string? ReferenceNumber
        {
            get { return this.ReferenceNumberValue; }
            set { SetProperty(ref ReferenceNumberValue, value); }
        }
        private string? FeesDescriptionValue;
        public string? FeesDescription
        {
            get { return this.FeesDescriptionValue; }
            set { SetProperty(ref FeesDescriptionValue, value); }
        }
        private bool FeesPaidValue;
        public bool FeesPaid
        {
            get { return this.FeesPaidValue; }
            set { SetProperty(ref FeesPaidValue, value); }
        }
        private string? RemarksValue;
        public string? Remarks
        {
            get { return this.RemarksValue; }
            set { SetProperty(ref RemarksValue, value); }
        }
        private bool SubmissionCompleteValue;
        public bool SubmissionComplete
        {
            get { return this.SubmissionCompleteValue; }
            set { SetProperty(ref SubmissionCompleteValue, value); }
        }
        private string? SubmissionDescriptionValue;
        public string? SubmissionDescription
        {
            get { return this.SubmissionDescriptionValue; }
            set { SetProperty(ref SubmissionDescriptionValue, value); }
        }
        private List<APPLICATION_ATTACHMENT> AttachmentsValue = new();
        public List<APPLICATION_ATTACHMENT> Attachments
        {
            get { return this.AttachmentsValue; }
            set { SetProperty(ref AttachmentsValue, value); }
        }
        private List<APPLICATION_AREA> AreasValue = new();
        public List<APPLICATION_AREA> Areas
        {
            get { return this.AreasValue; }
            set { SetProperty(ref AreasValue, value); }
        }
        private List<APPLICATION_COMPONENT> ComponentsValue = new();
        public List<APPLICATION_COMPONENT> Components
        {
            get { return this.ComponentsValue; }
            set { SetProperty(ref ComponentsValue, value); }
        }

        public string ApplicantId { get; set; }
        public string OperatorId { get; set; }
        public string RelatedWellUwi { get; set; }
        public string RelatedFacilityId { get; set; }
    }
}
