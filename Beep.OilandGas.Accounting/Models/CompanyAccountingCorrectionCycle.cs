using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.Accounting.Models;
/// <summary>One declared correction return cycle in the existing company process payload.</summary>
public sealed record CompanyAccountingCorrectionCycle(CompanyScope Company,string InstanceId,string WorkStepInstanceId,string AcknowledgementStepInstanceId,
    string ReturnIntentId,int CorrectionVersion,string CorrectionIntentId,string OriginalPreparerUserId)
{
    public const string PayloadKey="CompanyAccountingCorrectionCycle";
    public void Require(CompanyAccountingProcessBinding binding)
    {
        binding.Require(Company,InstanceId,"AP_INVOICE",binding.EntityId);
        static bool Intent(string? value)=>Guid.TryParseExact(value,"N",out var id) && id!=Guid.Empty && id.ToString("N")==value;
        if(new[]{WorkStepInstanceId,AcknowledgementStepInstanceId,OriginalPreparerUserId}.Any(value=>string.IsNullOrWhiteSpace(value) || value=="SYSTEM" || value.Length>128 || value.Any(char.IsControl)) ||
            WorkStepInstanceId==AcknowledgementStepInstanceId || CorrectionVersion is <2 or >APInvoiceCorrectionHistory.MaximumVersions || !Intent(ReturnIntentId) || !Intent(CorrectionIntentId))
            throw new InvalidOperationException("A bounded original-preparer correction return cycle is required.");
    }
    public static CompanyAccountingCorrectionCycle? FromPayload(string json,string instance,string type,string entity)
    {
        try
        {
            using var document=JsonDocument.Parse(json);var data=document.RootElement.GetProperty("Data");
            if(!data.TryGetProperty(PayloadKey,out var recorded))return null;
            if(data.EnumerateObject().Count(property=>property.Name==PayloadKey)!=1)throw new InvalidOperationException("The correction cycle is ambiguous.");
            var result=JsonSerializer.Deserialize<CompanyAccountingCorrectionCycle>(recorded.GetRawText(),new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow})??throw new InvalidOperationException("The correction cycle is unavailable.");
            var binding=CompanyAccountingProcessBinding.FromPayloadJson(json,instance,type,entity);result.Require(binding);return result;
        }
        catch(Exception invalid) when(invalid is JsonException or KeyNotFoundException){throw new InvalidOperationException("The recorded correction cycle is invalid.",invalid);}
    }
    public string? RoundFor(string step)=>step==WorkStepInstanceId || step==AcknowledgementStepInstanceId?ReturnIntentId:null;
    public static string? TaskRound(string? context)
    {
        try
        {
            using var document=JsonDocument.Parse(context??throw new InvalidOperationException("Recorded task context is required."));
            if(!document.RootElement.TryGetProperty("correctionCycle",out var round))return null;
            var value=round.GetString();if(document.RootElement.EnumerateObject().Count(property=>property.Name=="correctionCycle")!=1 || !Guid.TryParseExact(value,"N",out var id) || id==Guid.Empty || id.ToString("N")!=value)
                throw new InvalidOperationException("The task correction-cycle identity is invalid.");return value;
        }
        catch(Exception invalid) when(invalid is JsonException or InvalidOperationException){throw new InvalidOperationException("The recorded task correction cycle is invalid.",invalid);}
    }
}
