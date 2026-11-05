namespace TheTechIdea.Data.OilGas;

/// <summary>Complete owned saved commands for the exact current native ticket, before any business replay.</summary>
public sealed record RevenueDeliveryAcceptanceSavedReview(RevenueDeliveryAcceptanceReadReview Current,IReadOnlyList<DELIVERY_ACCEPTANCE_INTENT> Entries)
{
    public void Require(CompanyScope company,string actor,string field,DateTime from,DateTime through,string transaction)
    {
        if(Current?.Source?.Source?.Source is not {} source || Entries is null)throw new InvalidOperationException("The complete saved delivery review is required.");
        Current.Require();source.Require(company,actor,field,from,through);Current.Source.Source.Require(company,actor,field,from,through,transaction,source.Revision);
        if(Entries.Count>DELIVERY_ACCEPTANCE_INTENT.MaximumEntries || Entries.Any(row=>row is null) ||
            Entries.Sum(row=>(long)(row.COMMAND_JSON?.Length ??0))>DELIVERY_ACCEPTANCE_INTENT.MaximumSetCharacters ||
            Entries.Select(row=>row.SCOPE_KEY).Distinct(StringComparer.Ordinal).Count()!=Entries.Count ||
            Entries.Select(row=>row.DELIVERY_ACCEPTANCE_INTENT_ID).Distinct(StringComparer.Ordinal).Count()!=Entries.Count)
            throw new InvalidOperationException("One complete bounded unambiguous saved delivery set is required.");
        foreach(var row in Entries)
        {
            var original=row.Restore(company,actor,row.SCOPE_KEY);
            if(original.Original.Source.TicketId!=Current.Source.TicketId || original.Original.Source.Source.Source.FieldId!=field)
                throw new InvalidOperationException("The saved original delivery ticket or FIELD differs.");
        }
    }
}
