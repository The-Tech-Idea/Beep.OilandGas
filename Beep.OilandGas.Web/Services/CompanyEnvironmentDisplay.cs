using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Client.App;
using System.Globalization;

namespace Beep.OilandGas.Web.Services;

/// <summary>Display-only company settings associated with one exact selection generation.</summary>
public sealed record CompanyEnvironmentDisplay(long CompanyVersion, CompanyEnvironmentEffectiveReview Review)
{
    public bool Matches(CompanyClientContext company) => company.Selection is not null && company.Version == CompanyVersion &&
        company.Selection.Scope == Review.Company && Review.ReviewStatus == "Current";

    public string? LocalTime(DateTime utc)
    {
        var settings = Review.Settings;
        if (utc.Kind != DateTimeKind.Utc || Review.ReviewStatus != "Current" || settings is null || settings.Company != Review.Company ||
            settings.Warnings is null || settings.Warnings.Length != 0 || settings.TimeZoneId is not { Length: > 0 } id || id.Length > 128 ||
            settings.DisplayCulture is null || settings.DateTimeFormat is null || settings.Calendar is not ("Gregorian" or "Culture") ||
            settings.DisplayCulture.Length > 128 || settings.DateTimeFormat.Length > 128 || settings.DateTimeFormat.Any(char.IsControl)) return null;
        try
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(id);
            var local = TimeZoneInfo.ConvertTime(new DateTimeOffset(utc), zone);
            var culture = settings.DisplayCulture.Length == 0 ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(settings.DisplayCulture, predefinedOnly: true);
            if (settings.Calendar == "Gregorian" && culture.Calendar is not GregorianCalendar)
            {
                var calendar = culture.OptionalCalendars.OfType<GregorianCalendar>().FirstOrDefault();
                if (calendar is null) return null;
                culture = (CultureInfo)culture.Clone();
                culture.DateTimeFormat.Calendar = calendar;
            }
            var pattern = settings.DateTimeFormat.Length == 0
                ? culture.DateTimeFormat.ShortDatePattern + " " + culture.DateTimeFormat.ShortTimePattern
                : settings.DateTimeFormat;
            return local.ToString(pattern, culture) + " " + local.ToString("zzz", CultureInfo.InvariantCulture) + " (" + id + ")";
        }
        catch (Exception error) when (error is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException or FormatException)
        { return null; }
    }

}

/// <summary>Applies a display update only to the selection that requested it.</summary>
public sealed record CompanyEnvironmentDisplayUpdate(long CompanyVersion, CompanyEnvironmentEffectiveReview? Review)
{
    public CompanyEnvironmentDisplay? Apply(CompanyClientContext company, CompanyEnvironmentDisplay? previous)
    {
        if (company.Version != CompanyVersion) return previous;
        if (company.Selection is null || Review is null || Review.Company != company.Selection.Scope || Review.ReviewStatus != "Current") return null;
        return new CompanyEnvironmentDisplay(CompanyVersion, Review);
    }
}

/// <summary>Invalidates this circuit's company display after a confirmed setup change.</summary>
public sealed class CompanyEnvironmentChangeSignal
{
    public event Action? Changed;
    public void Notify() => Changed?.Invoke();
}

/// <summary>Bounds how often an open circuit rechecks its authorized company environment.</summary>
public class CompanyEnvironmentPollClock
{
    public virtual Task WaitAsync(CancellationToken cancellationToken) => Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
}
