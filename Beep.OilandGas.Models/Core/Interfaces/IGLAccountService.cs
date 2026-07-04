using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces;

/// <summary>
/// Service for managing GL Account master data and account balance calculations.
/// </summary>
public interface IGLAccountService
{
    /// <summary>Get GL account by account number.</summary>
    Task<GL_ACCOUNT?> GetAccountByNumberAsync(string accountNumber);

    /// <summary>Get all active GL accounts.</summary>
    Task<List<GL_ACCOUNT>> GetAllAccountsAsync();

    /// <summary>Get GL accounts filtered by account type.</summary>
    Task<List<GL_ACCOUNT>> GetAccountsByTypeAsync(string accountType);

    /// <summary>Get current balance for a GL account, optionally filtered by date and book.</summary>
    Task<decimal> GetAccountBalanceAsync(string accountNumber, DateTime? asOfDate = null, string? bookId = null);

    /// <summary>Create a new GL account.</summary>
    Task<GL_ACCOUNT> CreateAccountAsync(string accountNumber, string accountName, string accountType, string userId, string? category = null, string? normalBalance = null, string? description = null);

    /// <summary>Validate that a GL account exists and is active.</summary>
    Task<bool> ValidateAccountAsync(string accountNumber);

    /// <summary>Validate that a GL account is of the expected type.</summary>
    Task<bool> ValidateAccountTypeAsync(string accountNumber, string expectedType);

    /// <summary>Generate default GL accounts for initial setup.</summary>
    Task GenerateDefaultAccountsAsync(string userId);
}
