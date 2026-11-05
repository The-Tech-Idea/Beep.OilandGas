using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;

/// <summary>Saved proposals are reconciled with preserved native evidence, never treated as executable by discovery.</summary>
public sealed record CompanyCommercialAmendmentQueueRow(COMMERCIAL_TERMS_INTENT Entry,CompanyCommercialProcessSourceReview Source)
{
    private static bool Same(object? first,object? second)=>JsonSerializer.Serialize(first)==JsonSerializer.Serialize(second);
    public SalesContractTermsRecord? Recorded()
    {
        var command=Entry.Restore(Source.Company,Source.ActorUserId,Source.InstanceId);
        Source.Require(Source.Company,Source.ActorUserId,Source.InstanceId);
        if(Source.ProcessRevision!=command.Review.ProcessRevision || !Same(Source.Terms.Recorded,command.Review.Previous) || Source.CurrentNativeContract is null)
            throw new InvalidOperationException("The saved proposal's original closed process/predecessor is unavailable.");
        var proof=SalesContractTermsNativeProof.Require(Source.Company,Source.CurrentNativeContract);
        var versions=(proof.PriorVersions ?? []).Select(row=>(row.Record,row.Amendment)).Append((proof.Record,proof.Amendment));
        var matches=versions.Where(row=>Same(row.Record.Preparation,command.Amendment.Preparation)).ToArray();
        if(matches.Length>1)throw new InvalidOperationException("The saved proposal has ambiguous native outcomes.");
        if(matches.Length==0)return null;
        var expected=new SalesContractTermsAmendmentOrigin(command.Amendment.Previous.Revision,command.Amendment.PreviousNativeRevision,command.Amendment.Reason,command.Amendment.Process);
        if(matches[0].Amendment!=expected)throw new InvalidOperationException("The saved proposal's native origin changed.");
        return CompanyInventorySourceReview.Copy(matches[0].Record);
    }
    [JsonIgnore] public string Status
    {
        get
        {
            if(Recorded() is not null)return "RECORDED";
            var command=Entry.Restore(Source.Company,Source.ActorUserId,Source.InstanceId);
            var proof=SalesContractTermsNativeProof.Require(Source.Company,Source.CurrentNativeContract!);
            return proof.Revision==command.Amendment.PreviousNativeRevision && Same(proof.Record,command.Review.Previous)?"SAVED_PENDING":"RECONCILIATION_REQUIRED";
        }
    }
}
public sealed record CompanyCommercialAmendmentQueue(CompanyScope Company,string ActorUserId,string BindingRevision,IReadOnlyList<CompanyCommercialAmendmentQueueRow> Rows,string Revision)
{
    public const int MaximumEntries=32;
    public const int MaximumCommandCharacters=8388608;
    public void Require(CompanyScope company,string actor)
    {
        APInvoiceOwnership.RequireScope(company);
        if(Company!=company || ActorUserId!=actor || string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl) || !ProcessDefinitionRevision.IsValid(BindingRevision) || Rows is null || Rows.Count>MaximumEntries || Rows.Any(row=>row?.Entry is null || row.Source is null))
            throw new InvalidOperationException("The complete original company/account proposal queue is required.");
        if(Rows.Sum(row=>(long)row.Entry.COMMAND_JSON.Length)>MaximumCommandCharacters || Rows.Select(row=>row.Entry.PROCESS_INSTANCE_ID).Distinct(StringComparer.Ordinal).Count()!=Rows.Count || Rows.Select(row=>row.Entry.COMMERCIAL_TERMS_INTENT_ID).Distinct(StringComparer.Ordinal).Count()!=Rows.Count || !Rows.Select(row=>row.Entry.PROCESS_INSTANCE_ID).SequenceEqual(Rows.Select(row=>row.Entry.PROCESS_INSTANCE_ID).Order(StringComparer.Ordinal)))
            throw new InvalidOperationException("A bounded unique complete proposal queue is required.");
        foreach(var row in Rows)
        {
            var command=row.Entry.Restore(company,actor,row.Entry.PROCESS_INSTANCE_ID);
            row.Source.Require(company,actor,row.Entry.PROCESS_INSTANCE_ID);
            if(command.Review.BindingRevision!=BindingRevision)throw new InvalidOperationException("The saved queue binding changed.");
            _=row.Status;
        }
        if(Revision!=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company,ActorUserId,BindingRevision,Rows})))throw new InvalidOperationException("The proposal queue evidence changed.");
    }
}
