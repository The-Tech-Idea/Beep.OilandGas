using Beep.OilandGas.PermitsAndApplications.Models;
namespace Beep.OilandGas.PermitsAndApplications.Validation;

public static class PermitFieldValueResolver
{
    public static bool TryResolve(PermitValidationRequest request, string key, out string? value) =>
        Beep.OilandGas.PermitsAndApplications.Models.PermitApplicationSupportingRecords.TryResolve(request.Application,
            request.DrillingApplication, request.EnvironmentalApplication, request.InjectionApplication, key, out value)
        && !string.IsNullOrWhiteSpace(value);
}
