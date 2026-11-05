using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public sealed partial class WorkOrderExecutionPermitService
{
    public async Task<IReadOnlyList<CompanyWorkOrderSubmissionFacts>> ReadRecordedWorkAsync(FieldSelectionReview context,CompanyMaintenanceRecordedWorkWindow window,IReadOnlyList<string> authorizedOrders,Func<Task> authority)
    {
        window.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(context.FieldId!=window.FieldId || authorizedOrders is null || authorizedOrders.Count>CompanyMaintenanceRecordedWorkWindow.MaximumOrders || authorizedOrders.Any(order=>!LeaseSummaryValidator.Identifier(order) || window.WorkOrderId is not null && window.WorkOrderId!=order) || authorizedOrders.Distinct(StringComparer.Ordinal).Count()!=authorizedOrders.Count)throw new UnauthorizedAccessException("Complete current FIELD and authorized original work-order population are required.");
        var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original recorded-work CORE source is unavailable.");
        async Task Guard(){PPDMDataSourceTransaction.RequireIdle(source);await authority();if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original recorded-work CORE source changed.");RequireSubmissionSchema(source);}
        var repo=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_SUBMISSION),context.Database.ConnectionName,SubmissionTable);
        async Task<CompanyWorkOrderSubmissionFacts[]> Read()
        {
            await Guard();var results=new List<CompanyWorkOrderSubmissionFacts>();long bodyLength=0;
            foreach(var order in authorizedOrders.Order(StringComparer.Ordinal))
            {
                await Guard();var rows=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},new(){FieldName="FIELD_ID",Operator="=",FilterValue=window.FieldId},new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=order},new(){FieldName="ROW_CREATED_DATE",Operator=">=",FilterValue=window.FromUtc.ToString("O",CultureInfo.InvariantCulture),FieldType=typeof(DateTime),valueType=typeof(DateTime).FullName!},new(){FieldName="ROW_CREATED_DATE",Operator="<",FilterValue=window.ToUtc.ToString("O",CultureInfo.InvariantCulture),FieldType=typeof(DateTime),valueType=typeof(DateTime).FullName!}])).ToArray();
                if(rows.Length>CompanyMaintenanceRecordedWorkWindow.MaximumRecords || rows.Any(row=>row is not WORK_ORDER_EXECUTION_SUBMISSION))throw new InvalidOperationException("Complete bounded native report membership is required; no source rows are truncated.");
                foreach(var value in rows)
                {
                    var row=CompanyInventorySourceReview.Copy((WORK_ORDER_EXECUTION_SUBMISSION)value);
                    if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.FIELD_ID!=window.FieldId || row.WORK_ORDER_ID!=order || row.ROW_CREATED_DATE is not {} at || at.Kind!=DateTimeKind.Utc || at<window.FromUtc || at>=window.ToUtc)throw new UnauthorizedAccessException("Native reports must honor the exact company/FIELD/order/recorded-UTC predicates before private proof decoding.");
                    if(string.IsNullOrWhiteSpace(row.COMMAND_JSON) || row.COMMAND_JSON.Length>1024*1024 || (bodyLength+=row.COMMAND_JSON.Length)>16*1024*1024)throw new InvalidOperationException("Recorded-work proof exceeds this bounded source review; select a narrower period or exact work order.");
                    CompanyWorkOrderSubmissionFacts facts;
                    try{facts=CompanyWorkOrderSubmissionFacts.From(row);}
                    catch(JsonException malformed){throw new InvalidOperationException("Original recorded-work proof is malformed.",malformed);}
                    catch(ArgumentException malformed){throw new InvalidOperationException("Original recorded-work proof is invalid.",malformed);}
                    if(facts.Database!=context.Database || !window.Includes(facts))throw new InvalidOperationException("Original report proof differs from its current authorized CORE source.");results.Add(facts);
                    if(results.Count>CompanyMaintenanceRecordedWorkWindow.MaximumRecords)throw new InvalidOperationException("Recorded-work scope exceeds bounded complete membership; refine the selected period or work order.");
                }
                await Guard();
            }
            if(results.Select(row=>row.Query.SubmissionId).Distinct(StringComparer.Ordinal).Count()!=results.Count)throw new InvalidOperationException("Original recorded-work submission membership is ambiguous.");
            await Guard();return results.OrderByDescending(row=>row.RecordedAtUtc).ThenBy(row=>row.Query.SubmissionId,StringComparer.Ordinal).ToArray();
        }
        var first=await Read();var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original complete recorded-work membership changed during source review.");return closing;
    }
}
