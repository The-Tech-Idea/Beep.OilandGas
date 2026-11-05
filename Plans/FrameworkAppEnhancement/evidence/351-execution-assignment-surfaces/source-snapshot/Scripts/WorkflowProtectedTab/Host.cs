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
using TheTechIdea.Data.OilGas;
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
        builder.Services.AddSingleton(new Backend(original,assignment)); builder.Services.AddScoped<Scope>();
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
            source.AssignmentWrites,source.AssignmentRecoveries,source.AssignmentReads,source.AssignedReads,source.Mode,
            AssignmentRecorded=source.RecordedAssignment is not null,
            RecordedReports = source.Rows.Count, source.ReadOnly, source.RecoveryUnavailable, source.LastIntentRevision,
            Role = source.Original.RecorderRole, Unit = source.Original.DepthUnit,
            PeriodStart = source.Original.PeriodStartUtc.ToString("O"), PeriodEnd = source.Original.PeriodEndUtc.ToString("O"),
            PreviousEnd = source.Rows.LastOrDefault()?.PERIOD_END_UTC?.ToString("O")
        });
        app.MapPost("/fixture/recovery-outage/{unavailable:bool}", (bool unavailable, Backend source) =>
        {
            source.RecoveryUnavailable = unavailable;
            return Microsoft.AspNetCore.Http.Results.Ok(new { source.RecoveryUnavailable });
        });
        await app.RunAsync();
    }
}

public sealed class Backend(DrillingDailyReportInput original,DrillingExecutionAssignmentReview assignment)
{
    public DrillingDailyReportInput Original { get; } = original;
    public DrillingExecutionAssignmentReview Assignment{get;}=assignment;
    public DrillingExecutionAssignment? RecordedAssignment;
    public string Mode="report";
    public int AssignmentWrites,AssignmentRecoveries,AssignmentReads,AssignedReads;
    public List<DRILLING_DAILY_REPORT> Rows { get; } = [];
    public int RecordRequests, RecoverRequests, SourceRequests, ProfileLoads, StartRecoverRequests;
    public bool ReadOnly;
    public bool RecoveryUnavailable;
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
        var result = new DrillingDailyReportHistory(Execution(), Copy(Rows.ToArray()), "", DateTimeOffset.UtcNow);
        result = result with { Revision = result.ContentRevision() }; result.Require(); return result;
    }
    public DrillingDailyReportBasis Basis()
    {
        var head = Rows.LastOrDefault(); var basis = Original.Basis with
        {
            Execution = Execution(), NextNumber = Rows.Count + 1, PreviousReportId = head?.DDR_ID, PreviousReportRevision = head?.REPORT_REVISION,
            PreviousPeriodEndUtc = head is null ? null : new DateTimeOffset(DateTime.SpecifyKind(head.PERIOD_END_UTC!.Value, DateTimeKind.Utc)),
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
        Client.Setup(c=>c.GetAssignedExecutionAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync((string id,CancellationToken _)=>
        {
            backend.AssignedReads++;var saved=backend.RecordedAssignment??throw new InvalidOperationException("No controlled responsibility is recorded.");
            var result=DrillingWorkflowBrowserFixture.Assigned(saved);if(id!=result.InstanceId || Company.Selection?.Scope!=result.Source.Basis.Scope().Company ||
                Company.Selection.UserId!=result.Source.Basis.Scope().ActorUserId || PersonaCode!=result.PersonaCode)throw new UnauthorizedAccessException("The controlled individual reporting recipient is required.");
            return result;
        });
        Client.Setup(c => c.GetDailyReportsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => { View(); backend.SourceRequests++; return backend.History(); });
        Client.Setup(c => c.GetDailyReportBasisAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => { Write(); backend.SourceRequests++; return backend.Basis(); });
        Client.Setup(c => c.RecordDailyReportAsync(It.IsAny<DrillingDailyReportInput>(), It.IsAny<CancellationToken>())).Returns((DrillingDailyReportInput input, CancellationToken _) =>
        {
            Write(); input.Require(); backend.RecordRequests++;
            if (input.Basis.NextNumber != backend.Rows.Count + 1) throw new InvalidOperationException("The controlled report head changed.");
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
        Client.Setup(c => c.GetExecutionStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => { View(); backend.SourceRequests++; return backend.Execution(); });
        Client.Setup(c => c.RecoverExecutionStartAsync(It.IsAny<DrillingExecutionStart>(), It.IsAny<CancellationToken>())).ReturnsAsync((DrillingExecutionStart input, CancellationToken _) =>
        {
            View(); backend.StartRecoverRequests++;
            if (backend.RecoveryUnavailable) throw new HttpRequestException("Controlled read-only outcome lookup outage.");
            var receipt = new DrillingExecutionStartReceipt(input.Revision(), input.OperationId(), Backend.Copy(backend.Original.Basis.Execution.Recorded!), DateTimeOffset.UtcNow); receipt.Require(input); return receipt;
        });
    }
    private void View()
    {
        var scope = backend.Original.Basis.Scope();
        if (Company.Selection?.Scope != scope.Company || Company.Selection.UserId != scope.ActorUserId)
            throw new UnauthorizedAccessException("The controlled source belongs to another company/account.");
    }
    private void Write() { View(); if (backend.ReadOnly) throw new UnauthorizedAccessException("Current reporting edit authority is absent."); }
    private void Select(CompanyScope company, string actor)
    {
        var permissions = new List<string> { "Drilling.View", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Drilling.ViewDailyReports" };
        if (!backend.ReadOnly) permissions.AddRange(["Drilling.Create", "Drilling.Edit", "Drilling.ManageWellProgram","Workflow.Start"]);
        var roles=backend.Mode=="assigned" && backend.RecordedAssignment is {} saved?[saved.Workflow.Step.AssignedRole]:
            backend.ReadOnly?["ReaderRole"]:backend.Mode=="assignment"?backend.Assignment.SenderRoles.ToArray():backend.Original.Basis.RecorderRoles.ToArray();
        Company.TrySelect(Company.Clear(), actor, new(company, roles, permissions.ToArray()));
        Personas.Raise(p => p.Changed += null!);
    }
    private string PersonaCode=>backend.Mode=="assigned" && backend.RecordedAssignment is {} saved?saved.Workflow.Authority.Recipients[0].PersonaCode:
        backend.ReadOnly?"UNMAPPED":backend.Mode=="assignment"?backend.Assignment.SenderPersonaCode!:backend.Original.Basis.PersonaCode!;
    public void Mode(string mode){backend.Mode=mode;OriginalCompany();}
    public void OriginalCompany()
    {
        if(backend.Mode=="assigned" && backend.RecordedAssignment is {} saved){var recipient=saved.Workflow.Authority.Recipients[0];Select(saved.Scope().Company,recipient.UserId);return;}
        var scope = backend.Original.Basis.Scope(); Select(scope.Company, scope.ActorUserId);
    }
    public void OtherCompany() => Select(new("tenant", "foreign-browser-company"), backend.Original.Basis.Scope().ActorUserId);
    public void OtherAccount() => Select(backend.Original.Basis.Scope().Company, "other-browser-account");
    public void ReadOnly() { backend.ReadOnly = true; OriginalCompany(); }
    public void Editable() { backend.ReadOnly = false; OriginalCompany(); }
    public void Dispose() => Company.Dispose();
}
