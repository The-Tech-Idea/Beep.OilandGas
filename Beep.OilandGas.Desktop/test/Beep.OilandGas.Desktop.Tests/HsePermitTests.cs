using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-07: the legal transitions (from the owner's own rules), the explicit jurisdiction, the foreign-asset refusal, the
/// decision's rationale and conditions, the segregation of duties, the rendered document and the disabled PDF export - the
/// parts that need no database. The inbox, the writes and the append-only history are proven in the gateway test.
/// </summary>
public sealed class HsePermitTests
{
    private const string PageFileName = "PermitsPageView.cs";

    private static readonly OgHseScope Scope = new("SMOKE", "F-1");

    [Fact]
    public void Only_the_owners_legal_transitions_are_offered()
    {
        // AC-1: the list comes from PermitStatusTransitionRules, not from a second table written here.
        Assert.Equal(
            [PermitApplicationStatusCodes.Submitted, PermitApplicationStatusCodes.Withdrawn],
            PermitRules.NextStates(PermitApplicationStatusCodes.Draft).OrderBy(state => state, StringComparer.Ordinal));

        Assert.Contains(PermitApplicationStatusCodes.UnderReview,
            PermitRules.NextStates(PermitApplicationStatusCodes.Submitted));
        Assert.Contains(PermitApplicationStatusCodes.Approved,
            PermitRules.NextStates(PermitApplicationStatusCodes.UnderReview));

        // A withdrawn application is terminal, and an approval cannot go back to review.
        Assert.Empty(PermitRules.NextStates(PermitApplicationStatusCodes.Withdrawn));
        Assert.DoesNotContain(PermitApplicationStatusCodes.UnderReview,
            PermitRules.NextStates(PermitApplicationStatusCodes.Approved));
        Assert.DoesNotContain(PermitApplicationStatusCodes.Draft,
            PermitRules.NextStates(PermitApplicationStatusCodes.Submitted));

        // The refusal names what is allowed rather than only what is not.
        var illegal = PermitRules.CheckTransition(PermitApplicationStatusCodes.Draft, PermitApplicationStatusCodes.Approved);

        Assert.Equal(PermitRules.CodeTransitionIllegal, illegal!.Value.Code);
        Assert.Contains(PermitApplicationStatusCodes.Submitted, illegal.Value.Message, StringComparison.Ordinal);

        Assert.Null(PermitRules.CheckTransition(PermitApplicationStatusCodes.Draft, PermitApplicationStatusCodes.Submitted));
    }

    [Fact]
    public void A_new_application_needs_an_explicit_jurisdiction()
    {
        // AC-2: no country is assumed.
        Assert.Null(PermitRules.ValidateCreate(Inputs(), Scope));

        var noCountry = PermitRules.ValidateCreate(Inputs() with { Country = "  " }, Scope);

        Assert.Equal(PermitRules.CodeJurisdictionRequired, noCountry!.Value.Code);
        Assert.Contains("None is assumed", noCountry.Value.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("UnitedStates", noCountry.Value.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("USA", noCountry.Value.Message, StringComparison.OrdinalIgnoreCase);

        var unknownCountry = PermitRules.ValidateCreate(Inputs() with { Country = "Atlantis" }, Scope);

        Assert.Equal(PermitRules.CodeJurisdictionRequired, unknownCountry!.Value.Code);

        // The type is the owner's enum, checked rather than guessed.
        Assert.Equal(PermitRules.CodeTypeInvalid,
            PermitRules.ValidateCreate(Inputs() with { Type = "TELEPORT" }, Scope)!.Value.Code);
        Assert.Equal(PermitRules.CodeInputRequired,
            PermitRules.ValidateCreate(Inputs() with { IntentId = "ab" }, Scope)!.Value.Code);
        Assert.Equal(PermitRules.CodeScopeRequired,
            PermitRules.ValidateCreate(Inputs() with { Scope = " " }, Scope)!.Value.Code);
    }

    [Fact]
    public void A_foreign_asset_is_refused()
    {
        // AC-3: the application is filed for this field's asset.
        Assert.Equal(PermitRules.CodeAssetRequired,
            PermitRules.ValidateCreate(Inputs() with { AssetId = null }, Scope)!.Value.Code);

        var foreign = PermitRules.ValidateCreate(Inputs() with { FieldId = "F-2" }, Scope);

        Assert.Equal(PermitRules.CodeForeignAsset, foreign!.Value.Code);
        Assert.Contains("F-2", foreign.Value.Message, StringComparison.Ordinal);
        Assert.Contains("F-1", foreign.Value.Message, StringComparison.Ordinal);

        // With no field selected at all, nothing can be shown to belong to it.
        Assert.Equal(PermitRules.CodeForeignAsset,
            PermitRules.ValidateCreate(Inputs(), new OgHseScope("SMOKE", null))!.Value.Code);
    }

    [Fact]
    public void A_decision_needs_its_rationale_and_an_approval_needs_its_conditions()
    {
        // AC-4.
        Assert.Null(PermitRules.ValidateDecision(PermitApplicationStatusCodes.Submitted, "Filed with the regulator.", null));

        Assert.Equal(PermitRules.CodeRationaleRequired,
            PermitRules.ValidateDecision(PermitApplicationStatusCodes.Approved, " ", "Monitor pressure")!.Value.Code);

        Assert.Equal(PermitRules.CodeConditionsRequired,
            PermitRules.ValidateDecision(PermitApplicationStatusCodes.Approved, "Approved by the regulator.", null)!.Value.Code);

        // A rejection needs its rationale but authorises nothing, so no conditions are required.
        Assert.Null(PermitRules.ValidateDecision(PermitApplicationStatusCodes.Rejected, "Incomplete application.", null));
    }

    [Fact]
    public void The_applicant_cannot_decide_their_own_application()
    {
        // AC-4's segregation of duties, for the decision states only.
        var selfDecision = PermitRules.CheckSeparateDuties("sara", "sara", PermitApplicationStatusCodes.Approved);

        Assert.Equal(PermitRules.CodeSelfDecision, selfDecision!.Value.Code);
        Assert.Contains("sara", selfDecision.Value.Message, StringComparison.Ordinal);
        Assert.Contains("cannot decide", selfDecision.Value.Message, StringComparison.Ordinal);

        Assert.Null(PermitRules.CheckSeparateDuties("sara", "nadia", PermitApplicationStatusCodes.Approved));

        // A rejection is a decision too, so it is blocked for the applicant as well.
        Assert.Equal(PermitRules.CodeSelfDecision,
            PermitRules.CheckSeparateDuties("sara", "sara", PermitApplicationStatusCodes.Rejected)!.Value.Code);

        // Submitting your own draft is not a decision, so it is not blocked.
        Assert.Null(PermitRules.CheckSeparateDuties("sara", "sara", PermitApplicationStatusCodes.Submitted));
    }

    [Fact]
    public void The_document_is_the_records_own_content()
    {
        // AC-5: the text a PDF would render is built from the record, so the two cannot disagree.
        var application = Application(PermitApplicationStatusCodes.Approved);
        var document = application.Document();

        Assert.Contains("Reference: REF-1", document, StringComparison.Ordinal);
        Assert.Contains("Type: Drilling", document, StringComparison.Ordinal);
        Assert.Contains("Jurisdiction: Canada / Alberta", document, StringComparison.Ordinal);
        Assert.Contains("Asset: WELL-1", document, StringComparison.Ordinal);
        Assert.Contains("Field: F-1", document, StringComparison.Ordinal);
        Assert.Contains("State: approved", document, StringComparison.Ordinal);
        Assert.Contains("Applicant: sara", document, StringComparison.Ordinal);
        Assert.Contains("Decision: Approved by the regulator. · conditions: Monitor pressure", document,
            StringComparison.Ordinal);
        Assert.Contains("Scope: Drill the 12-1 well", document, StringComparison.Ordinal);
        Assert.Contains("STATUS HISTORY", document, StringComparison.Ordinal);
        Assert.Contains("submitted", document, StringComparison.Ordinal);
        Assert.Contains("approved", document, StringComparison.Ordinal);

        // AC-5's other half: the renderer is unavailable, and the command says so instead of producing a file.
        var permits = new OgPermits(() => null, new Quiet());
        var rendered = permits.RenderPdf("PERMIT-x");

        Assert.False(rendered.Success);
        Assert.Equal(PermitRules.CodeRendererUnavailable, rendered.Reference);
        Assert.Contains("PDF export is disabled", rendered.Message!, StringComparison.Ordinal);
        Assert.Contains("wkhtmltopdf", rendered.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_history_is_what_the_state_is_read_from()
    {
        // AC-6: the latest entry is the state, and the entries are the record.
        var application = Application(PermitApplicationStatusCodes.UnderReview);

        Assert.Equal(3, application.History.Count);
        Assert.Equal(PermitApplicationStatusCodes.UnderReview, application.State);
        Assert.Equal(PermitApplicationStatusCodes.Draft, application.History[0].State);
        Assert.Contains("nadia", application.History[^1].Describe(), StringComparison.Ordinal);
        Assert.Contains("under_review", application.History[^1].Describe(), StringComparison.Ordinal);

        // Every state has a member, including the key whose name differs from its member.
        Assert.Equal(PermitApplicationStatus.AdditionalInformationRequired,
            PermitRules.MemberFor(PermitApplicationStatusCodes.AdditionalInformationRequired));
        Assert.Equal(PermitApplicationStatus.UnderReview, PermitRules.MemberFor("UnderReview"));
        Assert.Null(PermitRules.MemberFor("TELEPORT"));
    }

    [Fact]
    public void The_page_offers_only_the_legal_states_and_shows_the_renderer_reason()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-1: the picker is bound from the application's legal set.
        Assert.Contains("application.LegalNextStates", page, StringComparison.Ordinal);

        // AC-5: the reason is on the page and the document is the record's own text.
        Assert.Contains("RendererNotice", page, StringComparison.Ordinal);
        Assert.Contains("application.Document()", page, StringComparison.Ordinal);

        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex("\\b(USA|United States)\\b"), page);
    }

    private static OgPermitInputs Inputs() =>
        new(
            "permit-abc",
            PermitApplicationType.Drilling.ToString(),
            Country.Canada.ToString(),
            null,
            "WELL",
            "WELL-1",
            "F-1",
            "Drill the 12-1 well");

    private static OgPermitApplication Application(string state) =>
        new(
            "PERMIT-1",
            "REF-1",
            PermitApplicationType.Drilling.ToString(),
            "Canada / Alberta",
            "WELL-1",
            "F-1",
            "Drill the 12-1 well",
            state,
            new DateTime(2026, 11, 1),
            new DateTime(2026, 11, 5),
            "Approved by the regulator. · conditions: Monitor pressure",
            "sara",
            [
                new OgPermitHistoryEntry(new DateTime(2026, 10, 30), PermitApplicationStatusCodes.Draft,
                    "Application filed.", "sara"),
                new OgPermitHistoryEntry(new DateTime(2026, 11, 1), PermitApplicationStatusCodes.Submitted,
                    "Filed with the regulator.", "sara"),
                new OgPermitHistoryEntry(new DateTime(2026, 11, 3), PermitApplicationStatusCodes.UnderReview,
                    "The regulator is reviewing it.", "nadia"),
            ]);

    private sealed class Quiet : TheTechIdeaWeb.Diagnostics.IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            TheTechIdeaWeb.Diagnostics.FailureSeverity severity = TheTechIdeaWeb.Diagnostics.FailureSeverity.Error) =>
            TheTechIdeaWeb.Diagnostics.FailureReferenceFormat.New();
    }
}
