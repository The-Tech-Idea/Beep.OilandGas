using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.DataBase;
using System.Text.Json;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
public class HSEIncidentService : IHSEService
{
    private const string PrimaryDetailType = "PRIMARY";
    private const string PrimaryFieldComponentRole = HseReferenceCodes.FieldContext.PrimaryRole;
    private const string PrimaryFieldComponentType = HseReferenceCodes.FieldContext.ComponentType;
    private const string InvestigatorRole = HseReferenceCodes.Investigator.Role;
    private const string StatusResponseType = "STATUS";
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ICompanyFieldSelectionService _selections;
    private readonly IPPDMRowAccessPolicy _rowAccess;
    private readonly ICompanyIncidentCommands? _commands;
    private FieldSelectionReview? _review;
    private readonly ILogger<HSEIncidentService> _logger;
    private static readonly Dictionary<string, List<string>> _transitions = new()
    {
        [IncidentState.Reported] = new()
        {
            "investigate"
        },
        [IncidentState.UnderInvestigation] = new()
        {
            "rca_start",
            "cancel"
        },
        [IncidentState.PendingRCA] = new()
        {
            "rca_complete",
            "cancel"
        },
        [IncidentState.PendingCorrectiveAction] = new()
        {
            "cancel"
        },
        [IncidentState.PendingCloseOut] = new(),
        [IncidentState.Closed] = new List<string>(),
        [IncidentState.Cancelled] = new List<string>(),
    };
    private static readonly Dictionary<string, (string From, string To)> _triggerMap = new()
    {
        ["investigate"] = (IncidentState.Reported, IncidentState.UnderInvestigation),
        ["rca_start"] = (IncidentState.UnderInvestigation, IncidentState.PendingRCA),
        ["rca_complete"] = (IncidentState.PendingRCA, IncidentState.PendingCorrectiveAction),
        ["cancel"] = ("ANY", IncidentState.Cancelled),
    };
    public HSEIncidentService(IDMEEditor editor, ICommonColumnHandler commonColumnHandler, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, ICompanyFieldSelectionService selections, IPPDMRowAccessPolicy rowAccess, ILogger<HSEIncidentService> logger, ICompanyIncidentCommands? commands = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _selections = selections ?? throw new ArgumentNullException(nameof(selections));
        _rowAccess = rowAccess ?? throw new ArgumentNullException(nameof(rowAccess));
        _logger = logger;
        _commands = commands;
    }

    // ── Create ─────────────────────────────────────────────────────────────────
    // The existing profile owns authority; this is only a fence for one phase request.
    private async Task<string> RequireCurrentBindingAsync(string? fieldId = null, string? actor = null, bool requireField = true)
    {
        var current = await _selections.GetCurrentFieldSelectionAsync();
        if (current.SelectionContext is not { } review)
            throw RefusalException.Conflict("The company FIELD review is unavailable.");
        FieldSelectionReviewValidator.RequireCurrent(current, review.Company, review.ActorUserId);
        if (requireField && !current.SelectionAvailable || fieldId is not null && fieldId != current.FieldId || actor is not null && actor != review.ActorUserId || _review is not null && _review != review)
            throw RefusalException.Conflict("The HSE account, company, database or FIELD differs from its reviewed request.");
        _review = review;
        return review.Database.ConnectionName;
    }

    private async Task<T> ConfirmAsync<T>(T result)
    {
        await RequireCurrentBindingAsync();
        return result;
    }

    private async Task ConfirmAsync()
    {
        await RequireCurrentBindingAsync();
    }

    public async Task<HSEIncidentRecord> ReportIncidentAsync(ReportIncidentRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        ArgumentNullException.ThrowIfNull(request);
        if (request.FieldId != _review!.FieldId)
            throw RefusalException.Conflict("The submitted HSE FIELD differs from the reviewed selection.");
        if (string.IsNullOrWhiteSpace(request.FieldId))
        {
            throw new ArgumentException("Field ID is required", nameof(request));
        }

        if (_commands is null) throw RefusalException.Forbidden("A reviewed incident command owner is required for reporting.");
        var options = await _commands.ReviewReportOptionsAsync(_review!);
        if (options.Context != _review || options.Revision != request.OptionsRevision ||
            !options.Options.Contains(new IncidentReportOption(request.IncidentSetId, request.IncidentType, MapTierToSeverityId(request.Tier))))
            throw RefusalException.Conflict("The recorded incident options changed. Review the incident type and severity again.");
        await RequireCurrentBindingAsync(request.FieldId, userId, true);

        var incidentId = BuildIncidentId(request.FieldId, request.IncidentDate);
        var incidentRepo = await BuildRepoAsync("HSE_INCIDENT");
        var detailRepo = await BuildRepoAsync("HSE_INCIDENT_DETAIL");
        var componentRepo = await BuildRepoAsync("HSE_INCIDENT_COMPONENT");
        var responseRepo = await BuildRepoAsync("HSE_INCIDENT_RESPONSE");
        var header = new HSE_INCIDENT
        {
            INCIDENT_ID = incidentId,
            ACTIVE_IND = "Y",
            EFFECTIVE_DATE = request.IncidentDate,
            INCIDENT_DATE = request.IncidentDate,
            LOST_TIME_IND = request.IncidentType == IncidentType.Injury ? "N" : null,
            REPORTED_BY_BA_ID = userId,
            REPORTED_BY_NAME = userId,
            REPORTED_IND = "Y",
            REMARK = request.Description,
            SOURCE = "Beep.OilandGas.HSE",
            WORK_RELATED_IND = "Y",
        };
        var detail = new HSE_INCIDENT_DETAIL
        {
            INCIDENT_ID = incidentId,
            DETAIL_OBS_NO = 1,
            ACTIVE_IND = "Y",
            DETAIL_TYPE = PrimaryDetailType,
            EFFECTIVE_DATE = request.IncidentDate,
            INCIDENT_DATE = request.IncidentDate,
            INCIDENT_SEVERITY_ID = MapTierToSeverityId(request.Tier),
            INCIDENT_SET_ID = request.IncidentSetId,
            INCIDENT_TYPE_ID = request.IncidentType,
            REMARK = request.Description,
            SOURCE = "Beep.OilandGas.HSE",
        };
        var component = new HSE_INCIDENT_COMPONENT
        {
            INCIDENT_ID = incidentId,
            COMPONENT_OBS_NO = 1,
            ACTIVE_IND = "Y",
            COMPONENT_ROLE = PrimaryFieldComponentRole,
            COMPONENT_TYPE = PrimaryFieldComponentType,
            FIELD_ID = request.FieldId,
            INCIDENT_TYPE = request.IncidentType,
            JURISDICTION = request.Jurisdiction,
            REMARK = request.Location,
        };
        var response = new HSE_INCIDENT_RESPONSE
        {
            INCIDENT_ID = incidentId,
            RESPONSE_OBS_NO = 1,
            ACTIVE_IND = "Y",
            EFFECTIVE_DATE = DateTime.UtcNow,
            REMARK = "Incident reported",
            RESPONSE_RESULT = IncidentState.Reported,
            RESPONSE_TYPE = StatusResponseType,
            SOURCE = "Beep.OilandGas.HSE",
        };
        await incidentRepo.InsertAsync(header, userId);
        await detailRepo.InsertAsync(detail, userId);
        await componentRepo.InsertAsync(component, userId);
        await responseRepo.InsertAsync(response, userId);
        _logger.LogInformation("HSE incident {IncidentId} reported for field {FieldId}", incidentId, request.FieldId);
        return await ConfirmAsync(MapIncidentRecord(header, detail, component, null, response));
    }

    // ── Assign Investigator ────────────────────────────────────────────────────
    public async Task<InvestigatorAssignmentReview> ReviewInvestigatorAssignmentAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(incidentId)) throw RefusalException.Invalid("incidentId is required.");
        if (_commands is null) throw RefusalException.Forbidden("A reviewed incident command owner is required for investigator review.");
        return await ConfirmAsync(await _commands.ReviewInvestigatorAssignmentAsync(_review!, incidentId));
    }

    public async Task AssignInvestigatorAsync(string incidentId, AssignIncidentInvestigatorRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        _ = await GetByIdAsync(incidentId) ?? throw RefusalException.NotFound("The incident is unavailable in the current FIELD.");
        if (_commands is null) throw RefusalException.Forbidden("A reviewed incident command owner is required for investigator assignment.");
        await _commands.AssignInvestigatorAsync(_review!, incidentId, request, userId);
        await ConfirmAsync();
    }

    // ── Query ──────────────────────────────────────────────────────────────────
    public async Task<HseIncidentPage> GetVisibleFieldIncidentPageAsync(string fieldId, int pageNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!LeaseSummaryValidator.Identifier(fieldId) || pageNumber < 1)
            throw RefusalException.Invalid("Select a recorded FIELD and positive incident page.");
        await RequireCurrentBindingAsync(fieldId, null, true);
        var context = _review!;
        var source = _editor.GetDataSource(context.Database.ConnectionName)
            ?? throw RefusalException.Conflict("The current incident source is unavailable.");
        if (source is not IBoundedPagedDataSource pages || _rowAccess is not IPPDMFilteredReadPolicy policy)
            throw RefusalException.Conflict("The configured connector and company policy must support bounded authorized incident reads.");
        async Task Guard()
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RequireCurrentBindingAsync(fieldId, null, true);
            if (!ReferenceEquals(source, _editor.GetDataSource(context.Database.ConnectionName)))
                throw RefusalException.Conflict("The incident source changed during page review.");
        }
        async Task<string[]> VisibleKeys()
        {
            await Guard();
            var groups = await policy.GetReadConjunctionsForFiltersAsync(typeof(HSE_INCIDENT_COMPONENT), nameof(HSE_INCIDENT_COMPONENT),
                [new() { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId }]);
            if (groups.Count > 256 || groups.Any(group => group.Count != 2 ||
                group.Count(filter => filter.FieldName == "FIELD_ID" && filter.Operator == "=" && filter.FilterValue == fieldId) != 1 ||
                group.Count(filter => filter.FieldName == "INCIDENT_ID" && filter.Operator == "=" && LeaseSummaryValidator.Identifier(filter.FilterValue)) != 1))
                throw RefusalException.Conflict("The policy did not supply exact authorized incident keys for the selected FIELD.");
            var keys = groups.Select(group => group.Single(filter => filter.FieldName == "INCIDENT_ID").FilterValue).Order(StringComparer.Ordinal).ToArray();
            if (keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
                throw RefusalException.Conflict("The authorized incident keys are ambiguous.");
            await Guard(); return keys;
        }
        var keys = await VisibleKeys();
        var offset = ((long)pageNumber - 1) * HseIncidentPage.PageSize;
        if (pageNumber > 1 && offset >= keys.Length)
            throw RefusalException.Conflict("The incident page is outside the current visible population. Review the first page.");
        var window = keys.Skip((int)offset).Take(HseIncidentPage.PageSize).ToArray();
        if (window.Length > 0)
        {
            var request = new BoundedPageRequest(1, window.Length, 256 * 1024,
                [new PageOrder("INCIDENT_ID")], [new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }],
                window.Select(id => new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = id }));
            await Guard();
            var response = await PPDMDataSourceTransaction.WithSourceAsync(source, () => pages.ReadPageAsync(nameof(HSE_INCIDENT), request, cancellationToken));
            await Guard();
            if (response is null || response.RequestId != request.RequestId || response.PageNumber != 1 || response.PageSize != window.Length ||
                response.TotalRecords != window.Length || response.PayloadBytes > request.MaxPayloadBytes || source.ErrorObject?.Flag == TheTechIdea.Beep.ConfigUtil.Errors.Failed)
                throw RefusalException.Conflict("The provider did not confirm the exact bounded incident-key window.");
            var rows = JsonSerializer.Deserialize<HSE_INCIDENT[]>(response.CopyPayload())
                ?? throw RefusalException.Conflict("The incident page payload is unavailable.");
            _ = new ProviderPageInfo(request, response.TotalRecords, rows.Length, response.PayloadBytes);
            if (rows.Length != window.Length || rows.Any(row => row is null || row.ACTIVE_IND != "Y") ||
                !rows.Select(row => row.INCIDENT_ID).Order(StringComparer.Ordinal).SequenceEqual(window))
                throw RefusalException.Forbidden("The provider returned a different incident population before private detail reads.");
        }
        var closingKeys = await VisibleKeys();
        if (!keys.SequenceEqual(closingKeys, StringComparer.Ordinal))
            throw RefusalException.Conflict("Incident visibility changed during page review. Review again.");
        await Guard();
        var result = new HseIncidentPage(context, pageNumber, keys.Length, window); result.Require(); return result;
    }

    public async Task<List<HSEIncidentRecord>> GetFieldIncidentsAsync(string fieldId, DateRangeFilter? range)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            throw RefusalException.Invalid("fieldId is required.");
        await RequireCurrentBindingAsync(fieldId, null, true);
        var componentRepo = await BuildRepoAsync("HSE_INCIDENT_COMPONENT");
        var componentFilters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "FIELD_ID",
                Operator = "=",
                FilterValue = fieldId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var componentRows = await componentRepo.GetAsync(componentFilters);
        if (componentRows.Any(row => row is not HSE_INCIDENT_COMPONENT component || string.IsNullOrWhiteSpace(component.INCIDENT_ID)))
            throw RefusalException.Conflict("The FIELD incident associations could not be confirmed.");
        var incidentIds = componentRows.Cast<HSE_INCIDENT_COMPONENT>().Select(component => component.INCIDENT_ID).ToHashSet(StringComparer.Ordinal);
        if (incidentIds.Count == 0)
        {
            return await ConfirmAsync(new List<HSEIncidentRecord>());
        }

        var incidentFilters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        if (range is not null)
        {
            incidentFilters.Add(new AppFilter { FieldName = "INCIDENT_DATE", Operator = ">=", FilterValue = range.Start.ToString("yyyy-MM-dd") });
            incidentFilters.Add(new AppFilter { FieldName = "INCIDENT_DATE", Operator = "<=", FilterValue = range.End.ToString("yyyy-MM-dd") });
        }

        var incidentRepo = await BuildRepoAsync("HSE_INCIDENT");
        var incidentRows = await incidentRepo.GetAsync(incidentFilters);
        if (incidentRows.Any(row => row is not HSE_INCIDENT incident || string.IsNullOrWhiteSpace(incident.INCIDENT_ID)))
            throw RefusalException.Conflict("The incident headers could not be confirmed.");
        var incidents = incidentRows.Cast<HSE_INCIDENT>().Where(incident => incidentIds.Contains(incident.INCIDENT_ID)).OrderByDescending(incident => incident.INCIDENT_DATE ?? DateTime.MinValue).ToList();
        var records = new List<HSEIncidentRecord>(incidents.Count);
        foreach (var incident in incidents)
        {
            var record = await BuildIncidentRecordAsync(incident.INCIDENT_ID);
            if (record is null || record.IncidentId != incident.INCIDENT_ID || record.FieldId != fieldId)
                throw RefusalException.Conflict("An incident changed or differs from the reviewed FIELD during the read.");
            records.Add(record);
        }

        return await ConfirmAsync(records);
    }

    public async Task<HSEIncidentRecord?> GetByIdAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var record = await BuildIncidentRecordAsync(incidentId);
        if (record is not null && (record.IncidentId != incidentId || record.FieldId != _review!.FieldId))
            throw RefusalException.NotFound("The incident is outside the reviewed HSE FIELD.");
        return await ConfirmAsync(record);
    }

    // ── Update Tier ────────────────────────────────────────────────────────────
    public async Task UpdateTierAsync(string incidentId, int tier, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        _ = await GetByIdAsync(incidentId) ?? throw RefusalException.NotFound("The incident is unavailable in the current FIELD.");
        var existing = await GetPrimaryDetailAsync(incidentId) ?? throw new InvalidOperationException($"Incident {incidentId} not found");
        existing.INCIDENT_SEVERITY_ID = MapTierToSeverityId(tier);
        var repo = await BuildRepoAsync("HSE_INCIDENT_DETAIL");
        await repo.UpdateAsync(existing, userId);
        await ConfirmAsync();
    }

    // ── Transition ─────────────────────────────────────────────────────────────
    public async Task<bool> TransitionAsync(string incidentId, string trigger, string? reason, string userId)
    {
        if (trigger is "close" or "ca_done")
            throw RefusalException.Forbidden("Incident verification and closure require reviewed lifecycle commands.");
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var incident = await GetByIdAsync(incidentId) ?? throw new InvalidOperationException($"Incident {incidentId} not found");
        if (!_triggerMap.TryGetValue(trigger, out var transition))
            return await ConfirmAsync(false);
        if (transition.From != "ANY" && incident.CurrentState != transition.From)
            return await ConfirmAsync(false);
        var responseRepo = await BuildRepoAsync("HSE_INCIDENT_RESPONSE");
        var nextObsNo = await GetNextResponseObsNoAsync(incidentId);
        var response = new HSE_INCIDENT_RESPONSE
        {
            INCIDENT_ID = incidentId,
            RESPONSE_OBS_NO = nextObsNo,
            ACTIVE_IND = "Y",
            EFFECTIVE_DATE = DateTime.UtcNow,
            REMARK = string.IsNullOrWhiteSpace(reason) ? trigger : reason,
            RESPONSE_RESULT = transition.To,
            RESPONSE_TYPE = StatusResponseType,
            SOURCE = "Beep.OilandGas.HSE",
        };
        await responseRepo.InsertAsync(response, userId);
        _logger.LogInformation("Incident {IncidentId} transitioned via {Trigger} to {State}", incidentId, trigger, transition.To);
        return await ConfirmAsync(true);
    }

    public async Task<List<string>> GetAvailableTriggersAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var incident = await GetByIdAsync(incidentId);
        if (incident == null)
            return await ConfirmAsync(new List<string>());
        if (_transitions.TryGetValue(incident.CurrentState, out var triggers))
            return await ConfirmAsync(new List<string>(triggers));
        return await ConfirmAsync(new List<string>());
    }

    // ── Internal helpers ───────────────────────────────────────────────────────
    private async Task<HSEIncidentRecord?> BuildIncidentRecordAsync(string incidentId)
    {
        var header = await GetIncidentHeaderAsync(incidentId);
        if (header is null)
        {
            return null;
        }

        var component = await GetPrimaryFieldComponentAsync(incidentId);
        var detail = await GetPrimaryDetailAsync(incidentId);
        var investigator = await GetInvestigatorAsync(incidentId);
        var response = await GetLatestStatusResponseAsync(incidentId);
        var result = MapIncidentRecord(header, detail, component, investigator, response);
        if (header.SOURCE == "Beep.OilandGas.HSE" && incidentId.Length == 36 && incidentId.StartsWith("HSE-", StringComparison.Ordinal) &&
            Guid.TryParseExact(incidentId[4..], "N", out var intentId) && incidentId == "HSE-" + intentId.ToString("N").ToUpperInvariant())
        {
            if (_commands is null || !string.IsNullOrEmpty(component?.JURISDICTION))
                throw RefusalException.Conflict("The original incident jurisdiction is unavailable or ambiguous.");
            result.Jurisdiction = await _commands.ReadReportedJurisdictionAsync(_review!, incidentId);
            await ConfirmAsync();
        }
        return result;
    }

    private async Task<HSE_INCIDENT?> GetIncidentHeaderAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT");
        return await repo.GetByIdAsync(incidentId) as HSE_INCIDENT;
    }

    private async Task<HSE_INCIDENT_DETAIL?> GetPrimaryDetailAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_DETAIL");
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_ID",
                Operator = "=",
                FilterValue = incidentId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        filters.Add(new AppFilter { FieldName = "DETAIL_TYPE", Operator = "=", FilterValue = PrimaryDetailType });
        var primary = (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_DETAIL>().ToArray();
        if (primary.Length != 1) throw RefusalException.Conflict("The incident primary detail is unavailable or ambiguous.");
        return primary[0];
    }

    private async Task<HSE_INCIDENT_COMPONENT?> GetPrimaryFieldComponentAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_COMPONENT");
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_ID",
                Operator = "=",
                FilterValue = incidentId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        filters.Add(new AppFilter { FieldName = "COMPONENT_ROLE", Operator = "=", FilterValue = PrimaryFieldComponentRole });
        var primary = (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_COMPONENT>().ToArray();
        if (primary.Length != 1 || primary[0].COMPONENT_TYPE != PrimaryFieldComponentType || primary[0].FIELD_ID != _review!.FieldId)
            throw RefusalException.Conflict("The incident primary FIELD context is unavailable or ambiguous.");
        return primary[0];
    }

    private async Task<HSE_INCIDENT_BA?> GetInvestigatorAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_BA");
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_ID",
                Operator = "=",
                FilterValue = incidentId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var investigators = (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_BA>().Where(ba => ba.INVOLVED_BA_ROLE == InvestigatorRole).ToArray();
        if (investigators.Length > 1) throw RefusalException.Conflict("The current recorded investigator is ambiguous.");
        return investigators.SingleOrDefault();
    }

    private async Task<HSE_INCIDENT_RESPONSE?> GetLatestStatusResponseAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_RESPONSE");
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_ID",
                Operator = "=",
                FilterValue = incidentId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        return (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_RESPONSE>().Where(response => string.Equals(response.RESPONSE_TYPE, StatusResponseType, StringComparison.OrdinalIgnoreCase)).OrderByDescending(response => response.EFFECTIVE_DATE ?? DateTime.MinValue).ThenByDescending(response => response.RESPONSE_OBS_NO).FirstOrDefault();
    }

    private async Task<decimal> GetNextResponseObsNoAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_RESPONSE");
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_ID",
                Operator = "=",
                FilterValue = incidentId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var maxObsNo = (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_RESPONSE>().Select(response => response.RESPONSE_OBS_NO).DefaultIfEmpty(0).Max();
        return maxObsNo + 1;
    }

    private static HSEIncidentRecord MapIncidentRecord(HSE_INCIDENT header, HSE_INCIDENT_DETAIL? detail, HSE_INCIDENT_COMPONENT? component, HSE_INCIDENT_BA? investigator, HSE_INCIDENT_RESPONSE? response)
    {
        return new HSEIncidentRecord
        {
            IncidentId = header.INCIDENT_ID ?? string.Empty,
            FieldId = component?.FIELD_ID ?? string.Empty,
            IncidentType = detail?.INCIDENT_TYPE_ID ?? component?.INCIDENT_TYPE ?? string.Empty,
            Tier = MapSeverityIdToTier(detail?.INCIDENT_SEVERITY_ID),
            IncidentDate = header.INCIDENT_DATE ?? detail?.INCIDENT_DATE ?? header.EFFECTIVE_DATE ?? throw RefusalException.Conflict("The recorded incident date is unavailable."),
            Location = component?.REMARK ?? string.Empty,
            Description = detail?.REMARK ?? header.REMARK ?? string.Empty,
            Jurisdiction = component?.JURISDICTION ?? string.Empty,
            CurrentState = response?.RESPONSE_RESULT ?? throw RefusalException.Conflict("The recorded incident state is unavailable."),
            InvestigatorId = investigator?.INVOLVED_PARTY_BA_ID,
        };
    }

    private static string BuildIncidentId(string fieldId, DateTime incidentDate)
    {
        var seq = DateTime.UtcNow.Ticks % 10000;
        return $"HSE-{fieldId}-{incidentDate:yyyyMMdd}-{seq:D4}";
    }

    private static string MapTierToSeverityId(int tier)
    {
        if (tier < 1 || tier > 4) throw RefusalException.Invalid("HSE tier must be between 1 and 4.");
        return $"TIER_{tier}";
    }

    private static int MapSeverityIdToTier(string? severityId)
    {
        return severityId switch {
            "TIER_1" => 1, "TIER_2" => 2, "TIER_3" => 3, "TIER_4" => 4,
            _ => throw RefusalException.Conflict("The recorded HSE severity is unavailable or unsupported.")
        };
    }

    private async Task<PPDMGenericRepository> BuildRepoAsync(string tableName)
    {
        var meta = await _metadata.GetTableMetadataAsync(tableName);
        if (meta is null || meta.EntityTypeName != tableName)
            throw RefusalException.Conflict("Canonical HSE metadata is unavailable.");
        var entityType = typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + meta.EntityTypeName) ?? throw RefusalException.Conflict("The canonical HSE entity is unavailable.");
        return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, await RequireCurrentBindingAsync(), tableName, rowAccess: _rowAccess);
    }
}
