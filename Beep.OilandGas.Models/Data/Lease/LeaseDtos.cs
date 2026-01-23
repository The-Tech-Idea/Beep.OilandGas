using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Lease
{
    /// <summary>
    /// DTO for lease information.
    /// </summary>
    public class Lease : ModelEntityBase
    {
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string LeaseNumberValue = string.Empty;

        public string LeaseNumber

        {

            get { return this.LeaseNumberValue; }

            set { SetProperty(ref LeaseNumberValue, value); }

        }
        private string? LessorNameValue;

        public string? LessorName

        {

            get { return this.LessorNameValue; }

            set { SetProperty(ref LessorNameValue, value); }

        }
        private string? LessorIdValue;

        public string? LessorId

        {

            get { return this.LessorIdValue; }

            set { SetProperty(ref LessorIdValue, value); }

        }
        private string? LesseeNameValue;

        public string? LesseeName

        {

            get { return this.LesseeNameValue; }

            set { SetProperty(ref LesseeNameValue, value); }

        }
        private string? LesseeIdValue;

        public string? LesseeId

        {

            get { return this.LesseeIdValue; }

            set { SetProperty(ref LesseeIdValue, value); }

        }
        private DateTime? LeaseDateValue;

        public DateTime? LeaseDate

        {

            get { return this.LeaseDateValue; }

            set { SetProperty(ref LeaseDateValue, value); }

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
        private decimal LeaseAreaValue;

        public decimal LeaseArea

        {

            get { return this.LeaseAreaValue; }

            set { SetProperty(ref LeaseAreaValue, value); }

        }
        private string? AreaUnitValue;

        public string? AreaUnit

        {

            get { return this.AreaUnitValue; }

            set { SetProperty(ref AreaUnitValue, value); }

        }
        private string? LocationValue;

        public string? Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
        private string? LegalDescriptionValue;

        public string? LegalDescription

        {

            get { return this.LegalDescriptionValue; }

            set { SetProperty(ref LegalDescriptionValue, value); }

        }
        private decimal RoyaltyRateValue;

        public decimal RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }
        private decimal BonusPaymentValue;

        public decimal BonusPayment

        {

            get { return this.BonusPaymentValue; }

            set { SetProperty(ref BonusPaymentValue, value); }

        }
        private decimal AnnualRentalValue;

        public decimal AnnualRental

        {

            get { return this.AnnualRentalValue; }

            set { SetProperty(ref AnnualRentalValue, value); }

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
        private List<LandRight> LandRightsValue = new();

        public List<LandRight> LandRights

        {

            get { return this.LandRightsValue; }

            set { SetProperty(ref LandRightsValue, value); }

        }
        private List<MineralRight> MineralRightsValue = new();

        public List<MineralRight> MineralRights

        {

            get { return this.MineralRightsValue; }

            set { SetProperty(ref MineralRightsValue, value); }

        }
        private string? LeaseNameValue;

        public string? LeaseName

        {

            get { return this.LeaseNameValue; }

            set { SetProperty(ref LeaseNameValue, value); }

        }
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private object StartDateValue;

        public object StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private object EndDateValue;

        public object EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }

    /// <summary>
    /// DTO for land rights.
    /// </summary>
    public class LandRight : ModelEntityBase
    {
        private string LandRightIdValue = string.Empty;

        public string LandRightId

        {

            get { return this.LandRightIdValue; }

            set { SetProperty(ref LandRightIdValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string RightTypeValue = string.Empty;

        public string RightType

        {

            get { return this.RightTypeValue; }

            set { SetProperty(ref RightTypeValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

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

    /// <summary>
    /// DTO for mineral rights.
    /// </summary>
    public class MineralRight : ModelEntityBase
    {
        private string MineralRightIdValue = string.Empty;

        public string MineralRightId

        {

            get { return this.MineralRightIdValue; }

            set { SetProperty(ref MineralRightIdValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string OwnerIdValue = string.Empty;

        public string OwnerId

        {

            get { return this.OwnerIdValue; }

            set { SetProperty(ref OwnerIdValue, value); }

        }
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

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
        private decimal RoyaltyInterestValue;

        public decimal RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

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
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// DTO for surface agreements.
    /// </summary>
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

    /// <summary>
    /// DTO for royalty information.
    /// </summary>
    public class Royalty : ModelEntityBase
    {
        private string RoyaltyIdValue = string.Empty;

        public string RoyaltyId

        {

            get { return this.RoyaltyIdValue; }

            set { SetProperty(ref RoyaltyIdValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string OwnerIdValue = string.Empty;

        public string OwnerId

        {

            get { return this.OwnerIdValue; }

            set { SetProperty(ref OwnerIdValue, value); }

        }
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }
        private decimal RoyaltyRateValue;

        public decimal RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }
        private string? RoyaltyTypeValue;

        public string? RoyaltyType

        {

            get { return this.RoyaltyTypeValue; }

            set { SetProperty(ref RoyaltyTypeValue, value); }

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
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// DTO for creating a new lease.
    /// </summary>
    public class CreateLease : ModelEntityBase
    {
        private string LeaseNumberValue = string.Empty;

        public string LeaseNumber

        {

            get { return this.LeaseNumberValue; }

            set { SetProperty(ref LeaseNumberValue, value); }

        }
        private string? LessorIdValue;

        public string? LessorId

        {

            get { return this.LessorIdValue; }

            set { SetProperty(ref LessorIdValue, value); }

        }
        private string? LesseeIdValue;

        public string? LesseeId

        {

            get { return this.LesseeIdValue; }

            set { SetProperty(ref LesseeIdValue, value); }

        }
        private DateTime? LeaseDateValue;

        public DateTime? LeaseDate

        {

            get { return this.LeaseDateValue; }

            set { SetProperty(ref LeaseDateValue, value); }

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
        private decimal LeaseAreaValue;

        public decimal LeaseArea

        {

            get { return this.LeaseAreaValue; }

            set { SetProperty(ref LeaseAreaValue, value); }

        }
        private string? AreaUnitValue;

        public string? AreaUnit

        {

            get { return this.AreaUnitValue; }

            set { SetProperty(ref AreaUnitValue, value); }

        }
        private string? LocationValue;

        public string? Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
        private string? LegalDescriptionValue;

        public string? LegalDescription

        {

            get { return this.LegalDescriptionValue; }

            set { SetProperty(ref LegalDescriptionValue, value); }

        }
        private decimal RoyaltyRateValue;

        public decimal RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }
        private decimal BonusPaymentValue;

        public decimal BonusPayment

        {

            get { return this.BonusPaymentValue; }

            set { SetProperty(ref BonusPaymentValue, value); }

        }
        private decimal AnnualRentalValue;

        public decimal AnnualRental

        {

            get { return this.AnnualRentalValue; }

            set { SetProperty(ref AnnualRentalValue, value); }

        }
        private object StartDateValue;

        public object StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private object EndDateValue;

        public object EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }

    /// <summary>
    /// DTO for updating a lease.
    /// </summary>
    public class UpdateLease : ModelEntityBase
    {
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
        private decimal AnnualRentalValue;

        public decimal AnnualRental

        {

            get { return this.AnnualRentalValue; }

            set { SetProperty(ref AnnualRentalValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }
}








