namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>How the installation reaches its data. Mirrors the ERP deployment modes.</summary>
public enum OgDeploymentMode
{
    /// <summary>A local file database on this workstation; no API.</summary>
    Personal,

    /// <summary>A shared company database reached through the API service.</summary>
    CompanyDatabase,

    /// <summary>A remote API service only.</summary>
    CompanyApi,
}

/// <summary>The signed-in context every page is scoped by: company, field, book, period and user.</summary>
public sealed class OgSession
{
    public OgSession(
        string connectionName,
        string companyId,
        string companyName,
        string fieldId,
        string fieldName,
        string userId,
        string userDisplayName,
        string cultureName,
        string timeZoneId,
        string baseCurrencyCode,
        string bookCode,
        string periodId,
        string periodName,
        OgDeploymentMode mode,
        string personaCode)
    {
        ConnectionName = connectionName;
        CompanyId = companyId;
        CompanyName = companyName;
        FieldId = fieldId;
        FieldName = fieldName;
        UserId = userId;
        UserDisplayName = userDisplayName;
        CultureName = cultureName;
        TimeZoneId = timeZoneId;
        BaseCurrencyCode = baseCurrencyCode;
        BookCode = bookCode;
        PeriodId = periodId;
        PeriodName = periodName;
        Mode = mode;
        PersonaCode = personaCode;
    }

    public string ConnectionName { get; }
    public string CompanyId { get; }
    public string CompanyName { get; }
    public string FieldId { get; }
    public string FieldName { get; }
    public string UserId { get; }
    public string UserDisplayName { get; }
    public string CultureName { get; }
    public string TimeZoneId { get; }
    public string BaseCurrencyCode { get; }
    public string BookCode { get; }
    public string PeriodId { get; }
    public string PeriodName { get; }
    public OgDeploymentMode Mode { get; }
    public string PersonaCode { get; }

    public bool HasField => !string.IsNullOrWhiteSpace(FieldId);
    public bool HasPeriod => !string.IsNullOrWhiteSpace(PeriodId);

    /// <summary>Initials for the shell avatar.</summary>
    public string UserInitials
    {
        get
        {
            var parts = (UserDisplayName ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..1].ToUpperInvariant(),
                _ => (parts[0][..1] + parts[^1][..1]).ToUpperInvariant(),
            };
        }
    }
}
