using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Xunit;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class PersonaTaskInboxStateTests
{
    private sealed class Personas : IPersonaContextService
    {
        public AppUserPersona? CurrentProfile { get; set; } = new() { UserId="actor", PersonaCode="ACCOUNTANT" };
        public AppPersona? CurrentPersona { get; set; } = new() { Code="ACCOUNTANT", Name="Accountant" };
        public IReadOnlyList<AppPersona> Catalog => CurrentPersona is null ? [] : [CurrentPersona];
        public event Action? Changed;
        public Task EnsureLoadedAsync() => Task.CompletedTask;
        public Task ReloadAsync() => Task.CompletedTask;
        public Task SwitchAsync(string code) { CurrentPersona=new() { Code=code }; Changed?.Invoke(); return Task.CompletedTask; }
    }
    private sealed class Inbox : IUnifiedTaskInboxService
    {
        public Func<Task<UnifiedInbox>> Load = () => Task.FromResult(Result());
        public int Calls;
        public string ExpectedPersona="ACCOUNTANT";
        public Task<UnifiedInbox> GetInboxAsync(string persona) { Calls++; Assert.Equal(ExpectedPersona,persona); return Load(); }
        public Task<InboxCounts> GetInboxCountsAsync(string persona) => throw new NotSupportedException();
        public Task<List<UnifiedTask>> GetFilteredTasksAsync(InboxFilter filter,string persona) => throw new NotSupportedException();
    }
    private sealed class DelayedPersonas : IPersonaContextService
    {
        public TaskCompletionSource Ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public AppUserPersona? CurrentProfile { get; private set; } = new() { UserId = "actor", PersonaCode = "ACCOUNTANT" };
        public AppPersona? CurrentPersona { get; private set; } = new() { Code = "ACCOUNTANT", Name = "Accountant" };
        public IReadOnlyList<AppPersona> Catalog => [CurrentPersona!];
        public event Action? Changed;
        public Task EnsureLoadedAsync() => Ready.Task;
        public Task ReloadAsync() => Ready.Task;
        public Task SwitchAsync(string code) { CurrentProfile = new() { UserId = "actor", PersonaCode = code }; CurrentPersona = new() { Code = code }; Changed?.Invoke(); return Task.CompletedTask; }
        public void Complete() { Changed?.Invoke(); Ready.SetResult(); }
    }
    [Fact]
    public async Task PendingPersonaInitializationCannotAdoptAnotherCompany()
    {
        using var company = Selected(); var personas = new DelayedPersonas(); var inbox = new Inbox();
        using var state = new PersonaTaskInboxState(inbox, company, personas); state.Start();
        var load = state.LoadAsync();
        var version = company.Clear();
        Assert.True(company.TrySelect(version, "actor", new(new("tenant", "B"), ["Reader"], [])));
        personas.Complete(); await load;
        Assert.Equal(0, inbox.Calls); Assert.Null(state.Current); Assert.False(state.Loading);
    }
    [Fact]
    public async Task OnlyLatestLoadCanContinueAfterPersonaInitialization()
    {
        using var company = Selected(); var personas = new DelayedPersonas(); var inbox = new Inbox();
        using var state = new PersonaTaskInboxState(inbox, company, personas); state.Start();
        var first = state.LoadAsync(); var second = state.LoadAsync();
        personas.Complete(); await Task.WhenAll(first, second);
        Assert.Equal(1, inbox.Calls); Assert.NotNull(state.Current); Assert.False(state.Loading);
    }
    [Fact]
    public async Task PendingPersonaInitializationCannotAdoptAnotherWorkspace()
    {
        using var company = Selected(); var personas = new DelayedPersonas(); var inbox = new Inbox();
        using var state = new PersonaTaskInboxState(inbox, company, personas); state.Start();
        var load = state.LoadAsync(); await personas.SwitchAsync("BUYER"); personas.Complete(); await load;
        Assert.Equal(0, inbox.Calls); Assert.Null(state.Current); Assert.False(state.Loading);
    }
    private static CompanyClientContext Selected()
    {
        var company=new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","A"),["Reader"],[]))); return company;
    }
    private static UnifiedInbox Result() => new() { Company=new("tenant","A"),ActorUserId="actor",PersonaCode="ACCOUNTANT",
        CriticalTasks=[new() { TaskId="task",TaskType="REVIEW",Priority=1,WorkflowName="process/opaque",StepName="step",ScopeKind="FIELD",AssignedRole="Reader",EntityId="invoice",EntityType="INVOICE",Route="/ppdm39/process/instances/process%2Fopaque" }],Counts=new() { TotalPending=1,Critical=1,Reviews=1 } };
    [Fact]
    public void TaskScheduleDoesNotInventTimezoneOrDeadline()
    {
        var now = new DateTime(2026, 10, 28, 9, 0, 0, DateTimeKind.Utc);
        Assert.Equal("UNSCHEDULED", WorkflowTaskSchedule.Classify("WELL", null, now));
        Assert.Equal("UNSCHEDULED", WorkflowTaskSchedule.Classify("DRILLING_OPERATION", now.AddDays(-1), now));
        Assert.Equal("UNCONFIRMED", WorkflowTaskSchedule.Classify("WELL", DateTime.SpecifyKind(now.AddDays(-1), DateTimeKind.Unspecified), now));
        Assert.Equal("UNCONFIRMED", WorkflowTaskSchedule.Classify("WELL", DateTime.SpecifyKind(now.AddDays(-1), DateTimeKind.Local), now));
        Assert.Equal("BREACHED", WorkflowTaskSchedule.Classify("WELL", now.AddSeconds(-1), now));
        Assert.Equal("ON_TRACK", WorkflowTaskSchedule.Classify("WELL", now, now));
        Assert.Throws<ArgumentException>(() => WorkflowTaskSchedule.Classify("WELL", now, DateTime.SpecifyKind(now, DateTimeKind.Unspecified)));
    }

    [Fact]
    public async Task UnconfirmedDueTimeIsNotOverdueOrSortedAheadOfKnownUtcTime()
    {
        using var company = Selected(); var response = Result(); var ambiguous = response.CriticalTasks[0];
        ambiguous.DueDate = new DateTime(2026, 1, 1, 12, 30, 0, DateTimeKind.Unspecified); ambiguous.SlaStatus = "UNCONFIRMED";
        var known = new UnifiedTask { TaskId = "utc-task", TaskType = "REVIEW", Priority = 1, WorkflowName = "process/utc", StepName = "step", ScopeKind = "FIELD", AssignedRole = "Reader", EntityId = "well", EntityType = "WELL", Route = WorkflowTaskRoute.For("FIELD", "process/utc", "WELL"), DueDate = new DateTime(2026, 12, 1, 12, 0, 0, DateTimeKind.Utc), SlaStatus = "ON_TRACK" };
        response.CriticalTasks.Add(known); response.Counts = new() { TotalPending = 2, Critical = 2, Reviews = 2, Overdue = 0 };
        using var state = new PersonaTaskInboxState(new Inbox { Load = () => Task.FromResult(response) }, company, new Personas()); state.Start(); await state.LoadAsync();
        Assert.Equal("utc-task", state.VisibleRows[0].TaskId); Assert.Equal(0, state.SelectedCounts.Overdue);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        var panel = ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.PersonaAssignedWorkPanel>(p => p.Add(x => x.Review, state));
        Assert.Contains("Recorded due date (time zone unconfirmed)", panel.Markup); Assert.Contains("Due (UTC)", panel.Markup);
        ambiguous.SlaStatus = "BREACHED"; Assert.Throws<InvalidOperationException>(() => WorkflowInboxReview.RequireTask(ambiguous, ["Reader"]));
    }

    [Fact]
    public async Task ScheduleFiltersShowOnlyCurrentCompanyAndSelectedSource()
    {
        using var company = Selected(); var response = Result(); var uncertain = response.CriticalTasks[0];
        uncertain.DueDate = new DateTime(2026, 1, 1, 12, 30, 0, DateTimeKind.Unspecified); uncertain.SlaStatus = "UNCONFIRMED";
        var unscheduled = new UnifiedTask { TaskId = "no-date", TaskType = "REVIEW", Priority = 1, WorkflowName = "process/no-date", StepName = "step", ScopeKind = "FIELD", AssignedRole = "Reader", EntityId = "well", EntityType = "WELL", Route = WorkflowTaskRoute.For("FIELD", "process/no-date", "WELL") };
        response.CriticalTasks.Add(unscheduled); response.Counts = new() { TotalPending = 2, Critical = 2, Reviews = 2 };
        using var state = new PersonaTaskInboxState(new Inbox { Load = () => Task.FromResult(response) }, company, new Personas()); state.Start(); await state.LoadAsync();
        Assert.Equal(1, state.ScheduleCount("UNCONFIRMED")); Assert.Equal(1, state.ScheduleCount("UNSCHEDULED")); Assert.Equal(0, state.SelectedCounts.Overdue);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        var panel = ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.PersonaAssignedWorkPanel>(p => p.Add(x => x.Review, state));
        panel.FindAll("button").Single(button => button.TextContent.Contains("Time zone unconfirmed: 1")).Click();
        Assert.Equal("task", Assert.Single(state.VisibleRows).TaskId);
        panel.FindAll("button").Single(button => button.TextContent.Contains("No confirmed deadline: 1")).Click();
        Assert.Equal("no-date", Assert.Single(state.VisibleRows).TaskId);
        state.SelectSource("DRILLING_OPERATION"); Assert.Empty(state.VisibleRows); Assert.Equal(0, state.ScheduleCount("UNSCHEDULED"));
        company.Clear(); Assert.Empty(state.VisibleRows); Assert.Equal(0, state.ScheduleCount("UNCONFIRMED"));
    }

    [Fact]
    public async Task WorkspaceShowsCompanyZoneOnlyForConfirmedMatchingCompanyAndUtcTime()
    {
        using var company = Selected(); var response = Result(); var task = response.CriticalTasks[0];
        task.DueDate = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc); task.SlaStatus = "ON_TRACK";
        using var state = new PersonaTaskInboxState(new Inbox { Load = () => Task.FromResult(response) }, company, new Personas()); state.Start(); await state.LoadAsync();
        var settings = new CompanyEnvironmentPreview(company.Selection!.Scope, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], []) { TimeZoneId = "Asia/Kuwait", DisplayCulture = "en-GB", DateTimeFormat = "yyyy-MM-dd HH:mm", Calendar = "Gregorian" };
        var effective = new CompanyEnvironmentEffectiveReview(company.Selection.Scope, new string('A',64), new string('B',64), null, new string('C',64), settings) { ReviewStatus = "Current" };
        var display = new CompanyEnvironmentDisplay(company.Version, effective); Assert.True(display.Matches(company));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices();
        var panel = ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.PersonaAssignedWorkPanel>(p => p.Add(x => x.Review, state).AddCascadingValue(display));
        Assert.Contains("Due (UTC)", panel.Markup); Assert.Contains("2026-01-01 15:00 +03:00 (Asia/Kuwait)", panel.Markup);
        Assert.Null((display with { Review = effective with { ReviewStatus = "ReviewRequired" } }).LocalTime(task.DueDate.Value));
        Assert.Null((display with { Review = effective with { Settings = settings with { TimeZoneId = "not/a-zone" } } }).LocalTime(task.DueDate.Value));
        Assert.Null(display.LocalTime(DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Unspecified)));
        var oslo = display with { Review = effective with { Settings = settings with { TimeZoneId = "Europe/Oslo" } } };
        Assert.Contains("13:00 +01:00", oslo.LocalTime(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)));
        Assert.Contains("14:00 +02:00", oslo.LocalTime(new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc)));
        var localized = display with { Review = effective with { Settings = settings with { DateTimeFormat = "dd/MM/yyyy HH:mm" } } };
        Assert.Contains("01/01/2026 15:00 +03:00", localized.LocalTime(task.DueDate.Value));
        var localizedPanel = ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.PersonaAssignedWorkPanel>(p => p.Add(x => x.Review, state).AddCascadingValue(localized));
        Assert.Contains("01/01/2026 15:00 +03:00", localizedPanel.Markup); Assert.Contains("Due (UTC)", localizedPanel.Markup);
        Assert.Null((display with { Review = effective with { Settings = settings with { DisplayCulture = "invalid-culture-name" } } }).LocalTime(task.DueDate.Value));
        Assert.Null((display with { Review = effective with { Settings = settings with { Calendar = "invalid" } } }).LocalTime(task.DueDate.Value));
        Assert.Null((display with { Review = effective with { Settings = settings with { DateTimeFormat = "yyyy-MM-dd '" } } }).LocalTime(task.DueDate.Value));
        var saudi = display with { Review = effective with { Settings = settings with { DisplayCulture = "ar-SA", Calendar = "Gregorian" } } };
        Assert.Contains("2026-01-01", saudi.LocalTime(task.DueDate.Value));
        var saudiCalendar = saudi with { Review = saudi.Review with { Settings = saudi.Review.Settings with { Calendar = "Culture" } } };
        Assert.DoesNotContain("2026-01-01", saudiCalendar.LocalTime(task.DueDate.Value));
        company.Clear(); Assert.False(display.Matches(company)); Assert.True(company.TrySelect(company.Version, "actor", new(new("tenant", "A"), ["Reader"], []))); Assert.False(display.Matches(company));
    }

    [Fact]
    public async Task CriticalTasksAndEncodedSourceIdentityArePreserved()
    {
        using var company=Selected(); var inbox=new Inbox(); using var state=new PersonaTaskInboxState(inbox,company,new Personas()); state.Start();
        await state.LoadAsync(); Assert.Equal(1,state.Current!.Counts.TotalPending); Assert.Equal("task",Assert.Single(state.Current.CriticalTasks).TaskId); Assert.False(state.Loading);
    }
    [Fact] public async Task ProgramTasksKeepQualifiedSourceCountsAndTheActualReviewerRoute()
    {
        using var company=Selected();var response=Result();var row=response.CriticalTasks[0];row.EntityType="DRILLING_PROGRAM";row.WorkflowName=DrillingProgramReviewStart.InstancePrefix+"original";row.TaskType="APPROVAL";row.ProgramReviewRevision=new('A',64);row.Route=WorkflowTaskRoute.For("FIELD",row.WorkflowName,row.EntityType);response.Counts.Approvals=1;response.Counts.Reviews=0;
        using var state=new PersonaTaskInboxState(new Inbox{Load=()=>Task.FromResult(response)},company,new Personas());state.Start();await state.LoadAsync();Assert.Contains("DRILLING_PROGRAM",state.AvailableSources);state.SelectSource("DRILLING_PROGRAM");Assert.Equal(1,state.SelectedCounts.Approvals);Assert.Equal(row.ProgramReviewRevision,Assert.Single(state.VisibleRows).ProgramReviewRevision);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();var panel=ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.PersonaAssignedWorkPanel>(p=>p.Add(c=>c.Review,state));Assert.Contains("Drilling program reviews",panel.Markup);Assert.Contains("Open assigned program review",panel.Markup);Assert.Equal("/drilling/program-review/assigned?processId="+Uri.EscapeDataString(row.WorkflowName),state.VisibleRows[0].Route);
        var invalid=System.Text.Json.JsonSerializer.Deserialize<UnifiedTask>(System.Text.Json.JsonSerializer.Serialize(row))!;invalid.ProgramReviewRevision=null;Assert.Throws<InvalidOperationException>(()=>WorkflowInboxReview.RequireTask(invalid,["Reader"]));invalid.ProgramReviewRevision=row.ProgramReviewRevision;invalid.EntityType="WELL";Assert.Throws<InvalidOperationException>(()=>WorkflowInboxReview.RequireTask(invalid,["Reader"]));
    }
    [Fact] public async Task InvestmentAssignmentsKeepQualifiedSourceCountsAndTheExactReceivingLink()
    {
        using var company=Selected();var response=Result();var row=response.CriticalTasks[0];row.EntityType="AFE";
        row.WorkflowName=DevelopmentInvestmentHandoff.InstancePrefix+"original";row.TaskType="APPROVAL";row.InvestmentReviewRevision=new('A',64);
        row.Route=WorkflowTaskRoute.For("FIELD",row.WorkflowName,row.EntityType);response.Counts.Approvals=1;response.Counts.Reviews=0;
        using var state=new PersonaTaskInboxState(new Inbox{Load=()=>Task.FromResult(response)},company,new Personas());state.Start();await state.LoadAsync();
        Assert.Contains("AFE",state.AvailableSources);state.SelectSource("AFE");Assert.Equal(1,state.SelectedCounts.Approvals);
        var visible=Assert.Single(state.VisibleRows);Assert.Equal(row.InvestmentReviewRevision,visible.InvestmentReviewRevision);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();
        var panel=ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.PersonaAssignedWorkPanel>(parameters=>parameters.Add(value=>value.Review,state));
        Assert.Contains("Investment review assignments",panel.Markup);Assert.Contains("Open assigned investment source",panel.Markup);
        Assert.Equal("/economics/afe?investmentHandoff="+Uri.EscapeDataString(row.WorkflowName),visible.Route);
        var invalid=System.Text.Json.JsonSerializer.Deserialize<UnifiedTask>(System.Text.Json.JsonSerializer.Serialize(row))!;invalid.InvestmentReviewRevision=null;
        Assert.Throws<InvalidOperationException>(()=>WorkflowInboxReview.RequireTask(invalid,["Reader"]));
        invalid.InvestmentReviewRevision=row.InvestmentReviewRevision;invalid.EntityType="WELL";
        Assert.Throws<InvalidOperationException>(()=>WorkflowInboxReview.RequireTask(invalid,["Reader"]));
    }
    [Fact] public async Task DevelopmentReviewsUseTheirQualifiedTaskRevisionAndExactReceivingRouteInSourceFilters()
    {
        using var company=Selected();var response=Result();var row=response.CriticalTasks[0];row.EntityType="FIELD_DEVELOPMENT_PLAN";
        row.TaskType="APPROVAL";row.DevelopmentReviewRevision=new('A',64);row.Route=WorkflowTaskRoute.For("FIELD",row.WorkflowName,row.EntityType);
        response.Counts.Approvals=1;response.Counts.Reviews=0;
        using var state=new PersonaTaskInboxState(new Inbox{Load=()=>Task.FromResult(response)},company,new Personas());state.Start();await state.LoadAsync();
        Assert.Contains("FIELD_DEVELOPMENT_PLAN",state.AvailableSources);state.SelectSource("FIELD_DEVELOPMENT_PLAN");
        Assert.Equal(1,state.SelectedCounts.Approvals);Assert.Equal("/development/fdp/assigned?processId="+Uri.EscapeDataString(row.WorkflowName),Assert.Single(state.VisibleRows).Route);
        Assert.Equal(row.DevelopmentReviewRevision,state.VisibleRows[0].DevelopmentReviewRevision);
        var invalid=System.Text.Json.JsonSerializer.Deserialize<UnifiedTask>(System.Text.Json.JsonSerializer.Serialize(row))!;invalid.DevelopmentReviewRevision=null;
        Assert.Throws<InvalidOperationException>(()=>WorkflowInboxReview.RequireTask(invalid,["Reader"]));
    }
    [Theory][InlineData("AP_INVOICE")][InlineData("PURCHASE_ORDER")][InlineData("JOURNAL_ENTRY")][InlineData("AR_PAYMENT")]
    public async Task CompanyAccountingTasksUseExactCompanyProcessRoute(string type)
    {
        using var company=Selected(); var result=Result(); var row=result.CriticalTasks[0];
        row.ScopeKind="COMPANY_ACCOUNTING";row.EntityType=type;row.WorkflowName="process/ميزانية%2F";
        row.Route=WorkflowTaskRoute.For(row.ScopeKind,row.WorkflowName,row.EntityType);
        var inbox=new Inbox{Load=()=>Task.FromResult(result)};using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();
        await state.LoadAsync();Assert.Equal(row.Route,Assert.Single(state.VisibleRows).Route);Assert.StartsWith("/accounting/processes/",row.Route);
        Assert.Contains("%252F",row.Route);Assert.Equal(1,state.Current!.Counts.Reviews);
    }
    [Theory][InlineData(true,"SALES_PAYMENT_APPROVAL_REQUIRED")][InlineData(true,"SALES_PAYMENT_REJECTION_RECORDED")][InlineData(false,"ELIGIBLE")]
    public async Task PaymentAssignmentsPreserveExactRoleRouteCountsAndNativeEvidenceFilters(bool independent,string eligibility)
    {
        using var company=Selected();var result=Result();var row=result.CriticalTasks[0];row.ScopeKind="COMPANY_ACCOUNTING";row.EntityType="AR_PAYMENT";row.StepDisplayName="Recorded payment review";row.Route=WorkflowTaskRoute.For(row.ScopeKind,row.WorkflowName,row.EntityType);row.TaskType=independent?"APPROVAL":"REVIEW";result.Counts.Approvals=independent?1:0;result.Counts.Reviews=independent?0:1;
        row.AccountingStepReview=new(company.Selection!.Scope,"actor",row.WorkflowName,"definition",row.EntityType,row.EntityId,row.StepName,"configured-step",row.StepDisplayName,row.AssignedRole,independent?"maker":"actor","starter",independent,independent?"Workflow.Approve":"Workflow.Execute",eligibility=="ELIGIBLE",eligibility,new string('A',64),new string('B',64),new string('C',64),new string('D',64),new string('E',64),new string('F',64));
        var inbox=new Inbox{Load=()=>Task.FromResult(result)};using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();state.SelectSource("AR_PAYMENT");state.SelectAccountingEvidence(eligibility);Assert.Equal(1,state.SourceCount("AR_PAYMENT"));Assert.Equal(1,state.EvidenceCount(eligibility));Assert.Equal(1,state.SelectedCounts.TotalPending);Assert.Equal(row.Route,Assert.Single(state.VisibleRows).Route);Assert.Contains("payment",PersonaTaskInboxState.SalesPaymentMessage(row.AccountingStepReview),StringComparison.OrdinalIgnoreCase);
        company.Clear();Assert.Empty(state.VisibleRows);Assert.Null(state.Current);
    }
    [Theory][InlineData("SALES_JOURNAL_APPROVAL",true,"SALES_JOURNAL_APPROVAL_REQUIRED")][InlineData("SALES_JOURNAL_POSTING",false,"ELIGIBLE")][InlineData("SALES_JOURNAL_REJECTION_REVIEW",false,"SALES_JOURNAL_PREPARER_REQUIRED")]
    public async Task JournalAssignmentsPreserveActionRoleSourceCountsAndEvidenceFilters(string action,bool independent,string eligibility)
    {
        using var company=Selected();var result=Result();var row=result.CriticalTasks[0];row.ScopeKind="COMPANY_ACCOUNTING";row.EntityType="JOURNAL_ENTRY";row.StepDisplayName="Recorded journal task";row.Route=WorkflowTaskRoute.For(row.ScopeKind,row.WorkflowName,row.EntityType);
        row.AccountingStepReview=new(company.Selection!.Scope,"actor",row.WorkflowName,"definition",row.EntityType,row.EntityId,row.StepName,"configured-step",row.StepDisplayName,row.AssignedRole,"maker","maker",independent,independent?"Workflow.Approve":"Workflow.Execute",eligibility=="ELIGIBLE",eligibility,new string('A',64),new string('B',64),new string('C',64),new string('D',64),new string('E',64),new string('F',64),AccountingEvidenceAction:action);
        var inbox=new Inbox{Load=()=>Task.FromResult(result)};using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();state.SelectSource("JOURNAL_ENTRY");state.SelectAccountingEvidence(eligibility);
        Assert.Equal(1,state.SourceCount("JOURNAL_ENTRY"));Assert.Equal(1,state.EvidenceCount(eligibility));Assert.Equal(1,state.SelectedCounts.TotalPending);Assert.Equal(action,Assert.Single(state.VisibleRows).AccountingStepReview!.AccountingEvidenceAction);
        if(action==CompanyAccountingSourceTypes.JournalPosting){Assert.Contains("original poster",PersonaTaskInboxState.SalesJournalMessage(row.AccountingStepReview));Assert.Throws<InvalidOperationException>(()=>(row.AccountingStepReview with{AccountingEvidenceAction=CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY")}).Require(company.Selection.Scope,"actor",row.WorkflowName));}
        Assert.Throws<InvalidOperationException>(()=>(row.AccountingStepReview with{AccountingEvidenceAction=null}).Require(company.Selection.Scope,"actor",row.WorkflowName));company.Clear();Assert.Empty(state.VisibleRows);
    }
    [Theory][InlineData("missingScope")][InlineData("unsupported")][InlineData("wrongRoute")][InlineData("missingRole")][InlineData("foreignRole")]
    public async Task CompanyTaskScopeAndRouteCannotBeInferredOrSubstituted(string fault)
    {
        using var company=Selected();var result=Result();var row=result.CriticalTasks[0];row.ScopeKind="COMPANY_ACCOUNTING";row.EntityType="AP_INVOICE";
        row.Route=WorkflowTaskRoute.For(row.ScopeKind,row.WorkflowName,row.EntityType);
        switch(fault){case "missingScope":row.ScopeKind="";break;case "unsupported":row.EntityType="WELL";break;
            case "wrongRoute":row.Route="/ppdm39/process/instances/process%2Fopaque";break;case "missingRole":row.AssignedRole="";break;case "foreignRole":row.AssignedRole="Unassigned";break;}
        var inbox=new Inbox{Load=()=>Task.FromResult(result)};using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync());Assert.Null(state.Current);
    }
    [Theory]
    [InlineData("company")] [InlineData("actor")] [InlineData("persona")] [InlineData("count")] [InlineData("route")] [InlineData("duplicate")]
    public async Task UnconfirmedScopeCountsOrRoutesCannotPublish(string fault)
    {
        using var company=Selected(); var inbox=new Inbox(); var result=Result();
        switch(fault) {
            case "company":result.Company=new("tenant","B");break;
            case "actor":result.ActorUserId="other";break;
            case "persona":result.PersonaCode="ENGINEER";break;
            case "count":result.Counts.TotalPending=2;break;
            case "route":result.CriticalTasks[0].Route="https://other.example";break;
            default:result.CriticalTasks.Add(result.CriticalTasks[0]);break;
        }
        inbox.Load=()=>Task.FromResult(result); using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync());Assert.Null(state.Current);Assert.False(state.Loading);
    }
    [Fact]
    public async Task CompanyChangeClearsWorkAndRejectsLateAwayAndBackResponse()
    {
        using var company=Selected();var inbox=new Inbox();using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        var pending=new TaskCompletionSource<UnifiedInbox>(TaskCreationOptions.RunContinuationsAsynchronously);inbox.Load=()=>pending.Task;var load=state.LoadAsync();
        company.Clear();Assert.Null(state.Current);Assert.True(company.TrySelect(company.Version,"actor",new(new("tenant","A"),["Reader"],[])));
        pending.SetResult(Result());await load;Assert.Null(state.Current);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task MissingCompanyOrEligiblePersonaNeverRequestsTasks(bool missingPersona)
    {
        using var company=missingPersona?Selected():new CompanyClientContext();var inbox=new Inbox();var personas=new Personas();if(missingPersona)personas.CurrentPersona=null;
        using var state=new PersonaTaskInboxState(inbox,company,personas);state.Start();await state.LoadAsync();Assert.Equal(0,inbox.Calls);Assert.Null(state.Current);
    }
    [Fact]
    public async Task FailedRefreshDoesNotShowAnEmptySuccessOrOldCounts()
    {
        using var company=Selected();var inbox=new Inbox();using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        inbox.Load=()=>throw new HttpRequestException("unavailable");await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadAsync());Assert.Null(state.Current);Assert.False(state.Loading);
    }
    [Fact]
    public async Task PersonaChangeAndDisposalInvalidateWork()
    {
        using var company=Selected();var personas=new Personas();var inbox=new Inbox();var state=new PersonaTaskInboxState(inbox,company,personas);state.Start();state.Start();await state.LoadAsync();
        await personas.SwitchAsync("ENGINEER");Assert.Null(state.Current);state.Dispose();var events=0;state.Changed+=()=>events++;company.Clear();Assert.Equal(0,events);
    }
    private static UnifiedInbox WorkSummary()
    {
        UnifiedTask Row(string id,string type,int priority,bool overdue)=>new(){TaskId=id,TaskType=type,Priority=priority,
            WorkflowName="process/"+id,StepName="step",ScopeKind="FIELD",AssignedRole="Reader",EntityId="source/"+id,EntityType="INVOICE",
            Route="/ppdm39/process/instances/"+Uri.EscapeDataString("process/"+id),SlaStatus=overdue?"BREACHED":"UNSCHEDULED",
            DueDate=overdue?new DateTime(2026,10,1,0,0,0,DateTimeKind.Utc):null};
        return new(){Company=new("tenant","A"),ActorUserId="actor",PersonaCode="ACCOUNTANT",
            CriticalTasks=[Row("A","APPROVAL",1,true)],HighPriorityTasks=[Row("B","REVIEW",2,false)],
            NormalTasks=[Row("D","REVIEW",3,false),Row("C","DATA_ENTRY",3,true)],
            Counts=new(){TotalPending=4,Critical=1,Overdue=2,Approvals=1,Reviews=2,DataEntry=1}};
    }
    private static UnifiedInbox SourceSummary()
    {
        var result=WorkSummary();
        foreach(var row in result.CriticalTasks.Concat(result.NormalTasks))
        {
            row.ScopeKind="COMPANY_ACCOUNTING";row.EntityType=row.TaskId=="C"?"PURCHASE_ORDER":"AP_INVOICE";
            row.Route=WorkflowTaskRoute.For(row.ScopeKind,row.WorkflowName,row.EntityType);
        }
        // Matching entity text alone cannot move FIELD work into the company invoice summary.
        result.HighPriorityTasks[0].EntityType="AP_INVOICE";
        return result;
    }
    [Fact]
    public async Task AvailableSourceFiltersFollowRecordedScopeAndClearWithCompany()
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(SourceSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        Assert.Equal(new[]{"AP_INVOICE","PURCHASE_ORDER","FIELD"},state.AvailableSources);
        state.SelectSource("AP_INVOICE");state.SetFilters("DATA_ENTRY","OVERDUE");
        Assert.Equal(new[]{"AP_INVOICE","PURCHASE_ORDER","FIELD"},state.AvailableSources);
        Assert.Equal(0,state.SourceCount("AR_PAYMENT"));Assert.DoesNotContain("AR_PAYMENT",state.AvailableSources);
        company.Clear();Assert.Empty(state.AvailableSources);
    }
    [Fact]
    public async Task EngineeringFieldAssignmentsDoNotShowCompanyAccountingSourceFilters()
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(WorkSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        // FIELD tasks with INVOICE entity text still belong to the FIELD source, not company accounting.
        Assert.Equal(new[]{"FIELD"},state.AvailableSources);
        inbox.Load=()=>Task.FromResult(new UnifiedInbox{Company=new("tenant","A"),ActorUserId="actor",PersonaCode="ACCOUNTANT"});
        await state.LoadAsync();Assert.Empty(state.AvailableSources);Assert.Empty(state.VisibleRows);
    }
    [Theory][InlineData("Reader","A")][InlineData("Controller","D")]
    public async Task AssignedRoleAndSourceSummariesRetainExactContributingRows(string role,string taskId)
    {
        using var company=Selected();company.TrySelect(company.Clear(),"actor",new(new("tenant","A"),["Reader","Controller"],[]));
        var result=SourceSummary();foreach(var row in result.NormalTasks)row.AssignedRole="Controller";
        var inbox=new Inbox{Load=()=>Task.FromResult(result)};using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        Assert.Equal(new[]{"Controller","Reader"},state.AssignedRoles);state.SelectRole(role);Assert.Equal(2,state.SelectedCounts.TotalPending);
        state.SelectSource("AP_INVOICE");Assert.Equal(1,state.SelectedCounts.TotalPending);Assert.Equal(1,state.RoleCount(role));Assert.Equal(taskId,Assert.Single(state.VisibleRows).TaskId);
        Assert.Throws<ArgumentException>(()=>state.SelectRole("Unassigned"));Assert.Equal(role,state.AssignedRole);
        state.SelectSummary(role=="Reader"?"APPROVAL":"REVIEW");Assert.Equal(taskId,Assert.Single(state.VisibleRows).TaskId);
        state.SelectRole(null);Assert.Equal(2,state.SelectedCounts.TotalPending);Assert.Equal(2,state.VisibleRows.Count);
        state.SelectRole(role);inbox.Load=()=>throw new InvalidOperationException("Provider unavailable");await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync());
        Assert.Null(state.Current);Assert.Null(state.AssignedRole);Assert.Empty(state.AssignedRoles);Assert.Empty(state.VisibleRows);
    }
    [Theory][InlineData("AP_INVOICE","A,D",2)][InlineData("PURCHASE_ORDER","C",1)][InlineData("FIELD","B",1)]
    public async Task SourceSummaryUsesExactOwnedScopeAndContributingTasks(string source,string ids,int count)
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(SourceSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        state.SetFilters("DATA_ENTRY","OVERDUE");state.SelectSource(source);
        Assert.Equal(ids,string.Join(",",state.VisibleRows.Select(row=>row.TaskId)));Assert.Equal(count,state.SourceCount(source));Assert.Equal(count,state.SelectedCounts.TotalPending);
        Assert.Equal("ALL",state.WorkType);Assert.Equal("ALL",state.Attention);Assert.Equal(4,state.Current!.Counts.TotalPending);Assert.Equal(1,inbox.Calls);
        Assert.All(state.VisibleRows,row=>Assert.Equal(WorkflowTaskRoute.For(row.ScopeKind,row.WorkflowName,row.EntityType),row.Route));
    }
    [Theory][InlineData("APPROVAL","A",1)][InlineData("REVIEW","D",1)][InlineData("OVERDUE","A",1)][InlineData("CRITICAL","A",1)][InlineData("DATA_ENTRY","",0)]
    public async Task TaskSummariesRespectSelectedSourceAndKeepMatchingCounts(string summary,string ids,int count)
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(SourceSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();state.SelectSource("AP_INVOICE");state.SelectSummary(summary);
        Assert.Equal("AP_INVOICE",state.SourceKind);Assert.Equal(ids,string.Join(",",state.VisibleRows.Select(row=>row.TaskId)));Assert.Equal(count,state.VisibleRows.Count);
        Assert.Equal(2,state.SelectedCounts.TotalPending);Assert.Equal(1,state.SelectedCounts.Approvals);Assert.Equal(1,state.SelectedCounts.Reviews);Assert.Equal(0,state.SelectedCounts.DataEntry);
    }
    [Fact] public async Task FailedReloadClearsSourceSummaryWithoutFabricatedSuccess()
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(SourceSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();state.SelectSource("AP_INVOICE");
        inbox.Load=()=>throw new HttpRequestException();await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadAsync());
        Assert.Null(state.Current);Assert.Equal("ALL",state.SourceKind);Assert.Empty(state.VisibleRows);
    }
    [Fact] public async Task ChangedCompanyResetsSourceAndRejectsLateSummary()
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(SourceSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();state.SelectSource("AP_INVOICE");
        var response=new TaskCompletionSource<UnifiedInbox>(TaskCreationOptions.RunContinuationsAsynchronously);inbox.Load=()=>response.Task;var pending=state.LoadAsync();
        company.Clear();Assert.True(company.TrySelect(company.Version,"actor",new(new("tenant","A"),["Reader"],[])));response.SetResult(SourceSummary());await pending;
        Assert.Null(state.Current);Assert.Equal("ALL",state.SourceKind);Assert.Empty(state.VisibleRows);
    }
    [Fact] public async Task InvalidSourceDoesNotAlterReviewedDrilldownAndConsumersCannotAlterCounts()
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(SourceSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();state.SelectSource("AP_INVOICE");
        Assert.Throws<ArgumentException>(()=>state.SelectSource("WELL"));Assert.Throws<ArgumentException>(()=>state.SourceCount("COMPANY"));
        state.SelectedCounts.TotalPending=99;state.VisibleRows[0].ScopeKind="FIELD";Assert.Equal(2,state.SelectedCounts.TotalPending);Assert.Equal("AP_INVOICE",state.SourceKind);
    }
    [Theory]
    [InlineData("ALL","A,B,C,D",4)][InlineData("APPROVAL","A",1)][InlineData("REVIEW","B,D",2)]
    [InlineData("DATA_ENTRY","C",1)][InlineData("OVERDUE","A,C",2)][InlineData("CRITICAL","A",1)]
    public async Task SummaryDrillsIntoExactContributingRecords(string summary,string ids,int count)
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(WorkSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        state.SetFilters("REVIEW","CRITICAL");state.SelectSummary(summary);
        Assert.Equal(ids,string.Join(",",state.VisibleRows.Select(row=>row.TaskId)));Assert.Equal(count,state.VisibleRows.Count);
        Assert.Equal(4,state.Current!.Counts.TotalPending);Assert.Equal(1,inbox.Calls);
        Assert.All(state.VisibleRows,row=>Assert.Equal("/ppdm39/process/instances/"+Uri.EscapeDataString(row.WorkflowName),row.Route));
    }
    [Fact] public async Task WorkTypeAndAttentionIntersectWithoutChangingSourceCounts()
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(WorkSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        state.SetFilters("REVIEW","OVERDUE");Assert.Empty(state.VisibleRows);Assert.Equal(2,state.Current!.Counts.Reviews);
        state.SetFilters("DATA_ENTRY","OVERDUE");Assert.Equal("C",Assert.Single(state.VisibleRows).TaskId);
        state.SetFilters("APPROVAL","CRITICAL");Assert.Equal("A",Assert.Single(state.VisibleRows).TaskId);Assert.Equal(1,inbox.Calls);
    }
    [Fact] public async Task ContextChangeClearsDrilldownAndSelections()
    {
        using var company=Selected();var personas=new Personas();var inbox=new Inbox{Load=()=>Task.FromResult(WorkSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,personas);state.Start();await state.LoadAsync();
        state.SetFilters("APPROVAL","OVERDUE");await personas.SwitchAsync("ENGINEER");
        Assert.Empty(state.VisibleRows);Assert.Equal("ALL",state.WorkType);Assert.Equal("ALL",state.Attention);
    }
    [Fact] public async Task FailedReloadCannotRetainDrilldownRecords()
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(WorkSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();state.SelectSummary("OVERDUE");
        inbox.Load=()=>throw new HttpRequestException();await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadAsync());
        Assert.Empty(state.VisibleRows);Assert.Null(state.Current);Assert.Equal("ALL",state.Attention);
    }
    [Fact] public async Task InvalidFilterCannotBecomeAnUnfilteredSuccess()
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(WorkSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();state.SelectSummary("REVIEW");
        Assert.Throws<ArgumentException>(()=>state.SelectSummary("UNKNOWN"));Assert.Throws<ArgumentException>(()=>state.SetFilters("ALL","UNKNOWN"));
        Assert.Equal("B,D",string.Join(",",state.VisibleRows.Select(row=>row.TaskId)));
    }
    [Fact] public async Task DifferentLandingRouteCannotRequestOrShowAnotherWorkspaceQueue()
    {
        using var company=Selected();var personas=new Personas();personas.CurrentPersona!.DefaultRoute="/accounting/dashboard";
        var inbox=new Inbox();using var state=new PersonaTaskInboxState(inbox,company,personas);state.Start();
        await state.LoadAsync(["/production/engineer-workspace"]);Assert.Equal(0,inbox.Calls);Assert.Null(state.Current);
        Assert.False(state.MatchesWorkspace(["/production/engineer-workspace"]));
    }
    [Theory][InlineData("/production/engineer-workspace")][InlineData("/production/operations")][InlineData("/ppdm39/workorder")][InlineData("/accounting/revenue-workbench")][InlineData("/accounting/cost-workbench")][InlineData("/reservoir/workspace")][InlineData("/exploration/prospect-board")][InlineData("/hse/dashboard")][InlineData("/ppdm39/hse")][InlineData("/executive/dashboard")][InlineData("/ppdm39/data-management")][InlineData("/admin/dashboard")][InlineData("/data/audit")]
    public async Task CatalogRouteCanExposeItsOwnAssignedSourceQueue(string route)
    {
        using var company=Selected();var personas=new Personas();personas.CurrentPersona!.DefaultRoute=route;
        var inbox=new Inbox();using var state=new PersonaTaskInboxState(inbox,company,personas);state.Start();
        await state.LoadAsync([route]);Assert.Equal(1,inbox.Calls);Assert.Equal("task",Assert.Single(state.VisibleRows).TaskId);
        Assert.Equal("/ppdm39/process/instances/process%2Fopaque",state.VisibleRows[0].Route);
    }
    [Fact] public async Task ChangedRouteWhileLoadingDiscardsLateQueue()
    {
        using var company=Selected();var personas=new Personas();personas.CurrentPersona!.DefaultRoute="/production/operations";
        var pending=new TaskCompletionSource<UnifiedInbox>(TaskCreationOptions.RunContinuationsAsynchronously);var inbox=new Inbox{Load=()=>pending.Task};
        using var state=new PersonaTaskInboxState(inbox,company,personas);state.Start();var load=state.LoadAsync(["/production/operations"]);
        personas.CurrentPersona.DefaultRoute="/accounting/dashboard";pending.SetResult(Result());await load;Assert.Null(state.Current);
    }
    [Fact] public async Task GeneralInboxAcceptsCurrentEligibleWorkspaceWithoutALandingRestriction()
    {
        using var company=Selected();var inbox=new Inbox();using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();
        await state.LoadAsync();Assert.Equal(1,inbox.Calls);Assert.NotNull(state.Current);
    }
    [Fact] public async Task ReviewedTransportCannotChangeCountsOrSourceAfterPublication()
    {
        using var company=Selected();var result=WorkSummary();var inbox=new Inbox{Load=()=>Task.FromResult(result)};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        result.Counts.TotalPending=90;result.Company=new("tenant","other");result.CriticalTasks[0].Route="https://foreign.example";result.HighPriorityTasks.Clear();
        Assert.Equal(4,state.Current!.Counts.TotalPending);Assert.Equal(new("tenant","A"),state.Current.Company);
        state.SelectSummary("CRITICAL");Assert.Equal("/ppdm39/process/instances/process%2FA",Assert.Single(state.VisibleRows).Route);
        state.SelectSummary("REVIEW");Assert.Equal("B,D",string.Join(",",state.VisibleRows.Select(row=>row.TaskId)));
    }
    [Fact] public async Task ConsumerSnapshotsCannotAlterTheNextDrilldown()
    {
        using var company=Selected();var inbox=new Inbox{Load=()=>Task.FromResult(WorkSummary())};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        state.Current!.Counts.Approvals=70;state.Current!.CriticalTasks.Clear();state.VisibleRows[0].Route="https://foreign.example";
        state.SelectSummary("APPROVAL");Assert.Equal(1,state.Current!.Counts.Approvals);
        Assert.Equal("/ppdm39/process/instances/process%2FA",Assert.Single(state.VisibleRows).Route);
    }
    [Theory][InlineData("/procurement",1)][InlineData("/accounting/dashboard",0)]
    public async Task ConfiguredBuyerLandingUsesItsOwnPersonaAndRecordedProcess(string route,int calls)
    {
        using var company=Selected();var personas=new Personas{
            CurrentProfile=new(){UserId="actor",PersonaCode="BUYER"},
            CurrentPersona=new(){Code="BUYER",Name="Company buyer",DefaultRoute=route}};
        var result=Result();result.PersonaCode="BUYER";result.CriticalTasks[0].EntityType="PURCHASE_ORDER";result.CriticalTasks[0].EntityId="order/1";
        var inbox=new Inbox{ExpectedPersona="BUYER",Load=()=>Task.FromResult(result)};
        using var state=new PersonaTaskInboxState(inbox,company,personas);state.Start();await state.LoadAsync(["/procurement"]);
        Assert.Equal(calls,inbox.Calls);
        if(calls==0)Assert.Null(state.Current);
        else {Assert.Equal("BUYER",state.Current!.PersonaCode);Assert.Equal("order/1",Assert.Single(state.VisibleRows).EntityId);Assert.Equal("/ppdm39/process/instances/process%2Fopaque",state.VisibleRows[0].Route);}
    }
    [Fact] public async Task StaleProfileCannotRequestAnotherSelectedPersonaQueue()
    {
        using var company=Selected();var personas=new Personas{CurrentPersona=new(){Code="BUYER",DefaultRoute="/procurement"}};
        var inbox=new Inbox();using var state=new PersonaTaskInboxState(inbox,company,personas);state.Start();
        await state.LoadAsync(["/procurement"]);Assert.Equal(0,inbox.Calls);Assert.Null(state.Current);
    }
    private sealed class NoBrowser : Microsoft.JSInterop.IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("Static assigned-work rendering has no browser.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    private sealed class StaticNavigation : Microsoft.AspNetCore.Components.NavigationManager
    {
        public StaticNavigation() => Initialize("https://render.invalid/","https://render.invalid/procurement");
        protected override void NavigateToCore(string uri,bool forceLoad)=>throw new InvalidOperationException("Static render cannot navigate.");
    }
    [Theory][InlineData("ALL")][InlineData("APPROVAL")][InlineData("EMPTY")][InlineData("CLEARED")][InlineData("COMPANY")]
    [InlineData("AP_INVOICE")][InlineData("PURCHASE_ORDER")][InlineData("FIELD")]
    public async Task ActualAssignedWorkPanelRendersRecordedCountsAndExactFilteredSources(string mode)
    {
        var sourceMode=mode is "AP_INVOICE" or "PURCHASE_ORDER" or "FIELD";
        using var company=Selected();var result=sourceMode?SourceSummary():WorkSummary();result.NormalTasks[0].EntityDescription="<script>recorded demand</script>";
        result.CriticalTasks[0].FromPersona="MAINTENANCE";result.CriticalTasks[0].FromUserName="<script>requester</script>";
        if(mode=="COMPANY"){var row=result.CriticalTasks[0];row.ScopeKind="COMPANY_ACCOUNTING";row.EntityType="AP_INVOICE";row.Route=WorkflowTaskRoute.For(row.ScopeKind,row.WorkflowName,row.EntityType);}
        if(mode=="EMPTY"){result.CriticalTasks=[];result.HighPriorityTasks=[];result.NormalTasks=[];result.Counts=new();}
        var inbox=new Inbox{Load=()=>Task.FromResult(result)};
        using var state=new PersonaTaskInboxState(inbox,company,new Personas());state.Start();await state.LoadAsync();
        if(mode=="APPROVAL")state.SelectSummary(mode);
        if(sourceMode)state.SelectSource(mode);
        if(mode=="CLEARED")company.Clear();
        var services=new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        Microsoft.Extensions.DependencyInjection.LoggingServiceCollectionExtensions.AddLogging(services);
        Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<Microsoft.JSInterop.IJSRuntime,NoBrowser>(services);
        Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<Microsoft.AspNetCore.Components.NavigationManager,StaticNavigation>(services);
        MudBlazor.Services.ServiceCollectionExtensions.AddMudServices(services);
        await using var provider=Microsoft.Extensions.DependencyInjection.ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services);
        var logger=Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>(provider);
        await using var renderer=new Microsoft.AspNetCore.Components.Web.HtmlRenderer(provider,logger);
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{
            var rendered=await renderer.RenderComponentAsync<Fixtures.PersonaAssignedWorkRenderHost>(Microsoft.AspNetCore.Components.ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));return rendered.ToHtmlString();});
        if(mode=="CLEARED"){Assert.DoesNotContain("Pending work:",html);Assert.DoesNotContain("process%2F",html);return;}
        Assert.Contains("Company buyer",html);Assert.Contains("tenant / A",html);
        if(sourceMode)
        {
            Assert.Contains("Invoice workflow assignments: 2",html);Assert.Contains("Purchase-order workflow assignments: 1",html);Assert.Contains("Field workflow assignments: 1",html);
            Assert.Contains("Selected source: "+mode,html);Assert.Contains("aria-pressed=\"true\"",html);
            Assert.Contains(mode=="AP_INVOICE"?"Pending work: 2":"Pending work: 1",html);
            var expected=state.VisibleRows.Select(row=>row.Route).ToHashSet(StringComparer.Ordinal);
            foreach(var row in SourceSummary().CriticalTasks.Concat(SourceSummary().HighPriorityTasks).Concat(SourceSummary().NormalTasks))
                if(expected.Contains(row.Route))Assert.Contains(row.Route,html);else Assert.DoesNotContain(row.Route,html);
            return;
        }
        Assert.Contains(mode=="EMPTY"?"Pending work: 0":"Pending work: 4",html);
        if(mode=="EMPTY"){Assert.Contains("No pending work is visible",html);Assert.DoesNotContain("process%2F",html);return;}
        Assert.Contains(mode=="COMPANY"?"/accounting/processes/process%2FA":"/ppdm39/process/instances/process%2FA",html);
        Assert.Contains("Assigned role: Reader",html);
        Assert.Contains("Recorded handoff workspace: MAINTENANCE",html);Assert.Contains("&lt;script&gt;requester",html);
        Assert.Contains("Recorded task creation: Unavailable",html);Assert.DoesNotContain("<script>",html);
        if(mode=="APPROVAL"){Assert.DoesNotContain("process%2FB",html);Assert.DoesNotContain("process%2FD",html);Assert.Contains("Showing 1 of 4",html);}
        else {Assert.Contains("&lt;script&gt;recorded demand",html);Assert.DoesNotContain("<script>",html);Assert.Contains("Showing 4 of 4",html);}
    }
}
