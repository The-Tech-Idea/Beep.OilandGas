using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Production;
public sealed record WellTestReview(CompanyScope Company,string Actor,string Field,string Well,DateTimeOffset RetrievedAtUtc,IReadOnlyList<WellTestResponse> Rows);
