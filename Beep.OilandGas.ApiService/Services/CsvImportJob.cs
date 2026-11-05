using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed record CsvImportJob(string OperationId, string TableName, CompanyScope Company, AssetDatabaseScope Database,
    string UserId, bool ValidateForeignKeys, byte[] Content)
{
    public string ConnectionName => Database.ConnectionName;
    public const int MaxUploadBytes = 2 * 1024 * 1024;
    public static string QueueKey(string operationId) => $"csv-import:{operationId}";
}

