using System.Text.Json;
namespace TheTechIdea.Data.OilGas;
/// <summary>A complete independently reviewed page; totals count saved proposals, not unresolved outcomes.</summary>
public sealed record CompanyCommercialAmendmentPage(CompanyCommercialAmendmentQueue Queue,int PageNumber,int PageSize,int TotalRecords,string Revision)
{
    public int TotalPages=>TotalRecords==0?0:(int)(((long)TotalRecords+PageSize-1)/PageSize);
    public bool HasNextPage=>(long)PageNumber*PageSize<TotalRecords;
    public bool HasPreviousPage=>PageNumber>1;
    public static void RequireRequest(int page,int size)
    {
        if(page<1 || size<1 || size>CompanyCommercialAmendmentQueue.MaximumEntries || (long)(page-1)*size>int.MaxValue)throw new ArgumentException("A bounded original commercial proposal page is required.");
    }
    public void Require(CompanyScope company,string actor,int page,int size)
    {
        RequireRequest(page,size);
        if(Queue is null || PageNumber!=page || PageSize!=size || TotalRecords<0 || PageNumber>Math.Max(1,TotalPages))throw new InvalidOperationException("The original proposal page metadata is unconfirmed.");
        Queue.Require(company,actor);
        var expected=(int)Math.Min(size,Math.Max(0L,TotalRecords-(long)(page-1)*size));
        if(Queue.Rows.Count!=expected || Revision!=Digest(Queue,page,size,TotalRecords))throw new InvalidOperationException("The complete proposal page and counts are unconfirmed.");
    }
    public static CompanyCommercialAmendmentPage Create(CompanyCommercialAmendmentQueue queue,int page,int size,int total)
    {
        var result=new CompanyCommercialAmendmentPage(CompanyInventorySourceReview.Copy(queue),page,size,total,Digest(queue,page,size,total));result.Require(queue.Company,queue.ActorUserId,page,size);return result;
    }
    private static string Digest(CompanyCommercialAmendmentQueue queue,int page,int size,int total)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{queue.Revision,PageNumber=page,PageSize=size,TotalRecords=total}));
}
