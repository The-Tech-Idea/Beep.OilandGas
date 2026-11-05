using System.Security.Claims;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.Text.Json;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public static class ProcessTransitionReviews
{
    public static ProcessTransitionReview Create(CompanyScope company,string actor,ClaimsPrincipal user,ProcessInstance instance,ProcessDefinition definition,IReadOnlyList<ProcessHistoryEntry> history, JOURNAL_ENTRY? journal = null, IReadOnlyList<JOURNAL_ENTRY_LINE>? journalLines = null, WorkOrderDetailModel? workOrder = null, IReadOnlyList<InspectionCondition>? inspections = null)
    {
        if(string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(instance.InstanceId) || string.IsNullOrWhiteSpace(instance.FieldId) || definition.ProcessId!=instance.ProcessId || history.Any(row=>row.InstanceId!=instance.InstanceId))
            throw new InvalidOperationException("The source action review cannot be confirmed.");
        var step=instance.StepInstances?.SingleOrDefault(row=>row.StepId==instance.CurrentStepId);
        var template=definition.Steps?.SingleOrDefault(row=>row.StepId==instance.CurrentStepId);
        var allowed=step is not null && template is not null && ProcessStepRoleChecks.CanAct(user,actor,instance,step,definition,template);
        var isJournal=instance.EntityType=="JOURNAL_ENTRY";
        if (journal is not null && (!isJournal || journal.JOURNAL_ENTRY_ID!=instance.EntityId))
            throw new InvalidOperationException("The recorded journal does not match the workflow source.");
        if(journal is not null && (journalLines is null || journalLines.Any(row => row is null ||
            string.IsNullOrWhiteSpace(row.JOURNAL_ENTRY_LINE_ID) || row.JOURNAL_ENTRY_ID!=journal.JOURNAL_ENTRY_ID || string.IsNullOrWhiteSpace(row.GL_ACCOUNT_ID)) ||
            journalLines.Select(row=>row.JOURNAL_ENTRY_LINE_ID).Distinct(StringComparer.Ordinal).Count()!=journalLines.Count) || journal is null && journalLines is not null)
            throw new InvalidOperationException("Recorded journal lines cannot be confirmed for this source.");
        var isWorkOrder=instance.EntityType=="WORK_ORDER";
        if(isWorkOrder && (definition.EntityType!="WORK_ORDER" || definition.ProcessType!="WORK_ORDER"))
            throw new InvalidOperationException("The recorded process definition does not establish a work-order workflow.");
        if(workOrder is not null && (!isWorkOrder || workOrder.InstanceId!=instance.EntityId || workOrder.FieldId!=instance.FieldId || workOrder.Source!="WORK_ORDER"))
            throw new InvalidOperationException("The recorded maintenance header does not match the workflow source.");
        if(workOrder is null && inspections is not null)throw new InvalidOperationException("Inspection evidence has no confirmed maintenance header.");
        WorkOrderSourceReview? maintenance=null;
        if(workOrder is not null)
        {
            RecordedWorkOrderComponentChecks.Require(workOrder);
            WorkOrderInspectionRules.RequireProjected(instance.FieldId,instance.EntityId,inspections);
            maintenance=new(workOrder.InstanceId,workOrder.FieldId,workOrder.InstanceName,workOrder.WoSubType,workOrder.RequestDate,workOrder.DueDate,workOrder.Instructions,
                Array.AsReadOnly(workOrder.Components.OrderBy(row=>row.ComponentId,StringComparer.Ordinal).ToArray()),
                Array.AsReadOnly(inspections!.OrderBy(row=>row.CondSeq).ToArray()),DateTimeOffset.UtcNow);
            maintenance.Require(instance.FieldId,instance.EntityId);
        }
        var independentStep=step is not null && template is not null && ProcessStepRoleChecks.RequiresIndependentActor(step,definition,template);
        var independentJournal=journal is not null && !string.IsNullOrWhiteSpace(journal.ROW_CREATED_BY) && journal.ROW_CREATED_BY!=actor;
        var choices=(definition.Transitions is null ? Enumerable.Empty<ProcessTransition>() : definition.Transitions.Values).Where(row=>allowed && !string.IsNullOrWhiteSpace(row.TransitionId) &&
            (!isWorkOrder || maintenance is not null) &&
            (!isJournal || journal is not null && (!(independentStep || row.RequiresApproval) || independentJournal)) &&
            ProcessStepRoleChecks.CanTransition(user,actor,instance,definition,row.FromStateId,row.ToStateId,row.Trigger))
            .OrderBy(row=>row.TransitionId,StringComparer.Ordinal).Select(row=>new ProcessTransitionChoice(row.TransitionId,row.FromStateId,row.ToStateId,row.Trigger,row.RequiresApproval)).ToArray();
        var roles=user.Identities.SelectMany(identity=>identity.Claims.Where(row=>row.Type==identity.RoleClaimType)).Select(row=>row.Value).Distinct().Order(StringComparer.Ordinal).ToArray();
        var evidence=JsonSerializer.Serialize(new { Company=company,Actor=actor,Roles=roles,Instance=instance,Definition=definition,WorkOrder=workOrder,Inspections=inspections?.OrderBy(row=>row.CondSeq),Journal=journal,JournalLines=journalLines?.OrderBy(row=>row.JOURNAL_ENTRY_LINE_ID,StringComparer.Ordinal),History=history.OrderBy(row=>row.HistoryId,StringComparer.Ordinal) });
        var revision=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(evidence)));
        // The existing journal writer stores book in SOURCE and a matching BOOK= remark. Do not infer a default.
        var book=journal is not null && !string.IsNullOrWhiteSpace(journal.SOURCE) && journal.REMARK=="BOOK="+journal.SOURCE ? journal.SOURCE : null;
        var sourceReview=journal is null ? null : new JournalSourceReview(journal.JOURNAL_ENTRY_ID,journal.ENTRY_NUMBER,journal.ENTRY_DATE,
            journal.STATUS,journal.DESCRIPTION,journal.ROW_CREATED_BY,journal.ROW_CREATED_DATE,journal.REFERENCE_NUMBER,
            journal.SOURCE_MODULE,book,journal.TOTAL_DEBIT,journal.TOTAL_CREDIT,DateTimeOffset.UtcNow,
            Array.AsReadOnly(journalLines!.OrderBy(row=>row.LINE_NUMBER ?? int.MaxValue).ThenBy(row=>row.JOURNAL_ENTRY_LINE_ID,StringComparer.Ordinal)
                .Select(row=>new JournalSourceLineReview(row.JOURNAL_ENTRY_LINE_ID,row.JOURNAL_ENTRY_ID,row.LINE_NUMBER,row.GL_ACCOUNT_ID,
                    row.DEBIT_AMOUNT,row.CREDIT_AMOUNT,row.CURRENCY_CODE,row.EXCHANGE_RATE,row.COST_CENTER_ID,row.DESCRIPTION)).ToArray()));
        var review=new ProcessTransitionReview(company,actor,instance.FieldId,instance.InstanceId,instance.CurrentState,instance.CurrentStepId,revision,Array.AsReadOnly(choices),sourceReview,maintenance);
        review.Require(company,actor,instance.InstanceId);
        return review;
    }
}
