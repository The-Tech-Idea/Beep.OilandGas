using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyAccountingProcessReads
{
    public async Task<CompanyAccountingProcessStartCatalog> CatalogAsync(ClaimsPrincipal user,CompanyAccountingProcessStartCatalogQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);query.Require();
        var original=await context.CaptureProcessAsync(user);
        async Task Guard()
        {
            var current=await context.CaptureProcessAsync(user);
            if(user.Identity?.IsAuthenticated!=true || current.Access.Scope!=original.Access.Scope || current.Actor!=original.Actor || current.Binding!=original.Binding ||
                string.IsNullOrWhiteSpace(current.Actor) || !ProcessDefinitionRevision.IsValid(current.Binding) || current.Access.Roles is null || !current.Access.Roles.Any(user.IsInRole) ||
                current.Access.Permissions?.Contains("Workflow.Start",StringComparer.Ordinal)!=true ||
                current.Access.Permissions.Contains(CompanyAccountingSourceTypes.ReadPermission(query.EntityType),StringComparer.Ordinal)!=true)
                throw new UnauthorizedAccessException("Current company source and workflow start authority are required.");
        }
        async Task<(Beep.OilandGas.Models.Processes.ProcessDefinition[] Rows,string Revision)> Definitions()
        {
            await Guard();
            var rows=await original.Process.GetProcessDefinitionsByTypeAsync("FINANCIAL") ?? throw new InvalidOperationException("Recorded financial workflow definitions are unavailable.");
            if(rows.Count>128 || rows.Any(row=>row is null || string.IsNullOrWhiteSpace(row.ProcessId) || row.ProcessId.Length>128 || row.ProcessId.Any(char.IsControl) || !row.IsActive || row.ProcessType!="FINANCIAL") ||
                rows.Select(row=>row.ProcessId).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
                throw new InvalidOperationException("The recorded financial workflow catalog is ambiguous.");
            var frozen=JsonSerializer.Deserialize<Beep.OilandGas.Models.Processes.ProcessDefinition[]>(JsonSerializer.Serialize(rows.OrderBy(row=>row.ProcessId,StringComparer.Ordinal)))!;
            return(frozen,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(frozen)));
        }
        await Guard();
        var source=await ReadStartSourceAsync(user,original.Access.Scope,original.Actor,query.EntityType,query.EntityId,query.ExpectedSourceRevision);
        var initial=await Definitions();
        var choices=new List<CompanyAccountingProcessStartReview>();
        foreach(var definition in initial.Rows.Where(row=>row.EntityType==query.EntityType))
        {
            var review=await ReviewStartAsync(user,new(definition.ProcessId,query.EntityType,query.EntityId,query.ExpectedSourceRevision));
            if(review.DefinitionRevision!=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)))
                throw new InvalidOperationException("A workflow choice changed during catalog review.");
            choices.Add(review);
        }
        var closing=await Definitions();
        var closingSource=await ReadStartSourceAsync(user,original.Access.Scope,original.Actor,query.EntityType,query.EntityId,query.ExpectedSourceRevision);
        await Guard();
        if(initial.Revision!=closing.Revision || source!=closingSource)throw new InvalidOperationException("The displayed source or workflow catalog changed during review.");
        var catalog=new CompanyAccountingProcessStartCatalog(original.Access.Scope,original.Actor,query.EntityType,query.EntityId,source.Revision,original.Binding,Array.AsReadOnly(choices.ToArray()));
        catalog.Require(original.Access.Scope,original.Actor,query);return catalog;
    }
}
