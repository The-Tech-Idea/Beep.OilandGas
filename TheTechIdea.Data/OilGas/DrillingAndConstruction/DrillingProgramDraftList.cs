using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;

namespace TheTechIdea.Data.OilGas;
public sealed record DrillingProgramDraftList([property:JsonRequired]DrillingProgramBasis Basis,[property:JsonRequired]IReadOnlyList<DRILLING_PROGRAM> Drafts,
    [property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Basis=Basis.Revision,Rows=Drafts.OrderBy(row=>row.PROGRAM_ID,StringComparer.Ordinal)}));
    public void Require()
    {
        Basis.Require();if(Drafts is null || Drafts.Count>128 || Drafts.Any(row=>row is null) || Drafts.Select(row=>row.PROGRAM_ID).Distinct(StringComparer.Ordinal).Count()!=Drafts.Count || Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)throw new InvalidOperationException("The complete original well/funding program rowset is unavailable.");
        foreach(var row in Drafts)
        {
            if(row.OWNER_TENANT_ID!=Basis.Scope().Company.TenantId || row.OWNER_COMPANY_BA_ID!=Basis.Scope().Company.BusinessAssociateId)throw new InvalidOperationException("The program row belongs to another company.");
            var original=CreateDrillingProgramDraft.FromRecord(row);var scope=original.Basis.Scope();var current=Basis.Scope();
            if(scope.Company!=current.Company || scope.FieldId!=current.FieldId || scope.CoreDatabase!=current.CoreDatabase || scope.PlanningDatabase!=current.PlanningDatabase || scope.LifecycleDatabase!=current.LifecycleDatabase || original.Basis.ExecutionDatabase!=Basis.ExecutionDatabase ||
                original.Basis.Well.Uwi!=Basis.Well.Uwi || original.Basis.Funding.Source.Afe()!=Basis.Funding.Source.Afe() || original.Basis.Funding.Progress!.NativeRevision!=Basis.Funding.Progress!.NativeRevision)
                throw new InvalidOperationException("The program is not bound to this exact original approved well/funding source.");
        }
    }
}
