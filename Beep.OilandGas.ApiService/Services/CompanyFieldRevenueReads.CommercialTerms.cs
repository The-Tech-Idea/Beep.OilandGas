using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyFieldRevenueReads
{
    private async Task<(CompanyScope Company,string Actor,string Connection,Func<Task> Guard)> AmendmentDiscoveryAuthority(ClaimsPrincipal user,Func<Task> requireLifecycleAuthority)
    {
        var original=await context.CaptureAsync(user);var connection=await resolve();
        var roles=JsonSerializer.Serialize(original.Access.Roles?.Order(StringComparer.Ordinal));var grants=JsonSerializer.Serialize(original.Access.Permissions?.Order(StringComparer.Ordinal));
        async Task Guard()
        {
            await requireLifecycleAuthority();var current=await context.CaptureAsync(user);
            if(user.Identity?.IsAuthenticated!=true || current.Actor!=original.Actor || current.Access.Scope!=original.Access.Scope || current.Binding!=original.Binding || await resolve()!=connection ||
                current.Access.Roles?.Any(user.IsInRole)!=true || current.Access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true || !current.Access.Permissions.Contains("ProductionAccounting.Adjust",StringComparer.Ordinal) ||
                JsonSerializer.Serialize(current.Access.Roles.Order(StringComparer.Ordinal))!=roles || JsonSerializer.Serialize(current.Access.Permissions.Order(StringComparer.Ordinal))!=grants)throw new UnauthorizedAccessException("Original saved proposal discovery authority changed.");
        }
        await Guard();return(original.Access.Scope,original.Actor,connection,Guard);
    }
    public async Task<IReadOnlyList<COMMERCIAL_TERMS_INTENT>> ReadAmendmentIntentsAsync(ClaimsPrincipal user,Func<Task> requireLifecycleAuthority)
    {
        var authority=await AmendmentDiscoveryAuthority(user,requireLifecycleAuthority);
        return await production.GetCommercialAmendmentIntentsAsync(authority.Company,authority.Actor,authority.Connection,authority.Guard);
    }
    public async Task<(IReadOnlyList<COMMERCIAL_TERMS_INTENT> Entries,int Total)> ReadAmendmentIntentPageAsync(ClaimsPrincipal user,int page,int size,Func<Task> requireLifecycleAuthority)
    {
        CompanyCommercialAmendmentPage.RequireRequest(page,size);var authority=await AmendmentDiscoveryAuthority(user,requireLifecycleAuthority);
        return await production.GetCommercialAmendmentIntentPageAsync(authority.Company,authority.Actor,authority.Connection,page,size,authority.Guard);
    }
    public async Task<COMMERCIAL_TERMS_INTENT> SaveAmendmentIntentAsync(ClaimsPrincipal user,CompanyCommercialAmendmentSubmission command,Func<Task> requireLifecycleAuthority)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var review=command.Review;
        var authority=await CommercialAuthority(user,review.Company,review.ActorUserId,review.Source.Source.FieldId,"ProductionAccounting.Adjust");
        async Task Guard(){await authority.Guard();await requireLifecycleAuthority();await authority.Guard();}
        return await production.SaveCommercialAmendmentIntentAsync(command,authority.Connection,Guard,()=>CurrentContract(user,command.Amendment.Preparation.Source));
    }
    public async Task<COMMERCIAL_TERMS_INTENT?> ReadAmendmentIntentAsync(ClaimsPrincipal user,CompanyScope company,string actor,string process,string field,Func<Task> requireLifecycleAuthority)
    {
        var authority=await CommercialAuthority(user,company,actor,field,"ProductionAccounting.Adjust");
        async Task Guard(){await authority.Guard();await requireLifecycleAuthority();await authority.Guard();}
        return await production.GetCommercialAmendmentIntentAsync(company,actor,process,authority.Connection,Guard);
    }
    public async Task<(Beep.OilandGas.Models.Data.ProductionAccounting.SALES_CONTRACT Native,SalesContractTermsRecord Record)> ReadAmendmentPredecessorAsync(ClaimsPrincipal user,string contract)
    {
        var original=await context.CaptureAsync(user);
        if(user.Identity?.IsAuthenticated!=true || original.Access.Roles?.Any(user.IsInRole)!=true || original.Access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true ||
            !original.Access.Permissions.Contains("ProductionAccounting.Adjust",StringComparer.Ordinal))throw new UnauthorizedAccessException();
        var connection=await resolve();production.RequireCommercialTermsSchema(connection);
        var row=await production.GetRecordedSalesContractAsync(contract,original.Access.Scope,connection);
        var record=SalesContractTermsNativeProof.Require(original.Access.Scope,row).Record;
        var authority=await CommercialAuthority(user,original.Access.Scope,original.Actor,record.Preparation.Source.Source.FieldId,"ProductionAccounting.Adjust");
        if(authority.Connection!=connection)throw new UnauthorizedAccessException("The original commercial binding changed.");
        await authority.Guard();var closing=await production.GetRecordedSalesContractAsync(contract,original.Access.Scope,connection);await authority.Guard();
        var closingAuthority=await context.CaptureAsync(user);
        if(closingAuthority.Actor!=original.Actor || closingAuthority.Access.Scope!=original.Access.Scope || closingAuthority.Binding!=original.Binding ||
            JsonSerializer.Serialize(closingAuthority.Access.Roles.Order(StringComparer.Ordinal))!=JsonSerializer.Serialize(original.Access.Roles.Order(StringComparer.Ordinal)) ||
            JsonSerializer.Serialize(closingAuthority.Access.Permissions.Order(StringComparer.Ordinal))!=JsonSerializer.Serialize(original.Access.Permissions.Order(StringComparer.Ordinal)) || await resolve()!=connection)
            throw new UnauthorizedAccessException("The original amendment predecessor authority changed.");
        if(JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The amendment predecessor changed.");
        return(CompanyInventorySourceReview.Copy(row),CompanyInventorySourceReview.Copy(record));
    }
    public async Task<RevenueSalesContractReview> ReadAmendmentSourceAsync(ClaimsPrincipal user,SalesContractTermsRecord previous)
    {
        previous.Require();var source=await CurrentContract(user,previous.Preparation.Source);
        return SalesContractTermsAmendment.NormalizeSource(source,previous);
    }
    public async Task<SalesContractTermsRecord> AmendTermsAsync(ClaimsPrincipal user,SalesContractTermsAmendment amendment,Func<Task> requireLifecycleAuthority)
    {
        amendment=CompanyInventorySourceReview.Copy(amendment);amendment.Require();
        var source=amendment.Preparation.Source;
        var authority=await CommercialAuthority(user,amendment.Preparation.Company,amendment.Preparation.ActorUserId,source.Source.FieldId,"ProductionAccounting.Adjust");
        async Task Guard(){await authority.Guard();await requireLifecycleAuthority();await authority.Guard();}
        return await production.AmendSalesContractTermsAsync(amendment,authority.Connection,()=>CurrentContract(user,source),Guard);
    }
    /// <summary>Historical rows are frozen evidence; current authority and the containing native envelope remain mandatory.</summary>
    public async Task<(SalesContractTermsReadReview Terms,Beep.OilandGas.Models.Data.ProductionAccounting.SALES_CONTRACT? CurrentNativeContract)> ReadProcessTermsAsync(ClaimsPrincipal user,string contractId,string preparationRevision,bool capturedRejection=false)
    {
        if(!ProcessDefinitionRevision.IsValid(preparationRevision))throw new ArgumentException("The original commercial preparation revision is required.");
        async Task<(CompanyScope Company,string Actor,string Binding,string Roles,string Grants)> Capture(string? field=null)
        {
            var current=await context.CaptureAsync(user);var access=current.Access;
            if(user.Identity?.IsAuthenticated!=true || access.Roles?.Any(user.IsInRole)!=true || access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true ||
                field is not null && !(await assets.CheckAssetAccessAsync(current.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException();
            return(access.Scope,current.Actor,current.Binding,JsonSerializer.Serialize(access.Roles.Order(StringComparer.Ordinal)),JsonSerializer.Serialize(access.Permissions.Order(StringComparer.Ordinal)));
        }
        var authority=await Capture();var connection=await resolve();production.RequireCommercialTermsSchema(connection);
        var native=await production.GetRecordedSalesContractAsync(contractId,authority.Company,connection);
        var proof=SalesContractTermsNativeProof.Require(authority.Company,native);
        if(!capturedRejection && CompanyAccountingSourceTypes.CommercialPreparationRevision(proof.Record)==preparationRevision)
        {
            var current=await ReadTermsByContractAsync(user,contractId);
            if(current.Recorded is null || CompanyAccountingSourceTypes.CommercialPreparationRevision(current.Recorded)!=preparationRevision ||
                await Capture()!=authority || await resolve()!=connection || JsonSerializer.Serialize(current.Source.Contract)!=JsonSerializer.Serialize(native))
                throw new InvalidOperationException("The current commercial process version changed.");
            return(current,null);
        }
        var historical=SalesContractTermsNativeProof.HistoricalReview(authority.Company,authority.Actor,native,preparationRevision);
        var field=historical.Source.Source.FieldId;
        if(await Capture(field)!=authority || await resolve()!=connection)throw new UnauthorizedAccessException("Current historical source authority changed.");
        var closing=await production.GetRecordedSalesContractAsync(contractId,authority.Company,connection);
        _=SalesContractTermsNativeProof.Require(authority.Company,closing);
        if(await Capture(field)!=authority || await resolve()!=connection)throw new UnauthorizedAccessException("Current historical source authority changed.");
        if(JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(native))throw new InvalidOperationException("The preserved commercial envelope changed during review.");
        return(historical,CompanyInventorySourceReview.Copy(native));
    }
    public async Task<SalesContractTermsReadReview> ReadTermsByContractAsync(ClaimsPrincipal user,string contractId)
    {
        var original=await context.CaptureAsync(user);
        if(user.Identity?.IsAuthenticated!=true || original.Access.Roles?.Any(user.IsInRole)!=true || original.Access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true)throw new UnauthorizedAccessException();
        var connection=await resolve();production.RequireCommercialTermsSchema(connection);
        var native=await production.GetRecordedSalesContractAsync(contractId,original.Access.Scope,connection);
        var record=SalesContractTermsNativeProof.Require(original.Access.Scope,native).Record;var source=record.Preparation.Source;
        var review=await ReadTermsAsync(user,source.Source.FieldId,source.Source.From,source.Source.Through,source.TransactionId);
        var closing=await context.CaptureAsync(user);
        if(review.Source.Contract.SALES_CONTRACT_ID!=contractId || review.Recorded is null || closing.Actor!=original.Actor || closing.Access.Scope!=original.Access.Scope || closing.Binding!=original.Binding || await resolve()!=connection ||
            JsonSerializer.Serialize(native)!=JsonSerializer.Serialize(review.Source.Contract))throw new InvalidOperationException("The original commercial process source changed.");
        return review;
    }
    private async Task<RevenueSalesContractReview> CurrentContract(ClaimsPrincipal user,RevenueSalesContractReview original)
    {
        var source=await GetAsync(user,original.Source.FieldId,original.Source.From,original.Source.Through);
        return await GetContractAsync(user,source.FieldId,source.From,source.Through,original.TransactionId,source.Revision);
    }
    public async Task<SalesContractTermsReadReview> ReadTermsAsync(ClaimsPrincipal user,string field,DateTime from,DateTime through,string transaction)
    {
        var source=await GetAsync(user,field,from,through);
        production.RequireCommercialTermsSchema(await resolve());
        var contract=await GetContractAsync(user,field,from,through,transaction,source.Revision);
        var recorded=contract.Contract.TERMS_WORKFLOW_JSON is null?null:SalesContractTermsNativeProof.Require(source.Company,contract.Contract).Record;
        var review=new SalesContractTermsReadReview(contract,recorded);review.Require(source.Company,source.ActorUserId,field,from,through,transaction);return review;
    }
    private async Task<(string Connection,Func<Task> Guard)> CommercialAuthority(ClaimsPrincipal user,CompanyScope company,string actor,string field,string grant)
    {
        async Task<string> Capture()
        {
            var current=await context.CaptureAsync(user);var access=current.Access;
            if(user.Identity?.IsAuthenticated!=true || current.Actor!=actor || access.Scope!=company || access.Roles?.Any(user.IsInRole)!=true ||
                access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true || !access.Permissions.Contains(grant,StringComparer.Ordinal) ||
                !(await assets.CheckAssetAccessAsync(actor,field,"FIELD","WRITE")).HasAccess)
                throw new UnauthorizedAccessException("Current company commercial command authority is required.");
            return JsonSerializer.Serialize(new{access.Scope,current.Actor,current.Binding,Roles=access.Roles.Order(StringComparer.Ordinal),Grants=access.Permissions.Order(StringComparer.Ordinal)});
        }
        var authority=await Capture();var connection=await resolve();
        async Task Guard(){if(await Capture()!=authority || await resolve()!=connection)throw new UnauthorizedAccessException("Original commercial authority or binding changed.");}
        await Guard();return(connection,Guard);
    }
    public async Task<SalesContractTermsRecord> PrepareTermsAsync(ClaimsPrincipal user,string field,SalesContractTermsPreparation preparation)
    {
        preparation=CompanyInventorySourceReview.Copy(preparation);preparation.Require();
        if(preparation.Source.Source.FieldId!=field)throw new ArgumentException("Commercial route and original FIELD differ.");
        var authority=await CommercialAuthority(user,preparation.Company,preparation.ActorUserId,field,"ProductionAccounting.Adjust");
        return await production.PrepareSalesContractTermsAsync(preparation,authority.Connection,()=>CurrentContract(user,preparation.Source),authority.Guard);
    }
    public async Task<SalesContractTermsRecord> DecideTermsAsync(ClaimsPrincipal user,string field,SalesContractTermsDecisionSubmission submission)
    {
        submission=CompanyInventorySourceReview.Copy(submission);submission.Require();var command=submission.Command;var original=submission.Preparation.Preparation;
        if(original.Source.Source.FieldId!=field)throw new ArgumentException("Commercial route and original FIELD differ.");
        var authority=await CommercialAuthority(user,command.Company,command.ActorUserId,field,"ProductionAccounting.Approve");
        return await production.DecideSalesContractTermsAsync(submission.Preparation,command,authority.Connection,()=>CurrentContract(user,original.Source),authority.Guard);
    }
}
