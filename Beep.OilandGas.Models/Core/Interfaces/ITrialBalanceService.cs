using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface ITrialBalanceService
{
    Task<List<GL_ACCOUNT>> GenerateTrialBalanceAsync(DateTime? asOfDate = null, string? bookId = null);
    Task<List<GL_ACCOUNT>> GetTrialBalanceByTypeAsync(string accountType, DateTime? asOfDate = null, string? bookId = null);
    Task<(bool IsBalanced, decimal TotalDebits, decimal TotalCredits, decimal Difference)> ValidateGLAsync(DateTime? asOfDate = null, string? bookId = null);
    Task<List<GL_ACCOUNT>> GetPostClosingTrialBalanceAsync(DateTime? asOfDate = null, string? bookId = null);
    Task<string> ExportToCSVAsync(DateTime? asOfDate = null, string? bookId = null);
    Task<bool> CanClosePeriodAsync(DateTime? asOfDate = null, string? bookId = null);
}
