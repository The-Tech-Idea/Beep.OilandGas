using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Accounting.Models;

/// <summary>Explicit company accounting source binding stored in the existing process payload.</summary>
public sealed record CompanyAccountingProcessBinding(CompanyScope Company,string InstanceId,string EntityType,string EntityId)
{
    public const string PayloadKey="CompanyAccountingBinding";
    public void Require(CompanyScope company,string instanceId,string entityType,string entityId)
    {
        APInvoiceOwnership.RequireScope(company);
        if(Company!=company || InstanceId!=instanceId || EntityType!=entityType || EntityId!=entityId ||
            EntityType is not ("AP_INVOICE" or "PURCHASE_ORDER" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT") ||
            new[]{InstanceId,EntityId}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl)))
            throw new InvalidOperationException("The recorded company accounting process binding does not match its owned source.");
    }
    public static CompanyAccountingProcessBinding FromJson(string json,CompanyScope company,string instanceId,string entityType,string entityId)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(json) || json.Length>4096)throw new InvalidOperationException("The recorded accounting process binding is unavailable.");
            using var document=JsonDocument.Parse(json);
            var root=document.RootElement;
            RequireProperties(root,[nameof(Company),nameof(InstanceId),nameof(EntityType),nameof(EntityId)]);
            RequireProperties(root.GetProperty(nameof(Company)),[nameof(CompanyScope.TenantId),nameof(CompanyScope.BusinessAssociateId)]);
            var binding=JsonSerializer.Deserialize<CompanyAccountingProcessBinding>(json)
                ?? throw new InvalidOperationException("The recorded accounting process binding is unavailable.");
            binding.Require(company,instanceId,entityType,entityId);return binding;
        }
        catch(JsonException error){throw new InvalidOperationException("The recorded accounting process binding is invalid.",error);}
    }
    public static CompanyAccountingProcessBinding FromPayloadJson(string json,string instanceId,string entityType,string entityId)
    {
        try
        {
            using var payload=JsonDocument.Parse(json);
            if(payload.RootElement.GetProperty("InstanceId").GetString()!=instanceId)
                throw new InvalidOperationException("The company process payload parent is unconfirmed.");
            var data=payload.RootElement.GetProperty("Data");
            if(data.ValueKind!=JsonValueKind.Object || data.EnumerateObject().Count(property=>property.Name==PayloadKey)!=1)
                throw new InvalidOperationException("The company process payload ownership is missing or ambiguous.");
            var ownerJson=data.GetProperty(PayloadKey).GetRawText();
            var owner=JsonSerializer.Deserialize<CompanyAccountingProcessBinding>(ownerJson) ?? throw new InvalidOperationException("Recorded company ownership is unavailable.");
            return FromJson(ownerJson,owner.Company,instanceId,entityType,entityId);
        }
        catch(Exception invalid) when(invalid is JsonException or KeyNotFoundException or ArgumentException)
        {throw new InvalidOperationException("The recorded company process ownership is invalid.",invalid);}
    }
    private static void RequireProperties(JsonElement element,string[] names)
    {
        if(element.ValueKind!=JsonValueKind.Object)throw new InvalidOperationException("The accounting process binding shape is invalid.");
        var properties=element.EnumerateObject().Select(row=>row.Name).ToArray();
        if(properties.Length!=names.Length || names.Any(name=>properties.Count(value=>value==name)!=1))
            throw new InvalidOperationException("The accounting process binding is incomplete or ambiguous.");
    }
}
