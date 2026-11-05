using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    private sealed record ReportingTransferProof(DrillingReportingReassignment Command, DateTime At, ReportingInitialSnapshot Before);

    private static List<ReportingTransferProof> ReportingTransfers(ProcessInstance process)
    {
        if (!process.ProcessData.Data.TryGetValue(DrillingReportingReassignment.PayloadKey, out var value)) return [];
        var json=JsonSerializer.Serialize(value);
        var proofs = JsonSerializer.Deserialize<List<ReportingTransferProof>>(json)
            ?? throw new InvalidOperationException("The recorded reporting transfer proof is unavailable.");
        if (proofs.Count is < 1 or > 255 || proofs.Any(row => row is null)) throw new InvalidOperationException("The reporting generation history is unavailable or exceeds its bound.");
        if(!ReportingJsonEquivalent(json,proofs))throw new InvalidOperationException("The reporting transfer proof contains unlisted or ambiguous fields.");
        return proofs;
    }

    private static bool ReportingJsonEquivalent(string? json,object expected)
    {
        if(string.IsNullOrEmpty(json))return false;
        using var document=JsonDocument.Parse(json);
        static bool Unique(JsonElement element)
        {
            if(element.ValueKind==JsonValueKind.Object)
            {
                var names=new HashSet<string>(StringComparer.Ordinal);
                foreach(var property in element.EnumerateObject())if(!names.Add(property.Name) || !Unique(property.Value))return false;
            }
            else if(element.ValueKind==JsonValueKind.Array)
                foreach(var item in element.EnumerateArray())if(!Unique(item))return false;
            return true;
        }
        return Unique(document.RootElement) && JsonElement.DeepEquals(document.RootElement,JsonSerializer.SerializeToElement(expected));
    }

    private static ProcessInstance ReportingModel(ProcessInstance process)
    {
        var result = CompanyInventorySourceReview.Copy(process);
        result.ProcessData.Data.Remove(DrillingReportingClosure.PayloadKey);
        result.ProcessData.Data.Remove(DrillingReportingReassignment.PayloadKey);
        result.History.RemoveAll(row => row.Action is "REPORTING_CLOSED" or "REPORTING_TRANSFERRED");
        return result;
    }
    public static string ReportingProcessRevision(ProcessInstance process)=>ReportingDigest(ReportingModel(process));

    // Projection only: callers must first admit the complete native assignment graph.
    public static IReadOnlyList<DrillingReportingReassignment> RecordedReportingReassignments(ProcessInstance process) =>
        ReportingTransfers(process).Select(row => CompanyInventorySourceReview.Copy(row.Command)).ToArray();

    internal static DrillingReportingReassignment? ReportingTransferForRouting(string json, DrillingExecutionAssignment original)
    {
        var data = JsonSerializer.Deserialize<Beep.OilandGas.Models.Data.Process.PROCESS_DATA>(json)
            ?? throw new InvalidOperationException("The reporting generation envelope is unavailable.");
        var last = ReportingTransfers(new() { ProcessData = data }).LastOrDefault()?.Command;
        if (last is not null) { last.Require(); if (ReportingDigest(last.Review.Responsibility.Root) != ReportingDigest(original)) throw new InvalidOperationException("The routed reporting generation belongs to another original assignment."); }
        return last;
    }

    private static DrillingReportingGeneration ReportingGeneration(ProcessInstance process, DrillingExecutionAssignment original)
    {
        var last = ReportingTransfers(process).LastOrDefault()?.Command;
        return last is null ? DrillingReportingGeneration.Initial(original) :
            new(original,last.Review.Responsibility.Number+1,last.Review.Destination,TransferStepId(process.InstanceId,last),ReportingDigest(last),
                last.Scope().ActorUserId,last.Review.ManagerRole,last.Review.ManagerPersona,last.Review.AuthorityRevision);
    }

    // Projection only, after the caller admits the complete native graph through its owner.
    public static DrillingReportingGeneration CurrentReportingGeneration(ProcessInstance process)
    {
        var root=DrillingExecutionAssignment.FromPayload(JsonSerializer.Serialize(process.ProcessData),process.InstanceId,process.FieldId,process.EntityId);
        var result=ReportingGeneration(process,root);result.Require();return CompanyInventorySourceReview.Copy(result);
    }

    // Pure identities for documents already validated by the enclosing native owner.
    private static string TransferStepId(string id,DrillingReportingReassignment command)=>DrillingReportingReassignment.StepIdFor(id,command.IntentId,command.Review.Destination.Step.StepId);
    private static string TransferHistoryId(string id,DrillingReportingReassignment command)=>DrillingReportingReassignment.HistoryIdFor(id,command.IntentId);

    private static ProcessStepInstance TransferNextStep(ReportingInitialSnapshot before, DrillingReportingReassignment command)
    {
        var step = command.Review.Destination.Step;
        return new() { InstanceId = before.Model.InstanceId, StepInstanceId = TransferStepId(before.Model.InstanceId,command), StepId = step.StepId,
            StepName = step.StepName, SequenceNumber = checked(before.Model.StepInstances.Max(row => row.SequenceNumber) + 1),
            Status = StepStatus.PENDING, RequiredRole = step.AssignedRole, ApprovalRequired = false, SlaHours = step.SlaHours };
    }

    private static string TransferGraph(ReportingInitialSnapshot current, IEnumerable<ReportingTransferProof> proofs, IEnumerable<PROCESS_HISTORY> histories) =>
        JsonSerializer.Serialize(new { Current = current, Transfers = proofs.ToArray(), History = histories.ToArray() });

    private static string TransferHistoryData(ReportingTransferProof proof, ReportingInitialSnapshot after) => JsonSerializer.Serialize(new
    {
        IntentRevision = ReportingDigest(proof.Command), BeforeRevision = ReportingDigest(proof.Before), AfterRevision = ReportingDigest(after), NewStepInstanceId = TransferStepId(after.Model.InstanceId,proof.Command)
    });

    private static void RequireTransferCreation(ModelEntityBase actual, string actor, DateTime at)
    {
        if (actual.ROW_CREATED_BY != actor || actual.ROW_CREATED_DATE is not { } created || created.Kind == DateTimeKind.Local || created < at)
            throw new InvalidOperationException("A transferred reporting row differs from its actual creation audit.");
        RequireReportingChanged(actual, actor, created);
    }

    private static void RequireTransferBusiness<T>(T actual, T expected) where T : ModelEntityBase
    {
        if (typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Any(property => !Equals(property.GetValue(actual), property.GetValue(expected))))
            throw new InvalidOperationException("The transferred reporting row differs from its declared business fields.");
    }

    private void RequireReportingTransferredBusiness(ReportingInitialSnapshot after, ReportingTransferProof proof)
    {
        var command = proof.Command; var before = proof.Before; var actor = command.Scope().ActorUserId; var at = proof.At;
        var expected = CompanyInventorySourceReview.Copy(before.Model);
        if (expected.Status != ProcessStatus.IN_PROGRESS || expected.CompletionDate is not null || !string.IsNullOrEmpty(expected.CompletedBy))
            throw new InvalidOperationException("Only an ongoing reporting generation can be transferred.");
        var former = expected.StepInstances.Single(row => row.StepInstanceId == command.Review.Responsibility.StepInstanceId);
        if (former.Status != StepStatus.PENDING || former.StartDate is not null || former.CompletionDate is not null)
            throw new InvalidOperationException("The former reporting generation is not pending.");
        former.Status = StepStatus.COMPLETED; former.StartDate = at; former.CompletionDate = at; former.CompletedBy = actor;
        former.Outcome = "REPORTING_TRANSFERRED"; former.Notes = command.Reason;
        var next = TransferNextStep(before, command); expected.StepInstances.Add(next);
        expected.CurrentStepId = next.StepId; expected.CurrentState = "REPORTING_REASSIGNED"; expected.ProcessData.LastUpdated = at;
        if (JsonSerializer.Serialize(expected) != JsonSerializer.Serialize(after.Model) || ReportingDigest(before.Start) != ReportingDigest(after.Start))
            throw new InvalidOperationException("The transfer changed undeclared reporting process, stage or initial history fields.");
        RequireReportingChanged(after.Row, actor, at);
        var row = CompanyInventorySourceReview.Copy(before.Row); CopyBusiness(row, ConvertToProcessInstanceEntity(expected));
        row.ROW_CHANGED_BY = actor; row.ROW_CHANGED_DATE = after.Row.ROW_CHANGED_DATE;
        if (ReportingDigest(row) != ReportingDigest(after.Row)) throw new InvalidOperationException("The transfer changed undeclared native process fields or original creation audit.");
        if (after.Steps.Count != before.Steps.Count + 1 || after.Steps.Select(step => step.PROCESS_STEP_INSTANCE_ID).Distinct(StringComparer.Ordinal).Count() != after.Steps.Count)
            throw new InvalidOperationException("The complete transferred reporting stage population changed.");
        foreach (var old in before.Steps)
        {
            var actual = after.Steps.SingleOrDefault(step => step.PROCESS_STEP_INSTANCE_ID == old.PROCESS_STEP_INSTANCE_ID)
                ?? throw new InvalidOperationException("A former reporting stage disappeared.");
            var saved = CompanyInventorySourceReview.Copy(old);
            if (old.PROCESS_STEP_INSTANCE_ID == command.Review.Responsibility.StepInstanceId)
            { CopyBusiness(saved, ConvertToProcessStepInstanceEntity(former)); RequireReportingChanged(actual, actor, at); saved.ROW_CHANGED_BY = actor; saved.ROW_CHANGED_DATE = actual.ROW_CHANGED_DATE; }
            if (ReportingDigest(saved) != ReportingDigest(actual)) throw new InvalidOperationException("The transfer altered a former stage or its original audit.");
        }
        var newStep = after.Steps.Single(row => row.PROCESS_STEP_INSTANCE_ID == next.StepInstanceId);
        RequireTransferBusiness(newStep, ConvertToProcessStepInstanceEntity(next)); RequireTransferCreation(newStep, actor, at);
        if (after.Tasks.Count != before.Tasks.Count + command.Review.Destination.Step.PersonaCodes.Count) throw new InvalidOperationException("The complete transferred reporting task population changed.");
        foreach (var old in before.Tasks)
        {
            var actual = after.Tasks.SingleOrDefault(task => task.CROSS_TASK_ID == old.CROSS_TASK_ID) ?? throw new InvalidOperationException("A former reporting task disappeared.");
            var saved = CompanyInventorySourceReview.Copy(old);
            if (command.Review.Responsibility.TaskIds().Contains(old.CROSS_TASK_ID, StringComparer.Ordinal))
            {
                if (old.TASK_STATUS != "PENDING") throw new InvalidOperationException("A former reporting task was not pending.");
                RequireReportingChanged(actual, actor, old.ROW_CHANGED_DATE!.Value);
                if (actual.ROW_CHANGED_DATE > at) throw new InvalidOperationException("Former task transfer audit exceeds its recording UTC.");
                saved.TASK_STATUS = "COMPLETED"; saved.ROW_CHANGED_BY = actor; saved.ROW_CHANGED_DATE = actual.ROW_CHANGED_DATE;
            }
            if (ReportingDigest(saved) != ReportingDigest(actual)) throw new InvalidOperationException("The transfer altered an unrelated task or its original audit.");
        }
        foreach (var persona in command.Review.Destination.Step.PersonaCodes)
        {
            var id = DrillingExecutionAssignmentReceipt.TaskId(expected.InstanceId, next.StepInstanceId, persona);
            var actual = after.Tasks.SingleOrDefault(task => task.CROSS_TASK_ID == id) ?? throw new InvalidOperationException("A new reporting destination task is missing.");
            var role = command.Review.Destination.Step.AssignedRole;
            var task = new CROSS_PERSONA_TASK { CROSS_TASK_ID = id, PROCESS_INSTANCE_ID = expected.InstanceId, PROCESS_STEP_INSTANCE_ID = next.StepInstanceId,
                TARGET_PERSONA_CODE = persona, ASSIGNED_ROLE = role, TASK_TYPE = "DATA_ENTRY", TASK_STATUS = "PENDING", PRIORITY = 3,
                ENTITY_TYPE = "DRILLING_OPERATION", ENTITY_ID = expected.EntityId, ENTITY_DESCRIPTION = command.Review.Responsibility.Root.Source.Basis.Source.Program.PROGRAM_NAME,
                ROUTE = WorkflowTaskRoute.For("FIELD", expected.InstanceId, "DRILLING_OPERATION"),
                TASK_CONTEXT_JSON = JsonSerializer.Serialize(new { assignedRole = role, entityType = "DRILLING_OPERATION", entityId = expected.EntityId, processInstanceId = expected.InstanceId, stepInstanceId = next.StepInstanceId, scopeKind = "FIELD" }) };
            RequireTransferBusiness(actual, task); RequireTransferCreation(actual, actor, at);
        }
    }

    private async Task<string> ConfirmReportingTransfersAsync(ProcessInstance process, DrillingExecutionAssignment original, CrossPersonaTaskRouter router)
    {
        var proofs = ReportingTransfers(process); var id = original.InstanceId();
        if (ReportingClosure(process) is not null || process.InstanceId != id || process.ProcessData.Data.Count != 2)
            throw new InvalidOperationException("The complete current reporting generation is not a qualified pending transfer graph.");
        var actual = await ReportingSnapshotAsync(process, router);
        var histories = (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID", id))).OfType<PROCESS_HISTORY>().ToArray();
        if (histories.Length != proofs.Count + 1 || process.History.Count != histories.Length) throw new InvalidOperationException("The complete reporting transfer history changed.");
        return await ConfirmReportingPendingAsync(actual,original,proofs,histories,router);
    }

    private async Task<string> ConfirmReportingPendingAsync(ReportingInitialSnapshot actual,DrillingExecutionAssignment original,
        IReadOnlyList<ReportingTransferProof> proofs,IReadOnlyList<PROCESS_HISTORY> histories,CrossPersonaTaskRouter router)
    {
        var id=original.InstanceId();var originalRevision=ReportingDigest(original);
        if(histories.Count!=proofs.Count+1 || histories.Count(row=>row.EVENT_TYPE=="PROCESS_STARTED")!=1 || histories.Any(row=>row.EVENT_TYPE is not ("PROCESS_STARTED" or "REPORTING_TRANSFERRED")))
            throw new InvalidOperationException("The complete pending reporting history is ambiguous.");
        if(proofs.Count==0)return await ConfirmExecutionAssignmentInitialAsync(actual.Model,original,router,actual);
        var initial = await ConfirmExecutionAssignmentInitialAsync(proofs[0].Before.Model, original, router, proofs[0].Before);
        var recorded = new List<PROCESS_HISTORY>();
        for (int index = 0; index < proofs.Count; index++)
        {
            var proof = proofs[index]; var command = proof.Command; command.Require();
            var previous=index==0?null:proofs[index-1].Command;
            var from=previous is null?DrillingReportingGeneration.Initial(original):new DrillingReportingGeneration(original,index+1,previous.Review.Destination,
                TransferStepId(id,previous),ReportingDigest(previous),previous.Scope().ActorUserId,previous.Review.ManagerRole,previous.Review.ManagerPersona,previous.Review.AuthorityRevision);
            var priorGraph = index == 0 ? initial : TransferGraph(proof.Before, proofs.Take(index), recorded);
            if (ReportingDigest(command.Review.Responsibility.Root) != originalRevision || command.Review.Responsibility.Number != index + 1 ||
                ReportingDigest(command.Review.Responsibility)!=ReportingDigest(from) ||
                command.Review.ProcessRevision != ReportingDigest(proof.Before.Model) || command.Review.NativeRevision != ProcessDefinitionRevision.FromJson(priorGraph) ||
                proof.At.Kind == DateTimeKind.Local || proof.At < command.ReassignedAtUtc.UtcDateTime || proof.At > DateTime.UtcNow)
                throw new InvalidOperationException("The frozen reporting transfer does not match its exact previous generation.");
            var after = index + 1 < proofs.Count ? proofs[index + 1].Before : actual;
            RequireReportingTransferredBusiness(after, proof);
            var historyId=TransferHistoryId(id,command);
            var matches = histories.Where(row => row.PROCESS_HISTORY_ID == historyId).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("The canonical reporting transfer identity is missing or ambiguous.");
            var history = matches[0];
            var expected = ConvertToProcessHistoryEntity(new ProcessHistoryEntry { HistoryId = historyId, InstanceId = id, StepInstanceId = command.Review.Responsibility.StepInstanceId,
                Action = "REPORTING_TRANSFERRED", PreviousState = proof.Before.Model.CurrentState, NewState = "REPORTING_REASSIGNED", Timestamp = proof.At, PerformedBy = command.Scope().ActorUserId, Notes = command.Reason });
            expected.EVENT_DATA_JSON = TransferHistoryData(proof, after); RequireTransferBusiness(history, expected); RequireTransferCreation(history, command.Scope().ActorUserId, proof.At); recorded.Add(history);
        }
        return TransferGraph(actual, proofs, recorded);
    }
}
