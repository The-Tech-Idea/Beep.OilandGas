using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;

namespace TheTechIdea.Data.OilGas;

/// <summary>Complete existing native order/components reviewed for an intervention link.</summary>
public sealed record ProductionInterventionWorkOrderReview(WORK_ORDER Header, WORK_ORDER_COMPONENT[] Components, string Revision)
{
    public static ProductionInterventionWorkOrderReview Create(WORK_ORDER header, IEnumerable<WORK_ORDER_COMPONENT> components)
    {
        var value = new ProductionInterventionWorkOrderReview(CompanyInventorySourceReview.Copy(header),
            components.OrderBy(row => row.COMPONENT_ID, StringComparer.Ordinal).Select(CompanyInventorySourceReview.Copy).ToArray(), string.Empty);
        return value with { Revision = Digest(value) };
    }

    public void Require(string order, string field, string well)
    {
        if (!LeaseSummaryValidator.Identifier(order) || !LeaseSummaryValidator.Identifier(field) || !LeaseSummaryValidator.Identifier(well) ||
            Header is null || Header.WORK_ORDER_ID != order || Header.ACTIVE_IND != "Y" || Header.WORK_ORDER_TYPE != WorkOrderSubType.Corrective ||
            Header.COMPLETE_DATE is not null || Components is not { Length: > 0 and <= 256 } || Revision != Digest(this) ||
            Components.Any(row => row is null || row.WORK_ORDER_ID != order || row.ACTIVE_IND != "Y" ||
                !LeaseSummaryValidator.Identifier(row.COMPONENT_ID) || !LeaseSummaryValidator.Identifier(row.FIELD_ID)) ||
            Components.Select(row => row.COMPONENT_ID).Distinct(StringComparer.Ordinal).Count() != Components.Length ||
            !Components.Any(row => row.FIELD_ID == field && row.UWI == well))
            throw new InvalidOperationException("An active incomplete corrective WORK_ORDER and its exact recorded FIELD/well component are required.");
    }

    private static string Digest(ProductionInterventionWorkOrderReview value) => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with { Revision = string.Empty }));
}

/// <summary>The existing company row policy owns order READ grants and all contributing FIELD entitlements.</summary>
public interface IProductionInterventionWorkOrderReader
{
    Task<ProductionInterventionWorkOrderReview> ReviewInterventionWorkOrderAsync(FieldSelectionReview context, string order, string well);
}
