using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers.WorkOrder;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact] public Task ActualBudgetHttpRequiresCompanyPreparationAndWritesOneNativeAfeWithOriginalRecovery()=>BudgetHttp();
    [Theory][InlineData(WorkOrderBudgetDecision.Approve)][InlineData(WorkOrderBudgetDecision.Reject)]
    public Task ActualIndependentBudgetDecisionHttpRecordsExistingApprovalAndKeepsOriginalDraft(WorkOrderBudgetDecision decision)=>BudgetHttp(decision);
    [Fact] public Task ActualLaborCostHttpUsesRecordedQuantityApprovedBudgetAndExplicitRateOnce()=>BudgetHttp(WorkOrderBudgetDecision.Approve,true);
    [Fact] public Task ActualMaterialCostHttpValuesRecordedConsumptionAlongsideLaborWithoutAnotherStockMovement()=>BudgetHttp(WorkOrderBudgetDecision.Approve,true,true);
    private async Task BudgetHttp(WorkOrderBudgetDecision? decision=null,bool laborCost=false,bool materialCost=false)
    {
        using var f=await Ready();var order=laborCost?"order-الكويت":"order-budget";await f.Grant("FIELD","A","WRITE");await f.Grant("WORK_ORDER",order,"WRITE");
        f.EnableLeaseTransactions();
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);
        var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var owners=MaintenanceReadOwners(f,modules,profile);
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="PRODUCTION",ConnectionName="selected-tenant",ConnectionKey="SELECTED-TENANT",ConcurrencyStamp="budget-production-version"});await f.Db.SaveChangesAsync();
        f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID=order,WORK_ORDER_TYPE="CORRECTIVE",INSTRUCTIONS="Recorded work scope",ACTIVE_IND="Y"});
        f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID=order,COMPONENT_ID="budget-component",FIELD_ID="A",ACTIVE_IND="Y"});
        var creator=new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object);f.Editor.SetupGet(value=>value.classCreator).Returns(creator);
        f.Source.Setup(value=>value.CheckEntityExist("ACCOUNTING_COST")).Returns(true);
        f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName=="ACCOUNTING_COST"),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(ACCOUNTING_COST)));
        f.Metadata.Setup(value=>value.GetTableMetadataAsync("ACCOUNTING_COST")).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName="ACCOUNTING_COST",EntityTypeName=typeof(ACCOUNTING_COST).FullName!,PrimaryKeyColumn="ACCOUNTING_COST_ID",Module="PRODUCTION"});
        f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("ACCOUNTING_COST")).ReturnsAsync(["ACCOUNTING_COST_ID"]);
        f.Source.Setup(value=>value.CheckEntityExist("AFE")).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName=="AFE"),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(AFE)));
        f.Metadata.Setup(value=>value.GetTableMetadataAsync("AFE")).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName="AFE",EntityTypeName=typeof(AFE).FullName!,PrimaryKeyColumn="AFE_ID",Module="PRODUCTION"});
        f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("AFE")).ReturnsAsync(["AFE_ID"]);
        var binding=await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],await modules.ResolveAsync("PRODUCTION"));
        var config=new CompanyLedgerBookConfigurationCommand(Guid.NewGuid().ToString("N"),f.Gate.Company.Scope,"book-maker",Guid.NewGuid().ToString("N"),"recorded-book","Recorded KWD book","KWD",3,"TO_EVEN","calendar","UTC","Explicit configured book",new string('A',64),binding);
        var book=config.CreateRow();book.ROW_CREATED_BY=config.ActorUserId;book.ROW_CHANGED_BY=config.ActorUserId;book.ROW_CREATED_DATE=DateTime.UtcNow;book.ROW_CHANGED_DATE=book.ROW_CREATED_DATE;book.CONFIGURATION_JSON=CompanyLedgerBookConfigurationProof.Create(config,book);
        // Explicit native-book source seam; actual book-reader/provider admission is a separate gate.
        var books=new Mock<ICompanyLedgerBookReader>();books.Setup(value=>value.GetCompanyBooksAsync(It.IsAny<CompanyScope>(),It.IsAny<string>(),It.IsAny<Func<Task>>())).Returns(async(CompanyScope company,string actor,Func<Task> guard)=>{await guard();return CompanyLedgerBookReview.Create(company,actor,[book]);});
        var native=new AfeService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object);
        var service=new CompanyWorkOrderBudgetService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),boundary.Orchestrator.Object,owners.Order,books.Object,native,new AuthorizationWorkflowService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object));
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyWorkOrderBudgetController),services=>{services.AddSingleton(service);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();});
        var query=new CompanyWorkOrderBudgetQuery(order,"A",config.BookId);var prefix=host.Url+"/api/field/current/workorder/"+order+"/budget";
        int Reads()=>f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="AFE");
        var initial=Reads();using(var denied=await host.Http.PostAsJsonAsync(prefix+"/review",query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(initial,Reads());Assert.Empty(f.Writes);
        using(var deniedDiscovery=await host.Http.PostAsJsonAsync(prefix+"/financial-sources",new CompanyWorkOrderFinancialQuery(order,"A")))Assert.Equal(HttpStatusCode.Forbidden,deniedDiscovery.StatusCode);Assert.Equal(initial,Reads());
        var view=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Accounting.View"};var prepare=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Accounting.PrepareWorkOrderBudget"};
        f.Db.RoleClaims.AddRange(view,prepare);await f.Db.SaveChangesAsync();
        async Task<T> Post<T>(string path,object body)
        {
            using var response=await host.Http.PostAsJsonAsync(prefix+path,body);Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());Assert.Equal("no-store",response.Headers.CacheControl?.ToString());return (await response.Content.ReadFromJsonAsync<T>())!;
        }
        var review=await Post<CompanyWorkOrderBudgetReview>("/review",query);review.Require();Assert.Empty(review.Budgets);Assert.Equal("KWD",review.Book.FunctionalCurrency);
        var emptyFinancial=await Post<CompanyWorkOrderFinancialSources>("/financial-sources",new CompanyWorkOrderFinancialQuery(order,"A"));emptyFinancial.Require();Assert.Empty(emptyFinancial.Budgets);Assert.Single(emptyFinancial.Books.Books);Assert.Empty(f.Writes);
        var command=new CompanyWorkOrderBudgetCommand(review,Guid.NewGuid(),"AFE-work-budget","Explicit work-order budget",12.875m,DateTime.UtcNow,"Original native work scope and currency");
        var result=await Post<CompanyWorkOrderBudgetRecovery>("",command);var reference=new CompanyWorkOrderBudgetReference(query,command.IntentId);result.Require(f.Gate.Company.Scope,f.Gate.Company.UserId,reference);
        Assert.Single(f.Writes);Assert.Single(f.Rows.OfType<AFE>());Assert.Equal("DRAFT",result.Recorded.STATUS);Assert.Null(result.Recorded.ACTUAL_COST);Assert.Null(result.Recorded.APPROVAL_DATE);
        var original=JsonSerializer.Serialize(result.Recorded);Assert.Equal(original,JsonSerializer.Serialize((await Post<CompanyWorkOrderBudgetRecovery>("/recovery/reference",reference)).Recorded));
        Assert.Equal(original,JsonSerializer.Serialize((await Post<CompanyWorkOrderBudgetRecovery>("",command)).Recorded));Assert.Single(f.Writes);
        var current=await Post<CompanyWorkOrderBudgetReview>("/review",query);Assert.Single(current.Budgets);Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(current.Budgets));
        if(decision is {} action)
        {
            var reviewPermission=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Accounting.ReviewWorkOrderBudget"};f.Db.RoleClaims.Add(reviewPermission);await f.Db.SaveChangesAsync();
            var decisionQuery=new CompanyWorkOrderBudgetDecisionQuery(query,result.Recorded.AFE_ID);
            using(var self=await host.Http.PostAsJsonAsync(prefix+"/decision/review",decisionQuery))Assert.Equal(HttpStatusCode.Forbidden,self.StatusCode);Assert.Single(f.Writes);
            const string reviewer="budget-reviewer";f.Db.Users.Add(new OilGasUser{Id=reviewer,UserName=reviewer,IsActive=true,ConcurrencyStamp="budget-reviewer-version"});f.Db.UserRoles.Add(new IdentityUserRole<string>{UserId=reviewer,RoleId="admin"});await f.Db.SaveChangesAsync();
            f.Request.User=new(new System.Security.Claims.ClaimsIdentity([new("party_id",reviewer)],"fixture"));await f.Select("company",reviewer);await f.Grant("FIELD","A","WRITE");await f.Grant("WORK_ORDER",order,"WRITE");
            var reviewerProfile=Profile(f);await Save(reviewerProfile);using var reviewerBoundary=new Boundary(f,reviewerProfile);
            reviewerBoundary.Access.Setup(value=>value.CheckAssetAccessAsync(reviewer,"A","FIELD",It.IsAny<string?>(),null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
            var reviewerTenant=new RepositoryTenantContext();reviewerTenant.SelectCompany(f.Gate.Company);var reviewerModules=new ModuleConnectionResolver(f.Db,f.Editor.Object,reviewerTenant);var reviewerOwners=MaintenanceReadOwners(f,reviewerModules,reviewerProfile);
            f.Source.Setup(value=>value.CheckEntityExist("APPROVAL_WORKFLOW")).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName=="APPROVAL_WORKFLOW"),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(APPROVAL_WORKFLOW)));
            f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("APPROVAL_WORKFLOW")).ReturnsAsync(["APPROVAL_WORKFLOW_ID"]);
            f.Metadata.Setup(value=>value.GetTableMetadataAsync("APPROVAL_WORKFLOW")).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName="APPROVAL_WORKFLOW",EntityTypeName=typeof(APPROVAL_WORKFLOW).FullName!,PrimaryKeyColumn="APPROVAL_WORKFLOW_ID",Module="PRODUCTION"});
            var approvalOwner=new AuthorizationWorkflowService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object);
            var reviewerService=new CompanyWorkOrderBudgetService(f.Gate,reviewerModules,reviewerProfile,reviewerProfile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),reviewerBoundary.Orchestrator.Object,reviewerOwners.Order,books.Object,native,approvalOwner);
            await using var reviewerHost=await HttpHost.Start(f,reviewerProfile,reviewerBoundary,true,typeof(CompanyWorkOrderBudgetController),services=>{services.AddSingleton<Func<string>>(()=>reviewer);services.AddSingleton(reviewerService);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();services.AddAuthentication(options=>{options.DefaultAuthenticateScheme="receiving-fixture";options.DefaultChallengeScheme="receiving-fixture";}).AddScheme<AuthenticationSchemeOptions,ReceivingAuthentication>("receiving-fixture",_=>{});});
            async Task<T> ReviewPost<T>(string path,object body)
            {
                using var response=await reviewerHost.Http.PostAsJsonAsync(reviewerHost.Url+"/api/field/current/workorder/"+order+"/budget"+path,body);Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());Assert.Equal("no-store",response.Headers.CacheControl?.ToString());return (await response.Content.ReadFromJsonAsync<T>())!;
            }
            var decisionReview=await ReviewPost<CompanyWorkOrderBudgetDecisionReview>("/decision/review",decisionQuery);decisionReview.Require();Assert.Equal("actor",decisionReview.Original().CreatedBy);
            var independent=new CompanyWorkOrderBudgetDecisionCommand(decisionReview,Guid.NewGuid(),action,"Independent original work scope/book/currency and amount decision");
            var decided=await ReviewPost<CompanyWorkOrderBudgetDecisionRecovery>("/decision",independent);decided.Require(f.Gate.Company.Scope,reviewer,independent.Reference());
            Assert.Equal(2,f.Writes.Count);Assert.Single(f.Rows.OfType<APPROVAL_WORKFLOW>());Assert.Equal(action==WorkOrderBudgetDecision.Approve?"APPROVED":"REJECTED",decided.Recorded.STATUS);Assert.Equal(original,JsonSerializer.Serialize(f.Rows.OfType<AFE>().Single()));
            (await ReviewPost<CompanyWorkOrderBudgetDecisionRecovery>("/decision/recovery/reference",independent.Reference())).Require(f.Gate.Company.Scope,reviewer,independent.Reference());
            (await ReviewPost<CompanyWorkOrderBudgetDecisionRecovery>("/decision",independent)).Require(f.Gate.Company.Scope,reviewer,independent.Reference());Assert.Equal(2,f.Writes.Count);
            var publicStatus=await ReviewPost<CompanyWorkOrderBudgetDecisionStatus>("/decision/status",decisionQuery);publicStatus.Require();Assert.Equal(action==WorkOrderBudgetDecision.Approve,publicStatus.IsApprovedForCurrentScope);
            var discovered=await ReviewPost<CompanyWorkOrderFinancialSources>("/financial-sources",new CompanyWorkOrderFinancialQuery(order,"A"));discovered.Require();Assert.Single(discovered.Budgets);Assert.Equal(decisionQuery,discovered.Budgets[0].Budget.Query);Assert.Equal(publicStatus.Recorded,discovered.Budgets[0].Budget.Recorded);Assert.Equal(2,f.Writes.Count);
            Assert.DoesNotContain(independent.Rationale,JsonSerializer.Serialize(publicStatus));Assert.DoesNotContain(independent.IntentId.ToString(),JsonSerializer.Serialize(publicStatus));
            var header=f.Rows.OfType<WORK_ORDER>().Single(row=>row.WORK_ORDER_ID==order);var instructions=header.INSTRUCTIONS;header.INSTRUCTIONS="Changed current scope after independent decision";
            var changedStatus=await ReviewPost<CompanyWorkOrderBudgetDecisionStatus>("/decision/status",decisionQuery);changedStatus.Require();Assert.False(changedStatus.IsApprovedForCurrentScope);Assert.Equal(publicStatus.Recorded,changedStatus.Recorded);Assert.Contains("Original budget work scope changed.",changedStatus.Blockers);header.INSTRUCTIONS=instructions;
            if(laborCost)
            {
                f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Accounting.RecordWorkOrderActualCost"});await f.Db.SaveChangesAsync();
                f.Http.HttpContext=f.Request;f.Request.User=new(new System.Security.Claims.ClaimsIdentity([new("party_id",reviewer)],"fixture"));await f.Select("company",reviewer);
                var costProfile=Profile(f);await Save(costProfile);using var costBoundary=new Boundary(f,costProfile);costBoundary.Access.Setup(value=>value.CheckAssetAccessAsync(reviewer,"A","FIELD",It.IsAny<string?>(),null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
                var costTenant=new RepositoryTenantContext();costTenant.SelectCompany(f.Gate.Company);var costModules=new ModuleConnectionResolver(f.Db,f.Editor.Object,costTenant);var costOwners=MaintenanceReadOwners(f,costModules,costProfile);
                var core=await costModules.ResolveAsync("PPDM_CORE");var coreDatabase=new AssetDatabaseScope(core,await costModules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],core));
                // Already-recorded original start/labor command seam; actual API must read and validate native quantity/audit independently.
                var start=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Original(true,coreDatabase,"A").Command;var started=start.Prepare(DateTime.UtcNow);
                var comparison=CompanyWorkOrderSubmissionInspectionReview.Create(new(start,started),start.Review.Prerequisites.Requirements,DateTime.UtcNow);
                var laborReview=new CompanyWorkOrderLaborUsageReview(comparison,start.Review.Stage,start.Review.Prerequisites.Personnel,[],null,DateTime.UtcNow);
                var from=started.STARTED_AT_UTC;var to=laborReview.ComparedAtUtc;var report=new CompanyWorkOrderLaborUsageCommand(laborReview,Guid.NewGuid(),from,to,(decimal)(to.Ticks-from.Ticks)/TimeSpan.TicksPerSecond,WorkOrderLaborTimeUnit.Second,"Actual recorded work","Original executor report");var actualLabor=report.Prepare(DateTime.UtcNow);f.Rows.Add(actualLabor);
                foreach(var (table,type,key) in new[]{("WORK_ORDER_LABOR_USAGE",typeof(WORK_ORDER_LABOR_USAGE),"LABOR_ID"),("ACCOUNTING_COST",typeof(ACCOUNTING_COST),"ACCOUNTING_COST_ID")})
                {
                    f.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName==table),true)).Returns(()=>creator.ConvertToEntityStructure(type));
                    f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([key]);f.Metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=key});
                }
                var laborOwner=new WorkOrderExecutionPermitService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object);
                var gl=new Beep.OilandGas.Accounting.Services.GLAccountService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.Accounting.Services.GLAccountService>.Instance);
                var journals=new Beep.OilandGas.Accounting.Services.JournalEntryService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,gl,Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.Accounting.Services.JournalEntryService>.Instance);
                var inventoryOwner=new Beep.OilandGas.Accounting.Services.InventoryService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new Beep.OilandGas.Accounting.Services.AccountingBasisPostingService(journals),Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.Accounting.Services.InventoryService>.Instance,resolveConnection:()=>costModules.ResolveAsync("PRODUCTION"));
                var costService=new CompanyWorkOrderBudgetService(f.Gate,costModules,costProfile,costProfile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),costBoundary.Orchestrator.Object,costOwners.Order,books.Object,native,approvalOwner,laborOwner,inventoryOwner);
                await using var costHost=await HttpHost.Start(f,costProfile,costBoundary,true,typeof(CompanyWorkOrderBudgetController),services=>{services.AddSingleton<Func<string>>(()=>reviewer);services.AddSingleton(costService);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();services.AddAuthentication(options=>{options.DefaultAuthenticateScheme="receiving-fixture";options.DefaultChallengeScheme="receiving-fixture";}).AddScheme<AuthenticationSchemeOptions,ReceivingAuthentication>("receiving-fixture",_=>{});});
                async Task<T> CostPost<T>(string path,object body)
                {
                    using var response=await costHost.Http.PostAsJsonAsync(costHost.Url+"/api/field/current/workorder/"+Uri.EscapeDataString(order)+"/budget"+path,body);Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());return (await response.Content.ReadFromJsonAsync<T>())!;
                }
                var source=new CompanyWorkOrderLaborCostQuery(decisionQuery,actualLabor.LABOR_ID);var costReview=await CostPost<CompanyWorkOrderLaborCostReview>("/labor-cost/review",source);costReview.Require();Assert.Equal(actualLabor.QUANTITY,costReview.Labor.Quantity);Assert.Empty(costReview.Costs);
                var emptyCosts=await CostPost<CompanyWorkOrderActualCostReview>("/actual-costs",decisionQuery);emptyCosts.Require();Assert.Empty(emptyCosts.Costs);Assert.Equal(0m,emptyCosts.RecordedTotal());Assert.Equal(2,f.Writes.Count);
                var valuation=new CompanyWorkOrderLaborCostCommand(costReview,Guid.NewGuid(),1m,costReview.Labor.Unit,"KWD",costReview.ComparedAtUtc,"recorded-category","Explicit actual labor valuation");
                var actual=await CostPost<CompanyWorkOrderLaborCostRecovery>("/labor-cost",valuation);actual.Require(f.Gate.Company.Scope,reviewer,valuation.Reference());Assert.Equal(3,f.Writes.Count);Assert.Single(f.Rows.OfType<ACCOUNTING_COST>());Assert.Equal(valuation.Amount(),actual.Recorded.AMOUNT);
                var totals=await CostPost<CompanyWorkOrderActualCostReview>("/actual-costs",decisionQuery);totals.Require();Assert.Single(totals.Costs);Assert.Equal(valuation.Amount(),totals.RecordedTotal());
                Assert.Equal(totals.Budget.Original().BudgetAmount-valuation.Amount(),totals.RemainingOriginalBudget());Assert.Equal(actualLabor.LABOR_ID,totals.Costs[0].SourceId);
                Assert.DoesNotContain(valuation.Reason,JsonSerializer.Serialize(totals));Assert.DoesNotContain(valuation.IntentId.ToString(),JsonSerializer.Serialize(totals));Assert.Equal(3,f.Writes.Count);
                (await CostPost<CompanyWorkOrderLaborCostRecovery>("/labor-cost/recovery/reference",valuation.Reference())).Require(f.Gate.Company.Scope,reviewer,valuation.Reference());
                (await CostPost<CompanyWorkOrderLaborCostRecovery>("/labor-cost",valuation)).Require(f.Gate.Company.Scope,reviewer,valuation.Reference());Assert.Equal(3,f.Writes.Count);Assert.Equal(original,JsonSerializer.Serialize(f.Rows.OfType<AFE>().Single()));Assert.Null(f.Rows.OfType<AFE>().Single().ACTUAL_COST);
                if(materialCost)
                {
                    // Already-recorded canonical consumption seam; source owner must validate both physical rows.
                    var production=costReview.Budget.Current.BudgetDatabase;var accepted=new Beep.OilandGas.ApiService.Tests.WorkOrderMaterialAcceptanceFixture(requirementProof:true,inventoryDatabase:production,coreDatabase:coreDatabase,fieldId:"A");
                    var old=CompanyWorkOrderMaterialAllocationCommand.Read(accepted.Allocation).Review.Source;var executor=old.Context with{ActorUserId="executor",Revision="actual-consumption-authority"};
                    var stage=old.WorkOrder.Stage with{Context=old.WorkOrder.Stage.Context with{ActorUserId="executor"}};var work=old.WorkOrder with{Stage=stage,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,old.WorkOrder.InspectionDatabase,old.WorkOrder.Order,old.WorkOrder.Inspections)};
                    var physicalInventory=CompanyInventorySourceReview.Create(executor.Company,executor.ActorUserId,accepted.Inventory,[]);
                    var materialSource=old with{Context=executor,WorkOrder=work,Inventory=physicalInventory,Blockers=CompanyWorkOrderMaterialSourceReview.GetBlockers(old.Query,physicalInventory)};
                    var allocationReview=CompanyWorkOrderMaterialAllocationReview.Create(materialSource,[accepted.Allocation],DateTime.UtcNow);var startIntent=Guid.NewGuid();var permit=new CompanyWorkOrderExecutionPermitReference(accepted.Permit.Task,accepted.Permit.IntentId);
                    var recordedStart=new CompanyWorkOrderRecordedStart(executor.Company,permit,startIntent,new CompanyWorkOrderExecutionStartReference(permit,startIntent).StartId(executor.Company),accepted.Permit.PermitId(),"executor","employee","ConfiguredRole","execute",allocationReview.ComparedAtUtc,allocationReview.ComparedAtUtc,new string('A',64),new string('B',64));
                    var useReview=new CompanyWorkOrderMaterialUsageReview(recordedStart,CompanyWorkOrderMaterialReservationFacts.From(accepted.Allocation),allocationReview,accepted.Permit.ValidToUtc,DateTime.UtcNow);
                    var use=new CompanyWorkOrderMaterialUsageCommand(useReview,Guid.NewGuid(),1m,"EA",DateTime.UtcNow,"Original private physical consumption rationale");var consumption=use.Prepare(DateTime.UtcNow);var movement=use.PrepareMovement(consumption.ROW_CREATED_DATE!.Value);
                    var stock=CompanyInventorySourceReview.Copy(accepted.Inventory);stock.QUANTITY_ON_HAND=4m;f.Rows.AddRange([consumption,movement,stock]);
                    foreach(var (table,type,key) in new[]{("WORK_ORDER_MATERIAL_USAGE",typeof(WORK_ORDER_MATERIAL_USAGE),"USAGE_ID"),("INVENTORY_ITEM",typeof(Beep.OilandGas.Models.Data.ProductionAccounting.INVENTORY_ITEM),"INVENTORY_ITEM_ID"),("INVENTORY_TRANSACTION",typeof(Beep.OilandGas.Models.Data.ProductionAccounting.INVENTORY_TRANSACTION),"INVENTORY_TRANSACTION_ID")})
                    {
                        f.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName==table),true)).Returns(()=>creator.ConvertToEntityStructure(type));
                        f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([key]);f.Metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=key,Module="PRODUCTION"});
                    }
                    var stockBefore=JsonSerializer.Serialize(stock);var movementBefore=JsonSerializer.Serialize(movement);var materialQuery=new CompanyWorkOrderMaterialCostQuery(decisionQuery,consumption.USAGE_ID);
                    var materialReview=await CostPost<CompanyWorkOrderMaterialCostReview>("/material-cost/review",materialQuery);materialReview.Require();Assert.Equal(1m,materialReview.Material.Quantity);Assert.Single(materialReview.Costs);
                    Assert.DoesNotContain(use.Reason,JsonSerializer.Serialize(materialReview));Assert.DoesNotContain(use.IntentId.ToString(),JsonSerializer.Serialize(materialReview));
                    var materialCommand=new CompanyWorkOrderMaterialCostCommand(materialReview,Guid.NewGuid(),12.345m,"EA","KWD",materialReview.ComparedAtUtc,"Recorded-material","Explicit physical source valuation");
                    var materialResult=await CostPost<CompanyWorkOrderMaterialCostRecovery>("/material-cost",materialCommand);materialResult.Require(f.Gate.Company.Scope,reviewer,materialCommand.Reference());Assert.Equal(4,f.Writes.Count);Assert.Equal(2,f.Rows.OfType<ACCOUNTING_COST>().Count());
                    (await CostPost<CompanyWorkOrderMaterialCostRecovery>("/material-cost/recovery/reference",materialCommand.Reference())).Require(f.Gate.Company.Scope,reviewer,materialCommand.Reference());
                    (await CostPost<CompanyWorkOrderMaterialCostRecovery>("/material-cost",materialCommand)).Require(f.Gate.Company.Scope,reviewer,materialCommand.Reference());Assert.Equal(4,f.Writes.Count);
                    var all=await CostPost<CompanyWorkOrderActualCostReview>("/actual-costs",decisionQuery);all.Require();Assert.Equal(2,all.Costs.Count);Assert.Equal(valuation.Amount()+12.345m,all.RecordedTotal());Assert.Contains(all.Costs,row=>row.SourceKind=="WORK_ORDER_MATERIAL_USAGE" && row.SourceId==consumption.USAGE_ID);
                    var sources=await CostPost<CompanyWorkOrderFinancialSources>("/financial-sources",new CompanyWorkOrderFinancialQuery(order,"A"));sources.Require();Assert.Equal(all.RecordedTotal(),Assert.Single(sources.Budgets).RecordedTotal());Assert.Equal(4,f.Writes.Count);Assert.DoesNotContain(materialCommand.Reason,JsonSerializer.Serialize(sources));
                    Assert.Equal(stockBefore,JsonSerializer.Serialize(stock));Assert.Equal(movementBefore,JsonSerializer.Serialize(movement));Assert.Null(movement.UNIT_COST);Assert.Null(movement.TOTAL_COST);Assert.Equal(original,JsonSerializer.Serialize(f.Rows.OfType<AFE>().Single()));
                }
                return;
            }
            f.Db.RoleClaims.Remove(reviewPermission);await f.Db.SaveChangesAsync();using(var revokedDecision=await reviewerHost.Http.PostAsJsonAsync(reviewerHost.Url+"/api/field/current/workorder/"+order+"/budget/decision/recovery/reference",independent.Reference()))Assert.Equal(HttpStatusCode.Forbidden,revokedDecision.StatusCode);
            f.Http.HttpContext=f.Request;f.Request.User=new(new System.Security.Claims.ClaimsIdentity([new("party_id",reviewer)],"fixture"));await f.Select("company",reviewer);
            await f.Grant("FIELD","A","READ");await f.Grant("WORK_ORDER",order,"READ");
            f.Db.RoleClaims.Remove(prepare);await f.Db.SaveChangesAsync();await f.Select("company",reviewer);
            var viewerProfile=Profile(f);await Save(viewerProfile);using var viewerBoundary=new Boundary(f,viewerProfile);
            viewerBoundary.Access.Setup(value=>value.CheckAssetAccessAsync(reviewer,"A","FIELD",It.IsAny<string?>(),null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
            var viewerTenant=new RepositoryTenantContext();viewerTenant.SelectCompany(f.Gate.Company);var viewerModules=new ModuleConnectionResolver(f.Db,f.Editor.Object,viewerTenant);var viewerOwners=MaintenanceReadOwners(f,viewerModules,viewerProfile);
            var viewerService=new CompanyWorkOrderBudgetService(f.Gate,viewerModules,viewerProfile,viewerProfile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),viewerBoundary.Orchestrator.Object,viewerOwners.Order,books.Object,native,approvalOwner);
            await using var viewerHost=await HttpHost.Start(f,viewerProfile,viewerBoundary,true,typeof(CompanyWorkOrderBudgetController),services=>{services.AddSingleton<Func<string>>(()=>reviewer);services.AddSingleton(viewerService);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();services.AddAuthentication(options=>{options.DefaultAuthenticateScheme="receiving-fixture";options.DefaultChallengeScheme="receiving-fixture";}).AddScheme<AuthenticationSchemeOptions,ReceivingAuthentication>("receiving-fixture",_=>{});});
            using var viewerResponse=await viewerHost.Http.PostAsJsonAsync(viewerHost.Url+"/api/field/current/workorder/"+order+"/budget/decision/status",decisionQuery);Assert.True(viewerResponse.StatusCode==HttpStatusCode.OK,await viewerResponse.Content.ReadAsStringAsync());
            var viewOnly=(await viewerResponse.Content.ReadFromJsonAsync<CompanyWorkOrderBudgetDecisionStatus>())!;viewOnly.Require();Assert.Equal(publicStatus.Recorded,viewOnly.Recorded);
            using var viewerCosts=await viewerHost.Http.PostAsJsonAsync(viewerHost.Url+"/api/field/current/workorder/"+order+"/budget/actual-costs",decisionQuery);
            Assert.True(viewerCosts.StatusCode==HttpStatusCode.OK,await viewerCosts.Content.ReadAsStringAsync());Assert.Equal("no-store",viewerCosts.Headers.CacheControl?.ToString());
            var viewerTotal=(await viewerCosts.Content.ReadFromJsonAsync<CompanyWorkOrderActualCostReview>())!;viewerTotal.Require();Assert.Empty(viewerTotal.Costs);Assert.Equal(0m,viewerTotal.RecordedTotal());Assert.Equal(publicStatus.Recorded,viewerTotal.Budget.Recorded);
            using var viewerDiscovery=await viewerHost.Http.PostAsJsonAsync(viewerHost.Url+"/api/field/current/workorder/"+order+"/budget/financial-sources",new CompanyWorkOrderFinancialQuery(order,"A"));
            Assert.True(viewerDiscovery.StatusCode==HttpStatusCode.OK,await viewerDiscovery.Content.ReadAsStringAsync());Assert.Equal("no-store",viewerDiscovery.Headers.CacheControl?.ToString());
            var viewerSources=(await viewerDiscovery.Content.ReadFromJsonAsync<CompanyWorkOrderFinancialSources>())!;viewerSources.Require();Assert.Single(viewerSources.Budgets);Assert.Equal(publicStatus.Recorded,viewerSources.Budgets[0].Budget.Recorded);Assert.Equal(2,f.Writes.Count);
            Assert.Equal(action==WorkOrderBudgetDecision.Approve,viewOnly.IsApprovedForCurrentScope);Assert.Equal(2,f.Writes.Count);
            Assert.Equal(2,f.Writes.Count);Assert.Equal(original,JsonSerializer.Serialize(f.Rows.OfType<AFE>().Single()));return;
        }
        f.Db.RoleClaims.Remove(prepare);await f.Db.SaveChangesAsync();var before=Reads();using(var revoked=await host.Http.PostAsJsonAsync(prefix+"/recovery/reference",reference))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);
        Assert.Equal(before,Reads());Assert.Single(f.Writes);Assert.Equal(original,JsonSerializer.Serialize(f.Rows.OfType<AFE>().Single()));
    }
}
