using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
namespace Beep.OilandGas.Models.Data.WorkOrder;
/// <summary>Single reader for the recorded activity's existing whitespace-delimited workflow references.</summary>
public static class ProductionInterventionMarkers
{
    private static readonly Regex Marker=new(@"(?<!\S)(?<key>WORK_ORDER_ID|AFE_ID|AFE_NUMBER|ABANDONMENT_ID|PROCESS_INSTANCE_ID|SOURCE_PROCESS):(?<value>[^\s]*)",RegexOptions.CultureInvariant,TimeSpan.FromSeconds(1));
    public static IReadOnlyDictionary<string,string> Read(string? remark)
    {
        var references=new Dictionary<string,string>(StringComparer.Ordinal);
        foreach(Match match in Marker.Matches(remark ?? string.Empty))
        {
            var key=match.Groups["key"].Value;var value=match.Groups["value"].Value;
            if(string.IsNullOrWhiteSpace(value) || !references.TryAdd(key,value))
                throw new InvalidOperationException("Recorded intervention references are empty or ambiguous; no handoff can be selected.");
        }
        return new ReadOnlyDictionary<string,string>(references);
    }
    public static string Note(string? remark)
    {
        Read(remark);return Marker.Replace(remark ?? string.Empty,string.Empty).Trim();
    }
    public static void RequireRationale(string? note)
    {
        if(Marker.IsMatch(note ?? string.Empty))throw new ArgumentException("Put workflow links in their explicit reference fields; rationale must not contain reserved workflow markers.");
    }
    public static void RequireReference(string? identity)
    {
        if(!string.IsNullOrEmpty(identity) && identity.Any(char.IsWhiteSpace))
            throw new ArgumentException("The current recorded workflow reference format requires an identity without whitespace.");
    }
}
