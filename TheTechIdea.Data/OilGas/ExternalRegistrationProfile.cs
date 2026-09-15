namespace TheTechIdea.Data.OilGas;

// Display metadata from a validated external identity, never an account lookup key.
public sealed record ExternalRegistrationProfile(string? FullName, string? Email, bool EmailVerified = false);
