using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

/// <summary>Explicit proposed terms. Dates use the source's stored calendar, with no inferred zone or conversion.</summary>
public sealed record SalesContractEffectiveTerms(string Version,DateTime EffectiveFrom,DateTime? EffectiveThrough,
    string CurrencyCode,decimal Price,string PriceUnit,string PricingMethod,string Notes)
{
    public const string FixedUnitPriceMethod="FIXED_UNIT_PRICE";
    public void Require()
    {
        SalesContractTermsChecks.Text(Version,128);SalesContractTermsChecks.Text(CurrencyCode,16);
        SalesContractTermsChecks.Text(PriceUnit,128);SalesContractTermsChecks.Text(PricingMethod,128);SalesContractTermsChecks.Text(Notes,4096);
        if(Price<0 || EffectiveFrom==default || EffectiveFrom.TimeOfDay!=TimeSpan.Zero || EffectiveFrom.Kind!=DateTimeKind.Unspecified ||
            EffectiveThrough is {} end && (end==default || end.TimeOfDay!=TimeSpan.Zero || end.Kind!=DateTimeKind.Unspecified || end<EffectiveFrom))
            throw new InvalidOperationException("Explicit nonnegative pricing and stored calendar-date effective terms are required.");
    }
}

/// <summary>A preparation intent is evidence and proposed terms, never permission or a native record update.</summary>
public sealed record SalesContractTermsPreparation(string IntentId,CompanyScope Company,string ActorUserId,
    RevenueSalesContractReview Source,SalesContractEffectiveTerms Terms)
{
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);SalesContractTermsChecks.Text(ActorUserId,128);
        if(!Guid.TryParseExact(IntentId,"N",out var id) || id==Guid.Empty || IntentId!=id.ToString("N") || Source is null || Terms is null)
            throw new InvalidOperationException("The original commercial preparation intent is incomplete.");
        Source.Require(Company,ActorUserId,Source.Source.FieldId,Source.Source.From,Source.Source.Through,Source.TransactionId,Source.Source.Revision);
        Terms.Require();
    }
    public void RequireCurrentSource(RevenueSalesContractReview current)
    {
        Require();SalesContractTermsChecks.SameSource(Source,current);
    }
}

public enum SalesContractTermsDecision { Approve,Reject }

/// <summary>Decision intent must identify the exact frozen preparation; the API independently supplies fresh authority.</summary>
public sealed record SalesContractTermsDecisionCommand(string IntentId,CompanyScope Company,string ActorUserId,
    string PreparationRevision,SalesContractTermsDecision Decision,string Reason)
{
    public void Require(SalesContractTermsRecord preparation)
    {
        preparation.Require();SalesContractTermsChecks.Text(ActorUserId,128);SalesContractTermsChecks.Text(Reason,4096);
        if(!Guid.TryParseExact(IntentId,"N",out var id) || id==Guid.Empty || IntentId!=id.ToString("N") || !Enum.IsDefined(Decision) ||
            Company!=preparation.Preparation.Company || PreparationRevision!=preparation.Revision || preparation.Decision is not null)
            throw new InvalidOperationException("The decision must match the exact current undecided commercial preparation.");
        var source=preparation.Preparation.Source;
        var transaction=source.Source.Rows.Single(row=>row.REVENUE_TRANSACTION_ID==source.TransactionId);
        // Current role/grant checks belong to the API; factual maker identities govern this source separation.
        if(new[]{preparation.Preparation.ActorUserId,source.Contract.ROW_CREATED_BY,transaction.ROW_CREATED_BY}.Any(actor=>!string.IsNullOrWhiteSpace(actor) && actor==ActorUserId))
            throw new InvalidOperationException("A commercial preparer or recorded source maker cannot decide these terms.");
        if(string.IsNullOrWhiteSpace(source.Contract.ROW_CREATED_BY) || string.IsNullOrWhiteSpace(transaction.ROW_CREATED_BY))
            throw new InvalidOperationException("Recorded source maker identities are required for independent commercial approval.");
    }
}

/// <summary>Frozen preparation/decision evidence for the existing native owner. Construction does not persist or approve a contract.</summary>
public sealed record SalesContractTermsRecord(SalesContractTermsPreparation Preparation,DateTime PreparedAtUtc,
    SalesContractTermsDecisionCommand? Decision,DateTime? DecidedAtUtc,string Revision)
{
    public void Require()
    {
        if(Preparation is null)throw new InvalidOperationException("The frozen commercial preparation is missing.");
        Preparation.Require();SalesContractTermsChecks.Utc(PreparedAtUtc);
        if(Decision is null && DecidedAtUtc is not null || Decision is not null && DecidedAtUtc is null ||
            Revision!=Digest(Preparation,PreparedAtUtc,Decision,DecidedAtUtc))throw new InvalidOperationException("Commercial preparation or decision evidence changed.");
        if(Decision is {} decision)
        {
            SalesContractTermsChecks.Utc(DecidedAtUtc!.Value);
            if(DecidedAtUtc<PreparedAtUtc)throw new InvalidOperationException("Commercial decision predates its preparation.");
            decision.Require(Create(Preparation,PreparedAtUtc));
        }
    }
    public static SalesContractTermsRecord Create(SalesContractTermsPreparation preparation,DateTime preparedAtUtc)
    {
        preparation=CompanyInventorySourceReview.Copy(preparation);preparation.Require();SalesContractTermsChecks.Utc(preparedAtUtc);
        return new(preparation,preparedAtUtc,null,null,Digest(preparation,preparedAtUtc,null,null));
    }
    public SalesContractTermsRecord Decide(SalesContractTermsDecisionCommand command,RevenueSalesContractReview current,DateTime decidedAtUtc)
    {
        Require();command.Require(this);Preparation.RequireCurrentSource(current);SalesContractTermsChecks.Utc(decidedAtUtc);
        var result=new SalesContractTermsRecord(CompanyInventorySourceReview.Copy(Preparation),PreparedAtUtc,CompanyInventorySourceReview.Copy(command),decidedAtUtc,Digest(Preparation,PreparedAtUtc,command,decidedAtUtc));
        result.Require();return result;
    }
    private static string Digest(SalesContractTermsPreparation preparation,DateTime preparedAtUtc,SalesContractTermsDecisionCommand? decision,DateTime? decidedAtUtc)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{preparation,preparedAtUtc,decision,decidedAtUtc}));
}

internal static class SalesContractTermsChecks
{
    internal static void Text(string value,int maximum)
    {if(string.IsNullOrWhiteSpace(value) || value.Length>maximum || value.Any(char.IsControl))throw new InvalidOperationException("Explicit bounded commercial terms and identities are required.");}
    internal static void Utc(DateTime value)
    {if(value==default || value.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("A recorded UTC command time is required.");}
    internal static void SameSource(RevenueSalesContractReview original,RevenueSalesContractReview current)
    {
        current.Require(original.Source.Company,current.Source.ActorUserId,original.Source.FieldId,original.Source.From,original.Source.Through,original.TransactionId,current.Source.Revision);
        // Fresh reviewer capture legitimately changes actor provenance; compare the same source facts in the maker's capture identity.
        var normalized=FieldRevenueSourceReview.Create(current.Source.Company,original.Source.ActorUserId,current.Source.FieldId,current.Source.From,current.Source.Through,current.Source.BindingRevision,current.Source.Rows);
        var review=RevenueSalesContractReview.Create(normalized,current.TransactionId,current.Contract);
        if(review.Revision!=original.Revision)throw new InvalidOperationException("Original revenue, native contract or binding changed before the commercial decision.");
    }
}
