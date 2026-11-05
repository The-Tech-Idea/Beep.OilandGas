using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Repository.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers.HSE;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.HSE;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact]
    public async Task FinalClosureHttpAcrossStoresRefusesRevocationAndReconcilesLostCommit()
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        using var lifecycleSource = await CompanyPPDMRowAccessTests.Fixture.Create();
        lifecycleSource.EnableCorrectiveActionTransactions(); lifecycleSource.EnableProcessGraphWrites(); lifecycleSource.EnableProcessGraphUpdates();
        CompanyIncidentCommandTests.EnableReportOptionProjections(f);
        f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = PermissionConstants.Workflow.Approve });
        var configured = new TheTechIdea.Beep.ConfigUtil.ConnectionProperties { ConnectionName = "hse-lifecycle", DatabaseType = TheTechIdea.Beep.Utilities.DataSourceType.SqlServer, Host = "server", Database = "hse-lifecycle" };
        f.Editor.Object.ConfigEditor.DataConnections.Add(configured);
        f.Db.Add(new AppModuleConnection { TenantId = "tenant", ConnectionKey = "HSE-LIFECYCLE", AssignedByUserId = "actor", PhysicalTargetKey = ModuleDatabaseTarget.Key(f.Editor.Object, configured) });
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "hse-lifecycle", ConnectionKey = "HSE-LIFECYCLE", ConcurrencyStamp = "lifecycle-version" });
        await f.Db.SaveChangesAsync();
        f.Editor.Setup(x => x.GetDataSource("hse-lifecycle")).Returns(lifecycleSource.Source.Object);
        f.Editor.Setup(x => x.OpenDataSource("hse-lifecycle")).Returns(System.Data.ConnectionState.Open);
        lifecycleSource.Source.SetupGet(x => x.DatasourceName).Returns("hse-lifecycle");
        var nativeMetadata = Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.HseIncidentNativeMetadata.AddTo(new());
        foreach (var type in new[] { typeof(HSE_INCIDENT_RCA_INTENT), typeof(HSE_CA_VERIFICATION), typeof(HSE_INCIDENT_CLOSURE) })
        {
            var name = type.Name; f.Metadata.Setup(x => x.GetTableMetadataAsync(name)).ReturnsAsync(nativeMetadata[name]);
            f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(name)).ReturnsAsync([nativeMetadata[name].PrimaryKeyColumn]);
        }
        foreach (var pair in new[] { ("PROCESS_DEFINITION", "PROCESS_DEFINITION_ID"), ("PROCESS_INSTANCE", "PROCESS_INSTANCE_ID"), ("PROCESS_STEP_INSTANCE", "PROCESS_STEP_INSTANCE_ID"),
            ("PROCESS_HISTORY", "PROCESS_HISTORY_ID"), ("PROCESS_APPROVAL", "PROCESS_APPROVAL_ID"), ("CROSS_PERSONA_TASK", "CROSS_TASK_ID") })
        {
            var info = new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata { EntityTypeName = pair.Item1, PrimaryKeyColumn = pair.Item2 };
            f.Metadata.Setup(x => x.GetTableMetadataAsync(pair.Item1)).ReturnsAsync(info); f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(pair.Item1)).ReturnsAsync([pair.Item2]);
            lifecycleSource.Metadata.Setup(x => x.GetTableMetadataAsync(pair.Item1)).ReturnsAsync(info); lifecycleSource.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(pair.Item1)).ReturnsAsync([pair.Item2]);
        }
        f.Source.Setup(x => x.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure request, bool _) =>
        {
            var type = new[] { typeof(HSE_INCIDENT_RCA_INTENT), typeof(HSE_CA_VERIFICATION), typeof(HSE_INCIDENT_CLOSURE) }.SingleOrDefault(t => t.Name == request.EntityName)
                ?? typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + request.EntityName)!;
            var shape = new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(type);
            var keys = f.Metadata.Object.GetPrimaryKeyColumnsAsync(type.Name).GetAwaiter().GetResult();
            foreach (var field in shape.Fields) { field.IsKey = keys.Contains(field.FieldName); if (field.Fieldtype == typeof(string).FullName) field.Size = field.Size1 = 262144; }
            return shape;
        });
        f.Rows.Add(new R_INCIDENT_CAUSE_TYPE { CAUSE_TYPE = "ROOT", ACTIVE_IND = "Y" });
        f.Rows.Add(new R_INCIDENT_CAUSE_CODE { CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", LONG_NAME = "Process cause", ACTIVE_IND = "Y" });
        lifecycleSource.Rows.Add(new PROCESS_DEFINITION { PROCESS_DEFINITION_ID = CompanyHseWorkflowStart.ProcessId, PROCESS_NAME = "Incident reporting", PROCESS_TYPE = "HSE", ENTITY_TYPE = CompanyHseWorkflowStart.EntityType, ACTIVE_IND = "Y", IS_ACTIVE = "Y", VERSION = "1",
            PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
                new ProcessStepDefinition { StepId = "INC_RCA", StepName = "Root cause", StepType = "ACTION", SequenceNumber = 1, NextStepId = "INC_CORRECTIVE_ACTIONS", RequiredRoles = ["Administrator"] },
                new ProcessStepDefinition { StepId = "INC_CORRECTIVE_ACTIONS", StepName = "Corrective actions", StepType = "ACTION", SequenceNumber = 2, NextStepId = "INC_VERIFICATION", RequiredRoles = ["Administrator"] },
                new ProcessStepDefinition { StepId = "INC_VERIFICATION", StepName = "Independent verification", StepType = "APPROVAL", RequiresApproval = true, SequenceNumber = 3, NextStepId = "INC_CLOSURE", RequiredRoles = ["Administrator"] },
                new ProcessStepDefinition { StepId = "INC_CLOSURE", StepName = "Final closure", StepType = "APPROVAL", RequiresApproval = true, SequenceNumber = 4, NextStepId = "", RequiredRoles = ["Administrator"] } } }) });
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        await commands.RecordCompletionAsync(context, "local", CompanyCorrectiveActionAuthorityTests.Plan, 1, "Valve inspected and tested by the operator", "actor");
        // The authentication seam signs in the reviewer; previously completed work has a distinct recorded operator author.
        var completion = f.Rows.OfType<PROJECT_STATUS>().Single(r => r.PROJECT_ID == CompanyCorrectiveActionAuthorityTests.Plan && r.STATUS == "COMPLETED"); completion.ROW_CREATED_BY = completion.ROW_CHANGED_BY = "operator";
        f.Rows.OfType<PROJECT_STEP>().Single(r => r.PROJECT_ID == CompanyCorrectiveActionAuthorityTests.Plan).ROW_CHANGED_BY = "operator";
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var personas = new Mock<IRolePersonaReader>(); personas.Setup(x => x.GetActivePersonasAsync("Administrator")).ReturnsAsync(["HSE"]);
        var process = new PPDMProcessService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, "hse-lifecycle", personaReader: personas.Object);
        using var boundary = new Boundary(f, profile);
        var facade = new PPDMHSEService("A", f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, commands);
        boundary.Orchestrator.Setup(x => x.GetHSEService()).Returns(facade);
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, boundary.Orchestrator.Object, CompanyIncidentCommandTests.Commands(f), new BoundProcessService(() => modules.ResolveAsync("LIFECYCLE"), _ => process), commands);
        await using var host = await HttpHost.Start(f, profile, boundary, true, typeof(IncidentWorkflowController), services =>
        {
            services.AddSingleton(service); services.AddSingleton<ICompanyCorrectiveActionCommands>(commands); services.AddSingleton<IClaimsTransformation, HandoffRoleBridge>();
            services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Error));
            services.AddAuthorization(options => { foreach (var permission in new[] { PermissionConstants.HSE.ManageIncidents, PermissionConstants.Workflow.Approve }) options.AddPolicy(permission, policy => policy.RequireAuthenticatedUser().RequireRole("Administrator")); });
        }, additionalControllers: [typeof(HSEController)]);
        host.Http.Timeout = TimeSpan.FromMinutes(5); var root = host.Url + "/api/field/current/hse/workflow";
        async Task<T> Read<T>(string path) { using var reply = await host.Http.GetAsync(root + path); Assert.True(reply.IsSuccessStatusCode, path + ": " + await reply.Content.ReadAsStringAsync()); return (await reply.Content.ReadFromJsonAsync<T>())!; }
        async Task<T> Send<T>(string path, object request) { using var reply = await host.Http.PostAsJsonAsync(root + path, request); Assert.True(reply.IsSuccessStatusCode, path + ": " + await reply.Content.ReadAsStringAsync()); return (await reply.Content.ReadFromJsonAsync<T>())!; }
        var started = (await Read<CompanyHseWorkflowReview>("/review?incidentId=local")).CreateIntent(Guid.NewGuid());
        (await Send<CompanyHseWorkflowReceipt>("/start", started)).Require(started);
        var rca = await Read<IncidentRcaSourceReview>("/rca/review?incidentId=local");
        var cause = new IncidentRcaIntent(Guid.NewGuid(), rca, Assert.Single(rca.Options), "The recorded process cause requires corrective action.");
        (await Send<IncidentRcaReceipt>("/rca/record-cause", cause)).Require(cause);
        var handoff = new CompanyHseRcaHandoffCommand(Guid.NewGuid(), await Read<CompanyHseRcaHandoffReview>("/rca/handoff-review?incidentId=local&intentId=" + cause.IntentId));
        (await Send<CompanyHseRcaHandoffReceipt>("/rca/handoff", handoff)).Require(handoff);
        var verificationReviewReply = await host.Http.GetAsync(host.Url + "/api/field/current/hse/incidents/local/cas/verification-review"); Assert.True(verificationReviewReply.IsSuccessStatusCode, await verificationReviewReply.Content.ReadAsStringAsync());
        var verification = new CAVerificationDecision(Guid.NewGuid(), (await verificationReviewReply.Content.ReadFromJsonAsync<CAVerificationReview>())!, "VERIFIED", "Independent pressure inspection confirmed repair effectiveness.");
        using var verificationReply = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/incidents/local/cas/verification-decision", verification); Assert.True(verificationReply.IsSuccessStatusCode, await verificationReply.Content.ReadAsStringAsync());
        CompanyHseClosureCommand? final = null;
        for (var i = 0; i < 3; i++)
        {
            var review = await Read<CompanyHseClosureReview>("/closure/review?incidentId=local");
            var command = new CompanyHseClosureCommand(Guid.NewGuid(), review, "Reviewed incident evidence and approve this exact assigned stage.");
            if (i == 2)
            {
                final = command;
                var permission = f.Db.RoleClaims.Single(c => c.ClaimValue == PermissionConstants.Workflow.Approve);
                f.Db.RoleClaims.Remove(permission); await f.Db.SaveChangesAsync();
                var before = f.Writes.Count + lifecycleSource.Writes.Count;
                using var denied = await host.Http.PostAsJsonAsync(root + "/closure/complete-stage", command); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
                Assert.Equal(before, f.Writes.Count + lifecycleSource.Writes.Count); Assert.Empty(f.Rows.OfType<HSE_INCIDENT_CLOSURE>()); Assert.Equal(ProcessStatus.IN_PROGRESS, (await process.GetProcessInstanceAsync(started.InstanceId()))!.Status);
                f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = PermissionConstants.Workflow.Approve }); await f.Db.SaveChangesAsync();
            }
            using var approvalResponse = await host.Http.PostAsJsonAsync(root + "/closure/complete-stage", command);
            Assert.True(approvalResponse.IsSuccessStatusCode, await approvalResponse.Content.ReadAsStringAsync());
            var approval = (await approvalResponse.Content.ReadFromJsonAsync<CompanyHseClosureReceipt>())!; approval.Require(command);
            Assert.Empty(f.Rows.OfType<HSE_INCIDENT_CLOSURE>());
            if (i == 2)
            {
                var approved = new CompanyHseClosureRecovery(command, approval); approved.Require();
                var permission = f.Db.RoleClaims.Single(c => c.ClaimValue == PermissionConstants.Workflow.Approve);
                f.Db.RoleClaims.Remove(permission); await f.Db.SaveChangesAsync();
                var beforeDenial = f.Writes.Count + lifecycleSource.Writes.Count;
                using var denied = await host.Http.PostAsJsonAsync(root + "/closure/record-native", approved);
                Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode); Assert.Equal(beforeDenial, f.Writes.Count + lifecycleSource.Writes.Count);
                Assert.Empty(f.Rows.OfType<HSE_INCIDENT_CLOSURE>());
                f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = PermissionConstants.Workflow.Approve }); await f.Db.SaveChangesAsync();
                f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true;
                using var response = await host.Http.PostAsJsonAsync(root + "/closure/record-native", approved);
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode); f.LeaseCommitFailed = false;
                Assert.Equal(ProcessStatus.COMPLETED, (await process.GetProcessInstanceAsync(started.InstanceId()))!.Status);
                Assert.Single(f.Rows.OfType<HSE_INCIDENT_CLOSURE>());
                var before = f.Writes.Count + lifecycleSource.Writes.Count;
                (await Send<CompanyHseClosureOutcome>("/closure/record-native", approved)).Require(); Assert.Equal(before, f.Writes.Count + lifecycleSource.Writes.Count);
            }
        }
        var writes = f.Writes.Count + lifecycleSource.Writes.Count;
        var finalApproval = await Send<CompanyHseClosureReceipt>("/closure/complete-stage", final!); finalApproval.Require(final!);
        (await Send<CompanyHseClosureOutcome>("/closure/record-native", new CompanyHseClosureRecovery(final!, finalApproval))).Require(); Assert.Equal(writes, f.Writes.Count + lifecycleSource.Writes.Count);
        Assert.Single(f.Rows.OfType<HSE_INCIDENT_CLOSURE>()); Assert.Single(f.Rows.OfType<HSE_INCIDENT_RESPONSE>(), r => r.INCIDENT_ID == "local" && r.RESPONSE_RESULT == "CLOSED");
        Assert.Equal(2, lifecycleSource.Rows.OfType<PROCESS_APPROVAL>().Count(r => r.APPROVAL_STATUS == "APPROVED"));
        Assert.All(lifecycleSource.Rows.OfType<CROSS_PERSONA_TASK>(), t => Assert.Equal("COMPLETED", t.TASK_STATUS));
        Assert.Empty(f.Rows.OfType<PROCESS_INSTANCE>()); Assert.Empty(lifecycleSource.Rows.OfType<HSE_INCIDENT_CLOSURE>());
    }

    [Theory]
    [InlineData("success")]
    [InlineData("lost-commit")]
    [InlineData("cross-author")]
    public async Task RcaHandoffHttpConnectsIncidentAndLifecycleOwners(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        CompanyIncidentCommandTests.EnableReportOptionProjections(f); f.EnableProcessGraphWrites(); f.EnableProcessGraphUpdates();
        f.Rows.Add(new R_INCIDENT_CAUSE_TYPE { CAUSE_TYPE = "ROOT", ACTIVE_IND = "Y" });
        f.Rows.Add(new R_INCIDENT_CAUSE_CODE { CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", LONG_NAME = "Process cause", ACTIVE_IND = "Y" });
        f.Metadata.Setup(x => x.GetTableMetadataAsync(nameof(HSE_INCIDENT_RCA_INTENT))).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata
        { EntityTypeName = typeof(HSE_INCIDENT_RCA_INTENT).FullName!, TableName = nameof(HSE_INCIDENT_RCA_INTENT), PrimaryKeyColumn = "HSE_INCIDENT_RCA_INTENT_ID" });
        f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(nameof(HSE_INCIDENT_RCA_INTENT))).ReturnsAsync(["HSE_INCIDENT_RCA_INTENT_ID"]);
        foreach (var pair in new[] { ("PROCESS_DEFINITION", "PROCESS_DEFINITION_ID"), ("PROCESS_INSTANCE", "PROCESS_INSTANCE_ID"),
            ("PROCESS_STEP_INSTANCE", "PROCESS_STEP_INSTANCE_ID"), ("PROCESS_HISTORY", "PROCESS_HISTORY_ID"), ("PROCESS_APPROVAL", "PROCESS_APPROVAL_ID"), ("CROSS_PERSONA_TASK", "CROSS_TASK_ID") })
        {
            f.Metadata.Setup(value => value.GetTableMetadataAsync(pair.Item1)).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata { EntityTypeName = pair.Item1, PrimaryKeyColumn = pair.Item2 });
            f.Metadata.Setup(value => value.GetPrimaryKeyColumnsAsync(pair.Item1)).ReturnsAsync([pair.Item2]);
        }
        f.Source.Setup(source => source.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure requested, bool _) =>
        {
            var type = requested.EntityName == nameof(HSE_INCIDENT_RCA_INTENT) ? typeof(HSE_INCIDENT_RCA_INTENT) : typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + requested.EntityName)!;
            var schema = new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(type);
            var keys = f.Metadata.Object.GetPrimaryKeyColumnsAsync(type.Name).GetAwaiter().GetResult().ToArray();
            foreach (var field in schema.Fields) { field.IsKey = keys.Contains(field.FieldName); if (field.Fieldtype == typeof(string).FullName) { field.Size = 16384; field.Size1 = 16384; } }
            return schema;
        });
        f.Rows.Add(new PROCESS_DEFINITION { PROCESS_DEFINITION_ID = CompanyHseWorkflowStart.ProcessId, PROCESS_NAME = "Incident reporting", PROCESS_TYPE = "HSE",
            ENTITY_TYPE = CompanyHseWorkflowStart.EntityType, ACTIVE_IND = "Y", IS_ACTIVE = "Y", VERSION = "1",
            PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
                new ProcessStepDefinition { StepId = "INC_RCA", StepName = "Root cause", StepType = "ACTION", SequenceNumber = 1, NextStepId = "INC_CORRECTIVE_ACTIONS", RequiredRoles = ["Administrator"] },
                new ProcessStepDefinition { StepId = "INC_CORRECTIVE_ACTIONS", StepName = "Corrective actions", StepType = "ACTION", SequenceNumber = 2, RequiredRoles = ["Administrator"] } } }) });
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" }); await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var personas = new Mock<IRolePersonaReader>(); personas.Setup(x => x.GetActivePersonasAsync("Administrator")).ReturnsAsync(["HSE"]);
        var process = new PPDMProcessService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, "selected-tenant", personaReader: personas.Object);
        var bound = new BoundProcessService(() => modules.ResolveAsync("LIFECYCLE"), _ => process);
        using var boundary = new Boundary(f, profile);
        boundary.Access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", PermissionConstants.HSE.ManageIncidents, null)).ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        boundary.Access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", "WRITE", null)).ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, boundary.Orchestrator.Object, CompanyIncidentCommandTests.Commands(f), bound, Mock.Of<ICompanyCorrectiveActionCommands>());
        await using var host = await HttpHost.Start(f, profile, boundary, true, typeof(IncidentWorkflowController), services =>
        {
            services.AddSingleton(service); services.AddSingleton<IClaimsTransformation, HandoffRoleBridge>();
            services.AddAuthorization(options => options.AddPolicy(PermissionConstants.HSE.ManageIncidents, policy => policy.RequireAuthenticatedUser().RequireRole("Administrator")));
        });
        host.Http.Timeout = TimeSpan.FromMinutes(6);
        var workflow = await host.Http.GetFromJsonAsync<CompanyHseWorkflowReview>(host.Url + "/api/field/current/hse/workflow/review?incidentId=local");
        using var started = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/start", workflow!.CreateIntent(Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.OK, started.StatusCode);
        var sourceReview = await host.Http.GetFromJsonAsync<IncidentRcaSourceReview>(host.Url + "/api/field/current/hse/workflow/rca/review?incidentId=local");
        var cause = new IncidentRcaIntent(Guid.NewGuid(), sourceReview!, Assert.Single(sourceReview!.Options), "The recorded process cause requires corrective action.");
        using var recorded = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/rca/record-cause", cause);
        Assert.Equal(HttpStatusCode.OK, recorded.StatusCode); (await recorded.Content.ReadFromJsonAsync<IncidentRcaReceipt>())!.Require(cause);
        if (scenario == "cross-author")
        {
            // Model an independently recorded cause; the HTTP principal remains the assigned actor.
            var authorCause = cause with { Source = cause.Source with { Context = cause.Source.Context with { ActorUserId = "another-author" } } };
            var oldIntent = Assert.Single(f.Rows.OfType<HSE_INCIDENT_RCA_INTENT>());
            var authorIntent = HSE_INCIDENT_RCA_INTENT.Create(authorCause);
            var audit = new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler();
            audit.PrepareForInsert(authorIntent, "another-author");
            var recordedCause = Assert.Single(f.Rows.OfType<HSE_INCIDENT_CAUSE>());
            recordedCause.ROW_CREATED_BY = "another-author"; recordedCause.ROW_CHANGED_BY = "another-author";
            f.Rows.Remove(oldIntent); f.Rows.Add(authorIntent);
            var shared = await host.Http.GetFromJsonAsync<IncidentRcaRecordedCause[]>(host.Url + "/api/field/current/hse/workflow/rca/shared?incidentId=local");
            Assert.Equal("another-author", Assert.Single(shared!).RecordedBy);
            var privateCauses = await host.Http.GetFromJsonAsync<IncidentRcaIntent[]>(host.Url + "/api/field/current/hse/workflow/rca/recorded?incidentId=local");
            Assert.Empty(privateCauses!);
        }
        var review = await host.Http.GetFromJsonAsync<CompanyHseRcaHandoffReview>(host.Url + "/api/field/current/hse/workflow/rca/handoff-review?incidentId=local&intentId=" + cause.IntentId);
        if (scenario == "cross-author") Assert.Equal("another-author", review!.Cause.RecordedBy);
        var command = new CompanyHseRcaHandoffCommand(Guid.NewGuid(), review!);
        if (scenario == "lost-commit") { f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true; }
        using var first = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/rca/handoff", command);
        Assert.Equal(scenario == "lost-commit" ? HttpStatusCode.InternalServerError : HttpStatusCode.OK, first.StatusCode);
        f.LeaseCommitFailed = false; var count = f.Writes.Count;
        using var retry = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/rca/handoff", command);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode); (await retry.Content.ReadFromJsonAsync<CompanyHseRcaHandoffReceipt>())!.Require(command);
        Assert.Equal(count, f.Writes.Count);
        var recovered = await host.Http.GetFromJsonAsync<CompanyHseRcaRecoveredHandoff>(host.Url + "/api/field/current/hse/workflow/rca/handoff-recovery?incidentId=local");
        Assert.NotNull(recovered); recovered.Require(); Assert.Equal(JsonSerializer.Serialize(command), JsonSerializer.Serialize(recovered.Original));
        Assert.Equal(count, f.Writes.Count);
        var result = await process.GetProcessInstanceAsync(command.Review.InstanceId);
        Assert.Equal("INC_CORRECTIVE_ACTIONS", result!.CurrentStepId); Assert.Equal(StepStatus.COMPLETED, result.StepInstances.Single(step => step.StepId == "INC_RCA").Status);
        Assert.Single(f.Rows.OfType<HSE_INCIDENT_CAUSE>()); Assert.Single(f.Rows.OfType<PROCESS_HISTORY>().Where(row => row.EVENT_TYPE == "RCA_HANDED_OFF"));
    }

    [Theory]
    [InlineData("success")]
    [InlineData("lost-commit")]
    [InlineData("write-denied")]
    public async Task RcaHttpRecordsTheOriginalAndReconcilesCommit(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        CompanyIncidentCommandTests.EnableReportOptionProjections(f);
        f.Rows.Add(new R_INCIDENT_CAUSE_TYPE { CAUSE_TYPE = "ROOT", ACTIVE_IND = "Y" });
        f.Rows.Add(new R_INCIDENT_CAUSE_CODE { CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", LONG_NAME = "Process cause", ACTIVE_IND = "Y" });
        f.Metadata.Setup(x => x.GetTableMetadataAsync(nameof(HSE_INCIDENT_RCA_INTENT))).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata
        { EntityTypeName = typeof(HSE_INCIDENT_RCA_INTENT).FullName!, TableName = nameof(HSE_INCIDENT_RCA_INTENT), PrimaryKeyColumn = "HSE_INCIDENT_RCA_INTENT_ID" });
        f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(nameof(HSE_INCIDENT_RCA_INTENT))).ReturnsAsync(["HSE_INCIDENT_RCA_INTENT_ID"]);
        f.Source.Setup(source => source.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure requested, bool _) =>
        {
            var type = requested.EntityName == nameof(HSE_INCIDENT_RCA_INTENT) ? typeof(HSE_INCIDENT_RCA_INTENT) : typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + requested.EntityName)!;
            var schema = new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(type);
            var keys = f.Metadata.Object.GetPrimaryKeyColumnsAsync(type.Name).GetAwaiter().GetResult().ToArray();
            foreach (var field in schema.Fields) { field.IsKey = keys.Contains(field.FieldName); if (field.Fieldtype == typeof(string).FullName) { field.Size = 16384; field.Size1 = 16384; } }
            return schema;
        });
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" });
        await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        using var boundary = new Boundary(f, profile);
        boundary.Access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", PermissionConstants.HSE.ManageIncidents, null)).ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        boundary.Access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", "WRITE", null)).ReturnsAsync(new AccessCheckResponse { HasAccess = scenario != "write-denied" });
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, boundary.Orchestrator.Object, CompanyIncidentCommandTests.Commands(f), Mock.Of<IProcessService>(), Mock.Of<ICompanyCorrectiveActionCommands>());
        await using var host = await HttpHost.Start(f, profile, boundary, true, typeof(IncidentWorkflowController), services =>
        {
            services.AddSingleton(service); services.AddSingleton<IClaimsTransformation, HandoffRoleBridge>();
            services.AddAuthorization(options => options.AddPolicy(PermissionConstants.HSE.ManageIncidents, policy => policy.RequireAuthenticatedUser().RequireRole("Administrator")));
        });
        using var response = await host.Http.GetAsync(host.Url + "/api/field/current/hse/workflow/rca/review?incidentId=local");
        if (scenario == "write-denied")
        { Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); Assert.Empty(f.Writes); return; }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var review = (await response.Content.ReadFromJsonAsync<IncidentRcaSourceReview>())!;
        var original = new IncidentRcaIntent(Guid.NewGuid(), review, Assert.Single(review.Options), "The recorded process cause requires corrective action.");
        if (scenario == "lost-commit") { f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true; }
        using var first = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/rca/record-cause", original);
        Assert.Equal(scenario == "lost-commit" ? HttpStatusCode.InternalServerError : HttpStatusCode.OK, first.StatusCode);
        f.LeaseCommitFailed = false;
        var count = f.Writes.Count;
        using var retry = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/rca/record-cause", original);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        (await retry.Content.ReadFromJsonAsync<IncidentRcaReceipt>())!.Require(original);
        Assert.Equal(count, f.Writes.Count); Assert.Single(f.Rows.OfType<HSE_INCIDENT_RCA_INTENT>()); Assert.Single(f.Rows.OfType<HSE_INCIDENT_CAUSE>());
    }

    [Theory]
    [InlineData("success")]
    [InlineData("write-failure")]
    [InlineData("lost-commit")]
    public async Task HseWorkflowHttpUsesActualLifecycleOwner(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        CompanyIncidentCommandTests.EnableReportOptionProjections(f); f.EnableProcessGraphWrites();
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" });
        await f.Db.SaveChangesAsync();
        foreach (var pair in new[] { ("PROCESS_DEFINITION", "PROCESS_DEFINITION_ID"), ("PROCESS_INSTANCE", "PROCESS_INSTANCE_ID"),
            ("PROCESS_STEP_INSTANCE", "PROCESS_STEP_INSTANCE_ID"), ("PROCESS_HISTORY", "PROCESS_HISTORY_ID"), ("PROCESS_APPROVAL", "PROCESS_APPROVAL_ID"), ("CROSS_PERSONA_TASK", "CROSS_TASK_ID") })
        {
            f.Metadata.Setup(value => value.GetTableMetadataAsync(pair.Item1)).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata { EntityTypeName = pair.Item1, PrimaryKeyColumn = pair.Item2 });
            f.Metadata.Setup(value => value.GetPrimaryKeyColumnsAsync(pair.Item1)).ReturnsAsync([pair.Item2]);
        }
        f.Rows.Add(new PROCESS_DEFINITION { PROCESS_DEFINITION_ID = CompanyHseWorkflowStart.ProcessId, PROCESS_NAME = "Incident reporting", PROCESS_TYPE = "HSE",
            ENTITY_TYPE = CompanyHseWorkflowStart.EntityType, ACTIVE_IND = "Y", IS_ACTIVE = "Y", VERSION = "1",
            PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] { new ProcessStepDefinition { StepId = "review", StepName = "Review", SequenceNumber = 1, RequiredRoles = ["HSEOfficer"] } } }) });
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company); var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var personas = new Mock<IRolePersonaReader>(); personas.Setup(x => x.GetActivePersonasAsync("HSEOfficer")).ReturnsAsync(["HSE"]);
        var process = new PPDMProcessService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, "selected-tenant", personaReader: personas.Object);
        var bound = new BoundProcessService(() => modules.ResolveAsync("LIFECYCLE"), _ => process);
        using var boundary = new Boundary(f, profile);
        boundary.Access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", PermissionConstants.HSE.ManageIncidents, null)).ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        boundary.Access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", "WRITE", null)).ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, boundary.Orchestrator.Object, CompanyIncidentCommandTests.Commands(f), bound, Mock.Of<ICompanyCorrectiveActionCommands>());
        await using var host = await HttpHost.Start(f, profile, boundary, true, typeof(IncidentWorkflowController), services =>
        {
            services.AddSingleton(service); services.AddSingleton<IClaimsTransformation, HandoffRoleBridge>();
            services.AddAuthorization(options => options.AddPolicy(PermissionConstants.HSE.ManageIncidents, policy => policy.RequireAuthenticatedUser().RequireRole("Administrator")));
        });
        using var reviewResponse = await host.Http.GetAsync(host.Url + "/api/field/current/hse/workflow/review?incidentId=local");
        Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);
        var original = (await reviewResponse.Content.ReadFromJsonAsync<CompanyHseWorkflowReview>())!.CreateIntent(Guid.NewGuid());
        if (scenario == "write-failure") f.LeaseFailWriteNumber = 3;
        if (scenario == "lost-commit") { f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true; }
        using var first = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/start", original);
        if (scenario == "write-failure")
        {
            Assert.Equal(HttpStatusCode.InternalServerError, first.StatusCode);
            Assert.Empty(f.Rows.OfType<PROCESS_INSTANCE>()); Assert.Empty(f.Rows.OfType<CROSS_PERSONA_TASK>());
            Assert.Contains("rollback", f.LeaseTransactions); return;
        }
        if (scenario == "lost-commit")
        {
            Assert.Equal(HttpStatusCode.InternalServerError, first.StatusCode);
            Assert.Single(f.Rows.OfType<PROCESS_INSTANCE>()); f.LeaseCommitFailed = false;
        }
        else Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var count = f.Writes.Count;
        using var retry = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/start", original);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        (await retry.Content.ReadFromJsonAsync<CompanyHseWorkflowReceipt>())!.Require(original);
        Assert.Equal(count, f.Writes.Count);
        Assert.Single(f.Rows.OfType<PROCESS_INSTANCE>()); Assert.Single(f.Rows.OfType<PROCESS_HISTORY>()); Assert.Single(f.Rows.OfType<CROSS_PERSONA_TASK>());
    }

    [Theory]
    [InlineData("allowed")]
    [InlineData("lost-response")]
    [InlineData("permission")]
    [InlineData("write")]
    [InlineData("anonymous")]
    [InlineData("permission-revoked")]
    [InlineData("incident-changed")]
    public async Task HseWorkflowHttpChecksFieldAndReconcilesTheOriginal(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready();
        CompanyIncidentCommandTests.EnableReportOptionProjections(f);
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = "selected-tenant", ConnectionKey = "SELECTED-TENANT", ConcurrencyStamp = "lifecycle-version" });
        await f.Db.SaveChangesAsync();
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company);
        var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
        var process = new Mock<IProcessService>(); var commands = process.As<ICompanyHseWorkflowCommands>();
        var definition = new ProcessDefinition { ProcessId = CompanyHseWorkflowStart.ProcessId, ProcessName = "Incident reporting", ProcessType = "HSE", EntityType = CompanyHseWorkflowStart.EntityType,
            Steps = [new ProcessStepDefinition { StepId = "review", StepName = "Review", SequenceNumber = 1, RequiredRoles = ["HSEOfficer"] }] };
        process.Setup(x => x.GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId)).ReturnsAsync(definition);
        var bound = new BoundProcessService(() => modules.ResolveAsync("LIFECYCLE"), _ => process.Object);
        using var boundary = new Boundary(f, profile);
        boundary.Access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", PermissionConstants.HSE.ManageIncidents, null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = scenario != "permission" });
        boundary.Access.Setup(x => x.CheckAssetAccessAsync("actor", "A", "FIELD", "WRITE", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = scenario != "write" });
        var service = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, boundary.Orchestrator.Object, CompanyIncidentCommandTests.Commands(f), bound, Mock.Of<ICompanyCorrectiveActionCommands>());
        CompanyHseWorkflowStart? recorded = null; var submissions = 0;
        commands.Setup(x => x.StartHseWorkflowAsync(It.IsAny<CompanyHseWorkflowStart>(), It.IsAny<Func<Task>>(), It.IsAny<Func<Task<string>>>()))
            .Returns(async (CompanyHseWorkflowStart original, Func<Task> guard, Func<Task<string>> read) =>
            {
                await guard(); Assert.Equal(original.IncidentRevision, await read());
                if (recorded is not null) Assert.Equal(recorded, original);
                recorded = original; submissions++;
                if (scenario == "lost-response" && submissions == 1) throw new InvalidOperationException("Simulated response failure after recording");
                return new CompanyHseWorkflowReceipt(original.Context.Company, original.Context.ActorUserId, original.Context.FieldId!, original.IntentId,
                    original.IncidentId, original.InstanceId(), "step", ["task"], new string('C', 64));
            });
        await using var host = await HttpHost.Start(f, profile, boundary, scenario != "anonymous", typeof(IncidentWorkflowController), services =>
        {
            services.AddSingleton(service);
            services.AddSingleton<IClaimsTransformation, HandoffRoleBridge>();
            services.AddAuthorization(options => options.AddPolicy(PermissionConstants.HSE.ManageIncidents, policy => policy.RequireAuthenticatedUser().RequireRole("Administrator")));
        });
        using var response = await host.Http.GetAsync(host.Url + "/api/field/current/hse/workflow/review?incidentId=local");
        if (scenario is "permission" or "write" or "anonymous")
        {
            Assert.Equal(scenario == "anonymous" ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Null(recorded); return;
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var review = (await response.Content.ReadFromJsonAsync<CompanyHseWorkflowReview>())!;
        Assert.Equal("local", review.IncidentId);
        var intent = review.CreateIntent(Guid.NewGuid());
        if (scenario == "permission-revoked")
        {
            f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(row => row.ClaimValue == PermissionConstants.HSE.ManageIncidents));
            await f.Db.SaveChangesAsync();
        }
        if (scenario == "incident-changed")
            f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.INCIDENT_ID == "local" && row.RESPONSE_TYPE == "STATUS").RESPONSE_RESULT = IncidentState.UnderInvestigation;
        using var first = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/start", intent);
        if (scenario is "permission-revoked" or "incident-changed")
        {
            Assert.Equal(scenario == "permission-revoked" ? HttpStatusCode.Forbidden : HttpStatusCode.Conflict, first.StatusCode);
            Assert.Null(recorded); return;
        }
        if (scenario == "lost-response")
        {
            Assert.Equal(HttpStatusCode.InternalServerError, first.StatusCode);
            using var retry = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/start", intent);
            Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
            (await retry.Content.ReadFromJsonAsync<CompanyHseWorkflowReceipt>())!.Require(intent);
            Assert.Equal(2, submissions);
        }
        else
        {
            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
            (await first.Content.ReadFromJsonAsync<CompanyHseWorkflowReceipt>())!.Require(intent);
            Assert.Equal(1, submissions);
        }
        Assert.Equal(intent, recorded);
    }
}
