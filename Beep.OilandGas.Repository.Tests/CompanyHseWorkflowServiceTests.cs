using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Controllers.HSE;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Repository;
using Moq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyHseWorkflowServiceTests
{
    [Theory]
    [InlineData("valid")]
    [InlineData("native-failure")]
    [InlineData("changed-source")]
    [InlineData("changed-cause")]
    [InlineData("changed-receipt")]
    [InlineData("revoked-between-phases")]
    public Task FinalIncidentClosureConfirmsLifecycleBeforeNativeStatus(string scenario) => RunClosureScenario(scenario);

    [Theory]
    [InlineData("resolution")][InlineData("resolution-approval-drift")]
    public Task ClosureResolutionConfirmsRecordedApproval(string scenario) => RunClosureScenario(scenario);

    [Theory]
    [InlineData("correction")][InlineData("correction-recovery")][InlineData("correction-native-drift")][InlineData("correction-role-denied")]
    public Task CorrectionReturnPreservesClosureApproval(string scenario) => RunClosureScenario(scenario);

    private async Task RunClosureScenario(string scenario)
    {
        using var f = await CompanyCorrectiveActionCommandTests.Ready(); var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Request.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "roles"));
        f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "Workflow.Approve" });
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" }); await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!; var lifecycle = new AssetDatabaseScope("selected-tenant", await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"], "selected-tenant"));
        var action = new CAVerificationAction(1, "action", "Valve tested", "COMPLETED", "operator", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "operator", "Valve inspected and tested");
        var actions = new CAVerificationReview(context, "local", "plan", new string('C', 64), [action]); var verification = new CAVerificationDecision(Guid.NewGuid(), actions, "VERIFIED", "Independent test confirmed the repair is effective."); var evidence = new IncidentClosureReview(actions, [scenario.StartsWith("correction", StringComparison.Ordinal) ? verification with { Decision = "REJECTED" } : verification]);
        var cause = new IncidentRcaRecordedCause(context.Company, context.Database, context.FieldId!, "local", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('F', 64));
        var incidents = new Mock<ICompanyIncidentCommands>(); incidents.Setup(x => x.ReadSharedRcaAsync(context, "local")).ReturnsAsync([scenario == "changed-cause" ? cause with { Summary = "A different recorded cause for the incident." } : cause]);
        var original = new CompanyHseClosureCommand(Guid.NewGuid(), new(evidence, lifecycle, "instance", "step", "INC_CLOSURE", "Administrator", new string('A', 64), new string('B', 64)) { RecordedCause = cause }, "Final independent incident closure approved.");
        var receipt = new CompanyHseClosureReceipt(original.IntentId, context.Company, "actor", "instance", "step", "INC_CLOSURE", "history", "approval", null, ["closed-task"], [], new string('D', 64));
        var native = new Mock<ICompanyCorrectiveActionCommands>(); native.Setup(x => x.ResolveIncidentPlanAsync(context, "local", "actor")).ReturnsAsync(new CAPlanResolution(context, "local", "plan")); native.Setup(x => x.ReviewClosureAsync(context, "local", "plan", "actor")).ReturnsAsync(scenario == "changed-source" ? evidence with { Actions = actions with { Revision = new string('F', 64) } } : evidence);
        var process = new Mock<IProcessService>(); var owner = process.As<ICompanyHseWorkflowCommands>(); var approvalConfirmed = false;
        owner.Setup(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<Func<string, Task>>(), It.IsAny<Func<Task>>())).Returns(async (CompanyHseClosureCommand command, Func<string, Task> role, Func<Task> source) => { await role("Administrator"); await source(); Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(command)); approvalConfirmed = true; return receipt; });
        owner.Setup(x => x.RecoverClosureStageAsync(context, lifecycle, "local", It.IsAny<Func<string, Task>>(), It.IsAny<Func<CompanyHseClosureCommand, Task>>())).Returns(async (FieldSelectionReview c, AssetDatabaseScope l, string id, Func<string, Task> role, Func<CompanyHseClosureCommand, Task> source) => { await role("Administrator"); await source(original); return new CompanyHseClosureRecovery(original, receipt); });
        native.Setup(x => x.RecordIncidentClosedAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CompanyHseClosureReceipt>(), It.IsAny<Func<Task>>())).Returns(async (CompanyHseClosureCommand command, CompanyHseClosureReceipt r, Func<Task> confirm) => { Assert.True(approvalConfirmed); Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(command)); await confirm(); if (scenario == "native-failure") throw new InvalidOperationException("Controlled native write failure after lifecycle approval"); return new IncidentClosedReceipt(original, receipt, 4); });
        var hse = new Mock<IFieldHSEService>(); hse.Setup(x => x.GetCaPlanAsync("local")).ReturnsAsync(new CAPlanResolution(context, "local", "plan"));
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns(context.FieldId); field.Setup(x => x.GetHSEService()).Returns(hse.Object);
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, field.Object, incidents.Object, new BoundProcessService(() => modules.ResolveAsync("LIFECYCLE"), _ => process.Object), native.Object);
        if (scenario.StartsWith("correction", StringComparison.Ordinal))
        {
            var review = original.Review with { Stage = "INC_VERIFICATION" };
            native.Setup(x => x.ReviewNativeClosureRevisionAsync(context, "local", "plan")).ReturnsAsync(review.NativeRevision);
            owner.Setup(x => x.ReviewCorrectionReturnAsync(It.IsAny<IncidentClosureReview>(), lifecycle, review.NativeRevision, It.IsAny<Func<string, Task>>())).Returns(async (IncidentClosureReview e, AssetDatabaseScope l, string revision, Func<string, Task> role) => { await role("Administrator"); return review; });
            var requested = new CompanyHseCorrectionCommand(Guid.NewGuid(), await service.ReviewCorrectionReturnAsync(f.Request.User, "local"), "Return the rejected valve evidence for corrective work.");
            owner.Setup(x => x.ReturnForCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>(), It.IsAny<Func<string, Task>>(), It.IsAny<Func<Task>>(), It.IsAny<Func<Task>>())).Returns(async (CompanyHseCorrectionCommand command, Func<string, Task> role, Func<Task> source, Func<Task> recorded) => { await role("Administrator"); await recorded(); if (scenario != "correction-recovery") await source(); return new CompanyHseCorrectionReceipt(command, new(command.IntentId, 1, context.Company, "instance", "action-step", "step", "closure-step", actions.Revision), "return-history", "rejection-approval", ["verification-task"], ["correction-task"], new string('D', 64)); });
            if (scenario == "correction-recovery") native.Setup(x => x.ReviewClosureAsync(context, "local", "plan", "actor")).ReturnsAsync(evidence with { Actions = actions with { Revision = new string('E', 64) } });
            if (scenario == "correction-native-drift") native.Setup(x => x.ReviewNativeClosureRevisionAsync(context, "local", "plan")).ReturnsAsync(new string('E', 64));
            if (scenario == "correction-role-denied") { f.Db.RoleClaims.RemoveRange(f.Db.RoleClaims.Where(c => c.ClaimValue == "Workflow.Approve")); await f.Db.SaveChangesAsync(); }
            if (scenario is "correction" or "correction-recovery") (await service.ReturnForCorrectionAsync(f.Request.User, requested)).Require();
            else await Assert.ThrowsAnyAsync<Exception>(() => service.ReturnForCorrectionAsync(f.Request.User, requested));
            owner.Verify(x => x.ReturnForCorrectionAsync(It.IsAny<CompanyHseCorrectionCommand>(), It.IsAny<Func<string, Task>>(), It.IsAny<Func<Task>>(), It.IsAny<Func<Task>>()), scenario == "correction-role-denied" ? Times.Never : Times.Once);
            owner.Verify(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<Func<string, Task>>(), It.IsAny<Func<Task>>()), Times.Never);
            native.Verify(x => x.RecordIncidentClosedAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CompanyHseClosureReceipt>(), It.IsAny<Func<Task>>()), Times.Never); return;
        }
        if (scenario.StartsWith("resolution", StringComparison.Ordinal))
        {
            approvalConfirmed = true;
            var sourceReview = new IncidentClosureResolutionReview(context, lifecycle, "local", "plan", original.IntentId, receipt.GraphRevision, original.Review.NativeRevision, new string('E', 64), [new(1, "STATUS", "REPORTED", null, "Reviewed current native history", "Y", DateTime.UtcNow.AddHours(-1), "HSE", "actor", DateTime.UtcNow.AddHours(-1))]);
            native.Setup(x => x.ReviewClosureResolutionAsync(context, It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CompanyHseClosureReceipt>())).ReturnsAsync(sourceReview);
            var reviewed = await service.ReviewClosureResolutionAsync(f.Request.User, "local"); Assert.Equal(sourceReview, reviewed);
            var resolution = new IncidentClosureResolution(Guid.NewGuid(), sourceReview, "Explicitly reviewed the native history and confirmed closure.");
            if (scenario == "resolution-approval-drift") resolution = resolution with { Review = sourceReview with { ApprovedGraphRevision = new string('F', 64) } };
            native.Setup(x => x.ResolveIncidentClosureAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CompanyHseClosureReceipt>(), It.IsAny<IncidentClosureResolution>(), It.IsAny<Func<Task>>()))
                .Returns(async (CompanyHseClosureCommand command, CompanyHseClosureReceipt r, IncidentClosureResolution requested, Func<Task> confirm) => { await confirm(); return new IncidentClosedReceipt(command, r, 4) { Resolution = requested }; });
            if (scenario == "resolution") (await service.ResolveClosureAsync(f.Request.User, resolution)).Require();
            else await Assert.ThrowsAsync<InvalidOperationException>(() => service.ResolveClosureAsync(f.Request.User, resolution));
            owner.Verify(x => x.CompleteClosureStageAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<Func<string, Task>>(), It.IsAny<Func<Task>>()), Times.Never);
            native.Verify(x => x.ResolveIncidentClosureAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CompanyHseClosureReceipt>(), It.IsAny<IncidentClosureResolution>(), It.IsAny<Func<Task>>()), scenario == "resolution" ? Times.Once : Times.Never); return;
        }
        async Task<CompanyHseClosureOutcome> Complete()
        {
            var approved = await service.CompleteClosureStageAsync(f.Request.User, original); approved.Require(original);
            native.Verify(x => x.RecordIncidentClosedAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CompanyHseClosureReceipt>(), It.IsAny<Func<Task>>()), Times.Never);
            if (scenario == "changed-receipt") approved = approved with { GraphRevision = new string('E', 64) };
            if (scenario == "revoked-between-phases")
            {
                f.Db.RoleClaims.RemoveRange(f.Db.RoleClaims.Where(c => c.RoleId == "admin" && c.ClaimValue == "Workflow.Approve"));
                await f.Db.SaveChangesAsync();
            }
            return await service.RecordApprovedIncidentClosedAsync(f.Request.User, new(original, approved));
        }
        if (scenario == "valid") (await Complete()).Require();
        else if (scenario == "revoked-between-phases")
        {
            var error = await Record.ExceptionAsync(Complete);
            Assert.True(error is UnauthorizedAccessException or Beep.OilandGas.Models.Core.Refusals.RefusalException);
            native.Verify(x => x.RecordIncidentClosedAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CompanyHseClosureReceipt>(), It.IsAny<Func<Task>>()), Times.Never);
        }
        else await Assert.ThrowsAsync<InvalidOperationException>(Complete);
        Assert.Equal(scenario is not ("changed-source" or "changed-cause"), approvalConfirmed);
        native.Verify(x => x.RecordIncidentClosedAsync(It.IsAny<CompanyHseClosureCommand>(), It.IsAny<CompanyHseClosureReceipt>(), It.IsAny<Func<Task>>()), scenario is "changed-source" or "changed-cause" or "revoked-between-phases" ? Times.Never : Times.Once);
    }
    [Theory]
    [InlineData("valid")][InlineData("missing")][InlineData("foreign")][InlineData("changed-cause")][InlineData("revoked")]
    public async Task RcaHandoffRecoveryRequiresRecordedCauseAndCurrentAuthority(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready(); var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Request.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "roles"));
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" }); await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!; var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var cause = new IncidentRcaRecordedCause(context.Company, context.Database, context.FieldId!, "local", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('A', 64));
        var lifecycle = new AssetDatabaseScope("selected-tenant", await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"], "selected-tenant"));
        var review = new CompanyHseRcaHandoffReview(context, lifecycle, cause, "instance", "step", "Administrator", new string('B', 64));
        var command = new CompanyHseRcaHandoffCommand(Guid.NewGuid(), review);
        var receipt = new CompanyHseRcaHandoffReceipt(context.Company, "actor", command.IntentId, "instance", "step", "history", "next", ["closed"], ["next-task"], new string('C', 64));
        var incidents = new Mock<ICompanyIncidentCommands>(); incidents.Setup(x => x.ReadSharedRcaAsync(It.IsAny<FieldSelectionReview>(), "local"))
            .ReturnsAsync(() => (IReadOnlyList<IncidentRcaRecordedCause>)[scenario == "changed-cause" ? cause with { Summary = "A different recorded cause observation." } : cause]);
        var process = new Mock<IProcessService>(); var owner = process.As<ICompanyHseWorkflowCommands>();
        owner.Setup(x => x.RecoverRcaHandoffAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<AssetDatabaseScope>(), "local", It.IsAny<Func<string, Task>>(), It.IsAny<Func<IncidentRcaRecordedCause, Task>>()))
            .Returns(async (FieldSelectionReview current, AssetDatabaseScope database, string id, Func<string, Task> guard, Func<IncidentRcaRecordedCause, Task> source) =>
            {
                if (scenario == "missing") return null;
                if (scenario == "revoked") { var identity = f.Request.User.Identities.Single(value => value.AuthenticationType == "roles"); identity.RemoveClaim(identity.FindFirst(ClaimTypes.Role)!); }
                await guard("Administrator"); await source(cause);
                if (scenario == "foreign") return new CompanyHseRcaRecoveredHandoff(command, receipt with { Actor = "other" });
                return new CompanyHseRcaRecoveredHandoff(command, receipt);
            });
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns("A");
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, field.Object, incidents.Object, new BoundProcessService(() => modules.ResolveAsync("LIFECYCLE"), _ => process.Object), Mock.Of<ICompanyCorrectiveActionCommands>());
        var controller = new IncidentWorkflowController(service) { ControllerContext = new ControllerContext { HttpContext = f.Request } };
        if (scenario == "valid") Assert.IsType<CompanyHseRcaRecoveredHandoff>(Assert.IsType<OkObjectResult>((await controller.RecoverRcaHandoffAsync("local")).Result).Value).Require();
        else if (scenario == "missing") Assert.IsType<NotFoundResult>((await controller.RecoverRcaHandoffAsync("local")).Result);
        else if (scenario == "revoked") Assert.IsType<ForbidResult>((await controller.RecoverRcaHandoffAsync("local")).Result);
        else await Assert.ThrowsAnyAsync<Exception>(() => controller.RecoverRcaHandoffAsync("local"));
        owner.Verify(x => x.CompleteRcaHandoffAsync(It.IsAny<CompanyHseRcaHandoffCommand>(), It.IsAny<Func<string, Task>>(), It.IsAny<Func<Task>>()), Times.Never);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("changed-cause")]
    [InlineData("wrong-role")]
    [InlineData("foreign-receipt")]
    public async Task RcaHandoffSubmissionKeepsOriginalCauseAndAuthority(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready(); var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Request.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "roles"));
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" }); await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!; var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var cause = new IncidentRcaRecordedCause(context.Company, context.Database, context.FieldId!, "local", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('A', 64));
        var lifecycle = new AssetDatabaseScope("selected-tenant", await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"], "selected-tenant"));
        var review = new CompanyHseRcaHandoffReview(context, lifecycle, cause, "instance", "step", scenario == "wrong-role" ? "OtherRole" : "Administrator", new string('B', 64));
        var original = new CompanyHseRcaHandoffCommand(Guid.NewGuid(), review); var incidents = new Mock<ICompanyIncidentCommands>();
        incidents.Setup(x => x.ReadSharedRcaAsync(It.IsAny<FieldSelectionReview>(), "local")).ReturnsAsync(() => (IReadOnlyList<IncidentRcaRecordedCause>)[scenario == "changed-cause" ? cause with { Summary = "A different recorded cause observation." } : cause]);
        var process = new Mock<IProcessService>(); var owner = process.As<ICompanyHseWorkflowCommands>();
        owner.Setup(x => x.CompleteRcaHandoffAsync(It.IsAny<CompanyHseRcaHandoffCommand>(), It.IsAny<Func<string, Task>>(), It.IsAny<Func<Task>>()))
            .Returns(async (CompanyHseRcaHandoffCommand command, Func<string, Task> guard, Func<Task> source) =>
            {
                await guard(command.Review.AssignedRole); await source();
                return new CompanyHseRcaHandoffReceipt(command.Review.Context.Company, scenario == "foreign-receipt" ? "other" : command.Review.Context.ActorUserId,
                    command.IntentId, command.Review.InstanceId, command.Review.StepInstanceId, "history", "next", ["closed"], ["next-task"], new string('C', 64));
            });
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns("A");
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, field.Object, incidents.Object, new BoundProcessService(() => modules.ResolveAsync("LIFECYCLE"), _ => process.Object), Mock.Of<ICompanyCorrectiveActionCommands>());
        var controller = new IncidentWorkflowController(service) { ControllerContext = new ControllerContext { HttpContext = f.Request } };
        if (scenario == "valid") Assert.IsType<CompanyHseRcaHandoffReceipt>(Assert.IsType<OkObjectResult>((await controller.CompleteRcaHandoffAsync(original)).Result).Value).Require(original);
        else if (scenario == "wrong-role") Assert.IsType<ForbidResult>((await controller.CompleteRcaHandoffAsync(original)).Result);
        else await Assert.ThrowsAnyAsync<Exception>(() => controller.CompleteRcaHandoffAsync(original));
        owner.Verify(x => x.CompleteRcaHandoffAsync(It.IsAny<CompanyHseRcaHandoffCommand>(), It.IsAny<Func<string, Task>>(), It.IsAny<Func<Task>>()), scenario is "wrong-role" or "changed-cause" ? Times.Never : Times.Once);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("wrong-role")]
    [InlineData("changed-cause")]
    [InlineData("foreign-review")]
    public async Task RcaHandoffReviewRequiresTheRecordedCauseAndAssignedRole(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready(); var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Request.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "roles"));
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" }); await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!; var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var original = new IncidentRcaRecordedCause(context.Company, context.Database, context.FieldId!, "local", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('A', 64));
        var incidents = new Mock<ICompanyIncidentCommands>(); var reads = 0;
        incidents.Setup(x => x.ReadSharedRcaAsync(It.IsAny<FieldSelectionReview>(), "local")).Returns(() =>
        { reads++; IReadOnlyList<IncidentRcaRecordedCause> rows = [scenario == "changed-cause" && reads > 1 ? original with { Summary = "A different recorded cause observation." } : original]; return Task.FromResult(rows); });
        var process = new Mock<IProcessService>(); var owner = process.As<ICompanyHseWorkflowCommands>();
        owner.Setup(x => x.ReviewRcaHandoffAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<AssetDatabaseScope>(), It.IsAny<IncidentRcaRecordedCause>(), It.IsAny<Func<string, Task>>()))
            .Returns(async (FieldSelectionReview current, AssetDatabaseScope lifecycle, IncidentRcaRecordedCause cause, Func<string, Task> guard) =>
            {
                var role = scenario == "wrong-role" ? "OtherRole" : "Administrator"; await guard(role);
                return new CompanyHseRcaHandoffReview(current, lifecycle, scenario == "foreign-review" ? cause with { Summary = "A different recorded cause observation." } : cause, "instance", "step", role, new string('B', 64));
            });
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns("A");
        var bound = new BoundProcessService(() => modules.ResolveAsync("LIFECYCLE"), _ => process.Object);
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, field.Object, incidents.Object, bound, Mock.Of<ICompanyCorrectiveActionCommands>());
        var controller = new IncidentWorkflowController(service) { ControllerContext = new ControllerContext { HttpContext = f.Request } };
        if (scenario == "valid") Assert.IsType<CompanyHseRcaHandoffReview>(Assert.IsType<OkObjectResult>((await controller.ReviewRcaHandoffAsync("local", original.IntentId)).Result).Value).Require();
        else if (scenario == "wrong-role") Assert.IsType<ForbidResult>((await controller.ReviewRcaHandoffAsync("local", original.IntentId)).Result);
        else await Assert.ThrowsAnyAsync<Exception>(() => controller.ReviewRcaHandoffAsync("local", original.IntentId));
        incidents.Verify(x => x.RecordRcaAsync(It.IsAny<IncidentRcaIntent>()), Times.Never);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("foreign")]
    [InlineData("duplicate")]
    [InlineData("role-removed")]
    public async Task SharedRcaEvidenceSupportsAnotherAuthorWithinCurrentCompanyAndField(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Request.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "roles"));
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" }); await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns("A");
        var incidents = new Mock<ICompanyIncidentCommands>();
        incidents.Setup(x => x.ReadSharedRcaAsync(It.IsAny<FieldSelectionReview>(), "local")).Returns((FieldSelectionReview current, string id) =>
        {
            if (scenario == "role-removed") { var identity = f.Request.User.Identities.Single(value => value.AuthenticationType == "roles"); identity.RemoveClaim(identity.FindFirst(ClaimTypes.Role)!); }
            var scope = scenario == "foreign" ? new CompanyScope("tenant", "other") : current.Company;
            var cause = new IncidentRcaRecordedCause(scope, current.Database, current.FieldId!, id, Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('A', 64));
            IReadOnlyList<IncidentRcaRecordedCause> rows = scenario == "duplicate" ? [cause, cause] : [cause]; return Task.FromResult(rows);
        });
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, field.Object, incidents.Object, Mock.Of<IProcessService>(), Mock.Of<ICompanyCorrectiveActionCommands>());
        var controller = new IncidentWorkflowController(service) { ControllerContext = new ControllerContext { HttpContext = f.Request } };
        if (scenario == "valid") Assert.Equal("another-author", Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<IncidentRcaRecordedCause>>(Assert.IsType<OkObjectResult>((await controller.ReadSharedRcaAsync("local")).Result).Value)).RecordedBy);
        else if (scenario == "role-removed") Assert.IsType<ForbidResult>((await controller.ReadSharedRcaAsync("local")).Result);
        else await Assert.ThrowsAnyAsync<Exception>(() => controller.ReadSharedRcaAsync("local"));
        incidents.Verify(x => x.ReadRecordedRcaAsync(It.IsAny<FieldSelectionReview>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("foreign")]
    [InlineData("duplicate")]
    [InlineData("role-removed")]
    public async Task RecordedRcaRecoveryKeepsCurrentCompanyAndAccount(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Request.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "roles"));
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" }); await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns("A");
        var incidents = new Mock<ICompanyIncidentCommands>();
        incidents.Setup(x => x.ReadRecordedRcaAsync(It.IsAny<FieldSelectionReview>(), "local")).Returns((FieldSelectionReview current, string id) =>
        {
            if (scenario == "role-removed") { var identity = f.Request.User.Identities.Single(value => value.AuthenticationType == "roles"); identity.RemoveClaim(identity.FindFirst(ClaimTypes.Role)!); }
            var context = scenario == "foreign" ? current with { Company = new("tenant", "other") } : current;
            var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
            var original = new IncidentRcaIntent(Guid.NewGuid(), new(context, id, new string('A', 64), 1, [], [option]), option, "Recorded cause for this incident.");
            IReadOnlyList<IncidentRcaIntent> rows = scenario == "duplicate" ? [original, original] : [original]; return Task.FromResult(rows);
        });
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, field.Object, incidents.Object, Mock.Of<IProcessService>(), Mock.Of<ICompanyCorrectiveActionCommands>());
        var controller = new IncidentWorkflowController(service) { ControllerContext = new ControllerContext { HttpContext = f.Request } };
        if (scenario == "valid") Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<IncidentRcaIntent>>(Assert.IsType<OkObjectResult>((await controller.ReadRecordedRcaAsync("local")).Result).Value));
        else if (scenario == "role-removed") Assert.IsType<ForbidResult>((await controller.ReadRecordedRcaAsync("local")).Result);
        else await Assert.ThrowsAnyAsync<Exception>(() => controller.ReadRecordedRcaAsync("local"));
        incidents.Verify(x => x.RecordRcaAsync(It.IsAny<IncidentRcaIntent>()), Times.Never);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("foreign-receipt")]
    [InlineData("role-removed")]
    [InlineData("lifecycle-changed")]
    [InlineData("foreign-company")]
    public async Task RcaRecordingConfirmsTheOriginalAndRechecksAuthority(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Request.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "roles"));
        var binding = new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" };
        f.Db.ModuleDatabases.Add(binding); await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company);
        var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns("A");
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        if (scenario == "foreign-company") context = context with { Company = new("tenant", "other") };
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var original = new IncidentRcaIntent(Guid.NewGuid(), new(context, "local", new string('A', 64), 1, [], [option]), option, "Reviewed root cause for this incident.");
        var incidents = new Mock<ICompanyIncidentCommands>();
        incidents.Setup(x => x.RecordRcaAsync(It.IsAny<IncidentRcaIntent>())).Returns(async (IncidentRcaIntent value) =>
        {
            if (scenario == "role-removed")
            {
                var identity = f.Request.User.Identities.Single(item => item.AuthenticationType == "roles");
                identity.RemoveClaim(identity.FindFirst(ClaimTypes.Role)!);
            }
            if (scenario == "lifecycle-changed") { binding.ConcurrencyStamp = "changed"; await f.Db.SaveChangesAsync(); }
            return new IncidentRcaReceipt(value.Source.Context.Company, scenario == "foreign-receipt" ? "other" : value.Source.Context.ActorUserId,
                value.Source.Context.FieldId!, value.IntentId, value.Source.IncidentId, value.Source.NextObservation, HSE_INCIDENT_RCA_INTENT.Create(value).COMMAND_HASH);
        });
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, field.Object, incidents.Object, Mock.Of<IProcessService>(), Mock.Of<ICompanyCorrectiveActionCommands>());
        var controller = new IncidentWorkflowController(service) { ControllerContext = new ControllerContext { HttpContext = f.Request } };
        if (scenario == "valid")
            Assert.IsType<IncidentRcaReceipt>(Assert.IsType<OkObjectResult>((await controller.RecordRcaAsync(original)).Result).Value).Require(original);
        else if (scenario is "role-removed" or "lifecycle-changed" or "foreign-company")
        {
            if (scenario == "foreign-company") await Assert.ThrowsAnyAsync<Exception>(() => controller.RecordRcaAsync(original));
            else Assert.IsType<ForbidResult>((await controller.RecordRcaAsync(original)).Result);
        }
        else await Assert.ThrowsAnyAsync<Exception>(() => controller.RecordRcaAsync(original));
        incidents.Verify(x => x.RecordRcaAsync(It.IsAny<IncidentRcaIntent>()), scenario == "foreign-company" ? Times.Never : Times.Once);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("foreign-company")]
    [InlineData("wrong-incident")]
    [InlineData("role-removed")]
    [InlineData("lifecycle-changed")]
    public async Task RcaReviewRequiresOriginalCompanyFieldAndLifecycleAuthority(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Request.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "roles"));
        var binding = new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" };
        f.Db.ModuleDatabases.Add(binding); await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company);
        var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns("A");
        var incident = new Mock<ICompanyIncidentCommands>();
        incident.Setup(x => x.ReviewRcaSourceAsync(It.IsAny<FieldSelectionReview>(), "local"))
            .Returns(async (FieldSelectionReview context, string id) =>
            {
                if (scenario == "role-removed")
                {
                    var identity = f.Request.User.Identities.Single(value => value.AuthenticationType == "roles");
                    identity.RemoveClaim(identity.FindFirst(ClaimTypes.Role)!);
                }
                if (scenario == "lifecycle-changed") { binding.ConcurrencyStamp = "changed"; await f.Db.SaveChangesAsync(); }
                if (scenario == "foreign-company") context = context with { Company = new("tenant", "other") };
                return new IncidentRcaSourceReview(context, scenario == "wrong-incident" ? "other" : id,
                    new string('A', 64), 1, [], [new("ROOT", "PROCESS", "Process")]);
            });
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, field.Object, incident.Object, Mock.Of<IProcessService>(), Mock.Of<ICompanyCorrectiveActionCommands>());
        var controller = new IncidentWorkflowController(service) { ControllerContext = new ControllerContext { HttpContext = f.Request } };
        if (scenario == "valid")
        {
            var review = Assert.IsType<IncidentRcaSourceReview>(Assert.IsType<OkObjectResult>((await controller.ReviewRcaAsync("local")).Result).Value);
            Assert.Equal("local", review.IncidentId); Assert.Equal("PROCESS", Assert.Single(review.Options).CauseCode);
        }
        else if (scenario is "role-removed" or "lifecycle-changed")
            Assert.IsType<ForbidResult>((await controller.ReviewRcaAsync("local")).Result);
        else await Assert.ThrowsAnyAsync<Exception>(() => controller.ReviewRcaAsync("local"));
        incident.Verify(x => x.RecordRcaAsync(It.IsAny<IncidentRcaIntent>()), Times.Never);
    }

    [Theory]
    [InlineData("own")]
    [InlineData("foreign")]
    [InlineData("mixed")]
    [InlineData("duplicate")]
    [InlineData("binding-changed")]
    [InlineData("malformed")]
    [InlineData("incident-unavailable")]
    [InlineData("field-changed")]
    public async Task WorkflowReaderUsesRecordedCompanyAndDatabaseOwnership(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Request.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "roles"));
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" });
        await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var lifecycle = new AssetDatabaseScope("selected-tenant", await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"], "selected-tenant"));
        ProcessInstance Instance(FieldSelectionReview owner, AssetDatabaseScope binding)
        {
            var original = new CompanyHseWorkflowStart(owner, binding, Guid.NewGuid(), "local", new string('A', 64), new string('B', 64));
            var id = original.InstanceId();
            return new ProcessInstance { InstanceId = id, EntityId = "local", EntityType = CompanyHseWorkflowStart.EntityType, FieldId = owner.FieldId!, ProcessId = CompanyHseWorkflowStart.ProcessId,
                ProcessData = new PROCESS_DATA { InstanceId = id, Data = new() { [CompanyHseWorkflowStart.PayloadKey] = original } } };
        }
        var own = Instance(context, lifecycle);
        var foreign = Instance(context with { Company = new("tenant", "other") }, lifecycle);
        var process = new Mock<IProcessService>();
        var rows = scenario switch { "foreign" => new List<ProcessInstance> { foreign }, "mixed" => [foreign, own], "duplicate" => [own, own],
            "binding-changed" => [Instance(context, lifecycle with { Fingerprint = "changed" })], "malformed" => [new ProcessInstance { EntityId = "local", EntityType = CompanyHseWorkflowStart.EntityType, FieldId = "A", ProcessId = CompanyHseWorkflowStart.ProcessId }], _ => [own] };
        var currentField = "A";
        process.Setup(x => x.GetProcessInstancesForEntityAsync("local", CompanyHseWorkflowStart.EntityType))
            .Callback(() => { if (scenario == "field-changed") currentField = "other"; }).ReturnsAsync(rows);
        var hse = new Mock<IFieldHSEService>(); hse.Setup(x => x.GetIncidentAsync("local")).ReturnsAsync(scenario == "incident-unavailable" ? null : new HSEIncidentRecord { IncidentId = "local", FieldId = "A" });
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns(() => currentField); field.Setup(x => x.GetHSEService()).Returns(hse.Object);
        var reader = new CompanyHseWorkflowReader(f.Gate, modules, profile, field.Object, process.Object);
        if (scenario is "own" or "mixed") Assert.Same(own, await reader.ReadAsync(f.Request.User, "local"));
        else if (scenario == "foreign") Assert.Null(await reader.ReadAsync(f.Request.User, "local"));
        else await Assert.ThrowsAnyAsync<Exception>(() => reader.ReadAsync(f.Request.User, "local"));
        if (scenario == "incident-unavailable") process.Verify(x => x.GetProcessInstancesForEntityAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("foreign-receipt")]
    [InlineData("binding-changed")]
    [InlineData("role-removed")]
    public async Task ReviewAndStartKeepCompanyFieldAndLifecycleBinding(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Request.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "roles"));
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" });
        await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company);
        var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var process = new Mock<IProcessService>();
        var owner = process.As<ICompanyHseWorkflowCommands>();
        var definition = new ProcessDefinition { ProcessId = CompanyHseWorkflowStart.ProcessId, ProcessName = "Incident reporting", ProcessType = "HSE", EntityType = CompanyHseWorkflowStart.EntityType,
            Steps = [new ProcessStepDefinition { StepId = "review", StepName = "Review", SequenceNumber = 1, RequiredRoles = ["HSEOfficer"] }] };
        process.Setup(x => x.GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId)).ReturnsAsync(definition);
        var bound = new BoundProcessService(() => modules.ResolveAsync("LIFECYCLE"), _ => process.Object);
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns("A");
        var incident = new Mock<ICompanyIncidentCommands>();
        incident.Setup(x => x.ReviewWorkflowSourceRevisionAsync(It.IsAny<FieldSelectionReview>(), "local")).ReturnsAsync(new string('A', 64));
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, field.Object, incident.Object, bound, Mock.Of<ICompanyCorrectiveActionCommands>());
        var controller = new IncidentWorkflowController(service) { ControllerContext = new ControllerContext { HttpContext = f.Request } };
        var review = Assert.IsType<CompanyHseWorkflowReview>(Assert.IsType<OkObjectResult>((await controller.ReviewAsync("local")).Result).Value);
        var original = review.CreateIntent(Guid.NewGuid());
        Assert.Equal("HSEOfficer", review.InitialRole);
        owner.Setup(x => x.StartHseWorkflowAsync(It.IsAny<CompanyHseWorkflowStart>(), It.IsAny<Func<Task>>(), It.IsAny<Func<Task<string>>>()))
            .Returns(async (CompanyHseWorkflowStart value, Func<Task> guard, Func<Task<string>> read) =>
            {
                await guard(); Assert.Equal(value.IncidentRevision, await read());
                return new CompanyHseWorkflowReceipt(value.Context.Company, scenario == "foreign-receipt" ? "other" : value.Context.ActorUserId,
                    value.Context.FieldId, value.IntentId, value.IncidentId, value.InstanceId(), "step", ["task"], new string('B', 64));
            });
        if (scenario == "binding-changed")
        {
            f.Db.ModuleDatabases.Single(x => x.ModuleId == "LIFECYCLE").ConcurrencyStamp = "changed";
            await f.Db.SaveChangesAsync();
        }
        if (scenario == "role-removed")
        {
            var identity = f.Request.User.Identities.Single(value => value.AuthenticationType == "roles");
            identity.RemoveClaim(identity.FindFirst(ClaimTypes.Role)!);
        }
        if (scenario == "valid")
        {
            var receipt = Assert.IsType<CompanyHseWorkflowReceipt>(Assert.IsType<OkObjectResult>((await controller.StartAsync(original)).Result).Value);
            Assert.Equal(original.InstanceId(), receipt.InstanceId);
        }
        else if (scenario is "binding-changed" or "role-removed")
            Assert.IsType<ForbidResult>((await controller.StartAsync(original)).Result);
        else await Assert.ThrowsAnyAsync<Exception>(() => controller.StartAsync(original));
        owner.Verify(x => x.StartHseWorkflowAsync(It.IsAny<CompanyHseWorkflowStart>(), It.IsAny<Func<Task>>(), It.IsAny<Func<Task<string>>>()),
            scenario is "binding-changed" or "role-removed" ? Times.Never : Times.Once);
    }

    [Fact]
    public void WorkflowEndpointsRequireManagePermissionAndCurrentFieldWrite()
    {
        var type = typeof(IncidentWorkflowController);
        Assert.Equal(Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.ManageIncidents,
            Assert.Single(type.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()).Policy);
        var field = Assert.Single(type.CustomAttributes, value => value.AttributeType == typeof(RequireCurrentFieldAccessAttribute));
        Assert.Equal(Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.ManageIncidents, field.ConstructorArguments[0].Value);
        Assert.Equal(true, field.ConstructorArguments[1].Value);
        Assert.True(Assert.Single(type.GetCustomAttributes(typeof(ResponseCacheAttribute), true).Cast<ResponseCacheAttribute>()).NoStore);
    }
}
