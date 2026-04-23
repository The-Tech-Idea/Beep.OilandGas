using System.Security.Claims;
using Beep.OilandGas.UserManagement.Models.Profile;
using Microsoft.AspNetCore.Components.Authorization;

namespace Beep.OilandGas.Web.Services;

public interface IPersonaContextService
{
    UserPersonaProfile? CurrentProfile { get; }
    PersonaDefinition? CurrentPersona { get; }
    IReadOnlyList<string> AllowedWorkflows { get; }
    Task EnsureLoadedAsync();
    bool CanAccessWorkflow(string workflowKey);
    bool CanAccessRoute(string routePath);
}

public class PersonaContextService : IPersonaContextService
{
    private readonly IIdentityServiceClient _identityClient;
    private readonly INavigationPolicyService _navigationPolicy;
    private readonly AuthenticationStateProvider _authStateProvider;

    private bool _isLoaded;
    private UserPersonaProfile? _currentProfile;
    private PersonaDefinition? _currentPersona;
    private List<string> _allowedWorkflows = [];

    public PersonaContextService(
        IIdentityServiceClient identityClient,
        INavigationPolicyService navigationPolicy,
        AuthenticationStateProvider authStateProvider)
    {
        _identityClient = identityClient;
        _navigationPolicy = navigationPolicy;
        _authStateProvider = authStateProvider;
    }

    public UserPersonaProfile? CurrentProfile => _currentProfile;
    public PersonaDefinition? CurrentPersona => _currentPersona;
    public IReadOnlyList<string> AllowedWorkflows => _allowedWorkflows;

    public async Task EnsureLoadedAsync()
    {
        if (_isLoaded)
        {
            return;
        }

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _isLoaded = true;
            return;
        }

        _currentProfile = await _identityClient.GetProfileAsync(userId);

        if (_currentProfile == null)
        {
            _isLoaded = true;
            return;
        }

        var personas = await _identityClient.GetPersonaCatalogAsync();
        _currentPersona = personas.FirstOrDefault(p => p.PERSONA_CODE == _currentProfile.PRIMARY_PERSONA);
        _allowedWorkflows = _navigationPolicy.GetAllowedWorkflows(_currentPersona);

        _isLoaded = true;
    }

    public bool CanAccessWorkflow(string workflowKey)
    {
        if (!_isLoaded || _currentPersona == null)
        {
            return true;
        }

        return _navigationPolicy.IsWorkflowAllowed(_currentPersona, workflowKey);
    }

    public bool CanAccessRoute(string routePath)
    {
        if (!_isLoaded || _currentPersona == null)
        {
            return true;
        }

        return _navigationPolicy.CanAccessRoute(_currentPersona, routePath);
    }
}
