namespace Beep.OilandGas.Models.Data.Common;

/// <summary>Exact source identity at a URI path boundary; decode the original escaped segment once.</summary>
public static class RecordedSourceId
{
    public static string[] FromDetailSegments(string location,string baseLocation,string relativePrefix,int count)
    {
        var uri=new Uri(location,UriKind.Absolute);var root=new Uri(baseLocation,UriKind.Absolute);
        if(!root.IsBaseOf(uri) || count<1)throw new FormatException("The source route is outside the app root.");
        var relative=root.MakeRelativeUri(uri).OriginalString.Split('?', '#')[0];
        if(!relative.StartsWith(relativePrefix,StringComparison.OrdinalIgnoreCase))throw new FormatException("The source route does not match this detail screen.");
        var segments=relative[relativePrefix.Length..].Split('/');if(segments.Length!=count)throw new FormatException("The source route has invalid membership.");
        return segments.Select(DecodeSegment).ToArray();
    }
    public static string DecodeSegment(string escaped)
    {
        if(string.IsNullOrEmpty(escaped) || escaped.Contains('/'))throw new FormatException("A single escaped source segment is required.");
        for(var i=0;i<escaped.Length;i++)if(escaped[i]=='%')
        {
            if(i+2>=escaped.Length || !Uri.IsHexDigit(escaped[i+1]) || !Uri.IsHexDigit(escaped[i+2]))throw new FormatException("The source escape is invalid.");
            i+=2;
        }
        var id=Uri.UnescapeDataString(escaped);
        if(string.IsNullOrWhiteSpace(id) || id.Length>128 || id.Any(char.IsControl))throw new FormatException("The source identity is empty or exceeds its contract.");
        return id;
    }
    public static string FromDetailLocation(string location,string baseLocation,string relativePrefix)
    {
        var uri=new Uri(location,UriKind.Absolute);var root=new Uri(baseLocation,UriKind.Absolute);
        if(!root.IsBaseOf(uri))throw new FormatException("The source route is outside the app root.");
        var relative=root.MakeRelativeUri(uri).OriginalString.Split('?', '#')[0];
        if(!relative.StartsWith(relativePrefix,StringComparison.OrdinalIgnoreCase))throw new FormatException("The source route does not match this detail screen.");
        return DecodeSegment(relative[relativePrefix.Length..]);
    }
}
