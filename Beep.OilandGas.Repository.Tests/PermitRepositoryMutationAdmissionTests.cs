using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PermitsAndApplications.Models;
#if RECORDED_APPROVAL_HTTP_FOCUSED
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using Moq;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed class PermitRepositoryMutationAdmissionTests
{
    public static IEnumerable<object[]> PermitCases()=>PermitApplicationSchema.Entities.Select(type=>new object[]{type.Name,"canonical"}).Concat(new[]{new object[]{nameof(PERMIT_DOCUMENT_STAGE),"type-alias"},new object[]{nameof(PERMIT_DOCUMENT_STAGE),"table-alias"},new object[]{nameof(PERMIT_DOCUMENT_STAGE),"case-alias"}});
    [Theory][MemberData(nameof(PermitCases))]
    public async Task EveryPermitManifestTableRefusesUnownedWritesBeforePreparation(string table,string mode)
    {
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);var columns=new Mock<ICommonColumnHandler>(MockBehavior.Strict);var defaults=new Mock<IPPDM39DefaultsRepository>(MockBehavior.Strict);var metadata=new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var type=mode=="table-alias"?typeof(FIELD):PermitApplicationSchema.Entities.Single(x=>x.Name==table);var target=mode=="type-alias"?"unrelated":mode is "case-alias" or "table-alias"?table.ToLowerInvariant():table;
        var row=Activator.CreateInstance(type)!;var before=PermitApplicationCommandOutcome.Payload(row);
        var repository=new ProbeRepository(editor.Object,columns.Object,defaults.Object,metadata.Object,type,target);
        foreach(var operation in Mutations(repository,row))
        {var error=await Assert.ThrowsAsync<RefusalException>(operation);Assert.Equal(RefusalKind.Forbidden,error.Kind);}
        Assert.Throws<RefusalException>(()=>repository.UnitOfWork);
        await Assert.ThrowsAsync<RefusalException>(()=>repository.SetParent(row));
        Assert.Equal(before,PermitApplicationCommandOutcome.Payload(row));editor.VerifyNoOtherCalls();columns.VerifyNoOtherCalls();defaults.VerifyNoOtherCalls();metadata.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task PermitReadPolicyCannotBecomeMutationOwnerWithoutOwnedTransaction()
    {
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);var source=new Mock<IDataSource>();editor.Setup(x=>x.GetDataSource("native")).Returns(source.Object);
        var columns=new Mock<ICommonColumnHandler>(MockBehavior.Strict);var defaults=new Mock<IPPDM39DefaultsRepository>(MockBehavior.Strict);var metadata=new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var row=new PERMIT_DOCUMENT_STAGE();var before=PermitApplicationCommandOutcome.Payload(row);var policy=new Mock<IPPDMRowAccessPolicy>(MockBehavior.Strict);
        var repository=new ProbeRepository(editor.Object,columns.Object,defaults.Object,metadata.Object,typeof(PERMIT_DOCUMENT_STAGE),nameof(PERMIT_DOCUMENT_STAGE),policy.Object);
        foreach(var operation in Mutations(repository,row)){var error=await Assert.ThrowsAnyAsync<Exception>(operation);Assert.True(error is InvalidOperationException or RefusalException,error.ToString());}
        Assert.Throws<RefusalException>(()=>repository.UnitOfWork);await Assert.ThrowsAnyAsync<Exception>(()=>repository.SetParent(row));
        Assert.Equal(before,PermitApplicationCommandOutcome.Payload(row));columns.VerifyNoOtherCalls();defaults.VerifyNoOtherCalls();metadata.VerifyNoOtherCalls();policy.VerifyNoOtherCalls();source.VerifyNoOtherCalls();
    }
    [Theory][InlineData("type")][InlineData("table")]
    public void UnprotectedRepositoryCannotCreatePermitUnitOfWorkThroughProtectedHelper(string mode)
    {
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);var metadata=new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var repository=new ProbeRepository(editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,typeof(FIELD),"FIELD");
        Assert.Throws<RefusalException>(()=>repository.OpenOther(mode=="type"?typeof(PERMIT_DOCUMENT_STAGE):typeof(FIELD),mode=="table"?"permit_document_stage":"unrelated"));
        editor.VerifyNoOtherCalls();metadata.VerifyNoOtherCalls();
    }
    private static IEnumerable<Func<Task>> Mutations(PPDMGenericRepository repository,object row)
    {
        yield return async()=>{await repository.InsertAsync(row,"actor");};
        yield return async()=>{await repository.UpdateAsync(row,"actor");};
        yield return async()=>{await repository.DeleteAsync(row);};
        yield return async()=>{await repository.SoftDeleteAsync("original","actor");};
        yield return async()=>{await repository.InsertWithParentKeysAsync(row,"actor",new(){{"PERMIT_APPLICATION","different-parent"}});};
        yield return async()=>{await repository.InsertBatchAsync([row],"actor");};
        yield return async()=>{await repository.UpdateBatchAsync([row],"actor");};
        yield return async()=>{await repository.InsertBatchWithParentKeysAsync([row],"actor",new(){{"PERMIT_APPLICATION","different-parent"}});};
        yield return async()=>{await repository.InsertBatchWithSizeAsync([row],"actor",1);};
        yield return async()=>{await repository.UpdateBatchAsync([row],"actor",1);};
        yield return async()=>{await repository.DeleteBatchAsync(["original"],"actor",false,1);};
        yield return async()=>{await repository.UpsertBatchAsync([row],"actor",1);};
    }
    private sealed class ProbeRepository(IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,Type type,string table,IPPDMRowAccessPolicy? policy=null):PPDMGenericRepository(editor,columns,defaults,metadata,type,"native",table,rowAccess:policy)
    {
        public object OpenOther(Type type,string table)=>GetOrCreateUnitOfWork(type,table);
        public Task SetParent(object row)=>ValidateAndSetParentKeysAsync(row,new(){{"PERMIT_APPLICATION","different-parent"}});
    }
}
#endif
