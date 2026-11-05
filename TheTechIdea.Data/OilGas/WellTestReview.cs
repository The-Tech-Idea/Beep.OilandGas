using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;
public sealed record WellTestReview(CompanyScope Company,string Actor,string Field,string Well,DateTimeOffset RetrievedAtUtc,IReadOnlyList<WellTestResponse> Rows);
