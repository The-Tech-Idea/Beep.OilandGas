namespace Beep.OilandGas.Models.Data.Process;
public sealed class ProcessInstanceRequest
{
    public string ProcessId {get;set;}=string.Empty;
    public string EntityId {get;set;}=string.Empty;
    public string? EntityType {get;set;}
    public string ExpectedFieldId {get;set;}=string.Empty;
    public string ExpectedDefinitionRevision {get;set;}=string.Empty;
    public string ExpectedSourceRevision {get;set;}=string.Empty;
}
