namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Allocation method enumeration for internal calculation use.
    /// </summary>
    public enum AllocationMethod
    {
        /// <summary>
        /// Equal allocation to all parties.
        /// </summary>
        Equal,

        /// <summary>
        /// Pro-rata allocation based on working interest.
        /// </summary>
        ProRataWorkingInterest,

        /// <summary>
        /// Pro-rata allocation based on net revenue interest.
        /// </summary>
        ProRataNetRevenueInterest,

        /// <summary>
        /// Measured allocation based on test data.
        /// </summary>
        Measured,

        /// <summary>
        /// Estimated allocation based on production history.
        /// </summary>
        Estimated
    }

    /// <summary>
    /// Nomination status enumeration.
    /// </summary>
    public enum NominationStatus
    {
        /// <summary>
        /// Pending approval.
        /// </summary>
        Pending,

        /// <summary>
        /// Approved.
        /// </summary>
        Approved,

        /// <summary>
        /// Rejected.
        /// </summary>
        Rejected,

        /// <summary>
        /// Cancelled.
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Imbalance status enumeration.
    /// </summary>
    public enum ImbalanceStatus
    {
        /// <summary>
        /// Balanced (within tolerance).
        /// </summary>
        Balanced,

        /// <summary>
        /// Over-delivered (actual > nominated).
        /// </summary>
        OverDelivered,

        /// <summary>
        /// Under-delivered (actual < nominated).
        /// </summary>
        UnderDelivered,

        /// <summary>
        /// Pending reconciliation.
        /// </summary>
        PendingReconciliation,

        /// <summary>
        /// Reconciled.
        /// </summary>
        Reconciled
    }

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
    /// Pricing method enumeration.
    /// </summary>
    public enum PricingMethod
    {
        /// <summary>
        /// Fixed price per barrel.
        /// </summary>
        Fixed,

        /// <summary>
        /// Index-based pricing (WTI, Brent, etc.).
        /// </summary>
        IndexBased,

        /// <summary>
        /// Posted price.
        /// </summary>
        PostedPrice,

        /// <summary>
        /// Spot price.
        /// </summary>
        SpotPrice,

        /// <summary>
        /// Regulated pricing.
        /// </summary>
        Regulated
    }

    /// <summary>
    /// Payment method enumeration.
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Check payment.
        /// </summary>
        Check,

        /// <summary>
        /// Wire transfer.
        /// </summary>
        WireTransfer,

        /// <summary>
        /// ACH (Automated Clearing House).
        /// </summary>
        ACH,

        /// <summary>
        /// Direct deposit.
        /// </summary>
        DirectDeposit
    }

    /// <summary>
    /// Payment status enumeration.
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Pending payment.
        /// </summary>
        Pending,

        /// <summary>
        /// Paid.
        /// </summary>
        Paid,

        /// <summary>
        /// Suspended.
        /// </summary>
        Suspended,

        /// <summary>
        /// Cancelled.
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Tax withholding type enumeration.
    /// </summary>
    public enum TaxWithholdingType
    {
        /// <summary>
        /// Invalid tax ID withholding.
        /// </summary>
        InvalidTaxId,

        /// <summary>
        /// Out of state withholding.
        /// </summary>
        OutOfState,

        /// <summary>
        /// Alien withholding.
        /// </summary>
        Alien,

        /// <summary>
        /// Backup withholding.
        /// </summary>
        BackupWithholding
    }

    /// <summary>
    /// Measurement standard organization.
    /// </summary>
    public enum MeasurementStandard
    {
        /// <summary>
        /// American Petroleum Institute standards.
        /// </summary>
        API,

        /// <summary>
        /// American Gas Association standards.
        /// </summary>
        AGA,

        /// <summary>
        /// International Organization for Standardization.
        /// </summary>
        ISO
    }

    /// <summary>
    /// Method of measurement.
    /// </summary>
    public enum MeasurementMethod
    {
        /// <summary>
        /// Manual measurement (tank gauging, manual sampling).
        /// </summary>
        Manual,

        /// <summary>
        /// Automatic metering (flow meters).
        /// </summary>
        Automatic,

        /// <summary>
        /// Automatic Custody Transfer.
        /// </summary>
        ACT,

        /// <summary>
        /// Lease Automatic Custody Transfer.
        /// </summary>
        LACT
    }

    /// <summary>
    /// Type of exchange contract.
    /// </summary>
    public enum ExchangeContractType
    {
        /// <summary>
        /// Physical exchange (crude for crude).
        /// </summary>
        PhysicalExchange,

        /// <summary>
        /// Buy/sell exchange (buy at one location, sell at another).
        /// </summary>
        BuySellExchange,

        /// <summary>
        /// Multi-party exchange.
        /// </summary>
        MultiPartyExchange,

        /// <summary>
        /// Time exchange (exchange volumes across time periods).
        /// </summary>
        TimeExchange
    }

    /// <summary>
    /// Type of exchange commitment.
    /// </summary>
    public enum ExchangeCommitmentType
    {
        /// <summary>
        /// Current month commitment.
        /// </summary>
        CurrentMonth,

        /// <summary>
        /// Forward month commitment.
        /// </summary>
        ForwardMonth,

        /// <summary>
        /// Annual commitment.
        /// </summary>
        Annual
    }

    /// <summary>
    /// Exchange commitment status.
    /// </summary>
    public enum ExchangeCommitmentStatus
    {
        /// <summary>
        /// Pending fulfillment.
        /// </summary>
        Pending,

        /// <summary>
        /// Partially fulfilled.
        /// </summary>
        PartiallyFulfilled,

        /// <summary>
        /// Fully fulfilled.
        /// </summary>
        Fulfilled,

        /// <summary>
        /// Cancelled.
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Report type enumeration.
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// Internal operational report.
        /// </summary>
        InternalOperational,

        /// <summary>
        /// Internal lease report.
        /// </summary>
        InternalLease,

        /// <summary>
        /// Governmental report (federal, state, local).
        /// </summary>
        Governmental,

        /// <summary>
        /// Joint interest statement.
        /// </summary>
        JointInterest,

        /// <summary>
        /// Royalty owner statement.
        /// </summary>
        RoyaltyOwner
    }
}



