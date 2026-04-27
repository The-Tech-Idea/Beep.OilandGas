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
    private readonly string _connectionName;
    private readonly ILogger<FacilityManagementService>? _logger;

    public FacilityManagementService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<FacilityManagementService>? logger = null)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
        _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        _logger = logger;
    }

    private PPDMGenericRepository Repo<T>(string tableName) =>
        new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(T), _connectionName, tableName, null);

    private async Task<FACILITY?> ResolveFacilityRowAsync(string facilityId, string? facilityType, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var repo = Repo<FACILITY>("FACILITY");
        var formattedId = _defaults.FormatIdForTable("FACILITY", facilityId);
        var byId = await repo.GetByIdAsync(formattedId) as FACILITY;
        if (byId != null && byId.ACTIVE_IND == "Y" &&
            (string.IsNullOrWhiteSpace(facilityType) || string.Equals(byId.FACILITY_TYPE, facilityType, StringComparison.Ordinal)))
            return byId;

        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = facilityId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        if (!string.IsNullOrWhiteSpace(facilityType))
            filters.Add(new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = facilityType });

        var rows = (await repo.GetAsync(filters)).Cast<FACILITY>().ToList();
        return rows.OrderByDescending(f => f.ROW_CHANGED_DATE ?? f.ROW_CREATED_DATE).FirstOrDefault();
    }

    public async Task<IReadOnlyList<FACILITY>> ListFacilitiesAsync(string? primaryFieldId, CancellationToken cancellationToken = default)
    {
        var repo = Repo<FACILITY>("FACILITY");
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

    public async Task<FACILITY> CreateFacilityAsync(FACILITY facility, string userId, CancellationToken cancellationToken = default)
    {
        if (facility == null) throw new ArgumentNullException(nameof(facility));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID is required.", nameof(userId));

        facility.ACTIVE_IND ??= "Y";
        if (facility is IPPDMEntity e)
            _commonColumnHandler.PrepareForInsert(e, userId);

        var repo = Repo<FACILITY>("FACILITY");
        var created = await repo.InsertAsync(facility, userId) as FACILITY
                      ?? throw new InvalidOperationException("Insert did not return a FACILITY row.");

        if (!string.IsNullOrWhiteSpace(created.PRIMARY_FIELD_ID))
            await TryInsertFacilityFieldLinkAsync(created, userId, cancellationToken).ConfigureAwait(false);

        return created;
    }

    private async Task TryInsertFacilityFieldLinkAsync(FACILITY created, string userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (await _metadata.GetTableMetadataAsync("FACILITY_FIELD") == null)
            return;

        var linkRepo = Repo<FACILITY_FIELD>("FACILITY_FIELD");
        var link = new FACILITY_FIELD
        {
            FACILITY_ID = created.FACILITY_ID,
            FACILITY_TYPE = created.FACILITY_TYPE,
            FIELD_ID = created.PRIMARY_FIELD_ID,
            ACTIVE_IND = "Y"
        };
        if (link is IPPDMEntity e)
            _commonColumnHandler.PrepareForInsert(e, userId);
        try
        {
            await linkRepo.InsertAsync(link, userId);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "FACILITY_FIELD link insert skipped for facility {Id}", created.FACILITY_ID);
        }
    }

    public async Task UpdateFacilityAsync(FACILITY facility, string userId, CancellationToken cancellationToken = default)
    {
        if (facility == null) throw new ArgumentNullException(nameof(facility));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID is required.", nameof(userId));

        var repo = Repo<FACILITY>("FACILITY");
        await repo.UpdateAsync(facility, userId);
    }

    public async Task<IReadOnlyList<FACILITY_CLASS>> ListFacilityClassesAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<FACILITY_CLASS>();

        var repo = Repo<FACILITY_CLASS>("FACILITY_CLASS");
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
        if (string.IsNullOrWhiteSpace(facilityClassType)) throw new ArgumentException("Facility class type is required.", nameof(facilityClassType));
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Facility not found.");

        var repo = Repo<FACILITY_CLASS>("FACILITY_CLASS");
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

        var repo = Repo<FACILITY_COMPONENT>("FACILITY_COMPONENT");
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
        var repo = Repo<FACILITY_COMPONENT>("FACILITY_COMPONENT");
        return (await repo.InsertAsync(component, userId) as FACILITY_COMPONENT)!;
    }

    public async Task<IReadOnlyList<FACILITY_STATUS>> ListFacilityStatusHistoryAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<FACILITY_STATUS>();

        var repo = Repo<FACILITY_STATUS>("FACILITY_STATUS");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        return (await repo.GetAsync(filters)).Cast<FACILITY_STATUS>()
            .OrderByDescending(s => s.EFFECTIVE_DATE ?? s.ROW_CREATED_DATE).ToList();
    }

    public async Task<FACILITY_STATUS> AddFacilityStatusAsync(FACILITY_STATUS status, string userId, bool enforceActiveLicenseForOperationalStatus = true, CancellationToken cancellationToken = default)
    {
        if (status == null) throw new ArgumentNullException(nameof(status));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID is required.", nameof(userId));

        if (enforceActiveLicenseForOperationalStatus && StatusImpliesOperational(status))
        {
            var ok = await FacilityHasActiveLicenseAsync(
                status.FACILITY_ID ?? string.Empty, status.FACILITY_TYPE, DateTime.UtcNow.Date, cancellationToken).ConfigureAwait(false);
            if (!ok)
                throw new InvalidOperationException("Operational/active status requires at least one active facility license.");
        }

        if (string.IsNullOrWhiteSpace(status.STATUS_ID))
            status.STATUS_ID = Guid.NewGuid().ToString("N");

        if (status is IPPDMEntity e)
            _commonColumnHandler.PrepareForInsert(e, userId);
        var repo = Repo<FACILITY_STATUS>("FACILITY_STATUS");
        return (await repo.InsertAsync(status, userId) as FACILITY_STATUS)!;
    }

    private static bool StatusImpliesOperational(FACILITY_STATUS s)
    {
        var a = (s.STATUS ?? string.Empty).ToUpperInvariant();
        var t = (s.STATUS_TYPE ?? string.Empty).ToUpperInvariant();
        return a.Contains("ACTIVE", StringComparison.Ordinal) || a.Contains("OPER", StringComparison.Ordinal)
               || t.Contains("ACTIVE", StringComparison.Ordinal) || t.Contains("OPER", StringComparison.Ordinal);
    }

    public async Task<IReadOnlyList<FACILITY_RATE>> ListFacilityRatesAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<FACILITY_RATE>();

        var repo = Repo<FACILITY_RATE>("FACILITY_RATE");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        return (await repo.GetAsync(filters)).Cast<FACILITY_RATE>().OrderByDescending(r => r.EFFECTIVE_DATE).ToList();
    }
}
