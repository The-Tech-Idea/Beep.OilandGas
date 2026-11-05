using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService
{
    private sealed record MaintenanceRowDelta(string Section,string Id,JsonElement? Before,JsonElement? After);
    private sealed record MaintenanceGraphDelta(string EvidenceKey,string BeforeRevision,string BeforeCanonicalRevision,string AfterCanonicalRevision,IReadOnlyList<MaintenanceRowDelta> Changes);
    private static readonly IReadOnlyDictionary<string,string> MaintenanceGraphKeys=new Dictionary<string,string>(StringComparer.Ordinal)
    {{"Process","PROCESS_INSTANCE_ID"},{"Steps","PROCESS_STEP_INSTANCE_ID"},{"History","PROCESS_HISTORY_ID"},{"Approvals","PROCESS_APPROVAL_ID"},{"Tasks","CROSS_TASK_ID"}};
    private static string MaintenanceCanonical(JsonNode node)
    {
        JsonNode? Sort(JsonNode? value)=>value switch
        {JsonObject obj=>new JsonObject(obj.OrderBy(pair=>pair.Key,StringComparer.Ordinal).Select(pair=>new KeyValuePair<string,JsonNode?>(pair.Key,Sort(pair.Value)))),JsonArray array=>new JsonArray(array.Select(Sort).ToArray()),_=>value?.DeepClone()};
        return Sort(node)!.ToJsonString();
    }
    private static JsonObject MaintenanceDeltaGraph(string graph,string evidenceKey)
    {
        var node=JsonNode.Parse(graph) as JsonObject??throw new InvalidOperationException("Complete native maintenance graph is required.");
        if(node.Count!=6 || node["Definition"] is not JsonObject || node["Process"] is not JsonObject process || !MaintenanceGraphKeys.Keys.Where(key=>key!="Process").All(key=>node[key] is JsonArray))throw new InvalidOperationException("Complete definition/process/step/history/approval/task graph is required.");
        var id=process["PROCESS_INSTANCE_ID"]?.GetValue<string>();if(!LeaseSummaryValidator.Identifier(id))throw new InvalidOperationException("Exact native maintenance process identity is required.");
        var payload=JsonNode.Parse(process["INSTANCE_DATA_JSON"]?.GetValue<string>()??throw new InvalidOperationException("Original maintenance payload is unavailable.")) as JsonObject??throw new InvalidOperationException("Original maintenance payload is malformed.");
        if(payload["Data"] is not JsonObject data)throw new InvalidOperationException("Original maintenance data dictionary is required.");data.Remove(evidenceKey);process["INSTANCE_DATA_JSON"]=MaintenanceCanonical(payload);
        foreach(var (section,key) in MaintenanceGraphKeys.Where(pair=>pair.Key!="Process"))
        {
            var rows=(JsonArray)node[section]!;if(rows.Count>4096 || rows.Any(row=>row is not JsonObject || !LeaseSummaryValidator.Identifier(row[key]?.GetValue<string>()) || row["PROCESS_INSTANCE_ID"]?.GetValue<string>()!=id) || rows.Select(row=>row![key]!.GetValue<string>()).Distinct(StringComparer.Ordinal).Count()!=rows.Count)throw new InvalidOperationException("Complete unique native maintenance row ownership is required.");
            node[section]=new JsonArray(rows.OrderBy(row=>row![key]!.GetValue<string>(),StringComparer.Ordinal).Select(row=>row!.DeepClone()).ToArray());
        }
        return node;
    }
    private static Dictionary<string,JsonNode> MaintenanceSection(JsonObject graph,string section)
    {
        var key=MaintenanceGraphKeys[section];return section=="Process"?new(StringComparer.Ordinal){{graph[section]![key]!.GetValue<string>(),graph[section]!}}:((JsonArray)graph[section]!).ToDictionary(row=>row![key]!.GetValue<string>(),row=>row!,StringComparer.Ordinal);
    }
    private static MaintenanceGraphDelta CaptureMaintenanceDelta(string evidenceKey,string beforeGraph,string afterGraph)
    {
        if(!LeaseSummaryValidator.Identifier(evidenceKey))throw new InvalidOperationException("An exact maintenance evidence key is required.");var originalRaw=JsonNode.Parse(beforeGraph)!.AsObject();var before=MaintenanceDeltaGraph(beforeGraph,evidenceKey);var after=MaintenanceDeltaGraph(afterGraph,evidenceKey);
        if(!JsonNode.DeepEquals(before["Definition"],after["Definition"]))throw new InvalidOperationException("A recorded maintenance transition cannot alter its original definition.");var changes=new List<MaintenanceRowDelta>();
        foreach(var section in MaintenanceGraphKeys.Keys)
        {
            var original=MaintenanceSection(before,section);var current=MaintenanceSection(after,section);
            foreach(var id in original.Keys.Union(current.Keys,StringComparer.Ordinal).Order(StringComparer.Ordinal))
            {
                original.TryGetValue(id,out var previous);current.TryGetValue(id,out var next);if(JsonNode.DeepEquals(previous,next))continue;
                if(next is null || previous is null && section is not ("History" or "Approvals" or "Tasks") || previous is not null && (section is "History" or "Approvals"))throw new InvalidOperationException("Only declared maintenance updates and append-only history/approval/task inserts are allowed.");
                changes.Add(new(section,id,previous is null?null:JsonSerializer.SerializeToElement(MaintenanceSection(originalRaw,section)[id]),JsonSerializer.SerializeToElement(next)));
            }
        }
        if(changes.Count is <1 or >512)throw new InvalidOperationException("A bounded nonempty native maintenance delta is required.");
        return new(evidenceKey,ProcessDefinitionRevision.FromJson(beforeGraph),ProcessDefinitionRevision.FromJson(MaintenanceCanonical(before)),ProcessDefinitionRevision.FromJson(MaintenanceCanonical(after)),changes);
    }
    private static string RestoreMaintenanceDelta(string currentGraph,MaintenanceGraphDelta delta)
    {
        if(!LeaseSummaryValidator.Identifier(delta.EvidenceKey) || new[]{delta.BeforeRevision,delta.BeforeCanonicalRevision,delta.AfterCanonicalRevision}.Any(value=>!ProcessDefinitionRevision.IsValid(value)) || delta.Changes is not {Count:>0 and <=512} || delta.Changes.Select(row=>(row.Section,row.Id)).Distinct().Count()!=delta.Changes.Count)throw new InvalidOperationException("Complete original maintenance delta/source revisions are required.");
        var graph=MaintenanceDeltaGraph(currentGraph,delta.EvidenceKey);if(ProcessDefinitionRevision.FromJson(MaintenanceCanonical(graph))!=delta.AfterCanonicalRevision)throw new InvalidOperationException("Current native maintenance graph differs from the recorded transition result.");
        foreach(var change in delta.Changes)
        {
            if(!MaintenanceGraphKeys.ContainsKey(change.Section) || change.After is null)throw new InvalidOperationException("Declared native maintenance change is malformed.");var rows=MaintenanceSection(graph,change.Section);
            if(!rows.TryGetValue(change.Id,out var actual) || !JsonNode.DeepEquals(actual,JsonNode.Parse(change.After.Value.GetRawText())))throw new InvalidOperationException("Exact recorded native transition row changed.");
            if(change.Section=="Process"){if(change.Before is null)throw new InvalidOperationException("Original native process row is required.");graph["Process"]=JsonNode.Parse(change.Before.Value.GetRawText());}
            else
            {
                var key=MaintenanceGraphKeys[change.Section];var array=(JsonArray)graph[change.Section]!;var index=array.Select((row,i)=>(row,i)).Single(pair=>pair.row![key]!.GetValue<string>()==change.Id).i;
                if(change.Before is null)array.RemoveAt(index);else array[index]=JsonNode.Parse(change.Before.Value.GetRawText());
            }
        }
        var restored=graph.ToJsonString();if(ProcessDefinitionRevision.FromJson(restored)!=delta.BeforeRevision)throw new InvalidOperationException("Restored native maintenance rows do not match the original reviewed graph revision.");
        graph=MaintenanceDeltaGraph(restored,delta.EvidenceKey);if(ProcessDefinitionRevision.FromJson(MaintenanceCanonical(graph))!=delta.BeforeCanonicalRevision)throw new InvalidOperationException("Restored native maintenance source graph differs from its retained original revision.");return restored;
    }
}
