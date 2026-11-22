namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>A UI language the application can switch to.</summary>
public sealed record OgCultureOption(string Name, string DisplayName, bool RightToLeft);

/// <summary>
/// Resolves localisation keys and owns the active UI culture. A key missing from the active culture
/// falls back to English, and an unknown key returns itself so it is visible and never blank.
/// </summary>
public interface ILocProvider
{
    string CultureName { get; }

    bool IsRightToLeft { get; }

    IReadOnlyList<OgCultureOption> Cultures { get; }

    /// <summary>Raised after the culture changes so the shell can re-apply direction and titles.</summary>
    event EventHandler? LanguageChanged;

    void SetCulture(string cultureName);

    string Get(string key, params object?[] args);
}
