using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM.Models;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdeaWeb.Diagnostics;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.ProductionOperations.Services;

/// <summary>
/// PPDM39-backed facility workflows; returns and persists <see cref="FACILITY"/> and related entities directly.
/// </summary>
public sealed partial class FacilityManagementService : IFacilityManagementService
{
    private const string PdenSubtypeFacility = "FACILITY";
    private const string PdenSourceDefault = "DEFAULT";

    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly IFailureReporter _failures;
    private readonly string _connectionName;
    private readonly ILogger<FacilityManagementService>? _logger;
    private readonly Func<string, Task<string>>? _resolveModuleConnection;
    private readonly IPPDMRowAccessPolicy? _maintenanceAssetAccess;
    private readonly string? _maintenanceEvidenceZone;

    public FacilityManagementService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        IFailureReporter failures,
        string connectionName = "PPDM39",
        ILogger<FacilityManagementService>? logger = null,
        Func<string, Task<string>>? resolveModuleConnection = null,
        IPPDMRowAccessPolicy? maintenanceAssetAccess = null, string? maintenanceEvidenceZone = null)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
        _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        _logger = logger;
        _resolveModuleConnection = resolveModuleConnection;
        _maintenanceAssetAccess=maintenanceAssetAccess;_maintenanceEvidenceZone=maintenanceEvidenceZone;
    }

    private async Task<PPDMGenericRepository> RepoAsync<T>(string tableName)
    {
        var module = typeof(T).Namespace == typeof(FACILITY).Namespace ? "PPDM_CORE" :
            typeof(T) == typeof(Beep.OilandGas.Models.Data.ProductionOperations.FACILITY_MEASUREMENT) ||
            typeof(T) == typeof(Beep.OilandGas.Models.Data.ProductionOperations.FACILITY_EQUIPMENT_ACTIVITY)
                ? "FACILITY" : throw new InvalidOperationException("Unknown facility storage owner.");
        var connection = _resolveModuleConnection is null ? _connectionName : await _resolveModuleConnection(module);
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("A module database binding is required.");
        return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(T), connection, tableName, null);
    }

    private async Task<FACILITY?> ResolveFacilityRowAsync(string facilityId, string? facilityType, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value == value.Trim() && !value.Any(char.IsControl);
        if (!Identifier(facilityId) || !Identifier(facilityType))
            throw RefusalException.Invalid("The original FACILITY_ID and FACILITY_TYPE are both required.");
        var definition = await _metadata.GetTableMetadataAsync("FACILITY");
        if (definition?.EntityTypeName != nameof(FACILITY) || !new[] { "FACILITY_ID", "FACILITY_TYPE" }.SequenceEqual(
            (definition.PrimaryKeyColumn ?? "").Split(',').Select(key => key.Trim()).Order(StringComparer.Ordinal)))
            throw RefusalException.Conflict("Canonical FACILITY metadata must preserve both native identity keys.");
        var connection = _resolveModuleConnection is null ? _connectionName : await _resolveModuleConnection("PPDM_CORE");
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("The original facility CORE binding is required.");
        var source = _editor.GetDataSource(connection) ?? throw new InvalidOperationException("The original facility datasource is unavailable.");
        var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(FACILITY), connection, "FACILITY");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = facilityId },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = facilityType },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        var rows = (await repo.GetAsync(filters)).ToArray();
        ct.ThrowIfCancellationRequested();
        var closingDefinition = await _metadata.GetTableMetadataAsync("FACILITY");
        if (closingDefinition?.EntityTypeName != nameof(FACILITY) || !new[] { "FACILITY_ID", "FACILITY_TYPE" }.SequenceEqual(
            (closingDefinition.PrimaryKeyColumn ?? "").Split(',').Select(key => key.Trim()).Order(StringComparer.Ordinal)))
            throw RefusalException.Conflict("The original qualified facility metadata changed during its read.");
        if (!ReferenceEquals(source, _editor.GetDataSource(connection)) ||
            (_resolveModuleConnection is not null && await _resolveModuleConnection("PPDM_CORE") != connection))
            throw RefusalException.Conflict("The original facility CORE binding changed during the qualified read.");
        if (rows.Length > 1 || rows.Any(row => row is not FACILITY value || value.FACILITY_ID != facilityId || value.FACILITY_TYPE != facilityType || value.ACTIVE_IND != "Y"))
            throw RefusalException.Conflict("The provider must return one exact active qualified facility, without choosing a latest or first match.");
        return rows.Length == 0 ? null : (FACILITY)rows[0];
    }

    public async Task<IReadOnlyList<FACILITY>> ListFacilitiesAsync(string? primaryFieldId, CancellationToken cancellationToken = default)
    {
        var repo = await RepoAsync<FACILITY>("FACILITY");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        if (!string.IsNullOrWhiteSpace(primaryFieldId))
        {
            var fieldKey = _defaults.FormatIdForTable("FACILITY", primaryFieldId);
            filters.Add(new AppFilter { FieldName = "PRIMARY_FIELD_ID", Operator = "=", FilterValue = fieldKey });
        }

        var list = (await repo.GetAsync(filters)).Cast<FACILITY>().ToList();
        return list.OrderBy(f => f.FACILITY_SHORT_NAME).ToList();
    }

    public Task<FACILITY?> GetFacilityAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default) =>
        ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken);

    public async Task<IReadOnlyList<FACILITY_CLASS>> ListFacilityClassesAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<FACILITY_CLASS>();

        var repo = await RepoAsync<FACILITY_CLASS>("FACILITY_CLASS");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        return (await repo.GetAsync(filters)).Cast<FACILITY_CLASS>().OrderBy(c => c.FACILITY_CLASS_SEQ_NO).ToList();
    }

    public async Task<FACILITY_CLASS> AddFacilityClassAsync(string facilityId, string? facilityType, string facilityClassType, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(facilityClassType)) throw RefusalException.Invalid("Facility class type is required.");
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false)
                ?? throw RefusalException.NotFound("Facility not found.");

        var repo = await RepoAsync<FACILITY_CLASS>("FACILITY_CLASS");
        var existing = (await repo.GetAsync(new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        })).Cast<FACILITY_CLASS>().ToList();

        var nextSeq = existing.Count == 0 ? 1 : existing.Max(x => x.FACILITY_CLASS_SEQ_NO) + 1;
        var row = new FACILITY_CLASS
        {
            FACILITY_ID = f.FACILITY_ID,
            FACILITY_TYPE = f.FACILITY_TYPE,
            FACILITY_CLASS_TYPE = facilityClassType.Trim(),
            FACILITY_CLASS_SEQ_NO = nextSeq,
            ACTIVE_IND = "Y",
            EFFECTIVE_DATE = DateTime.UtcNow.Date
        };
        if (row is IPPDMEntity e)
            _commonColumnHandler.PrepareForInsert(e, userId);
        return (await repo.InsertAsync(row, userId) as FACILITY_CLASS)!;
    }

    public async Task<IReadOnlyList<FACILITY_COMPONENT>> ListFacilityComponentsAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<FACILITY_COMPONENT>();

        var repo = await RepoAsync<FACILITY_COMPONENT>("FACILITY_COMPONENT");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        return (await repo.GetAsync(filters)).Cast<FACILITY_COMPONENT>().OrderBy(c => c.COMPONENT_OBS_NO).ToList();
    }

    public async Task<FACILITY_COMPONENT> AddFacilityComponentAsync(FACILITY_COMPONENT component, string userId, CancellationToken cancellationToken = default)
    {
        if (component == null) throw new ArgumentNullException(nameof(component));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID is required.", nameof(userId));

        if (component is IPPDMEntity e)
            _commonColumnHandler.PrepareForInsert(e, userId);
        var repo = await RepoAsync<FACILITY_COMPONENT>("FACILITY_COMPONENT");
        return (await repo.InsertAsync(component, userId) as FACILITY_COMPONENT)!;
    }

    public async Task<IReadOnlyList<FACILITY_STATUS>> ListFacilityStatusHistoryAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<FACILITY_STATUS>();

        var repo = await RepoAsync<FACILITY_STATUS>("FACILITY_STATUS");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        return (await repo.GetAsync(filters)).Cast<FACILITY_STATUS>()
            .OrderByDescending(s => s.EFFECTIVE_DATE ?? s.ROW_CREATED_DATE).ToList();
    }

    public Task<FACILITY_STATUS> AddFacilityStatusAsync(FACILITY_STATUS status, string userId, bool enforceActiveLicenseForOperationalStatus = true, CancellationToken cancellationToken = default)
    {
        if (status == null) throw new ArgumentNullException(nameof(status));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID is required.", nameof(userId));
        throw RefusalException.Forbidden("Facility status changes require the reviewed company maintenance return command with original closure, policy and native state evidence.");
    }

    public async Task<IReadOnlyList<FACILITY_RATE>> ListFacilityRatesAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<FACILITY_RATE>();

        var repo = await RepoAsync<FACILITY_RATE>("FACILITY_RATE");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        return (await repo.GetAsync(filters)).Cast<FACILITY_RATE>().OrderByDescending(r => r.EFFECTIVE_DATE).ToList();
    }
}
