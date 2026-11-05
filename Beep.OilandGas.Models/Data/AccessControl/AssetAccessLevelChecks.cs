namespace Beep.OilandGas.Models.Data.AccessControl;
/// <summary>Resource levels are independent of application permissions.</summary>
public static class AssetAccessLevelChecks
{
    private static int Rank(string? value)=>value?.ToUpperInvariant() switch{"READ"=>1,"WRITE"=>2,"DELETE"=>3,_=>0};
    public static bool RequiresApplicationPermission(string? requestedPermission) =>
        !string.IsNullOrWhiteSpace(requestedPermission) && Rank(requestedPermission) == 0;
    public static bool Meets(string? actual,string? requestedPermission)
    {
        var available=Rank(actual);
        if(available==0)return false;
        var requested=Rank(requestedPermission);
        // Other application permission identifiers are decided by the existing app permission owner.
        return requested==0 || available>=requested;
    }
}
