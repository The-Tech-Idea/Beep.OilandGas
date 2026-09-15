namespace Beep.OilandGas.LifeCycle.Services.Processes;

public interface IRolePersonaReader
{
    Task<List<string>> GetActivePersonasAsync(string roleName);
}
