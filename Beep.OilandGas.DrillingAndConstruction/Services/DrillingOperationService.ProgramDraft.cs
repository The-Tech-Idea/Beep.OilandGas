using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using System.Text.Json;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.DrillingAndConstruction.Services;
public partial class DrillingOperationService
{
    private static void RequireProgramSchema(IDataSource source)
    {
        var schema=source.GetEntityStructure(new EntityStructure{EntityName="DRILLING_PROGRAM",DatasourceEntityName="DRILLING_PROGRAM"},true)
            ??throw new InvalidOperationException("Review the DRILLING_EXECUTION Beep migration before saving a program.");
        var keys=schema.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="PROGRAM_ID")throw new InvalidOperationException("An exact drilling PROGRAM_ID primary key is required.");
        void Field(string name,Type type,int width=0,bool required=false)
        {
            var found=schema.Fields!.Where(field=>field.FieldName==name).ToArray();
            if(found.Length!=1 || found[0].Fieldtype!=type.FullName || required && found[0].AllowDBNull || width>0 && Math.Max(found[0].Size,found[0].Size1)<width)
                throw new InvalidOperationException("Review drilling program column "+name+" through Beep Migration before recording a draft.");
        }
        foreach(var name in new[]{"PROGRAM_ID","OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","AFE_ID","FDP_BASELINE_ID","INVESTMENT_HANDOFF_ID"})Field(name,typeof(string),128,true);
        foreach(var name in new[]{"PROGRAM_DRAFT_REVISION","INVESTMENT_OUTCOME_REVISION"})Field(name,typeof(string),64,true);
        Field("PROGRAM_DRAFT_JSON",typeof(string),CreateDrillingProgramDraft.MaximumBodyLength,true);Field("PROGRAM_VERSION",typeof(int),required:true);
        foreach(var name in new[]{"WELL_ID","FIELD_ID","PROGRAM_NAME","PROGRAM_STATUS","WELL_TYPE","WELL_PROFILE","DEPTH_OUOM","AFE_CURRENCY","AFE_NUMBER","DESCRIPTION_OBJECTIVE","DRILLING_CONTRACTOR","RIG_NAME","RIG_TYPE","SOURCE","ACTIVE_IND","PPDM_GUID","ROW_CREATED_BY","ROW_CHANGED_BY"})Field(name,typeof(string));
        foreach(var name in new[]{"PLANNED_MEASURED_DEPTH","PLANNED_TRUE_VERTICAL_DEPTH","PLANNED_DURATION_DAYS","AFE_COST"})Field(name,typeof(decimal));
        foreach(var name in new[]{"PLANNED_SPUD_DATE","PLANNED_COMPLETION_DATE","ROW_CREATED_DATE","ROW_CHANGED_DATE"})Field(name,typeof(DateTime));
    }
    public async Task<DrillingProgramDraftReceipt> RecordProgramDraftAsync(CreateDrillingProgramDraft input,Func<Task> authority,
        Func<Task<DrillingProgramBasis>> readBasis,bool recover=false)
    {
        input.Require();ArgumentNullException.ThrowIfNull(authority);ArgumentNullException.ThrowIfNull(readBasis);var command=CompanyInventorySourceReview.Copy(input);var scope=command.Basis.Scope();var target=command.Basis.ExecutionDatabase;
        var source=_editor.GetDataSource(target.ConnectionName)??throw new InvalidOperationException("The original drilling execution source is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(target.ConnectionName)))throw new InvalidOperationException("The original drilling program source changed.");
            MigrationConnectionTarget.Validate(_editor,source,target.ConnectionName);
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task Basis()
        {
            await Guard(false);var current=await readBasis();current.Require();
            var original=command.Basis;
            var unchanged=current.Scope()==scope && current.ExecutionDatabase==target && current.Well.Uwi==original.Well.Uwi && current.Well.FieldId==original.Well.FieldId &&
                current.Funding.HandoffId==original.Funding.HandoffId && current.Funding.Progress!.NativeRevision==original.Funding.Progress!.NativeRevision && current.Funding.Source.Afe()==original.Funding.Source.Afe() && current.Funding.Source.Recorded==original.Funding.Source.Recorded &&
                DevelopmentPlanRelease.ReviewEvidenceRevision(current.Funding.Source.Current.Source.Review)==DevelopmentPlanRelease.ReviewEvidenceRevision(original.Funding.Source.Current.Source.Review) && JsonSerializer.Serialize(current.Funding.Source.Current.Source.Baseline)==JsonSerializer.Serialize(original.Funding.Source.Current.Source.Baseline);
            if(!unchanged || !recover && current.Revision!=original.Revision)throw new InvalidOperationException("The original approved investment, recorded well or drilling binding changed. Recover the exact original draft.");await Guard(false);
        }
        var repository=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(DRILLING_PROGRAM),target.ConnectionName,"DRILLING_PROGRAM");
        AppFilter Eq(string key,string value)=>new(){FieldName=key,FilterValue=value,Operator="="};
        async Task<DRILLING_PROGRAM?> Read()
        {
            var byId=(await repository.GetAsync([Eq("PROGRAM_ID",command.ProgramId())]))?.ToArray()??throw new InvalidOperationException("The original program rowset is unavailable.");
            var byIntent=(await repository.GetAsync([Eq("OWNER_TENANT_ID",scope.Company.TenantId),Eq("OWNER_COMPANY_BA_ID",scope.Company.BusinessAssociateId),Eq("PPDM_GUID",command.IntentId.ToString("D"))]))?.ToArray()??throw new InvalidOperationException("The original program intent rowset is unavailable.");
            if(byId.Length>1 || byIntent.Length>1 || byId.Any(row=>row is not DRILLING_PROGRAM) || byIntent.Any(row=>row is not DRILLING_PROGRAM) || byId.Length!=byIntent.Length)
                throw new InvalidOperationException("The original program identity or intent population is ambiguous.");
            if(byId.Length==0)return null;var row=(DRILLING_PROGRAM)byId[0];
            if(row.OWNER_TENANT_ID!=scope.Company.TenantId || row.OWNER_COMPANY_BA_ID!=scope.Company.BusinessAssociateId)throw new UnauthorizedAccessException("The program belongs to another original company.");
            if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(byIntent[0]) || CreateDrillingProgramDraft.FromRecord(row).Revision()!=command.Revision())throw new InvalidOperationException("The recorded original program intent or native row differs.");return CompanyInventorySourceReview.Copy(row);
        }
        await Basis();DRILLING_PROGRAM? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);RequireProgramSchema(source);recorded=await Read();await Guard(true);
            if(recorded is null && !recover)
            {
                var row=command.ToRecord(DateTime.UtcNow);_commonColumnHandler.PrepareForInsert(row,scope.ActorUserId);
                // Bind the actual shared audit producer's creation/change timestamps before the native insert.
                row.PROGRAM_DRAFT_REVISION=CreateDrillingProgramDraft.Digest(row);CreateDrillingProgramDraft.FromRecord(row);await Guard(true);
                await PPDMDataSourceTransaction.WithSourceAsync(source,()=>{BeepOperationResult.EnsureSuccess(source.InsertEntity("DRILLING_PROGRAM",row),"Insert","DRILLING_PROGRAM");return Task.FromResult(true);});await Guard(true);recorded=await Read();
                if(recorded is null)throw new InvalidOperationException("The original drilling program was not confirmed by native readback.");
            }
            await Guard(true);
        });
        await Guard(false);RequireProgramSchema(source);var closing=await Read();
        if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original program commit is unconfirmed. Recover its retained input.");
        await Basis();var receipt=new DrillingProgramDraftReceipt(command.Revision(),command.ProgramId(),closing,DateTimeOffset.UtcNow);receipt.Require(command);return CompanyInventorySourceReview.Copy(receipt);
    }
}
