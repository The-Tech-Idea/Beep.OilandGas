using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;

namespace TheTechIdea.Data.OilGas;

/// <summary>Frozen native source, or an explicit CREATE_DRAFT proposal; neither grants authority or establishes a saved outcome.</summary>
public sealed record PermitApplicationSourceReview(
    [property: JsonRequired] PermitApplicationDataScope Context,
    [property: JsonRequired] string ApplicationJson,
    [property: JsonRequired] string HistoryJson,
    [property: JsonRequired] string Revision)
{
    [JsonRequired] public PermitApplicationSupportingSnapshot Supporting { get; init; } = PermitApplicationSupportingSnapshot.Empty;
    public static void RequireTarget(string applicationId, FieldSelectionReview expected)
    {
        if (!LeaseSummaryValidator.Identifier(applicationId) || expected is null || expected.Company is null ||
            !LeaseSummaryValidator.Identifier(expected.Company.TenantId) || !LeaseSummaryValidator.Identifier(expected.Company.BusinessAssociateId) ||
            !LeaseSummaryValidator.Identifier(expected.ActorUserId) || !LeaseSummaryValidator.Identifier(expected.FieldId))
            throw new InvalidOperationException("An exact permit and current company/account/FIELD selection are required.");
        FieldSelectionReviewValidator.RequireReview(expected, expected.Company, expected.ActorUserId);
    }
    public void RequireFor(string applicationId, FieldSelectionReview expected)
    {
        RequireTarget(applicationId, expected); Require();
        if (Context.Field != expected || ReadApplication().PERMIT_APPLICATION_ID != applicationId)
            throw new InvalidOperationException("The permit source review differs from the selected account/company/FIELD or application.");
    }
    private static readonly JsonSerializerOptions Json = new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
    public static PermitApplicationSourceReview Capture(PermitApplicationDataScope context, PERMIT_APPLICATION application,
        IReadOnlyList<PERMIT_STATUS_HISTORY> history, PermitApplicationSupportingSnapshot? supporting = null)
    {
        ArgumentNullException.ThrowIfNull(application); ArgumentNullException.ThrowIfNull(history);
        var source = JsonSerializer.Serialize(application, Json);
        var recorded = JsonSerializer.Serialize(history.OrderBy(row => row.PERMIT_STATUS_HISTORY_ID, StringComparer.Ordinal).ToArray(), Json);
        var details = supporting ?? PermitApplicationSupportingSnapshot.Empty;
        var review = new PermitApplicationSourceReview(context, source, recorded, Fingerprint(context, source, recorded, details)) { Supporting = details };
        review.Require(); return review;
    }
    private static string Fingerprint(PermitApplicationDataScope context, string application, string history, PermitApplicationSupportingSnapshot supporting) =>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Context = context, Application = application, History = history, Supporting = supporting }));
    public PERMIT_APPLICATION ReadApplication()
    {
        Require(); return JsonSerializer.Deserialize<PERMIT_APPLICATION>(ApplicationJson, Json)!;
    }
    public IReadOnlyList<PERMIT_STATUS_HISTORY> ReadHistory()
    {
        Require(); return JsonSerializer.Deserialize<PERMIT_STATUS_HISTORY[]>(HistoryJson, Json)!;
    }
    public void Require()
    {
        if (Context is null || ApplicationJson is not {Length: > 0 and <= 131072} || HistoryJson is not {Length: > 0 and <= 2097152} ||
            !ProcessDefinitionRevision.IsValid(Revision)) throw new InvalidOperationException("The permit source review is absent or exceeds its bounds.");
        Context.Require();
        var application = JsonSerializer.Deserialize<PERMIT_APPLICATION>(ApplicationJson, Json);
        var history = JsonSerializer.Deserialize<PERMIT_STATUS_HISTORY[]>(HistoryJson, Json);
        if (application is null || !LeaseSummaryValidator.Identifier(application.PERMIT_APPLICATION_ID) || application.ACTIVE_IND != "Y" ||
            application.OWNER_TENANT_ID != Context.Field.Company.TenantId || application.OWNER_COMPANY_ID != Context.Field.Company.BusinessAssociateId ||
            application.FIELD_ID != Context.Field.FieldId || !Enum.IsDefined(application.STATUS) || !Enum.IsDefined(application.COUNTRY) ||
            !Enum.IsDefined(application.REGULATORY_AUTHORITY) || !Enum.IsDefined(application.APPLICATION_TYPE) || !Enum.IsDefined(application.STATE_PROVINCE) ||
            history is null || history.Length > 4096 || history.Any(row => row is null || row.ACTIVE_IND != "Y" ||
                !LeaseSummaryValidator.Identifier(row.PERMIT_STATUS_HISTORY_ID) || row.PERMIT_APPLICATION_ID != application.PERMIT_APPLICATION_ID) ||
            history.Select(row => row.PERMIT_STATUS_HISTORY_ID).Distinct(StringComparer.Ordinal).Count() != history.Length ||
            Supporting is null || Revision != Fingerprint(Context, ApplicationJson, HistoryJson, Supporting))
            throw new InvalidOperationException("The permit source review changed or belongs to another company, FIELD or application history.");
        Supporting.Require(application);
    }
    public void RequireCurrent(PermitApplicationSourceReview current)
    {
        Require(); current.Require();
        if (Context != current.Context || Revision != current.Revision || ApplicationJson != current.ApplicationJson || HistoryJson != current.HistoryJson)
            throw new InvalidOperationException("The reviewed permit evidence changed. Read and review the current source again.");
    }
}

/// <summary>One retained original for a reviewed native submission or independent decision.</summary>
public sealed record PermitApplicationCommand(
    [property: JsonRequired] Guid IntentId,
    [property: JsonRequired] PermitApplicationSourceReview Review,
    [property: JsonRequired] string Action,
    [property: JsonRequired] string Reason)
{
    public PermitApplicationDraftChanges? DraftChanges {get;init;}
    public PermitApplicationCreationReview? CreationReview {get;init;}
    public PermitDrillingDraftChanges? DrillingChanges {get;init;}
    public PermitEnvironmentalDraftChanges? EnvironmentalChanges {get;init;}
    public PermitInjectionDraftChanges? InjectionChanges {get;init;}
    public static bool IsAuthorAction(string action)=>action is "SUBMIT" or "EDIT_DRAFT" or "CREATE_DRAFT" || PermitSupportingAmendments.EntityType(action) is not null;
    public void Require()
    {
        if (IntentId == Guid.Empty || Review is null || string.IsNullOrWhiteSpace(Reason) || Reason.Length > 4096 || Reason != Reason.Trim())
            throw new InvalidOperationException("Retain one permit original with a specific bounded reason before transport.");
        if(Action=="CREATE_DRAFT")
        {
            var creation=CreationReview??throw new InvalidOperationException("The exact creation source review is required.");
            creation.Require();Review.Require();
            if(EnvironmentalChanges is not null || InjectionChanges is not null || DrillingChanges is not null || Review.Supporting != PermitApplicationSupportingSnapshot.Empty || DraftChanges is not null || creation.Context!=Review.Context || Review.ReadHistory().Count!=0 ||
                PermitApplicationCommandOutcome.Payload(Review.ReadApplication())!=PermitApplicationCommandOutcome.Payload(creation.BuildDraft(IntentId)))
                throw new InvalidOperationException("The creation original differs from its exact proposed header, resources, identity, ownership or draft lifecycle.");
            return;
        }
        if(CreationReview is not null)throw new InvalidOperationException("Creation source evidence belongs only to a creation original.");
        if(Action=="AMEND_DRILLING")
            (DrillingChanges??throw new InvalidOperationException("Explicit reviewed drilling details are required.")).Require(Review);
        else if(DrillingChanges is not null)throw new InvalidOperationException("Drilling changes belong only to a supporting amendment original.");
        if(Action=="AMEND_ENVIRONMENTAL")
            (EnvironmentalChanges??throw new InvalidOperationException("Explicit reviewed environmental details are required.")).Require(Review);
        else if(EnvironmentalChanges is not null)throw new InvalidOperationException("Environmental changes belong only to their explicit amendment original.");
        if(Action=="AMEND_INJECTION")
            (InjectionChanges??throw new InvalidOperationException("Explicit reviewed injection details are required.")).Require(Review);
        else if(InjectionChanges is not null)throw new InvalidOperationException("Injection changes belong only to their explicit amendment original.");
        if(Action=="EDIT_DRAFT")
            (DraftChanges??throw new InvalidOperationException("The exact proposed draft changes are required.")).Require(Review.ReadApplication());
        else if(DraftChanges is not null)throw new InvalidOperationException("Draft changes belong only to an explicit draft-edit original.");
        if (!AllowedActions(Review).Contains(Action,StringComparer.Ordinal))
            throw new InvalidOperationException("The current permit stage, explicit applicant/country and recorded independent submitter must support this action.");
    }
    public static IReadOnlyList<string> AllowedActions(PermitApplicationSourceReview review)
    {
        ArgumentNullException.ThrowIfNull(review);var application=review.ReadApplication();
        if (application.STATUS is PermitApplicationStatus.Draft or PermitApplicationStatus.AdditionalInformationRequired)
            {
            var actions = new List<string> { "EDIT_DRAFT" };
            if (application.COUNTRY != Country.Other && !string.IsNullOrWhiteSpace(application.APPLICANT_ID)) actions.Add("SUBMIT");
            if (application.APPLICATION_TYPE == PermitApplicationType.Drilling && LeaseSummaryValidator.Identifier(application.WELL_ID)) actions.Add("AMEND_DRILLING");
            if(application.APPLICATION_TYPE==PermitApplicationType.Environmental)actions.Add("AMEND_ENVIRONMENTAL");
            if(application.APPLICATION_TYPE==PermitApplicationType.Injection && LeaseSummaryValidator.Identifier(application.WELL_ID))actions.Add("AMEND_INJECTION");
            return actions;
        }
        if (application.STATUS is PermitApplicationStatus.Submitted or PermitApplicationStatus.UnderReview &&
            LeaseSummaryValidator.Identifier(application.SUBMITTED_BY_USER_ID) && application.SUBMITTED_BY_USER_ID != review.Context.Field.ActorUserId)
            return ["APPROVE","REJECT","RETURN_FOR_CORRECTION"];
        return [];
    }
}
