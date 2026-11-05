using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public static class FacilityEvidenceRoute
{
    public static string For(FieldSelectionReview scope,FACILITY row)=>"/ppdm39/development/facilities?tenantId="+Uri.EscapeDataString(scope.Company.TenantId)+"&companyId="+Uri.EscapeDataString(scope.Company.BusinessAssociateId)+"&fieldId="+Uri.EscapeDataString(scope.FieldId!)+"&facilityId="+Uri.EscapeDataString(row.FACILITY_ID)+"&facilityType="+Uri.EscapeDataString(row.FACILITY_TYPE);
}

/// <summary>Recorded facility rows under the current company and reviewed FIELD.</summary>
public sealed class DevelopmentFacilityEvidenceState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IFieldSelectionContext fields;
    private readonly IDevelopmentServiceClient development;
    private readonly IDevelopmentDraftIntentStore? journal;
    private readonly Func<Exception,string,string,string> explain;
    private FACILITY[] rows=[];
    private long generation;
    private long installationRequest;
    private DevelopmentEquipmentInstallationReview? installationReview;
    private DevelopmentEquipmentInstallation? preparedInstallation,pendingInstallation;
    private bool installationRetained,installationAttempted;
    private DevelopmentEquipmentInstallationReceipt? installationReceipt;
    private long equipmentRequest;
    private DevelopmentFacilityEquipmentItem[] equipment=[];
    private long editRequest;
    private long creationRequest;
    private bool disposed;
    private DevelopmentFacilityEditReview? editReview;
    private DevelopmentFacilityEdit? preparedEdit,pendingEdit;
    private DevelopmentFacilityEditReceipt? editReceipt;
    private DevelopmentFacilityCreationDraft? creationDraft;
    private DevelopmentFacilityCreationReview? creationReview;
    private DevelopmentFacilityCreation? preparedCreation,pendingCreation;
    private DevelopmentFacilityCreationReceipt? creationReceipt;
    private bool creationRetained,creationAttempted;
    private bool writing,retentionConfirmed,submissionAttempted;
    public IReadOnlyList<FACILITY> Rows => Copy(rows);
    public IReadOnlyList<DevelopmentFacilityEquipmentItem> Equipment => equipment.ToArray();
    public DevelopmentEquipmentInstallationReview? InstallationReview => installationReview is null?null:Copy(installationReview);
    public DevelopmentEquipmentInstallation? PreparedInstallation => preparedInstallation is null?null:Copy(preparedInstallation);
    public DevelopmentEquipmentInstallation? PendingInstallation => pendingInstallation is null?null:Copy(pendingInstallation);
    public DevelopmentEquipmentInstallationReceipt? InstallationReceipt => installationReceipt is null?null:Copy(installationReceipt);
    public bool InstallationRetained=>installationRetained;
    public string? InstallationConfirmation { get; private set; }
    public bool CanRecoverInstallation=>!disposed && !writing && (installationRetained || installationAttempted) && pendingInstallation is not null && company.Selection is {} selected && selected.Scope==pendingInstallation.Review.Proposal.Context.Company && selected.UserId==pendingInstallation.Review.Proposal.Context.ActorUserId && selected.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal);
    public bool CanRetryInstallation=>CanRecoverInstallation && company.Selection is {} selected && selected.Roles.Contains(pendingInstallation!.Review.Proposal.EditorRole,StringComparer.Ordinal) && selected.Permissions.Contains("DevelopmentPlanning.Edit",StringComparer.Ordinal);
    public bool CanRetryInstallationRetention=>!disposed && !writing && pendingInstallation is not null && !installationRetained && !installationAttempted && company.Selection is {} selected && selected.Scope==pendingInstallation.Review.Proposal.Context.Company && selected.UserId==pendingInstallation.Review.Proposal.Context.ActorUserId && selected.Roles.Contains(pendingInstallation.Review.Proposal.EditorRole,StringComparer.Ordinal) && selected.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal) && selected.Permissions.Contains("DevelopmentPlanning.Edit",StringComparer.Ordinal);

    public bool InstallationReviewing { get; private set; }
    public string? InstallationError { get; private set; }
    public bool EquipmentLoading { get; private set; }
    public bool EquipmentLoaded { get; private set; }
    public string? EquipmentError { get; private set; }

    public FieldSelectionReview? Review { get; private set; }
    public string? FieldName { get; private set; }
    public DateTimeOffset? LoadedAtUtc { get; private set; }
    public bool Loading { get; private set; }
    public bool Loaded { get; private set; }
    public bool NeedsCompany => company.Selection is null;
    public bool NeedsField { get; private set; }
    public string? Error { get; private set; }
    public bool ReviewingEdit { get; private set; }
    public string? EditError { get; private set; }
    public DevelopmentFacilityEditReview? EditReview => editReview is null?null:Copy(editReview);
    public DevelopmentFacilityEdit? PreparedEdit => preparedEdit is null?null:Copy(preparedEdit);
    public DevelopmentFacilityEdit? PendingEdit => pendingEdit is null?null:Copy(pendingEdit);
    public DevelopmentFacilityEditReceipt? EditReceipt => editReceipt is null?null:Copy(editReceipt);
    public bool WritingEdit => writing;
    public bool RetentionConfirmed => retentionConfirmed;
    public bool CanRetryRetention=>pendingEdit is not null && !retentionConfirmed && !submissionAttempted && !writing && company.Selection is {} selected &&
        selected.Scope==pendingEdit.Review.Context.Company && selected.UserId==pendingEdit.Review.Context.ActorUserId && selected.Roles.Contains(pendingEdit.Review.EditorRole,StringComparer.Ordinal) &&
        selected.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal) && selected.Permissions.Contains("DevelopmentPlanning.Edit",StringComparer.Ordinal);
    public string? EditConfirmation { get; private set; }
    public DevelopmentFacilityCreationDraft? CreationDraft=>creationDraft is null?null:Copy(creationDraft);
    public DevelopmentFacilityCreationReview? CreationReview=>creationReview is null?null:Copy(creationReview);
    public bool ReviewingCreation { get; private set; }
    public DevelopmentFacilityCreation? PreparedCreation=>preparedCreation is null?null:Copy(preparedCreation);
    public DevelopmentFacilityCreation? PendingCreation=>pendingCreation is null?null:Copy(pendingCreation);
    public DevelopmentFacilityCreationReceipt? CreationReceipt=>creationReceipt is null?null:Copy(creationReceipt);
    public bool CreationRetained=>creationRetained;
    public string? CreationConfirmation { get; private set; }
    public string? CreationError { get; private set; }
    public event Action? Changed;

    public DevelopmentFacilityEvidenceState(CompanyClientContext company,IFieldSelectionContext fields,IDevelopmentServiceClient development,Func<Exception,string,string,string> explain,IDevelopmentDraftIntentStore? journal=null)
    {
        this.company=company;this.fields=fields;this.development=development;this.explain=explain;this.journal=journal;
        company.Changed+=Invalidate;fields.CurrentFieldChanged+=FieldChanged;
    }
    private static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    private void FieldChanged(string _) => Invalidate();
    private void Invalidate()
    {
        ++generation;++editRequest;++creationRequest;++equipmentRequest;++installationRequest;pendingInstallation=null;installationReceipt=null;installationRetained=false;installationAttempted=false;InstallationConfirmation=null;installationReview=null;preparedInstallation=null;InstallationReviewing=false;InstallationError=null;equipment=[];EquipmentLoading=false;EquipmentLoaded=false;EquipmentError=null;rows=[];Review=null;FieldName=null;LoadedAtUtc=null;Loading=false;Loaded=false;NeedsField=false;Error=null;editReview=null;preparedEdit=null;pendingEdit=null;editReceipt=null;creationDraft=null;creationReview=null;preparedCreation=null;pendingCreation=null;creationReceipt=null;creationRetained=false;creationAttempted=false;CreationConfirmation=null;ReviewingCreation=false;CreationError=null;EditError=null;EditConfirmation=null;ReviewingEdit=false;retentionConfirmed=false;submissionAttempted=false;
        if(!disposed)Changed?.Invoke();
    }
    public void ClearEquipment(){++installationRequest;installationReview=null;preparedInstallation=null;InstallationReviewing=false;InstallationError=null;++equipmentRequest;equipment=[];EquipmentLoading=false;EquipmentLoaded=false;EquipmentError=null;if(!disposed)Changed?.Invoke();}
    public async Task LoadEquipmentAsync(string id,string type)
    {
        if(disposed || !Loaded || Loading || Review is null || !rows.Any(row=>row.FACILITY_ID==id && row.FACILITY_TYPE==type))return;
        var sequence=++equipmentRequest;var version=generation;var snapshot=company.Snapshot;var field=Review.FieldId!;
        bool Current()=>!disposed && sequence==equipmentRequest && generation==version && snapshot==company.Snapshot;
        equipment=[];EquipmentLoading=true;EquipmentLoaded=false;EquipmentError=null;Changed?.Invoke();
        try
        {
            var result=await development.GetFacilityEquipmentAsync(id,type,field);
            if(result.Any(item=>item is null || !LeaseSummaryValidator.Identifier(item.EquipmentId) || item.InstallationObservation<0 || decimal.Truncate(item.InstallationObservation)!=item.InstallationObservation) || result.Select(item=>(item.EquipmentId,item.InstallationObservation)).Distinct().Count()!=result.Count)throw new InvalidOperationException("Facility equipment evidence is incomplete or ambiguous.");
            if(Current()){equipment=result.ToArray();EquipmentLoaded=true;}
        }
        catch(Exception error){var message=explain(error,"reading facility equipment","Facility equipment evidence could not be confirmed");if(Current())EquipmentError=message;}
        finally{if(Current()){EquipmentLoading=false;Changed?.Invoke();}}
    }
    public void ClearInstallation(){++installationRequest;installationReview=null;preparedInstallation=null;InstallationReviewing=false;InstallationError=null;if(!disposed)Changed?.Invoke();}
    public void ClearPreparedInstallation(){preparedInstallation=null;InstallationError=null;if(!disposed)Changed?.Invoke();}
    public void PrepareInstallation(string? utcText,bool confirmed)
    {
        if(disposed || writing || pendingInstallation is not null || pendingEdit is not null || pendingCreation is not null || installationReview is null || InstallationReviewing)return;preparedInstallation=null;InstallationError=null;
        try
        {
            if(string.IsNullOrWhiteSpace(utcText) || !utcText.EndsWith("Z",StringComparison.Ordinal) || !DateTimeOffset.TryParse(utcText,System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var instant) || instant.Offset!=TimeSpan.Zero)throw new InvalidOperationException("Enter the installation time in UTC ending with Z.");
            var original=new DevelopmentEquipmentInstallation(Copy(installationReview),Guid.NewGuid(),Guid.NewGuid(),instant.UtcDateTime,confirmed);original.Require();preparedInstallation=original;
        }
        catch(Exception error){InstallationError=explain(error,"preparing installation","Enter a valid UTC installation time and confirm the original");}
        Changed?.Invoke();
    }
    public async Task ReviewInstallationAsync(string facilityId,string facilityType,string equipmentId,string role,string reason)
    {
        if(disposed || writing || pendingInstallation is not null || pendingEdit is not null || pendingCreation is not null || !Loaded || Loading || InstallationReviewing || Review is null || company.Selection is not {} selected || !selected.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal) || !selected.Permissions.Contains("DevelopmentPlanning.Edit",StringComparer.Ordinal) || !selected.Roles.Contains(role,StringComparer.Ordinal) || !rows.Any(row=>row.FACILITY_ID==facilityId && row.FACILITY_TYPE==facilityType))return;
        var sequence=++installationRequest;var version=generation;var snapshot=company.Snapshot;
        bool Current()=>!disposed && sequence==installationRequest && version==generation && snapshot==company.Snapshot;
        installationReview=null;preparedInstallation=null;InstallationError=null;InstallationReviewing=true;Changed?.Invoke();
        try
        {
            var proposal=new DevelopmentEquipmentInstallationProposal(Copy(Review),facilityId,facilityType,equipmentId?.Trim()??"",role,reason?.Trim()??"");proposal.Require();
            var result=await development.ReviewEquipmentInstallationAsync(proposal);result.Require();if(result.Proposal!=proposal)throw new InvalidOperationException("The source review differs from the proposal.");if(Current())installationReview=Copy(result);
        }
        catch(Exception error){var message=explain(error,"reviewing equipment installation","Equipment installation evidence could not be confirmed");if(Current())InstallationError=message;}
        finally{if(Current()){InstallationReviewing=false;Changed?.Invoke();}}
    }
    public Task SubmitInstallationAsync()=>RecordInstallationAsync(false,false,false);
    public Task RetryInstallationRetentionAsync()=>RecordInstallationAsync(false,true,false);
    public Task RetryInstallationAsync()=>RecordInstallationAsync(false,false,true);
    public Task RecoverInstallationAsync()=>RecordInstallationAsync(true,false,false);
    private async Task RecordInstallationAsync(bool recover,bool retryRetention,bool retry)
    {
        if(disposed || journal is null || (recover?!CanRecoverInstallation:retryRetention?!CanRetryInstallationRetention:retry?!CanRetryInstallation:preparedInstallation is null || writing || pendingInstallation is not null || pendingEdit is not null || pendingCreation is not null || company.Selection is not {} current || !current.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal) || !current.Permissions.Contains("DevelopmentPlanning.Edit",StringComparer.Ordinal) || !current.Roles.Contains(preparedInstallation.Review.Proposal.EditorRole,StringComparer.Ordinal)))return;
        var original=Copy(recover || retryRetention || retry?pendingInstallation:preparedInstallation)!;var selected=company.Snapshot;var version=generation;var sequence=++installationRequest;
        bool Current()=>!disposed && version==generation && sequence==installationRequest && selected==company.Snapshot;
        writing=true;InstallationError=null;InstallationConfirmation=null;if(!recover && !retryRetention && !retry){pendingInstallation=original;preparedInstallation=null;installationReceipt=null;installationRetained=false;installationAttempted=false;}Changed?.Invoke();
        try
        {
            if(!recover){await journal.RetainEquipmentInstallationAsync(original);if(!Current())return;installationRetained=true;installationAttempted=true;Changed?.Invoke();if(!Current())return;}
            var result=recover?await development.RecoverEquipmentInstallationAsync(original):await development.SaveEquipmentInstallationAsync(original);result.Require(original);
            if(!Current())return;installationReceipt=Copy(result);installationReview=null;equipment=[];EquipmentLoaded=false;Changed?.Invoke();
            if(!Current())return;await journal.ClearEquipmentInstallationAsync(original);if(!Current())return;pendingInstallation=null;installationRetained=false;installationAttempted=false;InstallationConfirmation="The original equipment installation was confirmed. Reload the equipment evidence to inspect the recorded row.";
        }
        catch(Exception error){var message=explain(error,recover?"recovering the original installation":"saving the original installation","The outcome is unconfirmed. Preserve and recover the retained original before another installation");if(Current())InstallationError=message;}
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
    public void ClearEdit(){if(writing || pendingEdit is not null)return;++editRequest;editReview=null;preparedEdit=null;EditError=null;ReviewingEdit=false;if(!disposed)Changed?.Invoke();}
    public bool CanReviewEdit(string? id,string? type,string? role)=>!disposed && Loaded && !Loading && !ReviewingEdit && !writing && pendingEdit is null && Review is not null &&
        pendingCreation is null && pendingInstallation is null && company.Selection is {} selected && selected.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal) && selected.Permissions.Contains("DevelopmentPlanning.Edit",StringComparer.Ordinal) &&
        selected.Roles.Contains(role??"",StringComparer.Ordinal) && rows.Any(row=>row.FACILITY_ID==id && row.FACILITY_TYPE==type);
    public async Task ReviewEditAsync(string id,string type,string role)
    {
        if(!CanReviewEdit(id,type,role))return;
        var sequence=++editRequest;var version=generation;var captured=company.Snapshot;var context=Review!;
        var source=Copy(rows.Single(row=>row.FACILITY_ID==id && row.FACILITY_TYPE==type));
        bool Current()=>!disposed && sequence==editRequest && version==generation && captured==company.Snapshot;
        editReview=null;EditError=null;ReviewingEdit=true;Changed?.Invoke();
        try
        {
            var result=await development.ReviewFacilityEditAsync(id,type,role,context.FieldId!);if(!Current())return;
            result.Require();
            if(result.Context!=context || result.EditorRole!=role || result.SourceRevision!=DevelopmentFacilityEditReview.SourceDigest(source))
                throw new InvalidOperationException("The selected facility or company/FIELD review changed. Reload the source before editing.");
            editReview=Copy(result);
        }
        catch(Exception error){var message=explain(error,"reviewing facility edit","The current facility edit review could not be confirmed");if(Current())EditError=message;}
        finally{if(Current()){ReviewingEdit=false;Changed?.Invoke();}}
    }
    public bool CanPrepareEdit=>!disposed && !writing && !Loading && !ReviewingEdit && pendingEdit is null && pendingCreation is null && pendingInstallation is null && editReview is not null && company.Selection is {} selected &&
        selected.Scope==editReview.Context.Company && selected.UserId==editReview.Context.ActorUserId && selected.Roles.Contains(editReview.EditorRole,StringComparer.Ordinal) &&
        selected.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal) && selected.Permissions.Contains("DevelopmentPlanning.Edit",StringComparer.Ordinal);
    public bool CanPrepareCreation(string? role)=>!disposed && Loaded && !Loading && !writing && pendingEdit is null && pendingCreation is null && pendingInstallation is null && Review is {} review && company.Selection is {} selected &&
        selected.Scope==review.Company && selected.UserId==review.ActorUserId && selected.Roles.Contains(role??"",StringComparer.Ordinal) &&
        selected.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal) && selected.Permissions.Contains("DevelopmentPlanning.Create",StringComparer.Ordinal);
    public void PrepareCreation(string? role,string? id,string? type,string? operatorId,string? name,string? description,string? reason)
    {
        if(!CanPrepareCreation(role) || ReviewingCreation)return;++creationRequest;creationDraft=null;creationReview=null;CreationError=null;
        try
        {
            var draft=new DevelopmentFacilityCreationDraft(Copy(Review)!,role!,id?.Trim()??"",type?.Trim()??"",operatorId?.Trim()??"",name?.Trim()??"",string.IsNullOrWhiteSpace(description)?null:description.Trim(),reason?.Trim()??"");draft.Require();
            if(rows.Any(row=>row.FACILITY_ID==draft.FacilityId && row.FACILITY_TYPE==draft.FacilityType))throw new InvalidOperationException("This facility identity is already present in the displayed FIELD evidence.");
            creationDraft=draft;
        }
        catch(Exception error){CreationError=explain(error,"preparing a facility proposal","Enter a facility identity, name, current role and reason");}
        Changed?.Invoke();
    }
    public void ClearCreation(){if(writing || pendingCreation is not null)return;++creationRequest;creationDraft=null;creationReview=null;preparedCreation=null;ReviewingCreation=false;CreationError=null;if(!disposed)Changed?.Invoke();}
    public async Task ReviewCreationAsync()
    {
        if(creationDraft is null || ReviewingCreation || !CanPrepareCreation(creationDraft.CreatorRole))return;
        var draft=Copy(creationDraft);var sequence=++creationRequest;var version=generation;var selected=company.Snapshot;
        bool Current()=>!disposed && sequence==creationRequest && version==generation && selected==company.Snapshot;
        ReviewingCreation=true;creationReview=null;preparedCreation=null;CreationError=null;Changed?.Invoke();
        try
        {
            var result=await development.ReviewFacilityCreationAsync(draft);result.Require();
            if(JsonSerializer.Serialize(result.Draft)!=JsonSerializer.Serialize(draft))throw new InvalidOperationException("The source review differs from the original proposal.");
            if(Current())creationReview=Copy(result);
        }
        catch(Exception error){var message=explain(error,"reviewing the facility proposal","The facility source review could not be confirmed");if(Current())CreationError=message;}
        finally{if(Current()){ReviewingCreation=false;Changed?.Invoke();}}
    }
    public bool CanRecoverCreation=>!disposed && !writing && (creationRetained || creationAttempted) && pendingCreation is not null && company.Selection is {} selected && selected.Scope==pendingCreation.Review.Draft.Context.Company && selected.UserId==pendingCreation.Review.Draft.Context.ActorUserId && selected.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal);
    public bool CanRetryCreation=>CanRecoverCreation && company.Selection is {} selected && selected.Roles.Contains(pendingCreation!.Review.Draft.CreatorRole,StringComparer.Ordinal) && selected.Permissions.Contains("DevelopmentPlanning.Create",StringComparer.Ordinal);
    public bool CanRetryCreationRetention=>!disposed && !writing && pendingCreation is not null && !creationRetained && !creationAttempted && company.Selection is {} selected && selected.Scope==pendingCreation.Review.Draft.Context.Company && selected.UserId==pendingCreation.Review.Draft.Context.ActorUserId && selected.Roles.Contains(pendingCreation.Review.Draft.CreatorRole,StringComparer.Ordinal) && selected.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal) && selected.Permissions.Contains("DevelopmentPlanning.Create",StringComparer.Ordinal);
    public void PrepareCreationCommand(bool confirmed)
    {
        if(creationReview is null || !CanPrepareCreation(creationReview.Draft.CreatorRole))return;preparedCreation=null;CreationError=null;
        try{var original=new DevelopmentFacilityCreation(Copy(creationReview),Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),confirmed);original.Require();preparedCreation=original;}
        catch(Exception error){CreationError=explain(error,"confirming the facility proposal","Confirm the original facility creation");}Changed?.Invoke();
    }
    public Task SubmitCreationAsync()=>RecordCreationAsync(false,false,false);
    public Task RetryCreationRetentionAsync()=>RecordCreationAsync(false,true,false);
    public Task RetryCreationAsync()=>RecordCreationAsync(false,false,true);
    public Task RecoverCreationAsync()=>RecordCreationAsync(true,false,false);
    private async Task RecordCreationAsync(bool recover,bool retryRetention,bool retry)
    {
        if(disposed || journal is null || (recover?!CanRecoverCreation:retryRetention?!CanRetryCreationRetention:retry?!CanRetryCreation:preparedCreation is null || !CanPrepareCreation(preparedCreation.Review.Draft.CreatorRole)))return;
        var original=Copy(recover || retryRetention || retry?pendingCreation:preparedCreation)!;var selected=company.Snapshot;var version=generation;var sequence=++creationRequest;
        bool Current()=>!disposed && version==generation && sequence==creationRequest && selected==company.Snapshot;
        writing=true;CreationError=null;CreationConfirmation=null;if(!recover && !retryRetention && !retry){pendingCreation=original;preparedCreation=null;creationReceipt=null;creationRetained=false;creationAttempted=false;}Changed?.Invoke();
        try
        {
            if(!recover){await journal.RetainFacilityCreationAsync(original);if(!Current())return;creationRetained=true;creationAttempted=true;Changed?.Invoke();if(!Current())return;}
            var result=recover?await development.RecoverFacilityCreationAsync(original):await development.SaveFacilityCreationAsync(original);result.Require(original);
            if(!Current())return;creationReceipt=Copy(result);creationDraft=null;creationReview=null;rows=[];Review=null;Loaded=false;FieldName=null;LoadedAtUtc=null;Changed?.Invoke();
            if(!Current())return;await journal.ClearFacilityCreationAsync(original);if(!Current())return;pendingCreation=null;creationRetained=false;creationAttempted=false;CreationConfirmation="The original facility creation was confirmed. Reload to inspect current facility evidence.";
        }
        catch(Exception error){var message=explain(error,recover?"recovering the original facility creation":"saving the original facility creation","The original outcome is unconfirmed. Preserve and recover the retained input before preparing another facility");if(Current())CreationError=message;}
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
    public bool CanRecoverEdit=>!disposed && !writing && (retentionConfirmed || submissionAttempted) && pendingEdit is not null && company.Selection is {} selected &&
        selected.Scope==pendingEdit.Review.Context.Company && selected.UserId==pendingEdit.Review.Context.ActorUserId && selected.Permissions.Contains("DevelopmentPlanning.View",StringComparer.Ordinal);
    public bool CanRetryEdit=>CanRecoverEdit && company.Selection is {} selected && selected.Roles.Contains(pendingEdit!.Review.EditorRole,StringComparer.Ordinal) &&
        selected.Permissions.Contains("DevelopmentPlanning.Edit",StringComparer.Ordinal);
    public void PrepareEdit(string? name,string? description,string? reason,bool confirmed)
    {
        if(!CanPrepareEdit)return;preparedEdit=null;EditError=null;
        try{var original=new DevelopmentFacilityEdit(Copy(editReview)!,Guid.NewGuid(),name?.Trim()??"",string.IsNullOrWhiteSpace(description)?null:description.Trim(),reason?.Trim()??"",confirmed);original.Require();preparedEdit=original;}
        catch(Exception error){EditError=explain(error,"preparing a facility edit","Enter a changed name or description, a reason and explicit confirmation");}
        Changed?.Invoke();
    }
    public void CancelPreparedEdit(){if(writing || pendingEdit is not null)return;preparedEdit=null;Changed?.Invoke();}
    public Task SubmitEditAsync()=>RecordEditAsync(false,false,false);
    public Task RetryRetentionAsync()=>RecordEditAsync(false,true,false);
    public Task RetryEditAsync()=>RecordEditAsync(false,false,true);
    public Task RecoverEditAsync()=>RecordEditAsync(true,false,false);
    private async Task RecordEditAsync(bool recover,bool retryRetention,bool retryEdit)
    {
        if(disposed || journal is null || (recover?!CanRecoverEdit:retryRetention?!CanRetryRetention:retryEdit?!CanRetryEdit:!CanPrepareEdit || preparedEdit is null))return;
        var original=Copy(recover || retryRetention || retryEdit?pendingEdit:preparedEdit)!;var selected=company.Snapshot;var version=generation;var sequence=++editRequest;
        bool Current()=>!disposed && version==generation && sequence==editRequest && selected==company.Snapshot;
        writing=true;EditError=null;EditConfirmation=null;if(!recover && !retryRetention && !retryEdit){pendingEdit=original;preparedEdit=null;editReceipt=null;retentionConfirmed=false;submissionAttempted=false;}Changed?.Invoke();
        try
        {
            if(!recover){await journal.RetainFacilityEditAsync(original);if(!Current())return;retentionConfirmed=true;submissionAttempted=true;Changed?.Invoke();if(!Current())return;}
            var result=recover?await development.RecoverFacilityEditAsync(original):await development.SaveFacilityEditAsync(original);result.Require(original);
            if(!Current())return;editReceipt=Copy(result);editReview=null;rows=[];Review=null;Loaded=false;FieldName=null;LoadedAtUtc=null;Changed?.Invoke();
            if(!Current())return;await journal.ClearFacilityEditAsync(original);if(!Current())return;pendingEdit=null;retentionConfirmed=false;submissionAttempted=false;EditConfirmation="The original facility edit was confirmed. Reload to inspect current facility evidence.";
        }
        catch(Exception error){var message=explain(error,recover?"recovering the original facility edit":"saving the original facility edit","The original outcome is unconfirmed. Preserve and recover the retained input before trying another edit");if(Current())EditError=message;}
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task LoadAsync()
    {
        if(disposed)return;
        Invalidate();var version=generation;var captured=company.Snapshot;
        if(captured.Selection is null)return;
        bool Current()=>!disposed && version==generation && captured==company.Snapshot;
        Loading=true;Changed?.Invoke();
        try
        {
            if(journal is not null)
            {
                var retainedInstallation=await journal.ReadEquipmentInstallationAsync(captured.Selection.Scope,captured.Selection.UserId);if(!Current())return;
                if(retainedInstallation is not null){retainedInstallation.Require();pendingInstallation=Copy(retainedInstallation);installationRetained=true;installationAttempted=true;return;}
                var retainedCreation=await journal.ReadFacilityCreationAsync(captured.Selection.Scope,captured.Selection.UserId);if(!Current())return;
                if(retainedCreation is not null){retainedCreation.Require();pendingCreation=Copy(retainedCreation);creationRetained=true;creationAttempted=true;return;}
                var retained=await journal.ReadFacilityEditAsync(captured.Selection.Scope,captured.Selection.UserId);if(!Current())return;
                if(retained is not null){retained.Require();pendingEdit=Copy(retained);retentionConfirmed=true;submissionAttempted=true;return;}
            }
            var before=await fields.GetCurrentFieldSelectionAsync();if(!Current())return;
            FieldSelectionReviewValidator.RequireCurrent(before,captured.Selection.Scope,captured.Selection.UserId);
            if(!before.SelectionAvailable){NeedsField=true;return;}
            var review=before.SelectionContext!;
            var result=await development.GetFacilitiesAsync(fieldId:review.FieldId);if(!Current())return;
            if(result is null)throw new InvalidOperationException("Development facility evidence was unavailable.");
            var snapshot=Copy(result.ToArray());
            if(snapshot.Any(row=>row is null || string.IsNullOrWhiteSpace(row.FACILITY_ID) || string.IsNullOrWhiteSpace(row.FACILITY_TYPE) || row.PRIMARY_FIELD_ID!=review.FieldId) ||
                snapshot.Select(row=>(row.FACILITY_ID,row.FACILITY_TYPE)).Distinct().Count()!=snapshot.Length)
                throw new InvalidOperationException("The facility identities or FIELD evidence could not be confirmed.");
            var after=await fields.GetCurrentFieldSelectionAsync();if(!Current())return;
            FieldSelectionReviewValidator.RequireCurrent(after,captured.Selection.Scope,captured.Selection.UserId);
            if(!after.SelectionAvailable || after.SelectionContext!=review || after.FieldId!=before.FieldId)
                throw new InvalidOperationException("The company FIELD selection changed. Reload facility evidence.");
            rows=snapshot;Review=review;FieldName=before.FieldName;LoadedAtUtc=DateTimeOffset.UtcNow;Loaded=true;
        }
        catch(Exception error)
        {
            var message=explain(error,"loading development facility evidence","Development facility evidence could not be confirmed");
            if(Current())Error=message;
        }
        finally{if(Current()){Loading=false;Changed?.Invoke();}}
    }
    public void Dispose()
    {
        if(disposed)return;disposed=true;company.Changed-=Invalidate;fields.CurrentFieldChanged-=FieldChanged;Invalidate();
    }
}
