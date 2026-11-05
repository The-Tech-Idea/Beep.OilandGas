#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentReviewCatalogTests
{
    internal sealed class Fixture : IDisposable
    {
        internal DevelopmentDraftAuthorityTests.Fixture Draft=new();
        internal CompanyProcessStartTests.Fixture Workflow=new("ConfiguredReviewRole");
        internal DevelopmentReviewCatalogService Service;
        internal DevelopmentPlanSourceReview Source=null!;
        internal ModuleDatabaseBinding Binding;
        internal Action? OnDestinations;
        internal int PersonaReads;
        internal Repository.RepositoryTenantContext Tenant=null!;
        internal ModuleConnectionResolver Modules=null!;
        internal CompanyModuleConnection Company=null!;
        internal Fixture(bool start=false)
        {
            Draft.Permissions=start?["DevelopmentPlanning.View","DevelopmentPlanning.Create","Workflow.View","Workflow.Start"]:["DevelopmentPlanning.View","DevelopmentPlanning.Create","Workflow.View"];
            var configured=new ConnectionProperties{ConnectionName="lifecycle",DatabaseType=DataSourceType.SqlLite,ConnectionString="Data Source="+Path.Combine(Path.GetTempPath(),"fdp-review-catalog-fixture.db")};
            var connections=Draft.Drafts.Native.Editor.Object.ConfigEditor.DataConnections;connections.Add(configured);
            Workflow.Native.Source.SetupGet(source=>source.DatasourceName).Returns("lifecycle");
            Mock.Get(Workflow.Native.Source.Object.Dataconnection).SetupGet(value=>value.ConnectionProp).Returns(configured);
            Draft.Drafts.Native.Editor.Setup(editor=>editor.GetDataSource("lifecycle")).Returns(Workflow.Native.Source.Object);
            Draft.Db.Add(new AppModuleConnection{TenantId="tenant",ConnectionKey="LIFECYCLE",AssignedByUserId="administrator",PhysicalTargetKey=ModuleDatabaseTarget.Key(Draft.Drafts.Native.Editor.Object,configured)});
            Binding=new(){TenantId="tenant",ModuleId="LIFECYCLE",ConnectionName="lifecycle",ConnectionKey="LIFECYCLE",ConcurrencyStamp="original"};Draft.Db.Add(Binding);Draft.Db.SaveChanges();
            Workflow.Definition.PROCESS_TYPE="DEVELOPMENT";Workflow.Definition.ENTITY_TYPE="FIELD_DEVELOPMENT_PLAN";
            Workflow.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new ProcessStepDefinition{StepId="technical",StepName="Independent technical review",SequenceNumber=1,StepType="APPROVAL",RequiredRoles=["ConfiguredReviewRole"],RequiresApproval=true}}});
            Workflow.Native.Personas.Setup(reader=>reader.GetActivePersonasAsync("ConfiguredReviewRole")).ReturnsAsync(()=>{PersonaReads++;OnDestinations?.Invoke();return new List<string>{"RESERVOIR_ENGINEER"};});
            var resolver=new Repository.RepositoryCompanyContextResolver(Draft.Accounts.Object);var tenant=new Repository.RepositoryTenantContext();
            tenant.SelectCompany(resolver.ResolveAsync("planner",Draft.Context.Company).GetAwaiter().GetResult()!);
            var modules=new ModuleConnectionResolver(Draft.Db,Draft.Drafts.Native.Editor.Object,tenant);var company=new CompanyModuleConnection(modules,tenant,resolver);
            Tenant=tenant;Modules=modules;Company=company;
            Service=new(Draft.Authority,Draft.Drafts.Service,company,modules,new BoundProcessService(()=>Task.FromResult("lifecycle"),_=>Workflow.Service),Workflow.Native.Personas.Object,Draft.Drafts.Native.Editor.Object);
        }
        internal async Task UseCompanyRecipients()
        {
            Draft.Db.Users.Add(new(){Id="administrator",IsActive=true});
            Draft.Db.Add(new AppCompany{TenantId="tenant",BusinessAssociateId="company"});
            Draft.Db.Add(new AppCompany{TenantId="tenant",BusinessAssociateId="foreign"});
            Draft.Db.Add(new AppPersona{Code="RESERVOIR_ENGINEER",Name="Reservoir engineer"});
            Draft.Db.Add(new AppPersona{Code="FOREIGN_REVIEWER",Name="Foreign reviewer"});
            Draft.Db.Roles.Add(new(){Id="review-role",Name="ConfiguredReviewRole",NormalizedName="CONFIGUREDREVIEWROLE"});
            foreach(var actor in new[]{"planner","reviewer","foreign-reviewer"})
            {
                Draft.Db.Users.Add(new(){Id=actor,IsActive=true});
                var company=actor=="foreign-reviewer"?"foreign":"company";
                Draft.Db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId=company,UserId=actor,GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
                if(actor=="planner")continue;
                Draft.Db.Add(new AppUserPersona{UserId=actor,TenantId="tenant",BusinessAssociateId=company,PersonaCode=actor=="reviewer"?"RESERVOIR_ENGINEER":"FOREIGN_REVIEWER",ChangedBy="administrator",ChangedUtc=DateTime.UtcNow});
                Draft.Db.UserRoles.Add(new(){UserId=actor,RoleId="review-role"});
                Draft.Db.Add(new AppUserRoleExtension{UserId=actor,RoleId="review-role",TenantId="tenant",BusinessAssociateId=company,ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
            }
            await Draft.Db.SaveChangesAsync();
            var reader=new RepositoryRolePersonaReader(new Repository.RepositoryAccessService(Draft.Db),Tenant);
            Service=new(Draft.Authority,Draft.Drafts.Service,Company,Modules,new BoundProcessService(()=>Task.FromResult("lifecycle"),_=>Workflow.Service),reader,Draft.Drafts.Native.Editor.Object);
        }
        internal async Task Prepare()
        {
            var saved=await Draft.Drafts.Save();Source=await Draft.Drafts.Service.GetDevelopmentPlanAsync(Draft.Context,Draft.Planning,saved.FDP_ID,Draft.Drafts.Authority);
        }
        internal Task<DevelopmentReviewCatalog> Read()=>Service.ReadAsync(Draft.User,Source.PlanId,Source.Revision);
        public void Dispose()=>Draft.Dispose();
    }
    [Fact] public async Task ActualCatalogBindsExactDraftSeparateLifecycleAndConfiguredRolePersonaTypesWithoutBusinessWrites()
    {
        using var f=new Fixture();await f.Prepare();var before=f.Draft.Drafts.Native.Writes;var catalog=await f.Read();catalog.Require(f.Draft.Context,f.Source.PlanId,f.Source.Revision);
        var choice=Assert.Single(catalog.Choices);Assert.Equal("Recorded invoice review",choice.ProcessName);Assert.Equal("1.1.0",choice.Version);
        Assert.Equal("ConfiguredReviewRole",Assert.Single(choice.Steps).AssignedRole);Assert.Equal(new[]{"RESERVOIR_ENGINEER"},choice.Steps[0].PersonaCodes);
        Assert.Equal("lifecycle",catalog.LifecycleDatabase.ConnectionName);Assert.Equal("planning",catalog.Source.PlanningDatabase.ConnectionName);
        Assert.Equal(before,f.Draft.Drafts.Native.Writes);Assert.Equal(0,f.Workflow.Native.Writes);Assert.Empty(f.Workflow.Native.Transactions);
        Assert.DoesNotContain(f.Workflow.Native.Source.Invocations,call=>call.Method.Name is "InsertEntity" or "UpdateEntity" or "DeleteEntity");
    }
    [Theory][InlineData("empty")][InlineData("field-only")][InlineData("permission")][InlineData("inactive")][InlineData("no-version")][InlineData("wrong-step")][InlineData("unknown-role")][InlineData("binding")][InlineData("changed-definition")][InlineData("changed-source")]
    public async Task UnqualifiedOrChangedCatalogCannotSupplySuccessfulAssignments(string fault)
    {
        using var f=new Fixture();await f.Prepare();switch(fault)
        {
            case "empty":f.Workflow.Native.Rows.Clear();break;case "field-only":f.Workflow.Definition.ENTITY_TYPE="FIELD";break;case "permission":f.Draft.Permissions=["DevelopmentPlanning.View","DevelopmentPlanning.Create"];break;
            case "inactive":f.Workflow.Definition.ACTIVE_IND="N";break;case "no-version":f.Workflow.Definition.VERSION="";break;
            case "wrong-step":f.Workflow.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new ProcessStepDefinition{StepId="approve",StepName="Approval",StepType="ACTION",RequiredRoles=["ConfiguredReviewRole"]}}});break;
            case "unknown-role":f.Workflow.Native.Personas.Setup(reader=>reader.GetActivePersonasAsync("ConfiguredReviewRole")).ReturnsAsync(new List<string>());break;
            case "binding":f.OnDestinations=()=>{f.Binding.ConcurrencyStamp="changed";f.Draft.Db.SaveChanges();};break;
            case "changed-definition":f.OnDestinations=()=>{if(f.PersonaReads==1)f.Workflow.Definition.VERSION="new-version";};break;
            case "changed-source":f.OnDestinations=()=>{if(f.PersonaReads==1)f.Draft.Drafts.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single().FDP_NAME="changed";};break;
        }
        if(fault is "empty" or "field-only" or "inactive")Assert.Empty((await f.Read()).Choices);else await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());Assert.Equal(0,f.Workflow.Native.Writes);
    }
    [Fact] public async Task ActualTypedClientCatalogHttpKeepsExactDraftAndConfiguredRoles()
    {
        using var f=new Fixture();await f.Prepare();await using var host=await DevelopmentDraftHttpTests.Start(f.Draft,catalog:f.Service);
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();company.TrySelect(company.Clear(),"planner",new(f.Draft.Context.Company,["ConfiguredPlanner"],f.Draft.Permissions));
        var client=new Beep.OilandGas.Web.Services.DevelopmentServiceClient(new(host.Http,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company),Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.Web.Services.DevelopmentServiceClient>.Instance);
        var catalog=await client.GetDraftReviewWorkflowsAsync(f.Source.PlanId,f.Source.Revision);catalog.Require(f.Draft.Context,f.Source.PlanId,f.Source.Revision);
        Assert.Equal("ConfiguredReviewRole",Assert.Single(Assert.Single(catalog.Choices).Steps).AssignedRole);Assert.Equal(0,f.Workflow.Native.Writes);
        f.Draft.Permissions=["DevelopmentPlanning.View","DevelopmentPlanning.Create"];
        var denied=await Assert.ThrowsAsync<Beep.OilandGas.Web.Services.OilGasApiException>(()=>client.GetDraftReviewWorkflowsAsync(f.Source.PlanId,f.Source.Revision));Assert.Equal(System.Net.HttpStatusCode.Forbidden,denied.StatusCode);
    }
    [Fact] public async Task GenericFieldOnlyStartCannotTurnCatalogMetadataIntoAnUnreviewedDevelopmentHandoff()
    {
        using var f=new Fixture();await f.Prepare();await f.Read();
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Workflow.Service.StartProcessAsync("definition",f.Source.PlanId,"FIELD_DEVELOPMENT_PLAN","field","planner"));
        Assert.Equal(0,f.Workflow.Native.Writes);Assert.Empty(f.Workflow.Native.Transactions);
    }
    [Fact] public async Task CompanyCatalogPreservesMultipleTechnicalInvestmentAndRegulatoryReviewRoles()
    {
        using var f=new Fixture();await f.Prepare();var roles=new[]{"TechnicalReviewer","InvestmentReviewer","RegulatoryReviewer"};
        foreach(var role in roles)f.Workflow.Native.Personas.Setup(reader=>reader.GetActivePersonasAsync(role)).ReturnsAsync(new List<string>{"RESERVOIR_ENGINEER"});
        f.Workflow.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=roles.Select((role,index)=>new ProcessStepDefinition{StepId="stage-"+index,StepName=role,
            SequenceNumber=index,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=[role]}).ToArray()});
        var choice=Assert.Single((await f.Read()).Choices);Assert.Equal(roles,choice.Steps.Select(step=>step.AssignedRole));Assert.Equal(0,f.Workflow.Native.Writes);
    }
    [Fact] public async Task ActualCatalogResolvesCompanyRecipientsThroughCanonicalAppRbacAndRefusesGlobalOnlyDestinations()
    {
        using var f=new Fixture();await f.Prepare();await f.UseCompanyRecipients();
        Assert.Equal(new[]{"RESERVOIR_ENGINEER"},Assert.Single(Assert.Single((await f.Read()).Choices).Steps).PersonaCodes);
        (await f.Draft.Db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="reviewer")).ApprovalStatus="Revoked";
        await f.Draft.Db.SaveChangesAsync();await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Read());Assert.Equal(0,f.Workflow.Native.Writes);
    }
}
#endif
