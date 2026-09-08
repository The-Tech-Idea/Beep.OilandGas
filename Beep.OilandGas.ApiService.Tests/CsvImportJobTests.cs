using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.PPDM39;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;
using FileImportResult = Beep.OilandGas.PPDM39.Core.FileImportResult;
using FileImportError = Beep.OilandGas.PPDM39.Core.FileImportError;

namespace Beep.OilandGas.ApiService.Tests;

public class CsvImportJobTests
{
    [Theory]
    [InlineData(BackgroundOperationState.Failed)]
    [InlineData(BackgroundOperationState.Cancelled)]
    public async Task ProgressPollingReconcilesJobsThatNeverReachTheRunner(BackgroundOperationState state)
    {
        var sent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var client = new Mock<Microsoft.AspNetCore.SignalR.IClientProxy>();
        client.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(() => { sent.TrySetResult(); return Task.CompletedTask; });
        var hub = new Mock<Microsoft.AspNetCore.SignalR.IHubContext<ProgressHub>> { DefaultValue = DefaultValue.Mock };
        hub.Setup(h => h.Clients.Group(It.IsAny<string>())).Returns(client.Object);
        var queue = new Mock<IBackgroundOperationQueue>();
        using var service = new ProgressTrackingService(hub.Object, NullLogger<ProgressTrackingService>.Instance, queue.Object);
        var id = service.StartOperation("ImportCsv", "queued");
        await service.StartAsync(default);
        await sent.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await service.StopAsync(default);
        queue.Setup(q => q.GetStatus(CsvImportJob.QueueKey(id))).Returns(new BackgroundOperationStatus(CsvImportJob.QueueKey(id), state));
        var status = service.GetProgress(id)!;
        Assert.True(status.IsComplete);
        Assert.True(status.HasError);
        Assert.Contains(state.ToString(), status.StatusMessage);
    }

    [Fact]
    public async Task ActualUploadSizeIsCheckedEvenWhenDeclaredLengthIsWrong()
    {
        var queue = new Mock<IBackgroundOperationQueue>(MockBehavior.Strict);
        using var stream = new MemoryStream(new byte[CsvImportJob.MaxUploadBytes + 1]);
        var file = new Mock<IFormFile>();
        file.SetupGet(f => f.Length).Returns(1);
        file.Setup(f => f.OpenReadStream()).Returns(stream);
        var result = await Controller(queue.Object, Mock.Of<IProgressTrackingService>()).ImportCsv("WELL", file.Object);
        Assert.Equal(413, Assert.IsType<ObjectResult>(result.Result).StatusCode);
        queue.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task WorkerOwnsTemporaryFileAndChecksErrors(int outcome)
    {
        var progress = new Mock<IProgressTrackingService>();
        var executor = new Mock<ICsvImportExecutor>();
        string? tempPath = null;
        var job = new CsvImportJob("id", "WELL", "selected", "actor", true, new byte[] { 1, 2, 3 });
        executor.Setup(e => e.ExecuteAsync(job, It.IsAny<string>())).Returns(async (CsvImportJob _, string path) =>
        {
            tempPath = path;
            Assert.Equal(job.Content, await File.ReadAllBytesAsync(path));
            if (outcome == 2) throw new InvalidOperationException("provider detail");
            return new FileImportResult { SuccessCount = 1, Errors = outcome == 1 ? new() { new FileImportError() } : new() };
        });
        var worker = new CsvImportJobRunner(executor.Object, progress.Object);
        if (outcome == 0) await worker.RunAsync(job, default);
        else await Assert.ThrowsAsync<InvalidOperationException>(() => worker.RunAsync(job, default));
        Assert.NotNull(tempPath);
        Assert.False(File.Exists(tempPath));
        progress.Verify(p => p.CompleteOperation("id", outcome == 0, It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task CancelledWorkerNeverInvokesRepository()
    {
        var executor = new Mock<ICsvImportExecutor>(MockBehavior.Strict);
        var worker = new CsvImportJobRunner(executor.Object, Mock.Of<IProgressTrackingService>());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => worker.RunAsync(
            new("id", "WELL", "db", "actor", true, new byte[] { 1 }), new CancellationToken(true)));
        executor.VerifyNoOtherCalls();
    }

    private static PPDM39ImportExportController Controller(IBackgroundOperationQueue queue, IProgressTrackingService progress, bool actor = true) => new(
        Mock.Of<IDMEEditor>(), Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
        Mock.Of<IPPDMMetadataRepository>(), NullLogger<PPDM39ImportExportController>.Instance,
        NullLoggerFactory.Instance, progress, queue)
    {
        ControllerContext = new() { HttpContext = new DefaultHttpContext
        { User = new ClaimsPrincipal(new ClaimsIdentity(actor ? new[] { new Claim("sub", "real-actor") } : Array.Empty<Claim>(), "test")) } }
    };

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AdmissionCapturesOwnedBytesAndAuthenticatedActor(bool accepted)
    {
        CsvImportJob? received = null;
        var queue = new Mock<IBackgroundOperationQueue>();
        queue.Setup(q => q.TryEnqueue<CsvImportJobRunner, CsvImportJob>("csv-import:server-id", It.IsAny<CsvImportJob>(),
            It.IsAny<Func<CsvImportJobRunner, CsvImportJob, CancellationToken, Task>>()))
            .Callback<string, CsvImportJob, Func<CsvImportJobRunner, CsvImportJob, CancellationToken, Task>>((_, job, _) => received = job)
            .Returns(accepted);
        var progress = new Mock<IProgressTrackingService>();
        progress.Setup(p => p.StartOperation("ImportCsv", It.IsAny<string>())).Returns("server-id");
        ActionResult<OperationStartResponse> result;
        using (var upload = new MemoryStream(new byte[] { 1, 2, 3 }))
        {
            var file = new FormFile(upload, 0, 3, "file", "../../untrusted.csv");
            result = await Controller(queue.Object, progress.Object).ImportCsv("WELL", file, userId: "spoofed", connectionName: "selected", validateForeignKeys: false);
        }
        Assert.Equal(accepted ? 200 : 503, Assert.IsAssignableFrom<ObjectResult>(result.Result).StatusCode);
        Assert.Equal("real-actor", received!.UserId);
        Assert.Equal("selected", received.ConnectionName);
        Assert.False(received.ValidateForeignKeys);
        Assert.Equal(new byte[] { 1, 2, 3 }, received.Content);
        if (!accepted) progress.Verify(p => p.CompleteOperation("server-id", false, null, It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData("actor", 401)]
    [InlineData("operation", 400)]
    [InlineData("size", 413)]
    [InlineData("table", 400)]
    public async Task InvalidRequestsNeverQueue(string scenario, int expected)
    {
        var queue = new Mock<IBackgroundOperationQueue>(MockBehavior.Strict);
        var file = new Mock<IFormFile>();
        file.SetupGet(f => f.Length).Returns(scenario == "size" ? CsvImportJob.MaxUploadBytes + 1 : 1);
        var result = await Controller(queue.Object, Mock.Of<IProgressTrackingService>(), scenario != "actor")
            .ImportCsv(scenario == "table" ? "NOT_A_PPDM_TABLE" : "WELL", file.Object, operationId: scenario == "operation" ? "caller-id" : null);
        var code = result.Result is StatusCodeResult status ? status.StatusCode : ((ObjectResult)result.Result!).StatusCode;
        Assert.Equal(expected, code);
        queue.VerifyNoOtherCalls();
    }
}
