namespace Beep.OilandGas.HSE.Models;

public static class IncidentReportTime
{
    public static bool TryParseIncidentTime(string? text,out DateTime utc)
    {
        utc=default;
        if(string.IsNullOrEmpty(text) || text!=text.Trim() || text.Length>40)return false;
        var value=text.EndsWith('Z')?text[..^1]+"+00:00":text;
        string[] formats=["yyyy-MM-dd'T'HH:mmzzz","yyyy-MM-dd'T'HH:mm:sszzz","yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz"];
        if(!DateTimeOffset.TryParseExact(value,formats,System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var instant))return false;
        utc=instant.UtcDateTime;return true;
    }
}
