using System.Runtime.CompilerServices;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Development;
using Beep.OilandGas.LifeCycle.Services.Development.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Development;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// Submitting an FDP keeps its draft with the gate review's drafting step, where the plan is read back. It had started the
/// gate review only (and looked the gate up among DEVELOPMENT processes, where it is not), so the draft was thrown away.
/// </summary>
public sealed class FdpDraftStorageTests
{
    private const string FieldId = "FIELD-F-1";
    private const string UserId = "user-1";

    [Fact]
    public async Task SubmittedDraft_IsKeptWithTheDraftingStep_AndReadBack()
    {
        PROCESS_STEP_DATA? stored = null;
        var process = GateProcess();
        process.Setup(p => p.ExecuteStepAsync("fdp-1", "FDP_DRAFT", It.IsAny<PROCESS_STEP_DATA>(), UserId))
            .Callback<string, string, PROCESS_STEP_DATA, string>((_, _, data, _) => stored = data)
            .ReturnsAsync(true);
        // The store keeps step data as JSON (STEP_DATA_JSON); the read gives it back the way the store does.
        process.Setup(p => p.GetProcessInstanceAsync("fdp-1")).ReturnsAsync(() => Instance(
            JsonSerializer.Deserialize<PROCESS_STEP_DATA>(JsonSerializer.Serialize(stored))!));

        var draft = new SubmitFdpDraftRequest
        {
            StoiipMmbo = 820, ReservesClass = "2P — Proved+Probable", SelectedConcept = "FPSO tie-back",
            Npv10Mm = 410.5, Jurisdiction = "Regulator", RecoveryFactor = null,
            WellProgram = [new FdpWellProgramItem { WellName = "P-1", WellType = "Producer", PlannedYear = null }]
        };

        var instance = await CreateService(process.Object).SubmitFdpDraftAsync(FieldId, draft, UserId);
        var readBack = DevelopmentProcessService.StoredFdpDraft(instance);

        Assert.NotNull(readBack);
        Assert.Equal(820, readBack.StoiipMmbo);
        Assert.Null(readBack.RecoveryFactor);
        Assert.Equal("FPSO tie-back", readBack.SelectedConcept);
        Assert.Equal(410.5, readBack.Npv10Mm);
        Assert.Equal("P-1", Assert.Single(readBack.WellProgram).WellName);
        process.Verify(p => p.CancelProcessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ADraftThatCannotBeKept_WithdrawsTheGateReview_AndFails()
    {
        var process = GateProcess();
        var failure = new InvalidOperationException("store unavailable");
        process.Setup(p => p.ExecuteStepAsync("fdp-1", "FDP_DRAFT", It.IsAny<PROCESS_STEP_DATA>(), UserId)).ThrowsAsync(failure);
        process.Setup(p => p.CancelProcessAsync("fdp-1", It.IsAny<string>(), UserId)).ReturnsAsync(true);

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService(process.Object).SubmitFdpDraftAsync(FieldId, new SubmitFdpDraftRequest(), UserId));

        Assert.Same(failure, thrown);
        process.Verify(p => p.CancelProcessAsync("fdp-1", It.IsAny<string>(), UserId), Times.Once);
    }

    [Fact]
    public void AGateReviewWithNoKeptDraft_ReadsAsNone()
    {
        Assert.Null(DevelopmentProcessService.StoredFdpDraft(Instance(new PROCESS_STEP_DATA())));
    }

    private static Mock<IProcessService> GateProcess()
    {
        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessDefinitionAsync("GATE_FDP_REVIEW"))
            .ReturnsAsync(new ProcessDefinition { ProcessId = "GATE_FDP_REVIEW", ProcessType = "GATE_REVIEW" });
        process.Setup(p => p.StartProcessAsync("GATE_FDP_REVIEW", FieldId, "FIELD", FieldId, UserId))
            .ReturnsAsync(Instance(new PROCESS_STEP_DATA()));
        return process;
    }

    private static ProcessInstance Instance(PROCESS_STEP_DATA draftStepData) => new()
    {
        InstanceId = "fdp-1", ProcessId = "GATE_FDP_REVIEW", EntityId = FieldId, EntityType = "FIELD", FieldId = FieldId,
        Status = ProcessStatus.IN_PROGRESS,
        StepInstances = [new ProcessStepInstance { StepId = "FDP_DRAFT", SequenceNumber = 1, StepData = draftStepData }]
    };

    private static DevelopmentProcessService CreateService(IProcessService process) =>
        new(process, (PPDMDevelopmentService)RuntimeHelpers.GetUninitializedObject(typeof(PPDMDevelopmentService)),
            NullLogger<DevelopmentProcessService>.Instance);
}
