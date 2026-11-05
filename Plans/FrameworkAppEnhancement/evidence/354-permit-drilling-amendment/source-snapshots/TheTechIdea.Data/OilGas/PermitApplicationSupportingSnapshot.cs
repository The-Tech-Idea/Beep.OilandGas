using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;

namespace TheTechIdea.Data.OilGas;

/// <summary>Frozen supporting rows reviewed with the header; null means the exact row was absent.</summary>
public sealed record PermitApplicationSupportingSnapshot(
    [property: JsonRequired] string? DrillingJson,
    [property: JsonRequired] string? EnvironmentalJson,
    [property: JsonRequired] string? InjectionJson)
{
    public static readonly PermitApplicationSupportingSnapshot Empty = new(null, null, null);
    private static readonly JsonSerializerOptions Json = new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
    [JsonIgnore] public DRILLING_PERMIT_APPLICATION? Drilling => Read<DRILLING_PERMIT_APPLICATION>(DrillingJson);
    [JsonIgnore] public ENVIRONMENTAL_PERMIT_APPLICATION? Environmental => Read<ENVIRONMENTAL_PERMIT_APPLICATION>(EnvironmentalJson);
    [JsonIgnore] public INJECTION_PERMIT_APPLICATION? Injection => Read<INJECTION_PERMIT_APPLICATION>(InjectionJson);
    private static T? Read<T>(string? value) where T : class
    {
        if (value is null) return null;
        if (value.Length is 0 or > 131072) throw new InvalidOperationException("Supporting permit evidence exceeds its bounds.");
        return JsonSerializer.Deserialize<T>(value, Json) ?? throw new InvalidOperationException("Supporting permit evidence is absent.");
    }
    public static PermitApplicationSupportingSnapshot Capture(PERMIT_APPLICATION parent, DRILLING_PERMIT_APPLICATION? drilling,
        ENVIRONMENTAL_PERMIT_APPLICATION? environmental, INJECTION_PERMIT_APPLICATION? injection)
    {
        var result = new PermitApplicationSupportingSnapshot(drilling is null ? null : JsonSerializer.Serialize(drilling, Json),
            environmental is null ? null : JsonSerializer.Serialize(environmental, Json), injection is null ? null : JsonSerializer.Serialize(injection, Json));
        result.Require(parent); return result;
    }
    public void Require(PERMIT_APPLICATION parent)
    {
        var d=Drilling; var e=Environmental; var i=Injection;
        PermitApplicationSupportingRecords.RequireParent(parent,d,e,i);
        if (d is not null && d.ACTIVE_IND != "Y" || e is not null && e.ACTIVE_IND != "Y" || i is not null && i.ACTIVE_IND != "Y")
            throw new InvalidOperationException("Reviewed supporting evidence must be active.");
    }
}
