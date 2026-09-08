using Beep.OilandGas.ApiService.Controllers.PPDM39;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class UnavailableExecutionTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("plan-hash", "")]
    [InlineData("", "manifest-hash")]
    public async Task ExecutionCannotOmitReviewedHashes(string planHash, string manifestHash)
    {
        var service = new PPDM39SetupService(_editor.Object, NullLogger<PPDM39SetupService>.Instance,
            _columns.Object, _defaults.Object, _metadata.Object);
        var request = new SchemaMigrationExecuteRequest { PlanId = "unreviewed", ExpectedPlanHash = planHash,
            ExpectedManifestHash = manifestHash };
        var direct = await service.ExecuteSchemaMigrationPlanAsync(request);
        var background = await service.StartSchemaMigrationExecutionAsync(request);
        Assert.False(direct.Success);
        Assert.False(background.Success);
        Assert.Equal("Reviewed plan and manifest hashes are required.", direct.Message);
        Assert.Equal(direct.Message, background.Message);
        _editor.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("Beep.OilandGas.Models", "Beep.OilandGas.Models.Data.Security")]
    [InlineData("Beep.OilandGas.UserManagement", "Beep.OilandGas.UserManagement.Models.Identity")]
    public async Task AssemblyMigrationCannotCreateLegacyIdentityInModuleDatabase(string assembly, string modelNamespace)
    {
        var service = new PPDM39SetupService(_editor.Object, NullLogger<PPDM39SetupService>.Instance,
            _columns.Object, _defaults.Object, _metadata.Object);
        var result = await service.PlanSchemaMigrationAsync(new SchemaMigrationPlanRequest
        {
            ConnectionName = "module-db", TargetAssemblyName = assembly, TargetModelNamespace = modelNamespace
        });
        Assert.False(result.Success);
        Assert.Equal("Schema migration plan failed.", result.Message);
        _editor.VerifyNoOtherCalls();
    }

    private readonly Mock<IDMEEditor> _editor = new(MockBehavior.Strict);
    private readonly Mock<ICommonColumnHandler> _columns = new(MockBehavior.Strict);
    private readonly Mock<IPPDM39DefaultsRepository> _defaults = new(MockBehavior.Strict);
    private readonly Mock<IPPDMMetadataRepository> _metadata = new(MockBehavior.Strict);
    private readonly Mock<IProgressTrackingService> _progress = new(MockBehavior.Strict);

    private PPDM39WorkflowService WorkflowService() => new(_editor.Object, _columns.Object, _defaults.Object,
        _metadata.Object, NullLogger<PPDM39WorkflowService>.Instance, _progress.Object);

    [Fact]
    public void SetupActionsCannotBypassAdministratorAuthorization()
    {
        Assert.All(typeof(PPDM39SetupController).GetMethods(), method =>
            Assert.Empty(method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true)));
    }

    [Theory]
    [InlineData("Prodution")]
    [InlineData("")]
    [InlineData("99")]
    [InlineData("0")]
    public async Task InvalidMigrationEnvironmentIsRejectedBeforeDatabaseAccess(string environment)
    {
        var service = new PPDM39SetupService(_editor.Object, NullLogger<PPDM39SetupService>.Instance,
            _columns.Object, _defaults.Object, _metadata.Object);
        var result = await service.PlanSchemaMigrationAsync(new SchemaMigrationPlanRequest
        { ConnectionName = "module-db", EnvironmentTier = environment });
        Assert.False(result.Success);
        _editor.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MigrationActorsComeFromLocalPrincipal(bool authenticatedLocally)
    {
        var service = new PPDM39SetupService(_editor.Object, NullLogger<PPDM39SetupService>.Instance,
            _columns.Object, _defaults.Object, _metadata.Object);
        var migration = new Mock<IPPDM39SchemaMigrationService>(MockBehavior.Strict);
        if (authenticatedLocally)
        {
            migration.Setup(x => x.ApproveSchemaMigrationPlanAsync(It.Is<SchemaMigrationApprovalRequest>(r => r.ApprovedBy == "local-admin")))
                .ReturnsAsync(new SchemaMigrationApprovalResult { Success = true });
            migration.Setup(x => x.ExecuteSchemaMigrationPlanAsync(It.Is<SchemaMigrationExecuteRequest>(r => r.ExecutedBy == "local-admin")))
                .ReturnsAsync(new SchemaMigrationExecuteResult { Success = true });
            migration.Setup(x => x.StartSchemaMigrationExecutionAsync(It.Is<SchemaMigrationExecuteRequest>(r => r.ExecutedBy == "local-admin")))
                .ReturnsAsync(new OperationStartResponse { Success = true });
        }
        var controller = new PPDM39SetupController(service, migration.Object, _editor.Object,
            NullLogger<PPDM39SetupController>.Instance,
            commonColumnHandler: _columns.Object, defaults: _defaults.Object, metadata: _metadata.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };
        if (authenticatedLocally)
            controller.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "local-admin")], "repository"));
        var approval = await controller.ApproveSchemaMigration(new() { ApprovedBy = "spoofed" });
        var execution = await controller.ExecuteSchemaMigration(new() { ExecutedBy = "spoofed" });
        var start = await controller.StartSchemaMigration(new() { ExecutedBy = "spoofed" });
        if (authenticatedLocally)
        {
            Assert.IsType<OkObjectResult>(approval.Result);
            Assert.IsType<OkObjectResult>(execution.Result);
            Assert.IsType<OkObjectResult>(start.Result);
            migration.VerifyAll();
        }
        else
        {
            Assert.IsType<ForbidResult>(approval.Result);
            Assert.IsType<ForbidResult>(execution.Result);
            Assert.IsType<ForbidResult>(start.Result);
            migration.VerifyNoOtherCalls();
        }
    }

    [Fact]
    public async Task ModuleDatabaseCreationRejectsLegacySecurityBeforeDatabaseAccess()
    {
        var authorization = Assert.Single(typeof(PPDM39SetupController)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
        Assert.Equal("Administrator", authorization.Roles);
        Assert.False(new DatabaseCreationOptions().SeedDefaultSecurityData);
        var service = new PPDM39SetupService(_editor.Object, NullLogger<PPDM39SetupService>.Instance,
            _columns.Object, _defaults.Object, _metadata.Object);
        var controller = new PPDM39SetupController(service, service, _editor.Object,
            NullLogger<PPDM39SetupController>.Instance,
            commonColumnHandler: _columns.Object, defaults: _defaults.Object, metadata: _metadata.Object);
        var result = await controller.CreateDatabase(new CreateDatabaseRequest
        {
            Connection = new TheTechIdea.Beep.ConfigUtil.ConnectionProperties(),
            Options = new DatabaseCreationOptions { SeedDefaultSecurityData = true }
        });
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _editor.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("importcsv")]
    [InlineData("validate")]
    [InlineData("qualitycheck")]
    [InlineData("version")]
    [InlineData("unknown")]
    [InlineData(null)]
    public async Task WorkflowServiceNeverReportsPlaceholderSuccess(string? operation)
    {
        var workflow = new WorkflowDefinition
        {
            WorkflowId = "definition-id",
            Steps = operation == null ? new() : new() { new WorkflowStep
            { StepId = "step", OperationType = operation, DependsOn = "missing" } }
        };
        var result = await WorkflowService().ExecuteWorkflowAsync(workflow, "operation-id");
        Assert.False(result.Success);
        Assert.Equal("definition-id", result.WorkflowId);
        Assert.Equal("operation-id", result.OperationId);
        Assert.Equal(PPDM39WorkflowService.ExecutionUnavailableMessage, result.ErrorMessage);
        Assert.Empty(result.StepResults);
        _editor.VerifyNoOtherCalls();
        _metadata.VerifyNoOtherCalls();
        _progress.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WorkflowEndpointRejectsWithoutCreatingProgressOrChangingContext(bool valid)
    {
        var field = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        var controller = new PPDM39WorkflowController(WorkflowService(),
            NullLogger<PPDM39WorkflowController>.Instance, _progress.Object, field.Object);
        var request = new WorkflowExecutionRequest { Workflow = valid ? new WorkflowDefinition() : null! };
        var result = await controller.ExecuteWorkflow(request, "production");
        Assert.Equal(valid ? 501 : 400, Assert.IsAssignableFrom<ObjectResult>(result.Result).StatusCode);
        if (valid)
        {
            var response = Assert.IsType<OperationStartResponse>(((ObjectResult)result.Result!).Value);
            Assert.Empty(response.OperationId);
            Assert.Equal(PPDM39WorkflowService.ExecutionUnavailableMessage, response.Message);
        }
        _progress.VerifyNoOtherCalls();
        field.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("source", "target", 501)]
    [InlineData(" ", "target", 400)]
    [InlineData("source", "", 400)]
    public async Task CopyEndpointRejectsWithoutStartingAnOperation(string source, string target, int expected)
    {
        var service = new PPDM39SetupService(_editor.Object, NullLogger<PPDM39SetupService>.Instance,
            _columns.Object, _defaults.Object, _metadata.Object);
        var controller = new PPDM39SetupController(service, service, _editor.Object,
            NullLogger<PPDM39SetupController>.Instance, _progress.Object,
            commonColumnHandler: _columns.Object, defaults: _defaults.Object, metadata: _metadata.Object);
        var result = await controller.CopyDatabase(new CopyDatabaseRequest
        { SourceConnectionName = source, TargetConnectionName = target });
        Assert.Equal(expected, Assert.IsAssignableFrom<ObjectResult>(result.Result).StatusCode);
        if (expected == 501) Assert.False(Assert.IsType<CopyDatabaseResult>(((ObjectResult)result.Result!).Value).Success);
        _editor.VerifyNoOtherCalls();
        _progress.VerifyNoOtherCalls();
    }
}
