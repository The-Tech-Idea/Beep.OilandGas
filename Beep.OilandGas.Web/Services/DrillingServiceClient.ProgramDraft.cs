using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;

namespace Beep.OilandGas.Web.Services;
public partial class DrillingServiceClient
{
    public async Task<DrillingProgramDraftList> GetProgramDraftsAsync(string processId,string afeId,string wellUwi,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !LeaseSummaryValidator.Identifier(afeId) || string.IsNullOrWhiteSpace(wellUwi) || wellUwi.Length>128 || wellUwi.Any(char.IsControl))throw new InvalidOperationException("Choose an exact approved well/program source.");var result=await _apiClient.GetAsync<DrillingProgramDraftList>("/api/drilling/programs/drafts?processId="+Uri.EscapeDataString(processId)+"&afeId="+Uri.EscapeDataString(afeId)+"&wellUwi="+Uri.EscapeDataString(wellUwi),cancellationToken)??throw new InvalidOperationException("The original program rowset is unavailable.");result.Require();if(result.Basis.Well.Uwi!=wellUwi || result.Basis.Funding.Source.AfeId!=afeId || result.Basis.Funding.Source.Current.Source.Review.InstanceId!=processId)throw new InvalidOperationException("The programs belong to another original well/funding source.");return result;
    }
    public async Task<DrillingProgramBasis> ReviewProgramBasisAsync(string processId,string afeId,string wellUwi,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !LeaseSummaryValidator.Identifier(afeId) || string.IsNullOrWhiteSpace(wellUwi) || wellUwi.Length>128 || wellUwi.Any(char.IsControl))throw new InvalidOperationException("Choose the exact approved AFE source and recorded well.");
        var value=await _apiClient.GetAsync<DrillingProgramBasis>("/api/drilling/programs/basis?processId="+Uri.EscapeDataString(processId)+"&afeId="+Uri.EscapeDataString(afeId)+"&wellUwi="+Uri.EscapeDataString(wellUwi),cancellationToken)??throw new InvalidOperationException("The approved program basis is unavailable.");
        value.Require();if(value.Funding.Source.Current.Source.Review.InstanceId!=processId || value.Funding.Source.AfeId!=afeId || value.Well.Uwi!=wellUwi)throw new InvalidOperationException("The program basis belongs to another original source or well.");return value;
    }
    public Task<DrillingProgramDraftReceipt> RecordProgramDraftAsync(CreateDrillingProgramDraft original,CancellationToken cancellationToken=default)=>ProgramDraft(original,false,cancellationToken);
    public Task<DrillingProgramDraftReceipt> RecoverProgramDraftAsync(CreateDrillingProgramDraft original,CancellationToken cancellationToken=default)=>ProgramDraft(original,true,cancellationToken);
    private async Task<DrillingProgramDraftReceipt> ProgramDraft(CreateDrillingProgramDraft original,bool recover,CancellationToken cancellationToken)
    {
        original.Require();var value=await _apiClient.PostAsync<CreateDrillingProgramDraft,DrillingProgramDraftReceipt>("/api/drilling/programs/drafts"+(recover?"/recover":""),original,cancellationToken)??throw new InvalidOperationException("The original drilling program outcome is unconfirmed. Recover its retained input.");
        value.Require(original);return value;
    }
}
