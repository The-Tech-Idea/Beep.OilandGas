using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.DrillingAndConstruction.Services;
public partial class DrillingOperationService
{
    public async Task<DrillingProgramWellFacts> ReadProgramWellAsync(AssetDatabaseScope core,string field,string uwi,bool activeRequired,Func<Task> authority)
    {
        if(string.IsNullOrWhiteSpace(uwi) || uwi.Length>128 || uwi.Any(char.IsControl))throw new InvalidOperationException("Choose an exact recorded well UWI.");
        var source=_editor.GetDataSource(core.ConnectionName)??throw new InvalidOperationException("The original well source is unavailable.");
        async Task Guard(){PPDMDataSourceTransaction.RequireIdle(source);await authority();if(!ReferenceEquals(source,_editor.GetDataSource(core.ConnectionName)))throw new InvalidOperationException("The original well source changed.");MigrationConnectionTarget.Validate(_editor,source,core.ConnectionName);PPDMDataSourceTransaction.RequireIdle(source);}
        var repository=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WELL),core.ConnectionName,"WELL");
        async Task<DrillingProgramWellFacts> Read()
        {
            await Guard();var rows=(await repository.GetAsync([new AppFilter{FieldName="UWI",FilterValue=uwi,Operator="="},new AppFilter{FieldName="ASSIGNED_FIELD",FilterValue=field,Operator="="}]))?.ToArray()??throw new InvalidOperationException("The exact recorded well could not be read.");
            if(rows.Length!=1 || rows[0] is not WELL well || well.UWI!=uwi || well.ASSIGNED_FIELD!=field)throw new InvalidOperationException("The original well/FIELD membership is missing or ambiguous.");
            var result=DrillingProgramWellFacts.FromRecord(well,activeRequired);await Guard();return result;
        }
        var before=await Read();var after=await Read();if(before!=after)throw new InvalidOperationException("The recorded well changed during review.");return CompanyInventorySourceReview.Copy(after);
    }
    public async Task<DrillingProgramDraftList> ReadProgramDraftsAsync(DrillingProgramBasis basis,Func<Task> authority)
    {
        basis.Require();var source=_editor.GetDataSource(basis.ExecutionDatabase.ConnectionName)??throw new InvalidOperationException("The original program source is unavailable.");var scope=basis.Scope();
        async Task Guard(){PPDMDataSourceTransaction.RequireIdle(source);await authority();if(!ReferenceEquals(source,_editor.GetDataSource(basis.ExecutionDatabase.ConnectionName)))throw new InvalidOperationException("The original program source changed.");MigrationConnectionTarget.Validate(_editor,source,basis.ExecutionDatabase.ConnectionName);PPDMDataSourceTransaction.RequireIdle(source);}
        var repository=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(DRILLING_PROGRAM),basis.ExecutionDatabase.ConnectionName,"DRILLING_PROGRAM");
        async Task<DrillingProgramDraftList> Read()
        {
            await Guard();RequireProgramSchema(source);AppFilter Eq(string name,string value)=>new(){FieldName=name,FilterValue=value,Operator="="};var rows=(await repository.GetAsync([Eq("OWNER_TENANT_ID",scope.Company.TenantId),Eq("OWNER_COMPANY_BA_ID",scope.Company.BusinessAssociateId),Eq("FIELD_ID",scope.FieldId),Eq("WELL_ID",basis.Well.Uwi),Eq("AFE_ID",basis.Funding.Source.AfeId)]))?.ToArray()??throw new InvalidOperationException("The original program rowset is unavailable.");
            if(rows.Any(row=>row is not DRILLING_PROGRAM))throw new InvalidOperationException("The program rowset shape is invalid.");var result=new DrillingProgramDraftList(basis,rows.Cast<DRILLING_PROGRAM>().ToArray(),"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require();await Guard();return result;
        }
        var first=await Read();var second=await Read();if(first.Revision!=second.Revision)throw new InvalidOperationException("The original program rowset changed during readback.");return CompanyInventorySourceReview.Copy(second);
    }

}
