namespace TheTechIdea.Data.OilGas;

public sealed record PermitApplicationDataScope(FieldSelectionReview Field, AssetDatabaseScope Database)
{
    public void Require()
    {
        if (Field is null || Field.Company is null || Database is null ||
            !LeaseSummaryValidator.Identifier(Field.Company.TenantId) || !LeaseSummaryValidator.Identifier(Field.Company.BusinessAssociateId) ||
            !LeaseSummaryValidator.Identifier(Field.ActorUserId))
            throw new InvalidOperationException("Current permit company/account/FIELD and module binding are required.");
        FieldSelectionReviewValidator.RequireReview(Field, Field.Company, Field.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(Field.FieldId) || !LeaseSummaryValidator.Identifier(Database.ConnectionName) || !LeaseSummaryValidator.Identifier(Database.Fingerprint))
            throw new InvalidOperationException("The selected permit FIELD or module database is unavailable.");
    }
}

/// <summary>Current API-owned module/resource authority supplied to native permit repositories.</summary>
public interface ICompanyPermitApplicationDataAccess
{
    Task<PermitApplicationDataScope> RequireReadScopeAsync();
    Task<PermitApplicationInboxAuthority> ReviewInboxAuthorityAsync();
    Task<PermitApplicationCreationReview> ReviewCreationAsync(PermitApplicationCreationProposal proposal);
    IPPDMRowAccessPolicy ForRead(Type entityType, string tableName, string? applicationId = null);
    Task<T> WithDocumentStageAuthorityAsync<T>(PermitDocumentStageOriginal original, Func<Func<Task>, Task<T>> execute);
    Task<T> WithCommandAuthorityAsync<T>(PermitApplicationCommand original, Func<Func<Task>, Task<T>> execute);
}
