using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public partial class ContractorManagementService
{
    public Task<CompanyPersonnelCredentialFacts> ReadPersonnelCredentialsAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelSourceQuery query,Func<Task> requireAuthority)=>ReadPersonnelCredentialsCoreAsync(context,query,requireAuthority,false);
    private async Task<CompanyPersonnelCredentialFacts> ReadPersonnelCredentialsCoreAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelSourceQuery query,Func<Task> requireAuthority,bool owned)
    {
        query.Require();await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel credential source is unavailable.");
        async Task Guard()
        {
            await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
            if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original credential native target changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        PPDMGenericRepository Repo(Type type,string table,string[] keys)
        {
            if(!source.CheckEntityExist(table))throw new InvalidOperationException("Recorded licence status/condition schema is unavailable.");
            var shape=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var actual=shape?.Fields?.Where(field=>field.IsKey).ToArray();
            if(actual is null || actual.Any(field=>field.AllowDBNull) || !actual.Select(field=>field.FieldName).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal),StringComparer.Ordinal))throw new InvalidOperationException("Complete native licence status/condition composite keys are unavailable.");
            return new(_editor,_commonColumnHandler,_defaults,_metadata,type,context.Database.ConnectionName,table);
        }
        async Task<(BA_LICENSE_STATUS[] Statuses,BA_LICENSE_COND[] Conditions)> Read(CompanyPersonnelSourceFacts personnel)
        {
            personnel.Require();if(personnel.Employment is null || personnel.License is null)return([],[]);
            await Guard();List<AppFilter> filters=[new(){FieldName="LICENSEE_BA_ID",Operator="=",FilterValue=query.EmployeeBaId},new(){FieldName="LICENSE_ID",Operator="=",FilterValue=query.LicenseId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}];
            var statuses=(await Repo(typeof(BA_LICENSE_STATUS),"BA_LICENSE_STATUS",["LICENSEE_BA_ID","LICENSE_ID","STATUS_ID"]).GetAsync(filters))
                .Select(row=>row is BA_LICENSE_STATUS status?CompanyInventorySourceReview.Copy(status):row).ToArray();await Guard();
            if(statuses.Length>4096 || statuses.Any(row=>row is not BA_LICENSE_STATUS value || value.LICENSEE_BA_ID!=query.EmployeeBaId || value.LICENSE_ID!=query.LicenseId || value.ACTIVE_IND!="Y"))throw new UnauthorizedAccessException("Licence status population is unavailable or foreign.");
            var conditions=(await Repo(typeof(BA_LICENSE_COND),"BA_LICENSE_COND",["LICENSEE_BA_ID","LICENSE_ID","CONDITION_ID"]).GetAsync(filters))
                .Select(row=>row is BA_LICENSE_COND condition?CompanyInventorySourceReview.Copy(condition):row).ToArray();await Guard();
            if(conditions.Length>4096 || conditions.Any(row=>row is not BA_LICENSE_COND value || value.LICENSEE_BA_ID!=query.EmployeeBaId || value.LICENSE_ID!=query.LicenseId || value.ACTIVE_IND!="Y"))throw new UnauthorizedAccessException("Licence condition population is unavailable or foreign.");
            var typedStatuses=statuses.Cast<BA_LICENSE_STATUS>().Select(CompanyInventorySourceReview.Copy).OrderBy(row=>row.STATUS_ID,StringComparer.Ordinal).ToArray();var typedConditions=conditions.Cast<BA_LICENSE_COND>().Select(CompanyInventorySourceReview.Copy).OrderBy(row=>row.CONDITION_ID,StringComparer.Ordinal).ToArray();
            if(typedStatuses.Select(row=>row.STATUS_ID).Distinct(StringComparer.Ordinal).Count()!=typedStatuses.Length || typedConditions.Select(row=>row.CONDITION_ID).Distinct(StringComparer.Ordinal).Count()!=typedConditions.Length)throw new InvalidOperationException("Licence status/condition identities are ambiguous.");
            return(typedStatuses,typedConditions);
        }
        // Current membership/licence closes before any status or competency condition is accessed.
        var firstPersonnel=await ReadPersonnelSourceCoreAsync(context,query,Guard,owned,false);var first=await Read(firstPersonnel);
        var closingPersonnel=await ReadPersonnelSourceCoreAsync(context,query,Guard,owned,false);var closing=await Read(closingPersonnel);
        var finalPersonnel=await ReadPersonnelSourceCoreAsync(context,query,Guard,owned,false);
        if(firstPersonnel.Employment!=closingPersonnel.Employment || firstPersonnel.License!=closingPersonnel.License || closingPersonnel.Employment!=finalPersonnel.Employment || closingPersonnel.License!=finalPersonnel.License || JsonSerializer.Serialize(first.Statuses)!=JsonSerializer.Serialize(closing.Statuses) || JsonSerializer.Serialize(first.Conditions)!=JsonSerializer.Serialize(closing.Conditions))throw new InvalidOperationException("Complete original company membership/licence/status/condition evidence changed during review.");
        await Guard();
        var statuses=closing.Statuses.Select(row=>new CompanyPersonnelLicenseStatusFacts(row.LICENSEE_BA_ID,row.LICENSE_ID,row.STATUS_ID,row.LICENSE_STATUS,row.LICENSE_STATUS_TYPE,row.EFFECTIVE_DATE,row.EXPIRY_DATE,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row)))).ToArray();
        var conditions=closing.Conditions.Select(row=>new CompanyPersonnelLicenseConditionFacts(row.LICENSEE_BA_ID,row.LICENSE_ID,row.CONDITION_ID,row.CONDITION_CODE,row.CONDITION_TYPE,row.CONDITION_VALUE>0?row.CONDITION_VALUE:null,row.CONDITION_VALUE_UOM,row.EXEMPT_IND,row.EFFECTIVE_DATE,row.EXPIRY_DATE,row.PPDM_GUID,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row)))).ToArray();
        return CompanyPersonnelCredentialFacts.Create(finalPersonnel,statuses,conditions,DateTime.UtcNow);
    }
}
