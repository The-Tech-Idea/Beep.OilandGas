using System.Globalization;
namespace Beep.OilandGas.Models.Data.Common;
public static class ExplicitUtcInput
{
    public static bool TryParse(string? value,out DateTime utc)
    {
        string[] formats=["yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'","yyyy-MM-dd'T'HH:mm:ss'Z'","yyyy-MM-dd'T'HH:mm'Z'","yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz","yyyy-MM-dd'T'HH:mm:sszzz","yyyy-MM-dd'T'HH:mmzzz"];
        utc=default;if(string.IsNullOrWhiteSpace(value) || !(value.EndsWith("Z",StringComparison.Ordinal) || value.EndsWith("+00:00",StringComparison.Ordinal)) || !DateTimeOffset.TryParseExact(value,formats,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal,out var parsed) || parsed.Offset!=TimeSpan.Zero)return false;utc=parsed.UtcDateTime;return true;
    }
}
