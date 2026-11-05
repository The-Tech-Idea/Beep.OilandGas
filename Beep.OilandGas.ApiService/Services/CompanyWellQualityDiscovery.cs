using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Finds a bounded selection through the same company and asset row policy used by PPDM reads.</summary>
public sealed class CompanyWellQualityDiscovery(CompanyModuleConnection company, IDMEEditor editor,
    ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, IPPDMRowAccessPolicy rows)
{
    public async Task<CompanyWellQualitySample> FindAsync(CompanyScope scope, Func<Task> authority, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(scope); ArgumentNullException.ThrowIfNull(authority);
        await authority();
        var database = await company.ResolveAsync(token: token);
        if (company.Company.Scope != scope) throw RefusalException.Forbidden("The selected company changed.");
        var repository = new PPDMGenericRepository(editor, columns, defaults, metadata, typeof(WELL), database.ConnectionName,
            "WELL", rowAccess: rows, requiredReadColumns: ["UWI"]);
        async Task<string[]> Sample()
        {
            await authority();
            if (await company.ResolveAsync(token: token) != database || company.Company.Scope != scope)
                throw RefusalException.Conflict("The selected company or database changed.");
            var result = await repository.GetScopedSampleAsync(20, token);
            var ids = result.Cast<WELL>().Select(well => well.UWI).ToArray();
            if (ids.Length > 20 || ids.Any(id => string.IsNullOrWhiteSpace(id) || id.Length > 128 || id != id.Trim() || id.Any(char.IsControl)) ||
                ids.Distinct(StringComparer.Ordinal).Count() != ids.Length)
                throw RefusalException.Conflict("The visible well identities could not be confirmed.");
            return ids;
        }
        var first = await Sample(); var second = await Sample();
        await authority();
        if (!first.SequenceEqual(second, StringComparer.Ordinal) || await company.ResolveAsync(token: token) != database || company.Company.Scope != scope)
            throw RefusalException.Conflict("The visible well selection changed. Try again.");
        return new(scope, second);
    }
}
