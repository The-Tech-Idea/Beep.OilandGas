using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public partial class ContractorManagementService
{
    public Task<CompanyPersonnelSourceFacts> ReadPersonnelSourceAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelSourceQuery query,Func<Task> requireAuthority)=>ReadPersonnelSourceCoreAsync(context,query,requireAuthority,false);
    private async Task<CompanyPersonnelSourceFacts> ReadPersonnelSourceCoreAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelSourceQuery query,Func<Task> requireAuthority,bool owned,bool paired=true)
    {
        query.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);if(query.Permit.Task.FieldId!=context.FieldId)throw new UnauthorizedAccessException("Original personnel FIELD differs.");await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel core source is unavailable.");
        async Task Guard(){await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original personnel native target changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        PPDMGenericRepository Repo(Type type,string table,string[] keys)
        {
            if(!source.CheckEntityExist(table))throw new InvalidOperationException("Recorded personnel source schema is unavailable.");var shape=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var actual=shape?.Fields?.Where(field=>field.IsKey).ToArray();if(actual is null || actual.Any(field=>field.AllowDBNull) || !actual.Select(field=>field.FieldName).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal),StringComparer.Ordinal))throw new InvalidOperationException("Complete recorded personnel composite-key metadata is unavailable.");return new(_editor,_commonColumnHandler,_defaults,_metadata,type,context.Database.ConnectionName,table);
        }
        async Task<(BA_EMPLOYEE? Employee,BA_LICENSE? License)> Read()
        {
            await Guard();var employees=(await Repo(typeof(BA_EMPLOYEE),"BA_EMPLOYEE",["EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"]).GetAsync([new(){FieldName="EMPLOYER_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},new(){FieldName="EMPLOYEE_BA_ID",Operator="=",FilterValue=query.EmployeeBaId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]))
                .Select(row=>row is BA_EMPLOYEE employee?CompanyInventorySourceReview.Copy(employee):row).ToArray();await Guard();
            if(employees.Length>1 || employees.Any(row=>row is not BA_EMPLOYEE employee || employee.EMPLOYER_BA_ID!=context.Company.BusinessAssociateId || employee.EMPLOYEE_BA_ID!=query.EmployeeBaId || employee.ACTIVE_IND!="Y"))throw new InvalidOperationException("Exact current company employee membership is ambiguous or foreign.");if(employees.Length==0)return(null,null);var employee=CompanyInventorySourceReview.Copy((BA_EMPLOYEE)employees[0]);
            // Membership closes before any personal licence evidence is accessed.
            await Guard();var licenses=(await Repo(typeof(BA_LICENSE),"BA_LICENSE",["LICENSEE_BA_ID","LICENSE_ID"]).GetAsync([new(){FieldName="LICENSEE_BA_ID",Operator="=",FilterValue=query.EmployeeBaId},new(){FieldName="LICENSE_ID",Operator="=",FilterValue=query.LicenseId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]))
                .Select(row=>row is BA_LICENSE license?CompanyInventorySourceReview.Copy(license):row).ToArray();await Guard();if(licenses.Length>1 || licenses.Any(row=>row is not BA_LICENSE license || license.LICENSEE_BA_ID!=query.EmployeeBaId || license.LICENSE_ID!=query.LicenseId || license.ACTIVE_IND!="Y"))throw new InvalidOperationException("Exact recorded employee licence is ambiguous or foreign.");return(employee,licenses.Length==0?null:CompanyInventorySourceReview.Copy((BA_LICENSE)licenses[0]));
        }
        // The credential owner supplies three membership observations around its two complete
        // status/condition observations. Its private primitive does not nest a second pair.
        var first=await Read();var closing=paired?await Read():first;if(JsonSerializer.Serialize(first.Employee)!=JsonSerializer.Serialize(closing.Employee) || JsonSerializer.Serialize(first.License)!=JsonSerializer.Serialize(closing.License))throw new InvalidOperationException("Complete original employee/licence evidence changed during review.");await Guard();
        var employee=closing.Employee is {} e?new CompanyEmployeeSourceFacts(e.EMPLOYER_BA_ID,e.EMPLOYEE_BA_ID,e.EMPLOYEE_OBS_NO,e.EMPLOYEE_POSITION,e.STATUS,e.EFFECTIVE_DATE,e.EXPIRY_DATE,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(e))):null;var license=closing.License is {} l?new CompanyPersonnelLicenseFacts(l.LICENSEE_BA_ID,l.LICENSE_ID,l.LICENSE_TYPE,l.LICENSE_NUM,l.GRANTED_BY_BA_ID,l.GRANTED_DATE,l.EFFECTIVE_DATE,l.EXPIRY_DATE,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(l))):null;return CompanyPersonnelSourceFacts.Create(context,query,employee,license,DateTime.UtcNow);
    }
}
