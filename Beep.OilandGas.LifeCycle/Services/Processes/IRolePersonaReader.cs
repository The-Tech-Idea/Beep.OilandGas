namespace Beep.OilandGas.LifeCycle.Services.Processes;

public interface IRolePersonaReader
{
    /// <summary>Active persona destinations backed by current company memberships and approved standard role grants.</summary>
    Task<List<string>> GetActivePersonasAsync(string roleName);
}
