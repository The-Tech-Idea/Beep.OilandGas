using System.Globalization;
using System.Reflection;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.LeaseAcquisition.Constants;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.LeaseAcquisition.Services;

public partial class LeaseAcquisitionService
{
    async Task<LeaseSummary> ILeaseAcquisitionService.EvaluateLeaseAsync(string leaseId, string fieldId)
    {
        if (!Identifier(leaseId)) throw RefusalException.Invalid("An exact lease identifier is required.");
        var rows = await ReadLeasesAsync(new() { ["LeaseId"] = leaseId, ["FieldId"] = fieldId });
        if (rows.Count != 1) throw RefusalException.NotFound("No unique lease is available under current company and FIELD access.");
        return rows[0]; // Stored facts, never fixed opportunity scores or an inferred approval.
    }
    Task<List<LeaseSummary>> ILeaseAcquisitionService.GetAvailableLeasesAsync(Dictionary<string, string>? filters) => ReadLeasesAsync(filters);

    async Task<LeaseCreationContext> ILeaseAcquisitionService.GetCreationContextAsync(string fieldId)
    {
        if (!Identifier(fieldId)) throw RefusalException.Invalid("Select an exact FIELD before reviewing a creation intent.");
        var scope = await _leaseAccess.ResolveAsync();
        await _leaseAccess.RequireFieldAsync(scope, fieldId, "WRITE", scope.ActorUserId);
        return CreationContext(scope, fieldId);
    }

    async Task<LeaseCreationReceipt> ILeaseAcquisitionService.ReconcileLeaseCreationAsync(CreateLeaseAcquisition request, string userId)
    {
        ArgumentNullException.ThrowIfNull(request); request = request.Snapshot();
        var (scope, proofs) = await PrepareCreationAsync(request, userId);
        var recorded = await CreationRecordedAsync(scope, request.PropertyId!, proofs);
        return new(request.CommandId, recorded ? "Recorded" : "NotRecorded", recorded ? request.CommandId.ToString("N") : null, request.Context!);
    }

    async Task<string> ILeaseAcquisitionService.CreateLeaseAcquisitionAsync(CreateLeaseAcquisition request, string userId)
    {
        ArgumentNullException.ThrowIfNull(request); request = request.Snapshot();
        var (scope, proofs) = await PrepareCreationAsync(request, userId);
        var field = request.PropertyId!; var id = request.CommandId.ToString("N");
        if (await CreationRecordedAsync(scope, field, proofs)) return id;
        var rows = CreationRows(scope, request);
        var inserted = false;
        await _leaseAccess.ExecuteCommandAsync(scope, field, async () => {
            // Recheck after acquiring the existing datasource transaction lane.
            if (await CreationRecordedAsync(scope, field, proofs)) return;
            inserted = true;
            foreach (var row in rows)
            {
                await _leaseAccess.RequireFieldAsync(scope, field, "WRITE", userId);
                await Repository(scope, row, field, id, creating: true).InsertAsync(row, scope.ActorUserId);
                await _leaseAccess.RequireCurrentAsync(scope);
            }
        }, async () => {
            if (inserted) { await ConfirmWritesAsync(scope, proofs); return; }
            if (!await CreationRecordedAsync(scope, field, proofs))
                throw RefusalException.Conflict("Lease registration is not confirmed. Reconcile this same creation intent before another write.");
        });
        return id;
    }

    private async Task<(LeaseDataScope Scope, List<LeaseWriteProof> Proofs)> PrepareCreationAsync(CreateLeaseAcquisition request, string userId)
    {
        if (request.CommandId == Guid.Empty || request.Context is null)
            throw RefusalException.Invalid("A stable command ID and reviewed creation context are required.");
        if (request.WorkingInterest != 0 || request.NetRevenueInterest != 0 || request.RoyaltyRate != 0 || request.MineralOwnerBaId is not null || request.SurfaceOwnerBaId is not null ||
            request.GovernmentAgency is not null || request.IsFederal is not null || request.IsIndian is not null)
            throw RefusalException.Invalid("Economic interests, owner and government terms need their canonical domain workflow; core lease registration cannot discard them.");
        if (!Identifier(request.PropertyId) || !Identifier(request.LeaseNumber) || string.IsNullOrWhiteSpace(request.LeaseName) || request.LeaseName.Length > 1000 ||
            !Identifier(request.LeaseType) || request.EffectiveDate == default || request.ExpirationDate <= request.EffectiveDate ||
            request.PrimaryTermMonths is <= 0 || !request.CalculatedInterest.HasValue)
            throw RefusalException.Invalid("Provide the exact FIELD, lease number/type, name, explicit effective date, term and recorded calculated-interest value.");
        var scope = await _leaseAccess.ResolveAsync(); var field = request.PropertyId!;
        await _leaseAccess.RequireFieldAsync(scope, field, "WRITE", userId);
        if (request.Context != CreationContext(scope, field))
            throw RefusalException.Conflict("The creation intent does not match the current company, database, account, FIELD or lease interpretation. Review the original intent.");
        await _leaseAccess.ValidateCodesAsync(scope, request.LeaseType, LeaseReferenceCodes.Active);
        var proofs = new List<LeaseWriteProof>();
        foreach (var row in CreationRows(scope, request)) proofs.Add(await CaptureWriteAsync(scope, row, creating: true));
        return (scope, proofs);
    }

    private static LeaseCreationContext CreationContext(LeaseDataScope scope, string field) => new(scope.Company, scope.Database.Fingerprint,
        scope.ActorUserId, field, scope.LandRightSubtype, scope.CompanyComponentType, scope.StatusType, scope.DateZoneId);

    private object[] CreationRows(LeaseDataScope scope, CreateLeaseAcquisition request)
    {
        var field = request.PropertyId!;
        var effective = StorageDate(scope, request.EffectiveDate);
        var expires = request.ExpirationDate.HasValue ? StorageDate(scope, request.ExpirationDate.Value) : (DateTime?)null;
        var id = request.CommandId.ToString("N");
        var landRight = new LAND_RIGHT { LAND_RIGHT_SUBTYPE = scope.LandRightSubtype, LAND_RIGHT_ID = id, ACTIVE_IND = "Y",
            ACQTN_DATE = effective, EFFECTIVE_DATE = effective, EXPIRY_DATE = expires,
            CASE_SERIAL_NUM = request.LeaseNumber, CALCULATED_INTEREST = request.CalculatedInterest!.Value, REMARK = request.LeaseName };
        _ = _leaseAccess.IsCurrent(scope, landRight); // Validates date interpretation/range even for a future record.
        var agreement = new LAND_AGREEMENT { LAND_RIGHT_SUBTYPE = scope.LandRightSubtype, LAND_RIGHT_ID = id, ACTIVE_IND = "Y",
            EFFECTIVE_DATE = effective, EXPIRY_DATE = expires, LAND_AGREE_TYPE = request.LeaseType, REMARK = request.LeaseName };
        var status = new LAND_STATUS { LAND_RIGHT_SUBTYPE = scope.LandRightSubtype, LAND_RIGHT_ID = id, STATUS_TYPE = scope.StatusType,
            LAND_RIGHT_STATUS = LeaseReferenceCodes.Active, STATUS_SEQ_NO = 1, ACTIVE_IND = "Y", EFFECTIVE_DATE = effective,
            EFFECTIVE_TERM = request.PrimaryTermMonths?.ToString(CultureInfo.InvariantCulture) ?? "", EFFECTIVE_TERM_OUOM = request.PrimaryTermMonths.HasValue ? "MONTH" : "",
            EXPIRY_DATE = expires, REMARK = request.LeaseName };
        var link = new LAND_RIGHT_FIELD { LAND_RIGHT_SUBTYPE = scope.LandRightSubtype, LAND_RIGHT_ID = id, FIELD_ID = field,
            ACTIVE_IND = "Y", EFFECTIVE_DATE = effective, EXPIRY_DATE = expires };
        var association = new LAND_RIGHT_COMPONENT { LAND_RIGHT_SUBTYPE = scope.LandRightSubtype, LAND_RIGHT_ID = id, COMPONENT_OBS_NO = 1,
            LAND_COMPONENT_TYPE = scope.CompanyComponentType, BUSINESS_ASSOCIATE_ID = scope.Company.BusinessAssociateId, FIELD_ID = field,
            ACTIVE_IND = "Y", EFFECTIVE_DATE = effective, EXPIRY_DATE = expires };
        // The original owner persists canonical records; no title/operator approval or economic-interest record is synthesized.
        return [landRight, agreement, status, link, association];
    }

    private async Task<bool> CreationRecordedAsync(LeaseDataScope scope, string field, IReadOnlyList<LeaseWriteProof> proofs)
    {
        var found = 0;
        foreach (var proof in proofs)
        {
            var rows = await _leaseAccess.ReadAsync(scope, proof.Type, proof.Keys);
            if (rows.Length == 0) continue;
            // The status owner can change these two flags after registration. The original status row and all other values must still match.
            var values = proof.Type == typeof(LAND_RIGHT) || proof.Type == typeof(LAND_AGREEMENT)
                ? proof.Values.Where(value => value.Key != "ACTIVE_IND") : proof.Values;
            if (rows.Length != 1 || values.Any(value => !Equals(proof.Type.GetProperty(value.Key)!.GetValue(rows[0]), value.Value)) ||
                proof.Type.GetProperty("ROW_CREATED_BY")!.GetValue(rows[0]) as string != scope.ActorUserId)
                throw RefusalException.Conflict("The creation command identity already has different or ambiguous registration evidence. No new write is authorized.");
            if ((proof.Type == typeof(LAND_RIGHT) || proof.Type == typeof(LAND_AGREEMENT)) &&
                proof.Type.GetProperty("ACTIVE_IND")!.GetValue(rows[0]) as string is not ("Y" or "N"))
                throw RefusalException.Conflict("The recorded lease has an invalid active flag.");
            found++;
        }
        await _leaseAccess.RequireFieldAsync(scope, field, "WRITE", scope.ActorUserId);
        if (found != 0 && found != proofs.Count) throw RefusalException.Conflict("Only part of this creation intent is recorded. Reconcile the original records; do not create a replacement.");
        return found == proofs.Count;
    }

    async Task ILeaseAcquisitionService.UpdateLeaseStatusAsync(string leaseId, string status, string userId, string fieldId)
    {
        if (!Identifier(leaseId) || !LeaseReferenceCodes.IsDefinedLandRightOperationalStatus(status) || status != status.Trim().ToUpperInvariant())
            throw RefusalException.Invalid("Provide an exact lease ID and canonical ACTIVE, INACTIVE, PENDING or TERMINATED status.");
        var scope = await _leaseAccess.ResolveAsync();
        var summaries = await ReadLeasesAsync(new() { ["LeaseId"] = leaseId, ["FieldId"] = fieldId });
        if (summaries.Count != 1) throw RefusalException.NotFound("No unique lease is available under current company and FIELD access.");
        var field = summaries[0].FieldId; await _leaseAccess.RequireFieldAsync(scope, field, "WRITE", userId);
        await _leaseAccess.ValidateCodesAsync(scope, null, status);
        var proofs = new List<LeaseWriteProof>();
        await _leaseAccess.ExecuteCommandAsync(scope, field, async () => {
        var current = await ReadLeasesAsync(new() { ["LeaseId"] = leaseId, ["FieldId"] = field });
        if (current.Count != 1 || current[0].FieldId != field) throw RefusalException.Conflict("The lease FIELD association changed before the command started.");
        var right = (await Read<LAND_RIGHT>(scope, Key(scope, leaseId))).Single();
        var agreements = await Read<LAND_AGREEMENT>(scope, Key(scope, leaseId));
        var histories = await Read<LAND_STATUS>(scope, [..Key(scope, leaseId), Equal("STATUS_TYPE", scope.StatusType)]);
        if (agreements.Count > 1) throw RefusalException.Conflict("The complete lease agreement identity is ambiguous.");
        right.ACTIVE_IND = status == LeaseReferenceCodes.Active ? "Y" : "N";
        foreach (var row in new object[] { right }.Concat(agreements))
        {
            await RequireAssociationAsync(scope, leaseId, field, "WRITE", userId);
            if (row is LAND_AGREEMENT agreement) agreement.ACTIVE_IND = right.ACTIVE_IND;
            proofs.Add(await CaptureWriteAsync(scope, row, creating: false));
            await Repository(scope, row, field, leaseId).UpdateAsync(row, scope.ActorUserId);
            await _leaseAccess.RequireCurrentAsync(scope);
        }
        var latest = histories.Where(row => row.ACTIVE_IND == "Y" && _leaseAccess.IsCurrent(scope, row))
            .OrderByDescending(row => row.EFFECTIVE_DATE).ThenByDescending(row => row.STATUS_SEQ_NO).FirstOrDefault();
        var record = new LAND_STATUS { LAND_RIGHT_SUBTYPE = scope.LandRightSubtype, LAND_RIGHT_ID = leaseId,
            STATUS_TYPE = scope.StatusType, LAND_RIGHT_STATUS = status, STATUS_SEQ_NO = histories.Count == 0 ? 1 : histories.Max(row => row.STATUS_SEQ_NO) + 1,
            ACTIVE_IND = "Y", EFFECTIVE_DATE = StorageDate(scope, DateTime.UtcNow),
            EFFECTIVE_TERM = latest?.EFFECTIVE_TERM ?? "", EFFECTIVE_TERM_OUOM = latest?.EFFECTIVE_TERM_OUOM ?? "", REMARK = right.REMARK };
        await RequireAssociationAsync(scope, leaseId, field, "WRITE", userId);
        proofs.Add(await CaptureWriteAsync(scope, record, creating: true));
        await Repository(scope, record, field, leaseId).InsertAsync(record, scope.ActorUserId);
        await _leaseAccess.RequireCurrentAsync(scope);
        }, () => ConfirmWritesAsync(scope, proofs));
    }

    private async Task<List<LeaseSummary>> ReadLeasesAsync(Dictionary<string, string>? filters)
    {
        var selected = filters is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(filters, StringComparer.Ordinal);
        var allowed = new[] { "FieldId", "PropertyId", "LeaseId", "LeaseNumber", "LeaseName", "Status" };
        if (selected.Any(pair => !allowed.Contains(pair.Key, StringComparer.Ordinal) || string.IsNullOrWhiteSpace(pair.Value) || pair.Value != pair.Value.Trim() || pair.Value.Any(char.IsControl)))
            throw RefusalException.Invalid("Lease filters require supported exact names and nonblank values.");
        selected.TryGetValue("FieldId", out var field); selected.TryGetValue("PropertyId", out var property); selected.TryGetValue("LeaseId", out var leaseId);
        if (field is not null && property is not null && field != property) throw RefusalException.Invalid("The FIELD assertions do not match.");
        field ??= property;
        if (!Identifier(field) || leaseId is not null && !Identifier(leaseId))
            throw RefusalException.Invalid("Select an exact FIELD before requesting lease records.");
        var scope = await _leaseAccess.ResolveAsync();
        if (field is not null) await _leaseAccess.RequireFieldAsync(scope, field, "READ");
        var predicates = new List<AppFilter> { Equal("LAND_RIGHT_SUBTYPE", scope.LandRightSubtype), Equal("BUSINESS_ASSOCIATE_ID", scope.Company.BusinessAssociateId),
            Equal("LAND_COMPONENT_TYPE", scope.CompanyComponentType), Equal("ACTIVE_IND", "Y") };
        if (field is not null) predicates.Add(Equal("FIELD_ID", field));
        if (leaseId is not null) predicates.Add(Equal("LAND_RIGHT_ID", leaseId));
        var components = (await Read<LAND_RIGHT_COMPONENT>(scope, predicates)).Where(row => _leaseAccess.IsCurrent(scope, row)).ToList();
        var result = new List<LeaseSummary>(); var usedFields = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in components.GroupBy(row => row.LAND_RIGHT_ID, StringComparer.Ordinal))
        {
            if (!Identifier(group.Key) || group.Any(row => !Identifier(row.FIELD_ID)) || group.Select(row => row.FIELD_ID).Distinct(StringComparer.Ordinal).Count() != 1)
                throw RefusalException.Conflict("The lease needs a complete unambiguous FIELD association.");
            var resourceField = group.First().FIELD_ID;
            await _leaseAccess.RequireFieldAsync(scope, resourceField, "READ"); usedFields.Add(resourceField);
            var links = await Read<LAND_RIGHT_FIELD>(scope, [..Key(scope, group.Key), Equal("FIELD_ID", resourceField), Equal("ACTIVE_IND", "Y")]);
            if (links.Count != 1 || !_leaseAccess.IsCurrent(scope, links[0])) throw RefusalException.Conflict("The company lease association has no unique current canonical FIELD link.");
            var rights = await Read<LAND_RIGHT>(scope, Key(scope, group.Key));
            if (rights.Count != 1) throw RefusalException.Conflict("The complete company lease identity is missing or ambiguous.");
            var agreements = await Read<LAND_AGREEMENT>(scope, Key(scope, group.Key));
            if (agreements.Count > 1) throw RefusalException.Conflict("The complete lease agreement identity is ambiguous.");
            var history = await Read<LAND_STATUS>(scope, [..Key(scope, group.Key), Equal("STATUS_TYPE", scope.StatusType)]);
            var ordered = history.Where(row => row.ACTIVE_IND == "Y" && _leaseAccess.IsCurrent(scope, row)).OrderByDescending(row => row.EFFECTIVE_DATE).ThenByDescending(row => row.STATUS_SEQ_NO).ToList();
            if (ordered.Count > 1 && ordered[0].EFFECTIVE_DATE == ordered[1].EFFECTIVE_DATE && ordered[0].STATUS_SEQ_NO == ordered[1].STATUS_SEQ_NO)
                throw RefusalException.Conflict("The latest recorded lease status is ambiguous.");
            var right = rights[0]; var agreement = agreements.SingleOrDefault(); var latest = ordered.FirstOrDefault();
            int? term = null;
            if (!string.IsNullOrEmpty(latest?.EFFECTIVE_TERM))
            {
                if (latest.EFFECTIVE_TERM_OUOM != "MONTH" || !int.TryParse(latest.EFFECTIVE_TERM, NumberStyles.None, CultureInfo.InvariantCulture, out var months) || months <= 0)
                    throw RefusalException.Conflict("The recorded lease term has an unsupported or invalid unit/value.");
                term = months;
            }
            var summary = new LeaseSummary { LeaseId = right.LAND_RIGHT_ID, LandRightSubtype = right.LAND_RIGHT_SUBTYPE, DateZoneId = scope.DateZoneId,
                LeaseNumber = right.CASE_SERIAL_NUM ?? "", LeaseName = right.REMARK ?? "",
                Status = latest?.LAND_RIGHT_STATUS ?? "", LeaseType = agreement?.LAND_AGREE_TYPE ?? "", FieldId = resourceField,
                EffectiveDate = right.EFFECTIVE_DATE, ExpirationDate = right.EXPIRY_DATE, PrimaryTermMonths = term, CalculatedInterest = right.CALCULATED_INTEREST };
            if (selected.TryGetValue("LeaseNumber", out var number) && summary.LeaseNumber != number || selected.TryGetValue("LeaseName", out var name) && summary.LeaseName != name ||
                selected.TryGetValue("Status", out var expectedStatus) && summary.Status != expectedStatus) continue;
            result.Add(summary);
        }
        foreach (var row in result) await RequireAssociationAsync(scope, row.LeaseId, row.FieldId, "READ");
        foreach (var resourceField in usedFields) await _leaseAccess.RequireFieldAsync(scope, resourceField, "READ");
        await _leaseAccess.RequireCurrentAsync(scope);
        return result.OrderBy(row => row.LeaseNumber, StringComparer.Ordinal).ThenBy(row => row.LeaseId, StringComparer.Ordinal).ToList();
    }
    private async Task<List<T>> Read<T>(LeaseDataScope scope, List<AppFilter> filters) => (await _leaseAccess.ReadAsync(scope, typeof(T), filters)).Cast<T>().ToList();
    private async Task RequireAssociationAsync(LeaseDataScope scope, string id, string field, string level, string? actor = null)
    {
        var components = await Read<LAND_RIGHT_COMPONENT>(scope, [..Key(scope, id), Equal("BUSINESS_ASSOCIATE_ID", scope.Company.BusinessAssociateId),
            Equal("LAND_COMPONENT_TYPE", scope.CompanyComponentType), Equal("ACTIVE_IND", "Y")]);
        var current = components.Where(row => _leaseAccess.IsCurrent(scope, row)).ToArray();
        if (current.Length == 0 || current.Any(row => row.FIELD_ID != field)) throw RefusalException.Forbidden("Current company lease association no longer authorizes this FIELD record.");
        var links = await Read<LAND_RIGHT_FIELD>(scope, [..Key(scope, id), Equal("FIELD_ID", field), Equal("ACTIVE_IND", "Y")]);
        if (links.Count != 1 || !_leaseAccess.IsCurrent(scope, links[0])) throw RefusalException.Forbidden("The current lease FIELD link no longer authorizes this operation.");
        await _leaseAccess.RequireFieldAsync(scope, field, level, actor);
    }
    private PPDMGenericRepository Repository(LeaseDataScope scope, object row, string field, string id, bool creating = false) =>
        new(_editor, _commonColumnHandler, _defaults, _metadata, row.GetType(), scope.Database.ConnectionName, row.GetType().Name, rowAccess: _leaseAccess.ForMutation(scope, field, id, row, creating));
    private sealed record LeaseWriteProof(Type Type, List<AppFilter> Keys, Dictionary<string, object?> Values, string ActorColumn);
    private async Task<LeaseWriteProof> CaptureWriteAsync(LeaseDataScope scope, object row, bool creating)
    {
        var type = row.GetType(); var keys = (await _metadata.GetPrimaryKeyColumnsAsync(type.Name)).ToArray();
        if (keys.Length == 0 || keys.Any(key => type.GetProperty(key) is null)) throw RefusalException.Conflict("Lease command readback requires complete canonical keys.");
        var filters = keys.Select(key => Equal(key, Convert.ToString(type.GetProperty(key)!.GetValue(row), CultureInfo.InvariantCulture)!)).ToList();
        if (row is LAND_RIGHT_COMPONENT) { filters.Add(Equal("BUSINESS_ASSOCIATE_ID", scope.Company.BusinessAssociateId)); filters.Add(Equal("LAND_COMPONENT_TYPE", scope.CompanyComponentType)); filters.Add(Equal("ACTIVE_IND", "Y")); }
        var values = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(property => property.CanRead && !property.Name.StartsWith("ROW_", StringComparison.Ordinal) && property.Name != "PPDM_GUID")
            .ToDictionary(property => property.Name, property => property.GetValue(row), StringComparer.Ordinal);
        return new(type, filters, values, creating ? "ROW_CREATED_BY" : "ROW_CHANGED_BY");
    }
    private async Task ConfirmWritesAsync(LeaseDataScope scope, IReadOnlyList<LeaseWriteProof> proofs)
    {
        if (proofs.Count == 0) throw RefusalException.Conflict("The lease command produced no persistence evidence.");
        foreach (var proof in proofs)
        {
            var rows = await _leaseAccess.ReadAsync(scope, proof.Type, proof.Keys);
            if (rows.Length != 1 || proof.Values.Any(value => !Equals(proof.Type.GetProperty(value.Key)!.GetValue(rows[0]), value.Value)) ||
                proof.Type.GetProperty(proof.ActorColumn)!.GetValue(rows[0]) as string != scope.ActorUserId)
                throw RefusalException.Conflict("Lease persistence could not be confirmed with the complete identity, requested values and authenticated actor. Refresh before further changes.");
        }
    }
    private static DateTime StorageDate(LeaseDataScope scope, DateTime value) => value.Kind == DateTimeKind.Utc
        ? TimeZoneInfo.ConvertTimeFromUtc(value, TimeZoneInfo.FindSystemTimeZoneById(scope.DateZoneId)) : value;
    private static List<AppFilter> Key(LeaseDataScope scope, string id) => [Equal("LAND_RIGHT_SUBTYPE", scope.LandRightSubtype), Equal("LAND_RIGHT_ID", id)];
    private static AppFilter Equal(string column, string value) => new() { FieldName = column, Operator = "=", FilterValue = value };
    private static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value == value.Trim() && !value.Any(char.IsControl);
}
