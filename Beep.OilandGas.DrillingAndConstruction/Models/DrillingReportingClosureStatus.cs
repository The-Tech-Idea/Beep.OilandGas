using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.DrillingAndConstruction.Models;

/// <summary>Qualified original responsibility and historical closure; it does not attest current physical test readiness.</summary>
public sealed record DrillingReportingClosureStatus(
    [property:JsonRequired]DrillingReportingGeneration Responsibility,
    [property:JsonRequired]DrillingExecutionStatus Execution,
    [property:JsonRequired]IReadOnlyList<DrillingReportingReassignment> Transfers,
    [property:JsonRequired]DrillingReportingClosure? Closure,
    [property:JsonRequired]string NativeRevision,
    [property:JsonRequired]string Revision,
    [property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public DevelopmentAssignedReviewScope Scope()=>Execution.Basis.Scope();
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Responsibility=Responsibility.ContentRevision(),Execution=Execution.Revision,Transfers=Transfers.Select(row=>row.Revision()),Closure=Closure?.Revision(),NativeRevision}));
    public void Require(CompanyScope company,string actor,string assignmentId)
    {
        if(Responsibility is null || Execution is null || Transfers is null || Transfers.Count>255)throw new InvalidOperationException("The current responsibility, complete transfer history and current execution are required.");
        Responsibility.Require();var root=Responsibility.Root;root.RequireSource(Execution,false);var scope=Scope();
        var expected=DrillingReportingGeneration.Initial(root);
        foreach(var transfer in Transfers)
        {
            transfer.Require();
            if(transfer.Review.Responsibility.ContentRevision()!=expected.ContentRevision())throw new InvalidOperationException("The complete reporting generation history changed.");
            expected=DrillingReportingGeneration.After(transfer);
        }
        if(expected.ContentRevision()!=Responsibility.ContentRevision())throw new InvalidOperationException("The displayed reporting responsibility is not the latest qualified generation.");
        if(scope.Company!=company || scope.ActorUserId!=actor || root.InstanceId()!=assignmentId ||
            (actor!=root.Scope().ActorUserId && !root.Workflow.Authority.Recipients.Any(row=>row.UserId==actor) && !Transfers.Any(row=>row.Scope().ActorUserId==actor || row.Review.Destination.Authority.Recipients.Any(recipient=>recipient.UserId==actor))) ||
            !ProcessDefinitionRevision.IsValid(NativeRevision) || Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("Only an original reporting participant under current company/source read authority may inspect this responsibility.");
        if(Closure is {} closed)
        {
            closed.Require();
            if(closed.Review.Responsibility.ContentRevision()!=Responsibility.ContentRevision() || closed.ClosedAtUtc>CheckedAtUtc)
                throw new InvalidOperationException("The historical reporting closure differs from this exact original responsibility.");
        }
    }
}
