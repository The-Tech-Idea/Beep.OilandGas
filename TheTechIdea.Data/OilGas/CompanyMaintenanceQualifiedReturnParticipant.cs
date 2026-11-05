namespace TheTechIdea.Data.OilGas;

/// <summary>Private native proof owned by the existing complete return command, never a separate receipt.</summary>
public sealed record CompanyMaintenanceQualifiedReturnProof(string Body,DateTime RecordedAtUtc)
{
    public void Require()
    {
        if(string.IsNullOrWhiteSpace(Body) || Body.Length>2*1024*1024 || RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc==default)
            throw new InvalidOperationException("Complete bounded private qualified proof and declared audit UTC are required.");
    }
}
/// <summary>The active qualified owner joins the coordinator's same CORE transaction; domain authority remains in the caller.</summary>
public interface ICompanyMaintenanceQualifiedReturnParticipant
{
    Task<CompanyMaintenanceQualifiedReturnProof> PrepareMaintenanceQualifiedReturnAsync(CompanyMaintenanceAssetReturnCommand command,Func<Task> authority);
    Task ApplyMaintenanceQualifiedReturnAsync(CompanyMaintenanceAssetReturnCommand command,CompanyMaintenanceQualifiedReturnProof proof,Func<Task> authority);
    void RequireMaintenanceQualifiedReturnProof(CompanyMaintenanceAssetReturnCommand command,CompanyMaintenanceQualifiedReturnProof proof);
    Task VerifyMaintenanceQualifiedReturnAsync(CompanyMaintenanceAssetReturnCommand command,CompanyMaintenanceQualifiedReturnProof proof,FieldSelectionReview reader,bool owned,bool requireWrite,Func<Task> authority);
}
