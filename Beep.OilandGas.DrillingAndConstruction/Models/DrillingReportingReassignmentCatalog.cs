using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.DrillingAndConstruction.Models;

public sealed record DrillingReportingReassignmentCatalog(
    [property:JsonRequired]DrillingReportingGeneration Responsibility,
    [property:JsonRequired]DrillingExecutionStatus Execution,
    [property:JsonRequired]IReadOnlyList<DrillingExecutionAssignmentOption> Options,
    [property:JsonRequired]IReadOnlyList<string> ManagerRoles,
    [property:JsonRequired]string ManagerPersona,
    [property:JsonRequired]string AuthorityRevision,
    [property:JsonRequired]string NativeRevision,
    [property:JsonRequired]string Revision,
    [property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public DevelopmentAssignedReviewScope Scope()=>Execution.Basis.Scope();
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Responsibility=Responsibility.ContentRevision(),Execution=Execution.Revision,Options,ManagerRoles,ManagerPersona,AuthorityRevision,NativeRevision}));
    public void Require(CompanyScope company,string actor,string assignmentId)
    {
        if(Responsibility is null || Execution is null || Options is null || ManagerRoles is null)throw new InvalidOperationException("The current reporting generation, execution and configured management/receiving authority are required.");
        Responsibility.Require();Responsibility.Root.RequireSource(Execution,false);var scope=Scope();
        if(scope.Company!=company || scope.ActorUserId!=actor || Responsibility.Root.InstanceId()!=assignmentId || Responsibility.Number>=256 ||
            Options.Count>128 || Options.Any(row=>row is null) || Options.Select(row=>row.ProcessId).Distinct(StringComparer.Ordinal).Count()!=Options.Count ||
            ManagerRoles.Count is <1 or >256 || ManagerRoles.Any(role=>!LeaseSummaryValidator.Identifier(role)) || ManagerRoles.Distinct(StringComparer.Ordinal).Count()!=ManagerRoles.Count ||
            !LeaseSummaryValidator.Identifier(ManagerPersona) || ManagerPersona.Length>64 || !ProcessDefinitionRevision.IsValid(AuthorityRevision) || !ProcessDefinitionRevision.IsValid(NativeRevision) ||
            Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || CheckedAtUtc<Execution.CheckedAtUtc ||
            JsonSerializer.SerializeToUtf8Bytes(this).Length>DrillingExecutionAssignment.MaximumBodyLength)
            throw new InvalidOperationException("The current company/account reporting transfer catalog is unconfirmed.");
        foreach(var option in Options)option.Require();
    }
}

public sealed record DrillingReportingReassignmentSelection(
    [property:JsonRequired]string AssignmentId,[property:JsonRequired]string CatalogRevision,
    [property:JsonRequired]string ProcessId,[property:JsonRequired]string ManagerRole)
{
    public void Require()
    {
        if(!LeaseSummaryValidator.Identifier(AssignmentId) || !AssignmentId.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal) ||
            !ProcessDefinitionRevision.IsValid(CatalogRevision) || !LeaseSummaryValidator.Identifier(ProcessId) || !LeaseSummaryValidator.Identifier(ManagerRole))
            throw new InvalidOperationException("Choose an exact current reporting catalog, configured workflow and management role.");
    }
}
