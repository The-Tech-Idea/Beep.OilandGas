using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Unitization
{
    public class CreateUnitAgreementRequest : ModelEntityBase
    {
        private string UnitNameValue;

        public string UnitName

        {

            get { return this.UnitNameValue; }

            set { SetProperty(ref UnitNameValue, value); }

        }
        private string UnitNumberValue;

        public string UnitNumber

        {

            get { return this.UnitNumberValue; }

            set { SetProperty(ref UnitNumberValue, value); }

        }
        private string UnitOperatorBaIdValue;

        public string UnitOperatorBaId

        {

            get { return this.UnitOperatorBaIdValue; }

            set { SetProperty(ref UnitOperatorBaIdValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

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
        private string TermsAndConditionsValue;

        public string TermsAndConditions

        {

            get { return this.TermsAndConditionsValue; }

            set { SetProperty(ref TermsAndConditionsValue, value); }

        }
    }

    public class CreateParticipatingAreaRequest : ModelEntityBase
    {
        private string UnitAgreementIdValue;

        public string UnitAgreementId

        {

            get { return this.UnitAgreementIdValue; }

            set { SetProperty(ref UnitAgreementIdValue, value); }

        }
        private string AreaNameValue;

        public string AreaName

        {

            get { return this.AreaNameValue; }

            set { SetProperty(ref AreaNameValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

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
    }

    public class CreateTractParticipationRequest : ModelEntityBase
    {
        private string ParticipatingAreaIdValue;

        public string ParticipatingAreaId

        {

            get { return this.ParticipatingAreaIdValue; }

            set { SetProperty(ref ParticipatingAreaIdValue, value); }

        }
        private string UnitAgreementIdValue;

        public string UnitAgreementId

        {

            get { return this.UnitAgreementIdValue; }

            set { SetProperty(ref UnitAgreementIdValue, value); }

        }
        private string TractIdValue;

        public string TractId

        {

            get { return this.TractIdValue; }

            set { SetProperty(ref TractIdValue, value); }

        }
        private decimal ParticipationPercentageValue;

        public decimal ParticipationPercentage

        {

            get { return this.ParticipationPercentageValue; }

            set { SetProperty(ref ParticipationPercentageValue, value); }

        }
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private decimal? TractAcreageValue;

        public decimal? TractAcreage

        {

            get { return this.TractAcreageValue; }

            set { SetProperty(ref TractAcreageValue, value); }

        }
    }

    public class UnitApprovalResult : ModelEntityBase
    {
        private string AgreementIdValue;

        public string AgreementId

        {

            get { return this.AgreementIdValue; }

            set { SetProperty(ref AgreementIdValue, value); }

        }
        private bool IsApprovedValue;

        public bool IsApproved

        {

            get { return this.IsApprovedValue; }

            set { SetProperty(ref IsApprovedValue, value); }

        }
        private string ApproverIdValue;

        public string ApproverId

        {

            get { return this.ApproverIdValue; }

            set { SetProperty(ref ApproverIdValue, value); }

        }
        private DateTime ApprovalDateValue = DateTime.UtcNow;

        public DateTime ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string CommentsValue;

        public string Comments

        {

            get { return this.CommentsValue; }

            set { SetProperty(ref CommentsValue, value); }

        }
    }

    public class UnitOperationsSummary : ModelEntityBase
    {
        private string UnitAgreementIdValue;

        public string UnitAgreementId

        {

            get { return this.UnitAgreementIdValue; }

            set { SetProperty(ref UnitAgreementIdValue, value); }

        }
        private string UnitNameValue;

        public string UnitName

        {

            get { return this.UnitNameValue; }

            set { SetProperty(ref UnitNameValue, value); }

        }
        private int ParticipatingAreaCountValue;

        public int ParticipatingAreaCount

        {

            get { return this.ParticipatingAreaCountValue; }

            set { SetProperty(ref ParticipatingAreaCountValue, value); }

        }
        private int TractCountValue;

        public int TractCount

        {

            get { return this.TractCountValue; }

            set { SetProperty(ref TractCountValue, value); }

        }
        private decimal TotalParticipationPercentageValue;

        public decimal TotalParticipationPercentage

        {

            get { return this.TotalParticipationPercentageValue; }

            set { SetProperty(ref TotalParticipationPercentageValue, value); }

        }
        private decimal TotalWorkingInterestValue;

        public decimal TotalWorkingInterest

        {

            get { return this.TotalWorkingInterestValue; }

            set { SetProperty(ref TotalWorkingInterestValue, value); }

        }
        private decimal TotalNetRevenueInterestValue;

        public decimal TotalNetRevenueInterest

        {

            get { return this.TotalNetRevenueInterestValue; }

            set { SetProperty(ref TotalNetRevenueInterestValue, value); }

        }
        private DateTime? LastProductionDateValue;

        public DateTime? LastProductionDate

        {

            get { return this.LastProductionDateValue; }

            set { SetProperty(ref LastProductionDateValue, value); }

        }
        private decimal? TotalProductionVolumeValue;

        public decimal? TotalProductionVolume

        {

            get { return this.TotalProductionVolumeValue; }

            set { SetProperty(ref TotalProductionVolumeValue, value); }

        }
    }
}








