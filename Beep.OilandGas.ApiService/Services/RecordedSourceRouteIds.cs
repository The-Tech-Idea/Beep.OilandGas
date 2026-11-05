using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Reads a source parameter from its original escaped request path, rather than a partially decoded route value.</summary>
public static class RecordedSourceRouteIds
{
    public static string Read(HttpRequest request,string parameter)
    {
        var endpoint=request.HttpContext.GetEndpoint() as RouteEndpoint;
        var raw=request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;
        if(endpoint is null || string.IsNullOrEmpty(raw) || !raw.StartsWith('/'))throw new FormatException("The original source route is unavailable.");
        var path=raw.Split('?')[0];var pathBase=request.PathBase.ToUriComponent();
        if(!path.StartsWith(pathBase,StringComparison.OrdinalIgnoreCase))throw new FormatException("The source route base changed.");
        var segments=path[pathBase.Length..].TrimStart('/').Split('/');var pattern=endpoint.RoutePattern.PathSegments;
        if(segments.Length!=pattern.Count)throw new FormatException("The source route shape is invalid.");
        for(var i=0;i<pattern.Count;i++)
            if(pattern[i].Parts.Count==1 && pattern[i].Parts[0] is RoutePatternParameterPart part && part.Name==parameter)
                return RecordedSourceId.DecodeSegment(segments[i]);
        throw new FormatException("The source parameter is unavailable.");
    }
}
