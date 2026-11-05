#if RECORDED_APPROVAL_HTTP_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PermitsAndApplications.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;
namespace Beep.OilandGas.Repository.Tests;
public sealed class CompanyPermitApplicationDataAccessTests
{
    private sealed class Native : IDisposable
    {
        public required Fixture Core;
        public Mock<IDataSource> Source {get;} = new();
        public System.Data.IDbTransaction? ActiveTransaction;
        public Mock<System.Data.IDbConnection> Physical {get;} = new();
        public List<string> Transactions {get;} = [];
        public bool Persist;
        public bool CloneReads;
        public bool LoseCommitAck;
        public int FailWriteNumber;
        public List<object> Writes {get;}=[];
        public Action<object>? AfterWrite;
        private object[]? snapshot;
        private static object Copy(object row)=>System.Text.Json.JsonSerializer.Deserialize(System.Text.Json.JsonSerializer.Serialize(row,row.GetType()),row.GetType())!;
        public Native()
        {
            Source.As<IBeepTransactionEvidence>().SetupGet(x=>x.ActiveTransaction).Returns(()=>ActiveTransaction);
            Physical.SetupGet(x=>x.State).Returns(System.Data.ConnectionState.Open);
            Source.Setup(x=>x.BeginTransaction(It.IsAny<TheTechIdea.Beep.Addin.PassedArgs>())).Returns(()=>
            {
                var tx=new Mock<System.Data.IDbTransaction>();tx.SetupGet(x=>x.Connection).Returns(Physical.Object);tx.SetupGet(x=>x.IsolationLevel).Returns(System.Data.IsolationLevel.Serializable);
                snapshot=Rows.Select(Copy).ToArray();ActiveTransaction=tx.Object;Transactions.Add("begin");return new ErrorsInfo{Flag=Errors.Ok};
            });
            Source.Setup(x=>x.Commit(It.IsAny<TheTechIdea.Beep.Addin.PassedArgs>())).Returns(()=>{ActiveTransaction=null;snapshot=null;Transactions.Add("commit");return new ErrorsInfo{Flag=LoseCommitAck?Errors.Failed:Errors.Ok};});
            Source.Setup(x=>x.EndTransaction(It.IsAny<TheTechIdea.Beep.Addin.PassedArgs>())).Returns(()=>{if(snapshot is not null){Rows.Clear();Rows.AddRange(snapshot);snapshot=null;}ActiveTransaction=null;Transactions.Add("rollback");return new ErrorsInfo{Flag=Errors.Ok};});
        }
        public List<object> Rows {get;} = [];
        public List<string> Reads {get;} = [];
        public bool IgnoreFilters;
        public string? IgnoreFilterTable;
        public required ConnectionProperties SourceConnection;
        public Func<Task>? AfterRead;
        public required CompanyPermitApplicationDataAccess Access;
        public required ModuleDatabaseBinding Binding;
        public PermitApplicationLifecycleService Lifecycle => new(Core.Editor.Object, Core.Columns.Object, Core.Defaults.Object, Core.Metadata.Object, Access);
        public PPDMGenericRepository Repository(Type type, string? parent = null) => new(Core.Editor.Object, Core.Columns.Object, Core.Defaults.Object, Core.Metadata.Object, type, "permits-current", type.Name, rowAccess: Access.ForRead(type,type.Name,parent));
        public void EnableWrites()
        {
            Persist=true;CloneReads=true;
            var columns=new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler();
            Core.Columns.Setup(x=>x.PrepareForInsert(It.IsAny<IPPDMEntity>(),It.IsAny<string>())).Callback((IPPDMEntity row,string actor)=>columns.PrepareForInsert(row,actor));
            Core.Columns.Setup(x=>x.PrepareForUpdate(It.IsAny<IPPDMEntity>(),It.IsAny<string>())).Callback((IPPDMEntity row,string actor)=>columns.PrepareForUpdate(row,actor));
            ErrorsInfo Write(string table,object row,bool update)
            {
                Writes.Add(Copy(row));
                if(FailWriteNumber==Writes.Count)return new ErrorsInfo{Flag=Errors.Failed};
                Assert.NotNull(ActiveTransaction);
                var key=table=="PERMIT_APPLICATION"?"PERMIT_APPLICATION_ID":table+"_ID";
                var value=row.GetType().GetProperty(key)!.GetValue(row);
                var matches=Rows.Where(x=>x.GetType()==row.GetType() && Equals(x.GetType().GetProperty(key)!.GetValue(x),value)).ToArray();
                if(matches.Length!=(update?1:0))return new ErrorsInfo{Flag=Errors.Failed};
                if(update)Rows.Remove(matches.Single());var saved=Copy(row);Rows.Add(saved);AfterWrite?.Invoke(saved);
                return new ErrorsInfo{Flag=Errors.Ok};
            }
            Source.Setup(x=>x.InsertEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object row)=>Write(table,row,false));
            Source.Setup(x=>x.UpdateEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object row)=>Write(table,row,true));
            foreach(var type in new[]{typeof(PERMIT_APPLICATION),typeof(PERMIT_STATUS_HISTORY),typeof(PERMIT_APPLICATION_COMMAND)})
            {
                var key=type.Name+"_ID";var definition=new PPDMTableMetadata{EntityTypeName=type.FullName!,PrimaryKeyColumn=key};
                Core.Metadata.Setup(x=>x.GetTableMetadataAsync(type.Name)).ReturnsAsync(definition);Core.Metadata.Setup(x=>x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync(new[]{key});
            }
            Source.Setup(x=>x.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool _) =>
            {
                var type=new[]{typeof(PERMIT_APPLICATION),typeof(PERMIT_STATUS_HISTORY),typeof(PERMIT_APPLICATION_COMMAND)}.Single(x=>x.Name==request.EntityName);
                var shape=new TheTechIdea.Beep.Tools.ClassCreator(Core.Editor.Object).ConvertToEntityStructure(type);
                foreach(var field in shape.Fields){field.IsKey=field.FieldName==type.Name+"_ID";if(field.Fieldtype==typeof(string).FullName)field.Size=field.Size1=8388608;}
                return shape;
            });
        }
        public void Dispose()=>Core.Dispose();
    }
    private static object CopyForRead(object row)=>System.Text.Json.JsonSerializer.Deserialize(System.Text.Json.JsonSerializer.Serialize(row,row.GetType()),row.GetType())!;
    private static async Task<Native> Ready(bool module = true)
    {
        var core = await CompanyCorrectiveActionAuthorityTests.Ready();
        core.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=PermissionConstants.HSE.ViewPermits}); await core.Db.SaveChangesAsync();
        var connection = new ConnectionProperties{ConnectionName="permits-current",DatabaseType=DataSourceType.SqlServer,Host="permit-server",Database="permits-tenant"}; core.Editor.Object.ConfigEditor.DataConnections.Add(connection);
        var binding=new ModuleDatabaseBinding{TenantId="tenant",ModuleId="PERMITS",ConnectionName="permits-current",ConnectionKey="PERMITS-CURRENT",ConcurrencyStamp="permit-binding-v1"};
        if(module){core.Db.Add(new AppModuleConnection{TenantId="tenant",ConnectionKey="PERMITS-CURRENT",AssignedByUserId="actor",PhysicalTargetKey=ModuleDatabaseTarget.Key(core.Editor.Object,connection)});core.Db.ModuleDatabases.Add(binding);await core.Db.SaveChangesAsync();}
        var access = new CompanyPermitApplicationDataAccess(core.Gate,new ModuleConnectionResolver(core.Db,core.Editor.Object,core.CompanySelection),CompanyCorrectiveActionAuthorityTests.Profile(core),core.Editor.Object,core.Columns.Object,core.Defaults.Object,core.Metadata.Object,new RepositoryAssetAccessStore(core.Db,core.Gate,Mock.Of<IAssetGrantTargetVerifier>()),core.Policy);
        var sourceConnection=new ConnectionProperties{ConnectionName="permits-current",DatabaseType=DataSourceType.SqlServer,Host="permit-server",Database="permits-tenant"};
        var fixture=new Native{Core=core,Access=access,Binding=binding,SourceConnection=sourceConnection};
        var target=new Mock<IDataConnection>();target.As<IBeepConnectionEvidence>().SetupGet(x=>x.DbConn).Returns(fixture.Physical.Object);target.SetupGet(x=>x.ConnectionProp).Returns(sourceConnection);fixture.Source.SetupGet(x=>x.Dataconnection).Returns(target.Object);
        fixture.Source.SetupGet(x=>x.DatasourceName).Returns("permits-current");fixture.Source.SetupGet(x=>x.ErrorObject).Returns(new ErrorsInfo{Flag=Errors.Ok});
        core.Editor.Setup(x=>x.GetDataSource("permits-current")).Returns(()=>{core.Connections.Add("permits-current");return fixture.Source.Object;});
        core.Editor.Setup(x=>x.OpenDataSource("permits-current")).Returns(System.Data.ConnectionState.Open);
        foreach(var type in new[]{typeof(PERMIT_APPLICATION),typeof(PERMIT_STATUS_HISTORY),typeof(INJECTION_PERMIT_APPLICATION),typeof(MIT_RESULT)})
        {
            var key=type==typeof(PERMIT_APPLICATION)||type==typeof(INJECTION_PERMIT_APPLICATION)?"PERMIT_APPLICATION_ID":type.Name+"_ID";
            var definition=new PPDMTableMetadata{EntityTypeName=type.Name,PrimaryKeyColumn=key};core.Metadata.Setup(x=>x.GetTableMetadataAsync(type.Name)).ReturnsAsync(definition);core.Metadata.Setup(x=>x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync(new[]{key});
        }
        fixture.Source.Setup(x=>x.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).Returns(async(string table,List<AppFilter> filters)=>
        {
            fixture.Reads.Add(table);var rows=fixture.Rows.Where(row=>row.GetType().Name==table && (fixture.IgnoreFilters && (fixture.IgnoreFilterTable is null || fixture.IgnoreFilterTable==table) || filters.All(filter=>Convert.ToString(row.GetType().GetProperty(filter.FieldName)!.GetValue(row),System.Globalization.CultureInfo.InvariantCulture)==filter.FilterValue))).ToArray();
            if(fixture.CloneReads)rows=rows.Select(CopyForRead).ToArray();if(fixture.AfterRead is not null)await fixture.AfterRead();return (IEnumerable<object>)rows;
        });
        fixture.Rows.AddRange([new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="local",FIELD_ID="A",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",ACTIVE_IND="Y"},new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="other",FIELD_ID="B",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",ACTIVE_IND="Y"},new PERMIT_STATUS_HISTORY{PERMIT_STATUS_HISTORY_ID="history",PERMIT_APPLICATION_ID="local",ACTIVE_IND="Y"},new INJECTION_PERMIT_APPLICATION{PERMIT_APPLICATION_ID="local",ACTIVE_IND="Y"},new MIT_RESULT{MIT_RESULT_ID="mit",INJECTION_PERMIT_APPLICATION_ID="local",ACTIVE_IND="Y"}]);
        return fixture;
    }
    [Theory][InlineData("valid")][InlineData("permission")][InlineData("late-revocation")][InlineData("foreign")]
    public async Task DocumentStagingReusesFreshApplicationAuthorAuthority(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;
        foreach(var permission in new[]{PermissionConstants.HSE.IssuePermit,PermissionConstants.HSE.ApprovePermit})
            f.Core.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=permission});
        var grant=await f.Core.Db.Set<AppUserAssetAccess>().SingleAsync(x=>x.AssetType=="FIELD" && x.AssetId=="A");grant.AccessLevel="WRITE";await f.Core.Db.SaveChangesAsync();
        var context=await f.Access.RequireReadScopeAsync();
        var parent=f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local");parent.APPLICANT_ID="Applicant";parent.COUNTRY=Beep.OilandGas.Models.Data.PermitsAndApplications.Country.Kuwait;
        var review=PermitApplicationSourceReview.Capture(context,parent,[]);
        if(mode=="foreign")review=PermitApplicationSourceReview.Capture(context with {Field=context.Field with {ActorUserId="foreign-account"}},parent,[]);
        var original=PermitProviderQualificationTests.StageOriginal(review,new byte[]{0,255,12,42});var entered=0;
        async Task RemovePermission()
        {
            f.Core.Db.RoleClaims.Remove(f.Core.Db.RoleClaims.Single(x=>x.ClaimValue==PermissionConstants.HSE.IssuePermit));await f.Core.Db.SaveChangesAsync();
        }
        if(mode=="permission")await RemovePermission();
        async Task<bool> Execute()
        {
            return await f.Access.WithDocumentStageAuthorityAsync(original,async guard=>
            {
                entered++;await guard();if(mode=="late-revocation")await RemovePermission();await guard();return true;
            });
        }
        if(mode=="valid")
        {
            Assert.True(await Execute());Assert.Equal(1,entered);
            var command=new PermitApplicationCommand(Guid.NewGuid(),review,"SUBMIT","Reviewed existing lifecycle authority after shared extraction");
            Assert.True(await f.Access.WithCommandAuthorityAsync(command,async guard=>{await guard();return true;}));
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(Execute);Assert.Equal(mode=="late-revocation"?1:0,entered);
        }
        Assert.Empty(f.Writes);Assert.DoesNotContain(f.Reads,x=>x is "SOURCE_DOCUMENT" or "RM_INFORMATION_ITEM" or "RM_FILE_CONTENT");
    }
    [Theory][InlineData("valid")][InlineData("permission")][InlineData("missing-module")][InlineData("binding")][InlineData("physical-target")][InlineData("field-retired")][InlineData("source-target")][InlineData("source-name")][InlineData("missing-driver")]
    public async Task PermitScopeUsesCurrentModuleAndRefusesAuthorityOrBindingDrift(string mode)
    {
        using var f=await Ready(mode!="missing-module");f.Core.Http.HttpContext=f.Core.Request;
        if(mode=="missing-module"){await Assert.ThrowsAnyAsync<Exception>(()=>f.Access.RequireReadScopeAsync());Assert.Empty(f.Reads);return;}
        var scope=await f.Access.RequireReadScopeAsync();Assert.Equal("permits-current",scope.Database.ConnectionName);Assert.Equal("A",scope.Field.FieldId);Assert.NotEqual(scope.Field.Database,scope.Database);
        if(mode=="permission"){f.Core.Db.RoleClaims.Remove(f.Core.Db.RoleClaims.Single(x=>x.ClaimValue==PermissionConstants.HSE.ViewPermits));await f.Core.Db.SaveChangesAsync();}
        if(mode=="binding"){f.Binding.ConcurrencyStamp="permit-binding-v2";await f.Core.Db.SaveChangesAsync();}
        if(mode=="physical-target")f.Core.Editor.Object.ConfigEditor.DataConnections.Single(c=>c.ConnectionName=="permits-current").Database="changed-target";
        if(mode=="source-target")f.SourceConnection.Database="unrelated-target";
        if(mode=="source-name")f.Source.SetupGet(x=>x.DatasourceName).Returns("unrelated-name");
        if(mode=="missing-driver")f.Core.Editor.Setup(x=>x.GetDataSource("permits-current")).Returns((IDataSource)null!);
        if(mode=="field-retired")f.Core.Rows.OfType<Beep.OilandGas.PPDM39.Models.FIELD>().Single(x=>x.FIELD_ID=="A").ACTIVE_IND="N";
        if(mode=="valid")Assert.Equal(scope,await f.Access.RequireReadScopeAsync());else await Assert.ThrowsAnyAsync<Exception>(()=>f.Access.RequireReadScopeAsync());
        Assert.DoesNotContain("PPDM39",f.Core.Connections);Assert.Empty(f.Reads);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task NativePermitReadUsesBoundModuleAndRefusesIgnoredFieldFilters(bool ignored)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;f.IgnoreFilters=ignored;
        if(ignored)await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.GetByIdAsync("local"));
        else{var row=await f.Lifecycle.GetByIdAsync("local");Assert.NotNull(row);Assert.Equal("A",row!.FIELD_ID);Assert.Equal("local",row.PERMIT_APPLICATION_ID);Assert.Equal("tenant",row.OWNER_TENANT_ID);Assert.Equal("company",row.OWNER_COMPANY_ID);Assert.Contains("permits-current",f.Core.Connections);Assert.Single(f.Reads);}
        Assert.DoesNotContain("PPDM39",f.Core.Connections);Assert.Empty(f.Core.Writes);
    }
    [Fact]
    public async Task PermitMissingCommandSchemaUsesCanonicalMigrationRefusalBeforeWrites()
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;f.EnableWrites();
        var application=f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local");application.APPLICANT_ID="recorded applicant";
        var original=new PermitApplicationCommand(Guid.NewGuid(),PermitApplicationSourceReview.Capture(await f.Access.RequireReadScopeAsync(),application,f.Rows.OfType<PERMIT_STATUS_HISTORY>().ToArray()),"SUBMIT","Reviewed original source");
        f.Source.Setup(x=>x.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(new EntityStructure{EntityName="PERMIT_APPLICATION",Fields=[]});
        await Assert.ThrowsAsync<NativeSchemaCompatibilityException>(()=>f.Lifecycle.RecordCommandAsync(original));
        Assert.Empty(f.Writes);Assert.Empty(f.Transactions);
    }
    [Theory][InlineData("valid")][InlineData("update-date")][InlineData("history-date")]
    public async Task PermitCommandRejectsInvalidAuditBeforeProviderMutation(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;f.EnableWrites();
        f.Core.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=PermissionConstants.HSE.IssuePermit});
        var grant=await f.Core.Db.Set<AppUserAssetAccess>().SingleAsync(x=>x.AssetType=="FIELD" && x.AssetId=="A");grant.AccessLevel="WRITE";await f.Core.Db.SaveChangesAsync();
        var application=f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local");application.APPLICANT_ID="recorded applicant";
        var original=new PermitApplicationCommand(Guid.NewGuid(),PermitApplicationSourceReview.Capture(await f.Access.RequireReadScopeAsync(),application,f.Rows.OfType<PERMIT_STATUS_HISTORY>().ToArray()),"SUBMIT","Reviewed recorded source");
        var columns=new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler();
        if(mode=="update-date")f.Core.Columns.Setup(x=>x.PrepareForUpdate(It.IsAny<IPPDMEntity>(),It.IsAny<string>())).Callback((IPPDMEntity row,string actor)=>{columns.PrepareForUpdate(row,actor);row.ROW_CHANGED_DATE=DateTime.SpecifyKind(DateTime.MinValue,DateTimeKind.Utc);});
        if(mode=="history-date")f.Core.Columns.Setup(x=>x.PrepareForInsert(It.IsAny<IPPDMEntity>(),It.IsAny<string>())).Callback((IPPDMEntity row,string actor)=>{columns.PrepareForInsert(row,actor);if(row is PERMIT_STATUS_HISTORY)row.ROW_CREATED_DATE=DateTime.SpecifyKind(DateTime.MinValue,DateTimeKind.Utc);});
        if(mode=="valid"){var saved=await f.Lifecycle.RecordCommandAsync(original);saved.Require();Assert.Equal(3,f.Writes.Count);}
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecordCommandAsync(original));
            Assert.Equal(mode=="update-date"?0:1,f.Writes.Count);Assert.Empty(f.Rows.OfType<PERMIT_APPLICATION_COMMAND>());Assert.Single(f.Rows.OfType<PERMIT_STATUS_HISTORY>());
            Assert.Equal(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Draft,f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local").STATUS);
            Assert.Contains("rollback",f.Transactions);
        }
    }
    [Theory][InlineData("PERMIT_APPLICATION")][InlineData("PERMIT_STATUS_HISTORY")][InlineData("PERMIT_APPLICATION_COMMAND")]
    public async Task PermitNativeTablesRequireOwnedCommandBeforeAnyGenericWrite(string table)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;f.EnableWrites();
        var type=new[]{typeof(PERMIT_APPLICATION),typeof(PERMIT_STATUS_HISTORY),typeof(PERMIT_APPLICATION_COMMAND)}.Single(x=>x.Name==table);
        var row=Activator.CreateInstance(type)!;
        var direct=new PPDMGenericRepository(f.Core.Editor.Object,f.Core.Columns.Object,f.Core.Defaults.Object,f.Core.Metadata.Object,type,"permits-current",table);
        var unowned=await Assert.ThrowsAnyAsync<Exception>(()=>direct.InsertAsync(row,"actor"));
        Assert.Contains("command",unowned.Message,StringComparison.OrdinalIgnoreCase);
        var reads=f.Repository(type,type==typeof(PERMIT_APPLICATION)?null:"local");
        var generic=await Assert.ThrowsAnyAsync<Exception>(()=>reads.InsertAsync(row,"actor"));
        Assert.Contains("owned",generic.Message,StringComparison.OrdinalIgnoreCase);
        Assert.Empty(f.Writes);Assert.Empty(f.Transactions);
    }
    [Theory][InlineData("valid")][InlineData("header")][InlineData("history")][InlineData("owner")][InlineData("hash")][InlineData("audit")][InlineData("intent")][InlineData("source")]
    public void PermitCommandReceiptRejectsRewrittenOutcomeOriginalOwnershipAndAudit(string mode)
    {
        var original=new PermitApplicationCommand(Guid.NewGuid(),CommandSource(),"SUBMIT","Reviewed original source");
        var outcome=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);outcome.Require();
        if(mode=="header")
        {
            var application=outcome.ReadApplication();application.APPLICANT_ID="altered applicant";
            Assert.Throws<InvalidOperationException>(()=>(outcome with{ApplicationJson=System.Text.Json.JsonSerializer.Serialize(application)}).Require());return;
        }
        if(mode=="history")
        {
            var history=outcome.ReadHistory();history.STATUS_REMARKS="altered rationale";
            Assert.Throws<InvalidOperationException>(()=>(outcome with{HistoryJson=System.Text.Json.JsonSerializer.Serialize(history)}).Require());return;
        }
        var record=PERMIT_APPLICATION_COMMAND.Create(outcome);
        if(mode=="owner")record.OWNER_COMPANY_ID="another-company";
        if(mode=="hash")record.OUTCOME_JSON+=" ";
        if(mode=="audit")record.ROW_CREATED_BY="another-account";
        if(mode=="intent")record.PERMIT_APPLICATION_COMMAND_ID=Guid.NewGuid().ToString("N");
        if(mode=="source")record.SOURCE_REVISION=new string('0',64);
        if(mode=="valid")Assert.Equal(outcome,record.Restore(original.Review.Context));
        else Assert.Throws<InvalidOperationException>(()=>record.Restore(original.Review.Context));
    }
    [Theory][InlineData("submit")][InlineData("approve")][InlineData("reject")][InlineData("fail-update")][InlineData("fail-history")][InlineData("fail-record")][InlineData("lost-ack")][InlineData("late-permission")][InlineData("stale-source")][InlineData("changed-history")][InlineData("conflicting-original")][InlineData("schema")]
    public async Task PermitNativeCommandPersistsOriginalAuditAndRecoversOnlyItsExactEffect(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;f.EnableWrites();
        foreach(var permission in new[]{PermissionConstants.HSE.IssuePermit,PermissionConstants.HSE.ApprovePermit})
            f.Core.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=permission});
        var grant=await f.Core.Db.Set<AppUserAssetAccess>().SingleAsync(x=>x.AssetType=="FIELD" && x.AssetId=="A");grant.AccessLevel="WRITE";await f.Core.Db.SaveChangesAsync();
        var application=f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local");application.APPLICANT_ID="recorded applicant";application.ROW_CREATED_BY="issuer";
        if(mode is "approve" or "reject"){application.STATUS=Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted;application.SUBMITTED_BY_USER_ID="issuer";application.SUBMITTED_DATE=DateTime.UtcNow.AddDays(-1);}
        var review=PermitApplicationSourceReview.Capture(await f.Access.RequireReadScopeAsync(),application,f.Rows.OfType<PERMIT_STATUS_HISTORY>().ToArray());
        var original=new PermitApplicationCommand(Guid.NewGuid(),review,mode=="approve"?"APPROVE":mode=="reject"?"REJECT":"SUBMIT","Reviewed original source evidence");original.Require();
        f.FailWriteNumber=mode=="fail-update"?1:mode=="fail-history"?2:mode=="fail-record"?3:0;f.LoseCommitAck=mode=="lost-ack";
        if(mode=="late-permission")f.AfterWrite=row=>{if(row is PERMIT_APPLICATION){f.Core.Db.RoleClaims.Remove(f.Core.Db.RoleClaims.Single(x=>x.ClaimValue==PermissionConstants.HSE.IssuePermit));f.Core.Db.SaveChanges();}};
        if(mode=="stale-source")application.REMARKS="Changed after original retained";
        if(mode=="schema")
        {
            f.Source.Setup(x=>x.GetEntityStructure(It.Is<EntityStructure>(r=>r.EntityName==nameof(PERMIT_APPLICATION_COMMAND)),true)).Returns((EntityStructure request,bool _) =>
            {
                var shape=new TheTechIdea.Beep.Tools.ClassCreator(f.Core.Editor.Object).ConvertToEntityStructure(typeof(PERMIT_APPLICATION_COMMAND));
                foreach(var field in shape.Fields){field.IsKey=field.FieldName=="PERMIT_APPLICATION_COMMAND_ID";if(field.Fieldtype==typeof(string).FullName)field.Size=field.Size1=field.FieldName=="OUTCOME_JSON"?100:8388608;}return shape;
            });
        }
        if(mode.StartsWith("fail-",StringComparison.Ordinal) || mode is "late-permission" or "stale-source" or "schema")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecordCommandAsync(original));
            Assert.Empty(f.Rows.OfType<PERMIT_APPLICATION_COMMAND>());Assert.Single(f.Rows.OfType<PERMIT_STATUS_HISTORY>());
            Assert.Equal(original.Review.ReadApplication().STATUS,f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local").STATUS);
            Assert.Contains("rollback",f.Transactions);return;
        }
        PermitApplicationCommandOutcome? outcome=null;
        if(mode=="lost-ack")await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecordCommandAsync(original));else outcome=await f.Lifecycle.RecordCommandAsync(original);
        Assert.Equal(3,f.Writes.Count);Assert.Single(f.Rows.OfType<PERMIT_APPLICATION_COMMAND>());Assert.Equal(2,f.Rows.OfType<PERMIT_STATUS_HISTORY>().Count());
        var before=f.Writes.Count;
        if(mode=="lost-ack")
        {
            var connection=f.Core.Editor.Object.ConfigEditor.DataConnections.Single(x=>x.ConnectionName=="permits-current");
            f.Core.Db.RoleClaims.Remove(f.Core.Db.RoleClaims.Single(x=>x.ClaimValue==PermissionConstants.HSE.IssuePermit));await f.Core.Db.SaveChangesAsync();
            await f.Core.Select("company");f.Core.Http.HttpContext=f.Core.Request;f.Core.Editor.Object.ConfigEditor.DataConnections.Add(connection);
            f.Access=new(f.Core.Gate,new ModuleConnectionResolver(f.Core.Db,f.Core.Editor.Object,f.Core.CompanySelection),CompanyCorrectiveActionAuthorityTests.Profile(f.Core),f.Core.Editor.Object,f.Core.Columns.Object,f.Core.Defaults.Object,f.Core.Metadata.Object,new RepositoryAssetAccessStore(f.Core.Db,f.Core.Gate,Mock.Of<IAssetGrantTargetVerifier>()),f.Core.Policy);
            outcome=await f.Lifecycle.RecoverCommandAsync(original);
        }
        Assert.NotNull(outcome);outcome.Require();Assert.Equal(original,outcome.Original);
        Assert.Equal(mode=="approve"?"Approved":mode=="reject"?"Rejected":"Submitted",outcome.ReadApplication().STATUS.ToString());Assert.Equal("actor",outcome.ReadHistory().UPDATED_BY);Assert.Equal(original.Reason,outcome.ReadHistory().STATUS_REMARKS);
        if(mode=="changed-history")
        {
            f.Rows.OfType<PERMIT_STATUS_HISTORY>().Single(x=>x.PERMIT_STATUS_HISTORY_ID==outcome.ReadHistory().PERMIT_STATUS_HISTORY_ID).STATUS_REMARKS="altered";
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecoverCommandAsync(original));
        }
        else if(mode=="conflicting-original")await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecordCommandAsync(original with{Reason="different original"}));
        else Assert.Equal(outcome,await f.Lifecycle.RecordCommandAsync(original));
        Assert.Equal(before,f.Writes.Count);Assert.Null(f.ActiveTransaction);
    }
    [Theory][InlineData("header")][InlineData("classification")][InlineData("dependency")][InlineData("stale")][InlineData("rollback")][InlineData("lost-ack")][InlineData("narrow-schema")]
    public async Task ReviewedPermitDraftNativeCommandChecksDependenciesAndPreservesAtomicOriginal(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;f.EnableWrites();
        f.Core.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=PermissionConstants.HSE.IssuePermit});
        var grant=await f.Core.Db.Set<AppUserAssetAccess>().SingleAsync(x=>x.AssetType=="FIELD" && x.AssetId=="A");grant.AccessLevel="WRITE";await f.Core.Db.SaveChangesAsync();
        foreach(var type in new[]{typeof(DRILLING_PERMIT_APPLICATION),typeof(ENVIRONMENTAL_PERMIT_APPLICATION),typeof(APPLICATION_ATTACHMENT)})
        {
            var key=type==typeof(APPLICATION_ATTACHMENT)?"APPLICATION_ATTACHMENT_ID":"PERMIT_APPLICATION_ID";
            f.Core.Metadata.Setup(x=>x.GetTableMetadataAsync(type.Name)).ReturnsAsync(new PPDMTableMetadata{EntityTypeName=type.FullName!,PrimaryKeyColumn=key});
            f.Core.Metadata.Setup(x=>x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync(new[]{key});
        }
        if(mode=="classification")f.Rows.RemoveAll(x=>x is INJECTION_PERMIT_APPLICATION or MIT_RESULT);
        var row=f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local");row.APPLICANT_ID="recorded applicant";
        var review=await f.Lifecycle.ReviewSourceAsync("local");var source=review.ReadApplication();
        var changes=new PermitApplicationDraftChanges(source.APPLICATION_TYPE,source.REGULATORY_AUTHORITY,source.COUNTRY,source.STATE_PROVINCE,"Revised applicant","Revised scope");
        if(mode is "classification" or "dependency")changes=changes with{Country=Beep.OilandGas.Models.Data.PermitsAndApplications.Country.Kuwait};
        // The seeded country may already equal Kuwait; choose a different defined non-Other country explicitly.
        if(mode is "classification" or "dependency")changes=changes with{Country=Enum.GetValues<Beep.OilandGas.Models.Data.PermitsAndApplications.Country>().First(x=>x!=source.COUNTRY && x!=Beep.OilandGas.Models.Data.PermitsAndApplications.Country.Other)};
        var original=new PermitApplicationCommand(Guid.NewGuid(),review,"EDIT_DRAFT","Reviewed the exact draft source before changing its header."){DraftChanges=changes};
        if(mode=="stale")row.REMARKS="Changed source";
        if(mode=="rollback")f.FailWriteNumber=3;
        if(mode=="lost-ack")f.LoseCommitAck=true;
        if(mode=="narrow-schema")
            f.Source.Setup(x=>x.GetEntityStructure(It.Is<EntityStructure>(r=>r.EntityName==nameof(PERMIT_APPLICATION)),true)).Returns((EntityStructure _,bool __)=>
            {
                var shape=new TheTechIdea.Beep.Tools.ClassCreator(f.Core.Editor.Object).ConvertToEntityStructure(typeof(PERMIT_APPLICATION));
                foreach(var field in shape.Fields){field.IsKey=field.FieldName=="PERMIT_APPLICATION_ID";if(field.Fieldtype==typeof(string).FullName)field.Size=field.Size1=field.FieldName=="APPLICANT_ID"?3:8388608;}return shape;
            });
        if(mode is "dependency" or "stale" or "rollback" or "narrow-schema")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecordCommandAsync(original));
            Assert.Empty(f.Rows.OfType<PERMIT_APPLICATION_COMMAND>());Assert.Single(f.Rows.OfType<PERMIT_STATUS_HISTORY>());
            Assert.Equal(source.APPLICANT_ID,f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local").APPLICANT_ID);
            if(mode!="rollback")Assert.Empty(f.Writes);Assert.Contains("rollback",f.Transactions);return;
        }
        if(mode=="lost-ack")await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecordCommandAsync(original));
        else await f.Lifecycle.RecordCommandAsync(original);
        Assert.Equal(3,f.Writes.Count);var recovered=await f.Lifecycle.RecoverCommandAsync(original);Assert.NotNull(recovered);Assert.Equal(original,recovered.Original);
        var saved=recovered.ReadApplication();Assert.Equal(changes.Applicant,saved.APPLICANT_ID);Assert.Equal(changes.Country,saved.COUNTRY);Assert.Equal(source.STATUS,saved.STATUS);
        Assert.Equal(source.FIELD_ID,saved.FIELD_ID);Assert.Equal(source.OWNER_COMPANY_ID,saved.OWNER_COMPANY_ID);Assert.Equal(source.SUBMITTED_DATE,saved.SUBMITTED_DATE);
        Assert.Equal(recovered,await f.Lifecycle.RecordCommandAsync(original));Assert.Equal(3,f.Writes.Count);Assert.Single(f.Rows.OfType<PERMIT_APPLICATION_COMMAND>());Assert.Equal(2,f.Rows.OfType<PERMIT_STATUS_HISTORY>().Count());
    }
    [Theory][InlineData("field-only")][InlineData("well")][InlineData("facility")][InlineData("foreign-well")][InlineData("foreign-facility-type")][InlineData("readonly")][InlineData("permission")][InlineData("physical-schema")][InlineData("metadata")][InlineData("source-drift")]
    public async Task PermitCreationReviewUsesCurrentNativeCompanyFieldAndCompleteResourceKeys(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;
        if(mode!="permission")f.Core.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=PermissionConstants.HSE.IssuePermit});
        var grant=await f.Core.Db.Set<AppUserAssetAccess>().SingleAsync(x=>x.AssetType=="FIELD" && x.AssetId=="A");grant.AccessLevel=mode=="readonly"?"READ":"WRITE";await f.Core.Db.SaveChangesAsync();
        var well=new Beep.OilandGas.PPDM39.Models.WELL{UWI="well",ASSIGNED_FIELD=mode=="foreign-well"?"B":"A",OPERATOR="company",ACTIVE_IND="Y"};
        f.Core.Rows.AddRange([well,new Beep.OilandGas.PPDM39.Models.FACILITY{FACILITY_ID="shared",FACILITY_TYPE="gas",PRIMARY_FIELD_ID="A",CURRENT_OPERATOR="company",ACTIVE_IND="Y"},new Beep.OilandGas.PPDM39.Models.FACILITY{FACILITY_ID="shared",FACILITY_TYPE="oil",PRIMARY_FIELD_ID="B",CURRENT_OPERATOR="company",ACTIVE_IND="Y"}]);
        f.Core.Source.Setup(x=>x.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool _)=>
        {
            var type=new[]{typeof(Beep.OilandGas.PPDM39.Models.FIELD),typeof(Beep.OilandGas.PPDM39.Models.WELL),typeof(Beep.OilandGas.PPDM39.Models.FACILITY)}.Single(x=>x.Name==request.EntityName);
            var shape=new TheTechIdea.Beep.Tools.ClassCreator(f.Core.Editor.Object).ConvertToEntityStructure(type);
            if(mode=="physical-schema" && type==typeof(Beep.OilandGas.PPDM39.Models.WELL))shape.Fields.RemoveAll(x=>x.FieldName=="ASSIGNED_FIELD");return shape;
        });
        var header=new PermitApplicationDraftChanges(default,default,Beep.OilandGas.Models.Data.PermitsAndApplications.Country.Kuwait,default,"Applicant",null);
        var facility=mode is "facility" or "foreign-facility-type";
        var proposal=new PermitApplicationCreationProposal(header,mode=="field-only" || facility?null:"well",facility?"shared":null,facility?(mode=="facility"?"gas":"oil"):null);
        if(mode=="metadata")
        {var definition=await f.Core.Metadata.Object.GetTableMetadataAsync("WELL");definition!.ForeignKeys.Clear();}
        var wellReads=0;
        if(mode=="source-drift")f.Core.AfterRead=()=>
        {if(f.Core.Queries.LastOrDefault()?.Any(x=>x.FieldName=="UWI")==true && ++wellReads==2)well.ROW_CHANGED_BY="changed-during-review";return Task.CompletedTask;};
        if(mode is "field-only" or "well" or "facility")
        {
            var review=await f.Access.ReviewCreationAsync(proposal);review.RequireFor(proposal,(await f.Access.RequireReadScopeAsync()).Field);
            Assert.Equal("A",review.Context.Field.FieldId);Assert.Contains(facility?"gas":mode=="well"?"well":"FIELD",review.LinkedSourcesJson);
        }
        else
        {
            var error=await Assert.ThrowsAnyAsync<Exception>(()=>f.Access.ReviewCreationAsync(proposal));
            if(mode=="physical-schema")Assert.IsType<NativeSchemaCompatibilityException>(error);
            else Assert.IsType<Beep.OilandGas.Models.Core.Refusals.RefusalException>(error);
            if(mode=="source-drift")Assert.Contains("resources changed",error.Message);
        }
        Assert.Empty(f.Core.Writes);Assert.Empty(f.Writes);Assert.Empty(f.Transactions);
    }
    [Theory][InlineData("created")][InlineData("linked")][InlineData("existing-id")][InlineData("stale-resources")][InlineData("fail-receipt")][InlineData("lost-ack")][InlineData("late-resources")][InlineData("missing-facility-column")][InlineData("foreign-receipt")]
    public async Task PermitNativeCreationOwnsDraftHistoryReceiptAndSameOriginalRecovery(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;f.EnableWrites();
        f.Core.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=PermissionConstants.HSE.IssuePermit});
        var grant=await f.Core.Db.Set<AppUserAssetAccess>().SingleAsync(x=>x.AssetType=="FIELD" && x.AssetId=="A");grant.AccessLevel="WRITE";await f.Core.Db.SaveChangesAsync();
        f.Core.Source.Setup(x=>x.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool _)=>
        {
            var type=new[]{typeof(Beep.OilandGas.PPDM39.Models.FIELD),typeof(Beep.OilandGas.PPDM39.Models.WELL),typeof(Beep.OilandGas.PPDM39.Models.FACILITY)}.Single(x=>x.Name==request.EntityName);
            return new TheTechIdea.Beep.Tools.ClassCreator(f.Core.Editor.Object).ConvertToEntityStructure(type);
        });
        f.Core.Rows.AddRange([new Beep.OilandGas.PPDM39.Models.WELL{UWI="well",ASSIGNED_FIELD="A",OPERATOR="company",ACTIVE_IND="Y"},new Beep.OilandGas.PPDM39.Models.FACILITY{FACILITY_ID="shared",FACILITY_TYPE="gas",PRIMARY_FIELD_ID="A",CURRENT_OPERATOR="company",ACTIVE_IND="Y"}]);
        var header=new PermitApplicationDraftChanges(default,default,Beep.OilandGas.Models.Data.PermitsAndApplications.Country.Kuwait,default,"Applicant","Proposed draft");
        var proposal=new PermitApplicationCreationProposal(header,mode=="linked"?"well":null,mode=="linked"?"shared":null,mode=="linked"?"gas":null);
        var creation=await f.Access.ReviewCreationAsync(proposal);var original=creation.PrepareOriginal(Guid.NewGuid(),"Reviewed the complete creation proposal and native resource evidence.");
        var id=original.Review.ReadApplication().PERMIT_APPLICATION_ID;
        Assert.Null(await f.Lifecycle.RecoverCommandAsync(original));Assert.Empty(f.Writes);
        if(mode=="existing-id")f.Rows.Add(original.Review.ReadApplication());
        var field=f.Core.Rows.OfType<Beep.OilandGas.PPDM39.Models.FIELD>().Single(x=>x.FIELD_ID=="A");
        if(mode=="stale-resources")field.ROW_CHANGED_BY="changed-after-review";
        if(mode=="fail-receipt")f.FailWriteNumber=3;if(mode=="lost-ack")f.LoseCommitAck=true;
        if(mode=="late-resources")f.AfterWrite=row=>{if(row is PERMIT_APPLICATION)field.ROW_CHANGED_BY="changed-during-module-write";};
        if(mode=="missing-facility-column")
            f.Source.Setup(x=>x.GetEntityStructure(It.Is<EntityStructure>(r=>r.EntityName==nameof(PERMIT_APPLICATION)),true)).Returns((EntityStructure _,bool __)=>
            {
                var shape=new TheTechIdea.Beep.Tools.ClassCreator(f.Core.Editor.Object).ConvertToEntityStructure(typeof(PERMIT_APPLICATION));
                shape.Fields.RemoveAll(x=>x.FieldName=="FACILITY_TYPE");foreach(var column in shape.Fields){column.IsKey=column.FieldName=="PERMIT_APPLICATION_ID";if(column.Fieldtype==typeof(string).FullName)column.Size=column.Size1=8388608;}return shape;
            });
        if(mode=="foreign-receipt")
        {
            var receipt=PERMIT_APPLICATION_COMMAND.Create(PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow));receipt.OWNER_COMPANY_ID="foreign";f.Rows.Add(receipt);f.IgnoreFilters=true;f.IgnoreFilterTable=nameof(PERMIT_APPLICATION_COMMAND);
            await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>f.Lifecycle.RecoverCommandAsync(original));Assert.Empty(f.Writes);return;
        }
        f.Core.AfterRead=()=>{Assert.Null(f.ActiveTransaction);return Task.CompletedTask;};
        if(mode is "existing-id" or "stale-resources" or "fail-receipt" or "missing-facility-column")
        {
            var error=await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecordCommandAsync(original));
            if(mode=="missing-facility-column")Assert.IsType<NativeSchemaCompatibilityException>(error);
            if(mode is "existing-id" or "stale-resources")Assert.IsType<Beep.OilandGas.Models.Core.Refusals.RefusalException>(error);
            Assert.Empty(f.Rows.OfType<PERMIT_APPLICATION_COMMAND>());Assert.DoesNotContain(f.Rows.OfType<PERMIT_STATUS_HISTORY>(),x=>x.PERMIT_APPLICATION_ID==id);
            Assert.Equal(mode=="existing-id"?1:0,f.Rows.OfType<PERMIT_APPLICATION>().Count(x=>x.PERMIT_APPLICATION_ID==id));
            if(mode!="fail-receipt")Assert.Empty(f.Writes);else{Assert.Equal(3,f.Writes.Count);Assert.Contains("rollback",f.Transactions);}return;
        }
        if(mode is "lost-ack" or "late-resources")await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecordCommandAsync(original));
        else await f.Lifecycle.RecordCommandAsync(original);
        Assert.Equal(3,f.Writes.Count);Assert.Contains("commit",f.Transactions);
        var outcome=await f.Lifecycle.RecoverCommandAsync(original);Assert.NotNull(outcome);Assert.Equal(original,outcome.Original);var saved=outcome.ReadApplication();
        Assert.Equal("Draft",saved.STATUS.ToString());Assert.Equal("actor",saved.ROW_CREATED_BY);Assert.Equal(proposal.FacilityType,saved.FACILITY_TYPE);Assert.Equal(proposal.WellId,saved.WELL_ID);
        Assert.Equal(outcome,await f.Lifecycle.RecordCommandAsync(original));Assert.Equal(3,f.Writes.Count);
        Assert.Single(f.Rows.OfType<PERMIT_APPLICATION>(),x=>x.PERMIT_APPLICATION_ID==id);Assert.Single(f.Rows.OfType<PERMIT_STATUS_HISTORY>(),x=>x.PERMIT_APPLICATION_ID==id);Assert.Single(f.Rows.OfType<PERMIT_APPLICATION_COMMAND>());
    }
    [Theory][InlineData("author")][InlineData("reviewer")][InlineData("readonly")][InlineData("no-command-permission")][InlineData("late-permission")][InlineData("provider-error")][InlineData("missing-status-column")]
    public async Task PermitInboxCapabilitiesUseCurrentAppPermissionsAndNativeFieldWriteAccess(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;
        foreach(var permission in mode=="no-command-permission"?Array.Empty<string>():mode=="reviewer"?new[]{PermissionConstants.HSE.ApprovePermit}:mode=="readonly"?new[]{PermissionConstants.HSE.IssuePermit,PermissionConstants.HSE.ApprovePermit}:new[]{PermissionConstants.HSE.IssuePermit})
            f.Core.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=permission});
        var grant=await f.Core.Db.Set<AppUserAssetAccess>().SingleAsync(x=>x.AssetType=="FIELD" && x.AssetId=="A");grant.AccessLevel=mode=="readonly"?"READ":"WRITE";await f.Core.Db.SaveChangesAsync();
        if(mode=="late-permission")f.Core.AfterRead=()=>{var permission=f.Core.Db.RoleClaims.SingleOrDefault(x=>x.ClaimValue==PermissionConstants.HSE.IssuePermit);if(permission is not null){f.Core.Db.RoleClaims.Remove(permission);f.Core.Db.SaveChanges();}return Task.CompletedTask;};
        f.Source.Setup(x=>x.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool _)=>
        {
            var shape=new TheTechIdea.Beep.Tools.ClassCreator(f.Core.Editor.Object).ConvertToEntityStructure(typeof(PERMIT_APPLICATION));
            if(mode=="missing-status-column")shape.Fields.RemoveAll(column=>column.FieldName=="STATUS");return shape;
        });
        if(mode=="missing-status-column")
        {await Assert.ThrowsAsync<NativeSchemaCompatibilityException>(()=>f.Lifecycle.GetApplicationsAsync());Assert.Empty(f.Reads);Assert.Empty(f.Writes);return;}
        if(mode=="provider-error")
        {f.Source.SetupGet(x=>x.ErrorObject).Returns(new ErrorsInfo{Flag=Errors.Failed});await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Lifecycle.GetApplicationsAsync());Assert.Empty(f.Writes);return;}
        if(mode=="late-permission")
        {
            await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>f.Access.ReviewInboxAuthorityAsync());
            Assert.Empty(f.Writes);Assert.Empty(f.Core.Writes);return;
        }
        else {var authority=await f.Access.ReviewInboxAuthorityAsync();Assert.Equal(mode=="author",authority.CanAuthor);Assert.Equal(mode=="reviewer",authority.CanReview);Assert.Equal("A",authority.Context.Field.FieldId);}
        var rows=await f.Lifecycle.GetApplicationsAsync();Assert.Single(rows);Assert.Equal("local",rows.Single().PERMIT_APPLICATION_ID);Assert.Single(await f.Lifecycle.GetByStatusAsync(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Draft));Assert.Empty(f.Writes);Assert.Empty(f.Core.Writes);
    }
    [Theory][InlineData("cycle")][InlineData("fail-receipt")][InlineData("lost-ack")]
    public async Task PermitCorrectionNativeReturnEditAndResubmitKeepExactOwnedReceipts(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;f.EnableWrites();
        foreach(var permission in new[]{PermissionConstants.HSE.ApprovePermit,PermissionConstants.HSE.IssuePermit})f.Core.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=permission});
        var grant=await f.Core.Db.Set<AppUserAssetAccess>().SingleAsync(x=>x.AssetType=="FIELD" && x.AssetId=="A");grant.AccessLevel="WRITE";await f.Core.Db.SaveChangesAsync();
        var row=f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local");row.STATUS=Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted;row.COUNTRY=Beep.OilandGas.Models.Data.PermitsAndApplications.Country.Kuwait;row.APPLICANT_ID="Applicant";row.SUBMITTED_BY_USER_ID="issuer";row.SUBMITTED_DATE=DateTime.UtcNow.AddDays(-1);row.SUBMISSION_COMPLETE=true;row.SUBMISSION_COMPLETE_IND="Y";
        var original=new PermitApplicationCommand(Guid.NewGuid(),await f.Lifecycle.ReviewSourceAsync("local"),"RETURN_FOR_CORRECTION","Independent reviewer requests the explicit missing application information.");
        f.FailWriteNumber=mode=="fail-receipt"?3:0;f.LoseCommitAck=mode=="lost-ack";
        if(mode=="fail-receipt")
        {await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecordCommandAsync(original));Assert.Empty(f.Rows.OfType<PERMIT_APPLICATION_COMMAND>());Assert.Single(f.Rows.OfType<PERMIT_STATUS_HISTORY>());Assert.Equal("Submitted",f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local").STATUS.ToString());Assert.Contains("rollback",f.Transactions);return;}
        if(mode=="lost-ack")await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.RecordCommandAsync(original));else await f.Lifecycle.RecordCommandAsync(original);
        var returned=await f.Lifecycle.RecoverCommandAsync(original);Assert.NotNull(returned);Assert.Equal("AdditionalInformationRequired",returned.ReadApplication().STATUS.ToString());Assert.Equal("ADDITIONAL_INFO_REQUIRED",returned.ReadHistory().STATUS);Assert.Equal("issuer",returned.ReadApplication().SUBMITTED_BY_USER_ID);
        Assert.Equal(returned,await f.Lifecycle.RecordCommandAsync(original));Assert.Equal(3,f.Writes.Count);
        if(mode=="lost-ack")return;
        Assert.Single(await f.Lifecycle.GetByStatusAsync(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.AdditionalInformationRequired));
        var review=await f.Lifecycle.ReviewSourceAsync("local");var source=review.ReadApplication();
        var edit=new PermitApplicationCommand(Guid.NewGuid(),review,"EDIT_DRAFT","Author supplied the information requested by the independent reviewer."){DraftChanges=new(source.APPLICATION_TYPE,source.REGULATORY_AUTHORITY,source.COUNTRY,source.STATE_PROVINCE,"Updated applicant","Requested details supplied")};
        await f.Lifecycle.RecordCommandAsync(edit);var submit=new PermitApplicationCommand(Guid.NewGuid(),await f.Lifecycle.ReviewSourceAsync("local"),"SUBMIT","Author resubmits the reviewed corrected application.");
        var submitted=await f.Lifecycle.RecordCommandAsync(submit);Assert.Equal("Submitted",submitted.ReadApplication().STATUS.ToString());Assert.Equal("actor",submitted.ReadApplication().SUBMITTED_BY_USER_ID);Assert.Null(submitted.ReadApplication().DECISION);Assert.True(submitted.ReadApplication().SUBMISSION_COMPLETE);
        Assert.Equal(9,f.Writes.Count);Assert.Equal(3,f.Rows.OfType<PERMIT_APPLICATION_COMMAND>().Count());Assert.Equal(4,f.Rows.OfType<PERMIT_STATUS_HISTORY>().Count());Assert.Equal(returned,await f.Lifecycle.RecoverCommandAsync(original));
        var self=new PermitApplicationCommand(Guid.NewGuid(),await f.Lifecycle.ReviewSourceAsync("local"),"APPROVE","An author cannot approve their own newly resubmitted application.");await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Lifecycle.RecordCommandAsync(self));Assert.Equal(9,f.Writes.Count);
    }
    [Theory][InlineData("valid")][InlineData("permission-before")][InlineData("grant-before")][InlineData("late-permission")][InlineData("late-grant")][InlineData("late-selection")][InlineData("late-binding")][InlineData("late-native-field")][InlineData("cross-source")]
    public async Task PermitCommandAuthorityKeepsCoreReadsOutsideOwnedModuleTransaction(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;
        f.Core.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=PermissionConstants.HSE.IssuePermit});
        var grant=await f.Core.Db.Set<AppUserAssetAccess>().SingleAsync(x=>x.AssetType=="FIELD" && x.AssetId=="A");grant.AccessLevel="WRITE";await f.Core.Db.SaveChangesAsync();
        var application=f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local");application.APPLICANT_ID="recorded applicant";
        var review=PermitApplicationSourceReview.Capture(await f.Access.RequireReadScopeAsync(),application,f.Rows.OfType<PERMIT_STATUS_HISTORY>().ToArray());
        var original=new PermitApplicationCommand(Guid.NewGuid(),review,"SUBMIT","Reviewed source for submission");original.Require();
        if(mode=="permission-before"){f.Core.Db.RoleClaims.Remove(f.Core.Db.RoleClaims.Single(x=>x.ClaimValue==PermissionConstants.HSE.IssuePermit));await f.Core.Db.SaveChangesAsync();}
        if(mode=="grant-before"){grant.AccessLevel="READ";await f.Core.Db.SaveChangesAsync();}
        var entered=false;
        async Task Run()=>await f.Access.WithCommandAuthorityAsync(original,async guard=>
        {
            entered=true;var coreReads=f.Core.Queries.Count+f.Core.EligibilityQueries.Count+f.Core.LeaseQueries.Count;
            await PPDMDataSourceTransaction.ExecuteAsync(f.Source.Object,async()=>
            {
                PPDMDataSourceTransaction.RequireOwned(f.Source.Object,System.Data.IsolationLevel.Serializable);
                await guard();Assert.NotNull(await f.Lifecycle.GetByIdAsync("local"));
                if(mode=="cross-source")
                    await new PPDMGenericRepository(f.Core.Editor.Object,f.Core.Columns.Object,f.Core.Defaults.Object,f.Core.Metadata.Object,
                        typeof(Beep.OilandGas.PPDM39.Models.FIELD),review.Context.Field.Database.ConnectionName,"FIELD",rowAccess:f.Core.Policy).GetByIdAsync("A");
                Assert.Equal(coreReads,f.Core.Queries.Count+f.Core.EligibilityQueries.Count+f.Core.LeaseQueries.Count);
                if(mode=="late-permission")f.Core.Db.RoleClaims.Remove(f.Core.Db.RoleClaims.Single(x=>x.ClaimValue==PermissionConstants.HSE.IssuePermit));
                if(mode=="late-grant")grant.AccessLevel="READ";
                if(mode=="late-selection"){var account=await f.Core.Db.Users.SingleAsync(x=>x.Id=="actor");account.ConcurrencyStamp="changed-selection";}
                if(mode=="late-binding")f.Binding.ConcurrencyStamp="changed-permit-binding";
                if(mode=="late-native-field")f.Core.Rows.OfType<Beep.OilandGas.PPDM39.Models.FIELD>().Single(x=>x.FIELD_ID=="A").ACTIVE_IND="N";
                await f.Core.Db.SaveChangesAsync();await guard();
            });
            return "confirmed";
        });
        if(mode=="valid")await Run();else await Assert.ThrowsAnyAsync<Exception>(Run);
        if(mode.EndsWith("-before",StringComparison.Ordinal)){Assert.False(entered);Assert.Empty(f.Transactions);}
        else if(mode is "valid" or "late-native-field")Assert.Equal(new[]{"begin","commit"},f.Transactions);
        else Assert.Equal(new[]{"begin","rollback"},f.Transactions);
        Assert.Null(f.ActiveTransaction);Assert.Empty(f.Core.Writes);
    }
    [Theory][InlineData("valid")][InlineData("changed-header")][InlineData("changed-history")]
    public async Task PermitSourceReviewFreezesExactEvidenceAndRefusesMixedReads(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;
        if(mode!="valid")f.AfterRead=()=>
        {
            if(f.Reads.Count(x=>x=="PERMIT_STATUS_HISTORY")==2)
            {
                f.AfterRead=null;
                if(mode=="changed-header")f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local").REMARKS="changed after initial review";
                else f.Rows.OfType<PERMIT_STATUS_HISTORY>().Single().STATUS_REMARKS="changed after initial review";
            }
            return Task.CompletedTask;
        };
        if(mode!="valid")await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.ReviewSourceAsync("local"));
        else
        {
            var review=await f.Lifecycle.ReviewSourceAsync("local");review.Require();
            Assert.Equal("local",review.ReadApplication().PERMIT_APPLICATION_ID);Assert.Single(review.ReadHistory());
            var application=review.ReadApplication();application.REMARKS="local edit";
            var history=review.ReadHistory();history[0].STATUS_REMARKS="local edit";
            Assert.NotEqual("local edit",review.ReadApplication().REMARKS);Assert.NotEqual("local edit",review.ReadHistory()[0].STATUS_REMARKS);
            Assert.Equal("permits-current",review.Context.Database.ConnectionName);
        }
        Assert.Empty(f.Core.Writes);
    }
    private static PermitApplicationSourceReview CommandSource()
    {
        var context=new PermitApplicationDataScope(new(new("tenant","company"),new("core","core-fingerprint"),"reviewer","selection-v1","A"),new("permits-current","permit-binding"));
        return PermitApplicationSourceReview.Capture(context,new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="local",FIELD_ID="A",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",APPLICANT_ID="recorded applicant",COUNTRY=Beep.OilandGas.Models.Data.PermitsAndApplications.Country.Kuwait,ACTIVE_IND="Y"},
            [new PERMIT_STATUS_HISTORY{PERMIT_STATUS_HISTORY_ID="history",PERMIT_APPLICATION_ID="local",ACTIVE_IND="Y"}]);
    }
    [Theory][InlineData("company")][InlineData("tenant")][InlineData("actor")]
    public void PermitOriginalScopeRefusesMissingCompanyTenantOrActor(string mode)
    {
        var review=CommandSource();var field=review.Context.Field;
        if(mode=="company")field=field with{Company=null!};
        if(mode=="tenant")field=field with{Company=field.Company with{TenantId=""}};
        if(mode=="actor")field=field with{ActorUserId=""};
        Assert.Throws<InvalidOperationException>(()=>(review.Context with{Field=field}).Require());
    }
    [Theory][InlineData("valid")][InlineData("payload")][InlineData("history")][InlineData("actor")][InlineData("database")][InlineData("company")][InlineData("field")][InlineData("duplicate-history")]
    public void PermitSourceOriginalBindsCompletePayloadHistoryAndContext(string mode)
    {
        var original=CommandSource();var changed=original;
        if(mode=="payload")changed=changed with{ApplicationJson=changed.ApplicationJson.Replace("recorded applicant","edited applicant",StringComparison.Ordinal)};
        if(mode=="history")changed=changed with{HistoryJson="[]"};
        if(mode=="actor")changed=changed with{Context=changed.Context with{Field=changed.Context.Field with{ActorUserId="another-account"}}};
        if(mode=="database")changed=changed with{Context=changed.Context with{Database=new("other","other-binding")}};
        if(mode=="company")changed=changed with{Context=changed.Context with{Field=changed.Context.Field with{Company=new("tenant","another-company")}}};
        if(mode=="field")changed=changed with{Context=changed.Context with{Field=changed.Context.Field with{FieldId="B"}}};
        if(mode=="duplicate-history")
        {
            var row=original.ReadHistory()[0];
            Assert.Throws<InvalidOperationException>(()=>PermitApplicationSourceReview.Capture(original.Context,original.ReadApplication(),[row,row]));return;
        }
        if(mode=="valid")
        {
            original.RequireCurrent(changed);
            var copy=System.Text.Json.JsonSerializer.Deserialize<PermitApplicationSourceReview>(System.Text.Json.JsonSerializer.Serialize(original))!;
            original.RequireCurrent(copy);
            Assert.Equal(original,PermitApplicationSourceReview.Capture(original.Context,original.ReadApplication(),original.ReadHistory()));
        }
        else Assert.ThrowsAny<Exception>(()=>original.RequireCurrent(changed));
    }
    [Theory][InlineData("submit")][InlineData("approve")][InlineData("reject")][InlineData("self")][InlineData("missing-submitter")][InlineData("terminal")][InlineData("reason")][InlineData("intent")][InlineData("action")][InlineData("unknown-country")]
    public void PermitOriginalRequiresRecordedSubmitterIndependentDecisionAndValidStage(string mode)
    {
        var review=CommandSource();var application=review.ReadApplication();var action="SUBMIT";
        if(mode is "approve" or "reject" or "self" or "missing-submitter")
        {
            application.STATUS=Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted;
            application.SUBMITTED_BY_USER_ID=mode=="self"?"reviewer":mode=="missing-submitter"?"":"issuer";
            action=mode=="reject"?"REJECT":"APPROVE";
        }
        if(mode=="terminal")application.STATUS=Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Approved;
        if(mode=="unknown-country")application.COUNTRY=Beep.OilandGas.Models.Data.PermitsAndApplications.Country.Other;
        var command=new PermitApplicationCommand(mode=="intent"?Guid.Empty:Guid.NewGuid(),PermitApplicationSourceReview.Capture(review.Context,application,review.ReadHistory()),mode=="action"?"approve":action,mode=="reason"?" ":"Reviewed recorded evidence");
        if(mode is "submit" or "approve" or "reject")
        {
            command.Require();var restored=System.Text.Json.JsonSerializer.Deserialize<PermitApplicationCommand>(System.Text.Json.JsonSerializer.Serialize(command))!;restored.Require();Assert.Equal(command,restored);
        }
        else Assert.Throws<InvalidOperationException>(command.Require);
    }
    [Theory][InlineData("permission")][InlineData("field")]
    public async Task PermitReadRefusesAuthorityRevokedDuringProviderRead(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;
        f.AfterRead=async()=>
        {
            f.AfterRead=null;
            if(mode=="permission")
            {
                f.Core.Db.RoleClaims.Remove(f.Core.Db.RoleClaims.Single(x=>x.ClaimValue==PermissionConstants.HSE.ViewPermits));
                await f.Core.Db.SaveChangesAsync();
            }
            else f.Core.Rows.OfType<Beep.OilandGas.PPDM39.Models.FIELD>().Single(x=>x.FIELD_ID=="A").ACTIVE_IND="N";
        };
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.GetByIdAsync("local"));
        Assert.Single(f.Reads);Assert.Empty(f.Core.Writes);
    }
    [Theory][InlineData("company")][InlineData("tenant")][InlineData("unowned")]
    public async Task PermitSharedFieldNeverAdoptsAnotherCompanyOrUnownedApplication(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;
        var row=f.Rows.OfType<PERMIT_APPLICATION>().Single(x=>x.PERMIT_APPLICATION_ID=="local");
        if(mode=="company")row.OWNER_COMPANY_ID="another-company";
        if(mode=="tenant")row.OWNER_TENANT_ID="another-tenant";
        if(mode=="unowned"){row.OWNER_TENANT_ID="";row.OWNER_COMPANY_ID="";}
        Assert.Null(await f.Lifecycle.GetByIdAsync("local"));
        f.IgnoreFilters=true;f.IgnoreFilterTable="PERMIT_APPLICATION";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.GetByIdAsync("local"));
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Repository(typeof(PERMIT_STATUS_HISTORY),"local").GetAsync([]));
        Assert.DoesNotContain("PERMIT_STATUS_HISTORY",f.Reads);Assert.Empty(f.Core.Writes);
    }
    [Theory][InlineData("history")][InlineData("mit")][InlineData("foreign-history")][InlineData("missing-injection")]
    public async Task PermitChildrenRequireExactVisibleApplicationAndInjectionParent(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;
        if(mode=="foreign-history")f.Rows.OfType<PERMIT_STATUS_HISTORY>().Single().PERMIT_APPLICATION_ID="other";
        if(mode=="missing-injection")f.Rows.Remove(f.Rows.OfType<INJECTION_PERMIT_APPLICATION>().Single());
        var type=mode is "mit" or "missing-injection"?typeof(MIT_RESULT):typeof(PERMIT_STATUS_HISTORY);
        if(mode=="foreign-history"){f.IgnoreFilters=true;f.IgnoreFilterTable="PERMIT_STATUS_HISTORY";}
        if(mode is "foreign-history" or "missing-injection")await Assert.ThrowsAnyAsync<Exception>(()=>f.Repository(type,"local").GetAsync([]));
        else{var rows=(await f.Repository(type,"local").GetAsync([])).ToArray();Assert.Single(rows);Assert.Equal(type,rows[0].GetType());}
        if(mode=="foreign-history")Assert.Contains("PERMIT_STATUS_HISTORY",f.Reads);
        if(mode=="missing-injection")Assert.DoesNotContain("MIT_RESULT",f.Reads);
        Assert.Empty(f.Core.Writes);
    }
    [Theory][InlineData(PPDMRowMutation.Insert)][InlineData(PPDMRowMutation.Update)][InlineData(PPDMRowMutation.Delete)]
    public async Task PermitReadPolicyNeverAuthorizesGenericMutation(PPDMRowMutation mutation)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;var policy=f.Access.ForRead(typeof(PERMIT_APPLICATION),"PERMIT_APPLICATION");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>policy.ValidateMutationAsync(typeof(PERMIT_APPLICATION),"PERMIT_APPLICATION",f.Rows[0],null,mutation,"actor"));Assert.Empty(f.Reads);Assert.Empty(f.Core.Writes);
    }
    [Fact]
    public async Task PermitFactoriesRejectMissingParentOrWrongCanonicalTypeBeforeReads()
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;Assert.Throws<InvalidOperationException>(()=>f.Access.ForRead(typeof(PERMIT_STATUS_HISTORY),"PERMIT_STATUS_HISTORY"));Assert.Throws<InvalidOperationException>(()=>f.Access.ForRead(typeof(Beep.OilandGas.PPDM39.Models.FIELD),"FIELD"));Assert.Empty(f.Reads);
    }
    [Theory][InlineData("valid")][InlineData("permission-before")][InlineData("grant-before")][InlineData("late-permission")][InlineData("late-grant")][InlineData("late-selection")][InlineData("late-binding")][InlineData("late-native-field")][InlineData("late-entitlement")][InlineData("late-core-target")][InlineData("exception-cleanup")][InlineData("nested-write")]
    public async Task PermitReadAuthorityBoundsNativeChecksAndRefusesCurrentAuthorityDrift(string mode)
    {
        using var f=await Ready();f.Core.Http.HttpContext=f.Core.Request;
        var permission=f.Core.Db.RoleClaims.Single(x=>x.ClaimValue==PermissionConstants.HSE.ViewPermits);
        var grant=await f.Core.Db.Set<AppUserAssetAccess>().SingleAsync(x=>x.AssetType=="FIELD" && x.AssetId=="A");
        if(mode=="permission-before"){f.Core.Db.RoleClaims.Remove(permission);await f.Core.Db.SaveChangesAsync();}
        if(mode=="grant-before"){grant.IsActive=false;await f.Core.Db.SaveChangesAsync();}
        int NativeReads()=>f.Core.Queries.Count+f.Core.EligibilityQueries.Count+f.Core.LeaseQueries.Count;
        var entered=false;var afterNested=0;
        async Task<string> Run()=>await f.Access.WithReadAuthorityAsync(async()=>
        {
            entered=true;var reads=NativeReads();var scope=await f.Access.RequireReadScopeAsync();
            Assert.NotNull(await f.Lifecycle.GetByIdAsync("local"));
            Assert.Equal(scope,await f.Access.WithReadAuthorityAsync(()=>f.Access.RequireReadScopeAsync()));
            await f.Access.RequireReadScopeAsync();Assert.Equal(reads,NativeReads());afterNested=reads;
            if(mode=="nested-write")
            {
                var command=new PermitApplicationCommand(Guid.NewGuid(),PermitApplicationSourceReview.Capture(scope,new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="local",OWNER_TENANT_ID=scope.Field.Company.TenantId,OWNER_COMPANY_ID=scope.Field.Company.BusinessAssociateId,FIELD_ID="A",ACTIVE_IND="Y",COUNTRY=Beep.OilandGas.Models.Data.PermitsAndApplications.Country.Kuwait,APPLICANT_ID="Applicant"},[]),"SUBMIT","Nested writes must use a separate authority owner");
                var error=await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Access.WithCommandAuthorityAsync(command,_=>Task.FromResult(true)));Assert.Contains("cannot nest",error.Message);
            }
            if(mode=="late-permission")f.Core.Db.RoleClaims.Remove(permission);
            if(mode=="late-grant")grant.IsActive=false;
            if(mode=="late-selection"){var user=await f.Core.Db.Users.SingleAsync(x=>x.Id=="actor");user.ConcurrencyStamp="changed-read-selection";}
            if(mode=="late-binding")f.Binding.ConcurrencyStamp="changed-read-binding";
            if(mode=="late-native-field")f.Core.Rows.OfType<Beep.OilandGas.PPDM39.Models.FIELD>().Single(x=>x.FIELD_ID=="A").FIELD_NAME="Changed native FIELD during read";
            if(mode=="late-entitlement")
            {var component=f.Core.Rows.OfType<Beep.OilandGas.PPDM39.Models.ENT_COMPONENT>().Single(x=>x.FIELD_ID=="A" && x.BUSINESS_ASSOCIATE_ID=="company");f.Core.Rows.OfType<Beep.OilandGas.PPDM39.Models.ENTITLEMENT>().Single(x=>x.ENTITLEMENT_ID==component.ENTITLEMENT_ID).ACTIVE_IND="N";}
            if(mode=="late-core-target")f.Core.Editor.Object.ConfigEditor.DataConnections.Single(x=>x.ConnectionName==scope.Field.Database.ConnectionName).Database="changed-native-target";
            await f.Core.Db.SaveChangesAsync();if(mode=="exception-cleanup")throw new IOException("Selected read callback failed");
            return "read-confirmed";
        });
        if(mode is "valid" or "nested-write")
        {
            Assert.Equal("read-confirmed",await Run());Assert.True(NativeReads()>afterNested);
            var before=NativeReads();Assert.Equal("new read",await f.Access.WithReadAuthorityAsync(()=>Task.FromResult("new read")));Assert.True(NativeReads()>before);
        }
        else await Assert.ThrowsAnyAsync<Exception>(Run);
        if(mode.EndsWith("-before",StringComparison.Ordinal))Assert.False(entered);else Assert.True(entered);
        if(mode=="exception-cleanup")
        {var before=NativeReads();await f.Access.RequireReadScopeAsync();Assert.True(NativeReads()>before);}
        Assert.Empty(f.Writes);Assert.Empty(f.Transactions);Assert.Empty(f.Core.Writes);
    }

}
#endif
