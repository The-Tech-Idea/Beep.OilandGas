using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
public static class WorkOrderInspectionMetadata
{
    public static Dictionary<string,PPDMTableMetadata> AddTo(Dictionary<string,PPDMTableMetadata> metadata)
    {
        const string name="WORK_ORDER_INSPECTION";
        const string permit="WORK_ORDER_EXECUTION_PERMIT";
        const string start="WORK_ORDER_EXECUTION_START";
        const string submission="WORK_ORDER_EXECUTION_SUBMISSION";
        const string labor="WORK_ORDER_LABOR_USAGE";
        const string assetReturn="WORK_ORDER_ASSET_RETURN";
        if(metadata.ContainsKey(assetReturn))throw new InvalidOperationException("Asset-return intent metadata already has another owner.");
        if(metadata.ContainsKey(labor))throw new InvalidOperationException("Work-order labor metadata already has another owner.");
        if(metadata.ContainsKey(submission))throw new InvalidOperationException("Work submission metadata already has another owner.");
        if(metadata.ContainsKey(start))throw new InvalidOperationException("Execution start metadata already has another owner.");
        const string assignment="WORK_ORDER_PERSONNEL_ASSIGNMENT";
        const string release="WORK_ORDER_PERSONNEL_RELEASE";
        const string policy="WORK_ORDER_PERSONNEL_POLICY";
        const string maintenancePolicy="WORK_ORDER_MAINTENANCE_RETURN_POLICY";
        if(metadata.ContainsKey(maintenancePolicy))throw new InvalidOperationException("Maintenance return policy metadata already has another owner.");
        const string identity="COMPANY_PERSONNEL_IDENTITY";if(metadata.ContainsKey(identity))throw new InvalidOperationException("Personnel identity metadata already has another owner.");
        if(metadata.ContainsKey(policy))throw new InvalidOperationException("Personnel policy metadata already has another owner.");
        if(metadata.ContainsKey(release))throw new InvalidOperationException("Personnel release metadata already has another owner.");
        if(metadata.ContainsKey(assignment))throw new InvalidOperationException("Personnel assignment metadata already has another owner.");
        if(metadata.ContainsKey(name))throw new InvalidOperationException("WORK_ORDER_INSPECTION metadata is already declared outside its canonical extension owner.");
        if(metadata.ContainsKey(permit))throw new InvalidOperationException("Work-order execution permit metadata is already declared outside its canonical extension owner.");
        WorkOrderDocumentSourceMetadata.Complete(metadata);
        metadata.Add(assetReturn,new(){TableName=assetReturn,EntityTypeName=typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_ASSET_RETURN).FullName!,PrimaryKeyColumn="RETURN_ID",Module="PPDM_CORE",SubjectArea="Work Orders",CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID","ROW_EFFECTIVE_DATE","ROW_EXPIRY_DATE","ROW_QUALITY"],ForeignKeys=[new(){ForeignKeyColumn="WORK_ORDER_ID",ReferencedTable="WORK_ORDER",ReferencedPrimaryKey="WORK_ORDER_ID"},new(){ForeignKeyColumn="FIELD_ID",ReferencedTable="FIELD",ReferencedPrimaryKey="FIELD_ID"},new(){ForeignKeyColumn="SUBMISSION_ID",ReferencedTable="WORK_ORDER_EXECUTION_SUBMISSION",ReferencedPrimaryKey="SUBMISSION_ID"}]});
        metadata.Add(name,new(){TableName=name,EntityTypeName=typeof(WORK_ORDER_INSPECTION).FullName!,PrimaryKeyColumn="INSPECTION_ID",Module="PPDM_CORE",SubjectArea="Work Orders",
            CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID","ROW_EFFECTIVE_DATE","ROW_EXPIRY_DATE","ROW_QUALITY"],
            ForeignKeys=[new(){ForeignKeyColumn="WORK_ORDER_ID",ReferencedTable="WORK_ORDER",ReferencedPrimaryKey="WORK_ORDER_ID"},new(){ForeignKeyColumn="FIELD_ID",ReferencedTable="FIELD",ReferencedPrimaryKey="FIELD_ID"}]});
        metadata.Add(permit,new(){TableName=permit,EntityTypeName=typeof(WORK_ORDER_EXECUTION_PERMIT).FullName!,PrimaryKeyColumn="PERMIT_ID",Module="PPDM_CORE",SubjectArea="Work Orders",
            CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID"],
            ForeignKeys=[new(){ForeignKeyColumn="WORK_ORDER_ID",ReferencedTable="WORK_ORDER",ReferencedPrimaryKey="WORK_ORDER_ID"},new(){ForeignKeyColumn="FIELD_ID",ReferencedTable="FIELD",ReferencedPrimaryKey="FIELD_ID"}]});
        metadata.Add(assignment,new(){TableName=assignment,EntityTypeName=typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_PERSONNEL_ASSIGNMENT).FullName!,PrimaryKeyColumn="ASSIGNMENT_ID",Module="PPDM_CORE",SubjectArea="Work Orders",CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID"],ForeignKeys=[new(){ForeignKeyColumn="WORK_ORDER_ID",ReferencedTable="WORK_ORDER",ReferencedPrimaryKey="WORK_ORDER_ID"},new(){ForeignKeyColumn="FIELD_ID",ReferencedTable="FIELD",ReferencedPrimaryKey="FIELD_ID"},new(){ForeignKeyColumn="EMPLOYEE_BA_ID",ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"}]});
        metadata.Add(release,new(){TableName=release,EntityTypeName=typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_PERSONNEL_RELEASE).FullName!,PrimaryKeyColumn="RELEASE_ID",Module="PPDM_CORE",SubjectArea="Work Orders",CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID"],ForeignKeys=[new(){ForeignKeyColumn="ASSIGNMENT_ID",ReferencedTable="WORK_ORDER_PERSONNEL_ASSIGNMENT",ReferencedPrimaryKey="ASSIGNMENT_ID"},new(){ForeignKeyColumn="WORK_ORDER_ID",ReferencedTable="WORK_ORDER",ReferencedPrimaryKey="WORK_ORDER_ID"},new(){ForeignKeyColumn="FIELD_ID",ReferencedTable="FIELD",ReferencedPrimaryKey="FIELD_ID"}]});
        metadata.Add(policy,new(){TableName=policy,EntityTypeName=typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_PERSONNEL_POLICY).FullName!,PrimaryKeyColumn="POLICY_RECORD_ID",Module="PPDM_CORE",SubjectArea="Work Orders",CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID"]});
        metadata.Add(maintenancePolicy,new(){TableName=maintenancePolicy,EntityTypeName=typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_MAINTENANCE_RETURN_POLICY).FullName!,PrimaryKeyColumn="POLICY_RECORD_ID",Module="PPDM_CORE",SubjectArea="Maintenance Return Policy",CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID"]});
        metadata.Add(identity,new(){TableName=identity,EntityTypeName=typeof(Beep.OilandGas.Models.Data.WorkOrder.COMPANY_PERSONNEL_IDENTITY).FullName!,PrimaryKeyColumn="IDENTITY_RECORD_ID",Module="PPDM_CORE",SubjectArea="Personnel Identity",CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID"],ForeignKeys=[new(){ForeignKeyColumn="PERSON_BA_ID",ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"},new(){ForeignKeyColumn="OWNER_COMPANY_BA_ID",ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"}]});
        metadata.Add(start,new(){TableName=start,EntityTypeName=typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_START).FullName!,PrimaryKeyColumn="START_ID",Module="PPDM_CORE",SubjectArea="Work Orders",CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID"],ForeignKeys=[new(){ForeignKeyColumn="PERMIT_ID",ReferencedTable=permit,ReferencedPrimaryKey="PERMIT_ID"},new(){ForeignKeyColumn="WORK_ORDER_ID",ReferencedTable="WORK_ORDER",ReferencedPrimaryKey="WORK_ORDER_ID"},new(){ForeignKeyColumn="FIELD_ID",ReferencedTable="FIELD",ReferencedPrimaryKey="FIELD_ID"},new(){ForeignKeyColumn="EMPLOYEE_BA_ID",ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"}]});
        metadata.Add(submission,new(){TableName=submission,EntityTypeName=typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_SUBMISSION).FullName!,PrimaryKeyColumn="SUBMISSION_ID",Module="PPDM_CORE",SubjectArea="Work Orders",CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID"],ForeignKeys=[new(){ForeignKeyColumn="START_ID",ReferencedTable=start,ReferencedPrimaryKey="START_ID"},new(){ForeignKeyColumn="PERMIT_ID",ReferencedTable=permit,ReferencedPrimaryKey="PERMIT_ID"},new(){ForeignKeyColumn="WORK_ORDER_ID",ReferencedTable="WORK_ORDER",ReferencedPrimaryKey="WORK_ORDER_ID"},new(){ForeignKeyColumn="FIELD_ID",ReferencedTable="FIELD",ReferencedPrimaryKey="FIELD_ID"},new(){ForeignKeyColumn="EMPLOYEE_BA_ID",ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"}]});
        metadata.Add(labor,new(){TableName=labor,EntityTypeName=typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_LABOR_USAGE).FullName!,PrimaryKeyColumn="LABOR_ID",Module="PPDM_CORE",SubjectArea="Work Orders",
            CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID"],
            ForeignKeys=[new(){ForeignKeyColumn="START_ID",ReferencedTable=start,ReferencedPrimaryKey="START_ID"},
                new(){ForeignKeyColumn="ASSIGNMENT_ID",ReferencedTable=assignment,ReferencedPrimaryKey="ASSIGNMENT_ID"},
                new(){ForeignKeyColumn="WORK_ORDER_ID",ReferencedTable="WORK_ORDER",ReferencedPrimaryKey="WORK_ORDER_ID"},
                new(){ForeignKeyColumn="FIELD_ID",ReferencedTable="FIELD",ReferencedPrimaryKey="FIELD_ID"},
                new(){ForeignKeyColumn="EMPLOYEE_BA_ID",ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"}]});
        return metadata;
    }
}
