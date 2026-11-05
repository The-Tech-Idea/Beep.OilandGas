using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed class CompanyHseAssignedActionReader(CompanyModuleConnection company,
    ICompanyFieldSelectionService selections, IFieldOrchestrator field, IContractorManagementService contractors)
{
    public async Task<CompanyHseAssignedActions> ReadAsync(ClaimsPrincipal user, CancellationToken token)
    {
        var actor = user.ActingUserId();
        async Task<(FieldSelectionReview Field, string Authority)> Capture()
        {
            token.ThrowIfCancellationRequested();
            var access = await company.RefreshCompanyAsync(token);
            company.RequireActor(actor);
            if (user.Identity?.IsAuthenticated != true || !access.Roles.Any(user.IsInRole) ||
                !access.Permissions.Contains(PermissionConstants.HSE.View, StringComparer.Ordinal))
                throw RefusalException.Forbidden("Current company HSE view authority is required.");
            var selected = await selections.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(selected, access.Scope, actor);
            if (!selected.SelectionAvailable || selected.FieldId != field.CurrentFieldId ||
                selected.SelectionContext!.Database != await company.ResolveAsync(token: token))
                throw RefusalException.Forbidden("The current company, account, database and FIELD are required.");
            return (selected.SelectionContext, ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new
            {
                access.Scope, access.UserId,
                Roles = access.Roles.Order(StringComparer.Ordinal).ToArray(),
                Permissions = access.Permissions.Order(StringComparer.Ordinal).ToArray()
            })));
        }
        var captured = await Capture();
        async Task Guard()
        {
            if (await Capture() != captured) throw RefusalException.Conflict("Assigned-action authority or FIELD changed. Review again.");
        }
        var identities = contractors as ICompanyPersonnelIdentityOwner
            ?? throw new InvalidOperationException("The existing recorded personnel identity owner is unavailable.");
        return await ReadPopulationAsync(captured.Field, captured.Authority, identities, field.GetHSEService(), Guard, token);
    }

    internal static async Task<CompanyHseAssignedActions> ReadPopulationAsync(FieldSelectionReview context, string revision,
        ICompanyPersonnelIdentityOwner identities, IFieldHSEService hse, Func<Task> guard, CancellationToken token)
    {
        var identityContext = new CompanyPersonnelPolicyContext(context.Company, context.Database, context.ActorUserId, revision);
        identityContext.Require();
        async Task Guard() { token.ThrowIfCancellationRequested(); await guard(); }
        await Guard();
        var catalog = await identities.ReadPersonnelIdentitiesAsync(identityContext, Guard); catalog.Require();
        if (catalog.Context != identityContext) throw RefusalException.Forbidden("The recorded identity population belongs to another company or binding.");
        var now = DateTime.UtcNow;
        var links = catalog.Versions.Where(row => row.UserId == context.ActorUserId && row.EffectiveFromUtc <= now && now < row.EffectiveToUtc).ToArray();
        if (links.Length != 1) throw RefusalException.Conflict("A current independently recorded account-to-person link is required. Ask the company identity administrator.");
        var person = await identities.ReadIdentityPersonAsync(identityContext, links[0].PersonBaId, Guard);
        person.Require(context.Company, links[0].PersonBaId);
        bool Covers(DateTime? start, DateTime? end) => start is { Kind: DateTimeKind.Utc } from && end is { Kind: DateTimeKind.Utc } to && from <= now && DateTime.UtcNow < to;
        if (!Covers(person.EffectiveFromUtc, person.EffectiveToUtc) || !Covers(person.Employment.EffectiveDate, person.Employment.ExpiryDate))
            throw RefusalException.Conflict("The recorded person and company employment must be current.");

        async Task<(string Proof, List<CompanyHseAssignedAction> Assigned, int Count)> Read()
        {
            await Guard();
            var incidents = await hse.GetIncidentsAsync(null) ?? throw RefusalException.Conflict("The FIELD incident population is unavailable.");
            if (incidents.Count > 64 || incidents.Any(row => row is null || !LeaseSummaryValidator.Identifier(row.IncidentId) || row.FieldId != context.FieldId) ||
                incidents.Select(row => row.IncidentId).Distinct(StringComparer.Ordinal).Count() != incidents.Count)
                throw RefusalException.Conflict("The FIELD population is foreign, ambiguous or exceeds the 64-incident assigned-action review limit.");
            var proof = new List<object>(); var assigned = new List<CompanyHseAssignedAction>();
            foreach (var incident in incidents.OrderBy(row => row.IncidentId, StringComparer.Ordinal))
            {
                await Guard();
                if (incident.CurrentState is not (IncidentState.Reported or IncidentState.UnderInvestigation or IncidentState.PendingRCA or
                    IncidentState.PendingCorrectiveAction or IncidentState.PendingCloseOut or IncidentState.Closed or IncidentState.Cancelled))
                    throw RefusalException.Conflict("An incident state is unconfirmed; no assigned-action total is available.");
                // Recorded plans can precede the incident's corrective-work state. Read every open incident.
                List<CAStatus> actions = incident.CurrentState is IncidentState.Closed or IncidentState.Cancelled
                    ? [] : await hse.GetCorrectiveActionsAsync(incident.IncidentId);
                if (actions is null || actions.Count > 4096 || actions.Any(row => row is null || row.StepSeq <= 0 ||
                    !LeaseSummaryValidator.Identifier(row.ResponsiblePerson) || row.DueDate.Kind != DateTimeKind.Utc || row.DueDate == default ||
                    row.Status is not ("OPEN" or "COMPLETED") || (row.Status == "COMPLETED") != row.CompletedDate.HasValue) ||
                    actions.Select(row => row.StepSeq).Distinct().Count() != actions.Count)
                    throw RefusalException.Conflict("Recorded action responsibility, dates or state cannot be confirmed.");
                proof.Add(new { incident.IncidentId, incident.CurrentState, Actions = actions.OrderBy(row => row.StepSeq).ToArray() });
                assigned.AddRange(actions.Where(row => row.Status == "OPEN" && row.DueDate < now && row.ResponsiblePerson == person.PersonBaId)
                    .Select(row => new CompanyHseAssignedAction(incident.IncidentId, row.StepSeq, row.Description, row.DueDate)));
                if (assigned.Count > 4096) throw RefusalException.Conflict("The assigned-action population exceeds the review limit.");
            }
            return (JsonSerializer.Serialize(proof), assigned, incidents.Count);
        }
        var first = await Read(); var closing = await Read();
        var closingCatalog = await identities.ReadPersonnelIdentitiesAsync(identityContext, Guard); closingCatalog.Require();
        var closingPerson = await identities.ReadIdentityPersonAsync(identityContext, person.PersonBaId, Guard);
        await Guard();
        if (first.Proof != closing.Proof || catalog.Revision != closingCatalog.Revision || closingCatalog.Context != identityContext ||
            person != closingPerson || links[0].EffectiveToUtc <= DateTime.UtcNow ||
            !Covers(closingPerson.EffectiveFromUtc, closingPerson.EffectiveToUtc) || !Covers(closingPerson.Employment.EffectiveDate, closingPerson.Employment.ExpiryDate))
            throw RefusalException.Conflict("Recorded responsibility, identity, employment or incident evidence changed. Review again.");
        var result = new CompanyHseAssignedActions(context, now, closing.Count,
            closing.Assigned.OrderBy(row => row.DueAtUtc).ThenBy(row => row.IncidentId, StringComparer.Ordinal).ThenBy(row => row.StepSequence).ToArray());
        result.Require(); return result;
    }
}
