using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.UserManagement.Models.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

[ApiController]
[Route("api/identity/profile")]
[Authorize]
public class PersonaProfileController : ControllerBase
{
    private readonly IPersonaProfileService _profileService;
    private readonly ILogger<PersonaProfileController> _logger;

    public PersonaProfileController(
        IPersonaProfileService profileService,
        ILogger<PersonaProfileController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    private string ActorUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

    /// <summary>Get the persona profile for a user.</summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetProfile(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { error = "User ID is required." });
        try
        {
            var profile = await _profileService.GetProfileAsync(userId);
            if (profile is null)
                return NotFound(new { message = $"No profile found for user {userId}." });
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve profile for user {UserId}", userId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Create or update the persona profile for a user.</summary>
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpsertProfile(string userId, [FromBody] UserPersonaProfile profile)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { error = "User ID is required." });
        if (profile is null)
            return BadRequest(new { error = "Profile body is required." });

        profile.USER_ID = userId;
        try
        {
            var result = await _profileService.UpsertProfileAsync(profile, ActorUserId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert profile for user {UserId}", userId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Switch the active persona for a user.</summary>
    [HttpPost("{userId}/switch-persona")]
    public async Task<IActionResult> SwitchPersona(string userId, [FromBody] SwitchPersonaRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(request?.PersonaCode))
            return BadRequest(new { error = "User ID and PersonaCode are required." });
        try
        {
            var ok = await _profileService.SwitchPersonaAsync(userId, request.PersonaCode, ActorUserId);
            if (!ok)
                return NotFound(new { message = $"No profile found for user {userId}." });
            return Ok(new { message = "Persona switched.", personaCode = request.PersonaCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch persona for user {UserId}", userId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Get the seeded persona catalog.</summary>
    [HttpGet("catalog")]
    public async Task<IActionResult> GetPersonaCatalog()
    {
        try
        {
            var catalog = await _profileService.GetPersonaCatalogAsync();
            return Ok(catalog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve persona catalog");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Get per-view preferences for a user/persona combination.</summary>
    [HttpGet("{userId}/preferences/{personaCode}")]
    public async Task<IActionResult> GetViewPreferences(string userId, string personaCode)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(personaCode))
            return BadRequest(new { error = "User ID and persona code are required." });
        try
        {
            var prefs = await _profileService.GetViewPreferencesAsync(userId, personaCode);
            return Ok(prefs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve view preferences for user {UserId}", userId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Set a single per-view preference for a user.</summary>
    [HttpPut("{userId}/preferences")]
    public async Task<IActionResult> SetViewPreference(string userId, [FromBody] PersonaViewPreference preference)
    {
        if (string.IsNullOrWhiteSpace(userId) || preference is null)
            return BadRequest(new { error = "User ID and preference body are required." });

        preference.USER_ID = userId;
        try
        {
            await _profileService.SetViewPreferenceAsync(preference, ActorUserId);
            return Ok(new { message = "Preference saved." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set view preference for user {UserId}", userId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }
}

public sealed record SwitchPersonaRequest(string PersonaCode);
