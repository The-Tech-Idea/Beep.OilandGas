using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Repository.Models;
#if RECORDED_APPROVAL_HTTP_FOCUSED
using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Beep.OilandGas.ApiService.Controllers.Permits;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using Xunit;
using Fixture=Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact]
    public Task PermitDocumentProviderHttpOwnsTwoTargetsAndCompanyPrivateRecovery()=>new PermitProviderQualificationTests().WithInstalledProviderAsync(async(editor,permits)=>
    {
        var coreConnection=new ConnectionProperties {ConnectionName="selected-tenant",DatabaseType=TheTechIdea.Beep.Utilities.DataSourceType.SqlLite,
            Database=Path.Combine(Path.GetDirectoryName(permits.Dataconnection.ConnectionProp.Database)!,"document-http-core.db"),GuidID=Guid.NewGuid().ToString()};
        coreConnection.ConnectionString="Data Source="+coreConnection.Database;editor.ConfigEditor.DataConnections.Add(coreConnection);
        var core=editor.GetDataSource(coreConnection.ConnectionName)??throw new InvalidOperationException("Setup-loaded editor must own the CORE document datasource.");
        Assert.Equal(ConnectionState.Open,core.Openconnection());
        try
        {
            PermitProviderQualificationTests.InstallLedgerSchema(editor,permits);PermitProviderQualificationTests.InstallDocumentCore(editor,core);
            var journal=new MigrationManager(editor,permits){ExecutionTargetIdentity=MigrationConnectionTarget.Fingerprint(permits.Dataconnection.ConnectionProp)};
            var journalPlan=journal.BuildMigrationPlanForTypes([typeof(PERMIT_DOCUMENT_STAGE)],detectRelationships:false);
            journal.ApproveMigrationPlan(journalPlan,"document-http-stage-journal");Assert.True(journal.ExecuteMigrationPlan(journalPlan).Success);
            using var f=await Fixture.Create(coreConnection);
            f.Rows.Add(new FIELD {FIELD_ID="A",ACTIVE_IND="Y"});await f.Grant("FIELD","A","WRITE",inherit:true);
            await f.Select("other-company");await f.Grant("FIELD","A","WRITE",inherit:true);await f.Select("company");
            InstallDocumentHttpAuthorityCore(editor,core,f.Rows.Select(x=>x.GetType()).Distinct().Select(type=>(type,Keys:DocumentHttpKeys(type))).ToArray());
            var columns=new CommonColumnHandler();f.Defaults.Setup(x=>x.GetActiveIndicatorYes()).Returns("Y");
            foreach(var row in f.Rows)
            {columns.PrepareForInsert((IPPDMEntity)row,"actor");Assert.Equal(Errors.Ok,core.InsertEntity(row.GetType().Name,row).Flag);}
            f.Editor.SetupGet(x=>x.ConfigEditor).Returns(editor.ConfigEditor);
            f.Editor.Setup(x=>x.GetDataSource(coreConnection.ConnectionName)).Returns(core);
            f.Editor.Setup(x=>x.GetDataSource(permits.DatasourceName)).Returns(permits);
            f.Editor.Setup(x=>x.OpenDataSource(coreConnection.ConnectionName)).Returns(ConnectionState.Open);
            f.Editor.Setup(x=>x.OpenDataSource(permits.DatasourceName)).Returns(ConnectionState.Open);
            var interruptConfirmation=true;var interrupted=false;
            f.Columns.Setup(x=>x.PrepareForInsert(It.IsAny<IPPDMEntity>(),It.IsAny<string>())).Callback((IPPDMEntity row,string actor)=>columns.PrepareForInsert(row,actor));
            f.Columns.Setup(x=>x.PrepareForUpdate(It.IsAny<IPPDMEntity>(),It.IsAny<string>())).Callback((IPPDMEntity row,string actor)=>
            {if(interruptConfirmation && row is PERMIT_DOCUMENT_STAGE){interrupted=true;throw new IOException("Selected fault after CORE commit before PERMITS confirmation.");}columns.PrepareForUpdate(row,actor);});
            foreach(var permission in new[]{PermissionConstants.HSE.ViewPermits,PermissionConstants.HSE.IssuePermit})
                f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=permission});
            var permitConnection=Assert.IsType<ConnectionProperties>(permits.Dataconnection.ConnectionProp);
            f.Db.Add(new AppModuleConnection {TenantId="tenant",ConnectionKey=permits.DatasourceName.ToUpperInvariant(),AssignedByUserId="actor",PhysicalTargetKey=ModuleDatabaseTarget.Key(f.Editor.Object,permitConnection)});
            f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding {TenantId="tenant",ModuleId="PERMITS",ConnectionName=permits.DatasourceName,ConnectionKey=permits.DatasourceName.ToUpperInvariant(),ConcurrencyStamp="document-http-v1"});await f.Db.SaveChangesAsync();
            var runtimeMetadata=new Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PPDMMetadataService(Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PPDMMetadataService>.Instance,Path.Combine(AppContext.BaseDirectory,"PPDM39Metadata.json"), [new Beep.OilandGas.PermitsAndApplications.Metadata.PermitApplicationNativeMetadata()]);
            const string applicationId="document/شركة?A";
            foreach(var tuple in new[]{(Id:applicationId,Company:"company",Applicant:"Selected document applicant"),(Id:"other-app",Company:"other-company",Applicant:"PRIVATE OTHER COMPANY")})
            {
                var row=new PERMIT_APPLICATION {PERMIT_APPLICATION_ID=tuple.Id,OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID=tuple.Company,FIELD_ID="A",APPLICANT_ID=tuple.Applicant,COUNTRY=Beep.OilandGas.Models.Data.PermitsAndApplications.Country.Kuwait,ACTIVE_IND="Y"};
                columns.PrepareForInsert(row,"actor");Assert.Equal(Errors.Ok,permits.InsertEntity(nameof(PERMIT_APPLICATION),row).Flag);
            }
            var timings=new System.Collections.Concurrent.ConcurrentDictionary<string,(long Count,double Seconds)>();
            var timingLog=Path.Combine(Path.GetDirectoryName(coreConnection.Database)!,"document-http-observations.jsonl");
            var placeholder=Profile(f);using var boundary=new Boundary(f,placeholder);
            await using var host=await HttpHost.Start(f,placeholder,boundary,true,typeof(PermitsController),services=>{RegisterPermitProviderHttp(services,f,runtimeMetadata);services.AddScoped<ICompanyFieldEligibility>(sp=>new TimedDocumentHttpEligibility(new PPDMCompanyFieldEligibility(sp.GetRequiredService<CompanyModuleConnection>(),f.Editor.Object,f.Columns.Object,f.Defaults.Object,sp.GetRequiredService<IPPDMMetadataRepository>(),Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration)),seconds=>timings.AddOrUpdate("eligibility",(1,seconds),(_,before)=>(before.Count+1,before.Seconds+seconds))));},companyMiddleware:true);
            foreach(var company in new[]{"company","other-company"})
            {
                using var scoped=host.Services.CreateScope();var selected=await scoped.ServiceProvider.GetRequiredService<RepositoryCompanyContextResolver>().ResolveAsync("actor",new("tenant",company));Assert.NotNull(selected);
                scoped.ServiceProvider.GetRequiredService<RepositoryTenantContext>().SelectCompany(selected);await Save(scoped.ServiceProvider.GetRequiredService<RepositoryUserProfileService>());
            }
            async Task<HttpResponseMessage> Send(string route,string? company,object? body=null)
            {
                using var request=new HttpRequestMessage(body is null?HttpMethod.Get:HttpMethod.Post,host.Url+"/api/field/current/permits/"+route);
                if(company is not null){request.Headers.Add(CompanyRequestHeaders.Tenant,"tenant");request.Headers.Add(CompanyRequestHeaders.Company,company);}
                if(body is not null)request.Content=JsonContent.Create(body,body.GetType());
                timings.Clear();var timer=System.Diagnostics.Stopwatch.StartNew();var path=route.Split('?')[0];
                void Record(int? status,string? failure)=>File.AppendAllText(timingLog,System.Text.Json.JsonSerializer.Serialize(new{At=DateTimeOffset.UtcNow,Route=path,Status=status,Failure=failure,Seconds=timer.Elapsed.TotalSeconds,Eligibility=timings.ToDictionary(x=>x.Key,x=>new{x.Value.Count,x.Value.Seconds})})+Environment.NewLine);
                try{var response=await host.Http.SendAsync(request);Record((int)response.StatusCode,null);return response;}
                catch(Exception error) when(error is HttpRequestException or TaskCanceledException){Record(null,error.GetType().Name);throw new InvalidOperationException("Selected document HTTP route "+path+" failed. Timings: "+timingLog,error);}
            }
            async Task<PermitApplicationSourceReview> Read(string id,string company="company")
            {
                using var response=await Send("application-review?applicationId="+Uri.EscapeDataString(id),company);
                Assert.True(response.IsSuccessStatusCode,$"Review {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");Assert.True(response.Headers.CacheControl?.NoStore);return (await response.Content.ReadFromJsonAsync<PermitApplicationSourceReview>())!;
            }
            async Task<T> Require<T>(HttpResponseMessage response)
            {using(response){Assert.True(response.IsSuccessStatusCode,$"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");Assert.True(response.Headers.CacheControl?.NoStore);return (await response.Content.ReadFromJsonAsync<T>())!;}}
            int Count(IDataSource source,string table)=>((System.Collections.IEnumerable)source.GetEntity(table,[])).Cast<object>().Count();
            using(var missing=await Send("application-review?applicationId="+Uri.EscapeDataString(applicationId),null))Assert.Equal(HttpStatusCode.BadRequest,missing.StatusCode);
            var review=await Read(applicationId);var other=await Read("other-app","other-company");Assert.Equal("other-company",other.Context.Field.Company.BusinessAssociateId);
            using(var foreign=await Send("application-review?applicationId="+Uri.EscapeDataString(applicationId),"other-company")){Assert.Equal(HttpStatusCode.NotFound,foreign.StatusCode);Assert.DoesNotContain("Selected document applicant",await foreign.Content.ReadAsStringAsync());}
            var bytes=Enumerable.Range(0,257).Select(i=>(byte)(i%256)).ToArray();var original=PermitProviderQualificationTests.StageOriginal(review,bytes);
            using(var foreign=await Send("application-document-stage/recovery","other-company",original))Assert.Equal(HttpStatusCode.Forbidden,foreign.StatusCode);
            var invalidVocabulary=original with{IntentId=Guid.NewGuid(),Document=original.Document with{Language="unrecorded-language"}};
            using(var invalid=await Send("application-document-stage","company",new PermitDocumentStageRequest(invalidVocabulary,bytes)))
            {Assert.Equal(HttpStatusCode.Conflict,invalid.StatusCode);Assert.Contains("PPDM setup",await invalid.Content.ReadAsStringAsync());}
            Assert.Equal(0,Count(permits,nameof(PERMIT_DOCUMENT_STAGE)));Assert.Equal(0,Count(core,nameof(SOURCE_DOCUMENT)));Assert.Equal(0,Count(core,nameof(RM_FILE_CONTENT)));
            using(var failed=await Send("application-document-stage","company",new PermitDocumentStageRequest(original,bytes)))Assert.Equal(HttpStatusCode.InternalServerError,failed.StatusCode);
            Assert.True(interrupted);Assert.Equal(1,Count(core,nameof(SOURCE_DOCUMENT)));Assert.Equal(1,Count(core,nameof(RM_INFORMATION_ITEM)));Assert.Equal(1,Count(core,nameof(RM_FILE_CONTENT)));Assert.Equal(1,Count(permits,nameof(PERMIT_DOCUMENT_STAGE)));
            var pending=await Require<PermitDocumentStageResponse>(await Send("application-document-stage/recovery","company",original));pending.Require(original);Assert.Null(pending.Receipt);
            interruptConfirmation=false;
            var staged=await Require<PermitDocumentStageResponse>(await Send("application-document-stage","company",new PermitDocumentStageRequest(original,bytes)));staged.Require(original);Assert.NotNull(staged.Receipt);
            var wire=System.Text.Json.JsonSerializer.Serialize(staged);Assert.DoesNotContain("CoreTarget",wire);Assert.DoesNotContain("PermitTarget",wire);Assert.DoesNotContain("Seed",wire);Assert.DoesNotContain(MigrationConnectionTarget.Fingerprint(coreConnection),wire);
            Assert.Equal(1,Count(core,nameof(RM_FILE_CONTENT)));Assert.Equal(1,Count(permits,nameof(PERMIT_DOCUMENT_STAGE)));
            var link=new PermitApplicationCommand(Guid.NewGuid(),await Read(applicationId),"ATTACH_DOCUMENT","Reviewed current application and exact confirmed document before linking"){Attachment=new(staged.Receipt!)};
            var linked=await Require<PermitApplicationCommandOutcome>(await Send("application-command","company",link));linked.Require();Assert.Equal(link,linked.Original);
            var current=await Read(applicationId);Assert.True(current.Supporting.AllDocumentsVerified);var query=new PermitAttachmentContentQuery(current,current.Supporting.Attachments.Single().APPLICATION_ATTACHMENT_ID);
            using(var foreign=await Send("application-attachment-download","other-company",query))Assert.Equal(HttpStatusCode.Forbidden,foreign.StatusCode);
            using(var download=await Send("application-attachment-download","company",query))
            {Assert.Equal(HttpStatusCode.OK,download.StatusCode);Assert.True(download.Headers.CacheControl?.NoStore);Assert.Equal("application/octet-stream",download.Content.Headers.ContentType!.MediaType);Assert.Equal("attachment",download.Content.Headers.ContentDisposition!.DispositionType);Assert.Equal("nosniff",download.Headers.GetValues("X-Content-Type-Options").Single());Assert.Equal(bytes,await download.Content.ReadAsByteArrayAsync());}
            var content=await Require<PermitAttachmentContent>(await Send("application-attachment-content","company",query));content.Require(query);Assert.Equal(bytes,content.Content);
            f.Db.RoleClaims.Remove(await f.Db.RoleClaims.SingleAsync(x=>x.ClaimValue==PermissionConstants.HSE.IssuePermit));await f.Db.SaveChangesAsync();
            var recovered=await Require<PermitDocumentStageResponse>(await Send("application-document-stage/recovery","company",original));recovered.Require(original);Assert.Equal(staged,recovered);
            var replay=await Require<PermitDocumentStageResponse>(await Send("application-document-stage","company",new PermitDocumentStageRequest(original,bytes)));Assert.Equal(staged,replay);
            var denied=original with{IntentId=Guid.NewGuid()};using(var refusal=await Send("application-document-stage","company",new PermitDocumentStageRequest(denied,bytes)))Assert.Equal(HttpStatusCode.Forbidden,refusal.StatusCode);
            var absent=await Require<PermitDocumentStageResponse>(await Send("application-document-stage/recovery","company",denied));absent.Require(denied);Assert.Null(absent.Receipt);
            var recoveredLink=await Require<PermitApplicationCommandRecovery>(await Send("application-command/recovery","company",link));recoveredLink.Require(link);Assert.Equal(linked,recoveredLink.Outcome);
            Assert.Equal(1,Count(core,nameof(RM_FILE_CONTENT)));Assert.Equal(1,Count(permits,nameof(PERMIT_DOCUMENT_STAGE)));Assert.Equal(1,Count(permits,nameof(APPLICATION_ATTACHMENT)));Assert.Equal(1,Count(permits,nameof(PERMIT_STATUS_HISTORY)));Assert.Equal(1,Count(permits,nameof(PERMIT_APPLICATION_COMMAND)));Assert.Empty(f.Writes);
        }
        finally{core.Closeconnection();}
    });
    private sealed class TimedDocumentHttpEligibility(ICompanyFieldEligibility inner,Action<double> observed):ICompanyFieldEligibility
    {
        public async Task<string?> GetMaximumLevelAsync(string fieldId)
        {var timer=System.Diagnostics.Stopwatch.StartNew();try{return await inner.GetMaximumLevelAsync(fieldId);}finally{observed(timer.Elapsed.TotalSeconds);}}
    }
    private static string[] DocumentHttpKeys(Type type)=>type.Name switch
    {"FIELD"=>["FIELD_ID"],"ENT_COMPONENT"=>["ENTITLEMENT_ID","COMPONENT_ID","COMPONENT_OBS_NO"],"ENTITLEMENT"=>["ENTITLEMENT_ID"],"R_ENT_TYPE"=>["ENTITLEMENT_TYPE"],"R_ENT_COMPONENT_TYPE"=>["ENT_COMP_TYPE"],"R_ACCESS_CONDITION"=>["ACCESS_CONDITION_CODE"],"R_ENT_ACCESS_TYPE"=>["ACCESS_TYPE"],_=>throw new InvalidOperationException("Unlisted controlled authority table "+type.Name)};
    private static void InstallDocumentHttpAuthorityCore(IDMEEditor editor,IDataSource core,IReadOnlyList<(Type Type,string[] Keys)> definitions)
    {
        var model=new MigrationModel{Source="Controlled document HTTP native authority schema",SourceId="permit-document-http-authority"};
        foreach(var (type,keys) in definitions)
        {
            var entity=new MigrationModelEntity{ClrTypeFullName=type.FullName!,TableName=type.Name};
            foreach(var property in type.GetProperties().Where(x=>x.CanRead && x.CanWrite && x.GetIndexParameters().Length==0))
            {var actual=Nullable.GetUnderlyingType(property.PropertyType)??property.PropertyType;entity.Properties.Add(new(){PropertyName=property.Name,ColumnName=property.Name,FieldType=actual.FullName!,IsPrimaryKey=keys.Contains(property.Name),IsNullable=!keys.Contains(property.Name),MaxLength=actual==typeof(string)?128:null});}
            model.Entities.Add(type.FullName!,entity);
        }
        var migration=new MigrationManager(editor,core){ExecutionTargetIdentity=MigrationConnectionTarget.Fingerprint(core.Dataconnection.ConnectionProp)};
        var plan=migration.BuildMigrationPlanForModel(model,detectRelationships:false);Assert.Equal(definitions.Count,plan.Operations.Count);migration.ApproveMigrationPlan(plan,"document-http-native-authority");var result=migration.ExecuteMigrationPlan(plan);Assert.True(result.Success,result.Message);
    }
}
#endif
