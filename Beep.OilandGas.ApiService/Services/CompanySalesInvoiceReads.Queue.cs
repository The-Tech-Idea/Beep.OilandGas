using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanySalesInvoiceReads
{
    public async Task<CompanySalesInvoiceQueueReview> ReadQueueAsync(ClaimsPrincipal user,string field)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        if(string.IsNullOrWhiteSpace(field) || field.Length>128 || field.Any(char.IsControl))throw new ArgumentException("Choose a bounded original FIELD.");
        var authority=await Authority(user,false,field:field);
        async Task<CompanySalesInvoiceQueueReview> Capture()
        {await authority.Guard();var rows=await invoices.GetCompanySalesInvoiceQueueAsync(authority.Company,field,authority.Guard);await authority.Guard();return CompanySalesInvoiceQueueReview.Create(authority.Company,authority.Actor,field,authority.Binding,rows);}
        var first=await Capture();var closing=await Capture();await authority.Guard();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The recorded current invoice queue changed during read-back.");return CompanyInventorySourceReview.Copy(closing);
    }
}
