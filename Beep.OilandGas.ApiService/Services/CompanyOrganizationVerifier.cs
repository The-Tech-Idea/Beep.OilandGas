using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public interface ICompanyOrganizationReader
{
    Task<string> ReadAsync(CompanyScope company, Func<Task> authority, CancellationToken token);
}

/// <summary>Reads only the selected company's exact PPDM business-associate key.</summary>
public sealed class CompanyOrganizationReader(CompanyModuleConnection connection, IDMEEditor editor,
    ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata) : ICompanyOrganizationReader
{
    public async Task<string> ReadAsync(CompanyScope company, Func<Task> authority, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); await authority();
        if (connection.Company.Scope != company || !CompanyPPDMEvidenceReader.Identifier(company.BusinessAssociateId))
            throw new UnauthorizedAccessException("The selected company changed or is invalid.");
        var database = await connection.ResolveAsync(token: token);
        var reader = new CompanyPPDMEvidenceReader(connection, editor, columns, defaults, metadata, database, company, connection.Company.UserId);
        await reader.OpenAsync();
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(BUSINESS_ASSOCIATE), ["BUSINESS_ASSOCIATE_ID"]);
        var rows = await reader.ReadAsync(typeof(BUSINESS_ASSOCIATE), [CompanyPPDMEvidenceReader.Equal("BUSINESS_ASSOCIATE_ID", company.BusinessAssociateId)], ["BUSINESS_ASSOCIATE_ID", "ACTIVE_IND"]);
        if (rows.Length > 1) throw new InvalidOperationException("The company organization is ambiguous.");
        var result = rows.Length == 0 ? "NotFound" : ((BUSINESS_ASSOCIATE)rows[0]).ACTIVE_IND switch
        {
            "Y" => "Active", "N" => "Inactive", _ => "Unconfirmed"
        };
        token.ThrowIfCancellationRequested(); await authority(); await reader.RequireCurrentAsync();
        return result;
    }
}

public sealed class CompanyOrganizationVerifier(ICompanyEnvironmentBindingReader bindings, ICompanyOrganizationReader reader)
{
    public async Task<CompanyOrganizationReview> ReviewAsync(CompanyScope company, Func<Task> authority, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(company); ArgumentNullException.ThrowIfNull(authority);
        await authority();
        var binding = await bindings.ResolveAsync("PPDM_CORE", token);
        if (!CompanyEnvironmentIdentity.IsRevision(binding)) throw new InvalidOperationException("The company PPDM binding is unavailable.");
        var first = await reader.ReadAsync(company, authority, token);
        var second = await reader.ReadAsync(company, authority, token);
        if (first != second || first is not ("Active" or "Inactive" or "NotFound" or "Unconfirmed"))
            throw new InvalidOperationException("The company organization changed during verification.");
        await authority();
        if (await bindings.ResolveAsync("PPDM_CORE", token) != binding) throw new InvalidOperationException("The company PPDM binding changed.");
        await authority();
        return new(company, binding!, first);
    }
}
