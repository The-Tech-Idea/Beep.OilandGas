using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CommercialIntentMigrationTests
{
    [Fact]public void InvoiceReviewerJournalAndNativeDispositionUseCanonicalBoundedBeepMetadata()
    {
        ModuleSchemaBoundary.Validate([typeof(SALES_INVOICE_REVIEW_INTENT),typeof(INVOICE)]);
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(SALES_INVOICE_REVIEW_INTENT));var key=Assert.Single(schema.Fields,field=>field.IsKey);
        Assert.Equal("SALES_INVOICE_REVIEW_INTENT_ID",key.FieldName);Assert.False(key.AllowDBNull);Assert.Equal(32,key.Size1);
        foreach(var (name,size) in new[]{("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),("INVOICE_ID",32),("SOURCE_REVISION",64),("COMMAND_JSON",SALES_INVOICE_REVIEW_INTENT.MaximumCommandLength),("RECORDED_REVISION",64)})
        {var field=Assert.Single(schema.Fields,value=>value.FieldName==name);Assert.False(field.AllowDBNull);Assert.Equal(typeof(string).FullName,field.Fieldtype);Assert.Equal(size,field.Size1);}
        schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(INVOICE));var proof=Assert.Single(schema.Fields,field=>field.FieldName=="REVIEW_WORKFLOW_JSON");Assert.True(proof.AllowDBNull);Assert.Equal(CompanySalesInvoiceDecisionProof.MaximumLength,proof.Size1);
    }
    [Theory][InlineData(typeof(Beep.OilandGas.Models.Data.ProductionAccounting.RUN_TICKET),"RUN_TICKET_ID")]
    [InlineData(typeof(Beep.OilandGas.Models.Data.ProductionAccounting.ALLOCATION_RESULT),"ALLOCATION_RESULT_ID")]
    [InlineData(typeof(Beep.OilandGas.Models.Data.ProductionAccounting.ALLOCATION_DETAIL),"ALLOCATION_DETAIL_ID")]
    public void AllocationAndTicketModelsHaveOneCanonicalOwnerAndBeepKeyMetadata(Type type,string identity)
    {
        Assert.Equal(typeof(RevenueDeliverySourceReview).Assembly,type.Assembly);ModuleSchemaBoundary.Validate([type]);
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(type);var key=Assert.Single(schema.Fields,field=>field.IsKey);
        Assert.Equal(identity,key.FieldName);Assert.Equal(128,key.Size1);
        if(type==typeof(Beep.OilandGas.Models.Data.ProductionAccounting.RUN_TICKET))
        {var acceptance=Assert.Single(schema.Fields,field=>field.FieldName=="DELIVERY_WORKFLOW_JSON");Assert.True(acceptance.AllowDBNull);Assert.Equal(RevenueDeliveryAcceptanceProof.MaximumLength,acceptance.Size1);}
        var ticket=new Beep.OilandGas.Models.Data.ProductionAccounting.RUN_TICKET{NetVolume=7,RunTicketNumber="original"};Assert.Equal(7,ticket.NET_VOLUME);Assert.Equal("original",ticket.RUN_TICKET_NUMBER);
        var allocation=new Beep.OilandGas.Models.Data.ProductionAccounting.ALLOCATION_RESULT{AllocationId="original",Details=[new(){AllocatedVolume=7}]};Assert.Equal("original",allocation.ALLOCATION_RESULT_ID);Assert.Equal(7,Assert.Single(allocation.Details).ALLOCATED_VOLUME);
        var options=new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
        var restored=System.Text.Json.JsonSerializer.Deserialize(System.Text.Json.JsonSerializer.Serialize(allocation,options),typeof(Beep.OilandGas.Models.Data.ProductionAccounting.ALLOCATION_RESULT),options);
        Assert.Equal("original",Assert.IsType<Beep.OilandGas.Models.Data.ProductionAccounting.ALLOCATION_RESULT>(restored).ALLOCATION_RESULT_ID);
    }
    [Fact]
    public void DeliveryJournalHasCanonicalBoundedRequiredBeepMetadata()
    {
        ModuleSchemaBoundary.Validate([typeof(DELIVERY_ACCEPTANCE_INTENT)]);
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(DELIVERY_ACCEPTANCE_INTENT));
        var key=Assert.Single(schema.Fields,field=>field.IsKey);Assert.Equal("DELIVERY_ACCEPTANCE_INTENT_ID",key.FieldName);Assert.False(key.AllowDBNull);Assert.Equal(32,key.Size1);
        foreach(var (name,size) in new[]{("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),("RUN_TICKET_ID",128),("INTENT_KIND",11),("SCOPE_KEY",64),("SOURCE_REVISION",64),("RECORDED_REVISION",64),("COMMAND_JSON",DELIVERY_ACCEPTANCE_INTENT.MaximumCommandLength)})
        {var field=Assert.Single(schema.Fields,value=>value.FieldName==name);Assert.False(field.AllowDBNull);Assert.Equal(typeof(string).FullName,field.Fieldtype);Assert.Equal(size,field.Size1);}
        Assert.Throws<ArgumentException>(()=>ModuleSchemaBoundary.Validate([typeof(RevenueDeliveryAcceptanceIntent)]));
    }
    [Fact]
    public void CanonicalBusinessIntentUsesBoundedNonnullableBeepMetadata()
    {
        ModuleSchemaBoundary.Validate([typeof(COMMERCIAL_TERMS_INTENT),typeof(INVENTORY_STOCK_INTENT),typeof(PO_INVENTORY_LINK_HISTORY)]);
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(COMMERCIAL_TERMS_INTENT));
        var key=Assert.Single(schema.Fields,field=>field.IsKey);Assert.Equal("COMMERCIAL_TERMS_INTENT_ID",key.FieldName);Assert.False(key.AllowDBNull);Assert.Equal(32,key.Size1);
        var command=Assert.Single(schema.Fields,field=>field.FieldName=="COMMAND_JSON");Assert.False(command.AllowDBNull);Assert.Equal(COMMERCIAL_TERMS_INTENT.MaximumCommandLength,command.Size1);
    }
    [Theory][InlineData(typeof(AppRoleExtension))][InlineData(typeof(AppUserRoleExtension))][InlineData(typeof(AppTenant))][InlineData(typeof(AppCompany))]
    [InlineData(typeof(AppCompanyMembership))][InlineData(typeof(AppPersona))][InlineData(typeof(Microsoft.AspNetCore.Identity.IdentityRole))]
    [InlineData(typeof(CompanyCommercialAmendmentIntentReview))]
    [InlineData(typeof(RevenueDeliveryAcceptanceSavedReview))]
    [InlineData(typeof(CompanyDeliverySavedWorkReview))]
    [InlineData(typeof(CompanyDeliverySavedWorkPage))]
    [InlineData(typeof(RevenueDeliveryAcceptanceCorrection))]
    [InlineData(typeof(RevenueDeliveryAcceptanceCorrectionOrigin))]
    [InlineData(typeof(RevenueDeliveryAcceptancePriorVersion))]
    [InlineData(typeof(CompanyDeliveryCorrectionReview))]
    [InlineData(typeof(RevenueDeliveryBillingBasisReview))]
    [InlineData(typeof(CompanyBillingPolicyRules))]
    [InlineData(typeof(CompanyBillingPolicyRecord))]
    [InlineData(typeof(CompanyBillingPolicyProof))]
    [InlineData(typeof(CompanyBillingPolicyReview))]
    [InlineData(typeof(CompanyBillingPolicyDraft))]
    [InlineData(typeof(CompanyBillingPolicyApproval))]
    [InlineData(typeof(CompanyBillingPolicyCommandReview))]
    [InlineData(typeof(CompanyBillingPolicyDraftSubmission))]
    [InlineData(typeof(CompanyBillingPolicyApprovalSubmission))]
    [InlineData(typeof(CompanyBillingPolicyResult))]
    [InlineData(typeof(CompanySalesInvoiceFacts))]
    [InlineData(typeof(CompanySalesInvoiceContributor))]
    [InlineData(typeof(CompanySalesInvoiceDraftReview))]
    [InlineData(typeof(CompanySalesInvoiceReviewSource))]
    [InlineData(typeof(CompanySalesInvoiceDecisionCommand))]
    [InlineData(typeof(CompanySalesInvoiceDecisionProof))]
    [InlineData(typeof(CompanySalesInvoiceProcessSourceReview))]
    [InlineData(typeof(CompanySalesInvoiceQueueRow))]
    [InlineData(typeof(CompanySalesInvoiceQueueTotal))]
    [InlineData(typeof(CompanySalesInvoiceQueueReview))]
    [InlineData(typeof(CompanySalesReceivableContributor))]
    [InlineData(typeof(CompanySalesReceivableSource))]
    [InlineData(typeof(CompanySalesReceivableCommand))]
    [InlineData(typeof(CompanySalesReceivableRecord))]
    [InlineData(typeof(CompanySalesReceivableProof))]
    [InlineData(typeof(CompanySalesReceivableOutcome))]
    [InlineData(typeof(CompanySalesReceivableStatusRow))]
    [InlineData(typeof(CompanySalesReceivableStatusReview))]
    [InlineData(typeof(CompanySalesReceivableRecovery))]
    [InlineData(typeof(CompanySalesReceivableWorkspaceReview))]
    [InlineData(typeof(CompanySalesReceivableReviewSource))]
    [InlineData(typeof(CompanySalesReceivableDecisionCommand))]
    [InlineData(typeof(CompanySalesReceivableDecisionProof))]
    [InlineData(typeof(CompanySalesReceivableReviewRecovery))]
    [InlineData(typeof(CompanySalesReceivableProcessSourceReview))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.CreateARInvoiceRequest))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.UpdateARInvoiceRequest))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.CreateARPaymentRequest))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.CreateARCreditMemoRequest))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.ARApprovalResult))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.ARAgingSummary))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.CustomerSummary))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.PaymentApplicationItem))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.PaymentApplicationRequest))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.PaymentApplicationResult))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.ARInvoice))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.ARInvoiceResponse))]
    [InlineData(typeof(CompanySalesInvoiceDraftCommand))]
    [InlineData(typeof(CompanySalesInvoiceDraftRecord))]
    [InlineData(typeof(CompanySalesInvoiceDraftProof))]
    [InlineData(typeof(CompanySalesInvoiceDraftOutcome))]
    [InlineData(typeof(CompanySalesInvoiceWorkspaceReview))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.CreateInvoiceRequest))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.UpdateInvoiceRequest))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.CreateInvoicePaymentRequest))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.InvoiceApprovalResult))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.InvoiceAgingSummary))]
    [InlineData(typeof(Beep.OilandGas.Accounting.Models.InvoicePaymentStatus))]
    [InlineData(typeof(CompanyDeliveryCorrectionSubmission))]
    public void RepositoryIdentityAndReadContractsCannotEnterBusinessMigrations(Type type)=>Assert.Throws<ArgumentException>(()=>ModuleSchemaBoundary.Validate([type]));
}
