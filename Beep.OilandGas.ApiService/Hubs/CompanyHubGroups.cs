using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;

namespace Beep.OilandGas.ApiService.Hubs;

public enum CompanyChannelKind { User, Persona, Process, Operation, Workflow }
public sealed record CompanyHubChannel(CompanyScope Scope, CompanyChannelKind Kind, string Resource, string? Field = null);

/// <summary>Canonical company-bound channels; no global or legacy group aliases.</summary>
public static class CompanyHubGroups
{
    public static string Name(CompanyHubChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        Valid(channel.Scope.TenantId, 64); Valid(channel.Scope.BusinessAssociateId, 128); Valid(channel.Resource, 512);
        if (channel.Kind != CompanyChannelKind.Persona && channel.Field is not null)
            throw new ArgumentException("Only persona channels specify a field.", nameof(channel));
        var prefix = $"tenant:{Uri.EscapeDataString(channel.Scope.TenantId)}:company:{Uri.EscapeDataString(channel.Scope.BusinessAssociateId)}";
        var resource = Uri.EscapeDataString(channel.Resource);
        return channel.Kind switch
        {
            CompanyChannelKind.User => $"{prefix}:user:{resource}",
            CompanyChannelKind.Process => $"{prefix}:process:{resource}",
            CompanyChannelKind.Operation => $"{prefix}:operation:{resource}",
            CompanyChannelKind.Workflow => $"{prefix}:workflow:{resource}",
            CompanyChannelKind.Persona => Persona(prefix, resource, channel.Field),
            _ => throw new ArgumentException("Unknown company channel.", nameof(channel))
        };
    }

    private static string Persona(string prefix, string resource, string? field)
    { Valid(field, 512); return $"{prefix}:persona:{resource}:field:{Uri.EscapeDataString(field!)}"; }
    private static void Valid(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > max || value.Any(char.IsControl))
            throw new ArgumentException("A complete, valid company channel is required.");
    }

    public static CompanyHubChannel? Parse(string? group)
    {
        if (group is null || group.Length > 5000) return null;
        var parts = group.Split(':');
        if (parts.Length is not (6 or 8) || parts[0] != "tenant" || parts[2] != "company") return null;
        var kind = parts[4] switch
        {
            "user" => CompanyChannelKind.User, "persona" => CompanyChannelKind.Persona,
            "process" => CompanyChannelKind.Process, "operation" => CompanyChannelKind.Operation,
            "workflow" => CompanyChannelKind.Workflow, _ => (CompanyChannelKind?)null
        };
        if (kind is null || (kind == CompanyChannelKind.Persona ? parts.Length != 8 || parts[6] != "field" : parts.Length != 6)) return null;
        try
        {
            var channel = new CompanyHubChannel(new(Uri.UnescapeDataString(parts[1]), Uri.UnescapeDataString(parts[3])),
                kind.Value, Uri.UnescapeDataString(parts[5]), parts.Length == 8 ? Uri.UnescapeDataString(parts[7]) : null);
            return Name(channel) == group ? channel : null;
        }
        catch (ArgumentException) { return null; }
    }
}
