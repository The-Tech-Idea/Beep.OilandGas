namespace Beep.OilandGas.ProductionAccounting.Ownership
{
    /// <summary>
    /// Division order status.
    /// </summary>
    public enum DivisionOrderStatus
    {
        /// <summary>
        /// Pending approval.
        /// </summary>
        Pending,

        /// <summary>
        /// Approved and active.
        /// </summary>
        Approved,

        /// <summary>
        /// Suspended.
        /// </summary>
        Suspended,

        /// <summary>
        /// Rejected.
        /// </summary>
        Rejected
    }

    /// <summary>
    /// Represents a division order.
    /// </summary>
    public class DivisionOrder
    {
        /// <summary>
        /// Gets or sets the division order identifier.
        /// </summary>
        public string DivisionOrderId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        public string PropertyOrLeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the owner information.
        /// </summary>
        public OwnerInformation Owner { get; set; } = new();

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        public decimal WorkingInterest { get; set; }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        public decimal NetRevenueInterest { get; set; }

        /// <summary>
        /// Gets or sets the royalty interest (decimal, 0-1).
        /// </summary>
        public decimal? RoyaltyInterest { get; set; }

        /// <summary>
        /// Gets or sets the overriding royalty interest (decimal, 0-1).
        /// </summary>
        public decimal? OverridingRoyaltyInterest { get; set; }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public DivisionOrderStatus Status { get; set; } = DivisionOrderStatus.Pending;

        /// <summary>
        /// Gets or sets the approval date.
        /// </summary>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// Gets or sets the approved by.
        /// </summary>
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Gets or sets any notes or comments.
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Represents owner information.
    /// </summary>
    public class OwnerInformation
    {
        /// <summary>
        /// Gets or sets the owner identifier.
        /// </summary>
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the owner name.
        /// </summary>
        public string OwnerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tax identification number.
        /// </summary>
        public string? TaxId { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public Address? Address { get; set; }

        /// <summary>
        /// Gets or sets the contact information.
        /// </summary>
        public ContactInformation? Contact { get; set; }
    }

    /// <summary>
    /// Represents an address.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        public string StreetAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        public string Country { get; set; } = "USA";
    }

    /// <summary>
    /// Represents contact information.
    /// </summary>
    public class ContactInformation
    {
        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string? EmailAddress { get; set; }
    }

    /// <summary>
    /// Represents a transfer order.
    /// </summary>
    public class TransferOrder
    {
        /// <summary>
        /// Gets or sets the transfer order identifier.
        /// </summary>
        public string TransferOrderId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        public string PropertyOrLeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the from owner.
        /// </summary>
        public OwnerInformation FromOwner { get; set; } = new();

        /// <summary>
        /// Gets or sets the to owner.
        /// </summary>
        public OwnerInformation ToOwner { get; set; } = new();

        /// <summary>
        /// Gets or sets the interest transferred (decimal, 0-1).
        /// </summary>
        public decimal InterestTransferred { get; set; }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the approval status.
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the approval date.
        /// </summary>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// Gets or sets the approved by.
        /// </summary>
        public string? ApprovedBy { get; set; }
    }

    /// <summary>
    /// Represents an ownership interest.
    /// </summary>
    public class OwnershipInterest
    {
        /// <summary>
        /// Gets or sets the ownership identifier.
        /// </summary>
        public string OwnershipId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the owner identifier.
        /// </summary>
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        public string PropertyOrLeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        public decimal WorkingInterest { get; set; }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        public decimal NetRevenueInterest { get; set; }

        /// <summary>
        /// Gets or sets the royalty interest (decimal, 0-1).
        /// </summary>
        public decimal? RoyaltyInterest { get; set; }

        /// <summary>
        /// Gets or sets the overriding royalty interest (decimal, 0-1).
        /// </summary>
        public decimal? OverridingRoyaltyInterest { get; set; }

        /// <summary>
        /// Gets or sets the effective start date.
        /// </summary>
        public DateTime EffectiveStartDate { get; set; }

        /// <summary>
        /// Gets or sets the effective end date.
        /// </summary>
        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>
        /// Gets or sets the division order reference.
        /// </summary>
        public string? DivisionOrderId { get; set; }
    }
}
