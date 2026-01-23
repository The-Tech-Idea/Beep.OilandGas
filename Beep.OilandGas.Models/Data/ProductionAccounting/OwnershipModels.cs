using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Division order status.
    /// </summary>


    /// <summary>
    /// Represents a division order (DTO for calculations/reporting).
    /// </summary>
    public class DivisionOrder : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the division order identifier.
        /// </summary>
        private string DivisionOrderIdValue = string.Empty;

        public string DivisionOrderId

        {

            get { return this.DivisionOrderIdValue; }

            set { SetProperty(ref DivisionOrderIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        private string PropertyOrLeaseIdValue = string.Empty;

        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the owner information.
        /// </summary>
        private OwnerInformation OwnerValue = new();

        public OwnerInformation Owner

        {

            get { return this.OwnerValue; }

            set { SetProperty(ref OwnerValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the royalty interest (decimal, 0-1).
        /// </summary>
        private decimal? RoyaltyInterestValue;

        public decimal? RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the overriding royalty interest (decimal, 0-1).
        /// </summary>
        private decimal? OverridingRoyaltyInterestValue;

        public decimal? OverridingRoyaltyInterest

        {

            get { return this.OverridingRoyaltyInterestValue; }

            set { SetProperty(ref OverridingRoyaltyInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        private DivisionOrderStatus StatusValue = DivisionOrderStatus.Pending;

        public DivisionOrderStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

        /// <summary>
        /// Gets or sets the approval date.
        /// </summary>
        private DateTime? ApprovalDateValue;

        public DateTime? ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the approved by.
        /// </summary>
        private string? ApprovedByValue;

        public string? ApprovedBy

        {

            get { return this.ApprovedByValue; }

            set { SetProperty(ref ApprovedByValue, value); }

        }

        /// <summary>
        /// Gets or sets any notes or comments.
        /// </summary>
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    /// <summary>
    /// Represents owner information.
    /// </summary>
    public class OwnerInformation : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the owner identifier.
        /// </summary>
        private string OwnerIdValue = string.Empty;

        public string OwnerId

        {

            get { return this.OwnerIdValue; }

            set { SetProperty(ref OwnerIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the owner name.
        /// </summary>
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the tax identification number.
        /// </summary>
        private string? TaxIdValue;

        public string? TaxId

        {

            get { return this.TaxIdValue; }

            set { SetProperty(ref TaxIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        private Address? AddressValue;

        public Address? Address

        {

            get { return this.AddressValue; }

            set { SetProperty(ref AddressValue, value); }

        }

        /// <summary>
        /// Gets or sets the contact information.
        /// </summary>
        private ContactInformation? ContactValue;

        public ContactInformation? Contact

        {

            get { return this.ContactValue; }

            set { SetProperty(ref ContactValue, value); }

        }
    }

    /// <summary>
    /// Represents an address.
    /// </summary>
    public class Address : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        private string StreetAddressValue = string.Empty;

        public string StreetAddress

        {

            get { return this.StreetAddressValue; }

            set { SetProperty(ref StreetAddressValue, value); }

        }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        private string CityValue = string.Empty;

        public string City

        {

            get { return this.CityValue; }

            set { SetProperty(ref CityValue, value); }

        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        private string StateValue = string.Empty;

        public string State

        {

            get { return this.StateValue; }

            set { SetProperty(ref StateValue, value); }

        }

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        private string ZipCodeValue = string.Empty;

        public string ZipCode

        {

            get { return this.ZipCodeValue; }

            set { SetProperty(ref ZipCodeValue, value); }

        }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        private string CountryValue = "USA";

        public string Country

        {

            get { return this.CountryValue; }

            set { SetProperty(ref CountryValue, value); }

        }
    }

    /// <summary>
    /// Represents contact information.
    /// </summary>
    public class ContactInformation : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        private string? PhoneNumberValue;

        public string? PhoneNumber

        {

            get { return this.PhoneNumberValue; }

            set { SetProperty(ref PhoneNumberValue, value); }

        }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        private string? EmailAddressValue;

        public string? EmailAddress

        {

            get { return this.EmailAddressValue; }

            set { SetProperty(ref EmailAddressValue, value); }

        }
    }

    /// <summary>
    /// Represents a transfer order.
    /// </summary>
    public class TransferOrder : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the transfer order identifier.
        /// </summary>
        private string TransferOrderIdValue = string.Empty;

        public string TransferOrderId

        {

            get { return this.TransferOrderIdValue; }

            set { SetProperty(ref TransferOrderIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        private string PropertyOrLeaseIdValue = string.Empty;

        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the from owner.
        /// </summary>
        private OwnerInformation FromOwnerValue = new();

        public OwnerInformation FromOwner

        {

            get { return this.FromOwnerValue; }

            set { SetProperty(ref FromOwnerValue, value); }

        }

        /// <summary>
        /// Gets or sets the to owner.
        /// </summary>
        private OwnerInformation ToOwnerValue = new();

        public OwnerInformation ToOwner

        {

            get { return this.ToOwnerValue; }

            set { SetProperty(ref ToOwnerValue, value); }

        }

        /// <summary>
        /// Gets or sets the interest transferred (decimal, 0-1).
        /// </summary>
        private decimal InterestTransferredValue;

        public decimal InterestTransferred

        {

            get { return this.InterestTransferredValue; }

            set { SetProperty(ref InterestTransferredValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the approval status.
        /// </summary>
        private bool IsApprovedValue;

        public bool IsApproved

        {

            get { return this.IsApprovedValue; }

            set { SetProperty(ref IsApprovedValue, value); }

        }

        /// <summary>
        /// Gets or sets the approval date.
        /// </summary>
        private DateTime? ApprovalDateValue;

        public DateTime? ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the approved by.
        /// </summary>
        private string? ApprovedByValue;

        public string? ApprovedBy

        {

            get { return this.ApprovedByValue; }

            set { SetProperty(ref ApprovedByValue, value); }

        }
    }

    /// <summary>
    /// Represents an ownership interest.
    /// </summary>
    public class OwnershipInterest : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the ownership identifier.
        /// </summary>
        private string OwnershipIdValue = string.Empty;

        public string OwnershipId

        {

            get { return this.OwnershipIdValue; }

            set { SetProperty(ref OwnershipIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the owner identifier.
        /// </summary>
        private string OwnerIdValue = string.Empty;

        public string OwnerId

        {

            get { return this.OwnerIdValue; }

            set { SetProperty(ref OwnerIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        private string PropertyOrLeaseIdValue = string.Empty;

        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the royalty interest (decimal, 0-1).
        /// </summary>
        private decimal? RoyaltyInterestValue;

        public decimal? RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the overriding royalty interest (decimal, 0-1).
        /// </summary>
        private decimal? OverridingRoyaltyInterestValue;

        public decimal? OverridingRoyaltyInterest

        {

            get { return this.OverridingRoyaltyInterestValue; }

            set { SetProperty(ref OverridingRoyaltyInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective start date.
        /// </summary>
        private DateTime EffectiveStartDateValue;

        public DateTime EffectiveStartDate

        {

            get { return this.EffectiveStartDateValue; }

            set { SetProperty(ref EffectiveStartDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective end date.
        /// </summary>
        private DateTime? EffectiveEndDateValue;

        public DateTime? EffectiveEndDate

        {

            get { return this.EffectiveEndDateValue; }

            set { SetProperty(ref EffectiveEndDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the division order reference.
        /// </summary>
        private string? DivisionOrderIdValue;

        public string? DivisionOrderId

        {

            get { return this.DivisionOrderIdValue; }

            set { SetProperty(ref DivisionOrderIdValue, value); }

        }
    }
}








