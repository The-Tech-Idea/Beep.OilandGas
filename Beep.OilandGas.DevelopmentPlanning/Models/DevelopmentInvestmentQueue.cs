using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.DevelopmentPlanning.Models;
public sealed record DevelopmentAfeIndexItem([property:JsonRequired]DevelopmentAfeFacts Afe,[property:JsonRequired]string ProcessId,
    [property:JsonRequired]string FieldId,[property:JsonRequired]string PlanName,[property:JsonRequired]int BaselineVersion)
{
    public void Require(){if(Afe is null || new[]{ProcessId,FieldId}.Any(value=>!LeaseSummaryValidator.Identifier(value)) || string.IsNullOrWhiteSpace(PlanName) || PlanName!=PlanName.Trim() || PlanName.Length>256 || PlanName.Any(char.IsControl) || BaselineVersion<2)throw new InvalidOperationException("The exact source-bound AFE index is unavailable.");Afe.Require();}
}
public sealed record DevelopmentInvestmentQueueItem([property:JsonRequired]DevelopmentAfeIndexItem Source,[property:JsonRequired]DevelopmentInvestmentFacts? Recorded);
public sealed record DevelopmentInvestmentQueue([property:JsonRequired]CompanyScope Company,[property:JsonRequired]string ActorUserId,
    [property:JsonRequired]AssetDatabaseScope CoreDatabase,[property:JsonRequired]AssetDatabaseScope BudgetDatabase,
    [property:JsonRequired]IReadOnlyList<DevelopmentInvestmentQueueItem> Items,[property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public const int MaximumItems=256;
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this with{Revision="",CheckedAtUtc=default}));
    public void Require(CompanyScope company,string actor)
    {
        PurchaseOrderOwnership.RequireScope(company);if(Company!=company || ActorUserId!=actor || !LeaseSummaryValidator.Identifier(actor) || CoreDatabase is null || BudgetDatabase is null ||
            new[]{CoreDatabase,BudgetDatabase}.Any(db=>!LeaseSummaryValidator.Identifier(db.ConnectionName) || !LeaseSummaryValidator.Identifier(db.Fingerprint)) || Items is null || Items.Count>MaximumItems || Items.Any(row=>row?.Source is null) ||
            Items.Select(row=>row.Source.Afe.AfeId).Distinct(StringComparer.Ordinal).Count()!=Items.Count || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || Revision!=ContentRevision())throw new InvalidOperationException("The complete current bounded company/account AFE investment queue is unconfirmed.");
        foreach(var row in Items){row.Source.Require();row.Recorded?.Require(company,row.Source.Afe);}
    }
}
