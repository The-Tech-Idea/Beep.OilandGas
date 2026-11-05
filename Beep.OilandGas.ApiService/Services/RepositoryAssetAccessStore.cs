using Beep.OilandGas.Models.Core.Refusals;
using System.Security.Cryptography;
using System.Text.Json;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class RepositoryAssetAccessStore(RepositoryDbContext repository, CompanyModuleConnection company,
    IAssetGrantTargetVerifier targets, TimeProvider? clock = null) : IUserAssetAccessStore, IWorkflowRecipientAssetReader
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    public Task<IReadOnlyList<AppUserAssetAccess>> ReadCompletionReviewRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users)
        =>ReadWorkflowFieldGrantsAsync(current.Company,current.ActorUserId,current.CoreDatabase,current.FieldId,users,
            ["Workflow.View","Workflow.Approve","Drilling.View","Drilling.Approve","Drilling.ViewDailyReports","DevelopmentPlanning.View","Accounting.View"]);
    public Task<IReadOnlyList<AppUserAssetAccess>> ReadExecutionRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users)
        =>ReadWorkflowFieldGrantsAsync(current.Company,current.ActorUserId,current.CoreDatabase,current.FieldId,users,
            ["Workflow.View","Workflow.Start","Drilling.View","Drilling.Edit","Drilling.ManageWellProgram","Drilling.ViewDailyReports","DevelopmentPlanning.View","Accounting.View"]);
    public Task<IReadOnlyList<AppUserAssetAccess>> ReadProgramReviewRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users)
        =>ReadWorkflowFieldGrantsAsync(current.Company,current.ActorUserId,current.CoreDatabase,current.FieldId,users,
            ["Workflow.View","Workflow.Approve","Drilling.View","Drilling.Approve","DevelopmentPlanning.View","Accounting.View"]);
    public Task<IReadOnlyList<AppUserAssetAccess>> ReadProgramRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users)
        =>ReadWorkflowFieldGrantsAsync(current.Company,current.ActorUserId,current.CoreDatabase,current.FieldId,users,
            ["Workflow.View","Workflow.Start","Drilling.View","Drilling.Create","DevelopmentPlanning.View","Accounting.View"]);
    public Task<IReadOnlyList<AppUserAssetAccess>> ReadInvestmentReviewRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users)
        =>ReadWorkflowFieldGrantsAsync(current.Company,current.ActorUserId,current.CoreDatabase,current.FieldId,users,
            ["Workflow.View","Workflow.Approve","DevelopmentPlanning.View","Accounting.View","Accounting.ReviewDevelopmentBudget"]);
    public Task<IReadOnlyList<AppUserAssetAccess>> ReadInvestmentRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users)
        =>ReadWorkflowFieldGrantsAsync(current.Company,current.ActorUserId,current.CoreDatabase,current.FieldId,users,
            ["Workflow.View","Workflow.Start","DevelopmentPlanning.View","Accounting.View","Accounting.PrepareDevelopmentBudget"]);
    public Task<IReadOnlyList<AppUserAssetAccess>> ReadRecipientFieldGrantsAsync(FieldSelectionReview current,IReadOnlyList<string> users)
    {
        FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        return ReadWorkflowFieldGrantsAsync(current.Company,current.ActorUserId,current.Database,current.FieldId!,users,["Workflow.Start","DevelopmentPlanning.Create"]);
    }
    public Task<IReadOnlyList<AppUserAssetAccess>> ReadReviewRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users)
        =>ReadWorkflowFieldGrantsAsync(current.Company,current.ActorUserId,current.CoreDatabase,current.FieldId,users,["Workflow.View","Workflow.Approve","DevelopmentPlanning.View","DevelopmentPlanning.Approve"]);
    private async Task<IReadOnlyList<AppUserAssetAccess>> ReadWorkflowFieldGrantsAsync(CompanyScope scope,string actorId,AssetDatabaseScope database,string fieldId,IReadOnlyList<string> users,string[] permissions)
    {
        APInvoiceOwnership.RequireScope(scope);
        if(!LeaseSummaryValidator.Identifier(fieldId) || users is null || users.Count is <1 or >256 ||
            users.Any(user=>!LeaseSummaryValidator.Identifier(user)) || users.Distinct(StringComparer.Ordinal).Count()!=users.Count)
            throw RefusalException.Invalid("A bounded current FIELD recipient lookup is required.");
        async Task Guard()
        {
            var actor=await company.RefreshCompanyAsync();company.RequireActor(actorId);
            if(actor.Scope!=scope || !permissions.All(permission=>actor.Permissions.Contains(permission,StringComparer.Ordinal)) || await company.ResolveAsync()!=database)
                throw RefusalException.Forbidden("Current company development workflow authority is required for assignment.");
        }
        await Guard();
        // Internal assignment projection only: existing selected FIELD grants, without a general subject override.
        var rows=await repository.Set<AppUserAssetAccess>().AsNoTracking().Where(grant=>users.Contains(grant.UserId) &&
            grant.DatabaseScope==database.Fingerprint && grant.OrganizationId==scope.BusinessAssociateId &&
            grant.AssetType=="FIELD" && grant.AssetId==fieldId && grant.FacilityType==null && grant.IsActive &&
            (grant.AccessLevel=="READ" || grant.AccessLevel=="WRITE" || grant.AccessLevel=="DELETE")).ToArrayAsync();
        await Guard();
        return rows.Where(grant=>users.Contains(grant.UserId,StringComparer.Ordinal) && grant.DatabaseScope==database.Fingerprint &&
            grant.OrganizationId==scope.BusinessAssociateId && grant.AssetType=="FIELD" && grant.AssetId==fieldId &&
            grant.FacilityType is null && grant.IsActive && grant.AccessLevel is "READ" or "WRITE" or "DELETE").OrderBy(grant=>grant.Id,StringComparer.Ordinal).ToArray();
    }
    public async Task<List<AppUserAssetAccess>> ReadAsync(string userId, string databaseScope, string? organizationId = null)
    {
        ValidateUserAndScope(userId, databaseScope);
        await RequireDatabaseScopeAsync(databaseScope);
        organizationId = RequireOrganization(organizationId);
        if (await company.ResolveSubjectAsync(userId) is null) return [];
        var grants = await (from grant in repository.Set<AppUserAssetAccess>().AsNoTracking()
                      join user in repository.Users on grant.UserId equals user.Id
                      where user.Id == userId && user.IsActive && grant.IsActive
                          && grant.DatabaseScope == databaseScope && grant.OrganizationId == organizationId
                      select grant).ToListAsync();
        return grants.Where(x => x.UserId == userId && x.DatabaseScope == databaseScope && x.OrganizationId == organizationId).ToList();
    }

    public async Task<bool> GrantAsync(string userId, string databaseScope, string assetType, string assetId,
        string accessLevel, bool inherit, string? organizationId = null, string? facilityType = null)
    {
        ValidateUserAndScope(userId, databaseScope);
        await RequireDatabaseScopeAsync(databaseScope);
        var actor = RequireAdministrator();
        assetType = NormalizeAssetType(assetType);
        if ((assetType is "WORK_ORDER" or "EQUIPMENT") && inherit)
            throw RefusalException.Invalid("Work-order and equipment grants are direct and cannot inherit access to other assets.");
        ValidateIdentifier(assetId, nameof(assetId));
        ValidateFacility(assetType, facilityType);
        organizationId = RequireOrganization(organizationId);
        accessLevel = accessLevel?.ToUpperInvariant() ?? "";
        if (accessLevel is not ("READ" or "WRITE" or "DELETE"))
            throw RefusalException.Invalid("Access level must be READ, WRITE, or DELETE.");
        if (await company.ResolveSubjectAsync(userId) is null) return false;

        await targets.ValidateAsync(databaseScope, assetType, assetId, accessLevel, facilityType);
        await RequireDatabaseScopeAsync(databaseScope);
        if (RequireAdministrator() != actor) throw RefusalException.Forbidden("The asset grant actor changed.");
        if (await company.ResolveSubjectAsync(userId) is null) return false;

        // The deterministic primary key arbitrates concurrent creation on every provider.
        string?[] key = [userId, databaseScope, assetType, assetId, organizationId];
        if (assetType == "FACILITY") key = [.. key, facilityType];
        var id = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(key)));
        var grant = await repository.Set<AppUserAssetAccess>().SingleOrDefaultAsync(x => x.Id == id);
        if (grant is not null && (grant.UserId != userId || grant.DatabaseScope != databaseScope ||
            grant.AssetType != assetType || grant.AssetId != assetId || grant.FacilityType != facilityType || grant.OrganizationId != organizationId))
            throw RefusalException.Conflict("The stored asset grant's identity is inconsistent. Review its ownership before changing access.");
        var now = time.GetUtcNow().UtcDateTime;
        if (grant is null)
        {
            grant = new AppUserAssetAccess { Id = id, UserId = userId, DatabaseScope = databaseScope,
                AssetType = assetType, AssetId = assetId, FacilityType = facilityType, OrganizationId = organizationId,
                CreatedBy = actor, CreatedUtc = now };
            repository.Add(grant);
        }
        grant.AccessLevel = accessLevel;
        grant.Inherit = inherit;
        grant.IsActive = true;
        grant.ChangedBy = actor;
        grant.ChangedUtc = now;
        grant.ConcurrencyStamp = Guid.NewGuid().ToString();
        await repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeAsync(string userId, string databaseScope, string assetType, string assetId, string? facilityType = null)
    {
        ValidateUserAndScope(userId, databaseScope);
        await RequireDatabaseScopeAsync(databaseScope);
        var actor = RequireAdministrator();
        var organizationId = RequireOrganization(null);
        assetType = NormalizeAssetType(assetType);
        ValidateIdentifier(assetId, nameof(assetId));
        ValidateFacility(assetType, facilityType);
        var grants = await repository.Set<AppUserAssetAccess>().Where(x => x.UserId == userId
            && x.DatabaseScope == databaseScope && x.OrganizationId == organizationId && x.AssetType == assetType && x.AssetId == assetId && x.FacilityType == facilityType && x.IsActive).ToListAsync();
        // SQL Server collation must not widen an exact asset identifier match.
        grants = grants.Where(x => x.UserId == userId && x.DatabaseScope == databaseScope && x.OrganizationId == organizationId && x.AssetType == assetType && x.AssetId == assetId && x.FacilityType == facilityType).ToList();
        foreach (var grant in grants)
        {
            grant.IsActive = false;
            grant.ChangedBy = actor;
            grant.ChangedUtc = time.GetUtcNow().UtcDateTime;
            grant.ConcurrencyStamp = Guid.NewGuid().ToString();
        }
        await repository.SaveChangesAsync();
        return grants.Count != 0;
    }

    private string RequireAdministrator()
    {
        if (!company.Company.Roles.Contains("Administrator", StringComparer.Ordinal))
            throw RefusalException.Forbidden("A current company Administrator assignment is required.");
        return company.Company.UserId;
    }

    private async Task RequireDatabaseScopeAsync(string fingerprint)
    {
        if ((await company.ResolveAsync()).Fingerprint != fingerprint)
            throw RefusalException.Conflict("Asset grants require the selected company's current reviewed database scope.");
    }

    private static void ValidateUserAndScope(string userId, string databaseScope)
    {
        ValidateIdentifier(userId, nameof(userId));
        if (databaseScope is null || databaseScope.Length != 64 || databaseScope.Any(c => !char.IsAsciiHexDigit(c))
            || databaseScope != databaseScope.ToUpperInvariant())
            throw new ArgumentException("A canonical database scope fingerprint is required.", nameof(databaseScope));
    }

    private static string NormalizeAssetType(string value)
    {
        value = value?.ToUpperInvariant() ?? "";
        return value is "FIELD" or "WELL" or "POOL" or "FACILITY" or "WORK_ORDER" or "EQUIPMENT" ? value
            : throw RefusalException.Invalid("Unsupported asset type.");
    }

    private string RequireOrganization(string? value)
    {
        var selected = company.Company.Scope.BusinessAssociateId;
        if (value is null) return selected;
        ValidateIdentifier(value, nameof(value));
        if (value != selected) throw RefusalException.Forbidden("Asset grants belong to the selected company.");
        return selected;
    }

    private static void ValidateIdentifier(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value != value.Trim() || value.Any(char.IsControl))
            throw RefusalException.Invalid($"{name}: an identifier of up to 128 characters without surrounding whitespace is required.");
    }

    private static void ValidateFacility(string assetType, string? facilityType)
    {
        if (assetType == "FACILITY") ValidateIdentifier(facilityType!, nameof(facilityType));
        else if (facilityType is not null) throw RefusalException.Invalid("Facility type applies only to facility grants.");
    }
}
