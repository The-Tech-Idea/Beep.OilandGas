namespace Beep.OilandGas.Models.Constants;

/// <summary>
/// Accounting reference values — the canonical set of codes for all accounting LOVs.
///
/// These constants ARE the reference values. They serve dual purpose:
/// (a) Seeded into PPDM R_* tables at setup as the initial LOV entries
/// (b) Referenced directly in code at runtime — no magic strings, no DB round-trips
///
/// Example: a Vendor is always created with:
///   ba.BA_CATEGORY = AccountingReferenceCodes.VendorLOVCodes.CategoryVendor;
///
/// Users may extend R_* tables with additional values (e.g., add "TRANSPORTER" category).
/// Those user-added values exist only in the database and are surfaced in UI dropdowns.
/// The standard workflow-driving values are always referenced through these constants.
/// </summary>
public static class AccountingReferenceCodes
{
    /// <summary>BA_CATEGORY values for BUSINESS_ASSOCIATE classification.</summary>
    public static class VendorLOVCodes
    {
        public const string CategoryVendor              = "VENDOR";
        public const string CategoryCustomer            = "CUSTOMER";
        public const string CategoryWIOwner             = "WORKING_INTEREST_OWNER";
        public const string CategoryRoyaltyOwner        = "ROYALTY_OWNER";
        public const string CategoryGovernment          = "GOVERNMENT";
        public const string CategoryEmployee            = "EMPLOYEE";
        public const string CategoryFinancialInstitution = "FINANCIAL_INSTITUTION";
        public const string CategoryPartner             = "PARTNER";

        public static readonly string[] All = { CategoryVendor, CategoryCustomer, CategoryWIOwner, CategoryRoyaltyOwner, CategoryGovernment, CategoryEmployee, CategoryFinancialInstitution, CategoryPartner };
    }

    /// <summary>BA_TYPE values — sub-classification within each BA_CATEGORY.</summary>
    public static class VendorTypeCodes
    {
        // Vendor types
        public const string Supplier           = "SUPPLIER";
        public const string ServiceProvider    = "SERVICE_PROVIDER";
        public const string UtilityProvider    = "UTILITY_PROVIDER";
        public const string LandOwner          = "LAND_OWNER";
        // Customer types
        public const string Purchaser          = "PURCHASER";
        public const string Operator           = "OPERATOR";
        public const string Marketer           = "MARKETER";
        public const string MidstreamCompany   = "MIDSTREAM_COMPANY";
        // WI Owner
        public const string Partner            = "PARTNER";
        public const string NonConsentingParty = "NON_CONSENTING_PARTY";
        // Royalty Owner
        public const string MineralOwner       = "MINERAL_OWNER";
        public const string ORRIHolder         = "ORRI_HOLDER";
        public const string NPIHolder          = "NPI_HOLDER";
        public const string WI_Owner           = "WORKING_INTEREST_OWNER";
        // Government
        public const string TaxAuthority       = "TAX_AUTHORITY";
        public const string Regulator          = "REGULATOR";
        public const string HostGovernment     = "HOST_GOVERNMENT";
        // Employee
        public const string Staff              = "STAFF";
        public const string Contractor         = "CONTRACTOR";
        // Financial Institution
        public const string Bank               = "BANK";
        public const string Lender             = "LENDER";
        public const string Investor           = "INVESTOR";

        public static readonly string[] All = { Supplier, ServiceProvider, UtilityProvider, LandOwner, Purchaser, Operator, Marketer, MidstreamCompany, Partner, NonConsentingParty, MineralOwner, ORRIHolder, NPIHolder, WI_Owner, TaxAuthority, Regulator, HostGovernment, Staff, Contractor, Bank, Lender, Investor };
    }

    /// <summary>BA Preference Type codes — stored in BA_PREFERENCE.PREFERENCE_TYPE.</summary>
    public static class PreferenceTypeCodes
    {
        public const string PaymentTerms       = "PAYMENT_TERMS";
        public const string CreditLimit        = "CREDIT_LIMIT";
        public const string BankingInfo        = "BANKING_INFO";
        public const string TaxWithholding     = "TAX_WITHHOLDING";
        public const string Currency           = "CURRENCY";
        public const string PriceIndex         = "PRICE_INDEX";
        public const string DeliveryPoint      = "DELIVERY_POINT";
        public const string InvoiceMethod      = "INVOICE_METHOD";
        public const string PaymentMethod      = "PAYMENT_METHOD";
        public const string StatementFrequency = "STATEMENT_FREQUENCY";
        public const string Language           = "LANGUAGE";

        public static readonly string[] All = { PaymentTerms, CreditLimit, BankingInfo, TaxWithholding, Currency, PriceIndex, DeliveryPoint, InvoiceMethod, PaymentMethod, StatementFrequency, Language };
    }

    /// <summary>BA Status codes.</summary>
    public static class BAStatusCodes
    {
        public const string Active      = "ACTIVE";
        public const string Inactive    = "INACTIVE";
        public const string Suspended   = "SUSPENDED";
        public const string Pending     = "PENDING";
        public const string Blacklisted = "BLACKLISTED";
        public const string UnderReview = "UNDER_REVIEW";

        public static readonly string[] All = { Active, Inactive, Suspended, Pending, Blacklisted, UnderReview };
    }

    /// <summary>Payment term codes (Net 30, Net 60, etc.).</summary>
    public static class PaymentTermCodes
    {
        public const string DueOnReceipt = "DUE_ON_RECEIPT";
        public const string Net10        = "NET_10";
        public const string Net15        = "NET_15";
        public const string Net30        = "NET_30";
        public const string Net45        = "NET_45";
        public const string Net60        = "NET_60";
        public const string Net90        = "NET_90";
        public const string TwoPercent10Net30 = "2PCT_10_NET_30";

        public static readonly string[] All = { DueOnReceipt, Net10, Net15, Net30, Net45, Net60, Net90, TwoPercent10Net30 };
    }

    /// <summary>Invoice type codes.</summary>
    public static class InvoiceTypeCodes
    {
        public const string Standard      = "STANDARD";
        public const string CreditMemo    = "CREDIT_MEMO";
        public const string DebitMemo     = "DEBIT_MEMO";
        public const string Prepayment    = "PREPAYMENT";
        public const string ProgressBilling = "PROGRESS_BILLING";
        public const string JIB_Statement = "JIB_STATEMENT";
        public const string RoyaltyStatement = "ROYALTY_STATEMENT";
        public const string RevenueInvoice = "REVENUE_INVOICE";

        public static readonly string[] All = { Standard, CreditMemo, DebitMemo, Prepayment, ProgressBilling, JIB_Statement, RoyaltyStatement, RevenueInvoice };
    }

    /// <summary>Purchase order status codes.</summary>
    public static class POStatusCodes
    {
        public const string Draft      = "DRAFT";
        public const string Pending    = "PENDING";
        public const string Approved   = "APPROVED";
        public const string Sent       = "SENT";
        public const string Partial    = "PARTIALLY_RECEIVED";
        public const string Received   = "RECEIVED";
        public const string Invoiced   = "INVOICED";
        public const string Closed     = "CLOSED";
        public const string Cancelled  = "CANCELLED";

        public static readonly string[] All = { Draft, Pending, Approved, Sent, Partial, Received, Invoiced, Closed, Cancelled };
    }

    /// <summary>Three-way match result codes.</summary>
    public static class MatchResultCodes
    {
        public const string Matched    = "MATCHED";
        public const string QtyMismatch = "QTY_MISMATCH";
        public const string PriceMismatch = "PRICE_MISMATCH";
        public const string NoReceipt  = "NO_RECEIPT";
        public const string NoPO       = "NO_PO";
        public const string Duplicate  = "DUPLICATE";

        public static readonly string[] All = { Matched, QtyMismatch, PriceMismatch, NoReceipt, NoPO, Duplicate };
    }

    /// <summary>AR aging bucket codes.</summary>
    public static class AgingBucketCodes
    {
        public const string Current    = "CURRENT";       // 0-30 days
        public const string PastDue30  = "PAST_DUE_30";   // 31-60
        public const string PastDue60  = "PAST_DUE_60";   // 61-90
        public const string PastDue90  = "PAST_DUE_90";   // 91-120
        public const string Doubtful   = "DOUBTFUL";      // >120 days
        public const string WriteOff   = "WRITE_OFF";

        public static readonly string[] All = { Current, PastDue30, PastDue60, PastDue90, Doubtful, WriteOff };
    }

    /// <summary>Payment method codes.</summary>
    public static class PaymentMethodCodes
    {
        public const string ACH        = "ACH";
        public const string Wire       = "WIRE";
        public const string Check      = "CHECK";
        public const string CreditCard  = "CREDIT_CARD";
        public const string DirectDebit = "DIRECT_DEBIT";

        public static readonly string[] All = { ACH, Wire, Check, CreditCard, DirectDebit };
    }

    /// <summary>Expense category codes.</summary>
    public static class ExpenseCategoryCodes
    {
        public const string Travel      = "TRAVEL";
        public const string Meals       = "MEALS";
        public const string Lodging     = "LODGING";
        public const string Supplies    = "SUPPLIES";
        public const string Mileage     = "MILEAGE";
        public const string Entertainment = "ENTERTAINMENT";
        public const string Training    = "TRAINING";
        public const string Office      = "OFFICE";
        public const string Software    = "SOFTWARE";
        public const string ProfessionalServices = "PROFESSIONAL_SERVICES";

        public static readonly string[] All = { Travel, Meals, Lodging, Supplies, Mileage, Entertainment, Training, Office, Software, ProfessionalServices };
    }

    /// <summary>Journal Entry status codes.</summary>
    public static class JournalEntryStatusCodes
    {
        public const string Draft    = "DRAFT";
        public const string Posted   = "POSTED";
        public const string Reversed = "REVERSED";
        public const string Voided   = "VOIDED";

        public static readonly string[] All = { Draft, Posted, Reversed, Voided };
    }

    /// <summary>Invoice/AP status codes.</summary>
    public static class InvoiceStatusCodes
    {
        public const string Draft    = "DRAFT";
        public const string Issued   = "ISSUED";
        public const string Received = "RECEIVED";
        public const string Approved = "APPROVED";
        public const string Paid     = "PAID";
        public const string Voided   = "VOIDED";

        public static readonly string[] All = { Draft, Issued, Received, Approved, Paid, Voided };
    }

    /// <summary>Revenue recognition status codes.</summary>
    public static class RevenueStatusCodes
    {
        public const string Pending    = "PENDING";
        public const string Recognized = "RECOGNIZED";
        public const string Deferred   = "DEFERRED";
        public const string Satisfied  = "SATISFIED";

        public static readonly string[] All = { Pending, Recognized, Deferred, Satisfied };
    }

    /// <summary>All reference code sets that need seeding.</summary>
    public static class SeedSets
    {
        public static readonly Dictionary<string, string[]> All = new()
        {
            ["R_BA_CATEGORY"] = VendorLOVCodes.All,
            ["R_BA_TYPE"]     = VendorTypeCodes.All,
            ["R_BA_PREF_TYPE"] = PreferenceTypeCodes.All,
            ["R_BA_STATUS"]   = BAStatusCodes.All,
        };
    }
}
