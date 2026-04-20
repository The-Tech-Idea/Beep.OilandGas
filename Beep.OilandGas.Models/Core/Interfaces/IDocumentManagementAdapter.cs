using Beep.OilandGas.Models.Data.Integrations;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IDocumentManagementAdapter
{
    Task<DocumentSyncResult> SyncFolderAsync(string folderPath, string fieldId, string userId);
    Task<DocumentSyncResult> SyncDocumentAsync(string documentId, string fieldId, string userId);
    Task<List<DocumentSummary>> GetAvailableDocumentsAsync(string folderPath, DateTime? modifiedAfter = null);
    Task<byte[]> GetDocumentContentAsync(string documentId);
}
