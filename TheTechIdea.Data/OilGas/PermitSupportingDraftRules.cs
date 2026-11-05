using Beep.OilandGas.Models.Data.PermitsAndApplications;
namespace TheTechIdea.Data.OilGas;

internal static class PermitSupportingDraftRules
{
    public static void Stage(PermitApplicationSourceReview review,PermitApplicationType kind,bool requireWell)
    {
        var parent=review.ReadApplication();
        if(parent.APPLICATION_TYPE!=kind || parent.STATUS is not (PermitApplicationStatus.Draft or PermitApplicationStatus.AdditionalInformationRequired) ||
            requireWell && !LeaseSummaryValidator.Identifier(parent.WELL_ID))
            throw new InvalidOperationException("Use the exact supporting type on a current draft or correction application with its required recorded well.");
    }
    public static void Text(int bound,bool multiline,params string?[] values)
    {
        if(values.Any(x=>x is not null && (x.Length>bound || x!=x.Trim() || x.Any(c=>char.IsControl(c) && !(multiline && c is ('\r' or '\n'))))))
            throw new InvalidOperationException($"Supporting values must be trimmed and bounded to {bound} characters.");
    }
    public static void Named(params string?[] values)
    {
        Text(128,false,values);
        if(values.Any(string.IsNullOrWhiteSpace))throw new InvalidOperationException("Explicit supporting type and fluid values are required.");
    }
    public static void Flags(params string?[] flags)
    {
        if(flags.Any(x=>x is not (null or "Y" or "N")))throw new InvalidOperationException("Supporting indicators must be unknown, Y or N.");
    }
    public static void Measurement(decimal? value,string? unit,bool zeroAllowed)
    {
        Text(128,false,unit);
        if(value is null ? unit is not null : string.IsNullOrWhiteSpace(unit) || (zeroAllowed?value<0:value<=0))
            throw new InvalidOperationException("Supply a supported positive measurement (zero only for waste volume) with its explicit recorded unit, or leave both unknown.");
    }
}
