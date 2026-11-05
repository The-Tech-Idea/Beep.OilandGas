using Beep.OilandGas.Models.Data.Company;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.PermitsAndApplications.Models;

/// <summary>Explicit read-only reconciliation of one retained original, including confirmed absence.</summary>
public sealed record PermitApplicationCommandRecovery(
    [property: JsonRequired] PermitApplicationCommand Original,
    [property: JsonRequired] PermitApplicationCommandOutcome? Outcome)
{
    public void Require(PermitApplicationCommand expected)
    {
        expected.Require();
        if (Original != expected) throw new InvalidOperationException("Permit recovery belongs to another retained original.");
        if (Outcome is not null)
        {
            Outcome.Require();
            if (Outcome.Original != expected) throw new InvalidOperationException("The recorded permit outcome belongs to another original.");
        }
    }
}
