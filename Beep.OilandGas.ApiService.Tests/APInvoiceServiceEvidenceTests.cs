using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class APInvoiceServiceEvidenceTests
{
    private static readonly CompanyScope Scope=new("tenant","company");
    private static AP_INVOICE Invoice()=>new(){AP_INVOICE_ID="invoice",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier",CURRENCY_CODE="KWD",TOTAL_AMOUNT=12m,ACTIVE_IND="Y"};
    private static APInvoiceService Service(Mock<IDMEEditor> editor,Func<Task<string>>? resolve)
    {
        var columns=Mock.Of<ICommonColumnHandler>();var defaults=Mock.Of<IPPDM39DefaultsRepository>();var metadata=Mock.Of<IPPDMMetadataRepository>();
        var accounts=new GLAccountService(editor.Object,columns,defaults,metadata,NullLogger<GLAccountService>.Instance,resolve);
        var journal=new JournalEntryService(editor.Object,columns,defaults,metadata,accounts,NullLogger<JournalEntryService>.Instance,resolve);
        return new(editor.Object,columns,defaults,metadata,new AccountingBasisPostingService(journal),NullLogger<APInvoiceService>.Instance,resolveConnection:resolve);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task MissingOrEmptyBindingCannotFallBack(bool empty)
    {
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);var service=Service(editor,empty?()=>Task.FromResult(""):null);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.GetBillByIdAsync(Scope,"invoice"));editor.VerifyNoOtherCalls();
    }
    [Theory][InlineData("OWNER_TENANT_ID")][InlineData("OWNER_COMPANY_BA_ID")][InlineData("CURRENCY_CODE")][InlineData("duplicate")]
    public async Task FreshMissingOrAmbiguousOwnershipSchemaRefusesBeforeQueries(string fault)
    {
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(AP_INVOICE));
        if(fault=="duplicate")schema.Fields.Add(schema.Fields.Single(field=>field.FieldName=="CURRENCY_CODE"));else schema.Fields.RemoveAll(field=>field.FieldName==fault);
        var source=new Mock<IDataSource>(MockBehavior.Strict);source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(schema);
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);editor.Setup(value=>value.GetDataSource("bound-production")).Returns(source.Object);
        var service=Service(editor,()=>Task.FromResult("bound-production"));await Assert.ThrowsAsync<InvalidOperationException>(()=>service.GetBillByIdAsync(Scope,"invoice"));
        source.Verify(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="AP_INVOICE" && request.Fields.Count==0),true),Times.Once);source.VerifyNoOtherCalls();
    }
    [Theory][InlineData("missing")][InlineData("nullable")][InlineData("compound")]
    public async Task UnconfirmedNativeIdentityCannotPublishSource(string fault)
    {
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(AP_INVOICE));var key=schema.Fields.Single(field=>field.IsKey);
        if(fault=="missing")key.IsKey=false;if(fault=="nullable")key.AllowDBNull=true;if(fault=="compound")schema.Fields.Single(field=>field.FieldName=="OWNER_COMPANY_BA_ID").IsKey=true;
        var source=new Mock<IDataSource>(MockBehavior.Strict);source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(schema);
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);editor.Setup(value=>value.GetDataSource("bound-production")).Returns(source.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Service(editor,()=>Task.FromResult("bound-production")).GetSourceReviewAsync(Scope,"invoice","accountant"));source.Verify(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true),Times.Once);source.VerifyNoOtherCalls();
    }
    private static (APInvoiceService Service,Mock<IDataSource> Source) Reads(Func<List<AppFilter>,IEnumerable<object>> read)
    {
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(AP_INVOICE));
        var source=new Mock<IDataSource>();source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(request.EntityName=="AP_INVOICE"?typeof(AP_INVOICE):request.EntityName=="AP_RECEIPT_ALLOCATION"?typeof(AP_RECEIPT_ALLOCATION):typeof(AP_INVOICE_LINE)));
        source.Setup(value=>value.GetEntityAsync("AP_INVOICE",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>read(filters));
        source.Setup(value=>value.GetEntityAsync("AP_INVOICE_LINE",It.IsAny<List<AppFilter>>())).ReturnsAsync(Array.Empty<object>());
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);editor.Setup(value=>value.GetDataSource("bound-production")).Returns(source.Object);
        return(Service(editor,()=>Task.FromResult("bound-production")),source);
    }
    [Fact] public async Task NativeRepositoryReadCarriesExactOriginalOwnerAndIdentity()
    {
        var fixture=Reads(filters=>{
            Assert.Equal(3,filters.Count);Assert.Contains(filters,f=>f.FieldName=="AP_INVOICE_ID" && f.FilterValue=="invoice" && f.Operator=="=");
            Assert.Contains(filters,f=>f.FieldName=="OWNER_TENANT_ID" && f.FilterValue=="tenant");Assert.Contains(filters,f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue=="company");return new[]{Invoice()};});
        var review=await fixture.Service.GetSourceReviewAsync(Scope,"invoice","accountant");review!.Require(Scope,"accountant","invoice");Assert.Equal(12m,review.Invoice.TOTAL_AMOUNT);
        fixture.Source.Verify(value=>value.GetEntityAsync("AP_INVOICE",It.IsAny<List<AppFilter>>()),Times.Exactly(2));
    }
    [Theory][InlineData("duplicate")][InlineData("foreign")][InlineData("wrong-id")]
    public async Task ProviderRowsCannotOverrideOriginalScope(string fault)
    {
        var invoice=Invoice();if(fault=="foreign")invoice.OWNER_COMPANY_BA_ID="other";if(fault=="wrong-id")invoice.AP_INVOICE_ID="other";
        var fixture=Reads(_=>fault=="duplicate"?new[]{invoice,invoice}:new[]{invoice});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.GetBillByIdAsync(Scope,"invoice"));
    }
    [Fact] public async Task SupplierReadCarriesOwnerActivityAndSupplierPredicates()
    {
        var fixture=Reads(filters=>{
            Assert.Equal(4,filters.Count);Assert.Contains(filters,f=>f.FieldName=="OWNER_TENANT_ID" && f.FilterValue=="tenant");
            Assert.Contains(filters,f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue=="company");Assert.Contains(filters,f=>f.FieldName=="VENDOR_BA_ID" && f.FilterValue=="supplier");Assert.Contains(filters,f=>f.FieldName=="ACTIVE_IND" && f.FilterValue=="Y");return new[]{Invoice()};});
        Assert.Equal("invoice",Assert.Single(await fixture.Service.GetVendorBillsAsync(Scope,"supplier")).AP_INVOICE_ID);
    }
    [Fact] public async Task ActualRepositoryFailurePropagatesInsteadOfNoLiability()
    {
        var fixture=Reads(_=>throw new InvalidOperationException("Recorded provider read failed."));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.GetVendorBillsAsync(Scope,"supplier"));
    }
    [Fact] public async Task TwoCompaniesWithTheSameLocalInvoiceIdentityRetainTheirOriginalOwner()
    {
        var fixture=Reads(filters=>{var invoice=Invoice();invoice.OWNER_COMPANY_BA_ID=filters.Single(f=>f.FieldName=="OWNER_COMPANY_BA_ID").FilterValue;return new[]{invoice};});
        var first=await fixture.Service.GetSourceReviewAsync(Scope,"invoice","accountant");
        var other=new CompanyScope("tenant","other");var second=await fixture.Service.GetSourceReviewAsync(other,"invoice","accountant");
        first!.Require(Scope,"accountant","invoice");second!.Require(other,"accountant","invoice");
        Assert.NotEqual(first.Revision,second.Revision);Assert.Equal("company",first.Invoice.OWNER_COMPANY_BA_ID);Assert.Equal("other",second.Invoice.OWNER_COMPANY_BA_ID);
    }
    [Fact] public async Task ConfirmedEmptyRowsRemainDistinctFromFailedReads()
    {
        var fixture=Reads(_=>Array.Empty<object>());Assert.Null(await fixture.Service.GetBillByIdAsync(Scope,"invoice"));
        Assert.Empty(await fixture.Service.GetVendorBillsAsync(Scope,"supplier"));
    }
    [Fact] public async Task UnreviewedAgingCannotAggregateCurrencyOrOpenAProvider()
    {
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Service(editor,null).GetAPAgingAsync(Scope));editor.VerifyNoOtherCalls();
    }
    [Theory][InlineData("null-payload")][InlineData("null-row")][InlineData("partial")]
    public async Task UnavailableOrPartialProviderEvidenceCannotBecomeAnEmptySupplierSuccess(string fault)
    {
        var fixture=Reads(_=>fault=="null-payload"?null!:fault=="null-row"?new object[]{null!}:new object[]{Invoice(),null!});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.GetVendorBillsAsync(Scope,"supplier"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.GetSourceReviewAsync(Scope,"invoice","accountant"));
    }
    [Theory][InlineData("null-payload")][InlineData("partial")]
    public async Task FailedNativeReloadClearsVisibleCompanyInvoiceRows(string fault)
    {
        IEnumerable<object>? raw=new[]{Invoice()};var fixture=Reads(_=>raw!);
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"accountant",new(Scope,["ConfiguredAccountant"],["Accounting.View"])));
        using var state=new Beep.OilandGas.Web.Services.APInvoiceWorkspaceState(company);
        await state.LoadVendorAsync("supplier",vendor=>fixture.Service.GetVendorBillsAsync(Scope,vendor));Assert.Single(state.Rows!);
        raw=fault=="null-payload"?null:new object[]{Invoice(),null!};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadVendorAsync("supplier",vendor=>fixture.Service.GetVendorBillsAsync(Scope,vendor)));
        Assert.Null(state.Rows);Assert.Null(state.Detail);Assert.Null(state.RetrievedAtUtc);
    }
    [Fact] public async Task NativeLineReadIsBoundToAnOwnedHeaderAndExactParentFilter()
    {
        var fixture=Reads(_=>new[]{Invoice()});
        fixture.Source.Setup(source=>source.GetEntityAsync("AP_INVOICE_LINE",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
            var filter=Assert.Single(filters);Assert.Equal("AP_INVOICE_ID",filter.FieldName);Assert.Equal("invoice",filter.FilterValue);Assert.Equal("=",filter.Operator);
            return new object[]{new AP_INVOICE_LINE{AP_INVOICE_LINE_ID="line",AP_INVOICE_ID="invoice",QUANTITY=2m,QUANTITY_UOM="m³",ACTIVE_IND="N"}};});
        var review=await fixture.Service.GetSourceReviewAsync(Scope,"invoice","accountant");review!.Require(Scope,"accountant","invoice");
        Assert.Equal("line",Assert.Single(review.Lines).AP_INVOICE_LINE_ID);Assert.Equal("N",review.Lines[0].ACTIVE_IND);
    }
    [Theory][InlineData("foreign")][InlineData("duplicate")][InlineData("null")]
    public async Task UnconfirmedNativeLineSetCannotPublishTheHeaderOnly(string fault)
    {
        var fixture=Reads(_=>new[]{Invoice()});var line=new AP_INVOICE_LINE{AP_INVOICE_LINE_ID="line",AP_INVOICE_ID=fault=="foreign"?"other":"invoice"};
        fixture.Source.Setup(source=>source.GetEntityAsync("AP_INVOICE_LINE",It.IsAny<List<AppFilter>>())).ReturnsAsync(fault=="null"?null!:fault=="duplicate"?new object[]{line,line}:new object[]{line});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.GetSourceReviewAsync(Scope,"invoice","accountant"));
    }
    [Fact] public async Task MissingOwnedHeaderCannotReadChildInvoiceData()
    {
        var fixture=Reads(_=>Array.Empty<object>());
        await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>fixture.Service.GetLinesForInvoiceAsync(Scope,"invoice"));
        fixture.Source.Verify(source=>source.GetEntityAsync("AP_INVOICE_LINE",It.IsAny<List<AppFilter>>()),Times.Never);
    }
    [Theory][InlineData("unit")][InlineData("key")][InlineData("duplicate")]
    public async Task UnconfirmedLineSchemaRefusesBeforeChildQuery(string fault)
    {
        var fixture=Reads(_=>new[]{Invoice()});var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(AP_INVOICE_LINE));
        if(fault=="unit")schema.Fields.RemoveAll(field=>field.FieldName=="QUANTITY_UOM");if(fault=="key")schema.Fields.Single(field=>field.IsKey).IsKey=false;
        if(fault=="duplicate")schema.Fields.Add(schema.Fields.Single(field=>field.FieldName=="AP_INVOICE_ID"));
        fixture.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="AP_INVOICE_LINE"),true)).Returns(schema);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.GetSourceReviewAsync(Scope,"invoice","accountant"));
        fixture.Source.Verify(source=>source.GetEntityAsync("AP_INVOICE_LINE",It.IsAny<List<AppFilter>>()),Times.Never);
    }
    [Fact] public async Task CompanyQueueUsesOnlyOriginalOwnerAndActivePredicates()
    {
        var fixture=Reads(filters=>{
            Assert.Equal(3,filters.Count);
            Assert.Contains(filters,f=>f.FieldName=="OWNER_TENANT_ID" && f.FilterValue=="tenant" && f.Operator=="=");
            Assert.Contains(filters,f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue=="company" && f.Operator=="=");
            Assert.Contains(filters,f=>f.FieldName=="ACTIVE_IND" && f.FilterValue=="Y" && f.Operator=="=");
            var second=Invoice();second.AP_INVOICE_ID="second";second.VENDOR_BA_ID="other-supplier";return new[]{Invoice(),second};});
        Assert.Equal(2,(await fixture.Service.GetCompanyBillsAsync(Scope)).Count);
    }
    [Theory][InlineData("foreign")][InlineData("inactive")][InlineData("duplicate")][InlineData("null")]
    public async Task CompanyQueueRefusesUnconfirmedReturnedRows(string fault)
    {
        var invoice=Invoice();if(fault=="foreign")invoice.OWNER_TENANT_ID="other";if(fault=="inactive")invoice.ACTIVE_IND="N";
        var fixture=Reads(_=>fault=="null"?null!:fault=="duplicate"?new[]{invoice,invoice}:new[]{invoice});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.GetCompanyBillsAsync(Scope));
    }
    [Fact] public async Task EmptySupplierAssertionRefusesBeforeOpeningProvider()
    {
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Service(editor,null).GetCompanyBillsAsync(Scope," "));editor.VerifyNoOtherCalls();
    }
    [Theory][InlineData("valid")][InlineData("missing-line")][InlineData("foreign")][InlineData("duplicate")][InlineData("null")][InlineData("schema")]
    [InlineData("owner-tenant")][InlineData("owner-company")][InlineData("owner-missing")][InlineData("schema-tenant")][InlineData("schema-company")]
    public async Task NativeReceiptAllocationReadRequiresOwnedParentExactLineAndConfirmedSchema(string fault)
    {
        var fixture=Reads(_=>new[]{Invoice()});
        fixture.Source.Setup(source=>source.GetEntityAsync("AP_INVOICE_LINE",It.IsAny<List<AppFilter>>())).ReturnsAsync(
            fault=="missing-line"?Array.Empty<object>():new object[]{new AP_INVOICE_LINE{AP_INVOICE_ID="invoice",AP_INVOICE_LINE_ID="line"}});
        var row=new AP_RECEIPT_ALLOCATION{OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",AP_RECEIPT_ALLOCATION_ID="allocation",AP_INVOICE_ID=fault=="foreign"?"other":"invoice",AP_INVOICE_LINE_ID="line",
            PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",PO_RECEIPT_ID="receipt",ALLOCATED_QUANTITY=2m,QUANTITY_UOM="bbl",ACTIVE_IND="N"};
        if(fault=="owner-tenant")row.OWNER_TENANT_ID="foreign";if(fault=="owner-company")row.OWNER_COMPANY_BA_ID="foreign";if(fault=="owner-missing")row.OWNER_COMPANY_BA_ID=null;
        fixture.Source.Setup(source=>source.GetEntityAsync("AP_RECEIPT_ALLOCATION",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
            Assert.Equal(4,filters.Count);Assert.Contains(filters,f=>f.FieldName=="OWNER_TENANT_ID" && f.FilterValue=="tenant" && f.Operator=="=");
            Assert.Contains(filters,f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue=="company" && f.Operator=="=");
            Assert.Contains(filters,f=>f.FieldName=="AP_INVOICE_ID" && f.FilterValue=="invoice" && f.Operator=="=");
            Assert.Contains(filters,f=>f.FieldName=="AP_INVOICE_LINE_ID" && f.FilterValue=="line" && f.Operator=="=");
            return fault=="null"?null!:fault=="duplicate"?new object[]{row,row}:new object[]{row};});
        if(fault is "schema" or "schema-tenant" or "schema-company"){
            var missing=fault=="schema-tenant"?"OWNER_TENANT_ID":fault=="schema-company"?"OWNER_COMPANY_BA_ID":"PO_RECEIPT_ID";
            var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(AP_RECEIPT_ALLOCATION));schema.Fields.RemoveAll(field=>field.FieldName==missing);
            fixture.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(table=>table.EntityName=="AP_RECEIPT_ALLOCATION"),true)).Returns(schema);
        }
        if(fault=="valid")Assert.Equal("N",Assert.Single(await fixture.Service.GetReceiptAllocationsForLineAsync(Scope,"invoice","line")).ACTIVE_IND);
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.GetReceiptAllocationsForLineAsync(Scope,"invoice","line"));
        if(fault is "missing-line" or "schema" or "schema-tenant" or "schema-company")fixture.Source.Verify(source=>source.GetEntityAsync("AP_RECEIPT_ALLOCATION",It.IsAny<List<AppFilter>>()),Times.Never);
    }
    [Theory][InlineData("valid")][InlineData("foreign")][InlineData("missing")][InlineData("line")][InlineData("receipt")][InlineData("duplicate")][InlineData("null")][InlineData("revoked")]
    [InlineData("owner-tenant")][InlineData("owner-company")][InlineData("owner-missing")]
    public async Task ReceiptConsumptionReadsOtherInvoicesAndRefusesUnconfirmedContributors(string fault)
    {
        AP_INVOICE_LINE Line(string invoice)=>new(){AP_INVOICE_ID=invoice,AP_INVOICE_LINE_ID="line-"+invoice,PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line"};
        var selected=Invoice();var source=new APLinePurchaseSourceReview(APInvoiceSourceReview.Create(Scope,"accountant",selected,[Line("invoice")]),"line-invoice",
            PurchaseOrderSourceReview.Create(Scope,"accountant",new(){PURCHASE_ORDER_ID="order",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier"},
                [new(){PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line"}],[new(){PO_RECEIPT_ID="receipt",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",RECEIVED_QUANTITY=5m,QUANTITY_UOM="bbl"}]));
        var fixture=Reads(filters=>{
            Assert.Contains(filters,f=>f.FieldName=="OWNER_TENANT_ID" && f.FilterValue=="tenant");
            Assert.Contains(filters,f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue=="company");
            var invoice=Invoice();invoice.AP_INVOICE_ID=filters.Single(f=>f.FieldName=="AP_INVOICE_ID").FilterValue;
            if(fault=="foreign")invoice.OWNER_COMPANY_BA_ID="foreign";
            return fault=="missing"?Array.Empty<object>():new object[]{invoice};});
        fixture.Source.Setup(s=>s.GetEntityAsync("AP_INVOICE_LINE",It.IsAny<List<AppFilter>>())).ReturnsAsync((string _,List<AppFilter> filters)=>
            fault=="line"?Array.Empty<object>():new object[]{Line(filters.Single().FilterValue)});
        var row=new AP_RECEIPT_ALLOCATION{OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",AP_RECEIPT_ALLOCATION_ID="second-allocation",AP_INVOICE_ID="second",AP_INVOICE_LINE_ID="line-second",PURCHASE_ORDER_ID="order",
            PO_LINE_ITEM_ID="po-line",PO_RECEIPT_ID=fault=="receipt"?"other":"receipt",ALLOCATED_QUANTITY=2m,QUANTITY_UOM="bbl",ACTIVE_IND="Y"};
        if(fault=="owner-tenant")row.OWNER_TENANT_ID="foreign";if(fault=="owner-company")row.OWNER_COMPANY_BA_ID="foreign";if(fault=="owner-missing")row.OWNER_COMPANY_BA_ID=null;
        fixture.Source.Setup(s=>s.GetEntityAsync("AP_RECEIPT_ALLOCATION",It.IsAny<List<AppFilter>>())).ReturnsAsync((string _,List<AppFilter> filters)=>{
            Assert.Equal(3,filters.Count);Assert.Contains(filters,f=>f.FieldName=="OWNER_TENANT_ID" && f.FilterValue=="tenant" && f.Operator=="=");
            Assert.Contains(filters,f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue=="company" && f.Operator=="=");
            var filter=Assert.Single(filters,f=>f.FieldName=="PO_RECEIPT_ID");Assert.Equal("receipt",filter.FilterValue);Assert.Equal("=",filter.Operator);
            return fault=="null"?null!:fault=="duplicate"?new object[]{row,row}:new object[]{row};});
        var authorityCalls=0;
        Task Guard(){if(++authorityCalls==2 && fault=="revoked")throw new UnauthorizedAccessException();return Task.CompletedTask;}
        if(fault=="valid"){
            var review=await fixture.Service.GetReceiptConsumptionAsync(Scope,"accountant",source,"receipt",Guard);
            Assert.Equal("second",Assert.Single(review.Contributors).Source.Invoice.Invoice.AP_INVOICE_ID);Assert.Equal(2m,review.RecordedActiveQuantity);Assert.Equal(3,authorityCalls);
        }
        else if(fault=="revoked")await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Service.GetReceiptConsumptionAsync(Scope,"accountant",source,"receipt",Guard));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.GetReceiptConsumptionAsync(Scope,"accountant",source,"receipt",Guard));
        Assert.DoesNotContain(fixture.Source.Invocations,call=>new[]{"Insert","Update","Delete","BeginTransaction","Commit"}.Any(prefix=>call.Method.Name.StartsWith(prefix,StringComparison.Ordinal)));
    }
    [Fact] public void AllocationBeepMetadataHasOneBoundedNonnullableKey()
    {
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(AP_RECEIPT_ALLOCATION));var key=Assert.Single(schema.Fields,field=>field.IsKey);
        Assert.Equal("AP_RECEIPT_ALLOCATION_ID",key.FieldName);Assert.False(key.AllowDBNull);Assert.Equal(128,key.Size);
        Assert.Equal(64,Assert.Single(schema.Fields,field=>field.FieldName=="QUANTITY_UOM").Size);
        Assert.Equal(typeof(CompanyScope).Assembly,typeof(AP_RECEIPT_ALLOCATION).Assembly);
        foreach(var name in new[]{"OWNER_TENANT_ID","OWNER_COMPANY_BA_ID"}){
            var owner=Assert.Single(schema.Fields,field=>field.FieldName==name);Assert.Equal(128,owner.Size);Assert.True(owner.AllowDBNull);
        }
    }
}
