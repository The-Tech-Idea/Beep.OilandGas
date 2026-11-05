using Beep.OilandGas.Repository.Models;
namespace Beep.OilandGas.Repository.Models;

// Display metadata from a validated external identity, never an account lookup key.
public sealed record ExternalRegistrationProfile(string? FullName, string? Email, bool EmailVerified = false);
