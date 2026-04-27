using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService
{
    public async Task<IReadOnlyList<FACILITY_EQUIPMENT>> ListFacilityEquipmentAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<FACILITY_EQUIPMENT>();

        var repo = Repo<FACILITY_EQUIPMENT>("FACILITY_EQUIPMENT");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        return (await repo.GetAsync(filters)).Cast<FACILITY_EQUIPMENT>().OrderBy(e => e.INSTALL_OBS_NO).ToList();
    }

    public async Task<FACILITY_EQUIPMENT> LinkEquipmentToFacilityAsync(string facilityId, string? facilityType, string equipmentId, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(equipmentId)) throw new ArgumentException("Equipment ID is required.", nameof(equipmentId));
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Facility not found.");

        var repo = Repo<FACILITY_EQUIPMENT>("FACILITY_EQUIPMENT");
        var existing = (await repo.GetAsync(new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "EQUIPMENT_ID", Operator = "=", FilterValue = equipmentId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        })).Cast<FACILITY_EQUIPMENT>().ToList();

        var nextObs = existing.Count == 0 ? 1m : existing.Max(x => x.INSTALL_OBS_NO) + 1m;
        var row = new FACILITY_EQUIPMENT
        {
            FACILITY_ID = f.FACILITY_ID,
            FACILITY_TYPE = f.FACILITY_TYPE,
            EQUIPMENT_ID = equipmentId,
            INSTALL_OBS_NO = nextObs,
            ACTIVE_IND = "Y",
            EFFECTIVE_DATE = DateTime.UtcNow.Date
        };
        if (row is IPPDMEntity e)
            _commonColumnHandler.PrepareForInsert(e, userId);
        return (await repo.InsertAsync(row, userId) as FACILITY_EQUIPMENT)!;
    }

    public async Task<IReadOnlyList<FACILITY_MAINTAIN>> ListFacilityMaintenanceAsync(string facilityId, string? facilityType, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<FACILITY_MAINTAIN>();

        var repo = Repo<FACILITY_MAINTAIN>("FACILITY_MAINTAIN");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        var rows = (await repo.GetAsync(filters)).Cast<FACILITY_MAINTAIN>().ToList();
        if (startDate.HasValue)
            rows = rows.Where(r => (r.SCHEDULE_START_DATE ?? r.ROW_CREATED_DATE ?? DateTime.MinValue) >= startDate.Value).ToList();
        if (endDate.HasValue)
            rows = rows.Where(r => (r.SCHEDULE_END_DATE ?? r.ACTUAL_END_DATE ?? r.ROW_CREATED_DATE ?? DateTime.MaxValue) <= endDate.Value).ToList();
        return rows;
    }

    public async Task<FACILITY_MAINTAIN> CreateFacilityMaintenanceAsync(FACILITY_MAINTAIN maintenance, string userId, CancellationToken cancellationToken = default)
    {
        if (maintenance == null) throw new ArgumentNullException(nameof(maintenance));
        if (string.IsNullOrWhiteSpace(maintenance.MAINTAIN_ID))
            maintenance.MAINTAIN_ID = _defaults.FormatIdForTable("FACILITY_MAINTAIN", Guid.NewGuid().ToString("N"));
        maintenance.ACTIVE_IND ??= "Y";
        if (maintenance is IPPDMEntity e)
            _commonColumnHandler.PrepareForInsert(e, userId);
        var repo = Repo<FACILITY_MAINTAIN>("FACILITY_MAINTAIN");
        return (await repo.InsertAsync(maintenance, userId) as FACILITY_MAINTAIN)!;
    }

    public async Task<IReadOnlyList<WORK_ORDER>> ListFacilityWorkOrdersAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<WORK_ORDER>();

        var compRepo = Repo<WORK_ORDER_COMPONENT>("WORK_ORDER_COMPONENT");
        var compFilters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        var components = (await compRepo.GetAsync(compFilters)).Cast<WORK_ORDER_COMPONENT>().ToList();
        var ids = components.Select(c => c.WORK_ORDER_ID).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (ids.Count == 0) return Array.Empty<WORK_ORDER>();

        var woRepo = Repo<WORK_ORDER>("WORK_ORDER");
        var result = new List<WORK_ORDER>();
        foreach (var id in ids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var wo = await woRepo.GetByIdAsync(id!) as WORK_ORDER;
            if (wo != null && wo.ACTIVE_IND == "Y")
                result.Add(wo);
        }
        return result.OrderByDescending(w => w.REQUEST_DATE ?? w.ROW_CREATED_DATE).ToList();
    }

    public async Task<WORK_ORDER> CreateFacilityWorkOrderAsync(WORK_ORDER workOrder, string facilityId, string facilityType, string userId, CancellationToken cancellationToken = default)
    {
        if (workOrder == null) throw new ArgumentNullException(nameof(workOrder));
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Facility not found.");

        if (string.IsNullOrWhiteSpace(workOrder.WORK_ORDER_ID))
            workOrder.WORK_ORDER_ID = Guid.NewGuid().ToString("N");
        workOrder.ACTIVE_IND ??= "Y";
        if (workOrder is IPPDMEntity e)
            _commonColumnHandler.PrepareForInsert(e, userId);

        var woRepo = Repo<WORK_ORDER>("WORK_ORDER");
        var created = await woRepo.InsertAsync(workOrder, userId) as WORK_ORDER
                      ?? throw new InvalidOperationException("Work order insert failed.");

        var comp = new WORK_ORDER_COMPONENT
        {
            WORK_ORDER_ID = created.WORK_ORDER_ID,
            COMPONENT_ID = Guid.NewGuid().ToString("N"),
            FACILITY_ID = f.FACILITY_ID,
            FACILITY_TYPE = f.FACILITY_TYPE,
            ACTIVE_IND = "Y",
            ACTIVITY_OBS_NO = 0
        };
        if (comp is IPPDMEntity ce)
            _commonColumnHandler.PrepareForInsert(ce, userId);
        var compRepo = Repo<WORK_ORDER_COMPONENT>("WORK_ORDER_COMPONENT");
        await compRepo.InsertAsync(comp, userId);
        return created;
    }

    public async Task<string> EnsureFacilityPdenAsync(string facilityId, string? facilityType, string userId, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Facility not found.");

        var linkRepo = Repo<PDEN_FACILITY>("PDEN_FACILITY");
        var existing = (await linkRepo.GetAsync(new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "PDEN_SUBTYPE", Operator = "=", FilterValue = PdenSubtypeFacility },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        })).Cast<PDEN_FACILITY>().FirstOrDefault();

        if (existing != null && !string.IsNullOrWhiteSpace(existing.PDEN_ID))
            return existing.PDEN_ID!;

        var pdenId = Guid.NewGuid().ToString("N");
        var pden = new PDEN
        {
            PDEN_ID = pdenId,
            PDEN_SUBTYPE = PdenSubtypeFacility,
            ACTIVE_IND = "Y",
            CURRENT_STATUS_DATE = DateTime.UtcNow,
            EFFECTIVE_DATE = DateTime.UtcNow,
            ON_PRODUCTION_DATE = DateTime.UtcNow,
            LAST_PRODUCTION_DATE = DateTime.UtcNow,
            PDEN_STATUS = "ACTIVE",
            REMARK = $"Facility PDEN for {f.FACILITY_ID}"
        };
        if (pden is IPPDMEntity pe)
            _commonColumnHandler.PrepareForInsert(pe, userId);
        var pdenRepo = Repo<PDEN>("PDEN");
        await pdenRepo.InsertAsync(pden, userId);

        var link = new PDEN_FACILITY
        {
            PDEN_SUBTYPE = PdenSubtypeFacility,
            PDEN_ID = pdenId,
            PDEN_SOURCE = PdenSourceDefault,
            FACILITY_ID = f.FACILITY_ID,
            FACILITY_TYPE = f.FACILITY_TYPE,
            NO_OF_OIL_WELLS = 0,
            NO_OF_GAS_WELLS = 0,
            NO_OF_INJECTION_WELLS = 0,
            ACTIVE_IND = "Y",
            EFFECTIVE_DATE = DateTime.UtcNow.Date
        };
        if (link is IPPDMEntity le)
            _commonColumnHandler.PrepareForInsert(le, userId);
        await linkRepo.InsertAsync(link, userId);
        return pdenId;
    }

    public async Task<IReadOnlyList<PDEN_VOL_SUMMARY>> ListFacilityProductionVolumesAsync(string facilityId, string? facilityType, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var linkRepo = Repo<PDEN_FACILITY>("PDEN_FACILITY");
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<PDEN_VOL_SUMMARY>();

        var link = (await linkRepo.GetAsync(new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "PDEN_SUBTYPE", Operator = "=", FilterValue = PdenSubtypeFacility },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        })).Cast<PDEN_FACILITY>().FirstOrDefault();

        if (link == null || string.IsNullOrWhiteSpace(link.PDEN_ID))
            return Array.Empty<PDEN_VOL_SUMMARY>();

        var volRepo = Repo<PDEN_VOL_SUMMARY>("PDEN_VOL_SUMMARY");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PDEN_ID", Operator = "=", FilterValue = link.PDEN_ID },
            new AppFilter { FieldName = "PDEN_SUBTYPE", Operator = "=", FilterValue = PdenSubtypeFacility },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        var rows = (await volRepo.GetAsync(filters)).Cast<PDEN_VOL_SUMMARY>()
            .Where(v => v.VOLUME_DATE >= startDate && v.VOLUME_DATE <= endDate)
            .OrderByDescending(v => v.VOLUME_DATE)
            .ToList();
        return rows;
    }

    public async Task<PDEN_VOL_SUMMARY> RecordFacilityProductionVolumeAsync(PDEN_VOL_SUMMARY volume, string userId, CancellationToken cancellationToken = default)
    {
        if (volume == null) throw new ArgumentNullException(nameof(volume));
        if (string.IsNullOrWhiteSpace(volume.PDEN_ID)) throw new ArgumentException("PDEN_ID is required on the volume row.", nameof(volume));
        volume.PDEN_SUBTYPE = PdenSubtypeFacility;
        volume.ACTIVE_IND ??= "Y";
        if (volume is IPPDMEntity e)
            _commonColumnHandler.PrepareForInsert(e, userId);
        var volRepo = Repo<PDEN_VOL_SUMMARY>("PDEN_VOL_SUMMARY");
        return (await volRepo.InsertAsync(volume, userId) as PDEN_VOL_SUMMARY)!;
    }

    public async Task<IReadOnlyList<FACILITY_LICENSE>> ListFacilityLicensesAsync(string facilityId, string? facilityType, CancellationToken cancellationToken = default)
    {
        var f = await ResolveFacilityRowAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (f == null) return Array.Empty<FACILITY_LICENSE>();

        var repo = Repo<FACILITY_LICENSE>("FACILITY_LICENSE");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = f.FACILITY_ID },
            new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = f.FACILITY_TYPE },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        return (await repo.GetAsync(filters)).Cast<FACILITY_LICENSE>().OrderByDescending(l => l.EFFECTIVE_DATE).ToList();
    }

    public async Task<FACILITY_LICENSE> CreateFacilityLicenseAsync(FACILITY_LICENSE license, string userId, CancellationToken cancellationToken = default)
    {
        if (license == null) throw new ArgumentNullException(nameof(license));
        if (string.IsNullOrWhiteSpace(license.LICENSE_ID))
            license.LICENSE_ID = Guid.NewGuid().ToString("N");
        license.ACTIVE_IND ??= "Y";
        if (license is IPPDMEntity e)
            _commonColumnHandler.PrepareForInsert(e, userId);
        var repo = Repo<FACILITY_LICENSE>("FACILITY_LICENSE");
        return (await repo.InsertAsync(license, userId) as FACILITY_LICENSE)!;
    }

    public async Task<bool> FacilityHasActiveLicenseAsync(string facilityId, string? facilityType, DateTime? asOf, CancellationToken cancellationToken = default)
    {
        var licenses = await ListFacilityLicensesAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        var check = asOf?.Date ?? DateTime.UtcNow.Date;
        return licenses.Any(l =>
            string.Equals(l.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase)
            && (!l.EXPIRY_DATE.HasValue || l.EXPIRY_DATE.Value.Date >= check));
    }

    public async Task<(int MaintenanceEvents, int WorkOrders, decimal? EstimatedAvailabilityPercent)> GetFacilityReliabilityMetricsAsync(
        string facilityId, string? facilityType, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var maint = await ListFacilityMaintenanceAsync(facilityId, facilityType, startDate, endDate, cancellationToken).ConfigureAwait(false);
        var wos = await ListFacilityWorkOrdersAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        var woInRange = wos.Count(w =>
            (w.REQUEST_DATE ?? w.ROW_CREATED_DATE ?? DateTime.MinValue) >= startDate
            && (w.REQUEST_DATE ?? w.ROW_CREATED_DATE ?? DateTime.MaxValue) <= endDate);
        decimal? avail = null;
        if (maint.Count + woInRange > 0)
            avail = 100m;
        return (maint.Count, woInRange, avail);
    }
}
