using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Accounting.Models;
namespace Beep.OilandGas.Accounting.Models;
/// <summary>Executed checks from the active period-closing owner; not a persisted close or lock receipt.</summary>
public sealed record AccountingPeriodCheckReview(CompanyScope Company,string ActorUserId,string FieldId,string BookId,DateTime PeriodEnd,
    DateTime GeneratedAtUtc,IReadOnlyList<AccountingPeriodCheck> Checks);
public sealed record AccountingPeriodCheck(string RuleId,string Name,string Description,bool Mandatory,bool Passed,string Module,string Scope);
