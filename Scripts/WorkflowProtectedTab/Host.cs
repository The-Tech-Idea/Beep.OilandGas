using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Auth.Tests;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;

namespace WorkflowProtectedTab;

// Isolated browser harness: actual pages/state/encrypted journal/component transport;
// controlled approved source and typed-client outcomes. No production auth/API/provider claim.
public static class Host
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseStaticWebAssets(); builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddRazorComponents().AddOilGasInteractiveServerComponents(); builder.Services.AddMudServices();
        var original = await DrillingWorkflowBrowserFixture.CreateAsync();
        var assignment = await DrillingWorkflowBrowserFixture.CreateAssignmentAsync();
        var reportingClosure = await DrillingWorkflowBrowserFixture.CreateReportingClosureAsync();
        var reportingTransfer = await DrillingWorkflowBrowserFixture.CreateReportingReassignmentAsync();
        builder.Services.AddSingleton(new Backend(original,assignment,reportingClosure,reportingTransfer)); builder.Services.AddScoped<Scope>();
        builder.Services.AddScoped(p => p.GetRequiredService<Scope>().Company);
        builder.Services.AddScoped(p => p.GetRequiredService<Scope>().Personas.Object);
        builder.Services.AddScoped(p => p.GetRequiredService<Scope>().Client.Object);
        builder.Services.AddScoped<IDevelopmentDraftIntentStore, DevelopmentDraftIntentStore>();
        builder.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var app = builder.Build(); app.UseAntiforgery(); app.MapStaticAssets();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        app.MapGet("/fixture/metadata", (Backend source) => new
        {
            source.RecordRequests, source.RecoverRequests, source.SourceRequests, source.ProfileLoads, source.StartRecoverRequests,
            source.AssignmentWrites,source.AssignmentRecoveries,source.AssignmentReads,source.AssignedReads,source.Mode,source.CompletionRoutingReads,source.CompletionRoutingMode,
            source.ClosureReads,source.ClosureReviews,source.ClosureWrites,source.ClosureRecoveries,ClosureRecorded=source.RecordedClosure is not null,ClosureCheckedAtUtc=source.ReportingClosure.Review.CheckedAtUtc,
            source.TransferReads,source.TransferReviews,source.TransferWrites,source.TransferRecoveries,TransferRecorded=source.RecordedTransfer is not null,TransferCheckedAtUtc=source.ReportingTransfer.Review.CheckedAtUtc,
            TransferManagerRole=source.ReportingTransfer.Review.ManagerRole,TransferWorkflow=source.ReportingTransfer.Review.Destination.ProcessId,
            source.ConflictReads,source.ConflictConfirmations,source.AssignmentConflictReads,source.AssignmentConflictConfirmations,source.ReportConflictReads,source.ReportConflictConfirmations,
            AssignmentRecorded=source.RecordedAssignment is not null,
            RecordedReports = source.Rows.Count, source.ReadOnly, source.RecoveryUnavailable, source.ProgressUnavailable, source.LastIntentRevision,
            Role = source.Original.RecorderRole, Unit = source.Original.DepthUnit,
            PeriodStart = source.Original.PeriodStartUtc.ToString("O"), PeriodEnd = source.Original.PeriodEndUtc.ToString("O"),
            PreviousEnd = source.History().Head?.LatestPeriodEndUtc.ToString("O")
        });
        app.MapPost("/fixture/recovery-outage/{unavailable:bool}", (bool unavailable, Backend source) =>
        {
            source.RecoveryUnavailable = unavailable;
            return Microsoft.AspNetCore.Http.Results.Ok(new { source.RecoveryUnavailable });
        });
        app.MapPost("/fixture/progress-outage/{unavailable:bool}", (bool unavailable, Backend source) =>
        {
            source.ProgressUnavailable = unavailable;
            return Microsoft.AspNetCore.Http.Results.Ok(new { source.ProgressUnavailable });
        });
        app.MapPost("/fixture/assignment/seed", (Backend source) =>
        {
            // Controlled recipient fixture only; no claim that this was an API/native assignment write.
            var review = source.Assignment;
            var original = new DrillingExecutionAssignment(review.Source, review.Options.Single(), review.SenderRoles.Single(),
                review.SenderPersonaCode!, review.SenderAuthorityRevision!, Guid.NewGuid(), "Controlled recipient responsibility for completion evidence inspection");
            original.Require(); source.RecordedAssignment = original;
            return Microsoft.AspNetCore.Http.Results.Ok(new { InstanceId = original.InstanceId() });
        });
        app.MapPost("/fixture/reports/seed/{count:int}", (int count, Backend source) =>
        {
            if (count < 1 || count > 100) throw new InvalidOperationException("Bound the diagnostic population.");
            source.Rows.Clear(); source.Rows.AddRange(DrillingWorkflowBrowserFixture.ReportHistory(source.Original,count));
            return Microsoft.AspNetCore.Http.Results.Ok(new { SeededReports=source.Rows.Count });
        });
        app.MapPost("/fixture/completion-routing/{mode}", (string mode, Backend source) =>
        {
            if (mode is not ("ready" or "blocked" or "empty" or "outage")) throw new InvalidOperationException("Choose an explicit diagnostic routing state.");
            source.CompletionRoutingMode=mode;return Microsoft.AspNetCore.Http.Results.Ok(new { mode });
        });
        app.MapPost("/fixture/reports/completion", (Backend source) =>
        {
            // Canonical controlled claim fixture; no API/native recording is claimed.
            var target=source.Rows.Last();var observed=DrillingDailyReportInput.FromRecord(target);
            var claim=new DrillingCompletionReport(DateTimeOffset.UtcNow,"Controlled work reported complete pending independent review",
                [new("Reported site inspection","Controlled completion certificate",DateTimeOffset.UtcNow,"REPORTED_FAIL","Controlled site evidence","Requires independent acceptance")],
                ["Independent inspection discrepancy remains unresolved"],true);
            var original=observed with{Basis=source.Basis(),IntentId=Guid.NewGuid(),Correction=new(target.DDR_ID,target.REPORT_REVISION,target.REPORT_NUMBER,"Explicit controlled completion claim"),Completion=claim};
            source.Rows.Add(original.ToRecord(DateTime.UtcNow));return Microsoft.AspNetCore.Http.Results.Ok(new { SeededReports=source.Rows.Count });
        });
        app.MapPost("/fixture/reports/append", (Backend source) =>
        {
            var row=source.Rows.Last();var original=DrillingDailyReportInput.FromRecord(row);
            var basis=original.Basis with{NextNumber=row.REPORT_NUMBER+1,PreviousReportId=row.DDR_ID,PreviousReportRevision=row.REPORT_REVISION,PreviousPeriodEndUtc=source.History().Head!.LatestPeriodEndUtc,CheckedAtUtc=DateTimeOffset.UtcNow};
            basis=basis with{Revision=basis.ContentRevision()};var input=original with{Basis=basis,IntentId=Guid.NewGuid(),Correction=null,PeriodStartUtc=basis.PreviousPeriodEndUtc!.Value,PeriodEndUtc=DateTimeOffset.UtcNow,Provenance="Appended observed site log "+basis.NextNumber};
            source.Rows.Add(input.ToRecord(DateTime.UtcNow));return Microsoft.AspNetCore.Http.Results.Ok(new { RecordedReports=source.Rows.Count });
        });
        await app.RunAsync();
    }
}

public sealed class Backend(DrillingDailyReportInput original,DrillingExecutionAssignmentReview assignment,DrillingReportingClosure reportingClosure,DrillingReportingReassignment reportingTransfer)
{
    public DrillingReportingReassignment ReportingTransfer{get;}=reportingTransfer;
    public DrillingReportingReassignment? RecordedTransfer;
    public int TransferReads,TransferReviews,TransferWrites,TransferRecoveries;
    public DrillingReportingReassignmentCatalog TransferCatalog()
    {
        var review=ReportingTransfer.Review;var responsibility=RecordedTransfer is {} recorded?DrillingReportingGeneration.After(recorded):Copy(review.Responsibility);
        var result=new DrillingReportingReassignmentCatalog(responsibility,Copy(review.Execution),[Copy(review.Destination)],[review.ManagerRole],review.ManagerPersona,review.AuthorityRevision,review.NativeRevision,"",DateTimeOffset.UtcNow);
        return result with{Revision=result.ContentRevision()};
    }
    public DrillingReportingClosure ReportingClosure{get;}=reportingClosure;
    public DrillingReportingClosure? RecordedClosure;
    public int ClosureReads,ClosureReviews,ClosureWrites,ClosureRecoveries;
    public DrillingDailyReportInput Original { get; } = original;
    public DrillingExecutionAssignmentReview Assignment{get;}=assignment;
    public DrillingExecutionAssignment? RecordedAssignment;
    public string Mode="report";
    public int AssignmentWrites,AssignmentRecoveries,AssignmentReads,AssignedReads;
    public List<DRILLING_DAILY_REPORT> Rows { get; } = [];
    public int RecordRequests, RecoverRequests, SourceRequests, ProfileLoads, StartRecoverRequests;
    public int ConflictReads,ConflictConfirmations,AssignmentConflictReads,AssignmentConflictConfirmations,ReportConflictReads,ReportConflictConfirmations;
    public bool ReadOnly;
    public bool RecoveryUnavailable;
    public bool ProgressUnavailable;
    public string CompletionRoutingMode="ready";
    public int CompletionRoutingReads;
    public string? LastIntentRevision;
    public static T Copy<T>(T value) where T : class => CompanyInventorySourceReview.Copy(value);
    public DrillingExecutionStatus Execution()
    {
        var execution = Copy(Original.Basis.Execution);
        if (ReadOnly)
        {
            var basis = execution.Basis with { RecorderRoles = [], PersonaCode = null, RecorderAuthorityRevision = null };
            basis = basis with { Revision = basis.ContentRevision() }; execution = execution with { Basis = basis };
        }
        execution = execution with { Revision = execution.ContentRevision(), CheckedAtUtc = DateTimeOffset.UtcNow }; execution.Require(); return execution;
    }
    public DrillingDailyReportHistory History()
    {
        var builder = new DrillingDailyReportHistoryBuilder(Execution(), DateTimeOffset.UtcNow);
        foreach (var row in Rows) builder.Add(row);
        return builder.Build();
    }
    public DrillingDailyReportPage Page(int after, int size, string? expected)
    {
        DrillingDailyReportPage.RequireRequest(after, size, expected);
        var builder=new DrillingDailyReportHistoryBuilder(Execution(),DateTimeOffset.UtcNow);var observations=new List<DrillingDailyReportObservation>();
        foreach(var row in Rows){var observed=builder.Add(row);if(row.REPORT_NUMBER>after && row.REPORT_NUMBER<=(long)after+size)observations.Add(observed);}
        var history=builder.Build();
        if (expected is not null && expected != history.Revision) throw new InvalidOperationException("Controlled history changed.");
        var result = new DrillingDailyReportPage(history, after, size, observations.Select(builder.QualifyDisplay).ToArray(), "");
        result = result with { Revision = result.ContentRevision() }; result.Require(); return result;
    }
    public DrillingDailyReportBasis Basis()
    {
        var head = History().Head; var basis = Original.Basis with
        {
            Execution = Execution(), NextNumber = Rows.Count + 1, PreviousReportId = head?.ReportId, PreviousReportRevision = head?.NativeRevision,
            PreviousPeriodEndUtc = head?.LatestPeriodEndUtc,
            CheckedAtUtc = DateTimeOffset.UtcNow
        };
        basis = basis with { Revision = basis.ContentRevision() }; basis.Require(); return basis;
    }
}

public sealed class Scope : IDisposable
{
    private readonly Backend backend;
    public CompanyClientContext Company { get; } = new();
    public Mock<IPersonaContextService> Personas { get; } = new();
    public Mock<IDrillingServiceClient> Client { get; } = new();
    public Scope(Backend backend)
    {
        this.backend = backend; OriginalCompany();
        Personas.SetupGet(p => p.CurrentPersona).Returns(() => new AppPersona { Code = PersonaCode, Name = "Controlled browser workspace" });
        Personas.SetupGet(p => p.CurrentProfile).Returns(() => new AppUserPersona { UserId = Company.Selection!.UserId, PersonaCode = PersonaCode });
        Personas.Setup(p => p.EnsureLoadedAsync()).Returns(() => { backend.ProfileLoads++; if (backend.ReadOnly && backend.Mode!="assigned") throw new InvalidOperationException("A read-only recovery must not need a mapped workspace lookup."); return Task.CompletedTask; });
        Client.Setup(c=>c.GetReportingReassignmentCatalogAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync((string id,CancellationToken _)=>
        {
            Write();backend.TransferReads++;if(id!=backend.ReportingTransfer.Review.Responsibility.Root.InstanceId())throw new InvalidOperationException("Choose the controlled reporting transfer.");return backend.TransferCatalog();
        });
        Client.Setup(c=>c.ReviewReportingReassignmentAsync(It.IsAny<DrillingReportingReassignmentSelection>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingReportingReassignmentSelection selected,CancellationToken _)=>
        {
            Write();selected.Require();backend.TransferReviews++;var review=backend.ReportingTransfer.Review;
            if(backend.RecordedTransfer is not null || selected.AssignmentId!=review.Responsibility.Root.InstanceId() || selected.ProcessId!=review.Destination.ProcessId || selected.ManagerRole!=review.ManagerRole || selected.CatalogRevision!=backend.TransferCatalog().Revision)
                throw new InvalidOperationException("The controlled reporting transfer selection changed.");return Backend.Copy(review);
        });
        DrillingReportingReassignmentReceipt TransferReceipt(DrillingReportingReassignment value,bool recorded)=>new(value.Revision(),value.Review.Responsibility.Root.InstanceId(),recorded,recorded?value.HistoryId():null,recorded?value.ReassignedAtUtc:null,recorded?value.Review.Responsibility.Number+1:null,recorded?value.StepInstanceId():null,recorded?value.TaskIds():[],new('F',64),DateTimeOffset.UtcNow);
        Client.Setup(c=>c.ReassignReportingAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>())).Returns((DrillingReportingReassignment value,CancellationToken _)=>
        {
            Write();value.Require();if(value.Review.Revision!=backend.ReportingTransfer.Review.Revision || backend.RecordedTransfer is not null)throw new InvalidOperationException("The controlled original reporting transfer changed.");
            backend.TransferWrites++;backend.RecordedTransfer=Backend.Copy(value);backend.LastIntentRevision=value.Revision();
            return Task.FromException<DrillingReportingReassignmentReceipt>(new HttpRequestException("Controlled lost transfer acknowledgement; native/API persistence qualifies separately."));
        });
        Client.Setup(c=>c.RecoverReportingReassignmentAsync(It.IsAny<DrillingReportingReassignment>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingReportingReassignment value,CancellationToken _)=>
        {
            View();backend.TransferRecoveries++;if(backend.RecoveryUnavailable)throw new HttpRequestException("Controlled reporting transfer recovery outage.");
            if(backend.RecordedTransfer is {} saved && saved.Revision()!=value.Revision())throw new InvalidOperationException("Another controlled transfer owns this responsibility.");return TransferReceipt(value,backend.RecordedTransfer is not null);
        });
        Client.Setup(c=>c.GetReportingResponsibilityAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync((string id,CancellationToken _) =>
        {
            View();backend.ClosureReads++;var source=backend.ReportingClosure;
            if(id!=source.Review.Responsibility.Root.InstanceId())throw new InvalidOperationException("Choose the controlled original reporting responsibility.");
            var result=new DrillingReportingClosureStatus(Backend.Copy(source.Review.Responsibility),Backend.Copy(source.Review.Evidence.Acceptance.Source.History.Execution),[],backend.RecordedClosure is {} closed?Backend.Copy(closed):null,new('F',64),"",DateTimeOffset.UtcNow);
            return result with{Revision=result.ContentRevision()};
        });
        Client.Setup(c=>c.ReviewReportingClosureAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync((string id,string completion,CancellationToken _) =>
        {
            Write();backend.ClosureReviews++;
            if(id!=backend.ReportingClosure.Review.Responsibility.Root.InstanceId() || completion!=backend.ReportingClosure.Review.Evidence.Acceptance.InstanceId || backend.RecordedClosure is not null)
                throw new InvalidOperationException("The controlled current closure source changed.");
            return Backend.Copy(backend.ReportingClosure.Review);
        });
        DrillingReportingClosureReceipt ClosureReceipt(DrillingReportingClosure value,bool recorded)=>new(value.Revision(),value.Review.Responsibility.Root.InstanceId(),recorded,recorded?value.HistoryId():null,recorded?value.ClosedAtUtc:null,value.Review.Responsibility.TaskIds(),new('F',64),DateTimeOffset.UtcNow);
        Client.Setup(c=>c.CloseReportingAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>())).Returns((DrillingReportingClosure value,CancellationToken _) =>
        {
            Write();value.Require();if(value.Review.Revision!=backend.ReportingClosure.Review.Revision || backend.RecordedClosure is not null)throw new InvalidOperationException("The controlled original closure changed.");
            backend.ClosureWrites++;backend.RecordedClosure=Backend.Copy(value);
            return Task.FromException<DrillingReportingClosureReceipt>(new HttpRequestException("Controlled lost acknowledgement after simulated closure; native/API persistence qualifies separately."));
        });
        Client.Setup(c=>c.RecoverReportingClosureAsync(It.IsAny<DrillingReportingClosure>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingReportingClosure value,CancellationToken _) =>
        {
            View();backend.ClosureRecoveries++;if(backend.RecoveryUnavailable)throw new HttpRequestException("Controlled closure recovery outage.");
            if(backend.RecordedClosure is {} saved && saved.Revision()!=value.Revision())throw new InvalidOperationException("Another controlled closure owns this responsibility.");
            return ClosureReceipt(value,backend.RecordedClosure is not null);
        });
        Client.Setup(c=>c.GetExecutionAssignmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>
        {
            View();backend.AssignmentReads++;var result=Backend.Copy(backend.Assignment);
            if(backend.ReadOnly)result=result with{Options=[],SenderRoles=[],SenderPersonaCode=null,SenderAuthorityRevision=null};
            if(backend.RecordedAssignment is {} recorded)result=result with{Options=[],ExistingInstanceId=recorded.InstanceId()};
            result=result with{Revision=result.ContentRevision(),CheckedAtUtc=DateTimeOffset.UtcNow};return result;
        });
        Client.Setup(c=>c.AssignExecutionAsync(It.IsAny<DrillingExecutionAssignment>(),It.IsAny<CancellationToken>())).Returns((DrillingExecutionAssignment input,CancellationToken _)=>
        {
            Write();input.Require();backend.AssignmentWrites++;if(backend.RecordedAssignment is not null)throw new InvalidOperationException("The controlled assignment is already recorded.");
            backend.RecordedAssignment=Backend.Copy(input);
            return Task.FromException<DrillingExecutionAssignmentReceipt>(new HttpRequestException("Controlled lost acknowledgement after simulated assignment; native/API persistence qualifies separately."));
        });
        Client.Setup(c=>c.RecoverExecutionAssignmentAsync(It.IsAny<DrillingExecutionAssignment>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingExecutionAssignment input,CancellationToken _)=>
        {
            View();backend.AssignmentRecoveries++;if(backend.RecoveryUnavailable)throw new HttpRequestException("Controlled assignment outcome lookup outage.");
            if(backend.RecordedAssignment is {} saved && saved.Revision()!=input.Revision())throw new InvalidOperationException("Another controlled assignment original exists.");
            return DrillingWorkflowBrowserFixture.AssignmentReceipt(input,backend.RecordedAssignment is not null);
        });
        DrillingExecutionAssignmentConflictReview AssignmentConflict(DrillingExecutionAssignment input)
        {
            View();if(backend.RecoveryUnavailable)throw new HttpRequestException("Controlled reporting-conflict lookup outage.");
            var competing=Backend.Copy(backend.RecordedAssignment??throw new InvalidOperationException("No controlled responsibility is recorded."));
            var result=new DrillingExecutionAssignmentConflictReview(input.Revision(),input.InstanceId(),true,competing,DrillingWorkflowBrowserFixture.AssignmentReceipt(competing,true),"",DateTimeOffset.UtcNow);
            result=result with{Revision=result.ContentRevision()};result.Require(input);return result;
        }
        Client.Setup(c=>c.GetExecutionAssignmentConflictAsync(It.IsAny<DrillingExecutionAssignment>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingExecutionAssignment input,CancellationToken _)=>{backend.AssignmentConflictReads++;return AssignmentConflict(input);});
        Client.Setup(c=>c.ConfirmExecutionAssignmentConflictAsync(It.IsAny<DrillingExecutionAssignmentConflictAcknowledgement>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingExecutionAssignmentConflictAcknowledgement input,CancellationToken _)=>{
            input.Require();backend.AssignmentConflictConfirmations++;var result=AssignmentConflict(input.Original);
            if(result.Revision!=input.ReviewRevision)throw new InvalidOperationException("The controlled reporting responsibility changed.");return result;
        });
        Client.Setup(c=>c.GetAssignedExecutionAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync((string id,CancellationToken _)=>
        {
            backend.AssignedReads++;var saved=backend.RecordedAssignment??throw new InvalidOperationException("No controlled responsibility is recorded.");
            var result=DrillingWorkflowBrowserFixture.Assigned(saved);if(id!=result.InstanceId || Company.Selection?.Scope!=result.Source.Basis.Scope().Company ||
                Company.Selection.UserId!=result.Source.Basis.Scope().ActorUserId || PersonaCode!=result.PersonaCode)throw new UnauthorizedAccessException("The controlled individual reporting recipient is required.");
            return result;
        });
        Client.Setup(c => c.GetDailyReportPageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync((string id, int after, int size, string? expected, CancellationToken _) =>
        {
            backend.SourceRequests++;
            if (backend.ProgressUnavailable) throw new HttpRequestException("Controlled progress source outage.");
            if (backend.Mode == "assigned" && backend.RecordedAssignment is {} saved)
            {
                // Recipient uses its own canonical controlled fixture, separate from the recorder fixture's operation.
                var packet = DrillingWorkflowBrowserFixture.Assigned(saved);
                if (id != packet.Source.Basis.ReviewInstanceId || Company.Selection?.Scope != packet.Source.Basis.Scope().Company ||
                    Company.Selection.UserId != packet.Source.Basis.Scope().ActorUserId || PersonaCode != packet.PersonaCode)
                    throw new UnauthorizedAccessException("The controlled original reporting recipient is required.");
                return DrillingWorkflowBrowserFixture.AssignedProgress(saved);
            }
            View(); return backend.Page(after, size, expected);
        });
        Client.Setup(c => c.GetCompletionRoutingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((string id,CancellationToken _) =>
        {
            View();backend.CompletionRoutingReads++;
            if(id!=backend.Original.Basis.Execution.Basis.ReviewInstanceId)throw new UnauthorizedAccessException("The exact controlled execution is required.");
            if(backend.CompletionRoutingMode=="outage")throw new HttpRequestException("Controlled completion routing outage.");
            return DrillingWorkflowBrowserFixture.CompletionRouting(backend.History(),backend.CompletionRoutingMode);
        });
        Client.Setup(c => c.GetDailyReportBasisAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => { Write(); backend.SourceRequests++; return backend.Basis(); });
        Client.Setup(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns((DrillingDailyReportInput input, CancellationToken _) =>
        {
            Write(); input.Require(); backend.RecordRequests++;
            var proof = new DrillingDailyReportHistoryBuilder(backend.Execution(), DateTimeOffset.UtcNow);
            foreach (var existing in backend.Rows) proof.Add(existing); proof.RequireAppend(input);
            var row = input.ToRecord(DateTime.UtcNow); DrillingDailyReportInput.FromRecord(row); backend.Rows.Add(row); backend.LastIntentRevision = input.Revision();
            return Task.FromException<DrillingDailyReportReceipt>(new HttpRequestException("Controlled lost acknowledgement after simulated record; native persistence qualifies separately."));
        });
        Client.Setup(c => c.RecoverDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).ReturnsAsync((DrillingDailyReportInput input, CancellationToken _) =>
        {
            View(); backend.RecoverRequests++;
            if (backend.RecoveryUnavailable) throw new HttpRequestException("Controlled read-only outcome lookup outage.");
            var row = backend.Rows.SingleOrDefault(row => row.DDR_ID == input.ReportId());
            var receipt = new DrillingDailyReportReceipt(input.Revision(), input.ReportId(), row is null ? null : Backend.Copy(row), DateTimeOffset.UtcNow); receipt.Require(input); return receipt;
        });
        DrillingDailyReportConflictReview ReportConflict(DrillingDailyReportInput input)
        {
            View(); if (backend.RecoveryUnavailable) throw new HttpRequestException("Controlled report-conflict lookup outage.");
            var history = backend.History();
            if (backend.Rows.Any(row => row.DDR_ID == input.ReportId())) throw new InvalidOperationException("The controlled original is recorded.");
            var row = backend.Rows.Single(report => report.REPORT_NUMBER == input.Basis.NextNumber);
            var result = new DrillingDailyReportConflictReview(input.Revision(), input.ReportId(), true, Backend.Copy(row), history.Revision, history.Count, "", DateTimeOffset.UtcNow);
            result = result with { Revision = result.ContentRevision() }; result.Require(input); return result;
        }
        Client.Setup(c => c.GetDailyReportConflictAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).ReturnsAsync((DrillingDailyReportInput input, CancellationToken _) => { backend.ReportConflictReads++; return ReportConflict(input); });
        Client.Setup(c => c.ConfirmDailyReportConflictAsync(It.IsAny<DrillingDailyReportConflictAcknowledgement>(), It.IsAny<CancellationToken>())).ReturnsAsync((DrillingDailyReportConflictAcknowledgement input, CancellationToken _) =>
        {
            input.Require(); backend.ReportConflictConfirmations++; var result = ReportConflict(input.Original);
            if (result.Revision != input.ReviewRevision) throw new InvalidOperationException("The controlled report history changed."); return result;
        });
        Client.Setup(c => c.GetExecutionStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => { View(); backend.SourceRequests++; return backend.Execution(); });
        DrillingExecutionConflictReview Conflict(DrillingExecutionStart input)
        {
            View();if(backend.RecoveryUnavailable)throw new HttpRequestException("Controlled conflict lookup outage.");
            var result=new DrillingExecutionConflictReview(input.Revision(),input.OperationId(),true,Backend.Copy(backend.Original.Basis.Execution.Recorded!),"",DateTimeOffset.UtcNow);
            result=result with{Revision=result.ContentRevision()};result.Require(input);return result;
        }
        Client.Setup(c=>c.GetExecutionConflictAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingExecutionStart input,CancellationToken _) => {backend.ConflictReads++;return Conflict(input);});
        Client.Setup(c=>c.ConfirmExecutionConflictAsync(It.IsAny<DrillingExecutionConflictAcknowledgement>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingExecutionConflictAcknowledgement input,CancellationToken _) =>
        {
            input.Require();backend.ConflictConfirmations++;var result=Conflict(input.Original);
            if(result.Revision!=input.Review.Revision)throw new InvalidOperationException("The controlled reviewed conflict changed.");return result;
        });
        Client.Setup(c => c.RecoverExecutionStartAsync(It.IsAny<DrillingExecutionStart>(), It.IsAny<CancellationToken>())).ReturnsAsync((DrillingExecutionStart input, CancellationToken _) =>
        {
            View(); backend.StartRecoverRequests++;
            if (backend.RecoveryUnavailable) throw new HttpRequestException("Controlled read-only outcome lookup outage.");
            var receipt = new DrillingExecutionStartReceipt(input.Revision(), input.OperationId(), Backend.Copy(backend.Original.Basis.Execution.Recorded!), DateTimeOffset.UtcNow); receipt.Require(input); return receipt;
        });
    }
    private DevelopmentAssignedReviewScope ExpectedScope=>backend.Mode=="transfer"?backend.ReportingTransfer.Scope():backend.Mode=="closure"?backend.ReportingClosure.Scope():backend.Original.Basis.Scope();
    private void View()
    {
        var scope = ExpectedScope;
        if (Company.Selection?.Scope != scope.Company || Company.Selection.UserId != scope.ActorUserId)
            throw new UnauthorizedAccessException("The controlled source belongs to another company/account.");
    }
    private void Write() { View(); if (backend.ReadOnly) throw new UnauthorizedAccessException("Current reporting edit authority is absent."); }
    private void Select(CompanyScope company, string actor)
    {
        var permissions = new List<string> { "Drilling.View", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Drilling.ViewDailyReports" };
        if (!backend.ReadOnly) permissions.AddRange(["Drilling.Create", "Drilling.Edit", "Drilling.ManageWellProgram","Workflow.Start"]);
        var roles=backend.Mode=="transfer" && !backend.ReadOnly?[backend.ReportingTransfer.Review.ManagerRole]:backend.Mode=="closure" && !backend.ReadOnly?[backend.ReportingClosure.Review.ManagerRole]:backend.Mode=="assigned" && backend.RecordedAssignment is {} saved?[saved.Workflow.Step.AssignedRole]:
            backend.ReadOnly?["ReaderRole"]:backend.Mode=="assignment"?backend.Assignment.SenderRoles.ToArray():backend.Original.Basis.RecorderRoles.ToArray();
        Company.TrySelect(Company.Clear(), actor, new(company, roles, permissions.ToArray()));
        Personas.Raise(p => p.Changed += null!);
    }
    private string PersonaCode=>backend.Mode=="assigned" && backend.RecordedAssignment is {} saved?saved.Workflow.Authority.Recipients[0].PersonaCode:
        backend.ReadOnly?"UNMAPPED":backend.Mode=="transfer"?backend.ReportingTransfer.Review.ManagerPersona:backend.Mode=="closure"?backend.ReportingClosure.Review.ManagerPersona:backend.Mode=="assignment"?backend.Assignment.SenderPersonaCode!:backend.Original.Basis.PersonaCode!;
    public void Mode(string mode){backend.Mode=mode;OriginalCompany();}
    public void OriginalCompany()
    {
        if(backend.Mode=="transfer"){var original=backend.ReportingTransfer.Scope();Select(original.Company,original.ActorUserId);return;}
        if(backend.Mode=="closure"){var original=backend.ReportingClosure.Scope();Select(original.Company,original.ActorUserId);return;}
        if(backend.Mode=="assigned" && backend.RecordedAssignment is {} saved){var recipient=saved.Workflow.Authority.Recipients[0];Select(saved.Scope().Company,recipient.UserId);return;}
        var scope = backend.Original.Basis.Scope(); Select(scope.Company, scope.ActorUserId);
    }
    public void OtherCompany() => Select(new("tenant", "foreign-browser-company"), ExpectedScope.ActorUserId);
    public void OtherAccount() => Select(ExpectedScope.Company, "other-browser-account");
    public void ReadOnly() { backend.ReadOnly = true; OriginalCompany(); }
    public void Editable() { backend.ReadOnly = false; OriginalCompany(); }
    public void Dispose() => Company.Dispose();
}
