using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;

namespace Beep.OilandGas.DrillingAndConstruction.Models;

/// <summary>Exact persisted program and its currently reconciled original funding/well source.</summary>
public sealed record DrillingProgramReviewSource([property:JsonRequired]DrillingProgramBasis Basis,[property:JsonRequired]DRILLING_PROGRAM Program,
    [property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Basis=Basis.Revision,Program}));
    public void Require()
    {
        Basis.Require();if(Program is null || Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)throw new InvalidOperationException("The exact original program review source is unavailable.");
        var list=new DrillingProgramDraftList(Basis,[Program],"",CheckedAtUtc);list=list with{Revision=list.ContentRevision()};list.Require();
    }
}
public sealed record DrillingProgramReviewStage([property:JsonRequired]DevelopmentReviewRoleStep Definition,[property:JsonRequired]DevelopmentReviewRecipientStep Authority);
public sealed record DrillingProgramReviewOption([property:JsonRequired]string ProcessId,[property:JsonRequired]string ProcessName,[property:JsonRequired]string Version,
    [property:JsonRequired]string DefinitionRevision,[property:JsonRequired]IReadOnlyList<DrillingProgramReviewStage> Stages)
{
    [JsonIgnore]public bool Ready=>Stages.Count>0 && Stages.All(stage=>stage.Authority.Recipients.Count>0);
    public void Require(DrillingProgramReviewSource source)
    {
        if(!LeaseSummaryValidator.Identifier(ProcessId) || string.IsNullOrWhiteSpace(ProcessName) || ProcessName.Length>256 || ProcessName.Any(char.IsControl) || !LeaseSummaryValidator.Identifier(Version) ||
            !ProcessDefinitionRevision.IsValid(DefinitionRevision) || Stages is null || Stages.Count is <1 or >64 || Stages.Any(stage=>stage is null || stage.Definition is null || stage.Authority is null) ||
            Stages.Select(stage=>stage.Definition.StepId).Distinct(StringComparer.Ordinal).Count()!=Stages.Count || Stages.Select(stage=>stage.Definition.Sequence).Distinct().Count()!=Stages.Count ||
            !Stages.Select(stage=>stage.Definition.Sequence).SequenceEqual(Stages.Select(stage=>stage.Definition.Sequence).Order()))throw new InvalidOperationException("The configured program review stages are unavailable or ambiguous.");
        foreach(var stage in Stages)
        {
            var step=stage.Definition;var authority=stage.Authority;var recipients=authority.Recipients;
            if(!LeaseSummaryValidator.Identifier(step.StepId) || string.IsNullOrWhiteSpace(step.StepName) || step.StepName.Length>256 || step.StepName.Any(char.IsControl) || step.Sequence<0 ||
                !LeaseSummaryValidator.Identifier(step.AssignedRole) || step.SlaHours<0 || step.PersonaCodes is null || step.PersonaCodes.Count>128 || step.PersonaCodes.Any(code=>!LeaseSummaryValidator.Identifier(code) || code.Length>64) ||
                step.PersonaCodes.Distinct(StringComparer.Ordinal).Count()!=step.PersonaCodes.Count || authority.StepId!=step.StepId || authority.AssignedRole!=step.AssignedRole || !ProcessDefinitionRevision.IsValid(authority.AuthorityRevision) ||
                recipients is null || recipients.Count>256 || recipients.Any(row=>row is null || !LeaseSummaryValidator.Identifier(row.UserId) || !LeaseSummaryValidator.Identifier(row.PersonaCode) || row.PersonaCode.Length>64 ||
                    row.UserId==source.Program.ROW_CREATED_BY || row.UserId==source.Basis.Scope().ActorUserId) || recipients.Select(row=>row.UserId).Distinct(StringComparer.Ordinal).Count()!=recipients.Count ||
                !step.PersonaCodes.Order(StringComparer.Ordinal).SequenceEqual(recipients.Select(row=>row.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)))
                throw new InvalidOperationException("Each program stage must retain its exact independent role/workspace recipient authority.");
        }
        foreach(var role in Stages.GroupBy(stage=>stage.Definition.AssignedRole,StringComparer.Ordinal))
            if(role.Select(stage=>JsonSerializer.Serialize(stage.Authority with{StepId=""})).Distinct(StringComparer.Ordinal).Count()!=1)throw new InvalidOperationException("Repeated program-review roles must use the same current source-qualified authority.");
    }
}
/// <summary>Readiness only. No submitted process, approval, task or program authorization is implied.</summary>
public sealed record DrillingProgramReviewReadiness([property:JsonRequired]DrillingProgramReviewSource Source,[property:JsonRequired]IReadOnlyList<DrillingProgramReviewOption> Options,
    [property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    [JsonRequired]public string? ExistingReviewInstanceId{get;init;}
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Source=Source.Revision,Options,ExistingReviewInstanceId}));
    public void Require(CompanyScope company,string actor,string programId,string programRevision)
    {
        Source.Require();var scope=Source.Basis.Scope();if(scope.Company!=company || scope.ActorUserId!=actor || Source.Program.PROGRAM_ID!=programId || Source.Program.PROGRAM_DRAFT_REVISION!=programRevision ||
            ExistingReviewInstanceId is not null && (!LeaseSummaryValidator.Identifier(ExistingReviewInstanceId) || !ExistingReviewInstanceId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal) || Options?.Count!=0) || Options is null || Options.Count>128 || Options.Any(option=>option is null) || Options.Select(option=>option.ProcessId).Distinct(StringComparer.Ordinal).Count()!=Options.Count || Options.Sum(option=>option.Stages?.Count??0)>256 ||
            CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || Revision!=ContentRevision())throw new InvalidOperationException("The program routing review must match the current original company, actor, saved version and complete configured catalog.");
        foreach(var option in Options)option.Require(Source);
        if(JsonSerializer.SerializeToUtf8Bytes(this).Length>8*1024*1024)throw new InvalidOperationException("The complete program routing review exceeds its bounded payload.");
    }
}
