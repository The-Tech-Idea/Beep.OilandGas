using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Repository.Models;
#if RECORDED_APPROVAL_HTTP_FOCUSED
using System.Data;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Beep.OilandGas.ApiService.Controllers.HSE;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact]
    public Task InstalledSqliteClosureRecoversLostHttpAcknowledgementAndReopens() => QualifyInstalledHseClosureAsync(HseClosureQualification.Transport);

    [Fact]
    public Task InstalledSqliteConcurrentClosureWorkersReconcileOneNativeEffect() => QualifyInstalledHseClosureAsync(HseClosureQualification.ConcurrentOriginal);

    [Fact]
    public Task InstalledSqliteClosureObservesCrossProcessAuthorityRevocation() => QualifyInstalledHseClosureAsync(HseClosureQualification.RevokedAuthority);

    [Fact]
    public Task InstalledSqliteClosureRejectsForeignAccountAndConflictingOriginal() => QualifyInstalledHseClosureAsync(HseClosureQualification.OriginalIsolation);

    [Fact]
    public Task InstalledSqliteCompetingFinalApprovalsRecordOneClosure() => QualifyInstalledHseClosureAsync(HseClosureQualification.CompetingApprovals);

    [Fact]
    public Task InstalledSqliteDifferentAccountsCompeteForOneFinalClosure() => QualifyInstalledHseClosureAsync(HseClosureQualification.DifferentAccounts);

    private enum HseClosureQualification { Transport, ConcurrentOriginal, RevokedAuthority, OriginalIsolation, CompetingApprovals, DifferentAccounts }
    private async Task QualifyInstalledHseClosureAsync(HseClosureQualification qualification)
    {
        CompanyPPDMRowAccessTests.Fixture? retainedFixture = null;
        ClosedProviderState? retained = null;
        try
        {
        await new PermitProviderQualificationTests().WithInstalledProviderAsync(async (editor, core) =>
        {
            var target = editor.ConfigEditor.DataConnections.Single(c => c.ConnectionName == core.DatasourceName);
            var f = await CompanyIncidentCommandTests.Ready(await CompanyPPDMRowAccessTests.Fixture.Create(target));
            retainedFixture = f;
            f.Db.Roles.Add(new IdentityRole { Id = "provider-hse", Name = "HSEOfficer", NormalizedName = "HSEOFFICER" });
            f.Db.UserRoles.Add(new IdentityUserRole<string> { UserId = "actor", RoleId = "provider-hse" });
            f.Db.Add(new AppUserRoleExtension { TenantId = "tenant", BusinessAssociateId = "company", UserId = "actor", RoleId = "provider-hse" });
            f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = PermissionConstants.Workflow.Approve });
            await f.Db.SaveChangesAsync();
            var columns = new CommonColumnHandler();
            var metadata = HseIncidentNativeMetadata.AddTo(new());
            foreach (var type in new[] { typeof(HSE_CA_VERIFICATION), typeof(HSE_INCIDENT_CLOSURE), typeof(HSE_INCIDENT_RCA_INTENT) })
            {
                f.Metadata.Setup(x => x.GetTableMetadataAsync(type.Name)).ReturnsAsync(metadata[type.Name]);
                f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync([metadata[type.Name].PrimaryKeyColumn]);
            }
            var step = f.Rows.OfType<PROJECT_STEP>().Single(r => r.PROJECT_ID == CompanyCorrectiveActionAuthorityTests.Plan);
            step.ACTUAL_END_DATE = DateTime.UtcNow.AddHours(-1); step.REMARK = "Valve pressure test completed by the operator.";
            step.ROW_CHANGED_BY = "operator";
            f.Rows.Add(new PROJECT_STATUS { PROJECT_ID = step.PROJECT_ID, STEP_ID = step.STEP_ID, STATUS_ID = "completed-status",
                STATUS_TYPE = "CA_STEP", STATUS = "COMPLETED", ACTIVE_IND = "Y", EFFECTIVE_DATE = step.ACTUAL_END_DATE,
                REMARK = step.REMARK, SOURCE = "Beep.OilandGas.HSE", ROW_CREATED_BY = "operator", ROW_CHANGED_BY = "operator",
                ROW_CREATED_DATE = DateTime.UtcNow.AddHours(-1), ROW_CHANGED_DATE = DateTime.UtcNow.AddHours(-1) });
            f.Rows.Add(new R_INCIDENT_CAUSE_TYPE { CAUSE_TYPE = "ROOT", ACTIVE_IND = "Y" });
            f.Rows.Add(new R_INCIDENT_CAUSE_CODE { CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", LONG_NAME = "Process cause", ACTIVE_IND = "Y" });
            var nativeTypes = f.Rows.Select(r => r.GetType()).Concat([typeof(HSE_CA_VERIFICATION), typeof(HSE_INCIDENT_CLOSURE), typeof(HSE_INCIDENT_RCA_INTENT), typeof(HSE_INCIDENT_CAUSE), typeof(PROJECT_COMPONENT)]).Distinct().ToArray();
            await Migrate(core, nativeTypes, async type => (await f.Metadata.Object.GetPrimaryKeyColumnsAsync(type.Name)).ToList());
            var proofColumn = core.GetEntityStructure(nameof(HSE_INCIDENT_CLOSURE), true).Fields.Single(field => field.FieldName == "COMMAND_JSON");
            Assert.True(Math.Max(proofColumn.Size, proofColumn.Size1) >= 65536, "Canonical closure proof width must survive Beep Migration.");
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(target.Database)!, "native-schema.json"), JsonSerializer.Serialize(new[] { typeof(HSE_INCIDENT_RCA_INTENT), typeof(HSE_INCIDENT_CAUSE), typeof(HSE_INCIDENT_CLOSURE) }.Select(type => new { table = type.Name, fields = core.GetEntityStructure(type.Name, true).Fields.Select(field => new { field.FieldName, field.Fieldtype, field.Size, field.Size1, field.IsKey }) })));
            foreach (var row in f.Rows)
                Assert.True(core.InsertEntity(row.GetType().Name, row).Flag == Errors.Ok, row.GetType().Name + ": " + core.ErrorObject.Message);
            f.Editor.Setup(x => x.GetDataSource(core.DatasourceName)).Returns(core);
            f.Editor.Setup(x => x.OpenDataSource(core.DatasourceName)).Returns(ConnectionState.Open);
            var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
            var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
            var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile,
                new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), editor, columns, f.Defaults.Object, f.Metadata.Object, f.Policy);
            var directory = Path.GetDirectoryName(target.Database)!;
            var lifecycleTarget = new ConnectionProperties { ConnectionName = "hse-lifecycle-physical", DatabaseType = target.DatabaseType,
                Database = Path.Combine(directory, "lifecycle.db"), ConnectionString = "Data Source=" + Path.Combine(directory, "lifecycle.db"), GuidID = Guid.NewGuid().ToString() };
            editor.ConfigEditor.DataConnections.Add(lifecycleTarget);
            var lifecycle = editor.GetDataSource(lifecycleTarget.ConnectionName);
            Assert.NotNull(lifecycle); Assert.Equal(ConnectionState.Open, lifecycle.Openconnection());
            Assert.NotSame(core, lifecycle);
            try
            {
                var lifecycleTypes = new[] { typeof(PROCESS_DEFINITION), typeof(PROCESS_INSTANCE), typeof(PROCESS_STEP_INSTANCE), typeof(PROCESS_HISTORY), typeof(PROCESS_APPROVAL), typeof(CROSS_PERSONA_TASK) };
                string Key(Type t) => t == typeof(CROSS_PERSONA_TASK) ? "CROSS_TASK_ID" : t.Name + "_ID";
                foreach (var type in lifecycleTypes)
                {
                    f.Metadata.Setup(x => x.GetTableMetadataAsync(type.Name)).ReturnsAsync(new PPDMTableMetadata { TableName = type.Name, EntityTypeName = type.FullName!, PrimaryKeyColumn = Key(type) });
                    f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync([Key(type)]);
                }
                await Migrate(lifecycle, lifecycleTypes, t => Task.FromResult(new List<string> { Key(t) }));
                var personas = new Mock<IRolePersonaReader>(); personas.Setup(x => x.GetActivePersonasAsync("HSEOfficer")).ReturnsAsync(["HSE"]);
                var process = new PPDMProcessService(editor, columns, f.Defaults.Object, f.Metadata.Object, lifecycle.DatasourceName, NullLogger<PPDMProcessService>.Instance, personaReader: personas.Object);
                await process.CreateProcessDefinitionAsync(IncidentReportingDefinition.Create(), "actor");
                var recorded = (await process.GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId))!;
                Assert.Equal(8, recorded.Steps.Count);
                Assert.Equal("INC_REPORT", recorded.Steps.OrderBy(step => step.SequenceNumber).First().StepId);
                f.Editor.Object.ConfigEditor.DataConnections.Add(lifecycleTarget);
                f.Db.Add(new AppModuleConnection { TenantId = "tenant", ConnectionKey = lifecycleTarget.ConnectionName.ToUpperInvariant(),
                    AssignedByUserId = "actor", PhysicalTargetKey = ModuleDatabaseTarget.Key(f.Editor.Object, lifecycleTarget) });
                f.Db.ModuleDatabases.Add(new ModuleDatabaseBinding { TenantId = "tenant", ModuleId = "LIFECYCLE", ConnectionName = lifecycleTarget.ConnectionName,
                    ConnectionKey = lifecycleTarget.ConnectionName.ToUpperInvariant(), ConcurrencyStamp = "provider-lifecycle-current" });
                await f.Db.SaveChangesAsync();
                var tenant = new Beep.OilandGas.Repository.RepositoryTenantContext(); tenant.SelectCompany(f.Gate.Company);
                var modules = new ModuleConnectionResolver(f.Db, f.Editor.Object, tenant);
                var binding = new AssetDatabaseScope(lifecycle.DatasourceName, await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"], lifecycle.DatasourceName));
                var start = new CompanyHseWorkflowStart(context, binding, Guid.NewGuid(), "local", new string('B', 64), ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(recorded)));
                Task Role(string role) { Assert.Equal("HSEOfficer", role); return Task.CompletedTask; }
                await process.StartHseWorkflowAsync(start, () => Task.CompletedTask, () => { PPDMDataSourceTransaction.RequireIdle(core); return Task.FromResult(start.IncidentRevision); });
                // Earlier reporting/investigation actions and task progression are explicit fixture composition.
                var router = new CrossPersonaTaskRouter(editor, columns, f.Defaults.Object, f.Metadata.Object,
                    () => Task.FromResult(lifecycle.DatasourceName), personas: personas.Object);
                foreach (var earlier in recorded.Steps.Where(step => step.SequenceNumber < 5).OrderBy(step => step.SequenceNumber))
                {
                    var before = (await process.GetProcessInstanceAsync(start.InstanceId()))!;
                    var current = before.StepInstances.Single(step => step.StepId == earlier.StepId);
                    Assert.Equal(earlier.StepId, before.CurrentStepId);
                    await PPDMDataSourceTransaction.ExecuteAsync(lifecycle, async () =>
                        await router.CompleteTaskSetInOwnedTransactionAsync(before.InstanceId, current.StepInstanceId,
                            "HSEOfficer", CompanyHseWorkflowStart.EntityType, "local", "actor", lifecycle.DatasourceName));
                    Assert.True(await process.CompleteStepAsync(before.InstanceId, earlier.StepId, "RECORDED", "actor"));
                    var after = (await process.GetProcessInstanceAsync(start.InstanceId()))!;
                    var next = after.StepInstances.Single(step => step.StepId == after.CurrentStepId);
                    await PPDMDataSourceTransaction.ExecuteAsync(lifecycle, async () =>
                        await router.RouteTaskInOwnedTransactionAsync(after.InstanceId, next.StepInstanceId,
                            "HSEOfficer", CompanyHseWorkflowStart.EntityType, "local", "Recorded incident", "actor"));
                }
                var incidents = new PPDMCompanyIncidentCommands(f.Gate, profile, editor, columns, f.Defaults.Object, f.Metadata.Object, f.Policy);
                var causeSource = await incidents.ReviewRcaSourceAsync(context, "local");
                var cause = new IncidentRcaIntent(Guid.NewGuid(), causeSource, Assert.Single(causeSource.Options), "Recorded process cause requires the verified valve correction.");
                (await incidents.RecordRcaAsync(cause)).Require(cause);
                var shared = Assert.Single(await incidents.ReadSharedRcaAsync(context, "local"));
                var handoffReview = await process.ReviewRcaHandoffAsync(context, binding, shared, Role);
                var handoff = new CompanyHseRcaHandoffCommand(Guid.NewGuid(), handoffReview);
                async Task RequireCause()
                {
                    PPDMDataSourceTransaction.RequireIdle(core);
                    Assert.Equal(JsonSerializer.Serialize(shared), JsonSerializer.Serialize(Assert.Single(await incidents.ReadSharedRcaAsync(context, "local"))));
                }
                (await process.CompleteRcaHandoffAsync(handoff, Role, RequireCause)).Require(handoff);
                var actions = await commands.ReviewVerificationAsync(context, "local", step.PROJECT_ID, "actor");
                Assert.True(actions.CanReviewIndependently);
                var decision = new CAVerificationDecision(Guid.NewGuid(), actions, "VERIFIED", "Independent pressure inspection confirms the recorded repair.");
                (await commands.RecordVerificationAsync(decision)).Require();
                var evidence = await commands.ReviewClosureAsync(context, "local", step.PROJECT_ID, "actor");
                Assert.True(evidence.VerificationSatisfied);
                var nativeRevision = await commands.ReviewNativeClosureRevisionAsync(context, "local", step.PROJECT_ID);

                CompanyHseClosureCommand? final = null; CompanyHseClosureReceipt? receipt = null;
                for (var i = 0; i < 3; i++)
                {
                    var review = await process.ReviewClosureStageAsync(evidence, binding, nativeRevision, Role);
                    final = new CompanyHseClosureCommand(Guid.NewGuid(), review, "Reviewed this exact independently verified incident stage.");
                    if (i == 2 && qualification is HseClosureQualification.CompetingApprovals or HseClosureQualification.DifferentAccounts) { receipt = null; break; }
                    receipt = await process.CompleteClosureStageAsync(final, Role, RequireCause); receipt.Require(final);
                }
                Task ConfirmLifecycle() => Confirm();
                async Task Confirm()
                {
                    PPDMDataSourceTransaction.RequireIdle(core);
                    var confirmed = await process.CompleteClosureStageAsync(final!, Role, RequireCause);
                    Assert.Equal(JsonSerializer.Serialize(receipt), JsonSerializer.Serialize(confirmed));
                }
                if (qualification is not (HseClosureQualification.CompetingApprovals or HseClosureQualification.DifferentAccounts))
                {
                // Actual app permission revocation after lifecycle approval must leave the native store unchanged.
                var manage = f.Db.RoleClaims.Single(claim => claim.ClaimValue == Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.ManageIncidents);
                f.Db.RoleClaims.Remove(manage); await f.Db.SaveChangesAsync();
                await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(() => commands.RecordIncidentClosedAsync(final!, receipt!, ConfirmLifecycle));
                Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
                f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.ManageIncidents });
                await f.Db.SaveChangesAsync();
                }
                var closed = qualification switch
                {
                    HseClosureQualification.CompetingApprovals => await RaceInstalledFinalApprovalsAsync(f, core, lifecycle, final!, target, lifecycleTarget, directory, differentAccounts: false),
                    HseClosureQualification.DifferentAccounts => await RaceInstalledFinalApprovalsAsync(f, core, lifecycle, final!, target, lifecycleTarget, directory, differentAccounts: true),
                    HseClosureQualification.ConcurrentOriginal => await RaceInstalledHseClosureAsync(f, core, lifecycle, final!, receipt!, target, lifecycleTarget, commands, ConfirmLifecycle, directory),
                    HseClosureQualification.RevokedAuthority => await RevokeInstalledHseClosureAuthorityAsync(f, core, lifecycle, final!, receipt!, target, lifecycleTarget, commands, ConfirmLifecycle, directory),
                    HseClosureQualification.OriginalIsolation => await IsolateInstalledHseClosureOriginalAsync(f, core, lifecycle, final!, receipt!, target, lifecycleTarget, directory),
                    _ => await LoseHttpAcknowledgementThenRecover(f, profile, modules, incidents, commands, process, core, lifecycle, final!, directory)
                };
                closed.Require();
                final = closed.Original; receipt = closed.Lifecycle;
                // The hosted request clears its AsyncLocal accessor; direct fixture work starts a fresh authenticated context.
                f.Http.HttpContext = f.Request;
                Assert.Equal(ProcessStatus.COMPLETED, (await process.GetProcessInstanceAsync(start.InstanceId()))!.Status);
                var statuses = (await core.GetEntityAsync(nameof(HSE_INCIDENT_RESPONSE), [new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = "local" }])).Cast<object>().ToArray();
                Assert.Equal(3, statuses.Length);
                Assert.Single(statuses, row => (string?)row.GetType().GetProperty("RESPONSE_RESULT")!.GetValue(row) == "CLOSED");
                Assert.Single((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
                Assert.Single((await core.GetEntityAsync(nameof(HSE_CA_VERIFICATION), [])).Cast<object>());
                Assert.Single((await core.GetEntityAsync(nameof(HSE_INCIDENT_RCA_INTENT), [])).Cast<object>());
                Assert.Single((await core.GetEntityAsync(nameof(HSE_INCIDENT_CAUSE), [])).Cast<object>());
                Assert.All((await lifecycle.GetEntityAsync(nameof(CROSS_PERSONA_TASK), [])).Cast<object>(), row => Assert.Equal("COMPLETED", row.GetType().GetProperty("TASK_STATUS")!.GetValue(row)));
                Assert.Equal(2, (await lifecycle.GetEntityAsync(nameof(PROCESS_APPROVAL), [])).Cast<object>().Count());
                Assert.False(core.CheckEntityExist(nameof(PROCESS_INSTANCE)));
                Assert.False(lifecycle.CheckEntityExist(nameof(HSE_INCIDENT_CLOSURE)));
                retained = new(f, target, lifecycleTarget, binding, JsonSerializer.Serialize(final), JsonSerializer.Serialize(closed), JsonSerializer.Serialize(shared), core, lifecycle);
            }
            finally { lifecycle.Closeconnection(); lifecycle.Dispose(); }

            async Task Migrate(IDataSource source, Type[] types, Func<Type, Task<List<string>>> keys)
            {
                var annotated = types.Where(type => type.GetProperties().Any(property => property.IsDefined(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true))).ToArray();
                var model = new MigrationModel { Source = "Plan353FocusedProvider", SourceId = source.DatasourceName };
                foreach (var type in types.Except(annotated))
                {
                    var primary = await keys(type); Assert.NotEmpty(primary);
                    model.Entities.Add(type.FullName!, new MigrationModelEntity { ClrTypeFullName = type.FullName!, TableName = type.Name,
                        Properties = new ClassCreator(editor).ConvertToEntityStructure(type).Fields.Select(field => new MigrationModelProperty {
                            PropertyName = field.FieldName, ColumnName = field.FieldName, FieldType = field.Fieldtype,
                            IsPrimaryKey = primary.Contains(field.FieldName), IsNullable = !primary.Contains(field.FieldName),
                            MaxLength = field.Fieldtype == typeof(string).FullName ? 262144 : null }).ToList() });
                }
                var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = "hse-independent-stores" };
                if (model.Entities.Count > 0)
                {
                    var plan = migration.BuildMigrationPlanForModel(model, detectRelationships: false);
                    migration.ApproveMigrationPlan(plan, "plan353-focused-provider");
                    var result = migration.ExecuteMigrationPlan(plan); Assert.True(result.Success, result.Message + " | " + source.ErrorObject.Message);
                }
                if (annotated.Length > 0)
                {
                    // Match the application module owner: preserve canonical Key/MaxLength annotations.
                    var typed = new MigrationManager(editor, source) { ExecutionTargetIdentity = "hse-independent-stores" };
                    var plan = typed.BuildMigrationPlanForTypes(annotated, detectRelationships: false);
                    typed.ApproveMigrationPlan(plan, "plan353-focused-provider");
                    var result = typed.ExecuteMigrationPlan(plan); Assert.True(result.Success, result.Message + " | " + source.ErrorObject.Message);
                }
            }
        }, connectionName: "selected-tenant");
        Assert.NotNull(retained);
        await RecoverWithFreshOwners(retained);
        }
        finally { retainedFixture?.Dispose(); }
    }

    private static async Task<IncidentClosedReceipt> LoseHttpAcknowledgementThenRecover(CompanyPPDMRowAccessTests.Fixture f,
        RepositoryUserProfileService profile, ModuleConnectionResolver modules, PPDMCompanyIncidentCommands incidents,
        PPDMCompanyCorrectiveActionCommands commands, PPDMProcessService process, IDataSource core, IDataSource lifecycle,
        CompanyHseClosureCommand original, string directory)
    {
        var previousServices = f.Request.RequestServices;
        var boundary = new Boundary(f, profile);
        var dropped = 0; long bufferedBytes = 0; var calls = new List<object>();
        try
        {
            var coordinator = new CompanyHseWorkflowService(f.Gate, modules, profile, profile, boundary.Orchestrator.Object, incidents,
                new BoundProcessService(() => modules.ResolveAsync("LIFECYCLE"), _ => process), commands);
            await using var host = await HttpHost.Start(f, profile, boundary, true, typeof(IncidentWorkflowController), services =>
            {
                services.AddSingleton(coordinator); services.AddSingleton<ICompanyCorrectiveActionCommands>(commands);
                services.AddSingleton<IClaimsTransformation, HandoffRoleBridge>();
                services.AddAuthorization(options =>
                {
                    foreach (var permission in new[] { PermissionConstants.HSE.ManageIncidents, PermissionConstants.Workflow.Approve })
                        options.AddPolicy(permission, policy => policy.RequireAuthenticatedUser().RequireRole("Administrator"));
                });
            }, beforeEndpoints: app => app.Use(async (context, next) =>
            {
                var timer = System.Diagnostics.Stopwatch.StartNew();
                var marker = Environment.GetEnvironmentVariable("OILGAS_HSE_CLOSURE_PROFILE_MARKER");
                if (!string.IsNullOrWhiteSpace(marker) && context.Request.Headers["X-Qualification-Lose-Closure-Ack"] == "once")
                    File.WriteAllText(marker, JsonSerializer.Serialize(new { ProcessId = Environment.ProcessId, StartedUtc = DateTimeOffset.UtcNow, Directory = directory }));
                if (context.Request.Headers["X-Qualification-Lose-Closure-Ack"] != "once")
                {
                    await next(); calls.Add(new { stage = context.Request.Path.Value!.EndsWith("/complete-stage", StringComparison.Ordinal) ? "lifecycle-approval" : "native-retry", status = context.Response.StatusCode, milliseconds = timer.Elapsed.TotalMilliseconds }); return;
                }
                var responseStream = context.Response.Body;
                await using var buffer = new MemoryStream(); context.Response.Body = buffer;
                try
                {
                    await next();
                    if (context.Response.StatusCode != 200)
                    {
                        buffer.Position = 0; await buffer.CopyToAsync(responseStream); return;
                    }
                    var confirmed = JsonSerializer.Deserialize<CompanyHseClosureOutcome>(buffer.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                    confirmed.Require(); Assert.NotNull(confirmed.Native);
                    Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(confirmed.Original));
                    bufferedBytes = buffer.Length; Assert.True(bufferedBytes > 0);
                    Assert.Equal(0, Interlocked.Exchange(ref dropped, 1));
                    calls.Add(new { stage = "acknowledgement-dropped", status = context.Response.StatusCode, milliseconds = timer.Elapsed.TotalMilliseconds, bufferedBytes });
                    context.Abort();
                }
                finally { context.Response.Body = responseStream; }
            }));
            // Web AddHttpClient<ApiClient> uses the HttpClient default; do not give qualification a longer deadline.
            Assert.Equal(TimeSpan.FromSeconds(100), host.Http.Timeout);
            var approvalTimer = System.Diagnostics.Stopwatch.StartNew();
            using var approvalResponse = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/workflow/closure/complete-stage", original);
            Assert.True(approvalResponse.IsSuccessStatusCode, await approvalResponse.Content.ReadAsStringAsync());
            var approval = (await approvalResponse.Content.ReadFromJsonAsync<CompanyHseClosureReceipt>())!; approval.Require(original);
            var approvalClientMilliseconds = approvalTimer.Elapsed.TotalMilliseconds;
            Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
            var approved = new CompanyHseClosureRecovery(original, approval); approved.Require();
            var url = host.Url + "/api/field/current/hse/workflow/closure/record-native";
            var approvePermission = await f.Db.RoleClaims.SingleAsync(c => c.RoleId == "admin" && c.ClaimValue == PermissionConstants.Workflow.Approve);
            f.Db.RoleClaims.Remove(approvePermission); await f.Db.SaveChangesAsync();
            using (var denied = await host.Http.PostAsJsonAsync(url, approved))
                Assert.Equal(System.Net.HttpStatusCode.Forbidden, denied.StatusCode);
            Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
            f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = PermissionConstants.Workflow.Approve }); await f.Db.SaveChangesAsync();
            using var first = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(approved) };
            first.Headers.Add("X-Qualification-Lose-Closure-Ack", "once");
            Exception? uncertainty = null;
            try
            {
                using var reply = await host.Http.SendAsync(first);
                var delivered = await reply.Content.ReadFromJsonAsync<CompanyHseClosureOutcome>();
                Assert.Fail("The controlled lost acknowledgement unexpectedly delivered a closure outcome.");
            }
            catch (Exception error) when (error is HttpRequestException or IOException or JsonException) { uncertainty = error; }
            Assert.NotNull(uncertainty); Assert.Equal(1, dropped);
            Assert.Single((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
            string Hash(string name)
            {
                PPDMDataSourceTransaction.RequireIdle(core); PPDMDataSourceTransaction.RequireIdle(lifecycle);
                using var input = new FileStream(Path.Combine(directory, name), FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(input));
            }
            var nativeBeforeRetry = Hash("permits.db"); var lifecycleBeforeRetry = Hash("lifecycle.db");
            using var retry = await host.Http.PostAsJsonAsync(url, approved);
            Assert.True(retry.IsSuccessStatusCode, await retry.Content.ReadAsStringAsync()); Assert.True(retry.Headers.CacheControl?.NoStore);
            var recovered = (await retry.Content.ReadFromJsonAsync<CompanyHseClosureOutcome>())!; recovered.Require();
            Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(recovered.Original)); Assert.NotNull(recovered.Native);
            Assert.Equal(nativeBeforeRetry, Hash("permits.db")); Assert.Equal(lifecycleBeforeRetry, Hash("lifecycle.db"));
            File.WriteAllText(Path.Combine(directory, "http-loss-observations.json"), JsonSerializer.Serialize(new { Dropped = dropped,
                BufferedAcknowledgementBytes = bufferedBytes, Failure = uncertainty.GetType().Name, LifecycleApprovalClientMilliseconds = approvalClientMilliseconds, calls,
                NativeHashBeforeRetry = nativeBeforeRetry, NativeHashAfterRetry = Hash("permits.db"),
                LifecycleHashBeforeRetry = lifecycleBeforeRetry, LifecycleHashAfterRetry = Hash("lifecycle.db") }));
            return recovered.Native;
        }
        finally { boundary.Dispose(); f.Request.RequestServices = previousServices; f.Http.HttpContext = f.Request; }
    }

    private sealed record ClosedProviderState(CompanyPPDMRowAccessTests.Fixture Fixture, ConnectionProperties Core,
        ConnectionProperties Lifecycle, AssetDatabaseScope LifecycleBinding, string OriginalJson, string ClosedJson, string CauseJson,
        IDataSource PreviousCore, IDataSource PreviousLifecycle);

    private static async Task RecoverWithFreshOwners(ClosedProviderState saved)
    {
        var directory = Path.GetDirectoryName(saved.Core.Database)!;
        string Hash(string path) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path)));
        var beforeCore = Hash(saved.Core.Database); var beforeLifecycle = Hash(saved.Lifecycle.Database);
        await new PermitProviderQualificationTests().WithInstalledProviderAsync(async (editor, core) =>
        {
            Assert.NotSame(saved.PreviousCore, core);
            Assert.False(core.CheckEntityExist(nameof(PROCESS_INSTANCE)));
            var f = saved.Fixture;
            var winnerActor = JsonSerializer.Deserialize<CompanyHseClosureCommand>(saved.OriginalJson)!.Review.Context.ActorUserId;
            if (f.Gate.Company.UserId != winnerActor) await f.UseExistingRegistryAsync(Path.Combine(directory, "foundation-snapshot.db"), winnerActor);
            f.Editor.Setup(x => x.GetDataSource(core.DatasourceName)).Returns(core);
            f.Editor.Setup(x => x.OpenDataSource(core.DatasourceName)).Returns(ConnectionState.Open);
            var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
            var selected = await profile.GetCurrentFieldSelectionAsync();
            await profile.SetCurrentFieldSelectionAsync(new() { FieldId = "A", Context = selected.SelectionContext });
            var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
            var columns = new CommonColumnHandler();
            var incidents = new PPDMCompanyIncidentCommands(f.Gate, profile, editor, columns, f.Defaults.Object, f.Metadata.Object, f.Policy);
            var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile,
                new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), editor, columns, f.Defaults.Object, f.Metadata.Object, f.Policy);
            editor.ConfigEditor.DataConnections.Add(saved.Lifecycle);
            var lifecycle = editor.GetDataSource(saved.Lifecycle.ConnectionName);
            Assert.NotNull(lifecycle); Assert.NotSame(saved.PreviousLifecycle, lifecycle);
            Assert.Equal(ConnectionState.Open, lifecycle.Openconnection());
            try
            {
                var personas = new Mock<IRolePersonaReader>(); personas.Setup(x => x.GetActivePersonasAsync("HSEOfficer")).ReturnsAsync(["HSE"]);
                var process = new PPDMProcessService(editor, columns, f.Defaults.Object, f.Metadata.Object, lifecycle.DatasourceName,
                    NullLogger<PPDMProcessService>.Instance, personaReader: personas.Object);
                Task Role(string role) { Assert.Equal("HSEOfficer", role); return Task.CompletedTask; }
                async Task RequireCause(CompanyHseClosureCommand _)
                {
                    PPDMDataSourceTransaction.RequireIdle(core);
                    Assert.Equal(saved.CauseJson, JsonSerializer.Serialize(Assert.Single(await incidents.ReadSharedRcaAsync(context, "local"))));
                }
                var recovered = await process.RecoverClosureStageAsync(context, saved.LifecycleBinding, "local", Role, RequireCause);
                Assert.NotNull(recovered); recovered.Require();
                Assert.Equal(saved.OriginalJson, JsonSerializer.Serialize(recovered.Original));
                async Task ConfirmLifecycle()
                {
                    PPDMDataSourceTransaction.RequireIdle(core);
                    var again = await process.RecoverClosureStageAsync(context, saved.LifecycleBinding, "local", Role, RequireCause);
                    Assert.Equal(JsonSerializer.Serialize(recovered), JsonSerializer.Serialize(again));
                }
                var result = await commands.RecordIncidentClosedAsync(recovered.Original, recovered.Receipt, ConfirmLifecycle);
                result.Require(); Assert.Equal(saved.ClosedJson, JsonSerializer.Serialize(result));
                Assert.Single((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
                var statuses = (await core.GetEntityAsync(nameof(HSE_INCIDENT_RESPONSE), [new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = "local" }])).Cast<object>().ToArray();
                Assert.Equal(3, statuses.Length);
                Assert.Single(statuses, row => (string?)row.GetType().GetProperty("RESPONSE_RESULT")!.GetValue(row) == "CLOSED");
                Assert.Equal(2, (await lifecycle.GetEntityAsync(nameof(PROCESS_APPROVAL), [])).Cast<object>().Count());
                Assert.All((await lifecycle.GetEntityAsync(nameof(CROSS_PERSONA_TASK), [])).Cast<object>(), row => Assert.Equal("COMPLETED", row.GetType().GetProperty("TASK_STATUS")!.GetValue(row)));
            }
            finally { lifecycle.Closeconnection(); lifecycle.Dispose(); }
        }, retainedDirectory: directory, requireExisting: true, connectionName: saved.Core.ConnectionName, configuredConnection: saved.Core);
        Assert.Equal(beforeCore, Hash(saved.Core.Database)); Assert.Equal(beforeLifecycle, Hash(saved.Lifecycle.Database));
        File.WriteAllText(Path.Combine(directory, "reopen-observations.json"), JsonSerializer.Serialize(new { FreshEditor = true,
            FreshNativeSource = true, FreshLifecycleSource = true, OriginalRecoveredFromDatabase = true,
            NativeHashBefore = beforeCore, NativeHashAfter = Hash(saved.Core.Database),
            LifecycleHashBefore = beforeLifecycle, LifecycleHashAfter = Hash(saved.Lifecycle.Database),
            FoundationRegistry = "Retained controlled fixture; not a whole application process restart" }));
    }
}
#endif
