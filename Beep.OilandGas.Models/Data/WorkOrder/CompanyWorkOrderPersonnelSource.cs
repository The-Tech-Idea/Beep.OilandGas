using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.WorkOrder;
public sealed record CompanyWorkOrderPersonnelSourceQuery(CompanyWorkOrderExecutionPermitReference Permit,string EmployeeBaId,string LicenseId)
{
    public void Require(){if(Permit is null || !LeaseSummaryValidator.Identifier(EmployeeBaId) || !LeaseSummaryValidator.Identifier(LicenseId))throw new ArgumentException("Select the original pending permit and explicit employee/licence identities.");Permit.Require();}
}
public sealed record CompanyEmployeeSourceFacts(string EmployerBaId,string EmployeeBaId,decimal Observation,string? Position,string? Status,DateTime? EffectiveDate,DateTime? ExpiryDate,string Revision);
public sealed record CompanyPersonnelLicenseFacts(string EmployeeBaId,string LicenseId,string? Type,string? Number,string? GrantingAuthority,DateTime? GrantedDate,DateTime? EffectiveDate,DateTime? ExpiryDate,string Revision);
public sealed record CompanyPersonnelSourceFacts(FieldSelectionReview Context,CompanyWorkOrderPersonnelSourceQuery Query,CompanyEmployeeSourceFacts? Employment,CompanyPersonnelLicenseFacts? License,DateTime ComparedAtUtc,string Revision)
{
    public void Require()
    {
        Query.Require();FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);if(Context.FieldId!=Query.Permit.Task.FieldId || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc || Employment is null && License is not null || Employment is {} employee && (employee.EmployerBaId!=Context.Company.BusinessAssociateId || employee.EmployeeBaId!=Query.EmployeeBaId || !ProcessDefinitionRevision.IsValid(employee.Revision)) || License is {} license && (license.EmployeeBaId!=Query.EmployeeBaId || license.LicenseId!=Query.LicenseId || !ProcessDefinitionRevision.IsValid(license.Revision)))throw new InvalidOperationException("Original company employment/licence source context or membership changed.");
        var expected=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context,Query,Employment,License,ComparedAtUtc}));if(Revision!=expected)throw new InvalidOperationException("Complete original personnel source facts changed.");
    }
    public static CompanyPersonnelSourceFacts Create(FieldSelectionReview context,CompanyWorkOrderPersonnelSourceQuery query,CompanyEmployeeSourceFacts? employment,CompanyPersonnelLicenseFacts? license,DateTime utc){var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context=context,Query=query,Employment=employment,License=license,ComparedAtUtc=utc}));var result=new CompanyPersonnelSourceFacts(context,query,employment,license,utc,revision);result.Require();return result;}
}
public sealed record CompanyWorkOrderPersonnelSourceReview(CompanyWorkOrderExecutionPermitApprovalReview PermitReview,CompanyPersonnelSourceFacts Personnel,IReadOnlyList<string> Blockers)
{
    public static string[] GetBlockers(CompanyWorkOrderExecutionPermitIntent proposal,CompanyPersonnelSourceFacts source)
    {
        var result=new List<string>();if(source.Query.LicenseId!=proposal.PersonnelReference)result.Add("The selected licence does not match this permit's reported personnel reference.");
        if(source.Employment is not {} employment)result.Add("No exact active company employment membership is recorded for this employee.");else
        {
            if(string.IsNullOrWhiteSpace(employment.Position) || string.IsNullOrWhiteSpace(employment.Status))result.Add("Recorded employee position/status is unconfirmed.");Window(employment.EffectiveDate,employment.ExpiryDate,"Employment");
        }
        if(source.License is not {} license)result.Add("The exact active recorded licence is unavailable for this linked employee.");else
        {
            if(string.IsNullOrWhiteSpace(license.Type) || string.IsNullOrWhiteSpace(license.GrantingAuthority))result.Add("Recorded licence type/granting authority is unconfirmed.");if(license.GrantedDate is not {} granted || granted.Kind!=DateTimeKind.Utc || granted>source.ComparedAtUtc)result.Add("Recorded licence grant UTC is unconfirmed.");Window(license.EffectiveDate,license.ExpiryDate,"Licence");
        }
        result.Add("Current permit/task personnel assignment, competency requirements and jurisdiction-specific qualification evidence must be independently verified.");return result.ToArray();
        void Window(DateTime? from,DateTime? to,string label){if(from is not {} start || to is not {} end || start.Kind!=DateTimeKind.Utc || end.Kind!=DateTimeKind.Utc || end<=start){result.Add(label+" dates/time-zone are unconfirmed; no validity window is assumed.");return;}if(start>proposal.ValidFromUtc || end<proposal.ValidToUtc || source.ComparedAtUtc<start || source.ComparedAtUtc>=end)result.Add(label+" evidence does not cover the current complete permit validity window.");}
    }
    public void Require(CompanyScope company,string actor,CompanyWorkOrderPersonnelSourceQuery query)
    {
        query.Require();if(PermitReview is null || Personnel is null || Blockers is null || Personnel.Query!=query)throw new InvalidOperationException("Complete original permit/personnel source review is unavailable.");PermitReview.Require(company,actor,query.Permit);Personnel.Require();if(Personnel.Context.Company!=company || Personnel.Context.ActorUserId!=actor || Personnel.Context.Database!=PermitReview.CurrentSource.InspectionDatabase || Personnel.Context.FieldId!=query.Permit.Task.FieldId || !Blockers.SequenceEqual(GetBlockers(PermitReview.Proposal,Personnel),StringComparer.Ordinal))throw new InvalidOperationException("Original personnel/permit/binding/authority or complete blockers changed.");
    }
}
public interface ICompanyWorkOrderPersonnelSourceOwner
{
    Task<CompanyPersonnelSourceFacts> ReadPersonnelSourceAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelSourceQuery query,Func<Task> requireAuthority);
}
