using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.Text.Json;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

public partial class ContractorManagementService:ICompanyWorkOrderExecutorIdentityOwner
{
    private async Task<CompanyWorkOrderExecutorIdentity> ReadExecutorIdentityCoreAsync(CompanyWorkOrderExecutionPrerequisites prerequisites,CompanyMaintenanceAssignedStageReview stage,Func<Task> authority,bool owned)
    {
        var context=prerequisites.Requirements.Context;var query=prerequisites.Requirements.Approval.Permit;
        prerequisites.Require(context.Company,context.ActorUserId,query);
        var identityContext=new CompanyPersonnelPolicyContext(context.Company,context.Database,context.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(context)));
        async Task<CompanyPersonnelIdentityCatalog> Catalog()
        {
            var rows=await ReadIdentityRowsAsync(identityContext,authority,owned);
            return CompanyPersonnelIdentityCatalog.Create(identityContext,rows.Select(CompanyPersonnelIdentityVersion.From).ToArray(),DateTime.UtcNow);
        }
        var first=await Catalog();var utc=DateTime.UtcNow;
        var links=first.Versions.Where(row=>row.UserId==context.ActorUserId && row.EffectiveFromUtc<=utc && row.EffectiveToUtc>utc).ToArray();
        if(links.Length!=1 || links[0].PersonBaId!=prerequisites.Personnel.EmployeeBaId)throw new UnauthorizedAccessException("The actual executing account must have one current authoritative identity for the exact assigned employee.");
        var person=await ReadIdentityPersonCoreAsync(identityContext,links[0].PersonBaId,authority,owned);
        var closing=await Catalog();var closingPerson=await ReadIdentityPersonCoreAsync(identityContext,links[0].PersonBaId,authority,owned);
        if(first.Revision!=closing.Revision || person!=closingPerson)throw new InvalidOperationException("Complete current executor identity/person/employment changed during review.");
        var result=new CompanyWorkOrderExecutorIdentity(prerequisites,stage,closing.Revision,links[0],closingPerson,DateTime.UtcNow,"");
        result=result with{Revision=CompanyWorkOrderExecutorIdentity.Digest(result)};result.Require();await authority();return result;
    }
    public Task<CompanyWorkOrderExecutorIdentity> ReadExecutorIdentityAsync(CompanyWorkOrderExecutionPrerequisites prerequisites,CompanyMaintenanceAssignedStageReview stage,Func<Task> authority)
        =>ReadExecutorIdentityCoreAsync(prerequisites,stage,authority,false);
    public async Task RequireExecutorIdentityCoreAsync(CompanyWorkOrderExecutorIdentity original,Func<Task> authority)
    {
        // This owner verifies person/assignment facts; the start owner separately reobserves qualified native assets.
        original.Require();var current=await ReadExecutorIdentityCoreAsync(original.Prerequisites,original.Stage,authority,true);
        if(current.ComparedAtUtc<original.ComparedAtUtc || current.SourceRevision()!=(original with{QualifiedWorkAssets=null}).SourceRevision())throw new InvalidOperationException("The original complete executor/assigned-person identity proof changed inside the owned boundary.");
    }
}
