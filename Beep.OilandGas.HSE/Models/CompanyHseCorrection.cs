using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.HSE.Models;

/// <summary>The current correction round in the existing HSE process payload.</summary>
public sealed record CompanyHseCorrectionRound(Guid IntentId, int Number, CompanyScope Company, string InstanceId,
    string ActionStepId, string VerificationStepId, string ClosureStepId, string RejectedRevision)
{
    public const string PayloadKey = "CompanyHseCorrectionRound";
    public const int MaximumRounds = 4;
    public void Require()
    {
        if (Company is null || !LeaseSummaryValidator.Identifier(Company.TenantId) || !LeaseSummaryValidator.Identifier(Company.BusinessAssociateId) ||
            IntentId == Guid.Empty || Number is < 1 or > MaximumRounds || !ProcessDefinitionRevision.IsValid(RejectedRevision) ||
            new[] { InstanceId, ActionStepId, VerificationStepId, ClosureStepId }.Any(x => !LeaseSummaryValidator.Identifier(x)) ||
            new[] { ActionStepId, VerificationStepId, ClosureStepId }.Distinct(StringComparer.Ordinal).Count() != 3)
            throw new InvalidOperationException("A bounded incident correction round is required.");
    }
    public string? RoundFor(string step) => step == ActionStepId || step == VerificationStepId || step == ClosureStepId ? IntentId.ToString("N") : null;
    public static CompanyHseCorrectionRound? FromPayload(string json, string instance, string field, string incident)
    {
        using var document = JsonDocument.Parse(json);
        var data = document.RootElement.GetProperty("Data");
        if (!data.TryGetProperty(PayloadKey, out var value)) return null;
        if (data.EnumerateObject().Count(p => p.Name == PayloadKey) != 1) throw new InvalidOperationException("The incident correction round is ambiguous.");
        var round = JsonSerializer.Deserialize<CompanyHseCorrectionRound>(value.GetRawText(), new JsonSerializerOptions { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow })
            ?? throw new InvalidOperationException("The incident correction round is unavailable.");
        round.Require();
        var start = CompanyHseWorkflowStart.FromPayload(json, instance, field, incident);
        if (round.InstanceId != instance || round.Company != start.Context.Company) throw new InvalidOperationException("The incident correction round belongs to another workflow.");
        return round;
    }
}

public sealed record CompanyHseCorrectionCommand(Guid IntentId, CompanyHseClosureReview Review, string Reason)
{
    public void Require()
    {
        if (Review is null) throw new InvalidOperationException("A reviewed rejected verification is required.");
        Review.RequireCorrectionReview();
        if (IntentId == Guid.Empty || string.IsNullOrWhiteSpace(Reason) || Reason != Reason.Trim() || Reason.Length is < 20 or > 2000 ||
            Reason.Any(c => char.IsControl(c) && c is not ('\r' or '\n')) || JsonSerializer.Serialize(this).Length > 32768)
            throw new InvalidOperationException("An original identity and explicit correction reason are required.");
    }
}

public sealed record CompanyHseCorrectionReceipt(CompanyHseCorrectionCommand Original, CompanyHseCorrectionRound Round,
    string HistoryId, string ApprovalId, IReadOnlyList<string> CompletedTasks, IReadOnlyList<string> CorrectionTasks, string GraphRevision)
{
    public void Require()
    {
        if (Original is null || Round is null) throw new InvalidOperationException("The recorded correction return is required.");
        Original.Require(); Round.Require(); var r = Original.Review;
        if (Round.IntentId != Original.IntentId || Round.Company != r.Context.Company || Round.InstanceId != r.InstanceId ||
            Round.VerificationStepId != r.StepInstanceId || Round.RejectedRevision != r.Evidence.Actions.Revision ||
            !LeaseSummaryValidator.Identifier(HistoryId) || !LeaseSummaryValidator.Identifier(ApprovalId) || !ProcessDefinitionRevision.IsValid(GraphRevision) ||
            CompletedTasks is not { Count: > 0 and <= 128 } || CorrectionTasks is not { Count: > 0 and <= 128 } ||
            CompletedTasks.Concat(CorrectionTasks).Any(id => !LeaseSummaryValidator.Identifier(id)) ||
            CompletedTasks.Concat(CorrectionTasks).Distinct(StringComparer.Ordinal).Count() != CompletedTasks.Count + CorrectionTasks.Count)
            throw new InvalidOperationException("The correction return receipt differs from its original review.");
    }
}
