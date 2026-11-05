#if RECORDED_APPROVAL_HTTP_FOCUSED
using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class PermitProviderQualificationTests
{
    internal static void InstallDocumentCore(IDMEEditor editor,IDataSource core)
    {
        var model=new MigrationModel {Source="Native PPDM controlled staging schema",SourceId="permit-document-stage"};
        foreach(var type in new[]{typeof(SOURCE_DOCUMENT),typeof(RM_INFORMATION_ITEM),typeof(RM_FILE_CONTENT),typeof(R_DOCUMENT_TYPE),typeof(R_LANGUAGE),typeof(R_DIGITAL_FORMAT)})
        {
            var keys=DocumentCoreKeys(type);
            var entity=new MigrationModelEntity {ClrTypeFullName=type.FullName!,TableName=type.Name};
            foreach(var property in type.GetProperties().Where(x=>x.CanRead && x.CanWrite && x.GetIndexParameters().Length==0))
            {
                var actual=Nullable.GetUnderlyingType(property.PropertyType)??property.PropertyType;
                entity.Properties.Add(new(){PropertyName=property.Name,ColumnName=property.Name,FieldType=actual.FullName!,IsPrimaryKey=keys.Contains(property.Name),
                    IsNullable=!keys.Contains(property.Name),MaxLength=actual==typeof(string)?128:null});
            }
            model.Entities.Add(type.FullName!,entity);
        }
        var migration=new MigrationManager(editor,core){ExecutionTargetIdentity=MigrationConnectionTarget.Fingerprint(core.Dataconnection.ConnectionProp)};
        var plan=migration.BuildMigrationPlanForModel(model,detectRelationships:false);Assert.Equal(6,plan.Operations.Count);
        migration.ApproveMigrationPlan(plan,"document-core-controlled-schema");var result=migration.ExecuteMigrationPlan(plan);Assert.True(result.Success,result.Message);
        foreach(var row in new IPPDMEntity[]{new R_DOCUMENT_TYPE{DOCUMENT_TYPE="PLAN",ACTIVE_IND="Y"},new R_LANGUAGE{LANGUAGE_ID="en",ACTIVE_IND="Y"},new R_DIGITAL_FORMAT{DIGITAL_FORMAT="PDF",ACTIVE_IND="Y"}})
        {new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler().PrepareForInsert(row,"catalog-author");Assert.Equal(Errors.Ok,core.InsertEntity(row.GetType().Name,row).Flag);}
    }
    internal static string[] DocumentCoreKeys(Type type)=>type.Name switch
    {"SOURCE_DOCUMENT"=>["SOURCE_DOCUMENT_ID"],"RM_INFORMATION_ITEM"=>["INFORMATION_ITEM_ID","INFO_ITEM_SUBTYPE"],"RM_FILE_CONTENT"=>["FILE_ID"],"R_DOCUMENT_TYPE"=>["DOCUMENT_TYPE"],"R_LANGUAGE"=>["LANGUAGE_ID"],"R_DIGITAL_FORMAT"=>["DIGITAL_FORMAT"],_=>throw new InvalidOperationException("Unlisted native document type "+type.Name)};
    internal static PermitDocumentStageOriginal StageOriginal(PermitApplicationSourceReview review,byte[] bytes)=>new(Guid.NewGuid(),review,
        new("report.pdf","Reviewed report","PLAN","en","1","PDF","DOC",Convert.ToHexString(SHA256.HashData(bytes)),bytes.Length,
            DateTimeOffset.UtcNow.AddDays(-1),DateTimeOffset.UtcNow.AddYears(1)),"Reviewed native document upload and exact application ownership");

    [Theory][InlineData("valid")][InlineData("core-rollback")][InlineData("confirmation-loss")][InlineData("revoked")][InlineData("target-drift")][InlineData("collision")][InlineData("partial")][InlineData("changed-content")][InlineData("link-download")][InlineData("link-rollback")][InlineData("linked-change")][InlineData("reader")]
    public Task InstalledNativeDocumentStagingOwnsTwoTargetsAndSameOriginal(string mode)=>WithInstalledProviderAsync(async (editor,permits)=>
    {
        var corePath=Path.Combine(Path.GetDirectoryName(permits.Dataconnection.ConnectionProp.Database)!,"core.db");
        editor.ConfigEditor.DataConnections.Add(new ConnectionProperties {ConnectionName="core-provider",DatabaseType=TheTechIdea.Beep.Utilities.DataSourceType.SqlLite,
            ConnectionString="Data Source="+corePath,Database=corePath,GuidID=Guid.NewGuid().ToString()});
        var core=editor.GetDataSource("core-provider")??throw new InvalidOperationException("The same editor must own both configured native targets.");
        Assert.Equal(ConnectionState.Open,core.Openconnection());
        try
        {
        InstallLedgerSchema(editor,permits);InstallDocumentCore(editor,core);
        var migration=new MigrationManager(editor,permits){ExecutionTargetIdentity=MigrationConnectionTarget.Fingerprint(permits.Dataconnection.ConnectionProp)};
        var plan=migration.BuildMigrationPlanForTypes([typeof(PERMIT_DOCUMENT_STAGE)],detectRelationships:false);migration.ApproveMigrationPlan(plan,"permit-stage-journal");Assert.True(migration.ExecuteMigrationPlan(plan).Success);
        var fixture=CreateNativeFixture(editor,permits,core.DatasourceName);var service=fixture.Service;
        var parent=new PERMIT_APPLICATION {PERMIT_APPLICATION_ID="document-parent",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="field-a",COUNTRY=Country.Kuwait,APPLICANT_ID="Applicant",ACTIVE_IND="Y"};
        new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler().PrepareForInsert(parent,"author");Assert.Equal(Errors.Ok,permits.InsertEntity(nameof(PERMIT_APPLICATION),parent).Flag);
        var bytes=Enumerable.Range(0,1024).Select(i=>(byte)(i%256)).ToArray();var review=await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID);var original=StageOriginal(review,bytes);
        Assert.Null(await service.RecoverDocumentStageAsync(original));
        var connectionText=core.Dataconnection.ConnectionProp.ConnectionString;
        if(mode=="revoked")fixture.AllowWrites=false;
        if(mode=="core-rollback")fixture.InterruptDocument=true;
        if(mode is "confirmation-loss" or "collision" or "partial" or "changed-content")fixture.InterruptDocumentProof=true;
        if(mode=="target-drift")fixture.CommandGuard=()=>{core.Dataconnection.ConnectionProp.ConnectionString="Data Source=refused-target";return Task.CompletedTask;};
        if(mode is not ("valid" or "link-download" or "link-rollback" or "linked-change" or "reader"))
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>service.StageDocumentAsync(original,bytes));
            core.Dataconnection.ConnectionProp.ConnectionString=connectionText;fixture.CommandGuard=null;
            Assert.Null(await service.RecoverDocumentStageAsync(original));
            if(mode is "core-rollback" or "confirmation-loss" or "collision" or "partial" or "changed-content")Assert.True(fixture.DocumentInterrupted);
            fixture.AllowWrites=true;fixture.InterruptDocument=false;fixture.InterruptDocumentProof=false;
            var raw=await core.GetEntityAsync(nameof(RM_FILE_CONTENT),[]);
            var count=raw is System.Data.DataTable table?table.Rows.Count:raw is System.Collections.IEnumerable enumerable?enumerable.Cast<object>().Count():throw new InvalidOperationException("Native file population is unconfirmed.");
            Assert.Equal(mode is "confirmation-loss" or "collision" or "partial" or "changed-content"?1:0,count);
        }
        if(mode is "collision" or "partial" or "changed-content")
        {
            var raw=await permits.GetEntityAsync(nameof(PERMIT_DOCUMENT_STAGE),[]);
            var records=raw is System.Data.DataTable table?table.Rows.Cast<object>().ToArray():raw is System.Collections.IEnumerable enumerable?enumerable.Cast<object>().ToArray():throw new InvalidOperationException("Staging journal population is unconfirmed.");
            var record=Assert.Single(records);
            var json=record is System.Data.DataRow data?data["SEED_JSON"]:record is IDictionary<string,object> values?values["SEED_JSON"]:record.GetType().GetProperty("SEED_JSON")!.GetValue(record);
            var seed=JsonSerializer.Deserialize<PermitDocumentStageSeed>(Assert.IsType<string>(json))!;seed.Require();
            var rows=seed.BuildRows(bytes);
            if(mode=="collision")
            {
                var doc=(SOURCE_DOCUMENT)rows[0];doc.PPDM_GUID=Guid.NewGuid().ToString();Assert.Equal(Errors.Ok,core.UpdateEntity(nameof(SOURCE_DOCUMENT),doc).Flag);
            }
            else if(mode=="partial")Assert.Equal(Errors.Ok,core.DeleteEntity(nameof(RM_FILE_CONTENT),rows[2]).Flag);
            else
            {
                var file=(RM_FILE_CONTENT)rows[2];file.FILE_CONTENT=bytes.Select((b,i)=>i==0?(byte)(b+1):b).ToArray();Assert.Equal(Errors.Ok,core.UpdateEntity(nameof(RM_FILE_CONTENT),file).Flag);
            }
            var refused=await Assert.ThrowsAnyAsync<Exception>(()=>service.StageDocumentAsync(original,bytes));
            Assert.Contains(mode=="partial"?"partial":mode=="collision"?"nonce":"bytes differ",refused.ToString(),StringComparison.OrdinalIgnoreCase);
            Assert.Null(await service.RecoverDocumentStageAsync(original));Assert.Equal(review,await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID));
            PPDMDataSourceTransaction.RequireIdle(permits);PPDMDataSourceTransaction.RequireIdle(core);return;
        }
        var proof=await service.StageDocumentAsync(original,bytes);proof.Require(original);
        Assert.Equal(original,proof.Seed.Original);Assert.Equal(review,await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID));
        foreach(var name in new[]{nameof(SOURCE_DOCUMENT),nameof(RM_INFORMATION_ITEM),nameof(RM_FILE_CONTENT)})
        {
            var raw=await core.GetEntityAsync(name,[]);var rows=raw is System.Data.DataTable table?table.Rows.Cast<object>().ToArray():raw is System.Collections.IEnumerable enumerable?enumerable.Cast<object>().ToArray():throw new InvalidOperationException("Native staged population is unconfirmed.");
            var row=Assert.Single(rows);
            if(name==nameof(RM_FILE_CONTENT))
            {
                var actual=row is System.Data.DataRow data?data["FILE_CONTENT"]:row is IDictionary<string,object> values?values["FILE_CONTENT"]:row.GetType().GetProperty("FILE_CONTENT")!.GetValue(row);
                Assert.Equal(bytes,Assert.IsType<byte[]>(actual));
            }
        }
        foreach(var name in new[]{nameof(SOURCE_DOCUMENT),nameof(RM_INFORMATION_ITEM),nameof(RM_FILE_CONTENT)})Assert.False(permits.CheckEntityExist(name));
        Assert.False(core.CheckEntityExist(nameof(PERMIT_DOCUMENT_STAGE)));
        if(mode is "link-download" or "link-rollback" or "linked-change" or "reader")
        {
            var receipt=PermitDocumentStageReceipt.From(proof);receipt.RequireOriginal(original);
            var json=JsonSerializer.Serialize(receipt);Assert.DoesNotContain("CoreTarget",json);Assert.DoesNotContain("PermitTarget",json);Assert.DoesNotContain("Seed",json);
            var before=await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID);
            var command=new PermitApplicationCommand(Guid.NewGuid(),before,"ATTACH_DOCUMENT","Reviewed exact staged version/content and current application link") { Attachment=new(receipt) };
            if(mode=="link-rollback")
            {
                fixture.InterruptReceipt=true;await Assert.ThrowsAnyAsync<Exception>(()=>service.RecordCommandAsync(command));Assert.True(fixture.Interrupted);
                Assert.Equal(before,await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID));Assert.Null(await service.RecoverCommandAsync(command));fixture.InterruptReceipt=false;
            }
            var attached=await service.RecordCommandAsync(command);attached.Require();var id="PAF-"+command.IntentId.ToString("N");
            var linked=await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID);Assert.True(linked.Supporting.AllDocumentsVerified);Assert.Single(linked.Supporting.Attachments);
            Assert.Contains("SUBMIT",PermitApplicationCommand.AllowedActions(linked));
            var query=new PermitAttachmentContentQuery(linked,id);Assert.Equal(bytes,(await service.ReadAttachmentContentAsync(query)).Content);
            if(mode=="linked-change")
            {
                var file=(RM_FILE_CONTENT)proof.Seed.BuildRows(bytes)[2];file.FILE_CONTENT=bytes.Select((b,i)=>i==0?(byte)(b+1):b).ToArray();Assert.Equal(Errors.Ok,core.UpdateEntity(nameof(RM_FILE_CONTENT),file).Flag);
                var changed=await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID);Assert.False(changed.Supporting.AllDocumentsVerified);Assert.Equal("CHANGED",Assert.Single(changed.Supporting.Documents).State);
                Assert.DoesNotContain("SUBMIT",PermitApplicationCommand.AllowedActions(changed));
                await Assert.ThrowsAnyAsync<Exception>(()=>service.ReadAttachmentContentAsync(query));
                var pending=new PermitApplicationCommand(Guid.NewGuid(),linked,"SUBMIT","Previously verified file changed before transport");
                await Assert.ThrowsAnyAsync<Exception>(()=>service.RecordCommandAsync(pending));Assert.Null(await service.RecoverCommandAsync(pending));
                fixture.AllowWrites=false;Assert.Equal(attached,await service.RecoverCommandAsync(command));return;
            }
            var submitted=await service.RecordCommandAsync(new(Guid.NewGuid(),linked,"SUBMIT","Submitted with every linked document currently verified"));submitted.Require();
            if(mode=="reader")
            {
                fixture.CurrentScopeOverride=before.Context with {Field=before.Context.Field with {ActorUserId="independent-reviewer"}};
                var reviewed=await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID);Assert.Contains("APPROVE",PermitApplicationCommand.AllowedActions(reviewed));
                Assert.Equal(bytes,(await service.ReadAttachmentContentAsync(new(reviewed,id))).Content);
                var approved=await service.RecordCommandAsync(new(Guid.NewGuid(),reviewed,"APPROVE","Independent reviewer checked the exact current linked content"));approved.Require();
                fixture.CurrentScopeOverride=before.Context with {Field=before.Context.Field with {Company=new("tenant","foreign-company")}};
                await Assert.ThrowsAnyAsync<Exception>(()=>service.ReadAttachmentContentAsync(new(reviewed,id)));return;
            }
            fixture.AllowWrites=false;Assert.Equal(attached,await service.RecoverCommandAsync(command));Assert.Equal(attached,await service.RecordCommandAsync(command));return;
        }
        fixture.AllowWrites=false;Assert.Equal(proof,await service.RecoverDocumentStageAsync(original));Assert.Equal(proof,await service.StageDocumentAsync(original,bytes));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.StageDocumentAsync(original,bytes.Concat(new byte[]{1}).ToArray()));
        Assert.Empty((await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID)).ReadHistory());Assert.Empty(await fixture.ReceiptRepository.GetAsync([]));
        PPDMDataSourceTransaction.RequireIdle(permits);PPDMDataSourceTransaction.RequireIdle(core);
        }
        finally { core.Closeconnection(); }
    });

    [Theory][InlineData("hash")][InlineData("length")][InlineData("path")][InlineData("window")][InlineData("nonce")][InlineData("owner")]
    public void DocumentStagingOriginalAndOwnershipRejectUnboundValues(string mode)
    {
        var scope=new PermitApplicationDataScope(new(new("tenant","company"),new("core","binding"),"author","selection","field-a"),new("permits","binding"));
        var parent=new PERMIT_APPLICATION {PERMIT_APPLICATION_ID="parent",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="field-a",ACTIVE_IND="Y"};
        var bytes=new byte[]{0,255,10,13,42};var original=StageOriginal(PermitApplicationSourceReview.Capture(scope,parent,[]),bytes);
        if(mode=="hash"){Assert.Throws<InvalidOperationException>(()=>original.Document.RequireContent(new byte[]{0,255,10,13,43}));return;}
        if(mode=="length"){Assert.Throws<InvalidOperationException>(()=>(original with {Document=original.Document with {ContentBytes=0}}).Require());return;}
        if(mode=="path"){Assert.Throws<InvalidOperationException>(()=>(original with {Document=original.Document with {FileName="../private.pdf"}}).Require());return;}
        if(mode=="window"){Assert.Throws<InvalidOperationException>(()=>(original with {Document=original.Document with {EffectiveToUtc=original.Document.EffectiveFromUtc}}).Require());return;}
        var target=ProcessDefinitionRevision.FromJson("target");var seed=new PermitDocumentStageSeed(original,Guid.NewGuid(),DateTimeOffset.UtcNow,target,target);var journal=PERMIT_DOCUMENT_STAGE.Create(seed);
        if(mode=="nonce")journal.PPDM_GUID=Guid.NewGuid().ToString();else journal.OWNER_COMPANY_ID="foreign";
        Assert.Throws<InvalidOperationException>(()=>journal.Restore(original));
    }
    [Theory][InlineData("valid")][InlineData("missing-type")][InlineData("missing-language")][InlineData("inactive-format")][InlineData("dated-language")][InlineData("mid-core")][InlineData("changed-before-link")][InlineData("changed-after-link")]
    public Task InstalledDocumentVocabularyRequiresExactActiveNativeCodes(string mode)=>WithInstalledProviderAsync(async(editor,permits)=>
    {
        var connection=new ConnectionProperties{ConnectionName="vocabulary-core",DatabaseType=TheTechIdea.Beep.Utilities.DataSourceType.SqlLite,Database=Path.Combine(Path.GetDirectoryName(permits.Dataconnection.ConnectionProp.Database)!,"vocabulary-core.db"),GuidID=Guid.NewGuid().ToString()};connection.ConnectionString="Data Source="+connection.Database;editor.ConfigEditor.DataConnections.Add(connection);
        var core=editor.GetDataSource(connection.ConnectionName)!;Assert.Equal(ConnectionState.Open,core.Openconnection());
        try
        {
            InstallLedgerSchema(editor,permits);InstallDocumentCore(editor,core);
            var migration=new MigrationManager(editor,permits){ExecutionTargetIdentity=MigrationConnectionTarget.Fingerprint(permits.Dataconnection.ConnectionProp)};var plan=migration.BuildMigrationPlanForTypes([typeof(PERMIT_DOCUMENT_STAGE)],detectRelationships:false);migration.ApproveMigrationPlan(plan,"document-vocabulary-journal");Assert.True(migration.ExecuteMigrationPlan(plan).Success);
            var fixture=CreateNativeFixture(editor,permits,connection.ConnectionName);var service=fixture.Service;
            var parent=new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="vocabulary-parent",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="field-a",COUNTRY=Country.Kuwait,APPLICANT_ID="Applicant",ACTIVE_IND="Y"};new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler().PrepareForInsert(parent,"author");Assert.Equal(Errors.Ok,permits.InsertEntity(nameof(PERMIT_APPLICATION),parent).Flag);
            var bytes=new byte[]{0,255,42,10};var original=StageOriginal(await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID),bytes);
            if(mode=="missing-type")original=original with{Document=original.Document with{DocumentType="unrecorded"}};
            if(mode=="missing-language")original=original with{Document=original.Document with{Language="unrecorded"}};
            void DisableFormat()=>Assert.Equal(Errors.Ok,core.UpdateEntity(nameof(R_DIGITAL_FORMAT),new R_DIGITAL_FORMAT{DIGITAL_FORMAT="PDF",ACTIVE_IND="N"}).Flag);
            if(mode=="inactive-format")DisableFormat();
            if(mode=="dated-language")Assert.Equal(Errors.Ok,core.UpdateEntity(nameof(R_LANGUAGE),new R_LANGUAGE{LANGUAGE_ID="en",ACTIVE_IND="Y",EFFECTIVE_DATE=DateTime.UtcNow.AddDays(-1)}).Flag);
            var changed=false;
            if(mode=="mid-core")fixture.PreparingDocument=row=>{if(row is RM_FILE_CONTENT){DisableFormat();changed=true;}};
            int Count(IDataSource source,string table)=>((System.Collections.IEnumerable)source.GetEntity(table,[])).Cast<object>().Count();
            if(mode is "missing-type" or "missing-language" or "inactive-format" or "dated-language" or "mid-core")
            {
                await Assert.ThrowsAnyAsync<InvalidOperationException>(()=>service.StageDocumentAsync(original,bytes));
                Assert.Equal(0,Count(core,nameof(SOURCE_DOCUMENT)));Assert.Equal(0,Count(core,nameof(RM_INFORMATION_ITEM)));Assert.Equal(0,Count(core,nameof(RM_FILE_CONTENT)));
                Assert.Null(await service.RecoverDocumentStageAsync(original));Assert.Equal(mode=="mid-core"?1:0,Count(permits,nameof(PERMIT_DOCUMENT_STAGE)));
                if(mode!="mid-core")return;
                Assert.True(changed);fixture.PreparingDocument=null;
            }
            var proof=await service.StageDocumentAsync(original,bytes);proof.Require(original);
            Assert.Equal(1,Count(core,nameof(RM_FILE_CONTENT)));Assert.Equal(1,Count(permits,nameof(PERMIT_DOCUMENT_STAGE)));
            var receipt=PermitDocumentStageReceipt.From(proof);var link=new PermitApplicationCommand(Guid.NewGuid(),await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID),"ATTACH_DOCUMENT","Reviewed native catalog codes and original document before linking"){Attachment=new(receipt)};
            if(mode=="changed-before-link")
            {DisableFormat();await Assert.ThrowsAnyAsync<InvalidOperationException>(()=>service.RecordCommandAsync(link));Assert.Null(await service.RecoverCommandAsync(link));Assert.Equal(0,Count(permits,nameof(APPLICATION_ATTACHMENT)));Assert.Equal(proof,await service.RecoverDocumentStageAsync(original));return;}
            var recorded=await service.RecordCommandAsync(link);recorded.Require();var reviewed=await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID);var query=new PermitAttachmentContentQuery(reviewed,reviewed.Supporting.Attachments.Single().APPLICATION_ATTACHMENT_ID);
            if(mode=="changed-after-link")
            {
                DisableFormat();var changedReview=await service.ReviewSourceAsync(parent.PERMIT_APPLICATION_ID);Assert.Equal("CHANGED",changedReview.Supporting.Documents.Single().State);Assert.DoesNotContain("SUBMIT",PermitApplicationCommand.AllowedActions(changedReview));
                await Assert.ThrowsAnyAsync<InvalidOperationException>(()=>service.ReadAttachmentContentAsync(query));fixture.AllowWrites=false;Assert.Equal(recorded,await service.RecoverCommandAsync(link));Assert.Equal(proof,await service.RecoverDocumentStageAsync(original));return;
            }
            Assert.True(reviewed.Supporting.AllDocumentsVerified);Assert.Equal(bytes,(await service.ReadAttachmentContentAsync(query)).Content);Assert.Equal(1,Count(permits,nameof(PERMIT_APPLICATION_COMMAND)));
        }
        finally{core.Closeconnection();}
    });

    [Theory][InlineData("explicit-json")][InlineData("runtime-default")]
    public async Task PermitSetupAndRuntimeMetadataShareDataOwnedManifest(string mode)
    {
        Assert.Same(PermitApplicationSchema.Entities,Beep.OilandGas.PermitsAndApplications.Modules.PermitsModule.OwnedEntities);Assert.Equal(12,PermitApplicationSchema.Entities.Count);
        Assert.Throws<NotSupportedException>(()=>((IList<Type>)PermitApplicationSchema.Entities).Add(typeof(string)));Assert.Throws<InvalidOperationException>(()=>PermitApplicationSchema.Keys(typeof(string)));
        var metadata=new Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PPDMMetadataService(Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PPDMMetadataService>.Instance,mode=="explicit-json"?Path.Combine(AppContext.BaseDirectory,"PPDM39Metadata.json"):null);
        foreach(var type in PermitApplicationSchema.Entities)
        {
            Assert.Equal(typeof(PermitApplicationSchema).Assembly,type.Assembly);var definition=await metadata.GetTableMetadataAsync(type.Name);
            Assert.NotNull(definition);Assert.Equal(type.FullName,definition.EntityTypeName);Assert.Equal(type.Name,definition.TableName);Assert.Equal("PERMITS",definition.Module);
            Assert.Equal(PermitApplicationSchema.Keys(type).Order(StringComparer.Ordinal),definition.PrimaryKeyColumn.Split(',').Order(StringComparer.Ordinal));
        }
        Assert.Equal(12,(await metadata.GetTablesByModuleAsync("PERMITS")).Count());
        foreach(var type in new[]{typeof(SOURCE_DOCUMENT),typeof(RM_INFORMATION_ITEM),typeof(RM_FILE_CONTENT),typeof(R_DOCUMENT_TYPE),typeof(R_LANGUAGE),typeof(R_DIGITAL_FORMAT)})
        {
            var definition=await metadata.GetTableMetadataAsync(type.Name);Assert.Equal(type.FullName,definition.EntityTypeName);Assert.Equal(DocumentCoreKeys(type).Order(StringComparer.Ordinal),definition.PrimaryKeyColumn.Split(',').Select(x=>x.Trim()).Order(StringComparer.Ordinal));
        }
        var source=await metadata.GetTableMetadataAsync(nameof(SOURCE_DOCUMENT));Assert.Contains(source.ForeignKeys,x=>x.ForeignKeyColumn=="LANGUAGE" && x.ReferencedTable=="R_LANGUAGE" && x.ReferencedPrimaryKey=="LANGUAGE_ID");
        var duplicate=new Dictionary<string,Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata>{{"PERMIT_APPLICATION",new(){EntityTypeName="other-owner"}}};
        Assert.Throws<InvalidOperationException>(()=>Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PermitApplicationNativeMetadata.AddTo(duplicate));
    }

}
#endif
