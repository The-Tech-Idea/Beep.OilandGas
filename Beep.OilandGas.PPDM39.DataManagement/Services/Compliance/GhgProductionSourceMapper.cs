using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Compliance;

/// <summary>Projects recorded FIELD-linked PDEN identities without inferring emissions, legal categories or activity volumes.</summary>
public static class GhgProductionSourceMapper
{
    public static FieldGhgProductionSources Project(FieldSelectionReview field, IEnumerable<PDEN> source, string revision)
    {
        ArgumentNullException.ThrowIfNull(field); ArgumentNullException.ThrowIfNull(source);
        if (string.IsNullOrWhiteSpace(field.FieldId) || !CompanyEnvironmentIdentity.IsRevision(revision)) throw new InvalidOperationException("The FIELD source review is incomplete.");
        var identities = new HashSet<(string, string, string)>(); var result = new List<GhgProductionEntity>();
        foreach (var row in source)
        {
            if (row is null || row.FIELD_ID != field.FieldId || row.ACTIVE_IND != "Y" || !Valid(row.PDEN_SUBTYPE) || !Valid(row.PDEN_ID) || !Valid(row.SOURCE) ||
                !identities.Add((row.PDEN_SUBTYPE, row.PDEN_ID, row.SOURCE)))
                throw new InvalidOperationException("The production entity is foreign, ambiguous or incomplete.");
            result.Add(new(row.PDEN_SUBTYPE, row.PDEN_ID, row.SOURCE, row.PDEN_LONG_NAME, row.PRIMARY_PRODUCT));
        }
        return new(field, result.OrderBy(x => x.Subtype, StringComparer.Ordinal).ThenBy(x => x.ProductionEntityId, StringComparer.Ordinal).ThenBy(x => x.Source, StringComparer.Ordinal).ToArray(), revision);
    }
    public static FieldGhgDispositions ProjectDispositions(FieldSelectionReview field, GhgDispositionQuery query, IEnumerable<PDEN_VOL_DISPOSITION> source, string revision)
    {
        ArgumentNullException.ThrowIfNull(field); ArgumentNullException.ThrowIfNull(query); ArgumentNullException.ThrowIfNull(source);
        if (!Valid(query.Subtype) || !Valid(query.ProductionEntityId) || !Valid(query.Source) || !CompanyEnvironmentIdentity.IsRevision(query.ExpectedSourceRevision) || !CompanyEnvironmentIdentity.IsRevision(revision))
            throw new InvalidOperationException("The production source identity is incomplete.");
        var identities = new HashSet<(string, string, string, string, decimal, decimal)>(); var result = new List<GhgDispositionFact>();
        foreach (var row in source)
        {
            if (row is null || row.PDEN_SUBTYPE != query.Subtype || row.PDEN_ID != query.ProductionEntityId || row.PDEN_SOURCE != query.Source || row.ACTIVE_IND != "Y" ||
                !Valid(row.VOLUME_METHOD) || !Valid(row.ACTIVITY_TYPE) || !Valid(row.PRODUCT_TYPE) || !Valid(row.PERIOD_TYPE) || !Valid(row.VOLUME_UOM) ||
                row.DISPOSITION_OBS_NO < 0 || decimal.Truncate(row.DISPOSITION_OBS_NO) != row.DISPOSITION_OBS_NO || row.AMENDMENT_SEQ_NO < 0 || decimal.Truncate(row.AMENDMENT_SEQ_NO) != row.AMENDMENT_SEQ_NO ||
                !identities.Add((row.VOLUME_METHOD, row.ACTIVITY_TYPE, row.PRODUCT_TYPE, row.PERIOD_TYPE, row.DISPOSITION_OBS_NO, row.AMENDMENT_SEQ_NO)))
                throw new InvalidOperationException("The disposition is foreign, ambiguous or incomplete.");
            result.Add(new(row.VOLUME_METHOD, row.ACTIVITY_TYPE, row.PRODUCT_TYPE, row.PERIOD_TYPE, row.DISPOSITION_OBS_NO, row.AMENDMENT_SEQ_NO,
                row.VOLUME, row.VOLUME_UOM, row.VOLUME_START_DATE, row.VOLUME_END_DATE));
        }
        return new(field, query, result.OrderBy(x => x.VolumeMethod, StringComparer.Ordinal).ThenBy(x => x.ActivityType, StringComparer.Ordinal)
            .ThenBy(x => x.ProductType, StringComparer.Ordinal).ThenBy(x => x.PeriodType, StringComparer.Ordinal).ThenBy(x => x.Observation).ThenBy(x => x.Amendment).ToArray(), revision);
    }
    private static bool Valid(string? text) => !string.IsNullOrWhiteSpace(text) && text.Length <= 128 && text == text.Trim() && !text.Any(char.IsControl);
}
