#if HSE_SQLITE_FOCUSED
extern alias purchase;
using System.Data;
using System.Reflection;
using System.Text.Json;
using PROCESS_DEFINITION = purchase::Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_DEFINITION;
using PROCESS_INSTANCE = purchase::Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE;
using PROCESS_STEP_INSTANCE = purchase::Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_STEP_INSTANCE;
using PROCESS_HISTORY = purchase::Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_HISTORY;
using PROCESS_APPROVAL = purchase::Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_APPROVAL;
using CROSS_PERSONA_TASK = purchase::Beep.OilandGas.LifeCycle.Data.Tables.CROSS_PERSONA_TASK;
using PPDMProcessService = purchase::Beep.OilandGas.LifeCycle.Services.Processes.PPDMProcessService;
using IRolePersonaReader = purchase::Beep.OilandGas.LifeCycle.Services.Processes.IRolePersonaReader;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyHseWorkflowSqliteTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task RealSqliteDatasourcePersistsAndReconcilesHseWorkflowGraph(bool closure, bool correction)
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-hse-sqlite-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var connection = new ConnectionProperties { ConnectionName = "hse_lifecycle", DatabaseType = DataSourceType.SqlLite,
            ConnectionString = "Data Source=" + Path.Combine(directory, "lifecycle.db"), Database = Path.Combine(directory, "lifecycle.db"), GuidID = Guid.NewGuid().ToString() };
        var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>(); var driver = ConnectionHelper.CreateSQLiteConfig();
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes); config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties> { connection });
        config.SetupGet(x => x.DataTypesMap).Returns(TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSQLiteDataTypesMapping());
        config.As<IMigrationExecutionStorageProvider>().Setup(x => x.CaptureMigrationExecutionStorage()).Returns(new TheTechIdea.Beep.Services.Persistence.FileMigrationExecutionStorage(directory, new TheTechIdea.Beep.JsonLoaderService.JsonLoader()));
        var log = new Mock<IDMLogger>(); var messages = new List<string>(); log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        var queries = new TheTechIdea.Beep.ConfigUtil.Managers.QueryManager(log.Object, Mock.Of<IJsonLoader>(), string.Empty);
        config.Setup(x => x.GetSql(It.IsAny<Sqlcommandtype>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<QuerySqlRepo>>(), It.IsAny<DataSourceType>()))
            .Returns((Sqlcommandtype kind, string table, string schema, string filter, List<QuerySqlRepo> catalog, DataSourceType provider) => queries.GetSql(kind, table, schema ?? string.Empty, filter, provider));
        var errors = new ErrorsInfo(); var util = new TheTechIdea.Beep.Utils.Util(log.Object, errors, config.Object);
        using var handler = new AssemblyHandler(config.Object, errors, log.Object, util);
        using var editor = new DMEEditor(log.Object, util, errors, config.Object, handler); util.DME = editor;
        try
        {
            handler.ScanAssembly(Assembly.LoadFrom(Environment.GetEnvironmentVariable("OILGAS_SQLITE_CONNECTOR")!));
            handler.ScanAssembly(typeof(System.Data.SQLite.SQLiteConnection).Assembly);
            var source = editor.GetDataSource(connection.ConnectionName);
            Assert.NotNull(source); Assert.Equal(ConnectionState.Open, source.Openconnection());
            var metadata = new Mock<IPPDMMetadataRepository>();
            var types = new (Type Type, string Key)[] { (typeof(HSE_CA_VERIFICATION), "HSE_CA_VERIFICATION_ID"), (typeof(HSE_INCIDENT_CLOSURE), "HSE_INCIDENT_CLOSURE_ID"), (typeof(PROCESS_DEFINITION), "PROCESS_DEFINITION_ID"), (typeof(PROCESS_INSTANCE), "PROCESS_INSTANCE_ID"),
                (typeof(PROCESS_STEP_INSTANCE), "PROCESS_STEP_INSTANCE_ID"), (typeof(PROCESS_HISTORY), "PROCESS_HISTORY_ID"), (typeof(PROCESS_APPROVAL), "PROCESS_APPROVAL_ID"), (typeof(CROSS_PERSONA_TASK), "CROSS_TASK_ID") };
            var model = new MigrationModel { Source = "FocusedTest", SourceId = "hse-lifecycle" };
            foreach (var (type, key) in types)
            {
                var fields = new ClassCreator(editor).ConvertToEntityStructure(type).Fields;
                model.Entities.Add(type.FullName!, new MigrationModelEntity { ClrTypeFullName = type.FullName!, TableName = type.Name,
                    Properties = fields.Select(field => new MigrationModelProperty { PropertyName = field.FieldName, ColumnName = field.FieldName,
                        FieldType = field.Fieldtype, IsPrimaryKey = field.FieldName == key, IsNullable = field.FieldName != key,
                        MaxLength = Math.Max(field.Size, field.Size1) > 0 ? Math.Max(field.Size, field.Size1) : null }).ToList() });
                metadata.Setup(x => x.GetTableMetadataAsync(type.Name)).ReturnsAsync(new PPDMTableMetadata { EntityTypeName = type.FullName!, TableName = type.Name, PrimaryKeyColumn = key });
                metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync([key]);
            }
            var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = "hse-workflow-test" };
            var plan = migration.BuildMigrationPlanForModel(model, detectRelationships: false); migration.ApproveMigrationPlan(plan, "test");
            var migrated = migration.ExecuteMigrationPlan(plan);
            Assert.True(migrated.Success, migrated.Message + " | " + errors.Message + " | " + string.Join(" | ", messages.TakeLast(20)));
            var definition = new PROCESS_DEFINITION { PROCESS_DEFINITION_ID = CompanyHseWorkflowStart.ProcessId, PROCESS_NAME = "Incident review", PROCESS_TYPE = "HSE", ENTITY_TYPE = CompanyHseWorkflowStart.EntityType,
                ACTIVE_IND = "Y", IS_ACTIVE = "Y", VERSION = "1", PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] { new ProcessStepDefinition { StepId = "review", StepName = "Review", SequenceNumber = 1, RequiredRoles = ["HSEOfficer"] } } }) };
            if (closure) definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
                new ProcessStepDefinition { StepId = "INC_CORRECTIVE_ACTIONS", StepName = "Corrective actions", StepType = "ACTION", SequenceNumber = 1, NextStepId = "INC_VERIFICATION", RequiredRoles = ["HSEOfficer"] },
                new ProcessStepDefinition { StepId = "INC_VERIFICATION", StepName = "Independent verification", StepType = "APPROVAL", RequiresApproval = true, SequenceNumber = 2, NextStepId = "INC_CLOSURE", RequiredRoles = ["HSEOfficer"] },
                new ProcessStepDefinition { StepId = "INC_CLOSURE", StepName = "Final closure", StepType = "APPROVAL", RequiresApproval = true, SequenceNumber = 3, NextStepId = "", RequiredRoles = ["HSEOfficer"] } } });
            new CommonColumnHandler().PrepareForInsert(definition, "actor");
            Assert.Equal(Errors.Ok, source.InsertEntity(nameof(PROCESS_DEFINITION), definition).Flag);
            var personas = new Mock<IRolePersonaReader>(); personas.Setup(x => x.GetActivePersonasAsync("HSEOfficer")).ReturnsAsync(["HSE"]);
            var service = new PPDMProcessService(editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object, connection.ConnectionName, NullLogger<PPDMProcessService>.Instance, personaReader: personas.Object);
            var reviewed = (await service.GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId))!;
            var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
            var original = new CompanyHseWorkflowStart(context, new(connection.ConnectionName, "binding"), Guid.NewGuid(), "incident", new string('A', 64), ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(reviewed)));
            Task Authority() => Task.CompletedTask;
            Task<string> Incident() => Task.FromResult(original.IncidentRevision);
            var guards = 0;
            Task RevokeDuringWrite()
            {
                if (++guards == 4) throw new UnauthorizedAccessException("Controlled authority revocation");
                return Task.CompletedTask;
            }
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.StartHseWorkflowAsync(original, RevokeDuringWrite, Incident));
            Assert.Equal(4, guards);
            Assert.Empty((await source.GetEntityAsync(nameof(PROCESS_INSTANCE), new List<AppFilter>())).Cast<object>());
            Assert.Empty((await source.GetEntityAsync(nameof(PROCESS_HISTORY), new List<AppFilter>())).Cast<object>());
            Assert.Empty((await source.GetEntityAsync(nameof(CROSS_PERSONA_TASK), new List<AppFilter>())).Cast<object>());
            var first = await service.StartHseWorkflowAsync(original, Authority, Incident); first.Require(original);
            var retry = await service.StartHseWorkflowAsync(original, Authority, Incident); retry.Require(original);
            Assert.Equal(first.InstanceId, retry.InstanceId); Assert.Equal(first.GraphRevision, retry.GraphRevision); Assert.Equal(first.TaskIds, retry.TaskIds);
            Assert.Single((await source.GetEntityAsync(nameof(PROCESS_INSTANCE), new List<AppFilter>())).Cast<object>());
            Assert.Single((await source.GetEntityAsync(nameof(PROCESS_HISTORY), new List<AppFilter>())).Cast<object>());
            Assert.Single((await source.GetEntityAsync(nameof(CROSS_PERSONA_TASK), new List<AppFilter>())).Cast<object>());
            if (closure)
            {
                var action = new CAVerificationAction(1, "action", "Valve tested", "COMPLETED", "operator", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "operator", "Valve inspected and tested");
                var actions = new CAVerificationReview(context, "incident", "plan", new string('C', 64), [action]);
                var evidence = new IncidentClosureReview(actions, correction ? [] : [new CAVerificationDecision(Guid.NewGuid(), actions, "VERIFIED", "Independent test confirmed the repair is effective.")]);
                var returnedForCorrection = false;
                Task Role(string role) { Assert.Equal("HSEOfficer", role); return Task.CompletedTask; }
                for (var i = 0; i < 3; i++)
                {
                    if (i == 1 && !returnedForCorrection)
                    {
                        actions = actions with { Context = context with { ActorUserId = "independent-reviewer" } };
                        evidence = new IncidentClosureReview(actions, [new CAVerificationDecision(Guid.NewGuid(), actions, "VERIFIED", "Independent test confirmed the repair is effective.")]);
                    }
                    if (correction && i == 1 && !returnedForCorrection)
                    {
                        var rejection = new CAVerificationDecision(Guid.NewGuid(), actions, "REJECTED", "Independent pressure test rejected the first valve repair.");
                        evidence = new(actions, [rejection]);
                        var rejected = await service.ReviewCorrectionReturnAsync(evidence, original.LifecycleDatabase, new string('D', 64), Role);
                        var correctionCommand = new CompanyHseCorrectionCommand(Guid.NewGuid(), rejected, "Replace the failed valve and pressure test the new corrective work.");
                        var returned = await service.ReturnForCorrectionAsync(correctionCommand, Role, () => Task.CompletedTask, () => Task.CompletedTask); returned.Require();
                        Assert.Equal(JsonSerializer.Serialize(returned), JsonSerializer.Serialize(await service.ReturnForCorrectionAsync(correctionCommand, Role, () => Task.CompletedTask, () => Task.CompletedTask)));
                        actions = actions with { Revision = new string('E', 64), Actions = [action, action with { StepSequence = 2, StepId = "replacement", CompletedAt = DateTime.UtcNow, CompletionEvidence = "Replacement valve pressure tested" }] };
                        evidence = new(actions, [rejection, new(Guid.NewGuid(), actions, "VERIFIED", "Independent test confirmed replacement valve operation.")]);
                        returnedForCorrection = true; i = -1; continue;
                    }
                    var review = await service.ReviewClosureStageAsync(evidence, original.LifecycleDatabase, new string('D', 64), Role);
                    var command = new CompanyHseClosureCommand(Guid.NewGuid(), review, "Reviewed and approved this exact assigned incident stage.");
                    var receipt = await service.CompleteClosureStageAsync(command, Role, () => Task.CompletedTask); receipt.Require(command);
                    Assert.Equal(JsonSerializer.Serialize(receipt), JsonSerializer.Serialize(await service.CompleteClosureStageAsync(command, Role, () => Task.CompletedTask)));
                }
                var completed = (await service.GetProcessInstanceAsync(first.InstanceId))!;
                Assert.Equal(ProcessStatus.COMPLETED, completed.Status);
                Assert.Equal("actor", completed.StartedBy);
                Assert.Equal("independent-reviewer", completed.CompletedBy);
                var approvals = (await source.GetEntityAsync(nameof(PROCESS_APPROVAL), new List<AppFilter>())).Cast<object>().ToArray();
                Assert.Equal(correction ? 3 : 2, approvals.Length);
                Assert.All(approvals, approval => Assert.Equal("independent-reviewer", approval.GetType().GetProperty(nameof(PROCESS_APPROVAL.APPROVED_BY))!.GetValue(approval)));
            }
        }
        finally
        {
            editor.GetDataSource(connection.ConnectionName)?.Closeconnection();
            System.Data.SQLite.SQLiteConnection.ClearAllPools();
            try { Directory.Delete(directory, true); } catch (IOException error) { Console.WriteLine("Temporary SQLite test directory remains locked: " + error.Message); }
        }
    }
}
#endif
