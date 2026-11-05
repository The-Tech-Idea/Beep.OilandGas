using System.Globalization;

namespace Beep.OilandGas.Web.Pages.Legal;

/// <summary>
/// The privacy policy and the terms of service: where they are served, and the facts both documents state.
/// </summary>
/// <remarks>
/// <para>
/// The addresses are a contract with the identity server: its sign-in page for this application links
/// <c>https://oilandgas.thetechidea.app/privacy</c> and <c>/terms</c>, so neither may move. Both pages are public — a
/// person reads them before deciding to create an account — so they carry no authorization requirement and the main
/// layout's repository-setup gate lets them through (<see cref="IsLegalPage"/>): neither reads the repository.
/// </para>
/// <para>
/// The operator and the contact address are the ones TheTechIdea's own published legal pages state (TheTechIdea.WebSite's
/// and Beep Events' privacy policies): The Tech Idea, at support@thetechidea.com. Neither names a registered company, a
/// registered address or a governing law, and these pages do not either: an invented one would be a false statement in a
/// legal document.
/// </para>
/// </remarks>
public static class LegalPages
{
    /// <summary>The privacy policy's address.</summary>
    public const string Privacy = "/privacy";

    /// <summary>The terms of service's address.</summary>
    public const string Terms = "/terms";

    /// <summary>Who provides Beep Oil &amp; Gas, as TheTechIdea's published legal pages name it.</summary>
    public const string Operator = "The Tech Idea";

    /// <summary>Where privacy requests and questions about the terms go: the support address TheTechIdea publishes.</summary>
    public const string ContactEmail = "support@thetechidea.com";

    /// <summary><see cref="ContactEmail"/> as a link.</summary>
    public const string ContactHref = "mailto:" + ContactEmail;

    /// <summary>
    /// The identity service people sign in to Beep Oil &amp; Gas through — TheTechIdea's, named as TheTechIdea.WebSite's
    /// privacy policy names it.
    /// </summary>
    public const string IdentityService = "https://identity.thetechidea.app/";

    /// <summary><see cref="IdentityService"/> as the documents print it.</summary>
    public const string IdentityServiceName = "identity.thetechidea.app";

    /// <summary>When the current wording of both documents took effect.</summary>
    public static readonly DateOnly EffectiveDate = new(2026, 10, 7);

    /// <summary>The effective date as both documents print it.</summary>
    public static string EffectiveDateText => EffectiveDate.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Whether <paramref name="path"/> (a request's path, without its query) is one of the two documents.</summary>
    public static bool IsLegalPage(string path)
    {
        var trimmed = path.TrimEnd('/');
        return string.Equals(trimmed, Privacy, StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmed, Terms, StringComparison.OrdinalIgnoreCase);
    }
}
