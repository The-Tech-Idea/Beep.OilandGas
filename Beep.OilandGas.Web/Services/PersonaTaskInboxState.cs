using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Shared view state for the existing task inbox and persona landing; no task or workflow persistence.</summary>
public sealed class PersonaTaskInboxState(IUnifiedTaskInboxService inbox, CompanyClientContext company, IPersonaContextService personas) : IDisposable
{
    public static string SalesPaymentMessage(CompanyAccountingStepReview evidence)=>evidence.Eligibility switch
    {
        "ELIGIBLE"=>evidence.RequiresIndependentActor?"The recorded independent payment decision permits review-task observation. Release remains separate.":"The original maker can acknowledge rejection. The payment remains rejected.",
        "SALES_PAYMENT_APPROVAL_REQUIRED"=>"Open the original payment and record its independent financial/receipt decision first.",
        "SALES_PAYMENT_REVIEWER_REQUIRED"=>"Only the original recorded payment reviewer can complete or route this task.",
        "SALES_PAYMENT_DECISION_PERMISSION_REQUIRED"=>"Current payment decision authority is required to observe this review.",
        "SALES_PAYMENT_FIELD_WRITE_REQUIRED"=>"Current original FIELD WRITE authority is required for this payment task.",
        "SALES_PAYMENT_REJECTION_RECORDED"=>"Route the recorded payment rejection to its original maker.",
        "SALES_PAYMENT_RETURN_CONFIGURATION_REQUIRED"=>"Configure the original-maker rejection-review branch before routing rejection.",
        "SALES_PAYMENT_PREPARER_REQUIRED"=>"Only the original payment maker can acknowledge rejection.",
        "SALES_PAYMENT_PREPARATION_PERMISSION_REQUIRED"=>"Current payment preparation authority is required for maker rejection review.",
        "SALES_PAYMENT_REJECTION_REQUIRED"=>"A recorded native payment rejection is required for this maker task.",
        "WORKFLOW_PERMISSION_REQUIRED"=>"Current workflow authority is required for this payment assignment.",
        _=>"Current assigned role or independent actor refuses this payment task."
    };
    public static string SalesJournalMessage(CompanyAccountingStepReview evidence)=>evidence.Eligibility switch
    {
        "ELIGIBLE"=>evidence.AccountingEvidenceAction==CompanyAccountingSourceTypes.JournalPosting?"The original poster and complete native journal/GL correlation are confirmed. Observe the assigned posting task.":evidence.RequiresIndependentActor?"This independent reviewer's recorded approval permits routing to the separate posting assignment.":"The original journal maker can close rejection review. The journal remains rejected.",
        "SALES_JOURNAL_APPROVAL_REQUIRED"=>"Record the independent journal financial/source decision before completing review.",
        "SALES_JOURNAL_REVIEWER_REQUIRED"=>"Only the account that recorded the journal decision can complete or route this review.",
        "SALES_JOURNAL_DECISION_PERMISSION_REQUIRED"=>"Current journal decision permission is required for this review task.",
        "SALES_JOURNAL_FIELD_WRITE_REQUIRED"=>"Current original FIELD WRITE access is required for this journal task.",
        "SALES_JOURNAL_REJECTION_RECORDED"=>"Route the recorded journal rejection to its original maker. Posting will be skipped.",
        "SALES_JOURNAL_RETURN_CONFIGURATION_REQUIRED"=>"The recorded workflow needs its separate posting and maker rejection-review branches.",
        "SALES_JOURNAL_PREPARER_REQUIRED"=>"Only the original journal maker can close rejection review.",
        "SALES_JOURNAL_PREPARATION_PERMISSION_REQUIRED"=>"Current journal preparation permission is required for maker rejection review.",
        "SALES_JOURNAL_REJECTION_REQUIRED"=>"A recorded journal rejection is required for maker review.",
        "SALES_JOURNAL_POSTING_REQUIRED"=>"Post the approved journal through the separate authorized posting form, then review this task again.",
        "SALES_JOURNAL_POSTER_REQUIRED"=>"Only the account that performed the confirmed native posting can complete this task.",
        "SALES_JOURNAL_POSTING_PERMISSION_REQUIRED"=>"Current journal posting permission is required to observe this task.",
        "WORKFLOW_PERMISSION_REQUIRED"=>"Current workflow authority is required for this journal assignment.",
        _=>"The current assigned role or independent account refuses this journal task."
    };
    public static string SalesReceivableMessage(CompanyAccountingStepReview evidence)=>evidence.Eligibility switch
    {
        "ELIGIBLE"=>evidence.RequiresIndependentActor?"This reviewer's recorded receivable approval permits assigned workflow completion.":"The original maker can close rejection review. The receivable remains rejected.",
        "SALES_RECEIVABLE_APPROVAL_REQUIRED"=>"Open the process and record the independent financial/source decision before completing this task.",
        "SALES_RECEIVABLE_REVIEWER_REQUIRED"=>"Only the account that recorded the receivable decision can complete or route this task.",
        "SALES_RECEIVABLE_DECISION_PERMISSION_REQUIRED"=>"Current receivable decision authority is required to complete this task.",
        "SALES_RECEIVABLE_FIELD_WRITE_REQUIRED"=>"Current original FIELD WRITE access is required to complete this receivable task.",
        "SALES_RECEIVABLE_REJECTION_RECORDED"=>"Route the recorded receivable rejection to the configured original-maker review.",
        "SALES_RECEIVABLE_RETURN_CONFIGURATION_REQUIRED"=>"The workflow needs a configured rejection-review step before routing this decision.",
        "SALES_RECEIVABLE_PREPARER_REQUIRED"=>"Only the original receivable maker can close rejection review.",
        "SALES_RECEIVABLE_PREPARATION_PERMISSION_REQUIRED"=>"Current receivable preparation authority is required for the maker's rejection review.",
        "SALES_RECEIVABLE_REJECTION_REQUIRED"=>"A recorded receivable rejection is required for this maker review.",
        "SALES_RECEIVABLE_SOURCE_CHANGED"=>"The original financial/receiving source or applicable policy changed. Reconcile it before assigned task completion.",
        "WORKFLOW_PERMISSION_REQUIRED"=>"Current workflow authority is required for this assignment.",
        _=>"The current assigned role or independent account refuses receivable task completion."
    };
    public static string SalesInvoiceMessage(CompanyAccountingStepReview evidence)=>evidence.Eligibility switch
    {
        "ELIGIBLE"=>evidence.RequiresIndependentActor?"This reviewer's recorded invoice approval permits assigned workflow observation.":"The original maker can close rejection review. The invoice remains rejected.",
        "SALES_INVOICE_APPROVAL_REQUIRED"=>"Open the process and record the independent tax, charges and entitlement decision before completing this task.",
        "SALES_INVOICE_REVIEWER_REQUIRED"=>"Only the account that recorded the invoice decision can complete or route this review task.",
        "SALES_INVOICE_DECISION_PERMISSION_REQUIRED"=>"Current invoice decision authority is required to observe this task.",
        "SALES_INVOICE_FIELD_WRITE_REQUIRED"=>"Current original FIELD WRITE access is required to observe this invoice decision.",
        "SALES_INVOICE_REJECTION_RECORDED"=>"Route the recorded invoice rejection to the configured original-maker review.",
        "SALES_INVOICE_RETURN_CONFIGURATION_REQUIRED"=>"The recorded workflow needs a configured rejection-review step before routing this decision.",
        "SALES_INVOICE_PREPARER_REQUIRED"=>"Only the original invoice maker can close rejection review.",
        "SALES_INVOICE_PREPARATION_PERMISSION_REQUIRED"=>"Current invoice preparation authority is required for the maker's rejection review.",
        "SALES_INVOICE_REJECTION_REQUIRED"=>"A recorded invoice rejection is required for this maker review.",
        "SALES_INVOICE_SOURCE_CHANGED"=>"The original physical source or applicable billing policy changed. Reconcile it before assigned task observation.",
        "WORKFLOW_PERMISSION_REQUIRED"=>"Current workflow authority is required for this assignment.",
        _=>"The current assigned role or independent account refuses invoice task observation."
    };
    public static string DeliveryMessage(CompanyAccountingStepReview evidence)=>evidence.Eligibility switch
    {
        "ELIGIBLE"=>evidence.RequiresIndependentActor?"Recorded delivery approval by this reviewer permits workflow completion after current role checks.":"The original preparer can complete rejection review. Delivery acceptance remains rejected.",
        "DELIVERY_APPROVAL_REQUIRED"=>"Open the process to review original receiving evidence and record an independent delivery decision.",
        "DELIVERY_REVIEWER_REQUIRED"=>"Only the account that recorded the native delivery decision can complete this task.",
        "DELIVERY_APPROVAL_PERMISSION_REQUIRED"=>"Current company delivery approval permission is required for task completion.",
        "DELIVERY_REJECTION_RECORDED"=>"The delivery rejection is recorded. Route it to configured original-preparer review.",
        "DELIVERY_RETURN_CONFIGURATION_REQUIRED"=>"The original workflow has no rejection-review step. Ask the workflow administrator to reconcile this handoff.",
        "DELIVERY_PREPARER_REQUIRED"=>"Only the original delivery preparer can complete rejection review.",
        "DELIVERY_PREPARATION_PERMISSION_REQUIRED"=>"Current company preparation permission is required for delivery rejection review.",
        "WORKFLOW_PERMISSION_REQUIRED"=>"The current company assignment lacks the required workflow permission.",
        _=>"The current role or independent actor requirement refuses delivery task completion."
    };
    private int version, loadSequence;
    private bool disposed, started;
    private UnifiedInbox? current;
    // Keep recorded counts and drill-down rows independent of mutable transport/consumer objects.
    public UnifiedInbox? Current => current is null ? null : Snapshot(current);
    private static T Snapshot<T>(T value) => System.Text.Json.JsonSerializer.Deserialize<T>(System.Text.Json.JsonSerializer.Serialize(value))!;
    public string WorkType { get; private set; } = "ALL";
    public string Attention { get; private set; } = "ALL";
    public string SourceKind { get; private set; } = "ALL";
    public string AccountingEvidence { get; private set; } = "ALL";
    public int EvidenceCount(string eligibility)=>SelectedRows.Count(row=>row.AccountingStepReview?.Eligibility==eligibility);
    public void SelectAccountingEvidence(string eligibility)
    {
        if(eligibility is not ("ALL" or "ELIGIBLE" or "RECORDED_DISPOSITION_REQUIRED" or "RECORDED_CONTROLLER_REQUIRED" or "REQUEST_CORRECTION_REQUIRED" or "ORIGINAL_PREPARER_REQUIRED" or "CORRECTION_DATA_REQUIRED" or "CORRECTION_ACKNOWLEDGEMENT_REQUIRED" or "CORRECTION_WORK_EVIDENCE_REQUIRED" or "COMMERCIAL_APPROVAL_REQUIRED" or "COMMERCIAL_REVIEWER_REQUIRED" or "COMMERCIAL_APPROVAL_PERMISSION_REQUIRED" or "COMMERCIAL_REJECTION_RECORDED" or "COMMERCIAL_RETURN_CONFIGURATION_REQUIRED" or "COMMERCIAL_PREPARER_REQUIRED" or "COMMERCIAL_PREPARATION_PERMISSION_REQUIRED" or "DELIVERY_APPROVAL_REQUIRED" or "DELIVERY_REVIEWER_REQUIRED" or "DELIVERY_APPROVAL_PERMISSION_REQUIRED" or "DELIVERY_REJECTION_RECORDED" or "DELIVERY_RETURN_CONFIGURATION_REQUIRED" or "DELIVERY_PREPARER_REQUIRED" or "DELIVERY_PREPARATION_PERMISSION_REQUIRED" or "SALES_INVOICE_APPROVAL_REQUIRED" or "SALES_INVOICE_REVIEWER_REQUIRED" or "SALES_INVOICE_DECISION_PERMISSION_REQUIRED" or "SALES_INVOICE_FIELD_WRITE_REQUIRED" or "SALES_INVOICE_REJECTION_RECORDED" or "SALES_INVOICE_RETURN_CONFIGURATION_REQUIRED" or "SALES_INVOICE_PREPARER_REQUIRED" or "SALES_INVOICE_PREPARATION_PERMISSION_REQUIRED" or "SALES_INVOICE_REJECTION_REQUIRED" or "SALES_INVOICE_SOURCE_CHANGED" or "SALES_RECEIVABLE_APPROVAL_REQUIRED" or "SALES_RECEIVABLE_REVIEWER_REQUIRED" or "SALES_RECEIVABLE_DECISION_PERMISSION_REQUIRED" or "SALES_RECEIVABLE_FIELD_WRITE_REQUIRED" or "SALES_RECEIVABLE_REJECTION_RECORDED" or "SALES_RECEIVABLE_RETURN_CONFIGURATION_REQUIRED" or "SALES_RECEIVABLE_PREPARER_REQUIRED" or "SALES_RECEIVABLE_PREPARATION_PERMISSION_REQUIRED" or "SALES_RECEIVABLE_REJECTION_REQUIRED" or "SALES_RECEIVABLE_SOURCE_CHANGED" or "SALES_PAYMENT_APPROVAL_REQUIRED" or "SALES_PAYMENT_REVIEWER_REQUIRED" or "SALES_PAYMENT_DECISION_PERMISSION_REQUIRED" or "SALES_PAYMENT_FIELD_WRITE_REQUIRED" or "SALES_PAYMENT_RETURN_CONFIGURATION_REQUIRED" or "SALES_PAYMENT_PREPARER_REQUIRED" or "SALES_PAYMENT_PREPARATION_PERMISSION_REQUIRED" or "SALES_PAYMENT_REJECTION_REQUIRED" or "SALES_JOURNAL_APPROVAL_REQUIRED" or "SALES_JOURNAL_REVIEWER_REQUIRED" or "SALES_JOURNAL_DECISION_PERMISSION_REQUIRED" or "SALES_JOURNAL_FIELD_WRITE_REQUIRED" or "SALES_JOURNAL_REJECTION_RECORDED" or "SALES_PAYMENT_REJECTION_RECORDED" or "SALES_JOURNAL_RETURN_CONFIGURATION_REQUIRED" or "SALES_JOURNAL_PREPARER_REQUIRED" or "SALES_JOURNAL_PREPARATION_PERMISSION_REQUIRED" or "SALES_JOURNAL_REJECTION_REQUIRED" or "SALES_JOURNAL_POSTING_REQUIRED" or "SALES_JOURNAL_POSTER_REQUIRED" or "SALES_JOURNAL_POSTING_PERMISSION_REQUIRED"))throw new ArgumentException("Choose an available recorded accounting work status.");
        if(disposed || current is null)return;
        AccountingEvidence=eligibility;WorkType="ALL";Attention="ALL";Changed?.Invoke();
    }
    public string? AssignedRole { get; private set; }
    public IReadOnlyList<string> AssignedRoles=>Rows.Select(row=>row.AssignedRole).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    private IEnumerable<UnifiedTask> SelectedRows=>Rows.Where(row=>MatchesSource(row,SourceKind) && (AssignedRole is null || row.AssignedRole==AssignedRole));
    public int RoleCount(string role)=>Rows.Count(row=>MatchesSource(row,SourceKind) && row.AssignedRole==role);
    public int ScheduleCount(string status)
    {
        if (status is not ("UNCONFIRMED" or "UNSCHEDULED")) throw new ArgumentException("Choose a recorded schedule state.");
        return SelectedRows.Count(row => row.SlaStatus == status);
    }
    public void SelectRole(string? role)
    {
        if(disposed || current is null)return;
        if(role is not null && !AssignedRoles.Contains(role,StringComparer.Ordinal))throw new ArgumentException("Choose a recorded assigned role.");
        AssignedRole=role;WorkType="ALL";Attention="ALL";Changed?.Invoke();
    }
    private IEnumerable<UnifiedTask> Rows => current is null ? [] : current.CriticalTasks.Concat(current.HighPriorityTasks).Concat(current.NormalTasks);
    private static readonly string[] SourceKinds = ["ALL", "AP_INVOICE", "INVOICE", "AR_PAYMENT", "JOURNAL_ENTRY", "AR_INVOICE", "PURCHASE_ORDER", "SALES_CONTRACT", "RUN_TICKET", "FIELD", "WORK_ORDER", "FIELD_DEVELOPMENT_PLAN", "AFE", "DRILLING_PROGRAM", "DRILLING_OPERATION"];
    public IReadOnlyList<string> AvailableSources => SourceKinds.Where(kind => kind != "ALL" && SourceCount(kind) > 0).ToArray();
    private static bool MatchesSource(UnifiedTask row,string kind) => kind=="ALL" ||
        kind=="FIELD" && row.ScopeKind=="FIELD" ||
        kind=="WORK_ORDER" && row.ScopeKind=="FIELD" && row.EntityType=="WORK_ORDER" ||
        kind=="FIELD_DEVELOPMENT_PLAN" && row.ScopeKind=="FIELD" && row.EntityType=="FIELD_DEVELOPMENT_PLAN" ||
        kind=="AFE" && row.ScopeKind=="FIELD" && row.EntityType=="AFE" ||
        kind=="DRILLING_PROGRAM" && row.ScopeKind=="FIELD" && row.EntityType=="DRILLING_PROGRAM" ||
        kind=="DRILLING_OPERATION" && row.ScopeKind=="FIELD" && row.EntityType=="DRILLING_OPERATION" ||
        kind is "AP_INVOICE" or "PURCHASE_ORDER" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT" && row.ScopeKind=="COMPANY_ACCOUNTING" && row.EntityType==kind;
    public int SourceCount(string kind)
    {
        RequireSourceKind(kind);
        return Rows.Count(row=>MatchesSource(row,kind));
    }
    public InboxCounts SelectedCounts
    {
        get
        {
            var rows=SelectedRows.ToArray();
            return new(){TotalPending=rows.Length,Approvals=rows.Count(row=>row.TaskType=="APPROVAL"),Reviews=rows.Count(row=>row.TaskType=="REVIEW"),
                DataEntry=rows.Count(row=>row.TaskType=="DATA_ENTRY"),Overdue=rows.Count(row=>row.SlaStatus=="BREACHED"),Critical=rows.Count(row=>row.Priority==1)};
        }
    }
    private static void RequireSourceKind(string kind)
    {
        if(!SourceKinds.Contains(kind, StringComparer.Ordinal))throw new ArgumentException("Choose an available assigned-work source.");
    }
    public void SelectSource(string kind)
    {
        RequireSourceKind(kind);
        if(disposed || current is null)return;
        SourceKind=kind;WorkType="ALL";Attention="ALL";Changed?.Invoke();
    }
    public IReadOnlyList<UnifiedTask> VisibleRows => SelectedRows
        .Where(row=>AccountingEvidence=="ALL" || row.AccountingStepReview?.Eligibility==AccountingEvidence)
        .Where(row => (WorkType == "ALL" || row.TaskType == WorkType) &&
            (Attention == "ALL" || Attention == "OVERDUE" && row.SlaStatus == "BREACHED" || Attention == "CRITICAL" && row.Priority == 1 || Attention == "UNCONFIRMED" && row.SlaStatus == "UNCONFIRMED" || Attention == "UNSCHEDULED" && row.SlaStatus == "UNSCHEDULED"))
        .OrderBy(row => row.Priority).ThenBy(row => row.DueDate is { Kind: DateTimeKind.Utc } due ? due.Ticks : long.MaxValue).ThenBy(row => row.TaskId, StringComparer.Ordinal).Select(Snapshot).ToArray();
    public void SetFilters(string workType, string attention)
    {
        if (workType is not ("ALL" or "APPROVAL" or "REVIEW" or "DATA_ENTRY") || attention is not ("ALL" or "OVERDUE" or "CRITICAL" or "UNCONFIRMED" or "UNSCHEDULED"))
            throw new ArgumentException("Choose an available assigned-work filter.");
        if (disposed || current is null) return;
        WorkType=workType; Attention=attention; Changed?.Invoke();
    }
    public void SelectSummary(string summary)
    {
        switch(summary)
        {
            case "ALL": SetFilters("ALL","ALL");break;
            case "APPROVAL": case "REVIEW": case "DATA_ENTRY": SetFilters(summary,"ALL");break;
            case "OVERDUE": case "CRITICAL": case "UNCONFIRMED": case "UNSCHEDULED": SetFilters("ALL",summary);break;
            default: throw new ArgumentException("Choose an available assigned-work summary.");
        }
    }
    public bool Loading { get; private set; }
    public event Action? Changed;
    public void Start() { if (disposed || started) return; started=true; company.Changed += CompanyChanged; personas.Changed += Clear; }
    private void CompanyChanged() { ++loadSequence; Clear(); }
    private void Clear() { ++version; current=null; Loading=false; WorkType="ALL"; Attention="ALL"; SourceKind="ALL"; AccountingEvidence="ALL"; AssignedRole=null; Changed?.Invoke(); }
    public bool MatchesWorkspace(IReadOnlyCollection<string>? routes) => routes is null || routes.Count==0 ||
        personas.CurrentPersona is { } persona && routes.Contains(persona.DefaultRoute,StringComparer.Ordinal);
    public async Task LoadAsync(IReadOnlyCollection<string>? workspaceRoutes = null)
    {
        if (disposed) return;
        Clear(); var request = ++loadSequence; var selected = company.Snapshot;
        var initialPersona = personas.CurrentPersona?.Code; var initialProfile = personas.CurrentProfile?.PersonaCode;
        await personas.EnsureLoadedAsync();
        if (disposed || loadSequence != request || selected != company.Snapshot ||
            initialPersona is not null && initialPersona != personas.CurrentPersona?.Code ||
            initialProfile is not null && initialProfile != personas.CurrentProfile?.PersonaCode) return;
        var persona = personas.CurrentPersona?.Code;
        if (selected.Selection is null || string.IsNullOrWhiteSpace(persona) || personas.CurrentProfile?.UserId != selected.Selection.UserId || personas.CurrentProfile.PersonaCode != persona || !MatchesWorkspace(workspaceRoutes)) return;
        var generation=++version; Loading=true; Changed?.Invoke();
        try
        {
            var response=await inbox.GetInboxAsync(persona);
            if (disposed || version!=generation || selected!=company.Snapshot || personas.CurrentPersona?.Code!=persona || !MatchesWorkspace(workspaceRoutes)) return;
            if (response is null) throw new InvalidOperationException("The assigned-work response was unavailable.");
            var result=WorkflowInboxReview.Capture(response,selected.Selection.Scope,selected.Selection.UserId,persona,selected.Selection.Roles);
            current=result;
        }
        finally { if (!disposed && generation==version) { Loading=false; Changed?.Invoke(); } }
    }
    public void Dispose() { disposed=true; ++loadSequence; company.Changed-=CompanyChanged; personas.Changed-=Clear; Clear(); }
}
