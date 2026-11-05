namespace Beep.OilandGas.PPDM39.DataManagement.Models;

public sealed record SetupRepositoryConnectionOption(string ConnectionName, string Provider);
public sealed record SetupRepositoryConnectionSelection(string ConnectionName);
public sealed record SetupRepositoryConnectionTest(string ConnectionName, bool Connected);
/// <summary>Connection credentials are request-only and must never be returned by setup APIs.</summary>
public sealed record SetupRepositoryConnectionCreate(string ConnectionName, string Provider, string ConnectionString, string DriverId)
{
    public override string ToString() => $"SetupRepositoryConnectionCreate {{ ConnectionName = {ConnectionName}, Provider = {Provider}, ConnectionString = [redacted] }}";
}
