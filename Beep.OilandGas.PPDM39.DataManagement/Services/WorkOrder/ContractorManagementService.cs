using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

/// <remarks>
/// OILGAS-CATCH-01: a read that fails reaches the caller. Listing a work order's contractors answered an empty list on a
/// failure — so "has a contractor" read as no — and the qualification check answered "not qualified" with the
/// exception's text as the reason.
/// </remarks>
public partial class ContractorManagementService : IContractorManagementService, ICompanyWorkOrderPersonnelSourceOwner, ICompanyWorkOrderPersonnelAssignmentOwner, ICompanyWorkOrderPersonnelAssignmentReads, ICompanyWorkOrderPersonnelReleaseOwner, ICompanyPersonnelPolicyOwner, ICompanyPersonnelCredentialOwner, ICompanyPersonnelAcceptanceCoreProof
{
    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly WorkOrderDatabaseBinding _database;
    private readonly ILogger<ContractorManagementService> _logger;

    public ContractorManagementService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        Func<Task<(CompanyScope Company,string Actor,AssetDatabaseScope Database)>> resolveDatabase,
        ILogger<ContractorManagementService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _database = new(resolveDatabase ?? throw new ArgumentNullException(nameof(resolveDatabase)));
        _logger              = logger;
    }

    public async Task<ContractorAssignment> AssignContractorAsync(string instanceId,string stepId,string baId,string roleCode,string userId)
    {
        await _database.RequireAsync(userId);
        throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("Use the retained native company/FIELD/work-order personnel assignment command with current membership, source, explicit duty and UTC window.");
    }
    public async Task RemoveContractorAsync(string instanceId,string stepId,string baId,string userId)
    {
        await _database.RequireAsync(userId);
        throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("A project-step partial key cannot remove a work-order personnel assignment. Retain the original native assignment and use a reviewed release workflow.");
    }
    public async Task<List<ContractorAssignment>> GetAssignmentsAsync(string instanceId)
    {
        await _database.RequireAsync();
        throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("An identity-free project-step lookup cannot establish current company/FIELD/work-order personnel assignments.");
    }
    public async Task<ContractorQualificationResult> ValidateContractorAsync(string baId,string woType,string jurisdiction)
    {
        if(string.IsNullOrWhiteSpace(baId) || string.IsNullOrWhiteSpace(woType) || string.IsNullOrWhiteSpace(jurisdiction))throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Invalid("An explicit employee, work-order type and jurisdiction are required.");
        await _database.RequireAsync();
        // BA_LICENSE has no competency-level column or work-order/jurisdiction policy.
        // An unavailable policy is neither a fabricated qualification nor a recorded failure.
        throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("Qualification cannot be confirmed from the legacy licence shortcut. Review company-linked personnel source, current task assignment and recorded competency/jurisdiction evidence.");
    }
    public async Task<bool> HasContractorAssignedAsync(string instanceId)
    {
        var assignments = await GetAssignmentsAsync(instanceId);
        return assignments.Count > 0;
    }

    private async Task<PPDMGenericRepository> BuildRepoAsync(Type? entityType, string tableName)
    {
        var connection = await _database.RequireAsync();
        return new(_editor, _commonColumnHandler, _defaults, _metadata, entityType, connection, tableName);
    }
}
