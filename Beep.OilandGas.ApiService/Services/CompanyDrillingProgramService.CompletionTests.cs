using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Security.Claims;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyDrillingProgramService
{
    public async Task<DrillingCompletionTestInspection> InspectCompletionTestAsync(ClaimsPrincipal user,DrillingCompletionTestSelection input)
    {
        input.Require();var selection=CompanyInventorySourceReview.Copy(input);DrillingCompletionTestInspection? result=null;
        await ReadAssignedCompletionCoreAsync(user,selection.Review.InstanceId,async(current,guard,confirm)=>
        {
            if(current.Revision!=selection.Review.Revision)throw new InvalidOperationException("The displayed completion review/source/task changed. Reload before selecting a native test.");
            result=await native.InspectCompletionTestAsync(selection,guard,confirm);result.Require(selection);
        });
        return CompanyInventorySourceReview.Copy(result??throw new InvalidOperationException("The exact native test inspection is unconfirmed."));
    }
}
