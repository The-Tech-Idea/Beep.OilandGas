using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

/// <summary>Read-only assignment readiness from a configured native workflow and current independent recipients.</summary>
public sealed record DevelopmentInvestmentRoutingOption(
    [property:JsonRequired]string ProcessId,[property:JsonRequired]string Name,[property:JsonRequired]string Version,
    [property:JsonRequired]string DefinitionRevision,[property:JsonRequired]IReadOnlyList<DevelopmentReviewRecipientStep> Stages,
    [property:JsonRequired]IReadOnlyList<string> Blockers)
{
    public bool Ready => Blockers is {Count:0} && Stages is {Count:>0} && Stages.All(stage=>stage?.Recipients is {Count:>0});
    public void Require(DevelopmentInvestmentReview source)
    {
        if(!LeaseSummaryValidator.Identifier(ProcessId) || string.IsNullOrWhiteSpace(Name) || Name.Length>256 || Name.Any(char.IsControl) ||
            !LeaseSummaryValidator.Identifier(Version) || !ProcessDefinitionRevision.IsValid(DefinitionRevision) || Stages is null || Stages.Count is <1 or >64 ||
            Stages.Any(stage=>stage is null) || Stages.Select(stage=>stage.StepId).Distinct(StringComparer.Ordinal).Count()!=Stages.Count ||
            Blockers is null || Blockers.Count>64 || Blockers.Any(text=>string.IsNullOrWhiteSpace(text) || text.Length>512))
            throw new InvalidOperationException("A complete configured investment workflow and its current assignment evidence are required.");
        foreach(var stage in Stages)
        {
            if(!LeaseSummaryValidator.Identifier(stage.StepId) || !LeaseSummaryValidator.Identifier(stage.AssignedRole) || !ProcessDefinitionRevision.IsValid(stage.AuthorityRevision) ||
                stage.Recipients is null || stage.Recipients.Count>256 || stage.Recipients.Any(recipient=>recipient is null ||
                    !LeaseSummaryValidator.Identifier(recipient.UserId) || !LeaseSummaryValidator.Identifier(recipient.PersonaCode) || recipient.PersonaCode.Length>64 ||
                    recipient.UserId==source.Afe().CreatedBy || recipient.UserId==source.Current.Source.Review.Scope.ActorUserId) ||
                stage.Recipients.Select(recipient=>recipient.UserId).Distinct(StringComparer.Ordinal).Count()!=stage.Recipients.Count ||
                stage.Recipients.Count==0 && Blockers.Count==0)
                throw new InvalidOperationException("Every investment stage needs unambiguous independent current company recipients or an explicit blocker.");
        }
    }
}

public sealed record DevelopmentInvestmentRoutingReview(
    [property:JsonRequired]DevelopmentInvestmentReview Source,
    [property:JsonRequired]IReadOnlyList<DevelopmentInvestmentRoutingOption> Options,
    [property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public string? ExistingHandoffId{get;init;}
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{
        Source=Source.Current.Revision,Source.AfeId,Source.Recorded,Source.Blockers,Options,ExistingHandoffId}));
    public void Require(CompanyScope company,string actor,string processId,string afeId)
    {
        if(Source is null || Options is null || Options.Count>128 || Options.Any(option=>option is null) ||
            Options.Select(option=>option.ProcessId).Distinct(StringComparer.Ordinal).Count()!=Options.Count || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The exact source and complete current investment routing review are required.");
        Source.Require();var scope=Source.Current.Source.Review.Scope;
        if(ExistingHandoffId is not null && (!LeaseSummaryValidator.Identifier(ExistingHandoffId) || !ExistingHandoffId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal)))throw new InvalidOperationException("The recorded investment handoff reference is invalid.");
        if(Options.Sum(option=>option.Stages?.Count??0)>256)throw new InvalidOperationException("The complete investment routing catalog exceeds its supported stage bound.");
        if(scope.Company!=company || scope.ActorUserId!=actor || Source.Current.Source.Review.InstanceId!=processId || Source.AfeId!=afeId || Revision!=ContentRevision() ||
            Source.BlocksRouting() && Options.Count>0)
            throw new InvalidOperationException("The investment routing review belongs to another source/account/company or already decided request.");
        foreach(var option in Options)option.Require(Source);
        if(JsonSerializer.Serialize(this).Length>2*1024*1024)throw new InvalidOperationException("The investment routing review exceeds its supported bound.");
    }
}
