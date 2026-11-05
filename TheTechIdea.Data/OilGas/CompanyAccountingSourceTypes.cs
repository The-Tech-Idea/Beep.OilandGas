namespace TheTechIdea.Data.OilGas;
public static class CompanyAccountingSourceTypes
{
    public const string JournalPosting = "SALES_JOURNAL_POSTING";
    public const string CommercialApproval="SALES_CONTRACT_TERMS_APPROVAL";
    public const string CommercialRejectionReview="SALES_CONTRACT_REJECTION_REVIEW";
    public const string RouteCommercialRejection="ROUTE_COMMERCIAL_REJECTION";
    public const string DeliveryApproval="DELIVERY_ACCEPTANCE_APPROVAL";
    public const string DeliveryRejectionReview="DELIVERY_ACCEPTANCE_REJECTION_REVIEW";
    public const string RouteDeliveryRejection="ROUTE_DELIVERY_REJECTION";
    public const string InvoiceApproval="SALES_INVOICE_APPROVAL";
    public const string InvoiceRejectionReview="SALES_INVOICE_REJECTION_REVIEW";
    public const string RouteInvoiceRejection="ROUTE_SALES_INVOICE_REJECTION";
    public const string ReceivableApproval="SALES_RECEIVABLE_APPROVAL";
    public const string ReceivableRejectionReview="SALES_RECEIVABLE_REJECTION_REVIEW";
    public const string RouteReceivableRejection="ROUTE_SALES_RECEIVABLE_REJECTION";
    public static bool IsNativeDecision(string type)=>type is "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT";
    public static string Approval(string type)=>type switch{"SALES_CONTRACT"=>CommercialApproval,"RUN_TICKET"=>DeliveryApproval,"INVOICE"=>InvoiceApproval,"AR_INVOICE"=>ReceivableApproval,"AR_PAYMENT"=>"SALES_PAYMENT_APPROVAL","JOURNAL_ENTRY"=>"SALES_JOURNAL_APPROVAL",_=>throw new InvalidOperationException("Unsupported native decision source.")};
    public static string RejectionReview(string type)=>type switch{"SALES_CONTRACT"=>CommercialRejectionReview,"RUN_TICKET"=>DeliveryRejectionReview,"INVOICE"=>InvoiceRejectionReview,"AR_INVOICE"=>ReceivableRejectionReview,"AR_PAYMENT"=>"SALES_PAYMENT_REJECTION_REVIEW","JOURNAL_ENTRY"=>"SALES_JOURNAL_REJECTION_REVIEW",_=>throw new InvalidOperationException("Unsupported native decision source.")};
    public static string RejectionRoute(string type)=>type switch{"SALES_CONTRACT"=>RouteCommercialRejection,"RUN_TICKET"=>RouteDeliveryRejection,"INVOICE"=>RouteInvoiceRejection,"AR_INVOICE"=>RouteReceivableRejection,"AR_PAYMENT"=>"ROUTE_SALES_PAYMENT_REJECTION","JOURNAL_ENTRY"=>"ROUTE_SALES_JOURNAL_REJECTION",_=>throw new InvalidOperationException("Unsupported native decision source.")};
    public static string Prefix(string type)=>type switch{"SALES_CONTRACT"=>"COMMERCIAL","RUN_TICKET"=>"DELIVERY","INVOICE"=>"SALES_INVOICE","AR_INVOICE"=>"SALES_RECEIVABLE","AR_PAYMENT"=>"SALES_PAYMENT","JOURNAL_ENTRY"=>"SALES_JOURNAL",_=>throw new InvalidOperationException("Unsupported native decision source.")};
    public static string RejectionEligibility(string type)=>Prefix(type)+"_REJECTION_RECORDED";
    public static string RejectionHistory(string type)=>Prefix(type)+"_REJECTION_ROUTED";
    public static string RejectionReviewHistory(string type)=>Prefix(type)+"_REJECTION_REVIEWED";
    public static string DecisionApprovalType(string type)=>Prefix(type)+"_DECISION";
    public static string ReadPermission(string type)=>type switch{"AP_INVOICE" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT"=>"Accounting.View","PURCHASE_ORDER"=>"Procurement.View",_=>throw new InvalidOperationException("Unsupported company accounting source.")};
    public static string ReceivablePreparationRevision(CompanySalesReceivableOutcome outcome)
    {outcome.Require(outcome.Intent.Restore(new(outcome.Intent.OWNER_TENANT_ID,outcome.Intent.OWNER_COMPANY_BA_ID),outcome.Intent.ROW_CREATED_BY));return outcome.Receipt.RECORDED_REVISION;}
    public static string InvoicePreparationRevision(CompanySalesInvoiceDraftOutcome outcome)
    {outcome.Require(outcome.Intent.Restore(new(outcome.Intent.OWNER_TENANT_ID,outcome.Intent.OWNER_COMPANY_BA_ID),outcome.Intent.ROW_CREATED_BY));return outcome.Receipt.RECORDED_REVISION;}
    public static string JournalPreparationRevision(CompanySalesJournalPreparationOutcome outcome)
    {var proof=CompanySalesJournalPreparationProof.Require(outcome.Header);outcome.Require(proof.Command);if(proof.DraftedAtUtc is null)throw new InvalidOperationException("A completed original native journal draft is required before workflow correlation.");return ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(proof));}
    public static string CommercialPreparationRevision(SalesContractTermsRecord record)
    {record.Require();return SalesContractTermsRecord.Create(record.Preparation,record.PreparedAtUtc).Revision;}
    public static string DeliveryPreparationRevision(RevenueDeliveryAcceptanceRecord record)
    {record.Require();return RevenueDeliveryAcceptanceRecord.Create(record.Preparation,record.PreparedAtUtc).Revision;}
}
