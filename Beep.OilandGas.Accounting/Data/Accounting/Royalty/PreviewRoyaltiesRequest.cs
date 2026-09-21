using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.Accounting.Royalty;

/// <summary>Read-only royalty preview selection. Commercial terms come from recorded source data.</summary>
public sealed class PreviewRoyaltiesRequest
{
    [Required]
    public required string FieldId { get; init; }
    public required DateOnly ProductionDate { get; init; }
}
