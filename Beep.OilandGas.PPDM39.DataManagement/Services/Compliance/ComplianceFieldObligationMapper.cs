using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Compliance;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Compliance;

/// <summary>Projects exact recorded PPDM FIELD links and parent facts, without assigning regulatory meaning.</summary>
public static class ComplianceFieldObligationMapper
{
    public static FieldObligationSourceReview Project(FieldSelectionReview context, IReadOnlyList<OBLIGATION_COMPONENT> components, IReadOnlyList<OBLIGATION> obligations)
    {
        ArgumentNullException.ThrowIfNull(context); ArgumentNullException.ThrowIfNull(components); ArgumentNullException.ThrowIfNull(obligations);
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(context.FieldId) || components.Count > 256 || obligations.Count > 256)
            throw new InvalidOperationException("An exact FIELD and bounded obligation evidence are required.");
        static bool Observation(decimal value) => value >= 0 && decimal.Truncate(value) == value;
        if (components.Any(row => row is null || row.ACTIVE_IND != "Y" || row.FIELD_ID != context.FieldId || !LeaseSummaryValidator.Identifier(row.OBLIGATION_ID) || !Observation(row.OBLIGATION_SEQ_NO) || !Observation(row.COMPONENT_OBS_NO)) ||
            components.Select(row => (row.OBLIGATION_ID, row.OBLIGATION_SEQ_NO, row.COMPONENT_OBS_NO)).Distinct().Count() != components.Count ||
            obligations.Any(row => row is null || row.ACTIVE_IND != "Y" || !LeaseSummaryValidator.Identifier(row.OBLIGATION_ID) || !Observation(row.OBLIGATION_SEQ_NO)) ||
            obligations.Select(row => (row.OBLIGATION_ID, row.OBLIGATION_SEQ_NO)).Distinct().Count() != obligations.Count)
            throw new InvalidOperationException("The obligation or FIELD component keys are invalid or ambiguous.");
        var grouped = components.GroupBy(row => (row.OBLIGATION_ID, row.OBLIGATION_SEQ_NO)).ToArray();
        if (obligations.Count != grouped.Length || obligations.Any(row => !grouped.Any(group => group.Key == (row.OBLIGATION_ID, row.OBLIGATION_SEQ_NO))))
            throw new InvalidOperationException("Every FIELD component needs exactly one matching obligation and no foreign parents.");
        var result = grouped.OrderBy(group => group.Key.OBLIGATION_ID, StringComparer.Ordinal).ThenBy(group => group.Key.OBLIGATION_SEQ_NO)
            .Select(group => {
                var parent = obligations.Single(row => row.OBLIGATION_ID == group.Key.OBLIGATION_ID && row.OBLIGATION_SEQ_NO == group.Key.OBLIGATION_SEQ_NO);
                var links = group.OrderBy(row => row.COMPONENT_OBS_NO).Select(row => new RecordedObligationComponent(row.COMPONENT_OBS_NO, row.OBLIGATION_COMPONENT_TYPE, row.CRITICAL_DATE, row.FULFILLED_IND, row.FULFILLED_DATE, row.DESCRIPTION)).ToArray();
                return new RecordedFieldObligation(parent.OBLIGATION_ID, parent.OBLIGATION_SEQ_NO, parent.OBLIGATION_TYPE, parent.DESCRIPTION, parent.CRITICAL_DATE, parent.FULFILLED_IND, parent.FULFILLED_DATE, Array.AsReadOnly(links));
            }).ToArray();
        return new(context, Array.AsReadOnly(result));
    }
}
