using Beep.OilandGas.Models.Data.HSE;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface ICorrectiveActionService
{
    Task<string>       CreateCAPlanAsync(string incidentId, string userId);
    Task<string>       AddCorrectiveActionAsync(string incidentId, AddCARequest request, string userId);
    Task               AssignResponsiblePersonAsync(string incidentId, int stepSeq, string baId, string userId);
    Task               SetDueDateAsync(string incidentId, int stepSeq, DateTime dueDate, string userId);
    Task               RecordCompletionAsync(string incidentId, int stepSeq, string completionNotes, string userId);
    Task<bool>         AllCAsMeetDeadlineAsync(string incidentId);
    Task<List<CAStatus>> GetCAStatusAsync(string incidentId);
}
