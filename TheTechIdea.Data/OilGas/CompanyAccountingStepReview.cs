namespace TheTechIdea.Data.OilGas;

/// <summary>Current owned workflow-step eligibility; this evidence does not perform financial approval.</summary>
public sealed record CompanyAccountingStepReview(CompanyScope Company,string ActorUserId,string InstanceId,string ProcessId,
    string EntityType,string EntityId,string StepInstanceId,string StepId,string StepName,string AssignedRole,string SourceMakerUserId,
    string WorkflowStarterUserId,bool RequiresIndependentActor,string RequiredPermission,bool Eligible,string Eligibility,
    string SourceRevision,string DefinitionRevision,string BindingRevision,string ProcessRevision,string Revision,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] string? AccountingEvidenceRevision=null,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] APInvoiceCorrectionStepEvidence? CorrectionEvidence=null,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] string? AccountingEvidenceAction=null)
{
    public void Require(CompanyScope company,string actor,string instance)
    {
        APInvoiceOwnership.RequireScope(company);
        if(EntityType=="JOURNAL_ENTRY")
        {
            var approval=AccountingEvidenceAction==CompanyAccountingSourceTypes.Approval("JOURNAL_ENTRY");
            var posting=AccountingEvidenceAction==CompanyAccountingSourceTypes.JournalPosting;
            var rejection=AccountingEvidenceAction==CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY");
            if(!approval && !posting && !rejection || RequiresIndependentActor!=approval || RequiredPermission!=(approval?"Workflow.Approve":"Workflow.Execute") ||
                rejection && Eligible && ActorUserId!=SourceMakerUserId)
                throw new InvalidOperationException("The journal task must identify its configured review, posting or original-maker rejection action.");
        }
        else if(AccountingEvidenceAction is not null)throw new InvalidOperationException("The recorded accounting action belongs to another source type.");
        if(CorrectionEvidence is not null){CorrectionEvidence.Require(company,actor,EntityId);if(EntityType!="AP_INVOICE" || AccountingEvidenceRevision!=CorrectionEvidence.Revision || (CorrectionEvidence.Action!=CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork)!=RequiresIndependentActor)throw new InvalidOperationException("The correction evidence differs from its configured step.");}
        var ids=new[]{ActorUserId,InstanceId,ProcessId,EntityId,StepInstanceId,StepId,AssignedRole,SourceMakerUserId,WorkflowStarterUserId};
        if(Company!=company || ActorUserId!=actor || InstanceId!=instance || ids.Any(id=>string.IsNullOrWhiteSpace(id) || id.Length>128 || id.Any(char.IsControl)) ||
            EntityType is not ("AP_INVOICE" or "PURCHASE_ORDER" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT") || string.IsNullOrWhiteSpace(StepName) || StepName.Length>512 ||
            RequiredPermission is not ("Workflow.Approve" or "Workflow.Execute") ||
            Eligibility is not ("ELIGIBLE" or "WORKFLOW_PERMISSION_REQUIRED" or "ROLE_OR_INDEPENDENCE_REQUIRED" or "RECORDED_DISPOSITION_REQUIRED" or "RECORDED_CONTROLLER_REQUIRED" or "REQUEST_CORRECTION_REQUIRED" or "ORIGINAL_PREPARER_REQUIRED" or "CORRECTION_DATA_REQUIRED" or "CORRECTION_ACKNOWLEDGEMENT_REQUIRED" or "CORRECTION_WORK_EVIDENCE_REQUIRED" or "COMMERCIAL_APPROVAL_REQUIRED" or "COMMERCIAL_REVIEWER_REQUIRED" or "COMMERCIAL_APPROVAL_PERMISSION_REQUIRED" or "COMMERCIAL_REJECTION_RECORDED" or "COMMERCIAL_RETURN_CONFIGURATION_REQUIRED" or "COMMERCIAL_PREPARER_REQUIRED" or "COMMERCIAL_PREPARATION_PERMISSION_REQUIRED" or "DELIVERY_APPROVAL_REQUIRED" or "DELIVERY_REVIEWER_REQUIRED" or "DELIVERY_APPROVAL_PERMISSION_REQUIRED" or "DELIVERY_REJECTION_RECORDED" or "DELIVERY_RETURN_CONFIGURATION_REQUIRED" or "DELIVERY_PREPARER_REQUIRED" or "DELIVERY_PREPARATION_PERMISSION_REQUIRED" or "SALES_INVOICE_APPROVAL_REQUIRED" or "SALES_INVOICE_REVIEWER_REQUIRED" or "SALES_INVOICE_DECISION_PERMISSION_REQUIRED" or "SALES_INVOICE_FIELD_WRITE_REQUIRED" or "SALES_INVOICE_REJECTION_RECORDED" or "SALES_INVOICE_RETURN_CONFIGURATION_REQUIRED" or "SALES_INVOICE_PREPARER_REQUIRED" or "SALES_INVOICE_PREPARATION_PERMISSION_REQUIRED" or "SALES_INVOICE_REJECTION_REQUIRED" or "SALES_INVOICE_SOURCE_CHANGED" or "SALES_RECEIVABLE_APPROVAL_REQUIRED" or "SALES_RECEIVABLE_REVIEWER_REQUIRED" or "SALES_RECEIVABLE_DECISION_PERMISSION_REQUIRED" or "SALES_RECEIVABLE_FIELD_WRITE_REQUIRED" or "SALES_RECEIVABLE_REJECTION_RECORDED" or "SALES_RECEIVABLE_RETURN_CONFIGURATION_REQUIRED" or "SALES_RECEIVABLE_PREPARER_REQUIRED" or "SALES_RECEIVABLE_PREPARATION_PERMISSION_REQUIRED" or "SALES_RECEIVABLE_REJECTION_REQUIRED" or "SALES_RECEIVABLE_SOURCE_CHANGED" or "SALES_PAYMENT_APPROVAL_REQUIRED" or "SALES_PAYMENT_REVIEWER_REQUIRED" or "SALES_PAYMENT_DECISION_PERMISSION_REQUIRED" or "SALES_PAYMENT_FIELD_WRITE_REQUIRED" or "SALES_PAYMENT_REJECTION_RECORDED" or "SALES_PAYMENT_RETURN_CONFIGURATION_REQUIRED" or "SALES_PAYMENT_PREPARER_REQUIRED" or "SALES_PAYMENT_PREPARATION_PERMISSION_REQUIRED" or "SALES_PAYMENT_REJECTION_REQUIRED" or "SALES_JOURNAL_APPROVAL_REQUIRED" or "SALES_JOURNAL_REVIEWER_REQUIRED" or "SALES_JOURNAL_DECISION_PERMISSION_REQUIRED" or "SALES_JOURNAL_FIELD_WRITE_REQUIRED" or "SALES_JOURNAL_REJECTION_RECORDED" or "SALES_JOURNAL_RETURN_CONFIGURATION_REQUIRED" or "SALES_JOURNAL_PREPARER_REQUIRED" or "SALES_JOURNAL_PREPARATION_PERMISSION_REQUIRED" or "SALES_JOURNAL_REJECTION_REQUIRED" or "SALES_JOURNAL_POSTING_REQUIRED" or "SALES_JOURNAL_POSTER_REQUIRED" or "SALES_JOURNAL_POSTING_PERMISSION_REQUIRED") || Eligible!=(Eligibility=="ELIGIBLE") ||
            CompanyAccountingSourceTypes.IsNativeDecision(EntityType) && AccountingEvidenceRevision is null ||
            AccountingEvidenceRevision is not null && !ProcessDefinitionRevision.IsValid(AccountingEvidenceRevision) ||
            RequiresIndependentActor && Eligible && (ActorUserId==SourceMakerUserId || ActorUserId==WorkflowStarterUserId) ||
            new[]{SourceRevision,DefinitionRevision,BindingRevision,ProcessRevision,Revision}.Any(value=>!ProcessDefinitionRevision.IsValid(value)))
            throw new InvalidOperationException("The company step review is incomplete or belongs to another company, actor or process.");
    }
}
