using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface ISchedulingService
{
    /// <summary>
    /// Schedule a work order for a piece of equipment.
    /// Returns confirmed dates or a conflict reason.
    /// </summary>
    Task<ScheduleResult> ScheduleWorkOrderAsync(
        string instanceId, string equipmentId,
        DateTime proposedStart, TimeSpan duration, string userId);

    Task<List<ScheduleConflict>> GetConflictsAsync(
        string equipmentId, DateTime from, DateTime to,
        string? excludeInstanceId = null);

    Task<ScheduleResult> RescheduleAsync(
        string instanceId, DateTime newStart, string userId);

    /// <summary>
    /// All planned WOs for a field in the given date range (calendar view).
    /// </summary>
    Task<List<CalendarSlot>> GetFieldCalendarAsync(
        string fieldId, DateTime from, DateTime to);
}
