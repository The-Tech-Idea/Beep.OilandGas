using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class AccountingSourceHttpTests
{
    private sealed class TestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder)
        :AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if(Request.Headers["X-Test-Auth"]!="yes")return Task.FromResult(AuthenticateResult.NoResult());
            var actor=Request.Headers["X-Test-Actor"].FirstOrDefault() ?? "actor";
            var claims=new List<Claim>{new(ClaimTypes.NameIdentifier,actor),new("party_id",actor)};
            if(Request.Headers["X-Test-Role"]=="yes")claims.Add(new(ClaimTypes.Role,Request.Headers["X-Test-Assigned-Role"].FirstOrDefault() ?? "ConfiguredCompanyRole"));
            return Task.FromResult(AuthenticateResult.Success(new(new ClaimsPrincipal(new ClaimsIdentity(claims,Scheme.Name)),Scheme.Name)));
        }
    }
    internal sealed class ReadContext(IHttpContextAccessor accessor):ICompanyAccountingReadContext,ICompanyAccountingProcessReadContext
    {
        public string[] Permissions=["Accounting.View","Procurement.View"];
        public string[] Roles=["ConfiguredCompanyRole"];
        public string Company="company";
        public string Actor="actor";
        public string? StartBinding,Preparer;
        public string? PolicyBinding;
        public DateTime? PreparedAt;
        public bool SourceEdited,LineMakerMissing;
        public Action<int>? OnCapture;
        public string? VendorId;
        public string InvoiceId="invoice",OrderId="order",LineId="line",OrderLineId="po-line",ReceiptId="receipt";
        public int Calls,Reads,OrderReads;
        public Mock<IDataSource>? Source;
        public bool ChangeBinding,FailProvider;
        public string ProcessId="process",ProcessType="AP_INVOICE";
        public int ProcessReads;
        public bool MissingProcess,ForeignProcess,FieldProcess,ChangedProcess,ForeignNative,ChangedNative;
        public bool IncludeTaskStep;
        public string TaskStepRole="ConfiguredCompanyRole";
        public string TaskStepName="Recorded review step";
        public string? TaskStepAssignee;
        public Beep.OilandGas.Models.Processes.StepStatus TaskStepStatus=Beep.OilandGas.Models.Processes.StepStatus.PENDING;
        public Action<int>? OnProcessRead;
        public Mock<Beep.OilandGas.Models.Core.Interfaces.IAccessControlService> Resources=new(MockBehavior.Strict);
        public Beep.OilandGas.Models.Core.Interfaces.IProcessService? FixedProcess;
        public Mock<Beep.OilandGas.Models.Core.Interfaces.IProcessService> Processes=new(MockBehavior.Strict);
        public async Task<(RepositoryCompanyAccess Access,string Binding,string Actor,Beep.OilandGas.Models.Core.Interfaces.IProcessService Process)> CaptureProcessAsync(ClaimsPrincipal user)
        {var current=await CaptureAsync(user);return(current.Access,StartBinding is not null && !ChangeBinding?StartBinding:current.Binding+"+lifecycle",current.Actor,FixedProcess ?? Processes.Object);}
        public string RequestCompany()=>accessor.HttpContext!.Request.Headers[CompanyRequestHeaders.Company].FirstOrDefault()
            ?? accessor.HttpContext.Request.Headers["X-Test-Company"].FirstOrDefault() ?? "company";
        public Task<(RepositoryCompanyAccess Access,string Binding,string Actor)> CaptureAsync(ClaimsPrincipal user)
        {
            if(user.Identity?.IsAuthenticated!=true || user.FindFirstValue(ClaimTypes.NameIdentifier)!=Actor)throw new UnauthorizedAccessException();
            Calls++;OnCapture?.Invoke(Calls);Company=accessor.HttpContext!.Request.Headers[CompanyRequestHeaders.Company].FirstOrDefault()
                ?? accessor.HttpContext.Request.Headers["X-Test-Company"].FirstOrDefault() ?? "company";
            return Task.FromResult((new RepositoryCompanyAccess(new("tenant",Company),Roles,Permissions),ChangeBinding && Calls>1?"changed":PolicyBinding ?? "binding",Actor));
        }
    }
    internal sealed class Host(WebApplication app,HttpClient client,ReadContext context):IAsyncDisposable
    {
        public HttpClient Client=>client;public ReadContext Context=>context;
        public async ValueTask DisposeAsync(){client.Dispose();await app.DisposeAsync();}
    }
    internal static async Task<Host> Start(bool authenticate=true,bool role=true,string[]? permissions=null,Action<IServiceCollection,ReadContext>? configure=null,
        Action<IServiceCollection,ReadContext>? configureAccounting=null)
    {
        var builder=WebApplication.CreateBuilder();builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");builder.Services.AddHttpContextAccessor();
        builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions,TestAuthentication>("test",_=>{});builder.Services.AddAuthorization();
        builder.Services.AddControllers().AddApplicationPart(typeof(APController).Assembly);
        builder.Services.AddRefusalAnswers();
        var accessor=new HttpContextAccessor();var context=new ReadContext(accessor);if(permissions is not null)context.Permissions=permissions;
        builder.Services.AddSingleton<IHttpContextAccessor>(accessor);builder.Services.AddSingleton<ICompanyAccountingReadContext>(context);
        builder.Services.AddSingleton<ICompanyAccountingProcessReadContext>(context);
        builder.Services.AddScoped<CompanyAccountingProcessReads>();
        builder.Services.AddSingleton(new Mock<IDevelopmentInvestmentAssignedReader>(MockBehavior.Strict).Object);
        builder.Services.AddSingleton(new Mock<IDrillingProgramAssignedReviewReader>(MockBehavior.Strict).Object);
        builder.Services.AddSingleton(new Mock<IDevelopmentAssignedReviewReader>(MockBehavior.Strict).Object);
        builder.Services.AddSingleton(context.Resources.Object);
        configure?.Invoke(builder.Services,context);
        context.Processes.Setup(value=>value.GetProcessInstanceAsync(It.IsAny<string>())).ReturnsAsync((string id)=>{
            context.ProcessReads++;context.OnProcessRead?.Invoke(context.ProcessReads);if(context.MissingProcess)return null;
            var entity=context.ProcessType=="AP_INVOICE"?context.InvoiceId:context.OrderId;
            return new Beep.OilandGas.Models.Processes.ProcessInstance{InstanceId=context.ProcessId,ProcessId="definition",EntityType=context.ProcessType,EntityId=entity,
                FieldId=context.FieldProcess?"unrelated":"",CurrentState=context.ChangedProcess && context.ProcessReads>1?"changed":"review",
                Status=context.IncludeTaskStep?Beep.OilandGas.Models.Processes.ProcessStatus.IN_PROGRESS:Beep.OilandGas.Models.Processes.ProcessStatus.NOT_STARTED,
                CurrentStepId=context.IncludeTaskStep?"review":"",StepInstances=context.IncludeTaskStep?[new(){InstanceId=context.ProcessId,StepInstanceId="step",StepId="review",StepName=context.TaskStepName,RequiredRole=context.TaskStepRole,AssignedTo=context.TaskStepAssignee,Status=context.TaskStepStatus}]:[],
                StartDate=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc),StartedBy="maker",ProcessData=new(){InstanceId=context.ProcessId,Data=new(){
                    [CompanyAccountingProcessBinding.PayloadKey]=new CompanyAccountingProcessBinding(new("tenant",context.ForeignProcess?"foreign":context.RequestCompany()),context.ProcessId,context.ProcessType,entity)}}};
        });
        var source=new Mock<IDataSource>();context.Source=source;
        var types=new Dictionary<string,Type>{{"ACCOUNTING_METHOD",typeof(ACCOUNTING_METHOD)},{"AP_INVOICE",typeof(AP_INVOICE)},{"AP_INVOICE_LINE",typeof(AP_INVOICE_LINE)},
            {"PURCHASE_ORDER",typeof(PURCHASE_ORDER)},{"PO_LINE_ITEM",typeof(PO_LINE_ITEM)},{"PO_RECEIPT",typeof(PO_RECEIPT)},{"AP_RECEIPT_ALLOCATION",typeof(AP_RECEIPT_ALLOCATION)}};
        source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>
        {
            var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(types[request.EntityName]);
            // Explicit physical-key evidence for this provider fixture; reflection alone does not establish a deployed key.
            if(request.EntityName is "AP_INVOICE" or "AP_INVOICE_LINE" or "AP_RECEIPT_ALLOCATION" or "PURCHASE_ORDER" or "PO_LINE_ITEM" or "PO_RECEIPT")
                foreach(var field in schema.Fields){field.IsKey=field.FieldName==request.EntityName+"_ID";if(field.IsKey)field.AllowDBNull=false;}
            return schema;
        });
        source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
            context.Reads++;if(context.FailProvider)throw new InvalidOperationException("private provider detail");
            string Value(string field,string fallback)=>filters.SingleOrDefault(filter=>filter.FieldName==field)?.FilterValue ?? fallback;
            var company=Value("OWNER_COMPANY_BA_ID",context.Company);var vendor=context.VendorId ?? Value("VENDOR_BA_ID","supplier");
            if(context.ForeignNative)company="foreign";if(context.ChangedNative && context.ProcessReads>1)vendor="changed";
            if(table=="PURCHASE_ORDER")context.OrderReads++;
            return table switch{
                "ACCOUNTING_METHOD"=>Array.Empty<object>(),
                "AP_INVOICE"=>new object[]{new AP_INVOICE{AP_INVOICE_ID=context.InvoiceId,OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID=company,VENDOR_BA_ID=vendor,ACTIVE_IND="Y",ROW_CREATED_BY=context.Preparer,ROW_CREATED_DATE=context.PreparedAt,ROW_CHANGED_BY=context.SourceEdited?"editor":null,ROW_CHANGED_DATE=context.SourceEdited?context.PreparedAt:null,CURRENCY_CODE="KWD",TOTAL_AMOUNT=10m}},
                "AP_INVOICE_LINE"=>new object[]{new AP_INVOICE_LINE{AP_INVOICE_LINE_ID=context.LineId,AP_INVOICE_ID=context.InvoiceId,ROW_CREATED_BY=context.LineMakerMissing?null:context.Preparer,ROW_CREATED_DATE=context.LineMakerMissing?null:context.PreparedAt,PURCHASE_ORDER_ID=context.OrderId,PO_LINE_ITEM_ID=context.OrderLineId}},
                "PURCHASE_ORDER"=>new object[]{new PURCHASE_ORDER{PURCHASE_ORDER_ID=context.OrderId,OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID=company,VENDOR_BA_ID=vendor,ACTIVE_IND="Y",ROW_CREATED_BY=context.Preparer,ROW_CREATED_DATE=context.PreparedAt,ROW_CHANGED_BY=context.SourceEdited?"editor":null,ROW_CHANGED_DATE=context.SourceEdited?context.PreparedAt:null,CURRENCY_CODE="KWD"}},
                "PO_LINE_ITEM"=>new object[]{new PO_LINE_ITEM{PO_LINE_ITEM_ID=context.OrderLineId,PURCHASE_ORDER_ID=context.OrderId,ROW_CREATED_BY=context.LineMakerMissing?null:context.Preparer,ROW_CREATED_DATE=context.LineMakerMissing?null:context.PreparedAt}},
                "PO_RECEIPT"=>new object[]{new PO_RECEIPT{PO_RECEIPT_ID=context.ReceiptId,PURCHASE_ORDER_ID=context.OrderId,PO_LINE_ITEM_ID=context.OrderLineId,ACTIVE_IND="Y",RECEIVED_QUANTITY=5m,QUANTITY_UOM="bbl"}},
                "AP_RECEIPT_ALLOCATION"=>new object[]{new AP_RECEIPT_ALLOCATION{OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID=company,AP_RECEIPT_ALLOCATION_ID="allocation",AP_INVOICE_ID=context.InvoiceId,AP_INVOICE_LINE_ID=context.LineId,PURCHASE_ORDER_ID=context.OrderId,PO_LINE_ITEM_ID=context.OrderLineId,PO_RECEIPT_ID=context.ReceiptId,ALLOCATED_QUANTITY=1m,QUANTITY_UOM="bbl"}},
                _=>throw new InvalidOperationException("Unexpected native source.")};
        });
        var editor=new Mock<IDMEEditor>();editor.Setup(value=>value.GetDataSource("production")).Returns(source.Object);
        var columns=Mock.Of<ICommonColumnHandler>();var defaults=Mock.Of<IPPDM39DefaultsRepository>();var metadata=Mock.Of<IPPDMMetadataRepository>();
        Func<Task<string>> resolve=()=>Task.FromResult("production");
        var accounts=new GLAccountService(editor.Object,columns,defaults,metadata,NullLogger<GLAccountService>.Instance,resolve);
        var journals=new JournalEntryService(editor.Object,columns,defaults,metadata,accounts,NullLogger<JournalEntryService>.Instance,resolve);
        builder.Services.AddSingleton(new APInvoiceService(editor.Object,columns,defaults,metadata,new AccountingBasisPostingService(journals),NullLogger<APInvoiceService>.Instance,resolveConnection:resolve));
        builder.Services.AddSingleton(new PurchaseOrderService(editor.Object,columns,defaults,metadata,NullLogger<PurchaseOrderService>.Instance,resolve));
        builder.Services.AddSingleton(new AccountingPolicyService(editor.Object,columns,defaults,metadata,NullLogger<AccountingPolicyService>.Instance,resolve));
        configureAccounting?.Invoke(builder.Services,context);
        var app=builder.Build();app.UseExceptionHandler();app.UseAuthentication();app.UseAuthorization();app.MapControllers();await app.StartAsync();
        var client=new HttpClient{BaseAddress=new Uri(app.Urls.Single())};if(authenticate)client.DefaultRequestHeaders.Add("X-Test-Auth","yes");if(role)client.DefaultRequestHeaders.Add("X-Test-Role","yes");
        return new(app,client,context);
    }
    private const string AP="/api/accounting/traditional/ap",PO="/api/accounting/traditional/purchase-order";
    [Theory][InlineData("AP_INVOICE","allowed")][InlineData("PURCHASE_ORDER","allowed")]
    [InlineData("AP_INVOICE","anonymous")][InlineData("AP_INVOICE","role")][InlineData("AP_INVOICE","permission")]
    [InlineData("AP_INVOICE","missing")][InlineData("AP_INVOICE","foreign")][InlineData("AP_INVOICE","field")]
    [InlineData("AP_INVOICE","binding")][InlineData("AP_INVOICE","process-changed")][InlineData("AP_INVOICE","native-changed")]
    [InlineData("AP_INVOICE","native-foreign")][InlineData("PURCHASE_ORDER","native-foreign")]
    public async Task CompanyAccountingProcessHttpRequiresOwnedCurrentProcessAndNativeSource(string type,string condition)
    {
        await using var host=await Start(authenticate:condition!="anonymous",role:condition!="role",permissions:condition=="permission"?[]:null);
        host.Context.ProcessType=type;host.Context.MissingProcess=condition=="missing";host.Context.ForeignProcess=condition=="foreign";
        host.Context.FieldProcess=condition=="field";host.Context.ChangeBinding=condition=="binding";host.Context.ChangedProcess=condition=="process-changed";
        host.Context.ChangedNative=condition=="native-changed";host.Context.ForeignNative=condition=="native-foreign";
        var response=await host.Client.GetAsync("/api/accounting/processes/process");
        var expected=condition=="allowed"?HttpStatusCode.OK:condition=="anonymous"?HttpStatusCode.Unauthorized:
            condition is "role" or "permission" or "binding"?HttpStatusCode.Forbidden:condition=="missing"?HttpStatusCode.NotFound:HttpStatusCode.Conflict;
        Assert.Equal(expected,response.StatusCode);
        if(condition=="allowed"){
            var process=await response.Content.ReadFromJsonAsync<Beep.OilandGas.Models.Processes.ProcessInstance>();Assert.Equal(type,process!.EntityType);Assert.Equal("",process.FieldId);
            CompanyAccountingProcessBinding.FromJson(System.Text.Json.JsonSerializer.Serialize(process.ProcessData.Data[CompanyAccountingProcessBinding.PayloadKey]),new("tenant","company"),"process",type,type=="AP_INVOICE"?"invoice":"order");
            Assert.Equal(2,host.Context.ProcessReads);Assert.True(host.Context.Reads>0);
        }
        if(condition is "anonymous" or "role" or "permission" or "binding"){Assert.Equal(0,host.Context.ProcessReads);Assert.Equal(0,host.Context.Reads);}
        if(condition is "foreign" or "field" or "missing")Assert.Equal(0,host.Context.Reads);
        Assert.DoesNotContain(host.Context.Source!.Invocations,call=>new[]{"Insert","Update","Delete","BeginTransaction","Commit"}.Any(prefix=>call.Method.Name.StartsWith(prefix,StringComparison.Ordinal)));
    }
    [Fact] public async Task CompanyAccountingProcessHttpPreservesEncodedIdentityAndTwoCompanyOwners()
    {
        await using var host=await Start();host.Context.ProcessId="process/α%2F";
        foreach(var company in new[]{"first","second"}){
            host.Client.DefaultRequestHeaders.Remove("X-Test-Company");host.Client.DefaultRequestHeaders.Add("X-Test-Company",company);
            var process=await host.Client.GetFromJsonAsync<Beep.OilandGas.Models.Processes.ProcessInstance>("/api/accounting/processes/"+Uri.EscapeDataString(host.Context.ProcessId));
            Assert.Equal(host.Context.ProcessId,process!.InstanceId);
            var binding=CompanyAccountingProcessBinding.FromJson(System.Text.Json.JsonSerializer.Serialize(process.ProcessData.Data[CompanyAccountingProcessBinding.PayloadKey]),new("tenant",company),host.Context.ProcessId,"AP_INVOICE","invoice");
            Assert.Equal(company,binding.Company.BusinessAssociateId);
        }
    }
    [Theory][InlineData(AP)][InlineData(PO)] public async Task AnonymousRouteIsChallengedBeforeAnySourceRead(string route)
    {await using var host=await Start(authenticate:false);Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync(route)).StatusCode);Assert.Equal(0,host.Context.Reads);}
    [Theory][InlineData(AP,false)][InlineData(AP,true)][InlineData(PO,false)][InlineData(PO,true)]
    public async Task MissingStandardRoleOrRequiredPermissionIsForbiddenBeforeSourceRead(string route,bool missingPermission)
    {await using var host=await Start(role:missingPermission,permissions:missingPermission?[]:null);Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(route)).StatusCode);Assert.Equal(0,host.Context.Reads);}
    [Theory][InlineData(AP,"%20")][InlineData(PO,"%20")][InlineData(AP,"")][InlineData(PO,"")][InlineData(AP,"supplier&vendorId=other")][InlineData(PO,"supplier&vendorId=other")] public async Task EmptyOrRepeatedSupplierAssertionIsAnHttpBadRequest(string route,string value)
    {await using var host=await Start();Assert.Equal(HttpStatusCode.BadRequest,(await host.Client.GetAsync(route+"?vendorId="+value)).StatusCode);Assert.Equal(0,host.Context.Reads);}
    [Theory][InlineData(AP)][InlineData(PO)] public async Task LegacyCreationRemainsFencedWithoutAProviderWrite(string route)
    {await using var host=await Start();Assert.Equal(HttpStatusCode.Conflict,(await host.Client.PostAsync(route,null)).StatusCode);Assert.Equal(0,host.Context.Reads);host.Context.Source!.VerifyNoOtherCalls();}
    [Theory][InlineData(AP)][InlineData(PO)] public async Task ChangedBindingPreventsPublishingTheList(string route)
    {await using var host=await Start();host.Context.ChangeBinding=true;Assert.Equal(HttpStatusCode.Conflict,(await host.Client.GetAsync(route)).StatusCode);Assert.Equal(1,host.Context.Reads);}
    [Theory][InlineData(AP)][InlineData(PO)] public async Task ProviderFailureHasAFixedConflictWithoutProviderDetails(string route)
    {await using var host=await Start();host.Context.FailProvider=true;var response=await host.Client.GetAsync(route);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.DoesNotContain("private provider detail",await response.Content.ReadAsStringAsync());}
    [Theory][InlineData(AP)][InlineData(PO)] public async Task TwoCompaniesWithCollidingLocalIdsAndEncodedSupplierRetainTheirOwner(string route)
    {
        await using var host=await Start();foreach(var company in new[]{"first","second"}){
            host.Client.DefaultRequestHeaders.Remove("X-Test-Company");host.Client.DefaultRequestHeaders.Add("X-Test-Company",company);
            var response=await host.Client.GetAsync(route+"?vendorId="+Uri.EscapeDataString("supplier/a?ب"));Assert.Equal(HttpStatusCode.OK,response.StatusCode);
            var body=await response.Content.ReadAsStringAsync();Assert.Contains(company,body);using var json=System.Text.Json.JsonDocument.Parse(body);Assert.Equal("supplier/a?ب",json.RootElement[0].EnumerateObject().Single(property=>property.Name.Equals("VENDOR_BA_ID",StringComparison.OrdinalIgnoreCase)).Value.GetString());
        }
    }
    [Fact] public async Task ActualInvoiceToOwnedOrderToAllocationRoutesReturnSharedReviews()
    {
        await using var host=await Start();var invoice=await host.Client.GetFromJsonAsync<APInvoiceSourceReview>(AP+"/invoice");
        var purchase=await host.Client.GetFromJsonAsync<APLinePurchaseSourceReview>(AP+"/invoice/lines/line/purchase-source?expectedInvoiceRevision="+invoice!.Revision);
        var allocation=await host.Client.GetFromJsonAsync<APReceiptAllocationReview>(AP+"/invoice/lines/line/receipt-allocations?expectedInvoiceRevision="+invoice.Revision+"&expectedOrderRevision="+purchase!.Order.Revision);
        allocation!.Require(new("tenant","company"),"actor","invoice","line",invoice.Revision);Assert.Equal("receipt",Assert.Single(allocation.Allocations).PO_RECEIPT_ID);
    }
    [Fact] public async Task ProcurementPermissionIsRequiredAtThePurchaseSourceRoute()
    {
        await using var host=await Start(permissions:["Accounting.View"]);var invoice=await host.Client.GetFromJsonAsync<APInvoiceSourceReview>(AP+"/invoice");
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(AP+"/invoice/lines/line/purchase-source?expectedInvoiceRevision="+invoice!.Revision)).StatusCode);Assert.Equal(0,host.Context.OrderReads);
    }
    [Theory][InlineData(AP,"شركة α")][InlineData(PO,"شركة α")][InlineData(AP,"id/a?b")][InlineData(PO,"id/a?b")]
    [InlineData(AP,"literal%2F")][InlineData(PO,"literal%2F")][InlineData(AP,"percent%25")][InlineData(PO,"percent%25")]
    public async Task EncodedDetailLinkPreservesTheExactRecordedIdentity(string route,string id)
    {
        await using var host=await Start();host.Context.InvoiceId=id;host.Context.OrderId=id;
        var response=await host.Client.GetAsync(route+"/"+Uri.EscapeDataString(id));Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        if(route==AP){var review=await response.Content.ReadFromJsonAsync<APInvoiceSourceReview>();Assert.Equal(id,review!.Invoice.AP_INVOICE_ID);}
        else{var review=await response.Content.ReadFromJsonAsync<PurchaseOrderSourceReview>();Assert.Equal(id,review!.Header.PURCHASE_ORDER_ID);}
    }
    [Theory][InlineData(AP,"invoices")][InlineData(PO,"orders")]
    public async Task ExistingSupplierRoutePreservesOneExactEscapedSupplier(string root,string suffix)
    {
        await using var host=await Start();host.Context.VendorId="supplier/a?ب%2F";
        var response=await host.Client.GetAsync(root+"/vendor/"+Uri.EscapeDataString(host.Context.VendorId)+"/"+suffix);
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        using var json=System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(host.Context.VendorId,json.RootElement[0].EnumerateObject().Single(property=>property.Name.Equals("VENDOR_BA_ID",StringComparison.OrdinalIgnoreCase)).Value.GetString());
    }
    [Fact] public async Task EscapedInvoiceAndLineIdsReachTheExactOwnedOrderAndAllocation()
    {
        await using var host=await Start();host.Context.InvoiceId="invoice/a?ب";host.Context.LineId="line/%2F?λ";host.Context.OrderId="order/a?ب";
        var path=AP+"/"+Uri.EscapeDataString(host.Context.InvoiceId);var invoice=await host.Client.GetFromJsonAsync<APInvoiceSourceReview>(path);
        var purchase=await host.Client.GetFromJsonAsync<APLinePurchaseSourceReview>(path+"/lines/"+Uri.EscapeDataString(host.Context.LineId)+"/purchase-source?expectedInvoiceRevision="+invoice!.Revision);
        Assert.Equal(host.Context.LineId,purchase!.InvoiceLineId);Assert.Equal(host.Context.OrderId,purchase.Order.Header.PURCHASE_ORDER_ID);
        var allocation=await host.Client.GetFromJsonAsync<APReceiptAllocationReview>(path+"/lines/"+Uri.EscapeDataString(host.Context.LineId)+"/receipt-allocations?expectedInvoiceRevision="+invoice.Revision+"&expectedOrderRevision="+purchase.Order.Revision);
        allocation!.Require(new("tenant","company"),"actor",host.Context.InvoiceId,host.Context.LineId,invoice.Revision);Assert.Single(allocation.Allocations);
    }
    [Theory][InlineData("شركة α")][InlineData("id/a?b")][InlineData("literal%2F")][InlineData("percent%25")][InlineData("id#fragment")][InlineData("id+value")]
    public void WebDetailLocationPreservesOriginalIdentityExactlyOnce(string id)
    {
        var encoded=Uri.EscapeDataString(id);
        Assert.Equal(id,RecordedSourceId.FromDetailLocation("https://example.test/app/accounting/payables/invoices/"+encoded+"?tab=source","https://example.test/app/","accounting/payables/invoices/"));
        Assert.Equal(id,RecordedSourceId.FromDetailLocation("https://example.test/app/procurement/orders/"+encoded,"https://example.test/app/","procurement/orders/"));
    }
    [Theory][InlineData("%")][InlineData("%GG")][InlineData("")][InlineData("a/b")][InlineData("%0A")]
    public void InvalidSourceSegmentDoesNotAcquireARecordedIdentity(string escaped)
    {Assert.Throws<FormatException>(()=>RecordedSourceId.DecodeSegment(escaped));}
    [Theory][InlineData("https://other.test/app/procurement/orders/order")][InlineData("https://example.test/app/procurement/orders/order/extra")]
    public void WebDetailLocationRefusesOtherRootsOrExtraSegments(string location)
    {Assert.Throws<FormatException>(()=>RecordedSourceId.FromDetailLocation(location,"https://example.test/app/","procurement/orders/"));}
    [Theory][InlineData("allowed")][InlineData("changed")][InlineData("foreign")][InlineData("revoked")]
    public async Task ProposalHttpReviewIncludesOtherInvoiceContributorsOrRefusesTheWholeResult(string condition)
    {
        await using var host=await Start();var invoice=await host.Client.GetFromJsonAsync<APInvoiceSourceReview>(AP+"/invoice");
        var purchase=await host.Client.GetFromJsonAsync<APLinePurchaseSourceReview>(AP+"/invoice/lines/line/purchase-source?expectedInvoiceRevision="+invoice!.Revision);
        var allocation=await host.Client.GetFromJsonAsync<APReceiptAllocationReview>(AP+"/invoice/lines/line/receipt-allocations?expectedInvoiceRevision="+invoice.Revision+"&expectedOrderRevision="+purchase!.Order.Revision);
        host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareReceiptAllocation"];
        var globalReads=0;var source=host.Context.Source!;
        source.Setup(s=>s.GetEntityAsync("AP_RECEIPT_ALLOCATION",It.Is<List<AppFilter>>(filters=>filters.Count==3 && filters.Any(f=>f.FieldName=="PO_RECEIPT_ID"))))
            .ReturnsAsync((string _,List<AppFilter> filters)=>{
                globalReads++;if(condition=="revoked")host.Context.Permissions=["Accounting.View","Accounting.PrepareReceiptAllocation"];
                return new object[]{new AP_RECEIPT_ALLOCATION{OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",AP_RECEIPT_ALLOCATION_ID="other-allocation",AP_INVOICE_ID="second",AP_INVOICE_LINE_ID="second-line",
                    PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",PO_RECEIPT_ID="receipt",ALLOCATED_QUANTITY=condition=="changed" && globalReads==2?3m:2m,QUANTITY_UOM="bbl",ACTIVE_IND="Y"}};
            });
        source.Setup(s=>s.GetEntityAsync("AP_INVOICE",It.Is<List<AppFilter>>(filters=>filters.Any(f=>f.FieldName=="AP_INVOICE_ID" && f.FilterValue=="second"))))
            .ReturnsAsync(new object[]{new AP_INVOICE{AP_INVOICE_ID="second",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID=condition=="foreign"?"foreign":"company",VENDOR_BA_ID="supplier",ACTIVE_IND="Y"}});
        source.Setup(s=>s.GetEntityAsync("AP_INVOICE_LINE",It.Is<List<AppFilter>>(filters=>filters.Any(f=>f.FieldName=="AP_INVOICE_ID" && f.FilterValue=="second"))))
            .ReturnsAsync(new object[]{new AP_INVOICE_LINE{AP_INVOICE_ID="second",AP_INVOICE_LINE_ID="second-line",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line"}});
        var proposal=new APReceiptAllocationProposal("receipt",1m,"bbl",invoice.Revision,purchase.Order.Revision,allocation!.Revision);
        var response=await host.Client.PostAsJsonAsync(AP+"/invoice/lines/line/receipt-allocation-review",proposal);
        Assert.Equal(condition=="allowed"?HttpStatusCode.OK:condition=="revoked"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,response.StatusCode);
        if(condition=="allowed"){
            var returned=await response.Content.ReadFromJsonAsync<APReceiptAllocationProposalReview>();returned!.Require(new("tenant","company"),"actor","invoice","line",proposal);
            Assert.Equal("second",Assert.Single(returned.ReceiptConsumption!.Contributors).Source.Invoice.Invoice.AP_INVOICE_ID);Assert.Equal(2m,returned.ReceiptConsumption.RecordedActiveQuantity);Assert.Equal(2,globalReads);
        }
        Assert.DoesNotContain(source.Invocations,call=>new[]{"Insert","Update","Delete","BeginTransaction","Commit"}.Any(prefix=>call.Method.Name.StartsWith(prefix,StringComparison.Ordinal)));
    }
    [Theory][InlineData(false,"tenant")][InlineData(false,"company")][InlineData(false,"missing")]
    [InlineData(true,"tenant")][InlineData(true,"company")][InlineData(true,"missing")]
    public async Task AllocationRoutesRejectForeignOrUnownedRowsEvenWhenAllSourceIdsMatch(bool proposalRoute,string fault)
    {
        await using var host=await Start();var invoice=await host.Client.GetFromJsonAsync<APInvoiceSourceReview>(AP+"/invoice");
        var purchase=await host.Client.GetFromJsonAsync<APLinePurchaseSourceReview>(AP+"/invoice/lines/line/purchase-source?expectedInvoiceRevision="+invoice!.Revision);
        var allocationUrl=AP+"/invoice/lines/line/receipt-allocations?expectedInvoiceRevision="+invoice.Revision+"&expectedOrderRevision="+purchase!.Order.Revision;
        var history=await host.Client.GetFromJsonAsync<APReceiptAllocationReview>(allocationUrl);
        host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareReceiptAllocation"];
        host.Context.Source!.Setup(s=>s.GetEntityAsync("AP_RECEIPT_ALLOCATION",It.Is<List<AppFilter>>(filters=>
            !proposalRoute || filters.Any(f=>f.FieldName=="PO_RECEIPT_ID")))).ReturnsAsync((string _,List<AppFilter> filters)=>{
            Assert.Contains(filters,f=>f.FieldName=="OWNER_TENANT_ID" && f.FilterValue=="tenant");
            Assert.Contains(filters,f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue=="company");
            return new object[]{new AP_RECEIPT_ALLOCATION{OWNER_TENANT_ID=fault=="tenant"?"foreign":"tenant",OWNER_COMPANY_BA_ID=fault=="company"?"foreign":fault=="missing"?null:"company",
                AP_RECEIPT_ALLOCATION_ID="allocation",AP_INVOICE_ID="invoice",AP_INVOICE_LINE_ID="line",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",PO_RECEIPT_ID="receipt",ALLOCATED_QUANTITY=1m,QUANTITY_UOM="bbl",ACTIVE_IND="Y"}};
        });
        var response=proposalRoute?await host.Client.PostAsJsonAsync(AP+"/invoice/lines/line/receipt-allocation-review",
            new APReceiptAllocationProposal("receipt",1m,"bbl",invoice.Revision,purchase.Order.Revision,history!.Revision)):
            await host.Client.GetAsync(allocationUrl);
        Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);
        Assert.DoesNotContain(host.Context.Source.Invocations,call=>new[]{"Insert","Update","Delete","BeginTransaction","Commit"}.Any(prefix=>call.Method.Name.StartsWith(prefix,StringComparison.Ordinal)));
    }
    [Fact] public async Task CollidingAllocationIdentitiesInTwoCompaniesKeepExplicitOwnershipAndQueryScope()
    {
        await using var host=await Start();var revisions=new List<string>();
        foreach(var company in new[]{"first","second"}){
            host.Client.DefaultRequestHeaders.Remove("X-Test-Company");host.Client.DefaultRequestHeaders.Add("X-Test-Company",company);
            var invoice=await host.Client.GetFromJsonAsync<APInvoiceSourceReview>(AP+"/invoice");
            var purchase=await host.Client.GetFromJsonAsync<APLinePurchaseSourceReview>(AP+"/invoice/lines/line/purchase-source?expectedInvoiceRevision="+invoice!.Revision);
            var review=await host.Client.GetFromJsonAsync<APReceiptAllocationReview>(AP+"/invoice/lines/line/receipt-allocations?expectedInvoiceRevision="+invoice.Revision+"&expectedOrderRevision="+purchase!.Order.Revision);
            review!.Require(new("tenant",company),"actor","invoice","line",invoice.Revision);
            Assert.Equal(company,Assert.Single(review.Allocations).OWNER_COMPANY_BA_ID);revisions.Add(review.Revision);
            Assert.Contains(host.Context.Source!.Invocations,call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="AP_RECEIPT_ALLOCATION" &&
                ((List<AppFilter>)call.Arguments[1]).Any(f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue==company));
        }
        Assert.NotEqual(revisions[0],revisions[1]);
    }
    [Theory][InlineData("allowed")][InlineData("permission")][InlineData("quantity")][InlineData("unit")][InlineData("revision")]
    public async Task ReadOnlyProposalRouteRequiresPreparationAuthorityAndExactReceiptEvidence(string condition)
    {
        await using var host=await Start();var invoice=await host.Client.GetFromJsonAsync<APInvoiceSourceReview>(AP+"/invoice");
        var purchase=await host.Client.GetFromJsonAsync<APLinePurchaseSourceReview>(AP+"/invoice/lines/line/purchase-source?expectedInvoiceRevision="+invoice!.Revision);
        var allocation=await host.Client.GetFromJsonAsync<APReceiptAllocationReview>(AP+"/invoice/lines/line/receipt-allocations?expectedInvoiceRevision="+invoice.Revision+"&expectedOrderRevision="+purchase!.Order.Revision);
        if(condition!="permission")host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareReceiptAllocation"];
        host.Context.Reads=0;
        var proposal=new APReceiptAllocationProposal("receipt",condition=="quantity"?6m:1m,condition=="unit"?"m³":"bbl",invoice.Revision,purchase.Order.Revision,
            condition=="revision"?new string('A',64):allocation!.Revision);
        var response=await host.Client.PostAsJsonAsync(AP+"/invoice/lines/line/receipt-allocation-review",proposal);
        Assert.Equal(condition=="allowed"?HttpStatusCode.OK:condition=="permission"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,response.StatusCode);
        if(condition=="permission")Assert.Equal(0,host.Context.Reads);
        if(condition=="allowed"){
            var reviewed=await response.Content.ReadFromJsonAsync<APReceiptAllocationProposalReview>();reviewed!.Require(new("tenant","company"),"actor","invoice","line",proposal);
            Assert.Equal(8,reviewed.Comparisons.Count);
            Assert.Equal(APSourceComparisonOutcome.Unavailable,Assert.Single(reviewed.Comparisons,row=>row.Code=="prices").Outcome);
            Assert.Equal(APSourceComparisonOutcome.Unavailable,Assert.Single(reviewed.Comparisons,row=>row.Code=="acceptance").Outcome);
        }
        Assert.DoesNotContain(host.Context.Source!.Invocations,call=>new[]{"Insert","Update","Delete","BeginTransaction","Commit"}.Any(prefix=>call.Method.Name.StartsWith(prefix,StringComparison.Ordinal)));
    }
}
