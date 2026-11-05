using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Web.Services;

/// <summary>Page-lifetime protection only; not a durable API command receipt.</summary>
public sealed class WorkOrderCreateState(CompanyClientContext company)
{
    private readonly HashSet<(CompanyClientSelection Company, string Field)> uncertain = [];
    public bool IsBlocked(string? field) => company.Selection is { } selected && field is not null && uncertain.Any(x => SameCompany(x.Company, selected) && x.Field == field);
    public Attempt Begin(CreateWorkOrderRequest source, string field)
    {
        var snapshot = company.Snapshot;
        if (snapshot.Selection is null || string.IsNullOrWhiteSpace(field) || source.FieldId != field ||
            string.IsNullOrWhiteSpace(source.WorkOrderNumber) || string.IsNullOrWhiteSpace(source.InstanceName) || string.IsNullOrWhiteSpace(source.Jurisdiction))
            throw new InvalidOperationException("Confirm company, FIELD, recorded order number, title and jurisdiction before creating an order.");
        // Key on company and actor identity, independent of selection version or role collection instances.
        if (uncertain.Any(x => SameCompany(x.Company, snapshot.Selection) && x.Field == field))
            throw new InvalidOperationException("An earlier create outcome is unconfirmed. Review recorded orders before another attempt.");
        var request = new CreateWorkOrderRequest { FieldId=source.FieldId, WorkOrderNumber=source.WorkOrderNumber, InstanceName=source.InstanceName,
            WoSubType=source.WoSubType, EquipmentId=source.EquipmentId, WellId=source.WellId, Description=source.Description,
            Jurisdiction=source.Jurisdiction, ProposedStart=source.ProposedStart, Duration=source.Duration };
        uncertain.Add((snapshot.Selection, field));
        return new(snapshot.Version, snapshot.Selection, request);
    }
    public bool IsCurrent(Attempt attempt, string? field) => company.Snapshot == (attempt.Version, attempt.Company) && field == attempt.Request.FieldId;
    public void Confirm(Attempt attempt, WorkOrderSummary? result)
    {
        if (result is null || string.IsNullOrWhiteSpace(result.InstanceId) || result.FieldId != attempt.Request.FieldId ||
            result.Instructions != attempt.Request.RecordedInstructions || result.WoSubType != attempt.Request.WoSubType || result.InstanceName != attempt.Request.WorkOrderNumber || result.State != string.Empty || result.Source != "WORK_ORDER" || string.IsNullOrWhiteSpace(result.EvidenceUnavailableReason))
            throw new InvalidOperationException("The create response does not confirm the submitted order. Review recorded orders before retrying.");
        RecordedWorkOrderComponentChecks.RequireCreated(attempt.Request,result);
        uncertain.RemoveWhere(x => SameCompany(x.Company, attempt.Company) && x.Field == attempt.Request.FieldId);
    }
    private static bool SameCompany(CompanyClientSelection a, CompanyClientSelection b) => a.UserId == b.UserId && a.Scope == b.Scope;
    public sealed record Attempt(long Version, CompanyClientSelection Company, CreateWorkOrderRequest Request);
}
