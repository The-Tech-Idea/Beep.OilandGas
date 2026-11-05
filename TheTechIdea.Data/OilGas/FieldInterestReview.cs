using Beep.OilandGas.PPDM39.Models;

namespace TheTechIdea.Data.OilGas;

// Recorded PPDM evidence for review; this response does not grant legal title, operatorship or billing authority.
public sealed record FieldInterestReview(CompanyScope Company, string DatabaseScope, string FieldId,
    DateTime ReviewedUtc, string DateZoneId, List<FieldInterestSetReview> Sets);
public sealed record FieldInterestSetReview(INTEREST_SET InterestSet, List<INT_SET_COMPONENT> Components, List<INT_SET_PARTNER> CompanyPartners);

/// <summary>Canonical typed review invariants shared by API consumers; source JSON presence is checked before conversion.</summary>
public static class FieldInterestReviewValidation
{
    public static void RequireValid(FieldInterestReview? result, CompanyScope selected, string fieldId)
    {
        if (result is null || result.Company != selected || result.FieldId != fieldId || !Identifier(result.FieldId) ||
            result.DatabaseScope is not { Length: 64 } || !result.DatabaseScope.All(char.IsAsciiHexDigit) || result.DatabaseScope != result.DatabaseScope.ToUpperInvariant() ||
            result.ReviewedUtc == default || result.ReviewedUtc.Kind != DateTimeKind.Utc || !Identifier(result.DateZoneId) || result.Sets is null ||
            result.Sets.Sum(set => 1 + (set?.Components?.Count ?? 0) + (set?.CompanyPartners?.Count ?? 0)) > 256 ||
            result.Sets.Any(set => set is null || set.InterestSet is null || !Identifier(set.InterestSet.INTEREST_SET_ID) || !Identifier(set.InterestSet.INTEREST_SET_TYPE) ||
                set.InterestSet.ACTIVE_IND != "Y" || !Observation(set.InterestSet.INTEREST_SET_SEQ_NO) ||
                set.Components is null || set.Components.Count == 0 || set.CompanyPartners is null || set.CompanyPartners.Count == 0 ||
                set.Components.Any(row => row is null || row.FIELD_ID != fieldId || row.ACTIVE_IND != "Y" || !Identifier(row.COMPONENT_TYPE) ||
                    row.INTEREST_SET_ID != set.InterestSet.INTEREST_SET_ID || row.INTEREST_SET_SEQ_NO != set.InterestSet.INTEREST_SET_SEQ_NO || !Observation(row.COMPONENT_OBS_NO)) ||
                set.CompanyPartners.Any(row => row is null || row.PARTNER_BA_ID != selected.BusinessAssociateId || row.ACTIVE_IND != "Y" ||
                    row.INTEREST_SET_ID != set.InterestSet.INTEREST_SET_ID || row.INTEREST_SET_SEQ_NO != set.InterestSet.INTEREST_SET_SEQ_NO ||
                    !Observation(row.PARTNER_OBS_NO) || !Identifier(row.INTEREST_SET_ROLE)) ||
                set.Components.Select(row => row.COMPONENT_OBS_NO).Distinct().Count() != set.Components.Count ||
                set.CompanyPartners.Select(row => row.PARTNER_OBS_NO).Distinct().Count() != set.CompanyPartners.Count) ||
            result.Sets.Select(set => (set.InterestSet.INTEREST_SET_ID, set.InterestSet.INTEREST_SET_SEQ_NO)).Distinct().Count() != result.Sets.Count)
            throw new InvalidOperationException("The response has inconsistent company FIELD interest evidence.");
    }
    private static bool Observation(decimal value) => value >= 0 && decimal.Truncate(value) == value;
    private static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value == value.Trim() && !value.Any(char.IsControl);
}
public interface ICompanyFieldInterestReview
{
    Task<FieldInterestReview> ReadAsync(string fieldId);
}
