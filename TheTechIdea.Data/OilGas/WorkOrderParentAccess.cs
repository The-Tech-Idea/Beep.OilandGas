using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;
public static class WorkOrderParentAccess
{
    public static string[] Fields(string order,IReadOnlyList<WorkOrderComponentReference> components)
    {
        if(string.IsNullOrWhiteSpace(order) || components is null || components.Count==0 || components.Any(c=>c is null || c.WorkOrderId!=order || string.IsNullOrWhiteSpace(c.ComponentId) || string.IsNullOrWhiteSpace(c.FieldId)) ||
           components.Select(c=>c.ComponentId).Distinct(StringComparer.Ordinal).Count()!=components.Count)
            throw new InvalidOperationException("The recorded order has no complete unambiguous FIELD component scope.");
        return components.Select(c=>c.FieldId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }
    public static string? Intersect(string? grant,IEnumerable<string?> parentLevels)
    {
        static int Rank(string? level)=>level switch{"READ"=>1,"WRITE"=>2,"DELETE"=>3,_=>0};
        var parents=parentLevels.ToArray();if(parents.Length==0)return null;
        var rank=Math.Min(Rank(grant),parents.Min(Rank));return rank switch{1=>"READ",2=>"WRITE",3=>"DELETE",_=>null};
    }
}
