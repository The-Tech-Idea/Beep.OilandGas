using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService
{
    /// <summary>Requalifies the exact reviewed facility and physical intent schema without writing.</summary>
    public Task<FACILITY> ReadFacilityEditPreflightAsync(DevelopmentFacilityEdit input,Func<Task> authority)=>ReadFacilityEditPreflightCoreAsync(input,authority,false);
    private async Task<FACILITY> ReadFacilityEditPreflightCoreAsync(DevelopmentFacilityEdit input,Func<Task> authority,bool owned)
    {
        ArgumentNullException.ThrowIfNull(input);ArgumentNullException.ThrowIfNull(authority);
        var original=CompanyInventorySourceReview.Copy(input);original.Require();var context=original.Review.Context;
        if(_maintenanceAssetAccess is null || _resolveModuleConnection is null || context.Database.ConnectionName!=_connectionName)
            throw new InvalidOperationException("The selected CORE binding, current row policy and explicit module resolver are required.");
        var core=await _resolveModuleConnection("PPDM_CORE");var facility=await _resolveModuleConnection("FACILITY");
        if(core!=_connectionName || facility!=core)throw new InvalidOperationException("FACILITY and PPDM_CORE must use the same original Beep datasource for an atomic facility edit.");
        var source=_editor.GetDataSource(core)??throw new InvalidOperationException("The original facility source is unavailable.");
        async Task Guard()
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();
            if(await _resolveModuleConnection("PPDM_CORE")!=core || await _resolveModuleConnection("FACILITY")!=core || !ReferenceEquals(source,_editor.GetDataSource(core)))
                throw new InvalidOperationException("The original facility or intent binding changed during review.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        await RequireFacilityEditSchemaAsync(source,Guard);
        async Task<FACILITY> Read()
        {
            var rows=await MaintenanceAssetRowsCoreAsync<FACILITY>(Guard,[],true,
                new(){FieldName="FACILITY_ID",Operator="=",FilterValue=original.Review.Original.FACILITY_ID},
                new(){FieldName="FACILITY_TYPE",Operator="=",FilterValue=original.Review.Original.FACILITY_TYPE});
            if(rows.Length!=1 || rows[0].PRIMARY_FIELD_ID!=context.FieldId || DevelopmentFacilityEditReview.SourceDigest(rows[0])!=original.Review.SourceRevision)
                throw new InvalidOperationException("The complete original facility source or FIELD changed before edit.");
            var level=await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",rows[0],false);
            if(!AssetAccessLevelChecks.Meets(level,"WRITE"))throw new UnauthorizedAccessException("Direct WRITE access to the exact facility is required.");
            await Guard();return rows[0];
        }
        var opening=await Read();var closing=await Read();
        if(JsonSerializer.Serialize(opening)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original facility changed during preflight.");
        await Guard();return CompanyInventorySourceReview.Copy(closing);
    }
    private async Task RequireFacilityEditSchemaAsync(TheTechIdea.Beep.IDataSource source,Func<Task> Guard)
    {
        await MaintenanceAssetDefinitionsAsync(source,Guard,[new(typeof(FACILITY),["FACILITY_ID","FACILITY_TYPE"],[("PRIMARY_FIELD_ID","FIELD","FIELD_ID")]),new(typeof(FACILITY_EDIT_INTENT),["FACILITY_EDIT_INTENT_ID"],[])]);
        var intentShape=source.GetEntityStructure(new EntityStructure{EntityName="FACILITY_EDIT_INTENT",DatasourceEntityName="FACILITY_EDIT_INTENT"},true);
        foreach(var (name,capacity) in new[]{("FACILITY_EDIT_INTENT_ID",32),("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),
            ("FACILITY_ID",128),("FACILITY_TYPE",128),("COMMAND_JSON",262144),("RESULT_JSON",262144),("RESULT_REVISION",64),("RECORDED_REVISION",64)})
        {
            var columns=intentShape?.Fields?.Where(column=>column.FieldName==name).ToArray();
            if(columns is not {Length:1} || columns[0].AllowDBNull || columns[0].Fieldtype!=typeof(string).FullName || Math.Max(columns[0].Size,columns[0].Size1)<capacity)
                throw new InvalidOperationException("The complete original facility edit ownership, identity and result storage capacity is unconfirmed.");
        }
    }

}
