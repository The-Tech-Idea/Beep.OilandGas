using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Repository;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryRolePersonaReader(IRepositoryAccessService access, RepositoryTenantContext tenant) : IRolePersonaReader
{
    public async Task<List<string>> GetActivePersonasAsync(string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        var company = tenant.Company ?? throw new InvalidOperationException("A validated current company is required to resolve reviewer personas.");
        async Task ConfirmMembership()
        {
            var current = await access.GetCompanyAccessAsync(company.UserId, company.Scope);
            if (current?.Scope != company.Scope)
                throw new UnauthorizedAccessException("Current company membership is required to resolve reviewer personas.");
        }
        await ConfirmMembership();
        var recipients = await access.GetCompanyRoleRecipientsAsync(company.Scope, roleName)
            ?? throw new InvalidOperationException("The current company role recipients are unavailable.");
        await ConfirmMembership();
        return recipients.Select(recipient => recipient.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
    }
}
