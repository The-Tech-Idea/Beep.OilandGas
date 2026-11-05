using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
namespace Beep.OilandGas.Models.Data.WorkOrder;
/// <summary>Recorded labor quantity/source audit for authorized financial readers; no executor report or private recovery body.</summary>
public sealed record CompanyWorkOrderLaborUsageFacts(CompanyScope Company,AssetDatabaseScope Database,string FieldId,string WorkOrderId,
    string LaborId,string StartId,string AssignmentId,string EmployeeBaId,string ExecutorUserId,string DutyCode,
    DateTime FromUtc,DateTime ToUtc,decimal Quantity,WorkOrderLaborTimeUnit Unit,DateTime RecordedAtUtc,string Revision)
{
    public void Require()
    {
        if(Company is null || Database is null || new[]{Company.TenantId,Company.BusinessAssociateId,Database.ConnectionName,Database.Fingerprint,
            FieldId,WorkOrderId,LaborId,StartId,AssignmentId,EmployeeBaId,ExecutorUserId,DutyCode}.Any(value=>!LeaseSummaryValidator.Identifier(value)) ||
            FromUtc.Kind!=DateTimeKind.Utc || ToUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc.Kind!=DateTimeKind.Utc || ToUtc<=FromUtc || RecordedAtUtc<ToUtc ||
            Quantity<=0 || !ProcessDefinitionRevision.IsValid(Revision))throw new InvalidOperationException("Complete original labor/source/employee/duty/time quantity and audit are required.");
        var ticks=ToUtc.Ticks-FromUtc.Ticks;var unitTicks=CompanyWorkOrderLaborUsageCommand.TicksPerUnit(Unit);
        if(Quantity>(decimal)ticks/unitTicks || Quantity*unitTicks!=ticks)throw new InvalidOperationException("Recorded labor quantity/unit differs from its original actual interval.");
    }
    public static CompanyWorkOrderLaborUsageFacts From(WORK_ORDER_LABOR_USAGE row)
    {
        var command=CompanyWorkOrderLaborUsageCommand.Read(row);var context=command.Review.Execution.Current.Context;
        var result=new CompanyWorkOrderLaborUsageFacts(context.Company,context.Database,row.FIELD_ID,row.WORK_ORDER_ID,row.LABOR_ID,row.START_ID,row.ASSIGNMENT_ID,
            row.EMPLOYEE_BA_ID,row.EXECUTOR_USER_ID,row.DUTY_CODE,row.LABOR_FROM_UTC,row.LABOR_TO_UTC,row.QUANTITY,command.Unit,row.ROW_CREATED_DATE!.Value,row.RECORDED_REVISION);result.Require();return result;
    }
}
