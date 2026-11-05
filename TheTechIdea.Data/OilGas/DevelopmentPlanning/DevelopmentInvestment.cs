using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

public sealed record DevelopmentInvestmentFacts([property:JsonRequired]string Id,[property:JsonRequired]string AfeId,
    [property:JsonRequired]string AfeRevision,[property:JsonRequired]string Decision,[property:JsonRequired]string Reviewer,
    [property:JsonRequired]DateTimeOffset AtUtc,[property:JsonRequired]string RecordedRevision)
{
    public void Require(CompanyScope company,DevelopmentAfeFacts afe)
    {
        afe.Require();if(Id!=DevelopmentInvestmentCommand.IdFor(company,afe.AfeId) || AfeId!=afe.AfeId || AfeRevision!=afe.RecordedRevision || Decision is not ("APPROVE" or "REJECT") ||
            !LeaseSummaryValidator.Identifier(Reviewer) || Reviewer==afe.CreatedBy || AtUtc.Offset!=TimeSpan.Zero || AtUtc<afe.CreatedAtUtc || !ProcessDefinitionRevision.IsValid(RecordedRevision))
            throw new InvalidOperationException("The recorded independent investment decision must match its original AFE/currency/amount/author/audit.");
    }
}
public sealed record DevelopmentInvestmentReview([property:JsonRequired]DevelopmentAfeReview Current,[property:JsonRequired]string AfeId,
    [property:JsonRequired]DevelopmentInvestmentFacts? Recorded,[property:JsonRequired]IReadOnlyList<string> Blockers,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public const string MakerDecisionBlocker="The request maker cannot make its independent investment decision.";
    public bool BlocksRouting()=>Recorded is not null || Blockers.Any(blocker=>blocker!=MakerDecisionBlocker);
    public DevelopmentAfeFacts Afe()=>Current.Afes.Single(row=>row.AfeId==AfeId);
    public void Require()
    {
        if(Current is null || !LeaseSummaryValidator.Identifier(AfeId) || Blockers is null || Blockers.Count>8 || Blockers.Any(string.IsNullOrWhiteSpace) || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The complete current source-bound AFE investment review is required.");Current.Require();
        if(Current.Afes.Count(row=>row.AfeId==AfeId)!=1)throw new InvalidOperationException("One exact recorded development AFE is required.");Recorded?.Require(Current.Source.Review.Scope.Company,Afe());
    }
}
public sealed record DevelopmentInvestmentCommand([property:JsonRequired]DevelopmentInvestmentReview Review,[property:JsonRequired]Guid IntentId,
    [property:JsonRequired]string Decision,[property:JsonRequired]string Rationale)
{
    public const int MaximumBodyLength=4*1024*1024;
    public void Require()
    {
        if(Review is null)throw new InvalidOperationException("Inspect the original AFE request before an investment decision.");Review.Require();var afe=Review.Afe();var actor=Review.Current.Source.Review.Scope.ActorUserId;
        var book=Review.Current.Books.Books.SingleOrDefault(row=>row.BookId==afe.BookId);
        if(IntentId==Guid.Empty || Review.Recorded is not null || Review.Blockers.Count!=0 || actor==afe.CreatedBy || Decision is not ("APPROVE" or "REJECT") ||
            book is null || book.FunctionalCurrency!=afe.CurrencyCode || book.CurrencyDecimals!=afe.CurrencyDecimals || string.IsNullOrWhiteSpace(Rationale) || Rationale!=Rationale.Trim() || Rationale.Length>4096 ||
            Rationale.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t')) || JsonSerializer.Serialize(this).Length>MaximumBodyLength)
            throw new InvalidOperationException("An independent reviewer, unchanged company book, first explicit approve/reject decision and rationale are required.");
    }
    public static string IdFor(CompanyScope company,string afe)=>"development-investment-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,Afe=afe}));
    public string Id()=>IdFor(Review.Current.Source.Review.Scope.Company,Review.AfeId);
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public void RequireCurrent(DevelopmentInvestmentReview current)
    {Require();current.Require();if(current.AfeId!=Review.AfeId || current.Current.Revision!=Review.Current.Revision)throw new InvalidOperationException("The original AFE, approved source, book configuration or complete request population changed.");}
    public APPROVAL_WORKFLOW ToRecord(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || new DateTimeOffset(utc)<Review.CheckedAtUtc)throw new InvalidOperationException("Actual current investment decision producer UTC is required.");var scope=Review.Current.Source.Review.Scope;var afe=Review.Afe();
        var row=new APPROVAL_WORKFLOW{APPROVAL_WORKFLOW_ID=Id(),ENTITY_NAME="AFE",ENTITY_ID=afe.AfeId,AMOUNT=afe.RequestedAmount,STATUS=Decision=="APPROVE"?"APPROVED":"REJECTED",
            REQUESTED_BY=afe.CreatedBy,REQUESTED_DATE=afe.CreatedAtUtc.UtcDateTime,APPROVED_BY=scope.ActorUserId,APPROVAL_DATE=utc,COMMENTS=Rationale,
            OWNER_TENANT_ID=scope.Company.TenantId,OWNER_COMPANY_BA_ID=scope.Company.BusinessAssociateId,FIELD_ID=scope.FieldId,FDP_BASELINE_ID=afe.BaselineId,
            BOOK_ID=afe.BookId,CURRENCY_CODE=afe.CurrencyCode,ORIGINAL_AFE_REVISION=afe.RecordedRevision,DEVELOPMENT_DECISION_JSON=JsonSerializer.Serialize(this),
            ACTIVE_IND="Y",SOURCE="COMPANY_DEVELOPMENT_INVESTMENT",PPDM_GUID=IntentId.ToString("D"),ROW_CREATED_BY=scope.ActorUserId,ROW_CHANGED_BY=scope.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc};
        row.DEVELOPMENT_DECISION_REVISION=Digest(row);return row;
    }
    public static string Digest(APPROVAL_WORKFLOW row){var copy=CompanyInventorySourceReview.Copy(row);copy.DEVELOPMENT_DECISION_REVISION=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static DevelopmentInvestmentCommand FromRecord(APPROVAL_WORKFLOW row)
    {
        if(row?.DEVELOPMENT_DECISION_JSON is not {Length:>0 and <=MaximumBodyLength} body)throw new InvalidOperationException("The bounded original investment decision proof is unavailable.");
        var command=JsonSerializer.Deserialize<DevelopmentInvestmentCommand>(body)??throw new InvalidOperationException("The original investment decision is invalid.");command.Require();var actual=CompanyInventorySourceReview.Copy(row);
        DateTime Utc(DateTime? date){if(date is null || date.Value.Kind==DateTimeKind.Local)throw new InvalidOperationException("Original investment decision UTC audit is unavailable.");return DateTime.SpecifyKind(date.Value,DateTimeKind.Utc);}
        actual.ROW_CREATED_DATE=Utc(actual.ROW_CREATED_DATE);actual.ROW_CHANGED_DATE=Utc(actual.ROW_CHANGED_DATE);actual.REQUESTED_DATE=Utc(actual.REQUESTED_DATE);actual.APPROVAL_DATE=Utc(actual.APPROVAL_DATE);
        var expected=command.ToRecord(actual.ROW_CREATED_DATE.Value);actual.DEVELOPMENT_DECISION_REVISION=Digest(actual);
        if(row.DEVELOPMENT_DECISION_REVISION!=expected.DEVELOPMENT_DECISION_REVISION || JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The canonical investment decision differs from its original scope/AFE/amount/currency/reviewer/intent/audit.");return command;
    }
    public static DevelopmentInvestmentFacts Facts(APPROVAL_WORKFLOW row)
    {
        var original=FromRecord(row);var result=new DevelopmentInvestmentFacts(row.APPROVAL_WORKFLOW_ID,row.ENTITY_ID,row.ORIGINAL_AFE_REVISION!,original.Decision,row.APPROVED_BY,
            new DateTimeOffset(DateTime.SpecifyKind(row.APPROVAL_DATE!.Value,DateTimeKind.Utc)),row.DEVELOPMENT_DECISION_REVISION!);result.Require(original.Review.Current.Source.Review.Scope.Company,original.Review.Afe());return result;
    }
}
public sealed record DevelopmentInvestmentReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]APPROVAL_WORKFLOW? Recorded,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentInvestmentCommand original)
    {original.Require();if(IntentRevision!=original.Revision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || Recorded is not null && DevelopmentInvestmentCommand.FromRecord(Recorded).Revision()!=original.Revision())throw new InvalidOperationException("The investment decision does not confirm its exact retained original.");}
}
