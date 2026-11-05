using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
namespace Beep.OilandGas.Models.Data.HSE;

/// <summary>Recorded PPDM incident type and severity pairs for the selected company FIELD.</summary>
public sealed record IncidentReportOption(string IncidentSetId, string IncidentTypeId, string SeverityId);
public sealed record IncidentReportOptions(FieldSelectionReview Context, string Revision, IReadOnlyList<IncidentReportOption> Options);
