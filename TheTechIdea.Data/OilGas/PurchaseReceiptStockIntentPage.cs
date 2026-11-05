using System.Text.Json;
namespace TheTechIdea.Data.OilGas;
public sealed record PurchaseReceiptStockIntentPage(PurchaseReceiptStockIntentIndex Index,int PageNumber,int PageSize,int TotalRecords,string Revision)
{
    public int TotalPages=>TotalRecords==0?0:(int)(((long)TotalRecords+PageSize-1)/PageSize);
    public bool HasNextPage=>(long)PageNumber*PageSize<TotalRecords;
    public bool HasPreviousPage=>PageNumber>1;
    public static void RequireRequest(int page,int size)
    {if(page<1 || size<1 || size>50 || (long)(page-1)*size>int.MaxValue)throw new InvalidOperationException("A bounded original stock discovery page is required.");}
    public void Require(CompanyScope company,string actor,int page,int size)
    {
        RequireRequest(page,size);if(Index is null || PageNumber!=page || PageSize!=size || TotalRecords<0 || PageNumber>Math.Max(1,TotalPages))throw new InvalidOperationException("Original stock page metadata is unconfirmed.");
        Index.Require(company,actor);
        var expected=(int)Math.Min(size,Math.Max(0L,TotalRecords-(long)(page-1)*size));
        if(Index.Entries.Count!=expected || Revision!=Digest(Index,page,size,TotalRecords))throw new InvalidOperationException("Complete stock discovery page or revision is unconfirmed.");
    }
    public static PurchaseReceiptStockIntentPage Create(PurchaseReceiptStockIntentIndex index,int page,int size,int total)
    {
        var result=new PurchaseReceiptStockIntentPage(CompanyInventorySourceReview.Copy(index),page,size,total,Digest(index,page,size,total));result.Require(index.Company,index.ActorUserId,page,size);return result;
    }
    private static string Digest(PurchaseReceiptStockIntentIndex index,int page,int size,int total)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{index.Revision,PageNumber=page,PageSize=size,TotalRecords=total}));
}
