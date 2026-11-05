using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.ApiService.Controllers.Field;

[ApiController,Authorize,Route("api/field/current/development/facilities")]
[RequireCurrentFieldAccess]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class DevelopmentFacilityEvidenceController(DevelopmentFacilityReadAuthority authority,IFieldOrchestrator field,IFieldDevelopmentService development):ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FACILITY>>> GetAsync()
    {
        var original=await authority.CaptureAsync(User);
        var fieldId=field.CurrentFieldId;
        if(string.IsNullOrWhiteSpace(fieldId) || fieldId!=original.Context.FieldId)
            throw new InvalidOperationException("The facility FIELD differs from the reviewed current selection.");
        var rows=await development.GetFacilitiesForFieldAsync(fieldId)
            ?? throw new InvalidOperationException("Development facility evidence was unavailable.");
        if(rows.Any(row=>row is null || string.IsNullOrWhiteSpace(row.FACILITY_ID) || string.IsNullOrWhiteSpace(row.FACILITY_TYPE) || row.PRIMARY_FIELD_ID!=fieldId) ||
            rows.Select(row=>(row.FACILITY_ID,row.FACILITY_TYPE)).Distinct().Count()!=rows.Count)
            throw new InvalidOperationException("The recorded facility identities or FIELD could not be confirmed.");
        await original.ConfirmAsync();
        return Ok(rows);
    }

    [HttpGet("equipment")]
    public async Task<ActionResult<IReadOnlyList<DevelopmentFacilityEquipmentItem>>> EquipmentAsync([FromQuery]string facilityId,[FromQuery]string facilityType,[FromServices]CompanyDevelopmentFacilityEquipmentService equipment)
        => Ok(await equipment.ReadAsync(User,facilityId,facilityType));

    [HttpPost("equipment/installation-review")]
    public async Task<ActionResult<DevelopmentEquipmentInstallationReview>> ReviewEquipmentInstallationAsync([FromBody]DevelopmentEquipmentInstallationProposal proposal,[FromServices]CompanyEquipmentInstallationReviewService reviews)
        => Ok(await reviews.ReviewAsync(User,proposal));

    [HttpPost("equipment/installation")]
    public async Task<ActionResult<DevelopmentEquipmentInstallationReceipt>> InstallEquipmentAsync([FromBody]JsonElement input,[FromServices]CompanyEquipmentInstallationCommandService commands)=>Ok(await commands.ExecuteAsync(User,ParseInstallation(input)));
    [HttpPost("equipment/installation/recover")]
    public async Task<ActionResult<DevelopmentEquipmentInstallationReceipt>> RecoverEquipmentInstallationAsync([FromBody]JsonElement input,[FromServices]CompanyEquipmentInstallationCommandService commands)=>Ok(await commands.ExecuteAsync(User,ParseInstallation(input),true));
    private static DevelopmentEquipmentInstallation ParseInstallation(JsonElement input)
    {
        try{var original=input.Deserialize<DevelopmentEquipmentInstallation>(new JsonSerializerOptions(JsonSerializerDefaults.Web))??throw new InvalidOperationException();original.Require();return original;}
        catch(Exception error) when(error is JsonException or InvalidOperationException or ArgumentException){throw RefusalException.Invalid("The original equipment installation is invalid.");}
    }

    [HttpGet("edit-review")]
    public async Task<ActionResult<DevelopmentFacilityEditReview>> ReviewEditAsync([FromQuery]string facilityId,[FromQuery]string facilityType,[FromQuery]string editorRole)
    {
        var scope=await authority.CaptureEditAsync(User,facilityId,facilityType,editorRole);
        if(field.CurrentFieldId!=scope.Context.FieldId)throw new InvalidOperationException("The facility FIELD differs from the reviewed selection.");
        async Task<FACILITY> Read()
        {
            await scope.ConfirmAsync();
            var rows=await development.GetFacilitiesForFieldAsync(scope.Context.FieldId!) ?? throw new InvalidOperationException("Facility source evidence is unavailable.");
            if(rows.Any(row=>row is null || string.IsNullOrWhiteSpace(row.FACILITY_ID) || string.IsNullOrWhiteSpace(row.FACILITY_TYPE) || row.PRIMARY_FIELD_ID!=scope.Context.FieldId) ||
                rows.Select(row=>(row.FACILITY_ID,row.FACILITY_TYPE)).Distinct().Count()!=rows.Count)
                throw new InvalidOperationException("The facility source population is incomplete or ambiguous.");
            var matches=rows.Where(row=>row.FACILITY_ID==facilityId && row.FACILITY_TYPE==facilityType).ToArray();
            if(matches.Length!=1)throw new InvalidOperationException("The exact facility identity was not uniquely returned for the reviewed FIELD.");
            await scope.ConfirmAsync();return CompanyInventorySourceReview.Copy(matches[0]);
        }
        var original=await Read();var closing=await Read();
        if(DevelopmentFacilityEditReview.SourceDigest(original)!=DevelopmentFacilityEditReview.SourceDigest(closing))
            throw new InvalidOperationException("The original facility changed during edit review.");
        var review=new DevelopmentFacilityEditReview(scope.Context,original,editorRole,DevelopmentFacilityEditReview.SourceDigest(original),scope.AuthorityRevision,"",DateTimeOffset.UtcNow);
        review=review with{Revision=review.ContentRevision()};review.Require();await scope.ConfirmAsync();return Ok(review);
    }

    [HttpPost("edit")]
    public async Task<ActionResult<DevelopmentFacilityEditReceipt>> EditAsync([FromBody]JsonElement input,[FromServices]CompanyDevelopmentFacilityEditService commands)=>
        Ok(await commands.ExecuteAsync(User,ParseEdit(input)));

    [HttpPost("creation-review")]
    public async Task<ActionResult<DevelopmentFacilityCreationReview>> ReviewCreationAsync([FromBody]JsonElement input,[FromServices]CompanyDevelopmentFacilityCreationService commands)
    {
        DevelopmentFacilityCreationDraft draft;
        try{draft=input.Deserialize<DevelopmentFacilityCreationDraft>(new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new InvalidOperationException("The proposal is missing.");draft.Require();}
        catch(Exception error) when(error is JsonException or InvalidOperationException or ArgumentException){throw RefusalException.Invalid("The facility proposal is invalid.");}
        if(field.CurrentFieldId!=draft.Context.FieldId)throw RefusalException.Forbidden("The original facility FIELD changed.");
        return Ok(await commands.ReviewAsync(User,draft));
    }

    [HttpPost("creation")]
    public async Task<ActionResult<DevelopmentFacilityCreationReceipt>> CreateAsync([FromBody]JsonElement input,[FromServices]CompanyDevelopmentFacilityCreationService commands)=>
        Ok(await commands.ExecuteAsync(User,ParseCreation(input)));

    [HttpPost("creation/recover")]
    public async Task<ActionResult<DevelopmentFacilityCreationReceipt>> RecoverCreationAsync([FromBody]JsonElement input,[FromServices]CompanyDevelopmentFacilityCreationService commands)=>
        Ok(await commands.ExecuteAsync(User,ParseCreation(input),true));

    private static DevelopmentFacilityCreation ParseCreation(JsonElement input)
    {
        try{var original=input.Deserialize<DevelopmentFacilityCreation>(new JsonSerializerOptions(JsonSerializerDefaults.Web))??throw new InvalidOperationException("The original facility creation is missing.");original.Require();return original;}
        catch(Exception error) when(error is JsonException or InvalidOperationException or ArgumentException){throw RefusalException.Invalid("The original facility creation input is invalid.");}
    }

    [HttpPost("edit/recover")]
    public async Task<ActionResult<DevelopmentFacilityEditReceipt>> RecoverEditAsync([FromBody]JsonElement input,[FromServices]CompanyDevelopmentFacilityEditService commands)=>
        Ok(await commands.ExecuteAsync(User,ParseEdit(input),true));

    private static DevelopmentFacilityEdit ParseEdit(JsonElement input)
    {
        try
        {
            var original=input.Deserialize<DevelopmentFacilityEdit>(new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new InvalidOperationException("The original facility edit is missing.");
            original.Require();return original;
        }
        catch(Exception error) when(error is JsonException or InvalidOperationException or ArgumentException)
        {throw RefusalException.Invalid("The original facility edit input is invalid.");}
    }
}
