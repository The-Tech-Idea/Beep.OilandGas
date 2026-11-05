using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
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
    private async Task CompletePermitApprovalHttp(CompanyPPDMRowAccessTests.Fixture f,HttpHost reviewerHost,RepositoryUserProfileService reviewerProfile,Boundary reviewerBoundary,ModuleConnectionResolver reviewerModules,IContractorManagementService personnelOwner,Beep.OilandGas.Accounting.Services.InventoryService inventory,PPDMProcessService process,Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder.WorkOrderExecutionPermitService permitOwner,CompanyWorkOrderExecutionPermitIntent intent,CompanyWorkOrderExecutionPermitReference query,int expectedWrites,bool executionPrerequisites=false,bool executionStart=false,bool executionSubmission=false,bool executionMaterialUsage=false,bool executionLaborUsage=false)
    {
        var spans=new System.Collections.Concurrent.ConcurrentQueue<string>();var active=new System.Collections.Concurrent.ConcurrentDictionary<string,(string Name,DateTime At)>();
        using var listener=new System.Diagnostics.ActivityListener
        {
            ShouldListenTo=source=>source.Name=="Beep.OilandGas.WorkOrderPermitDecision",
            Sample=(ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> _)=>System.Diagnostics.ActivitySamplingResult.AllData,
            ActivityStarted=activity=>active[activity.Id!]=(activity.OperationName,DateTime.UtcNow),
            ActivityStopped=activity=>{active.TryRemove(activity.Id!,out _);spans.Enqueue($"{activity.OperationName}:{activity.Duration.TotalSeconds:F1}s");}
        };
        System.Diagnostics.ActivitySource.AddActivityListener(listener);
        var prefix="/api/field/current/maintenance-handoffs/execution-permit";
        var timingLog=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"oilgas-complete-approval-http-"+intent.IntentId.ToString("N")+".log");
        async Task<T> Post<T>(HttpHost host,string path,object body)
        {
            var elapsed=System.Diagnostics.Stopwatch.StartNew();HttpResponseMessage message;
            try{message=await host.Http.PostAsJsonAsync(host.Url+path,body);}
            catch(Exception error) when(error is HttpRequestException or TaskCanceledException)
            {
                var reads=f.Source.Invocations.Where(call=>call.Method.Name=="GetEntityAsync").GroupBy(call=>call.Arguments[0] as string).OrderByDescending(group=>group.Count()).Take(8).Select(group=>$"{group.Key}:{group.Count()}");
                var running=active.Values.Select(value=>$"{value.Name}:{(DateTime.UtcNow-value.At).TotalSeconds:F1}s active");
                throw new InvalidOperationException($"Protected request {path} failed after {elapsed.Elapsed.TotalSeconds:F1}s; cumulative native read counts {string.Join(",",reads)}; proof timing {string.Join(",",spans.TakeLast(48).Concat(running))}.",error);
            }
            using var response=message;
            // Endpoint/status/timing only: never retain private commands, identities or response bodies.
            System.IO.File.AppendAllText(timingLog,$"{DateTime.UtcNow:O} {path} {(int)response.StatusCode} {elapsed.Elapsed.TotalSeconds:F1}s{Environment.NewLine}");
            Assert.True(response.StatusCode==HttpStatusCode.OK,$"{path} ({(int)response.StatusCode}): {await response.Content.ReadAsStringAsync()}");return (await response.Content.ReadFromJsonAsync<T>())!;
        }
        var riskComponent=f.Rows.OfType<WORK_ORDER_COMPONENT>().Single(row=>row.COMPONENT_ID=="risk-component");
        var info=CompanyInventorySourceReview.Copy(f.Rows.OfType<RM_INFORMATION_ITEM>().Single());info.INFORMATION_ITEM_ID=riskComponent.INFORMATION_ITEM_ID;info.SOURCE_DOCUMENT_ID=intent.RiskReference;f.Rows.Add(info);
        var document=CompanyInventorySourceReview.Copy(f.Rows.OfType<SOURCE_DOCUMENT>().Single());document.SOURCE_DOCUMENT_ID=intent.RiskReference;f.Rows.Add(document);
        var file=CompanyInventorySourceReview.Copy(f.Rows.OfType<RM_FILE_CONTENT>().Single());file.FILE_ID="risk-file";file.INFORMATION_ITEM_ID=riskComponent.INFORMATION_ITEM_ID;f.Rows.Add(file);
        var creator=CompanyInventorySourceReview.Copy(f.Rows.OfType<RM_CREATOR>().Single());creator.CREATOR_ID="risk-creator";creator.INFORMATION_ITEM_ID=riskComponent.INFORMATION_ITEM_ID;f.Rows.Add(creator);
        foreach(var permission in new[]{"Maintenance.AssignPersonnel","HSE.ConfigurePersonnelPolicy"})
            if(!f.Db.RoleClaims.Any(row=>row.ClaimType=="permission" && row.ClaimValue==permission))f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=permission});
        await f.Db.SaveChangesAsync();
        if(executionStart)
        {
            const string actor="execution-technician";const string employee="employee-الكويت";
            f.Db.Users.Add(new OilGasUser{Id=actor,UserName=actor,IsActive=true,ConcurrencyStamp="executor-version"});f.Db.UserRoles.Add(new IdentityUserRole<string>{UserId=actor,RoleId="admin"});
            // Register the target in the authoritative selected-company tables before an
            // administrator reviews its native employee link; a global role is insufficient.
            var mappingCompany=intent.Context.Company;
            f.Db.Add(new AppCompanyMembership{TenantId=mappingCompany.TenantId,BusinessAssociateId=mappingCompany.BusinessAssociateId,UserId=actor,GrantedByUserId="permit-reviewer"});
            f.Db.Add(new AppUserRoleExtension{TenantId=mappingCompany.TenantId,BusinessAssociateId=mappingCompany.BusinessAssociateId,UserId=actor,RoleId="admin"});
            if(!f.Db.RoleClaims.Any(row=>row.ClaimValue=="HSE.ConfigurePersonnelIdentity"))f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="HSE.ConfigurePersonnelIdentity"});await f.Db.SaveChangesAsync();
            var dates=f.Rows.OfType<BA_EMPLOYEE>().Single(row=>row.EMPLOYEE_BA_ID==employee);var type="Reported person type";
            f.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID=employee,BA_LONG_NAME="Recorded assigned execution technician",BA_TYPE=type,ACTIVE_IND="Y",EFFECTIVE_DATE=dates.EFFECTIVE_DATE,EXPIRY_DATE=dates.EXPIRY_DATE});
            var identityService=new CompanyPersonnelIdentityService(f.Gate,reviewerModules,personnelOwner,f.Db);
            await using var identityHost=await HttpHost.Start(f,reviewerProfile,reviewerBoundary,true,typeof(CompanyPersonnelIdentityController),services=>
            {
                services.AddSingleton<Func<string>>(()=>"permit-reviewer");services.AddSingleton(identityService);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();
                services.AddAuthentication(options=>{options.DefaultAuthenticateScheme="receiving-fixture";options.DefaultChallengeScheme="receiving-fixture";}).AddScheme<AuthenticationSchemeOptions,ReceivingAuthentication>("receiving-fixture",_=>{});
            });
            var identity=await Post<CompanyPersonnelIdentityReview>(identityHost,"/api/company/current/personnel-identities/review",new CompanyPersonnelIdentityQuery(actor,employee));
            var mapping=new CompanyPersonnelIdentityCommand(identity,1,Guid.NewGuid(),DateTime.UtcNow,intent.ValidToUtc.AddHours(1),"Verified company employee/account source","Independent execution employee mapping before acceptance",true);
            // Cover the complete permit window explicitly; retain the actual native person/employment proof.
            mapping=mapping with{EffectiveFromUtc=intent.ValidFromUtc};
            mapping.RequireRecorded(await Post<COMPANY_PERSONNEL_IDENTITY>(identityHost,"/api/company/current/personnel-identities",mapping));expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);
        }

        var materialRow=f.Rows.OfType<WORK_ORDER_INSPECTION>().Single(row=>row.PERMIT_PREREQUISITE_KIND=="MATERIAL" && row.REGULATORY_REF==intent.MaterialReference);
        var allocation=f.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>().Single();var allocationCommand=CompanyWorkOrderMaterialAllocationCommand.Read(allocation);
        var material=await Post<CompanyWorkOrderMaterialReadinessReview>(reviewerHost,prefix+"/material-readiness",new CompanyWorkOrderMaterialReadinessQuery(query,allocationCommand.IntentId));
        var materialCommand=new CompanyWorkOrderMaterialAcceptanceCommand(material,materialRow.INSPECTION_ID,Guid.NewGuid(),"Independent complete original material verification");
        materialCommand.RequireRecorded(await Post<WORK_ORDER_INSPECTION>(reviewerHost,prefix+"/material-acceptance",materialCommand));expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);

        var personQuery=new CompanyWorkOrderPersonnelSourceQuery(query,"employee-الكويت",intent.PersonnelReference);
        var person=await Post<CompanyWorkOrderPersonnelSourceReview>(reviewerHost,prefix+"/personnel-source",personQuery);
        var assignment=new CompanyWorkOrderPersonnelAssignmentCommand(person,Guid.NewGuid(),"RecordedDuty",intent.ValidFromUtc,intent.ValidToUtc,"Original company duty assignment");
        var assigned=await Post<WORK_ORDER_PERSONNEL_ASSIGNMENT>(reviewerHost,prefix+"/personnel-assignments",assignment);assignment.RequireRecorded(assigned);expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);

        var policyService=new CompanyPersonnelPolicyService(f.Gate,reviewerModules,personnelOwner);
        await using(var policyHost=await HttpHost.Start(f,reviewerProfile,reviewerBoundary,true,typeof(CompanyPersonnelPoliciesController),services=>
        {
            services.AddSingleton<Func<string>>(()=>"permit-reviewer");services.AddSingleton(policyService);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();
            services.AddAuthentication(options=>{options.DefaultAuthenticateScheme="receiving-fixture";options.DefaultChallengeScheme="receiving-fixture";}).AddScheme<AuthenticationSchemeOptions,ReceivingAuthentication>("receiving-fixture",_=>{});
        }))
        {
            using var response=await policyHost.Http.GetAsync(policyHost.Url+"/api/company/personnel-policies/review");Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());var catalog=(await response.Content.ReadFromJsonAsync<CompanyPersonnelPolicyCatalog>())!;
            var utc=DateTime.UtcNow;var draft=new CompanyPersonnelPolicyDraft("RecordedDuty",person.PermitReview.CurrentSource.Order.WoSubType,intent.Jurisdiction,"Reported licence type","recorded-authority","CERTIFICATION",["VALID"],[new("TRAINING_HOURS","TRAINING","MINIMUM",8m,"HOUR","Original policy training reference")],utc.AddDays(-1),utc.AddDays(2),"Original company personnel policy"){AcceptedEmploymentStatuses=["ACTIVE"],AcceptedEmploymentPositions=[]};
            var command=new CompanyPersonnelPolicyCommand(catalog.Context,catalog.Revision,draft,1,Guid.NewGuid(),"Independent explicit company policy configuration");command.RequireRecorded(await Post<WORK_ORDER_PERSONNEL_POLICY>(policyHost,"/api/company/personnel-policies/record",command));expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);
        }
        foreach(var kind in new[]{"ISOLATION","RISK"})
        {
            var documentQuery=new CompanyWorkOrderPermitDocumentQuery(query,kind,kind=="ISOLATION"?"document-component":"risk-component");
            var contributors=await Post<CompanyWorkOrderDocumentContributorReview>(reviewerHost,prefix+"/documents/contributors",documentQuery);Assert.Single(contributors.Blockers);
            var content=await Post<CompanyWorkOrderPermitDocumentContent>(reviewerHost,prefix+"/documents/content",contributors.Document);content.Require(contributors.Document);Assert.Equal(new byte[]{1,2,3},content.Bytes);
            var pending=f.Rows.OfType<WORK_ORDER_INSPECTION>().Single(row=>row.PERMIT_PREREQUISITE_KIND==kind && row.REGULATORY_REF==(kind=="ISOLATION"?intent.IsolationReference:intent.RiskReference));
            var acceptance=new CompanyWorkOrderDocumentAcceptanceCommand(contributors.Document,pending.INSPECTION_ID,Guid.NewGuid(),"Independent original "+kind+" content and requirement verification",true,true){ContributorReview=contributors};
            acceptance.RequireRecorded(await Post<WORK_ORDER_INSPECTION>(reviewerHost,prefix+"/document-acceptance",acceptance));expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);
        }

        f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(row=>row.ClaimValue=="Maintenance.AssignPersonnel"));f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(row=>row.ClaimValue=="HSE.ConfigurePersonnelPolicy"));
        f.Db.Users.Add(new OilGasUser{Id="personnel-reviewer",UserName="personnel-reviewer",IsActive=true,ConcurrencyStamp="personnel-reviewer-version"});f.Db.UserRoles.Add(new IdentityUserRole<string>{UserId="personnel-reviewer",RoleId="admin"});await f.Db.SaveChangesAsync();
        f.Request.User=new(new ClaimsIdentity([new("party_id","personnel-reviewer")],"fixture"));await f.Select("company","personnel-reviewer");await f.Grant("FIELD","A","WRITE");await f.Grant("WORK_ORDER",query.Task.WorkOrderId,"WRITE");var profile=Profile(f);await Save(profile);
        using var boundary=new Boundary(f,profile);boundary.Access.Setup(value=>value.CheckAssetAccessAsync("personnel-reviewer","A","FIELD",It.IsAny<string?>(),null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var owners=MaintenanceReadOwners(f,modules,profile);
        var production=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),profile,f.Policy);
        var nativePersonnel=new Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder.ContractorManagementService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,async()=>{var access=await f.Gate.RefreshCompanyAsync();var connection=await modules.ResolveAsync("PPDM_CORE");return(access.Scope,access.UserId,new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],connection)));},Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder.ContractorManagementService>.Instance);
        var service=new CompanyMaintenanceHandoffService(f.Gate,modules,profile,profile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),new BoundProcessService(()=>modules.ResolveAsync("LIFECYCLE"),_=>process),production,boundary.Orchestrator.Object,owners.Order,owners.Inspections,permitOwner,inventory,nativePersonnel);
        await using var finalHost=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyMaintenanceHandoffController),services=>
        {
            services.AddSingleton<Func<string>>(()=>"personnel-reviewer");services.AddSingleton(service);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();
            services.AddAuthentication(options=>{options.DefaultAuthenticateScheme="receiving-fixture";options.DefaultChallengeScheme="receiving-fixture";}).AddScheme<AuthenticationSchemeOptions,ReceivingAuthentication>("receiving-fixture",_=>{});
        });
        var personnelQuery=new CompanyWorkOrderAssignedPersonnelQuery(query,assigned.ASSIGNMENT_ID);var qualifications=await Post<CompanyPersonnelPolicyEvidenceReview>(finalHost,prefix+"/personnel-assignments/policy-evidence",personnelQuery);Assert.Single(qualifications.Blockers);
        var personnelRow=f.Rows.OfType<WORK_ORDER_INSPECTION>().Single(row=>row.PERMIT_PREREQUISITE_KIND=="PERSONNEL" && row.REGULATORY_REF==intent.PersonnelReference);
        var personnelAcceptance=new CompanyWorkOrderPersonnelAcceptanceCommand(qualifications,personnelRow.INSPECTION_ID,Guid.NewGuid(),"Independent original company duty and qualification verification"){RequirementReferenceVersion=1};
        personnelAcceptance.RequireRecorded(await Post<WORK_ORDER_INSPECTION>(finalHost,prefix+"/personnel-acceptance",personnelAcceptance));expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);
        var ready=await Post<CompanyWorkOrderExecutionPermitApprovalReview>(finalHost,prefix+"/approval/review",query);ready.RequireCurrentPrerequisiteAcceptance(DateTime.UtcNow);Assert.Empty(ready.Blockers);Assert.Equal(ready.OriginalSource.RequirementFactsRevision(),ready.CurrentSource.RequirementFactsRevision());
        var decision=new CompanyWorkOrderExecutionPermitDecision(ready,Guid.NewGuid(),"Approve","Independent complete original permit approval");
        var stock=f.Rows.OfType<Beep.OilandGas.Models.Data.ProductionAccounting.INVENTORY_ITEM>().Single();var quantity=stock.QUANTITY_ON_HAND;stock.QUANTITY_ON_HAND=quantity-1m;
        using(var changedStock=await finalHost.Http.PostAsJsonAsync(finalHost.Url+prefix+"/decision",decision)){Assert.True(changedStock.StatusCode==HttpStatusCode.Conflict,await changedStock.Content.ReadAsStringAsync());Assert.Equal(expectedWrites,f.Writes.Count);Assert.Equal("ISSUED_PENDING_APPROVAL",f.Rows.OfType<WORK_ORDER_EXECUTION_PERMIT>().Single().STATUS);}stock.QUANTITY_ON_HAND=quantity;
        var originalFile=f.Rows.OfType<RM_FILE_CONTENT>().Single(row=>row.FILE_ID=="risk-file");var originalBytes=originalFile.FILE_CONTENT!.ToArray();originalFile.FILE_CONTENT=originalBytes.Reverse().ToArray();
        using(var changedDocument=await finalHost.Http.PostAsJsonAsync(finalHost.Url+prefix+"/decision",decision))
        {
            Assert.True(changedDocument.StatusCode==HttpStatusCode.Conflict,await changedDocument.Content.ReadAsStringAsync());Assert.Equal(expectedWrites,f.Writes.Count);Assert.Equal("ISSUED_PENDING_APPROVAL",f.Rows.OfType<WORK_ORDER_EXECUTION_PERMIT>().Single().STATUS);
        }
        f.Rows.OfType<RM_FILE_CONTENT>().Single(row=>row.FILE_ID=="risk-file").FILE_CONTENT=originalBytes;
        int PrivateReads()=>f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && new[]{"WORK_ORDER_EXECUTION_PERMIT","WORK_ORDER_INSPECTION","WORK_ORDER_PERSONNEL_ASSIGNMENT","RM_FILE_CONTENT","COMPANY_PERSONNEL_IDENTITY"}.Contains(call.Arguments[0] as string,StringComparer.Ordinal));
        var approvalPermission=f.Db.RoleClaims.Single(row=>row.ClaimValue=="HSE.ApprovePermit");f.Db.RoleClaims.Remove(approvalPermission);await f.Db.SaveChangesAsync();var reads=PrivateReads();
        using(var revoked=await finalHost.Http.PostAsJsonAsync(finalHost.Url+prefix+"/decision",decision)){Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(expectedWrites,f.Writes.Count);Assert.Equal(reads,PrivateReads());}f.Db.RoleClaims.Add(approvalPermission);await f.Db.SaveChangesAsync();
        var receipt=await Post<CompanyWorkOrderExecutionPermitDecisionReceipt>(finalHost,prefix+"/decision",decision);receipt.Require(decision);expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);
        var recorded=f.Rows.OfType<WORK_ORDER_EXECUTION_PERMIT>().Single();decision.RequireRecorded(recorded);Assert.Equal("APPROVED",recorded.STATUS);
        var reference=new CompanyWorkOrderExecutionPermitDecisionReference(query,decision.IntentId);var recovered=await Post<CompanyWorkOrderExecutionPermitDecisionRecovery>(finalHost,prefix+"/decision/recovery/reference",reference);recovered.Require(intent.Context.Company,"personnel-reviewer",reference);Assert.Equal(expectedWrites,f.Writes.Count);
        using var replacement=await finalHost.Http.PostAsJsonAsync(finalHost.Url+prefix+"/decision",decision with{IntentId=Guid.NewGuid()});Assert.Equal(HttpStatusCode.Conflict,replacement.StatusCode);Assert.Equal(expectedWrites,f.Writes.Count);
        if(!executionPrerequisites)return;
        const string executionActor="execution-technician";
        if(!f.Db.Users.Any(row=>row.Id==executionActor)){f.Db.Users.Add(new OilGasUser{Id=executionActor,UserName=executionActor,IsActive=true,ConcurrencyStamp="executor-version"});f.Db.UserRoles.Add(new IdentityUserRole<string>{UserId=executionActor,RoleId="admin"});await f.Db.SaveChangesAsync();}
        f.Request.User=new(new ClaimsIdentity([new("party_id",executionActor)],"fixture"));await f.Select("company",executionActor);
        await f.Grant("FIELD",query.Task.FieldId,"WRITE");await f.Grant("WORK_ORDER",query.Task.WorkOrderId,"WRITE");
        var executionProfile=Profile(f);await Save(executionProfile);using var executionBoundary=new Boundary(f,executionProfile);
        executionBoundary.Access.Setup(value=>value.CheckAssetAccessAsync(executionActor,query.Task.FieldId,"FIELD",It.IsAny<string?>(),null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        var executionTenant=new RepositoryTenantContext();executionTenant.SelectCompany(f.Gate.Company);var executionModules=new ModuleConnectionResolver(f.Db,f.Editor.Object,executionTenant);
        var executionOwners=MaintenanceReadOwners(f,executionModules,executionProfile);
        var executionProduction=new PPDMProductionService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new PPDMMappingService(),executionProfile,f.Policy);
        var executionPersonnel=new ContractorManagementService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,async()=>
        {var access=await f.Gate.RefreshCompanyAsync();var core=await executionModules.ResolveAsync("PPDM_CORE");return(access.Scope,access.UserId,new AssetDatabaseScope(core,await executionModules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],core)));},Microsoft.Extensions.Logging.Abstractions.NullLogger<ContractorManagementService>.Instance);
        var executionService=new CompanyMaintenanceHandoffService(f.Gate,executionModules,executionProfile,executionProfile,new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>()),new BoundProcessService(()=>executionModules.ResolveAsync("LIFECYCLE"),_=>process),executionProduction,executionBoundary.Orchestrator.Object,executionOwners.Order,executionOwners.Inspections,permitOwner,inventory,executionPersonnel);
        await using var executionHost=await HttpHost.Start(f,executionProfile,executionBoundary,true,typeof(CompanyMaintenanceHandoffController),services=>
        {
            services.AddSingleton<Func<string>>(()=>executionActor);services.AddSingleton(executionService);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();
            services.AddAuthentication(options=>{options.DefaultAuthenticateScheme="receiving-fixture";options.DefaultChallengeScheme="receiving-fixture";}).AddScheme<AuthenticationSchemeOptions,ReceivingAuthentication>("receiving-fixture",_=>{});
        });
        int ExecutionPrivateReads()=>f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && new[]{"WORK_ORDER_EXECUTION_PERMIT","WORK_ORDER_INSPECTION","WORK_ORDER_PERSONNEL_ASSIGNMENT","RM_FILE_CONTENT","COMPANY_PERSONNEL_IDENTITY"}.Contains(call.Arguments[0] as string,StringComparer.Ordinal));
        var beforeExecution=ExecutionPrivateReads();
        using(var denied=await executionHost.Http.PostAsJsonAsync(executionHost.Url+prefix+"/execution-prerequisites",query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);
        Assert.Equal(beforeExecution,ExecutionPrivateReads());
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ExecuteWork"});await f.Db.SaveChangesAsync();
        var proof=await Post<CompanyWorkOrderExecutionPrerequisites>(executionHost,prefix+"/execution-prerequisites",query);
        proof.Require(intent.Context.Company,executionActor,query);Assert.NotEqual(decision.Review.Context.ActorUserId,proof.Requirements.Context.ActorUserId);
        Assert.Equal(allocation.ALLOCATION_ID,proof.Material.AllocationId);Assert.Equal(assigned.ASSIGNMENT_ID,proof.Personnel.AssignmentId);Assert.Equal(2,proof.Documents.Count);
        Assert.Equal(expectedWrites,f.Writes.Count);decision.RequireRecorded(f.Rows.OfType<WORK_ORDER_EXECUTION_PERMIT>().Single());
        if(!executionStart)return;
        var startType=typeof(WORK_ORDER_EXECUTION_START);var schemaCreator=new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object);
        f.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_EXECUTION_START")).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_START",EntityTypeName=startType.FullName!,PrimaryKeyColumn="START_ID"});f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_START")).ReturnsAsync(["START_ID"]);
        f.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_START")).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName=="WORK_ORDER_EXECUTION_START"),true)).Returns(()=>schemaCreator.ConvertToEntityStructure(startType));
        var startReview=await Post<CompanyWorkOrderExecutorIdentity>(executionHost,prefix+"/work-start/review",query);startReview.Require();Assert.Equal(executionActor,startReview.Identity.UserId);Assert.Equal(assigned.EMPLOYEE_BA_ID,startReview.Person.PersonBaId);
        var startCommand=new CompanyWorkOrderExecutionStartCommand(startReview,Guid.NewGuid(),DateTime.UtcNow,"Explicit actual original reviewed work start");
        var startReference=new CompanyWorkOrderExecutionStartReference(query,startCommand.IntentId);
        using(var absentStart=await executionHost.Http.PostAsJsonAsync(executionHost.Url+prefix+"/work-start/recovery/reference",startReference))Assert.Equal(HttpStatusCode.Conflict,absentStart.StatusCode);
        var startReceipt=await Post<CompanyWorkOrderExecutionStartReceipt>(executionHost,prefix+"/work-start",startCommand);startReceipt.Require(startCommand);expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);startCommand.RequireRecorded(f.Rows.OfType<WORK_ORDER_EXECUTION_START>().Single());
        var recoveredStart=await Post<CompanyWorkOrderExecutionStartRecovery>(executionHost,prefix+"/work-start/recovery/reference",startReference);recoveredStart.Require(intent.Context.Company,executionActor,startReference);Assert.Equal(expectedWrites,f.Writes.Count);
        using(var replacementStart=await executionHost.Http.PostAsJsonAsync(executionHost.Url+prefix+"/work-start",startCommand with{IntentId=Guid.NewGuid()}))Assert.Equal(HttpStatusCode.Conflict,replacementStart.StatusCode);Assert.Equal(expectedWrites,f.Writes.Count);
        if(!executionSubmission)return;
        var submissionType=typeof(WORK_ORDER_EXECUTION_SUBMISSION);var submissionTable=submissionType.Name;
        f.Metadata.Setup(value=>value.GetTableMetadataAsync(submissionTable)).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName=submissionTable,EntityTypeName=submissionType.FullName!,PrimaryKeyColumn="SUBMISSION_ID"});f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(submissionTable)).ReturnsAsync(["SUBMISSION_ID"]);f.Source.Setup(value=>value.CheckEntityExist(submissionTable)).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName==submissionTable),true)).Returns(()=>schemaCreator.ConvertToEntityStructure(submissionType));
        if(executionMaterialUsage)
        {
            var usageType=typeof(WORK_ORDER_MATERIAL_USAGE);var usageTable=usageType.Name;
            f.Metadata.Setup(value=>value.GetTableMetadataAsync(usageTable)).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName=usageTable,EntityTypeName=usageType.FullName!,PrimaryKeyColumn="USAGE_ID"});
            f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(usageTable)).ReturnsAsync(["USAGE_ID"]);
            f.Source.Setup(value=>value.CheckEntityExist(usageTable)).Returns(true);
            f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName==usageTable),true)).Returns(()=>schemaCreator.ConvertToEntityStructure(usageType));
            var usageReview=await Post<CompanyWorkOrderMaterialUsageReview>(executionHost,prefix+"/material-usage/review",startReference);usageReview.Require();
            Assert.Equal(allocation.ALLOCATION_ID,usageReview.Reservation.AllocationId);Assert.Equal(startReceipt.StartId,usageReview.Start.StartId);Assert.Equal(expectedWrites,f.Writes.Count);
            var physical=new CompanyWorkOrderMaterialUsageCommand(usageReview,Guid.NewGuid(),1m,usageReview.Reservation.Unit,DateTime.UtcNow,"Actual quantity physically used for the original order");
            var physicalReference=new CompanyWorkOrderMaterialUsageReference(startReference,physical.IntentId);
            var used=await Post<CompanyWorkOrderMaterialUsageRecovery>(executionHost,prefix+"/material-usage",physical);
            used.Require(intent.Context.Company,executionActor,physicalReference);expectedWrites+=3;Assert.Equal(expectedWrites,f.Writes.Count);
            Assert.Equal(4m,f.Rows.OfType<Beep.OilandGas.Models.Data.ProductionAccounting.INVENTORY_ITEM>().Single().QUANTITY_ON_HAND);
            Assert.Null(used.Movement.UNIT_COST);Assert.Null(used.Movement.TOTAL_COST);
            (await Post<CompanyWorkOrderMaterialUsageRecovery>(executionHost,prefix+"/material-usage/recovery/reference",physicalReference)).Require(intent.Context.Company,executionActor,physicalReference);
            (await Post<CompanyWorkOrderMaterialUsageRecovery>(executionHost,prefix+"/material-usage",physical)).Require(intent.Context.Company,executionActor,physicalReference);
            Assert.Equal(expectedWrites,f.Writes.Count);decision.RequireRecorded(f.Rows.OfType<WORK_ORDER_EXECUTION_PERMIT>().Single());startCommand.RequireRecorded(f.Rows.OfType<WORK_ORDER_EXECUTION_START>().Single());
        }
        if(executionLaborUsage)
        {
            var laborType=typeof(WORK_ORDER_LABOR_USAGE);var laborTable=laborType.Name;
            f.Metadata.Setup(value=>value.GetTableMetadataAsync(laborTable)).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName=laborTable,EntityTypeName=laborType.FullName!,PrimaryKeyColumn="LABOR_ID"});
            f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(laborTable)).ReturnsAsync(["LABOR_ID"]);
            f.Source.Setup(value=>value.CheckEntityExist(laborTable)).Returns(true);
            f.Source.Setup(value=>value.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(shape=>shape.EntityName==laborTable),true)).Returns(()=>schemaCreator.ConvertToEntityStructure(laborType));
            var laborReview=await Post<CompanyWorkOrderLaborUsageReview>(executionHost,prefix+"/labor-usage/review",startReference);laborReview.Require();
            Assert.Equal(startReceipt.StartId,laborReview.Execution.Start.StartId);Assert.Equal(startCommand.Review.Prerequisites.Personnel,laborReview.Assignment);Assert.Equal(expectedWrites,f.Writes.Count);
            var from=laborReview.Execution.Start.StartedAtUtc;var to=laborReview.ComparedAtUtc;
            var labor=new CompanyWorkOrderLaborUsageCommand(laborReview,Guid.NewGuid(),from,to,(decimal)(to.Ticks-from.Ticks)/TimeSpan.TicksPerSecond,WorkOrderLaborTimeUnit.Second,"Actual executed pump work","Original employee duty report");
            var result=await Post<CompanyWorkOrderLaborUsageRecovery>(executionHost,prefix+"/labor-usage",labor);result.Require(intent.Context.Company,executionActor,labor.Reference());
            expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);Assert.Single(f.Rows.OfType<WORK_ORDER_LABOR_USAGE>());
            (await Post<CompanyWorkOrderLaborUsageRecovery>(executionHost,prefix+"/labor-usage/recovery/reference",labor.Reference())).Require(intent.Context.Company,executionActor,labor.Reference());
            (await Post<CompanyWorkOrderLaborUsageRecovery>(executionHost,prefix+"/labor-usage",labor)).Require(intent.Context.Company,executionActor,labor.Reference());
            Assert.Equal(expectedWrites,f.Writes.Count);decision.RequireRecorded(f.Rows.OfType<WORK_ORDER_EXECUTION_PERMIT>().Single());startCommand.RequireRecorded(f.Rows.OfType<WORK_ORDER_EXECUTION_START>().Single());
        }
        Assert.Equal(2,intent.RequirementEvidenceVersion);Assert.Equal(7,decision.Review.PrerequisiteEvidenceVersion);
        var blocked=await Post<CompanyWorkOrderSubmissionReview>(executionHost,prefix+"/work-submission/review",startReference);blocked.Require();Assert.Contains(blocked.Inspections.Blockers,value=>value.Contains("work-submission inspection evidence"));Assert.Equal(expectedWrites,f.Writes.Count);
        var completedUtc=DateTime.UtcNow;var actual=f.Rows.OfType<WORK_ORDER_INSPECTION>().Single(row=>row.APPLICABLE_STAGE=="WORK_SUBMISSION");var actualProjection=WorkOrderInspectionRules.Project(actual);
        f.Http.HttpContext=f.Request;f.Request.User=await new HandoffRoleBridge(f.Gate).TransformAsync(new(new ClaimsIdentity([new("party_id",executionActor)],"fixture")));
        // Actual existing native inspection owner; the protected report endpoints remain API-first.
        var recordedInspection=await executionOwners.Inspections.RecordResultAsync(query.Task.WorkOrderId,actual.COND_SEQ,new(){FieldId=query.Task.FieldId,InspectionId=actual.INSPECTION_ID,ExpectedSourceRevision=actualProjection.SourceRevision,Result="PASS",Notes="Actual executed work inspected after its original recorded start"},executionActor);Assert.Equal("PASS",recordedInspection.Status);expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);
        var reportReview=await Post<CompanyWorkOrderSubmissionReview>(executionHost,prefix+"/work-submission/review",startReference);reportReview.Require();Assert.Empty(reportReview.Inspections.Blockers);
        var report=new CompanyWorkOrderSubmissionCommand(reportReview,Guid.NewGuid(),completedUtc,"Explicit actual completed pump work",[],"Original executor report pending independent verification");
        var reportReceipt=await Post<CompanyWorkOrderSubmissionReceipt>(executionHost,prefix+"/work-submission",report);reportReceipt.Require(report);expectedWrites++;Assert.Equal(expectedWrites,f.Writes.Count);
        var reportReference=new CompanyWorkOrderSubmissionReference(startReference,report.IntentId);var recoveredReport=await Post<CompanyWorkOrderSubmissionRecovery>(executionHost,prefix+"/work-submission/recovery/reference",reportReference);recoveredReport.Require(intent.Context.Company,executionActor,reportReference);Assert.Equal(expectedWrites,f.Writes.Count);
        (await Post<CompanyWorkOrderSubmissionReceipt>(executionHost,prefix+"/work-submission",report)).Require(report);Assert.Equal(expectedWrites,f.Writes.Count);
        using(var replacementReport=await executionHost.Http.PostAsJsonAsync(executionHost.Url+prefix+"/work-submission",report with{IntentId=Guid.NewGuid()}))Assert.Equal(HttpStatusCode.Conflict,replacementReport.StatusCode);Assert.Equal(expectedWrites,f.Writes.Count);decision.RequireRecorded(f.Rows.OfType<WORK_ORDER_EXECUTION_PERMIT>().Single());startCommand.RequireRecorded(f.Rows.OfType<WORK_ORDER_EXECUTION_START>().Single());
    }
}
